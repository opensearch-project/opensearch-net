/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text;
using System.Text.Json;
using OpenSearch.Net.Utf8Json; // IJsonFormatterResolver (reused for source serializer parity)

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>LazyDocumentFormatter</c>.
	///
	/// A <see cref="LazyDocument"/> captures a JSON value verbatim so it can be deserialized later, on demand, into an
	/// arbitrary type (via the connection's source/request-response serializer). The crux is raw-JSON fidelity:
	/// <list type="bullet">
	/// <item><description>On <b>read</b> the whole value is buffered and its raw JSON text is captured into
	/// <see cref="LazyDocument.Bytes"/> (via <see cref="JsonElement.GetRawText"/>), exactly as the legacy formatter
	/// copied the raw byte segment. <see cref="Utf8JsonReader"/> is forward-only, so we buffer into a
	/// <see cref="JsonDocument"/>.</description></item>
	/// <item><description>On <b>write</b> the captured value is re-emitted <i>unindented</i> — the legacy formatter's
	/// <c>WriteUnindented</c> re-parsed the stored bytes and wrote them compactly; <see cref="JsonElement.WriteTo"/>
	/// through a (compact-by-default) <see cref="Utf8JsonWriter"/> reproduces that, preserving raw number formatting.
	/// </description></item>
	/// </list>
	/// The converter is <see cref="SettingsAwareConverter{T}"/> because constructing a <see cref="LazyDocument"/>
	/// requires the connection settings (source serializer / memory-stream factory), which the legacy formatter
	/// obtained from the formatter resolver.
	/// </summary>
	internal class LazyDocumentConverter : SettingsAwareConverter<LazyDocument>
	{
		private IJsonFormatterResolver _resolver;

		public LazyDocumentConverter(IConnectionSettingsValues settings) : base(settings) { }

		private IJsonFormatterResolver Resolver =>
			_resolver ?? (_resolver = new OpenSearchClientFormatterResolver(Settings));

		public override LazyDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var bytes = Encoding.UTF8.GetBytes(doc.RootElement.GetRawText());
			return new LazyDocument(bytes, Resolver);
		}

		public override void Write(Utf8JsonWriter writer, LazyDocument value, JsonSerializerOptions options)
		{
			if (value?.Bytes == null)
			{
				writer.WriteNullValue();
				return;
			}

			using var doc = JsonDocument.Parse(value.Bytes);
			doc.RootElement.WriteTo(writer);
		}
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>LazyDocumentInterfaceFormatter</c>. Behaves like
	/// <see cref="LazyDocumentConverter"/> for the <see cref="ILazyDocument"/> abstraction: reads a captured
	/// <see cref="LazyDocument"/>, and on write emits a concrete <see cref="LazyDocument"/> unindented (any other
	/// <see cref="ILazyDocument"/> implementation, or null, is written as JSON <c>null</c> — matching the legacy
	/// formatter's <c>default</c> branch).
	/// </summary>
	internal class LazyDocumentInterfaceConverter : SettingsAwareConverter<ILazyDocument>
	{
		private IJsonFormatterResolver _resolver;

		public LazyDocumentInterfaceConverter(IConnectionSettingsValues settings) : base(settings) { }

		private IJsonFormatterResolver Resolver =>
			_resolver ?? (_resolver = new OpenSearchClientFormatterResolver(Settings));

		public override ILazyDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var bytes = Encoding.UTF8.GetBytes(doc.RootElement.GetRawText());
			return new LazyDocument(bytes, Resolver);
		}

		public override void Write(Utf8JsonWriter writer, ILazyDocument value, JsonSerializerOptions options)
		{
			switch (value)
			{
				case LazyDocument lazyDocument when lazyDocument.Bytes != null:
					using (var doc = JsonDocument.Parse(lazyDocument.Bytes))
						doc.RootElement.WriteTo(writer);
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}
	}
}
