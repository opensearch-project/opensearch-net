/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

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
	/// </summary>
	public class DataContractResolver : DefaultJsonTypeInfoResolver
	{
		/// <summary> A shared instance applying the data-contract modifier. </summary>
		public static readonly DataContractResolver Instance = new();

		public DataContractResolver() => Modifiers.Add(ApplyDataContract);

		internal static void ApplyDataContract(JsonTypeInfo typeInfo)
		{
			if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

			var isDataContract = typeInfo.Type.GetCustomAttribute<DataContractAttribute>() != null;

			foreach (var property in typeInfo.Properties.ToList())
			{
				var member = property.AttributeProvider as MemberInfo;

				var ignore = member?.GetCustomAttribute<IgnoreDataMemberAttribute>() != null;
				var dataMember = member?.GetCustomAttribute<DataMemberAttribute>();

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
	}
}
