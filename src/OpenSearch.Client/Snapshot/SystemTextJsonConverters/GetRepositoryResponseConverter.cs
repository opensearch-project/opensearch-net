/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Collections.Generic;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.SnapshotConverters
{
	/// <summary>
	/// Converter for <see cref="GetRepositoryResponse"/>.
	/// The response is a dictionary-like object where keys are repository names:
	/// <code>
	/// {
	///   "my_backup": {"type": "fs", "settings": {"location": "/mnt/backup"}},
	///   "my_s3": {"type": "s3", "settings": {"bucket": "my-bucket"}}
	/// }
	/// </code>
	/// </summary>
	internal sealed class GetRepositoryResponseConverter : JsonConverter<GetRepositoryResponse>
	{
		public override GetRepositoryResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var response = new GetRepositoryResponse();

			if (reader.TokenType == JsonTokenType.Null)
				return response;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for GetRepositoryResponse but got {reader.TokenType}");

			var repositories = new Dictionary<string, ISnapshotRepository>();

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			foreach (var property in root.EnumerateObject())
			{
				var repoName = property.Name;

				// Skip server error fields
				if (repoName == "error" || repoName == "status")
					continue;

				var repoElement = property.Value;

				var repoType = repoElement.TryGetProperty("type", out var typeProp)
					? typeProp.GetString()
					: null;

				string settingsJson = null;
				if (repoElement.TryGetProperty("settings", out var settingsProp))
					settingsJson = settingsProp.GetRawText();

				var repo = DeserializeRepository(repoType, settingsJson, options);
				if (repo != null)
					repositories.Add(repoName, repo);
			}

			response.Repositories = repositories;
			return response;
		}

		private static ISnapshotRepository DeserializeRepository(string repoType, string settingsJson, JsonSerializerOptions options)
		{
			switch (repoType)
			{
				case "fs":
				{
					var settings = settingsJson != null
						? JsonSerializer.Deserialize<FileSystemRepositorySettings>(settingsJson, options)
						: null;
					return new FileSystemRepository(settings);
				}
				case "url":
				{
					var settings = settingsJson != null
						? JsonSerializer.Deserialize<ReadOnlyUrlRepositorySettings>(settingsJson, options)
						: null;
					return new ReadOnlyUrlRepository(settings);
				}
				case "azure":
				{
					var repo = new AzureRepository();
					if (settingsJson != null)
						repo.Settings = JsonSerializer.Deserialize<AzureRepositorySettings>(settingsJson, options);
					return repo;
				}
				case "s3":
				{
					var repo = new S3Repository();
					if (settingsJson != null)
						repo.Settings = JsonSerializer.Deserialize<S3RepositorySettings>(settingsJson, options);
					return repo;
				}
				case "hdfs":
				{
					var settings = settingsJson != null
						? JsonSerializer.Deserialize<HdfsRepositorySettings>(settingsJson, options)
						: null;
					return new HdfsRepository(settings);
				}
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, GetRepositoryResponse value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.Repositories != null)
			{
				foreach (var kvp in value.Repositories)
				{
					writer.WritePropertyName(kvp.Key);
					JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
				}
			}

			writer.WriteEndObject();
		}
	}
}
