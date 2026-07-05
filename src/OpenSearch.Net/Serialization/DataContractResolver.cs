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
		/// Optional hook (set by the high-level client) that returns a per-member converter based on the
		/// member's client-specific attributes (e.g. <c>[StringTimeSpan]</c>), which live in
		/// <c>OpenSearch.Client</c> and cannot be referenced from here (#388). Returns <c>null</c> to leave
		/// the member untouched.
		/// </summary>
		public static Func<MemberInfo, System.Text.Json.Serialization.JsonConverter> MemberConverterResolver;

		/// <summary>
		/// Optional per-member name/ignore override applied after the <c>[DataMember]</c> rules (#388).
		/// Used by the <em>source</em> serializer to reproduce the client's document field-name inference
		/// (camel-casing plus configured property mappings and mapping attributes), which the
		/// request/response contract does not apply. Returns <c>null</c> to leave the member untouched.
		/// </summary>
		private readonly Func<MemberInfo, (string Name, bool Ignore)?> _nameOverride;

		/// <summary>
		/// Optional per-member, value-based <c>ShouldSerialize</c> hook (set per connection settings by
		/// the high-level client, #388). Returns a predicate <c>(owner, value) =&gt; bool</c> for a member
		/// whose serialization depends on its runtime value — notably a document-derived <c>Routing</c>
		/// that infers to <c>null</c> and must be omitted (mirroring the bulk NDJSON serializer), logic
		/// that lives in <c>OpenSearch.Client</c> and cannot be referenced from here. Returns <c>null</c>
		/// to leave the member's default behavior.
		/// </summary>
		private readonly Func<MemberInfo, Func<object, object, bool>> _memberShouldSerialize;

		/// <summary>
		/// When <c>true</c>, an un-attributed member (no <c>[DataMember(Name)]</c>) is camel-cased,
		/// matching the high-level client's request/response wire format (and the vendored Utf8Json name
		/// mutator). When <c>false</c> — the default, used by the standalone
		/// <see cref="SystemTextJsonSerializer"/> via <see cref="Instance"/> — the member keeps its
		/// declared (PascalCase) name, so the low-level serializer performs raw <c>System.Text.Json</c>
		/// naming while still honoring <c>[DataMember]</c>/<c>[IgnoreDataMember]</c> (#388).
		/// </summary>
		private readonly bool _camelCaseUnattributed;

		/// <summary>
		/// Optional hook (set by the high-level client for the request/response serializer) that reports a
		/// serializer-attribute mapping (e.g. <c>[PropertyName]</c>) for a member (#388). Such a member is
		/// serialized (and named) even on an opt-in <c>[DataContract]</c>/<c>[InterfaceDataContract]</c>
		/// type that lacks <c>[DataMember]</c> — notably a user's custom <c>IProperty</c> implementation.
		/// Returns <c>null</c> when the member carries no serializer attribute.
		/// </summary>
		private readonly Func<MemberInfo, (string Name, bool Ignore)?> _serializerInclusion;

		/// <summary>
		/// Creates a resolver that applies the data-contract modifier to every object type, optionally
		/// with a per-member name/ignore override (document field-name inference for the source
		/// serializer), a value-based <c>ShouldSerialize</c> hook, camel-casing of un-attributed
		/// members for the request/response wire format, and a serializer-attribute inclusion hook (#388).
		/// </summary>
		public DataContractResolver(
			Func<MemberInfo, (string Name, bool Ignore)?> nameOverride = null,
			Func<MemberInfo, Func<object, object, bool>> memberShouldSerialize = null,
			bool camelCaseUnattributed = false,
			Func<MemberInfo, (string Name, bool Ignore)?> serializerInclusion = null)
		{
			_nameOverride = nameOverride;
			_memberShouldSerialize = memberShouldSerialize;
			_camelCaseUnattributed = camelCaseUnattributed;
			_serializerInclusion = serializerInclusion;
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

				// Drop a public convenience property that only shadows (by name) an interface [DataMember]
				// which is actually implemented EXPLICITLY by a different member. The client's mapping
				// attributes expose non-nullable public getters (e.g. TextAttribute.Boost => double) that
				// shadow the nullable explicit interface member (double? ITextProperty.Boost); serializing
				// the public one writes defaults (boost:0.0) the vendored serializer omitted. Dropping it
				// lets AddExplicitInterfaceProperties surface the (nullable) explicit member instead.
				if (member.GetCustomAttribute<DataMemberAttribute>(true) == null
					&& ShadowsExplicitInterfaceDataMember(member, interfaceMaps))
				{
					typeInfo.Properties.Remove(property);
					continue;
				}

				var ignore = GetAttribute<IgnoreDataMemberAttribute>(member, interfaceProps) != null;
				var dataMember = GetAttribute<DataMemberAttribute>(member, interfaceProps);
				// A serializer attribute ([PropertyName]) marks a member for serialization even without
				// [DataMember] on an opt-in type (e.g. a custom IProperty implementation).
				var inclusion = _serializerInclusion?.Invoke(member);
				var includedBySerializerAttribute = inclusion.HasValue && !inclusion.Value.Ignore
					&& !string.IsNullOrEmpty(inclusion.Value.Name);

				// On [DataContract] types serialization is opt-in: drop members without [DataMember] (unless
				// a serializer attribute includes them).
				if (ignore || (isDataContract && dataMember == null && !includedBySerializerAttribute))
				{
					typeInfo.Properties.Remove(property);
					continue;
				}

				if (dataMember != null && !string.IsNullOrEmpty(dataMember.Name))
					property.Name = dataMember.Name;
				else if (includedBySerializerAttribute)
					property.Name = inclusion.Value.Name;
				else if (_camelCaseUnattributed)
					// Request/response wire format: the vendored resolver camel-cases every un-named member
					// (its name mutator = ToCamelCase). Members carrying an explicit [DataMember(Name)]
					// keep it (handled above); the source path handles naming via _nameOverride below. The
					// standalone serializer leaves the declared (PascalCase) name untouched.
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

				// Client-specific member attributes (e.g. [StringTimeSpan]) via the client-provided hook.
				var memberConverter = MemberConverterResolver?.Invoke(member);
				if (memberConverter != null)
					property.CustomConverter = memberConverter;

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
				// to omit, for example, empty bool-query clause arrays (ShouldSerializeMust, etc.). These
				// are frequently declared as explicit interface implementations, so search interfaces too.
				var shouldSerialize = FindShouldSerialize(typeInfo.Type, member.Name);
				if (shouldSerialize != null)
					property.ShouldSerialize = (obj, _) => (bool)shouldSerialize.Invoke(obj, null);

				// Value-based ShouldSerialize (e.g. omit a document-derived Routing that infers to null).
				ApplyMemberShouldSerialize(property, member);

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

			AddExplicitInterfaceProperties(typeInfo, _camelCaseUnattributed);
			AddNonPublicDataMembers(typeInfo, _camelCaseUnattributed);
		}

		/// <summary>
		/// STJ's default resolver only surfaces public properties, but the client marks some non-public
		/// (e.g. <c>internal</c>) properties with <c>[DataMember]</c> — notably <c>ResponseBase.Error</c>
		/// and <c>ResponseBase.StatusCode</c>, which the server error is built from. Utf8Json serialized
		/// these via its <c>allowPrivate</c> semantics; mirror that by adding a property for each
		/// non-public <c>[DataMember]</c> instance property (walking the base types) not already surfaced.
		/// </summary>
		/// <summary>
		/// Finds a parameterless <c>bool ShouldSerialize&lt;Member&gt;()</c> method for the member, honoring
		/// the Utf8Json/Json.NET convention. The client frequently declares these as explicit interface
		/// implementations (e.g. <c>bool IBoolQuery.ShouldSerializeMust()</c>), which are not reachable by
		/// name on the concrete type, so the implemented interfaces are searched as well; invoking the
		/// interface method dispatches to the explicit implementation.
		/// </summary>
		private static MethodInfo FindShouldSerialize(Type type, string memberName)
		{
			var method = type.GetMethod("ShouldSerialize" + memberName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
			if (method != null && method.ReturnType == typeof(bool))
				return method;

			foreach (var interfaceType in type.GetInterfaces())
			{
				var interfaceMethod = interfaceType.GetMethod("ShouldSerialize" + memberName,
					BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
				if (interfaceMethod != null && interfaceMethod.ReturnType == typeof(bool))
					return interfaceMethod;
			}

			return null;
		}

		/// <summary>
		/// Applies the value-based <see cref="_memberShouldSerialize"/> hook to a property, combining it
		/// with any existing predicate (both must return true to serialize).
		/// </summary>
		private void ApplyMemberShouldSerialize(JsonPropertyInfo property, MemberInfo member)
		{
			var predicate = _memberShouldSerialize?.Invoke(member);
			if (predicate == null) return;

			var existing = property.ShouldSerialize;
			property.ShouldSerialize = existing == null
				? predicate
				: (obj, value) => existing(obj, value) && predicate(obj, value);
		}

		private void AddNonPublicDataMembers(JsonTypeInfo typeInfo, bool camelCaseDefault)
		{
			if (typeInfo.Type.IsInterface || typeInfo.Type.IsAbstract) return;

			var existing = new HashSet<string>(StringComparer.Ordinal);
			foreach (var p in typeInfo.Properties) existing.Add(p.Name);

			const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
			for (var type = typeInfo.Type; type != null && type != typeof(object); type = type.BaseType)
			{
				foreach (var member in type.GetProperties(flags))
				{
					if (member.GetCustomAttribute<IgnoreDataMemberAttribute>(true) != null) continue;
					var dataMember = member.GetCustomAttribute<DataMemberAttribute>(true);
					if (dataMember == null) continue;

					var name = !string.IsNullOrEmpty(dataMember.Name)
						? dataMember.Name
						: (camelCaseDefault ? ToCamelCase(member.Name) : member.Name);
					if (!existing.Add(name)) continue;

					var jsonProperty = typeInfo.CreateJsonPropertyInfo(member.PropertyType, name);
					jsonProperty.Get = member.CanRead ? member.GetValue : (Func<object, object>)null;
					jsonProperty.Set = member.CanWrite ? member.SetValue : (Action<object, object>)null;

					var shouldSerialize = FindShouldSerialize(typeInfo.Type, member.Name);
					if (shouldSerialize != null)
						jsonProperty.ShouldSerialize = (obj, _) => (bool)shouldSerialize.Invoke(obj, null);

					ApplyMemberShouldSerialize(jsonProperty, member);

					// Honor a member (or member-type) level [JsonFormatter] on a non-public [DataMember],
					// e.g. BulkUpdateBody.PartialUpdate/Upsert carry [JsonFormatter(CollapsedSourceFormatter<>)]
					// so the update doc/upsert route through the source serializer, not request/response.
					var formatter = member.GetCustomAttribute<Utf8Json.JsonFormatterAttribute>(true)
						?? member.PropertyType.GetCustomAttribute<Utf8Json.JsonFormatterAttribute>(false);
					if (formatter != null && TryGetPropertyConverter(formatter.FormatterType, member.PropertyType, out var converter))
						jsonProperty.CustomConverter = converter;

					typeInfo.Properties.Add(jsonProperty);
				}
			}
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
		private void AddExplicitInterfaceProperties(JsonTypeInfo typeInfo, bool camelCaseDefault)
		{
			if (typeInfo.Type.IsInterface || typeInfo.Type.IsAbstract) return;

			var existing = new HashSet<string>(StringComparer.Ordinal);
			foreach (var p in typeInfo.Properties) existing.Add(p.Name);

			// Add the most-derived interfaces first (those extending the most other interfaces) so a
			// fluent descriptor's own members precede its base-interface members, matching the vendored
			// Utf8Json member order (e.g. ICompletionSuggester.Fuzzy before ISuggester.Field) (#388).
			foreach (var interfaceType in typeInfo.Type.GetInterfaces().OrderByDescending(i => i.GetInterfaces().Length))
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

					var shouldSerialize = FindShouldSerialize(typeInfo.Type, interfaceProperty.Name);
					if (shouldSerialize != null)
						jsonProperty.ShouldSerialize = (obj, _) => (bool)shouldSerialize.Invoke(obj, null);

					var formatter = interfaceProperty.GetCustomAttribute<Utf8Json.JsonFormatterAttribute>(true)
						?? interfaceProperty.PropertyType.GetCustomAttribute<Utf8Json.JsonFormatterAttribute>(false);
					if (formatter != null && TryGetPropertyConverter(formatter.FormatterType, interfaceProperty.PropertyType, out var converter))
						jsonProperty.CustomConverter = converter;

					ApplyMemberShouldSerialize(jsonProperty, interfaceProperty);

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

		/// <summary>
		/// True when <paramref name="member"/> is a public property whose name matches an interface
		/// property carrying <c>[DataMember]</c> that is implemented <em>explicitly</em> by a different
		/// member (so <paramref name="member"/> merely shadows it and should not be serialized).
		/// </summary>
		private static bool ShadowsExplicitInterfaceDataMember(PropertyInfo member, InterfaceMapping[] interfaceMaps)
		{
			if (interfaceMaps == null) return false;
			var getter = member.GetMethod;
			if (getter == null || !getter.IsPublic) return false;

			foreach (var map in interfaceMaps)
			{
				var interfaceProperty = map.InterfaceType.GetProperty(member.Name);
				var interfaceGetter = interfaceProperty?.GetMethod;
				if (interfaceGetter == null) continue;
				if (interfaceProperty.GetCustomAttribute<DataMemberAttribute>(true) == null) continue;

				var index = Array.IndexOf(map.InterfaceMethods, interfaceGetter);
				if (index < 0) continue;

				var target = map.TargetMethods[index];
				// A true public implementation targets this member's own getter; an explicit
				// implementation targets a different (compiler-named) method.
				if (target != getter && target.Name != getter.Name)
					return true;
			}

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
