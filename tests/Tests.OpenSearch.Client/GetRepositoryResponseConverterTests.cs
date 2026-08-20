/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="GetRepositoryResponseConverter"/>: the wire body is a flat
	/// <c>name -&gt; { "type": ..., "settings": {...} }</c> map, optionally carrying the server-error envelope
	/// (<c>error</c>/<c>status</c>). Each repository is dispatched to a concrete type by its <c>type</c> discriminator
	/// (<c>fs</c>/<c>url</c>/<c>azure</c>/<c>s3</c>/<c>hdfs</c>/<c>source</c>); unknown types are skipped. Mirrors the
	/// read path of the legacy Utf8Json <c>GetRepositoryResponseFormatter</c> (the write path is a deliberate,
	/// documented divergence — the legacy write path emitted an unused reflection dump; this converter writes the
	/// canonical flat map so read/write round-trips).
	/// </summary>
	public class GetRepositoryResponseConverterTests
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
			options.Converters.Add(new ErrorConverter());
			options.Converters.Add(new SourceOnlyRepositoryConverter());
			options.Converters.Add(new GetRepositoryResponseConverter());
			return options;
		}

		private static GetRepositoryResponse Deserialize(string json) =>
			JsonSerializer.Deserialize<GetRepositoryResponse>(json, Options());

		[U] public void Read_FileSystemRepository()
		{
			var response = Deserialize(@"{""my-repo"":{""type"":""fs"",""settings"":{""location"":""some/location"",""chunk_size"":""64mb""}}}");
			response.Repositories.Should().HaveCount(1);
			var repo = response.FileSystem("my-repo");
			repo.Should().NotBeNull();
			repo.Type.Should().Be("fs");
			repo.Settings.Location.Should().Be("some/location");
			repo.Settings.ChunkSize.Should().Be("64mb");
		}

		[U] public void Read_S3Repository()
		{
			var response = Deserialize(@"{""my-repo"":{""type"":""s3"",""settings"":{""bucket"":""foobucket""}}}");
			var repo = response.S3("my-repo");
			repo.Should().NotBeNull();
			repo.Type.Should().Be("s3");
			repo.Settings.Bucket.Should().Be("foobucket");
		}

		[U] public void Read_AzureRepository()
		{
			var response = Deserialize(@"{""my-repo"":{""type"":""azure"",""settings"":{""container"":""foocontainer""}}}");
			var repo = response.Azure("my-repo");
			repo.Should().NotBeNull();
			repo.Type.Should().Be("azure");
			repo.Settings.Container.Should().Be("foocontainer");
		}

		[U] public void Read_ReadOnlyUrlRepository()
		{
			var response = Deserialize(@"{""my-repo"":{""type"":""url"",""settings"":{""location"":""http://some/location""}}}");
			var repo = response.ReadOnlyUrl("my-repo");
			repo.Should().NotBeNull();
			repo.Type.Should().Be("url");
			repo.Settings.Location.Should().Be("http://some/location");
		}

		[U] public void Read_HdfsRepository()
		{
			var response = Deserialize(@"{""my-repo"":{""type"":""hdfs"",""settings"":{""path"":""some/path"",""uri"":""foouri""}}}");
			var repo = response.Hdfs("my-repo");
			repo.Should().NotBeNull();
			repo.Type.Should().Be("hdfs");
			repo.Settings.Path.Should().Be("some/path");
			repo.Settings.Uri.Should().Be("foouri");
		}

		[U] public void Read_SourceOnlyRepository()
		{
			var response = Deserialize(@"{""my-repo"":{""type"":""source"",""settings"":{""delegate_type"":""fs"",""location"":""some/location""}}}");
			response.Repositories.Should().HaveCount(1);
			var repo = response.Repositories["my-repo"];
			repo.Should().BeAssignableTo<ISourceOnlyRepository>();
			((ISourceOnlyRepository)repo).DelegateType.Should().Be("fs");
		}

		[U] public void Read_UrlRepository_WithSettings()
		{
			// Concrete repo types have no parameterless ctor; they are constructed from their settings object
			// (CreateInstance<T>(resolvedSettings)), matching the legacy formatter. A body without a settings object
			// is not a shape the server emits for a configured repository.
			var response = Deserialize(@"{""my-repo"":{""type"":""url"",""settings"":{""url"":""http://example""}}}");
			var repo = response.Repositories["my-repo"];
			repo.Should().NotBeNull();
			repo.Type.Should().Be("url");
		}

		[U] public void Read_UnknownType_IsSkipped()
		{
			var response = Deserialize(@"{""my-repo"":{""type"":""unknown"",""settings"":{}}}");
			response.Repositories.Should().BeEmpty();
		}

		[U] public void Read_MultipleRepositories()
		{
			var response = Deserialize(@"{""fs-repo"":{""type"":""fs"",""settings"":{""location"":""l""}},""s3-repo"":{""type"":""s3"",""settings"":{""bucket"":""b""}}}");
			response.Repositories.Should().HaveCount(2);
			response.FileSystem("fs-repo").Should().NotBeNull();
			response.S3("s3-repo").Should().NotBeNull();
		}

		[U] public void Read_ServerErrorEnvelope_PopulatesError()
		{
			var response = Deserialize(@"{""error"":{""type"":""repository_missing_exception"",""reason"":""missing""},""status"":404}");
			response.Repositories.Should().BeEmpty();
			response.ServerError.Should().NotBeNull();
			response.ServerError.Status.Should().Be(404);
			response.ServerError.Error.Reason.Should().Be("missing");
		}

		[U] public void Read_ErrorAsString_PopulatesReason()
		{
			var response = Deserialize(@"{""error"":""boom"",""status"":500}");
			response.ServerError.Should().NotBeNull();
			response.ServerError.Error.Reason.Should().Be("boom");
			response.ServerError.Status.Should().Be(500);
		}

		[U] public void Read_Null_ReturnsEmptyRepositories()
		{
			var response = Deserialize("null");
			response.Should().NotBeNull();
			response.Repositories.Should().BeEmpty();
		}

		[U] public void RoundTrip_FileSystemRepository()
		{
			var options = Options();
			var json = @"{""my-repo"":{""type"":""fs"",""settings"":{""location"":""some/location"",""chunk_size"":""64mb""}}}";
			var response = JsonSerializer.Deserialize<GetRepositoryResponse>(json, options);
			var written = JsonSerializer.Serialize(response, options);
			var back = JsonSerializer.Deserialize<GetRepositoryResponse>(written, options);
			var repo = back.FileSystem("my-repo");
			repo.Should().NotBeNull();
			repo.Settings.Location.Should().Be("some/location");
			repo.Settings.ChunkSize.Should().Be("64mb");
		}
	}
}
