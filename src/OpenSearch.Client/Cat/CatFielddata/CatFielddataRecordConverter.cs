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
	/// System.Text.Json replacement for the legacy Utf8Json <c>CatFielddataRecordFormatter</c> (the legacy type lived
	/// in a file misleadingly named <c>CatFielddataRecordJsonConverter.cs</c> but was in fact an
	/// <c>IJsonFormatter&lt;CatFielddataRecord&gt;</c>).
	///
	/// Reads a <c>_cat/fielddata</c> record object, mapping each column (with its short aliases) to a property:
	/// <c>id</c>, <c>node</c>/<c>n</c>, <c>host</c>, <c>ip</c>, <c>field</c>, <c>size</c>. Unknown columns are skipped.
	/// Serialization is not supported (the legacy formatter threw <see cref="NotSupportedException"/>).
	/// </summary>
	internal class CatFielddataRecordConverter : JsonConverter<CatFielddataRecord>
	{
		public override CatFielddataRecord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			var record = new CatFielddataRecord();
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				var property = reader.GetString();
				reader.Read();

				switch (property)
				{
					case "id":
						record.Id = ReadStringOrNull(ref reader);
						break;
					case "node":
					case "n":
						record.Node = ReadStringOrNull(ref reader);
						break;
					case "host":
						record.Host = ReadStringOrNull(ref reader);
						break;
					case "ip":
						record.Ip = ReadStringOrNull(ref reader);
						break;
					case "field":
						record.Field = ReadStringOrNull(ref reader);
						break;
					case "size":
						record.Size = ReadStringOrNull(ref reader);
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return record;
		}

		// CatFielddataRecord is a read-only response record (the client only ever deserializes _cat/fielddata results;
		// it is never serialized into a request), so writing is intentionally unsupported. This mirrors the legacy
		// Utf8Json formatter, whose Serialize threw for the same reason.
		public override void Write(Utf8JsonWriter writer, CatFielddataRecord value, JsonSerializerOptions options) =>
			throw new NotSupportedException("CatFielddataRecord is a read-only response type and is never serialized.");

		private static string ReadStringOrNull(ref Utf8JsonReader reader) =>
			reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
	}
}
