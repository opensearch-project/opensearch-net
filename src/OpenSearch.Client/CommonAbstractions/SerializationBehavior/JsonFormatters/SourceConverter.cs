/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>SourceFormatter&lt;T&gt;</c>. A document body (a
	/// <c>_source</c>, an update <c>doc</c>/<c>upsert</c>, etc.) is (de)serialized through the connection settings'
	/// <see cref="IConnectionSettingsValues.SourceSerializer"/> rather than the request serializer, so a user-supplied
	/// source serializer (e.g. the Newtonsoft-based JsonNetSerializer) governs the document shape. The serialized
	/// bytes are spliced into the output via <c>WriteRawValue</c>; on read the value is buffered and handed to the
	/// source serializer.
	/// </summary>
	internal class SourceConverter<T> : SettingsAwareConverter<T>
	{
		public SourceConverter(IConnectionSettingsValues settings) : base(settings) { }

		protected virtual SerializationFormatting Formatting => SerializationFormatting.None;

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
			{
				reader.Read();
				return default;
			}

			using var doc = JsonDocument.ParseValue(ref reader);
			var bytes = System.Text.Encoding.UTF8.GetBytes(doc.RootElement.GetRawText());
			using var ms = Settings.MemoryStreamFactory.Create(bytes);
			return Settings.SourceSerializer.Deserialize<T>(ms);
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			WriteThroughSourceSerializer(writer, value);
		}

		protected void WriteThroughSourceSerializer(Utf8JsonWriter writer, T value)
		{
			using var ms = Settings.MemoryStreamFactory.Create();
			Settings.SourceSerializer.Serialize(value, ms, Formatting);
			writer.WriteRawValue(ms.ToArray(), skipInputValidation: true);
		}
	}

	/// <summary>
	/// <see cref="SourceConverter{T}"/> that forces compact (None) formatting, replacing the legacy
	/// <c>CollapsedSourceFormatter&lt;T&gt;</c>. (SourceConverter already defaults to None, so this is a distinct type
	/// only to keep the formatter→converter mapping one-to-one.)
	/// </summary>
	internal class CollapsedSourceConverter<T> : SourceConverter<T>
	{
		public CollapsedSourceConverter(IConnectionSettingsValues settings) : base(settings) { }
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy <c>SourceWriteFormatter&lt;T&gt;</c>: an OpenSearch.Client type is
	/// written through the request serializer (so its registered converters apply), while any other (user document)
	/// type is written through the source serializer.
	/// </summary>
	internal class SourceWriteConverter<T> : SourceConverter<T>
	{
		public SourceWriteConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (value.GetType().IsOpenSearchClientType())
				JsonSerializer.Serialize(writer, value, value.GetType(), options);
			else
				WriteThroughSourceSerializer(writer, value);
		}
	}
}
