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
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>GetRepositoryResponseFormatter</c>.
	///
	/// The wire body is a flat map of <c>repositoryName -&gt; { "type": "&lt;t&gt;", "settings": { ... } }</c>, optionally
	/// carrying the standard server-error envelope fields (<c>error</c>, <c>status</c>). Each repository is dispatched to
	/// a concrete type by its <c>type</c> discriminator:
	/// <list type="bullet">
	/// <item><c>fs</c> → <see cref="FileSystemRepository"/> / <see cref="FileSystemRepositorySettings"/></item>
	/// <item><c>url</c> → <see cref="ReadOnlyUrlRepository"/> / <see cref="ReadOnlyUrlRepositorySettings"/></item>
	/// <item><c>azure</c> → <see cref="AzureRepository"/> / <see cref="AzureRepositorySettings"/></item>
	/// <item><c>s3</c> → <see cref="S3Repository"/> / <see cref="S3RepositorySettings"/></item>
	/// <item><c>hdfs</c> → <see cref="HdfsRepository"/> / <see cref="HdfsRepositorySettings"/></item>
	/// <item><c>source</c> → delegated to <see cref="ISourceOnlyRepository"/> (the whole repository body is handed to
	/// its converter, which reads the nested <c>settings.delegate_type</c>).</item>
	/// </list>
	/// Any other <c>type</c> value is skipped (not added), exactly as in the legacy formatter.
	///
	/// <see cref="Utf8JsonReader"/> is forward-only and the legacy formatter re-scanned each repository body twice (once
	/// for <c>type</c>, once for <c>settings</c>), so we buffer into a <see cref="JsonDocument"/> and read from the DOM.
	///
	/// NOTE on the write path: the legacy formatter delegated to a dynamic camelCase reflection serializer that emitted
	/// the response object's public properties (never the flat wire map). Responses are only ever read from the server,
	/// never sent, so that output was effectively unused and non-round-trippable. To keep read/write symmetric (and to
	/// support round-trip tests) this converter instead writes the canonical flat repository map, dispatching each value
	/// by its runtime type. This is a deliberate, documented divergence — see the migration report.
	/// </summary>
	internal class GetRepositoryResponseConverter : JsonConverter<GetRepositoryResponse>
	{
		// A JSON null must yield an empty response (matching the legacy formatter). STJ skips the converter for a
		// null reference type unless HandleNull is true, so opt in and treat the null token as an empty object.
		public override bool HandleNull => true;

		public override GetRepositoryResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return new GetRepositoryResponse();

			var response = new GetRepositoryResponse();
			var repositories = new Dictionary<string, ISnapshotRepository>();

			if (reader.TokenType == JsonTokenType.Null)
			{
				response.Repositories = repositories;
				return response;
			}

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
			{
				response.Repositories = repositories;
				return response;
			}

			foreach (var property in root.EnumerateObject())
			{
				switch (property.Name)
				{
					case "error":
						if (property.Value.ValueKind == JsonValueKind.String)
							response.Error = new Error { Reason = property.Value.GetString() };
						else
							response.Error = property.Value.Deserialize<Error>(options);
						continue;
					case "status":
						if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out var status))
							response.StatusCode = status;
						continue;
				}

				var name = property.Name;
				var repositoryBody = property.Value;

				if (repositoryBody.ValueKind != JsonValueKind.Object)
					continue;

				string repositoryType = null;
				if (repositoryBody.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
					repositoryType = typeElement.GetString();

				JsonElement settings = default;
				var hasSettings = repositoryBody.TryGetProperty("settings", out settings);

				switch (repositoryType)
				{
					case "fs":
						repositories.Add(name, GetRepository<FileSystemRepository, FileSystemRepositorySettings>(hasSettings, settings, options));
						break;
					case "url":
						repositories.Add(name, GetRepository<ReadOnlyUrlRepository, ReadOnlyUrlRepositorySettings>(hasSettings, settings, options));
						break;
					case "azure":
						repositories.Add(name, GetRepository<AzureRepository, AzureRepositorySettings>(hasSettings, settings, options));
						break;
					case "s3":
						repositories.Add(name, GetRepository<S3Repository, S3RepositorySettings>(hasSettings, settings, options));
						break;
					case "hdfs":
						repositories.Add(name, GetRepository<HdfsRepository, HdfsRepositorySettings>(hasSettings, settings, options));
						break;
					case "source":
						// Legacy added the (possibly null) result unconditionally; preserve that.
						repositories.Add(name, repositoryBody.Deserialize<ISourceOnlyRepository>(options));
						break;
				}
			}

			response.Repositories = repositories;
			return response;
		}

		public override void Write(Utf8JsonWriter writer, GetRepositoryResponse value, JsonSerializerOptions options)
		{
			if (value?.Repositories == null)
			{
				writer.WriteStartObject();
				writer.WriteEndObject();
				return;
			}

			writer.WriteStartObject();
			foreach (var kvp in value.Repositories)
			{
				writer.WritePropertyName(kvp.Key);
				var repository = kvp.Value;
				if (repository == null)
				{
					writer.WriteNullValue();
					continue;
				}

				JsonSerializer.Serialize(writer, repository, repository.GetType(), options);
			}
			writer.WriteEndObject();
		}

		private static TRepository GetRepository<TRepository, TSettings>(bool hasSettings, JsonElement settings, JsonSerializerOptions options)
			where TRepository : ISnapshotRepository
			where TSettings : IRepositorySettings
		{
			if (!hasSettings || settings.ValueKind != JsonValueKind.Object)
				return typeof(TRepository).CreateInstance<TRepository>();

			var resolvedSettings = settings.Deserialize<TSettings>(options);
			return typeof(TRepository).CreateInstance<TRepository>(resolvedSettings);
		}
	}
}
