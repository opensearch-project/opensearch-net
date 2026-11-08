/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="CreateRepositoryConverter"/>: the create-repository request body is its single
	/// polymorphic <c>Repository</c> serialized as the concrete repository interface chosen by the repository
	/// <c>type</c> discriminator (<c>s3</c>/<c>azure</c>/<c>url</c>/<c>hdfs</c>/<c>fs</c>/<c>source</c>; anything else →
	/// base <see cref="ISnapshotRepository"/>). A null request or null repository writes an empty object; reading is not
	/// supported. Mirrors the legacy Utf8Json <c>CreateRepositoryFormatter</c>.
	/// </summary>
	public class CreateRepositoryConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new NullableStringIntConverter());
			options.Converters.Add(new NullableStringBooleanConverter());
			options.Converters.Add(new CreateRepositoryConverter());
			// The "source" branch serializes the repository as ISourceOnlyRepository, handled by its own converter.
			options.Converters.Add(new SourceOnlyRepositoryConverter());
			return options;
		}

		private static JsonElement Write(ICreateRepositoryRequest request) =>
			JsonDocument.Parse(JsonSerializer.Serialize(request, Options())).RootElement;

		[U] public void Write_NullRequest_WritesEmptyObject()
		{
			var json = JsonSerializer.Serialize<ICreateRepositoryRequest>(null, Options());
			json.Should().Be("{}");
		}

		[U] public void Write_NullRepository_WritesEmptyObject()
		{
			var request = new CreateRepositoryRequest("repo") { Repository = null };
			JsonSerializer.Serialize<ICreateRepositoryRequest>(request, Options()).Should().Be("{}");
		}

		[U] public void Write_FileSystem_DispatchesFsType()
		{
			var request = new CreateRepositoryRequest("repo")
			{
				Repository = new FileSystemRepository(new FileSystemRepositorySettings("some/location") { ChunkSize = "64mb" })
			};
			var root = Write(request);
			root.GetProperty("type").GetString().Should().Be("fs");
			root.GetProperty("settings").GetProperty("location").GetString().Should().Be("some/location");
			root.GetProperty("settings").GetProperty("chunk_size").GetString().Should().Be("64mb");
		}

		[U] public void Write_S3_DispatchesS3Type()
		{
			var request = new CreateRepositoryRequest("repo")
			{
				Repository = new S3Repository(new S3RepositorySettings("foobucket") { BasePath = "some/path" })
			};
			var root = Write(request);
			root.GetProperty("type").GetString().Should().Be("s3");
			root.GetProperty("settings").GetProperty("bucket").GetString().Should().Be("foobucket");
			root.GetProperty("settings").GetProperty("base_path").GetString().Should().Be("some/path");
		}

		[U] public void Write_Azure_DispatchesAzureType()
		{
			var request = new CreateRepositoryRequest("repo")
			{
				Repository = new AzureRepository(new AzureRepositorySettings { Container = "foocontainer", BasePath = "foopath" })
			};
			var root = Write(request);
			root.GetProperty("type").GetString().Should().Be("azure");
			root.GetProperty("settings").GetProperty("container").GetString().Should().Be("foocontainer");
		}

		[U] public void Write_ReadOnlyUrl_DispatchesUrlType()
		{
			var request = new CreateRepositoryRequest("repo")
			{
				Repository = new ReadOnlyUrlRepository(new ReadOnlyUrlRepositorySettings("http://some/location"))
			};
			var root = Write(request);
			root.GetProperty("type").GetString().Should().Be("url");
			root.GetProperty("settings").GetProperty("location").GetString().Should().Be("http://some/location");
		}

		[U] public void Write_Hdfs_DispatchesHdfsType()
		{
			var request = new CreateRepositoryRequest("repo")
			{
				Repository = new HdfsRepository(new HdfsRepositorySettings("some/path") { Uri = "foouri" })
			};
			var root = Write(request);
			root.GetProperty("type").GetString().Should().Be("hdfs");
			root.GetProperty("settings").GetProperty("path").GetString().Should().Be("some/path");
			root.GetProperty("settings").GetProperty("uri").GetString().Should().Be("foouri");
		}

		[U] public void Write_SourceOnly_DispatchesSourceTypeWithDelegate()
		{
			var request = new CreateRepositoryRequest("repo")
			{
				Repository = new SourceOnlyRepository(
					new FileSystemRepository(new FileSystemRepositorySettings("some/location")))
			};
			var root = Write(request);
			root.GetProperty("type").GetString().Should().Be("source");
			root.GetProperty("settings").GetProperty("delegate_type").GetString().Should().Be("fs");
			root.GetProperty("settings").GetProperty("location").GetString().Should().Be("some/location");
		}

		[U] public void Write_CustomRepository_FallsBackToBaseType()
		{
			var request = new CreateRepositoryRequest("repo") { Repository = new CustomRepository() };
			var root = Write(request);
			root.GetProperty("type").GetString().Should().Be("mytype");
		}

		[U] public void Read_ThrowsNotSupported()
		{
			Action read = () => JsonSerializer.Deserialize<ICreateRepositoryRequest>("{}", Options());
			read.Should().Throw<NotSupportedException>();
		}

		// A minimal custom repository whose type is not one of the known discriminators, to exercise the default branch.
		private class CustomRepository : ISnapshotRepository
		{
			public string Type => "mytype";
		}
	}
}
