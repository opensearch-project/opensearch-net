/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace OpenSearch.Net
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> contract resolver that makes the serializer honor the
	/// <c>System.Runtime.Serialization</c> attributes the client already uses
	/// (<see cref="DataMemberAttribute"/>, <see cref="IgnoreDataMemberAttribute"/>,
	/// <see cref="DataContractAttribute"/>), rather than requiring every model property to be
	/// re-annotated with <c>[JsonPropertyName]</c>/<c>[JsonIgnore]</c>.
	/// <para>
	/// This is foundational infrastructure for the Utf8Json → System.Text.Json migration
	/// (see GitHub issue #388): the client has ~3,700 <c>[DataMember]</c> attributes that must
	/// keep producing the same wire property names.
	/// </para>
	/// <list type="bullet">
	/// <item><c>[DataMember(Name = "x")]</c> → the JSON property name becomes <c>x</c>.</item>
	/// <item><c>[IgnoreDataMember]</c> → the property is not (de)serialized.</item>
	/// <item>On a <c>[DataContract]</c> type, only members with <c>[DataMember]</c> are serialized (opt-in semantics).</item>
	/// </list>
	/// <para>
	/// The client overwhelmingly declares these attributes on <em>interfaces</em> (for example
	/// <c>ICharGroupTokenizer.MaxTokenLength</c> carries <c>[DataMember(Name = "max_token_length")]</c>
	/// while the concrete <c>CharGroupTokenizer.MaxTokenLength</c> implements it implicitly with no
	/// attribute). The resolver therefore walks the concrete type's interface maps and inherits the
	/// attributes from the implemented interface property, mirroring how the vendored Utf8Json
	/// resolver (<c>MetaType</c>) resolved names.
	/// </para>
	/// </summary>
	public class DataContractResolver : DefaultJsonTypeInfoResolver
	{
		/// <summary> A shared instance applying the data-contract modifier. </summary>
		public static readonly DataContractResolver Instance = new();

		/// <summary>
		/// Maps a vendored Utf8Json formatter type (as referenced by a member's
		/// <c>[JsonFormatter(typeof(...))]</c> attribute) to the <see cref="System.Text.Json"/>
		/// converter that should be applied to that specific member (#388). This is the seam for
		/// per-property converters that cannot be registered globally because they target primitives
		/// (e.g. a <c>bool?</c> that the server may send as the string <c>"true"</c>); registering a
		/// global <c>JsonConverter&lt;bool?&gt;</c> would hijack all boolean handling. The map is
		/// populated by the high-level client (which owns both the formatter types and the converters)
		/// before any serialization occurs.
		/// </summary>
		public static readonly Dictionary<Type, System.Text.Json.Serialization.JsonConverter> PropertyConverterOverrides = new();

		/// <summary>
		/// Open-generic companion to <see cref="PropertyConverterOverrides"/> (#388): maps a vendored
		/// Utf8Json formatter's generic type definition (e.g. <c>SingleOrEnumerableFormatter&lt;&gt;</c>)
		/// to the <see cref="System.Text.Json"/> converter's generic type definition. When a member's
		/// <c>[JsonFormatter(typeof(F&lt;TArgs&gt;))]</c> references a closed generic whose definition is
		/// registered here, the resolver closes the converter over the same type arguments and applies
		/// it to that member.
		/// </summary>
		public static readonly Dictionary<Type, Type> PropertyConverterOverridesOpenGeneric = new();

		/// <summary>
		/// Optional per-member name/ignore override applied after the <c>[DataMember]</c> rules (#388).
		/// Used by the <em>source</em> serializer to reproduce the client's document field-name inference
		/// (camel-casing plus configured property mappings and mapping attributes), which the
		/// request/response contract does not apply. Returns <c>null</c> to leave the member untouched.
		/// </summary>
		private readonly Func<MemberInfo, (string Name, bool Ignore)?> _nameOverride;

		/// <summary> Creates a resolver that applies the data-contract modifier to every object type. </summary>
		public DataContractResolver() => Modifiers.Add(ApplyDataContract);

		/// <summary>
		/// Creates a resolver that, in addition to the data-contract rules, applies a per-member
		/// name/ignore override (document field-name inference for the source serializer, #388).
		/// </summary>
		public DataContractResolver(Func<MemberInfo, (string Name, bool Ignore)?> nameOverride)
		{
			_nameOverride = nameOverride;
			Modifiers.Add(ApplyDataContract);
		}

		internal void ApplyDataContract(JsonTypeInfo typeInfo)
		{
			if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

			// The client's models are (de)serialized by property, like a data-contract serializer.
			// Some — e.g. InlineScript(string script) — expose only a parameterized constructor whose
			// parameters do not bind to properties, which STJ cannot construct. When there is no usable
			// parameterless constructor, create an uninitialized instance and let the property setters
			// populate it. Types that DO have a parameterless constructor are left untouched, so
			// constructor-set defaults (e.g. TokenizerBase setting Type) still run.
			if (typeInfo.CreateObject == null
				&& !typeInfo.Type.IsAbstract
				&& !typeInfo.Type.IsValueType
				&& typeInfo.Type.GetConstructor(Type.EmptyTypes) == null)
			{
				var type = typeInfo.Type;
				typeInfo.CreateObject = () => CreateUninitialized(type);
			}

			// Opt-in serialization applies for [DataContract] types and for types implementing an
			// [InterfaceDataContract] interface (the client's convention for queries, aggregations,
			// scripts, …): only [DataMember] members serialize, so public helper/computed properties
			// (e.g. IAggregation.Meta on a metric agg, BucketAggregationBase.Aggregations) are excluded.
			var isDataContract = typeInfo.Type.GetCustomAttribute<DataContractAttribute>() != null
				|| ImplementsInterfaceDataContract(typeInfo.Type);

			// For a concrete class, pre-compute the interface maps so attributes declared on an
			// implemented interface member are honored on the implementing property.
			var interfaceMaps = typeInfo.Type.IsClass && !typeInfo.Type.IsAbstract
				? typeInfo.Type.GetInterfaces().Select(typeInfo.Type.GetInterfaceMap).ToArray()
				: null;

			foreach (var property in typeInfo.Properties.ToList())
			{
				if (property.AttributeProvider is not PropertyInfo member)
					continue;

				var interfaceProps = GetImplementedInterfaceProperties(member, interfaceMaps);

				var ignore = GetAttribute<IgnoreDataMemberAttribute>(member, interfaceProps) != null;
				var dataMember = GetAttribute<DataMemberAttribute>(member, interfaceProps);

				// On [DataContract] types serialization is opt-in: drop members without [DataMember].
				if (ignore || (isDataContract && dataMember == null))
				{
					typeInfo.Properties.Remove(property);
					continue;
				}

				if (dataMember != null && !string.IsNullOrEmpty(dataMember.Name))
					property.Name = dataMember.Name;
				else if (_nameOverride == null)
					// Request/response default: the vendored resolver camel-cases every un-named member
					// (its name mutator = ToCamelCase). Members carrying an explicit [DataMember(Name)]
					// keep it (handled above); the source path handles naming via _nameOverride below.
					property.Name = ToCamelCase(property.Name);

				// Member-level [StringEnum]: serialize the (nullable) enum as its string form even when the
				// enum type itself is not annotated, mirroring the vendored CreateEnumFormatterForProperty.
				var stringEnum = GetAttribute<StringEnumAttribute>(member, interfaceProps);
				if (stringEnum != null)
				{
					var enumType = Nullable.GetUnderlyingType(member.PropertyType) ?? member.PropertyType;
					if (enumType.IsEnum)
						property.CustomConverter = StringEnumConverterFactory.Instance.CreateConverter(member.PropertyType, null);
				}

				// Source serializer: apply the document field-name inference (camel-casing + configured
				// property mappings / mapping attributes), overriding the [DataMember] name above, and
				// honoring an inferred ignore (e.g. [Ignore], [PropertyName(Ignore = true)]).
				if (_nameOverride != null)
				{
					var over = _nameOverride(member);
					if (over.HasValue)
					{
						if (over.Value.Ignore)
						{
							typeInfo.Properties.Remove(property);
							continue;
						}

						if (!string.IsNullOrEmpty(over.Value.Name))
							property.Name = over.Value.Name;
					}
				}

				// Honor the ShouldSerialize<Member>() convention (Utf8Json/Json.NET): the client uses it
				// to omit, for example, empty bool-query clause arrays (ShouldSerializeMust, etc.).
				var shouldSerialize = typeInfo.Type.GetMethod(
					"ShouldSerialize" + member.Name, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
				if (shouldSerialize != null && shouldSerialize.ReturnType == typeof(bool))
					property.ShouldSerialize = (obj, _) => (bool)shouldSerialize.Invoke(obj, null);

				// Mirror Utf8Json (MetaType: allowPrivate || dm != null): a [DataMember] property with
				// only a non-public setter must still be writable on deserialize. STJ leaves Set null
				// for non-public setters, so wire it via reflection. This matters for data-driven
				// discriminants such as LanguageAnalyzer.Type (protected set), which would otherwise be
				// lost when reading a response back.
				if (dataMember != null && property.Set == null && member.SetMethod != null)
				{
					var setMethod = member.SetMethod;
					property.Set = (obj, value) => setMethod.Invoke(obj, new[] { value });
				}

				// Honor a member-level [JsonFormatter(typeof(F))] by applying the registered per-property
				// converter for F, if any (#388). Used for primitives the server may send as strings and
				// for generic wrappers such as single-or-enumerable members.
				// A member-level [JsonFormatter] takes precedence; otherwise honor a type-level
				// [JsonFormatter] on the member's declared type (e.g. the verbatim-keys dictionaries
				// such as IAnalyzers carry the attribute on the interface, not the member).
				var formatter = GetAttribute<Utf8Json.JsonFormatterAttribute>(member, interfaceProps)
					?? member.PropertyType.GetCustomAttribute<Utf8Json.JsonFormatterAttribute>(false);
				if (formatter != null && TryGetPropertyConverter(formatter.FormatterType, member.PropertyType, out var propertyConverter))
					property.CustomConverter = propertyConverter;
			}

			AddExplicitInterfaceProperties(typeInfo, _nameOverride == null);
		}

		/// <summary>
		/// Lower-cases the first character only, matching the client's <c>ToCamelCase</c> (and the
		/// vendored Utf8Json name mutator). Deliberately does NOT lower an all-caps prefix, so an
		/// un-named member such as <c>Pipeline</c> becomes <c>pipeline</c> while <c>ID</c> stays <c>iD</c>.
		/// </summary>
		internal static string ToCamelCase(string s)
		{
			if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0])) return s;
			return s.Length == 1 ? char.ToLowerInvariant(s[0]).ToString() : char.ToLowerInvariant(s[0]) + s.Substring(1);
		}

		/// <summary>
		/// STJ's default resolver ignores explicit interface implementations (they are non-public).
		/// The client's fluent descriptors implement their query/DSL interfaces explicitly, and are the
		/// <c>[ReadAs]</c> targets used on deserialize, so their <c>[DataMember]</c> members must be
		/// (de)serialized. Add a property for each interface <c>[DataMember]</c> not already surfaced,
		/// reading/writing through the interface (mirrors Utf8Json's <c>allowPrivate</c>).
		/// </summary>
		private static void AddExplicitInterfaceProperties(JsonTypeInfo typeInfo, bool camelCaseDefault)
		{
			if (typeInfo.Type.IsInterface || typeInfo.Type.IsAbstract) return;

			var existing = new HashSet<string>(StringComparer.Ordinal);
			foreach (var p in typeInfo.Properties) existing.Add(p.Name);

			foreach (var interfaceType in typeInfo.Type.GetInterfaces())
			{
				foreach (var interfaceProperty in interfaceType.GetProperties())
				{
					if (interfaceProperty.GetCustomAttribute<IgnoreDataMemberAttribute>() != null) continue;

					var dataMember = interfaceProperty.GetCustomAttribute<DataMemberAttribute>();
					if (dataMember == null) continue;

					var name = !string.IsNullOrEmpty(dataMember.Name)
						? dataMember.Name
						: (camelCaseDefault ? ToCamelCase(interfaceProperty.Name) : interfaceProperty.Name);
					if (!existing.Add(name)) continue; // already surfaced (public implementation or another interface)

					var jsonProperty = typeInfo.CreateJsonPropertyInfo(interfaceProperty.PropertyType, name);
					jsonProperty.Get = interfaceProperty.CanRead ? interfaceProperty.GetValue : null;
					jsonProperty.Set = interfaceProperty.CanWrite ? interfaceProperty.SetValue : null;

					var shouldSerialize = typeInfo.Type.GetMethod(
						"ShouldSerialize" + interfaceProperty.Name,
						BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
					if (shouldSerialize != null && shouldSerialize.ReturnType == typeof(bool))
						jsonProperty.ShouldSerialize = (obj, _) => (bool)shouldSerialize.Invoke(obj, null);

					var formatter = interfaceProperty.GetCustomAttribute<Utf8Json.JsonFormatterAttribute>(true)
						?? interfaceProperty.PropertyType.GetCustomAttribute<Utf8Json.JsonFormatterAttribute>(false);
					if (formatter != null && TryGetPropertyConverter(formatter.FormatterType, interfaceProperty.PropertyType, out var converter))
						jsonProperty.CustomConverter = converter;

					typeInfo.Properties.Add(jsonProperty);
				}
			}
		}

		private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, System.Text.Json.Serialization.JsonConverter> ClosedConverterCache = new();

		/// <summary>
		/// Resolves the per-property converter for a member's <c>[JsonFormatter]</c> formatter type:
		/// first an exact match in <see cref="PropertyConverterOverrides"/>, then an open-generic match
		/// in <see cref="PropertyConverterOverridesOpenGeneric"/> (closing the converter over the
		/// formatter's type arguments and caching the result).
		/// </summary>
		private static bool TryGetPropertyConverter(Type formatterType, Type declaredType,
			out System.Text.Json.Serialization.JsonConverter converter)
		{
			if (PropertyConverterOverrides.Count > 0 && PropertyConverterOverrides.TryGetValue(formatterType, out converter))
				return true;

			if (PropertyConverterOverridesOpenGeneric.Count > 0 && formatterType.IsGenericType
				&& PropertyConverterOverridesOpenGeneric.TryGetValue(formatterType.GetGenericTypeDefinition(), out var converterDefinition))
			{
				// The converter is closed over the formatter's own type arguments when they are concrete
				// (e.g. VerbatimDictionaryKeysFormatter<Analyzers, IAnalyzers, string, IAnalyzer>), but an
				// open formatter takes its arguments from the declared member type: a generic interface
				// supplies them directly (e.g. ISuggestDictionary<T> → SuggestDictionaryFormatter<>), while
				// a formatter that closes over the member's own type (e.g. TDocument Source →
				// SourceFormatter<>) uses the declared type itself.
				Type[] typeArguments;
				if (!formatterType.ContainsGenericParameters)
					typeArguments = formatterType.GetGenericArguments();
				else if (declaredType != null && declaredType.IsGenericType)
					typeArguments = declaredType.GetGenericArguments();
				else if (declaredType != null && converterDefinition.GetGenericArguments().Length == 1)
					typeArguments = new[] { declaredType };
				else
					typeArguments = formatterType.GetGenericArguments();

				var cacheKey = converterDefinition.MakeGenericType(typeArguments);
				converter = ClosedConverterCache.GetOrAdd(cacheKey,
					closed => (System.Text.Json.Serialization.JsonConverter)Activator.CreateInstance(closed));
				return true;
			}

			converter = null;
			return false;
		}

		private static bool ImplementsInterfaceDataContract(Type type)
		{
			if (type.GetCustomAttribute<Utf8Json.InterfaceDataContractAttribute>() != null) return true;
			foreach (var interfaceType in type.GetInterfaces())
				if (interfaceType.GetCustomAttribute<Utf8Json.InterfaceDataContractAttribute>() != null) return true;
			return false;
		}

		private static object CreateUninitialized(Type type) =>
#if NETSTANDARD2_0
			System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
#else
			System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
#endif

		/// <summary>
		/// Returns the attribute from the property itself, or failing that, from any interface
		/// property the member implements.
		/// </summary>
		private static TAttribute GetAttribute<TAttribute>(PropertyInfo member, List<PropertyInfo> interfaceProps)
			where TAttribute : Attribute
		{
			var attribute = member.GetCustomAttribute<TAttribute>(true);
			if (attribute != null) return attribute;

			if (interfaceProps != null)
			{
				foreach (var interfaceProp in interfaceProps)
				{
					attribute = interfaceProp.GetCustomAttribute<TAttribute>(true);
					if (attribute != null) return attribute;
				}
			}

			return null;
		}

		/// <summary>
		/// Finds the interface-declared properties that <paramref name="member"/> implements, by
		/// matching the property accessor against each interface map's target methods (matching on
		/// name and declaring type, as reflection may surface distinct <see cref="MethodInfo"/>
		/// instances for the same method).
		/// </summary>
		private static List<PropertyInfo> GetImplementedInterfaceProperties(PropertyInfo member, InterfaceMapping[] interfaceMaps)
		{
			if (interfaceMaps == null) return null;

			var accessor = member.GetMethod ?? member.SetMethod;
			if (accessor == null) return null;

			List<PropertyInfo> interfaceProps = null;

			foreach (var map in interfaceMaps)
			{
				for (var i = 0; i < map.TargetMethods.Length; i++)
				{
					var target = map.TargetMethods[i];
					if (target.Name != accessor.Name || target.DeclaringType != accessor.DeclaringType)
						continue;

					// Handle explicit interface implementations ("Namespace.IType.Member").
					var propertyName = member.Name.StartsWith(map.InterfaceType.FullName + ".", StringComparison.Ordinal)
						? member.Name.Substring(map.InterfaceType.FullName.Length + 1)
						: member.Name;

					var info = map.InterfaceType.GetProperty(propertyName);
					if (info != null)
						(interfaceProps ??= new List<PropertyInfo>()).Add(info);

					break;
				}
			}

			// Also match interface properties by name. A concrete type may declare a public helper
			// property (e.g. SpanQuery.IsWritable) that parallels an explicit interface implementation
			// (IQuery.IsWritable, marked [IgnoreDataMember]); the interface map points at the explicit
			// method, not the public one, so a name match is needed to inherit the attribute.
			foreach (var map in interfaceMaps)
			{
				var info = map.InterfaceType.GetProperty(member.Name);
				if (info == null) continue;
				interfaceProps ??= new List<PropertyInfo>();
				if (!interfaceProps.Contains(info))
					interfaceProps.Add(info);
			}

			return interfaceProps;
		}
	}
}
