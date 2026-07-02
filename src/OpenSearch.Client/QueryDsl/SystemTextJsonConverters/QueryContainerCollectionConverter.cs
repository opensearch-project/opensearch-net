/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Collections.Generic;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IEnumerable{QueryContainer}"/>.
	/// Handles reading/writing arrays of query containers while gracefully handling:
	/// - Null/empty collections
	/// - Filtering out null/conditionless queries on write
	/// - Single object (non-array) on read (wraps in a list)
	/// </summary>
	internal sealed class QueryContainerCollectionConverter : JsonConverter<IEnumerable<QueryContainer>>
	{
		private static readonly QueryContainerConverter ContainerConverter = new QueryContainerConverter();

		public override IEnumerable<QueryContainer> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var list = new List<QueryContainer>();
				while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
				{
					var container = ContainerConverter.Read(ref reader, typeof(QueryContainer), options);
					if (container != null)
						list.Add(container);
				}
				return list;
			}

			if (reader.TokenType == JsonTokenType.StartObject)
			{
				// Single query object instead of array - wrap in list
				var container = ContainerConverter.Read(ref reader, typeof(QueryContainer), options);
				if (container != null)
					return new List<QueryContainer> { container };
				return new List<QueryContainer>();
			}

			throw new JsonException($"Unexpected token {reader.TokenType} when reading query container collection");
		}

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
				if (container == null || !container.IsWritable)
					continue;
				ContainerConverter.Write(writer, container, options);
			}
			writer.WriteEndArray();
		}
	}
}
