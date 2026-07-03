/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// A read-only <see cref="System.Text.Json"/> converter for <see cref="GetRepositoryResponse"/>,
	/// replacing the vendored Utf8Json <c>GetRepositoryResponseFormatter</c> as part of #388. Each
	/// entry is <c>{ "&lt;name&gt;": { "type": …, "settings": { … } } }</c>; the <c>type</c> selects the
	/// concrete repository (fs/url/azure/s3/hdfs) whose settings are deserialized and passed to its
	/// constructor, while <c>source</c> deserializes the whole entry as an <see cref="ISourceOnlyRepository"/>.
	/// </summary>
	internal sealed class GetRepositoryResponseConverter : JsonConverter<GetRepositoryResponse>
	{
		public override GetRepositoryResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			var repositories = new Dictionary<string, ISnapshotRepository>();
			Error error = null;
			int? statusCode = null;

			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "error":
						error = member.Value.ValueKind == JsonValueKind.String
							? new Error { Reason = member.Value.GetString() }
							: member.Value.Deserialize<Error>(options);
						continue;
					case "status":
						if (member.Value.ValueKind == JsonValueKind.Number)
							statusCode = member.Value.GetInt32();
						continue;
				}

				string type = null;
				JsonElement settings = default;
				var hasSettings = false;
				if (member.Value.ValueKind == JsonValueKind.Object)
				{
					foreach (var property in member.Value.EnumerateObject())
					{
						if (property.NameEquals("type"))
							type = property.Value.GetString();
						else if (property.NameEquals("settings"))
						{
							settings = property.Value;
							hasSettings = true;
						}
					}
				}

				var repository = Build(type, settings, hasSettings, member.Value, options);
				if (repository != null)
					repositories.Add(member.Name, repository);
			}

			var response = new GetRepositoryResponse { Error = error, StatusCode = statusCode, Repositories = repositories };
			return response;
		}

		private static ISnapshotRepository Build(string type, JsonElement settings, bool hasSettings, JsonElement whole, JsonSerializerOptions options)
		{
			switch (type)
			{
				case "fs": return Create<FileSystemRepository, FileSystemRepositorySettings>(settings, hasSettings, options);
				case "url": return Create<ReadOnlyUrlRepository, ReadOnlyUrlRepositorySettings>(settings, hasSettings, options);
				case "azure": return Create<AzureRepository, AzureRepositorySettings>(settings, hasSettings, options);
				case "s3": return Create<S3Repository, S3RepositorySettings>(settings, hasSettings, options);
				case "hdfs": return Create<HdfsRepository, HdfsRepositorySettings>(settings, hasSettings, options);
				case "source": return whole.Deserialize<ISourceOnlyRepository>(options);
				default: return null;
			}
		}

		private static TRepository Create<TRepository, TSettings>(JsonElement settings, bool hasSettings, JsonSerializerOptions options)
			where TRepository : ISnapshotRepository
		{
			if (!hasSettings)
				return typeof(TRepository).CreateInstance<TRepository>();

			var resolved = settings.Deserialize<TSettings>(options);
			return typeof(TRepository).CreateInstance<TRepository>(resolved);
		}

		public override void Write(Utf8JsonWriter writer, GetRepositoryResponse value, JsonSerializerOptions options) =>
			throw new NotSupportedException($"{nameof(GetRepositoryResponse)} is a response type and is not serialized.");
	}
}
