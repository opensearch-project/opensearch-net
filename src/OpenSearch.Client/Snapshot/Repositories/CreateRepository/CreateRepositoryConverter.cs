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
	/// The <c>System.Text.Json</c> converter for <see cref="ICreateRepositoryRequest"/> (#388), replacing
	/// the vendored <c>CreateRepositoryFormatter</c>. The body is the repository serialized by its concrete
	/// runtime type (its <c>type</c> discriminator + <c>settings</c>).
	/// </summary>
	internal sealed class CreateRepositoryConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) => typeof(ICreateRepositoryRequest).IsAssignableFrom(typeToConvert);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
			(JsonConverter)Activator.CreateInstance(typeof(CreateRepositoryConverter<>).MakeGenericType(typeToConvert));
	}

	internal sealed class CreateRepositoryConverter<T> : JsonConverter<T>
	{
		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			var repository = ((ICreateRepositoryRequest)value)?.Repository;
			if (repository == null)
			{
				writer.WriteStartObject();
				writer.WriteEndObject();
				return;
			}

			JsonSerializer.Serialize(writer, repository, repository.GetType(), options);
		}

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}
}
