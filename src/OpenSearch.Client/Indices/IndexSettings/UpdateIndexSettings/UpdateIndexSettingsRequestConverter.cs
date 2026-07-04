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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// The <c>System.Text.Json</c> converter for <see cref="IUpdateIndexSettingsRequest"/> (#388),
	/// replacing the vendored <c>UpdateIndexSettingsRequestFormatter</c>. The body is the request's
	/// (dotted-key flattened) dynamic index settings.
	/// </summary>
	internal sealed class UpdateIndexSettingsRequestConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) => typeof(IUpdateIndexSettingsRequest).IsAssignableFrom(typeToConvert);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
			(JsonConverter)Activator.CreateInstance(typeof(UpdateIndexSettingsRequestConverter<>).MakeGenericType(typeToConvert));
	}

	internal sealed class UpdateIndexSettingsRequestConverter<T> : JsonConverter<T>
	{
		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			var request = (IUpdateIndexSettingsRequest)value;
			if (request?.IndexSettings == null)
			{
				writer.WriteStartObject();
				writer.WriteEndObject();
				return;
			}

			JsonSerializer.Serialize(writer, request.IndexSettings, typeof(IDynamicIndexSettings), options);
		}

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}
}
