/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonFormatterAttribute = OpenSearch.Net.Utf8Json.JsonFormatterAttribute;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>IndexRequestFormatter&lt;TDocument&gt;</c> (a
	/// <c>ProxyRequestFormatterBase</c>). An index request is a <em>proxy request</em>: the wire body IS the document
	/// (the <see cref="IIndexRequest{TDocument}.Document"/> property) serialized directly; every other member of the
	/// request wrapper (index, id, refresh, op_type, routing, …) is a URL/query parameter and never appears in the
	/// body. This converter therefore writes <c>request.Document</c> straight to the writer and, mirroring the legacy
	/// <c>Deserialize</c>, reconstructs an <see cref="IndexRequest{TDocument}"/> from a body read back as the document.
	///
	/// The document is (de)serialized through the supplied <see cref="JsonSerializerOptions"/> so the settings-aware
	/// contract resolver and every registered converter apply — matching the legacy path that delegated to the
	/// source serializer via <c>IProxyRequest.WriteJson</c>.
	/// </summary>
	internal class IndexRequestConverter<TDocument> : JsonConverter<IIndexRequest<TDocument>>
		where TDocument : class
	{
		private readonly IConnectionSettingsValues _settings;

		public IndexRequestConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override IIndexRequest<TDocument> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// The legacy ProxyRequestFormatterBase.Deserialize read the whole body as the document and rebuilt the
			// request. Index/create request bodies are outgoing only, so this path is effectively unused, but it is
			// preserved for parity. A JSON null yields a request with no document.
			if (reader.TokenType == JsonTokenType.Null)
				return new IndexRequest<TDocument>();

			var document = JsonSerializer.Deserialize<TDocument>(ref reader, options);
			return document == null ? new IndexRequest<TDocument>() : new IndexRequest<TDocument>(document);
		}

		public override void Write(Utf8JsonWriter writer, IIndexRequest<TDocument> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// The body IS the document. It must be written through the connection's SourceSerializer (as the legacy
			// IProxyRequest.WriteJson did) so a user-supplied source serializer governs the document shape.
			ProxyRequestDocumentWriter.Write(writer, value.Document, _settings, options);
		}
	}

	/// <summary>
	/// Constructs the closed <see cref="IndexRequestConverter{TDocument}"/> per document type. This is a
	/// <see cref="JsonConverterFactory"/> because <see cref="IndexRequestConverter{TDocument}"/> is an open generic
	/// that cannot be added to <c>JsonSerializerOptions.Converters</c> directly.
	///
	/// The bound type is discovered from the legacy <c>[JsonFormatter(typeof(IndexRequestFormatter&lt;&gt;))]</c>
	/// attribute already annotating <see cref="IIndexRequest{TDocument}"/>. Unlike the field-name query factory —
	/// whose attribute names a <em>closed</em> formatter — this attribute names an <em>open</em> unbound generic, so
	/// the document type argument is taken from the interface being converted, keeping the STJ path in lock-step with
	/// the legacy mapping without any per-type registration.
	/// </summary>
	internal class IndexRequestConverterFactory : JsonConverterFactory
	{
		private readonly IConnectionSettingsValues _settings;

		public IndexRequestConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

		public override bool CanConvert(Type typeToConvert) => IsIndexRequestProxy(typeToConvert);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (!IsIndexRequestProxy(typeToConvert))
				return null;

			var document = typeToConvert.GetGenericArguments()[0];
			var converterType = typeof(IndexRequestConverter<>).MakeGenericType(document);
			return (JsonConverter)Activator.CreateInstance(converterType, _settings);
		}

		// Only handle a closed generic interface whose legacy [JsonFormatter] points at the open IndexRequestFormatter<>.
		private static bool IsIndexRequestProxy(Type typeToConvert)
		{
			if (!typeToConvert.IsGenericType)
				return false;

			var formatterType = typeToConvert.GetCustomAttribute<JsonFormatterAttribute>()?.FormatterType;
			return formatterType != null
				&& formatterType.IsGenericTypeDefinition
				&& formatterType.GetGenericTypeDefinition() == typeof(IndexRequestFormatter<>);
		}
	}
}
