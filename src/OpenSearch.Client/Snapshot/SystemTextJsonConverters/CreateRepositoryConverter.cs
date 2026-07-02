/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.SnapshotConverters
{
	/// <summary>
	/// Converter for <see cref="ICreateRepositoryRequest"/>.
	/// Serializes/deserializes the repository based on its type discriminator:
	/// <code>
	/// {"type": "fs", "settings": {"location": "/mnt/backup"}}
	/// {"type": "s3", "settings": {"bucket": "my-bucket"}}
	/// </code>
	/// </summary>
	internal sealed class CreateRepositoryConverter : JsonConverter<ICreateRepositoryRequest>
	{
		public override ICreateRepositoryRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// Deserialization of CreateRepositoryRequest is not typically needed
			// as this is a write-only request type
			throw new NotSupportedException("Deserialization of ICreateRepositoryRequest is not supported.");
		}

		public override void Write(Utf8JsonWriter writer, ICreateRepositoryRequest value, JsonSerializerOptions options)
		{
			if (value?.Repository == null)
			{
				writer.WriteStartObject();
				writer.WriteEndObject();
				return;
			}

			var repository = value.Repository;

			switch (repository.Type)
			{
				case "fs":
					JsonSerializer.Serialize(writer, repository as FileSystemRepository, options);
					break;
				case "s3":
					JsonSerializer.Serialize(writer, repository as S3Repository, options);
					break;
				case "azure":
					JsonSerializer.Serialize(writer, repository as AzureRepository, options);
					break;
				case "hdfs":
					JsonSerializer.Serialize(writer, repository as HdfsRepository, options);
					break;
				case "url":
					JsonSerializer.Serialize(writer, repository as ReadOnlyUrlRepository, options);
					break;
				case "source":
					JsonSerializer.Serialize(writer, repository as SourceOnlyRepository, options);
					break;
				default:
					JsonSerializer.Serialize(writer, repository, repository.GetType(), options);
					break;
			}
		}
	}
}
