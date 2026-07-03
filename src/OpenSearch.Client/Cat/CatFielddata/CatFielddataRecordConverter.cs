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
	/// A read-only <see cref="System.Text.Json"/> converter for <see cref="CatFielddataRecord"/>,
	/// replacing the vendored Utf8Json <c>CatFielddataRecordFormatter</c> as part of #388. A flat object
	/// of string fields; <c>node</c> is also accepted under its abbreviated <c>n</c> alias.
	/// </summary>
	internal sealed class CatFielddataRecordConverter : JsonConverter<CatFielddataRecord>
	{
		public override CatFielddataRecord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var record = new CatFielddataRecord();
			foreach (var member in document.RootElement.EnumerateObject())
			{
				switch (member.Name)
				{
					case "id": record.Id = member.Value.GetString(); break;
					case "node":
					case "n": record.Node = member.Value.GetString(); break;
					case "host": record.Host = member.Value.GetString(); break;
					case "ip": record.Ip = member.Value.GetString(); break;
					case "field": record.Field = member.Value.GetString(); break;
					case "size": record.Size = member.Value.GetString(); break;
				}
			}

			return record;
		}

		public override void Write(Utf8JsonWriter writer, CatFielddataRecord value, JsonSerializerOptions options) =>
			throw new NotSupportedException($"{nameof(CatFielddataRecord)} is a response type and is not serialized.");
	}
}
