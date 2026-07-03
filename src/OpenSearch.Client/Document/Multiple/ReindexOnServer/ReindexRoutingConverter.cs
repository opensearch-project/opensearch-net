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
	/// A <see cref="System.Text.Json"/> converter for <see cref="ReindexRouting"/>, replacing the
	/// vendored Utf8Json <c>ReindexRoutingFormatter</c> as part of #388. The literals <c>"keep"</c>
	/// and <c>"discard"</c> map to <see cref="ReindexRouting.Keep"/> / <see cref="ReindexRouting.Discard"/>;
	/// any other string becomes a new routing value (prefixed with <c>=</c> by the type's constructor).
	/// On write the value's string form is emitted verbatim.
	/// </summary>
	internal sealed class ReindexRoutingConverter : JsonConverter<ReindexRouting>
	{
		public override void Write(Utf8JsonWriter writer, ReindexRouting value, JsonSerializerOptions options)
		{
			if (value == null)
				writer.WriteNullValue();
			else
				writer.WriteStringValue(value.ToString());
		}

		public override ReindexRouting Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			var value = reader.GetString();
			switch (value)
			{
				case "keep": return ReindexRouting.Keep;
				case "discard": return ReindexRouting.Discard;
				default: return new ReindexRouting(value);
			}
		}
	}
}
