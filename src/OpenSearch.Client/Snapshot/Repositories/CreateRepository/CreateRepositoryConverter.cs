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
	/// System.Text.Json replacement for the legacy Utf8Json <c>CreateRepositoryFormatter</c>.
	///
	/// An <see cref="ICreateRepositoryRequest"/> body is just its polymorphic <see cref="ICreateRepositoryRequest.Repository"/>,
	/// serialized as the concrete repository interface selected by the repository's <see cref="ISnapshotRepository.Type"/>
	/// discriminator (<c>s3</c>, <c>azure</c>, <c>url</c>, <c>hdfs</c>, <c>fs</c>, <c>source</c>; any other value falls
	/// back to the base <see cref="ISnapshotRepository"/>). This mirrors the legacy write-time dispatch exactly.
	///
	/// As in the legacy formatter, deserialization is not supported (the request is only ever written by the client).
	/// A null request or a request with a null repository writes an empty object <c>{}</c>.
	/// </summary>
	internal class CreateRepositoryConverter : JsonConverter<ICreateRepositoryRequest>
	{
		// A null request must serialize as an empty object (matching the legacy formatter). STJ skips the converter
		// for a null reference type unless HandleNull is true.
		public override bool HandleNull => true;

		public override ICreateRepositoryRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, ICreateRepositoryRequest value, JsonSerializerOptions options)
		{
			if (value?.Repository == null)
			{
				writer.WriteStartObject();
				writer.WriteEndObject();
				return;
			}

			switch (value.Repository.Type)
			{
				case "s3":
					Serialize<IS3Repository>(writer, value.Repository, options);
					break;
				case "azure":
					Serialize<IAzureRepository>(writer, value.Repository, options);
					break;
				case "url":
					Serialize<IReadOnlyUrlRepository>(writer, value.Repository, options);
					break;
				case "hdfs":
					Serialize<IHdfsRepository>(writer, value.Repository, options);
					break;
				case "fs":
					Serialize<IFileSystemRepository>(writer, value.Repository, options);
					break;
				case "source":
					Serialize<ISourceOnlyRepository>(writer, value.Repository, options);
					break;
				default:
					Serialize<ISnapshotRepository>(writer, value.Repository, options);
					break;
			}
		}

		private static void Serialize<TRepository>(Utf8JsonWriter writer, ISnapshotRepository value, JsonSerializerOptions options)
			where TRepository : class, ISnapshotRepository =>
			JsonSerializer.Serialize(writer, value as TRepository, options);
	}
}
