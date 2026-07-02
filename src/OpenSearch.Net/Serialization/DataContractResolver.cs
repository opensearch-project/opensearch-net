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
			}
		}

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

			return interfaceProps;
		}
	}
}
