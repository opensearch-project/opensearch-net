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

		/// <summary> Creates a resolver that applies the data-contract modifier to every object type. </summary>
		public DataContractResolver() => Modifiers.Add(ApplyDataContract);

		internal static void ApplyDataContract(JsonTypeInfo typeInfo)
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

			var isDataContract = typeInfo.Type.GetCustomAttribute<DataContractAttribute>() != null;

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
			}

			AddExplicitInterfaceProperties(typeInfo);
		}

		/// <summary>
		/// STJ's default resolver ignores explicit interface implementations (they are non-public).
		/// The client's fluent descriptors implement their query/DSL interfaces explicitly, and are the
		/// <c>[ReadAs]</c> targets used on deserialize, so their <c>[DataMember]</c> members must be
		/// (de)serialized. Add a property for each interface <c>[DataMember]</c> not already surfaced,
		/// reading/writing through the interface (mirrors Utf8Json's <c>allowPrivate</c>).
		/// </summary>
		private static void AddExplicitInterfaceProperties(JsonTypeInfo typeInfo)
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

					var name = !string.IsNullOrEmpty(dataMember.Name) ? dataMember.Name : interfaceProperty.Name;
					if (!existing.Add(name)) continue; // already surfaced (public implementation or another interface)

					var jsonProperty = typeInfo.CreateJsonPropertyInfo(interfaceProperty.PropertyType, name);
					jsonProperty.Get = interfaceProperty.CanRead ? interfaceProperty.GetValue : null;
					jsonProperty.Set = interfaceProperty.CanWrite ? interfaceProperty.SetValue : null;

					var shouldSerialize = typeInfo.Type.GetMethod(
						"ShouldSerialize" + interfaceProperty.Name,
						BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
					if (shouldSerialize != null && shouldSerialize.ReturnType == typeof(bool))
						jsonProperty.ShouldSerialize = (obj, _) => (bool)shouldSerialize.Invoke(obj, null);

					typeInfo.Properties.Add(jsonProperty);
				}
			}
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
