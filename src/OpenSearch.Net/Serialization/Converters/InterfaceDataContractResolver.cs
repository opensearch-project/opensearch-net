/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization.Metadata;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// Public marker attribute for the System.Text.Json data-contract model, usable on interfaces (unlike the
	/// framework's <see cref="DataContractAttribute"/>). It is the migration replacement for the Utf8Json-internal
	/// <c>[InterfaceDataContract]</c>: apply it to an interface (or type) whose <see cref="DataMemberAttribute"/>
	/// annotations should drive serialization via <see cref="InterfaceDataContractResolver"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
	public sealed class OpenSearchContractAttribute : Attribute { }

	/// <summary>
	/// A System.Text.Json <see cref="IJsonTypeInfoResolver"/> that reproduces the legacy Utf8Json
	/// <c>[InterfaceDataContract]</c> behaviour: serialization metadata (<see cref="DataMemberAttribute"/> /
	/// <see cref="IgnoreDataMemberAttribute"/>) is frequently declared on the <em>interfaces</em> a request type
	/// implements rather than on the concrete class. When a type (or one of its interfaces) is marked with
	/// <see cref="InterfaceDataContractAttribute"/>, this resolver:
	/// <list type="bullet">
	/// <item>opts the type into a data-contract model: only members carrying <see cref="DataMemberAttribute"/>
	/// (on the property or a matching interface property) are serialized;</item>
	/// <item>honours <see cref="IgnoreDataMemberAttribute"/> and the <c>Name</c> of <see cref="DataMemberAttribute"/>
	/// as declared on the interface.</item>
	/// </list>
	/// This is the System.Text.Json replacement for the metadata handling in the legacy <c>MetaType</c>.
	/// </summary>
	public class InterfaceDataContractResolver : DefaultJsonTypeInfoResolver
	{
		public override JsonTypeInfo GetTypeInfo(Type type, System.Text.Json.JsonSerializerOptions options)
		{
			var typeInfo = base.GetTypeInfo(type, options);

			if (typeInfo.Kind != JsonTypeInfoKind.Object)
				return typeInfo;

			// STJ only wires CreateObject for an accessible (public) parameterless ctor. Many generated request and
			// domain types (e.g. LikeDocument<T>) expose only an internal/private parameterless ctor, which the
			// legacy Utf8Json engine could construct. Fall back to it so those types can be deserialized.
			if (typeInfo.CreateObject == null)
			{
				var ctor = type.GetConstructor(
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
					binder: null, Type.EmptyTypes, modifiers: null);
				if (ctor != null && !type.IsAbstract)
					typeInfo.CreateObject = () => ctor.Invoke(null);
			}

			var interfaces = type.GetInterfaces();
			// Data-contract types opt into an allow-list model: ONLY [DataMember] members serialize. Plain types do
			// not opt in — all public members serialize — but both models still honour [IgnoreDataMember] and the
			// [DataMember(Name)] override (declared on the property or a matching interface property). This mirrors
			// the legacy Utf8Json MetaType, whose non-data-contract branch also used `dm?.Name ?? nameMutator(name)`.
			var isDataContract = HasInterfaceDataContract(type, interfaces);

			foreach (var property in typeInfo.Properties.ToArray())
			{
				var interfaceProp = FindInterfaceProperty(interfaces, property.Name);

				if (IsIgnored(property, interfaceProp))
				{
					typeInfo.Properties.Remove(property);
					continue;
				}

				var dataMember = GetDataMember(property, interfaceProp);

				// Opt-in removal applies only to data-contract types: without a [DataMember] the member is dropped.
				if (dataMember == null)
				{
					if (isDataContract)
						typeInfo.Properties.Remove(property);
					continue;
				}

				if (!string.IsNullOrEmpty(dataMember.Name))
					property.Name = dataMember.Name;
			}

			return typeInfo;
		}

		private static bool HasInterfaceDataContract(Type type, Type[] interfaces)
		{
			if (IsDataContract(type))
				return true;
			return interfaces.Any(IsDataContract);
		}

		// The legacy MetaType recognised both [DataContract] and the Utf8Json-internal [InterfaceDataContract]
		// as opting a type into the data-contract model. We honour both so existing annotations keep working.
		private static bool IsDataContract(Type type) =>
			type.GetCustomAttribute<OpenSearchContractAttribute>(true) != null ||
			type.GetCustomAttribute<InterfaceDataContractAttribute>(true) != null ||
			type.GetCustomAttribute<DataContractAttribute>(true) != null;

		private static PropertyInfo FindInterfaceProperty(Type[] interfaces, string clrOrJsonName)
		{
			// Match by CLR property name against interface-declared properties.
			foreach (var i in interfaces)
			{
				var p = i.GetProperty(clrOrJsonName, BindingFlags.Public | BindingFlags.Instance);
				if (p != null)
					return p;
			}

			return null;
		}

		private static bool IsIgnored(JsonPropertyInfo property, PropertyInfo interfaceProp)
		{
			if (property.AttributeProvider is MemberInfo m &&
				m.GetCustomAttribute<IgnoreDataMemberAttribute>(true) != null)
				return true;

			return interfaceProp?.GetCustomAttribute<IgnoreDataMemberAttribute>(true) != null;
		}

		private static DataMemberAttribute GetDataMember(JsonPropertyInfo property, PropertyInfo interfaceProp)
		{
			if (property.AttributeProvider is MemberInfo m)
			{
				var dm = m.GetCustomAttribute<DataMemberAttribute>(true);
				if (dm != null)
					return dm;
			}

			return interfaceProp?.GetCustomAttribute<DataMemberAttribute>(true);
		}
	}
}
