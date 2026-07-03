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
	/// A <see cref="System.Text.Json"/> converter for <see cref="TermsInclude"/> (#388): a value list
	/// is written as a string array, a partition as <c>{ "partition": …, "num_partitions": … }</c>,
	/// otherwise the pattern as a string.
	/// </summary>
	internal sealed class TermsIncludeConverter : JsonConverter<TermsInclude>
	{
		public override void Write(Utf8JsonWriter writer, TermsInclude value, JsonSerializerOptions options)
		{
			if (value == null) { writer.WriteNullValue(); return; }
			if (value.Values != null) { JsonSerializer.Serialize(writer, value.Values, options); return; }
			if (value.Partition.HasValue && value.NumberOfPartitions.HasValue)
			{
				writer.WriteStartObject();
				writer.WriteNumber("partition", value.Partition.Value);
				writer.WriteNumber("num_partitions", value.NumberOfPartitions.Value);
				writer.WriteEndObject();
				return;
			}
			writer.WriteStringValue(value.Pattern);
		}

		public override TermsInclude Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: return null;
				case JsonTokenType.String: return new TermsInclude(reader.GetString());
				case JsonTokenType.StartArray: return new TermsInclude(JsonSerializer.Deserialize<List<string>>(ref reader, options));
				case JsonTokenType.StartObject:
				{
					using var doc = JsonDocument.ParseValue(ref reader);
					var root = doc.RootElement;
					long partition = root.TryGetProperty("partition", out var p) ? p.GetInt64() : 0;
					long numPartitions = root.TryGetProperty("num_partitions", out var n) ? n.GetInt64() : 0;
					return new TermsInclude(partition, numPartitions);
				}
				default: throw new JsonException($"Cannot deserialize {nameof(TermsInclude)} from {reader.TokenType}.");
			}
		}
	}

	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="TermsExclude"/> (#388): a value list
	/// is written as a string array, otherwise the pattern as a string.
	/// </summary>
	internal sealed class TermsExcludeConverter : JsonConverter<TermsExclude>
	{
		public override void Write(Utf8JsonWriter writer, TermsExclude value, JsonSerializerOptions options)
		{
			if (value == null) { writer.WriteNullValue(); return; }
			if (value.Values != null) { JsonSerializer.Serialize(writer, value.Values, options); return; }
			writer.WriteStringValue(value.Pattern);
		}

		public override TermsExclude Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: return null;
				case JsonTokenType.String: return new TermsExclude(reader.GetString());
				case JsonTokenType.StartArray: return new TermsExclude(JsonSerializer.Deserialize<List<string>>(ref reader, options));
				default: throw new JsonException($"Cannot deserialize {nameof(TermsExclude)} from {reader.TokenType}.");
			}
		}
	}

	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="IncludeExclude"/> (significant terms)
	/// (#388): a value list as a string array, otherwise the pattern as a string.
	/// </summary>
	internal sealed class IncludeExcludeConverter : JsonConverter<IncludeExclude>
	{
		public override void Write(Utf8JsonWriter writer, IncludeExclude value, JsonSerializerOptions options)
		{
			if (value == null) { writer.WriteNullValue(); return; }
			if (value.Values != null) { JsonSerializer.Serialize(writer, value.Values, options); return; }
			writer.WriteStringValue(value.Pattern);
		}

		public override IncludeExclude Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: return null;
				case JsonTokenType.String: return new IncludeExclude(reader.GetString());
				case JsonTokenType.StartArray: return new IncludeExclude(JsonSerializer.Deserialize<List<string>>(ref reader, options));
				default: throw new JsonException($"Cannot deserialize {nameof(IncludeExclude)} from {reader.TokenType}.");
			}
		}
	}
}
