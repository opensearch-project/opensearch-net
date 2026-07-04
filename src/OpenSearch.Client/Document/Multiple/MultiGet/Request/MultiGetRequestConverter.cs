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
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// The <c>System.Text.Json</c> converter for <see cref="IMultiGetRequest"/> (#388), replacing the
	/// vendored <c>MultiGetRequestFormatter</c>. Emits <c>{ "ids": [ … ] }</c> when every document can be
	/// flattened to a bare id, otherwise <c>{ "docs": [ … ] }</c>. A request-level index that matches a
	/// document's index is elided from that document.
	/// </summary>
	internal sealed class MultiGetRequestConverterFactory : JsonConverterFactory
	{
		private readonly IConnectionSettingsValues _settings;

		public MultiGetRequestConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

		public override bool CanConvert(Type typeToConvert) => typeof(IMultiGetRequest).IsAssignableFrom(typeToConvert);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
			(JsonConverter)Activator.CreateInstance(typeof(MultiGetRequestConverter<>).MakeGenericType(typeToConvert), _settings);
	}

	internal sealed class MultiGetRequestConverter<T> : JsonConverter<T>
	{
		private readonly IConnectionSettingsValues _settings;

		public MultiGetRequestConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			var request = (IMultiGetRequest)value;

			writer.WriteStartObject();
			if (!(request?.Documents.HasAny()).GetValueOrDefault(false))
			{
				writer.WriteEndObject();
				return;
			}

			List<IMultiGetOperation> docs;
			if (request.Index != null)
			{
				var resolvedIndex = request.Index.GetString(_settings);
				docs = request.Documents.Select(d =>
				{
					if (d.Index == null) return d;
					if (string.Equals(resolvedIndex, d.Index.GetString(_settings))) d.Index = null;
					return d;
				}).ToList();
			}
			else
				docs = request.Documents.ToList();

			var flatten = docs.All(p => p.CanBeFlattened);

			writer.WritePropertyName(flatten ? "ids" : "docs");
			writer.WriteStartArray();
			foreach (var doc in docs)
			{
				if (flatten)
					JsonSerializer.Serialize(writer, doc.Id, options);
				else
					JsonSerializer.Serialize(writer, doc, doc.GetType(), options);
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}
}
