/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonFormatterAttribute = OpenSearch.Net.Utf8Json.JsonFormatterAttribute;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>CreateRequestFormatter&lt;TDocument&gt;</c> (a
	/// <c>ProxyRequestFormatterBase</c>). A create request is a <em>proxy request</em>: the wire body IS the document
	/// (the <see cref="ICreateRequest{TDocument}.Document"/> property) serialized directly; every other member of the
	/// request wrapper (index, id, refresh, routing, …) is a URL/query parameter and never appears in the body. This
	/// converter therefore writes <c>request.Document</c> straight to the writer and, mirroring the legacy
	/// <c>Deserialize</c>, reconstructs a <see cref="CreateRequest{TDocument}"/> from a body read back as the document.
	///
	/// The document is (de)serialized through the supplied <see cref="JsonSerializerOptions"/> so the settings-aware
	/// contract resolver and every registered converter apply — matching the legacy path that delegated to the
	/// source serializer via <c>IProxyRequest.WriteJson</c>.
	/// </summary>
	internal class CreateRequestConverter<TDocument> : JsonConverter<ICreateRequest<TDocument>>
		where TDocument : class
	{
		public override ICreateRequest<TDocument> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// The legacy ProxyRequestFormatterBase.Deserialize read the whole body as the document and rebuilt the
			// request. Index/create request bodies are outgoing only, so this path is effectively unused, but it is
			// preserved for parity. CreateRequest<TDocument> has only a protected parameterless ctor, so a null /
			// empty body (a degenerate create request) yields null rather than a document-less request.
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			var document = JsonSerializer.Deserialize<TDocument>(ref reader, options);
			return document == null ? null : new CreateRequest<TDocument>(document);
		}

		public override void Write(Utf8JsonWriter writer, ICreateRequest<TDocument> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// The body IS the document. Serialize with the compile-time TDocument (the legacy source serializer used
			// the static document type), which the options' resolver renders with field-name inference and mappings.
			JsonSerializer.Serialize(writer, value.Document, options);
		}
	}

	/// <summary>
	/// Constructs the closed <see cref="CreateRequestConverter{TDocument}"/> per document type. This is a
	/// <see cref="JsonConverterFactory"/> because <see cref="CreateRequestConverter{TDocument}"/> is an open generic
	/// that cannot be added to <c>JsonSerializerOptions.Converters</c> directly.
	///
	/// The bound type is discovered from the legacy <c>[JsonFormatter(typeof(CreateRequestFormatter&lt;&gt;))]</c>
	/// attribute already annotating <see cref="ICreateRequest{TDocument}"/>. As the attribute names an <em>open</em>
	/// unbound generic, the document type argument is taken from the interface being converted, keeping the STJ path
	/// in lock-step with the legacy mapping without any per-type registration.
	/// </summary>
	internal class CreateRequestConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) => IsCreateRequestProxy(typeToConvert);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (!IsCreateRequestProxy(typeToConvert))
				return null;

			var document = typeToConvert.GetGenericArguments()[0];
			var converterType = typeof(CreateRequestConverter<>).MakeGenericType(document);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}

		// Only handle a closed generic interface whose legacy [JsonFormatter] points at the open CreateRequestFormatter<>.
		private static bool IsCreateRequestProxy(Type typeToConvert)
		{
			if (!typeToConvert.IsGenericType)
				return false;

			var formatterType = typeToConvert.GetCustomAttribute<JsonFormatterAttribute>()?.FormatterType;
			return formatterType != null
				&& formatterType.IsGenericTypeDefinition
				&& formatterType.GetGenericTypeDefinition() == typeof(CreateRequestFormatter<>);
		}
	}
}
