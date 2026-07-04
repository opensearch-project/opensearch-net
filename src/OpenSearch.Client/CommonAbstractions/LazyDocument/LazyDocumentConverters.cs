/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="LazyDocument"/>, replacing the vendored
	/// Utf8Json <c>LazyDocumentFormatter</c> as part of #388. On read it captures the raw JSON of the
	/// current value into a <see cref="LazyDocument"/> for deferred deserialization (via the configured
	/// source/request-response serializer); on write it replays the captured JSON. Constructed with the
	/// connection settings, which the captured document uses for its later <c>As&lt;T&gt;()</c> calls.
	/// </summary>
	internal sealed class LazyDocumentConverter : JsonConverter<LazyDocument>
	{
		private readonly IConnectionSettingsValues _settings;

		public LazyDocumentConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override LazyDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var bytes = Encoding.UTF8.GetBytes(document.RootElement.GetRawText());
			return new LazyDocument(bytes, _settings);
		}

		public override void Write(Utf8JsonWriter writer, LazyDocument value, JsonSerializerOptions options)
		{
			if (value?.Bytes == null)
			{
				writer.WriteNullValue();
				return;
			}

			using var document = JsonDocument.Parse(value.Bytes);
			document.RootElement.WriteTo(writer);
		}
	}

	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="ILazyDocument"/> (#388); reads into a
	/// concrete <see cref="LazyDocument"/> and writes the captured JSON.
	/// </summary>
	internal sealed class LazyDocumentInterfaceConverter : JsonConverter<ILazyDocument>
	{
		private readonly IConnectionSettingsValues _settings;

		public LazyDocumentInterfaceConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override ILazyDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var bytes = Encoding.UTF8.GetBytes(document.RootElement.GetRawText());
			return new LazyDocument(bytes, _settings);
		}

		public override void Write(Utf8JsonWriter writer, ILazyDocument value, JsonSerializerOptions options)
		{
			if (value is LazyDocument lazyDocument && lazyDocument.Bytes != null)
			{
				using var document = JsonDocument.Parse(lazyDocument.Bytes);
				document.RootElement.WriteTo(writer);
				return;
			}

			writer.WriteNullValue();
		}
	}
}
