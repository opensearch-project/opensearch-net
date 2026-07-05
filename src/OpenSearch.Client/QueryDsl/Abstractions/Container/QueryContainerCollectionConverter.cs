/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for an <see cref="IEnumerable{QueryContainer}"/> of
	/// <see cref="QueryContainer"/> (e.g. a bool query's must/should/filter/must_not clauses or a
	/// dis_max/hybrid query list), replacing the vendored Utf8Json <c>QueryContainerCollectionFormatter</c>
	/// as part of #388. Only writable containers are written — null and conditionless (non-verbatim)
	/// clauses are skipped — mirroring the vendored formatter, which otherwise leaked <c>null</c>/<c>{}</c>
	/// entries into the array.
	/// </summary>
	internal sealed class QueryContainerCollectionConverter : JsonConverter<IEnumerable<QueryContainer>>
	{
		public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(IEnumerable<QueryContainer>);

		public override void Write(Utf8JsonWriter writer, IEnumerable<QueryContainer> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var container in value)
			{
				if (container != null && ((IQueryContainer)container).IsWritable)
					JsonSerializer.Serialize(writer, container, options);
			}
			writer.WriteEndArray();
		}

		public override IEnumerable<QueryContainer> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.StartObject:
					// A single query object is tolerated as a one-element collection.
					return new List<QueryContainer> { JsonSerializer.Deserialize<QueryContainer>(ref reader, options) };
				case JsonTokenType.StartArray:
				{
					var list = new List<QueryContainer>();
					using var document = JsonDocument.ParseValue(ref reader);
					foreach (var element in document.RootElement.EnumerateArray())
						list.Add(element.Deserialize<QueryContainer>(options));
					return list;
				}
				default:
					reader.Skip();
					return null;
			}
		}
	}
}
