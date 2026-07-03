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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Routing"/>, replacing the vendored
	/// Utf8Json <c>RoutingFormatter</c> as part of #388. Serializes an inferred document routing, a raw
	/// <see cref="long"/> as a number, or a string value; deserializes a JSON number into a long-based
	/// routing and any other token into a string-based routing. Constructed with the connection
	/// settings for document-routing inference.
	/// </summary>
	internal sealed class RoutingConverter : JsonConverter<Routing>
	{
		private readonly IConnectionSettingsValues _settings;

		public RoutingConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, Routing value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (value.Document != null)
			{
				var documentId = _settings.Inferrer.Routing(value.Document.GetType(), value.Document);
				writer.WriteStringValue(documentId);
			}
			else if (value.DocumentGetter != null)
			{
				var doc = value.DocumentGetter();
				var documentId = _settings.Inferrer.Routing(doc.GetType(), doc);
				writer.WriteStringValue(documentId);
			}
			else if (value.LongValue != null)
				writer.WriteNumberValue(value.LongValue.Value);
			else
				writer.WriteStringValue(value.StringValue);
		}

		public override Routing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Number
				? new Routing(reader.GetInt64())
				: new Routing(reader.GetString());
	}
}
