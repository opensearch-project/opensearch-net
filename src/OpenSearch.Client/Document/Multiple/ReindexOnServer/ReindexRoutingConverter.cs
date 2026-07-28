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
	/// System.Text.Json replacement for the legacy Utf8Json <c>ReindexRoutingFormatter</c> (which, despite the
	/// <c>*JsonConverter.cs</c> file name, was an <c>IJsonFormatter&lt;ReindexRouting&gt;</c>). Reads a JSON string:
	/// <c>"keep"</c> and <c>"discard"</c> map to the shared <see cref="ReindexRouting.Keep"/> /
	/// <see cref="ReindexRouting.Discard"/> instances, any other string becomes a new prefixed
	/// <see cref="ReindexRouting"/>. Writes <c>null</c> as JSON null, otherwise the
	/// <see cref="ReindexRouting.ToString"/> string.
	/// </summary>
	internal class ReindexRoutingConverter : JsonConverter<ReindexRouting>
	{
		public override ReindexRouting Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var value = reader.GetString();
			switch (value)
			{
				case "keep": return ReindexRouting.Keep;
				case "discard": return ReindexRouting.Discard;
				default: return new ReindexRouting(value);
			}
		}

		public override void Write(Utf8JsonWriter writer, ReindexRouting value, JsonSerializerOptions options)
		{
			if (value == null) writer.WriteNullValue();
			else writer.WriteStringValue(value.ToString());
		}
	}
}
