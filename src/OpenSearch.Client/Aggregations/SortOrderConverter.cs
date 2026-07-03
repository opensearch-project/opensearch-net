/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="ISortOrder"/> values
	/// (<see cref="TermsOrder"/>, <see cref="HistogramOrder"/>), replacing the vendored Utf8Json
	/// <c>SortOrderFormatter</c> as part of #388. Serialized as <c>{ "&lt;key&gt;": "asc|desc" }</c>.
	/// </summary>
	internal sealed class SortOrderConverter<TSortOrder> : JsonConverter<TSortOrder>
		where TSortOrder : class, ISortOrder, new()
	{
		public override void Write(Utf8JsonWriter writer, TSortOrder value, JsonSerializerOptions options)
		{
			if (value?.Key == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(value.Key);
			JsonSerializer.Serialize(writer, value.Order, options);
			writer.WriteEndObject();
		}

		public override TSortOrder Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			foreach (var member in root.EnumerateObject())
				return new TSortOrder { Key = member.Name, Order = member.Value.Deserialize<SortOrder>(options) };

			return null;
		}
	}
}
