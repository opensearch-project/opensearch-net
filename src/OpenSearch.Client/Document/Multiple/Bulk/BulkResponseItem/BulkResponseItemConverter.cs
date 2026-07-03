/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A read-only <see cref="System.Text.Json"/> converter for <see cref="BulkResponseItemBase"/>,
	/// replacing the vendored Utf8Json <c>BulkResponseItemFormatter</c> as part of #388. Each item is a
	/// single-property wrapper object whose key (<c>index</c>/<c>create</c>/<c>update</c>/<c>delete</c>)
	/// selects the concrete response-item type; the wrapped object is then deserialized normally.
	/// </summary>
	internal sealed class BulkResponseItemConverter : JsonConverter<BulkResponseItemBase>
	{
		public override BulkResponseItemBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) { reader.Skip(); return null; }

			using var document = JsonDocument.ParseValue(ref reader);
			foreach (var member in document.RootElement.EnumerateObject())
			{
				switch (member.Name)
				{
					case "delete": return member.Value.Deserialize<BulkDeleteResponseItem>(options);
					case "update": return member.Value.Deserialize<BulkUpdateResponseItem>(options);
					case "index": return member.Value.Deserialize<BulkIndexResponseItem>(options);
					case "create": return member.Value.Deserialize<BulkCreateResponseItem>(options);
				}
			}

			return null;
		}

		public override void Write(Utf8JsonWriter writer, BulkResponseItemBase value, JsonSerializerOptions options) =>
			throw new NotSupportedException($"{nameof(BulkResponseItemBase)} is a response type and is not serialized.");
	}
}
