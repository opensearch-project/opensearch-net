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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// The <see cref="System.Text.Json"/> counterpart of the vendored <c>ProxyRequestFormatterBase</c>
	/// (#388). <see cref="IIndexRequest{TDocument}"/> / <see cref="ICreateRequest{TDocument}"/> serialize
	/// as their document body: the request itself carries no wire members, the body is the document
	/// written through the source serializer (<see cref="IProxyRequest.WriteJson"/>). Without this the
	/// request/response contract resolver would emit an empty object.
	/// </summary>
	internal sealed class ProxyRequestConverterFactory : JsonConverterFactory
	{
		private readonly IConnectionSettingsValues _settings;

		public ProxyRequestConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

		public override bool CanConvert(Type typeToConvert) => typeof(IProxyRequest).IsAssignableFrom(typeToConvert);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
			(JsonConverter)Activator.CreateInstance(typeof(ProxyRequestConverter<>).MakeGenericType(typeToConvert), _settings);
	}

	internal sealed class ProxyRequestConverter<T> : JsonConverter<T>
	{
		private readonly IConnectionSettingsValues _settings;

		public ProxyRequestConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			using var stream = _settings.MemoryStreamFactory.Create();
			((IProxyRequest)value).WriteJson(_settings.SourceSerializer, stream, SerializationFormatting.None);
			// Write the source bytes verbatim (like the vendored WriteRaw) rather than re-emitting through
			// the outer writer, which would re-indent a compact document body when PrettyJson is enabled.
			writer.WriteRawValue(stream.ToArray(), skipInputValidation: true);
		}

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);

			var requestInterface = FindProxyInterface(typeToConvert);
			var concreteOpenType = ResolveConcreteOpenType(requestInterface);
			if (requestInterface == null || concreteOpenType == null)
				throw new NotSupportedException($"Cannot deserialize {typeToConvert} as a document (proxy) request.");

			var documentType = requestInterface.GetGenericArguments()[0];
			var bytes = Encoding.UTF8.GetBytes(document.RootElement.GetRawText());
			object documentValue;
			using (var stream = _settings.MemoryStreamFactory.Create(bytes, 0, bytes.Length))
				documentValue = _settings.SourceSerializer.Deserialize(documentType, stream);

			return (T)concreteOpenType.CreateGenericInstance(documentType, documentValue, null, null);
		}

		private static Type FindProxyInterface(Type type)
		{
			if (IsProxyInterface(type)) return type;
			foreach (var i in type.GetInterfaces())
				if (IsProxyInterface(i))
					return i;
			return null;
		}

		private static bool IsProxyInterface(Type type)
		{
			if (!type.IsGenericType) return false;
			var definition = type.GetGenericTypeDefinition();
			return definition == typeof(IIndexRequest<>) || definition == typeof(ICreateRequest<>);
		}

		private static Type ResolveConcreteOpenType(Type requestInterface)
		{
			if (requestInterface == null) return null;
			var definition = requestInterface.GetGenericTypeDefinition();
			if (definition == typeof(IIndexRequest<>)) return typeof(IndexRequest<>);
			if (definition == typeof(ICreateRequest<>)) return typeof(CreateRequest<>);
			return null;
		}
	}
}
