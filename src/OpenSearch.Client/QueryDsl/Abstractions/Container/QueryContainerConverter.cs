/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="QueryContainer"/>, replacing the
	/// vendored Utf8Json <c>QueryContainerFormatter</c> as part of #388.
	/// <para>
	/// A container holds a single query, serialized as its verb wrapper (e.g. <c>{ "bool": { ... } }</c>).
	/// Rather than a discriminator, each verb is a nullable property on <see cref="IQueryContainer"/>;
	/// writing the container as its interface with null-omission leaves only the populated verb. A raw
	/// query is written through verbatim.
	/// </para>
	/// <para>
	/// On read the single verb key selects the target property; the query interface's
	/// <c>[ReadAs]</c> attribute maps it to the concrete type to deserialize. Nested clause arrays
	/// (<c>bool</c>'s must/should/filter) recurse back through this converter.
	/// </para>
	/// </summary>
	internal sealed class QueryContainerConverter : JsonConverter<QueryContainer>
	{
		private static readonly IReadOnlyDictionary<string, PropertyInfo> VerbToProperty = BuildVerbMap();

		private static IReadOnlyDictionary<string, PropertyInfo> BuildVerbMap()
		{
			var map = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
			foreach (var property in typeof(IQueryContainer).GetProperties())
			{
				var dataMember = property.GetCustomAttribute<DataMemberAttribute>();
				if (dataMember?.Name != null)
					map[dataMember.Name] = property;
			}
			return map;
		}

		public override void Write(Utf8JsonWriter writer, QueryContainer value, JsonSerializerOptions options)
		{
			IQueryContainer container = value;

			var raw = container.RawQuery;
			if (raw != null && !string.IsNullOrEmpty(raw.Raw) && raw.IsWritable)
			{
				writer.WriteRawValue(raw.Raw);
				return;
			}

			// Serialize via the interface: null-omission + ShouldSerialize leave only the populated verb.
			JsonSerializer.Serialize(writer, container, typeof(IQueryContainer), options);
		}

		public override QueryContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var container = new QueryContainer();
			IQueryContainer target = container;

			foreach (var member in root.EnumerateObject())
			{
				if (!VerbToProperty.TryGetValue(member.Name, out var containerProperty))
					continue;

				var interfaceType = containerProperty.PropertyType;
				var concreteType = interfaceType.GetCustomAttribute<ReadAsAttribute>()?.Type ?? interfaceType;

				var query = member.Value.Deserialize(concreteType, options);
				containerProperty.SetValue(target, query);
				break; // a container holds exactly one query
			}

			return container;
		}
	}
}
