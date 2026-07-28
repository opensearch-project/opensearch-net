/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>RoutingFormatter</c>. Resolves a
	/// <see cref="Routing"/> built from a document instance through the runtime <c>Inferrer</c> when
	/// serializing — hence a <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class RoutingConverter : SettingsAwareConverter<Routing>
	{
		public RoutingConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override Routing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Number
				? new Routing(reader.GetInt64())
				: new Routing(reader.GetString());

		public override void Write(Utf8JsonWriter writer, Routing value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (value.Document != null)
			{
				var documentId = Settings.Inferrer.Routing(value.Document.GetType(), value.Document);
				writer.WriteStringValue(documentId);
			}
			else if (value.DocumentGetter != null)
			{
				var doc = value.DocumentGetter();
				var documentId = Settings.Inferrer.Routing(doc.GetType(), doc);
				writer.WriteStringValue(documentId);
			}
			else if (value.LongValue != null) writer.WriteNumberValue(value.LongValue.Value);
			else writer.WriteStringValue(value.StringValue);
		}
	}
}
