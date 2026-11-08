/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="SourceOnlyRepositoryConverter"/>: a source-only repository writes
	/// <c>{ "type": "source", "settings": { "delegate_type": "&lt;t&gt;", ...delegate settings flattened... } }</c>,
	/// choosing the delegate settings type by the runtime <c>DelegateType</c>
	/// (<c>s3</c>/<c>azure</c>/<c>url</c>/<c>hdfs</c>/<c>fs</c>; else base settings). A null/empty delegate type writes
	/// JSON null. On read the discriminator is the nested <c>settings.delegate_type</c>; a body with no <c>settings</c>
	/// object reads as null. Mirrors the legacy Utf8Json <c>SourceOnlyRepositoryFormatter</c>.
	/// </summary>
	public class SourceOnlyRepositoryConverterTests
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
			options.Converters.Add(new SourceOnlyRepositoryConverter());
			return options;
		}

		private static ISourceOnlyRepository Deserialize(string json) =>
			JsonSerializer.Deserialize<ISourceOnlyRepository>(json, Options());

		private static JsonElement Write(ISourceOnlyRepository value) =>
			JsonDocument.Parse(JsonSerializer.Serialize(value, Options())).RootElement;

		[U] public void Write_Null_WritesNull()
		{
			JsonSerializer.Serialize<ISourceOnlyRepository>(null, Options()).Should().Be("null");
		}

		[U] public void Write_EmptyDelegateType_WritesNull()
		{
			// A SourceOnlyRepository constructed with no delegate has a null DelegateType.
			ISourceOnlyRepository repo = new SourceOnlyRepository();
			JsonSerializer.Serialize(repo, Options()).Should().Be("null");
		}

		[U] public void Write_FileSystemDelegate_FlattensSettingsWithDelegateType()
		{
			ISourceOnlyRepository repo = new SourceOnlyRepository(
				new FileSystemRepository(new FileSystemRepositorySettings("some/location") { ChunkSize = "64mb" }));
			var root = Write(repo);
			root.GetProperty("type").GetString().Should().Be("source");
			var settings = root.GetProperty("settings");
			settings.GetProperty("delegate_type").GetString().Should().Be("fs");
			settings.GetProperty("location").GetString().Should().Be("some/location");
			settings.GetProperty("chunk_size").GetString().Should().Be("64mb");
		}

		[U] public void Write_S3Delegate_FlattensSettingsWithDelegateType()
		{
			ISourceOnlyRepository repo = new SourceOnlyRepository(
				new S3Repository(new S3RepositorySettings("foobucket") { BasePath = "some/path" }));
			var root = Write(repo);
			root.GetProperty("type").GetString().Should().Be("source");
			var settings = root.GetProperty("settings");
			settings.GetProperty("delegate_type").GetString().Should().Be("s3");
			settings.GetProperty("bucket").GetString().Should().Be("foobucket");
			settings.GetProperty("base_path").GetString().Should().Be("some/path");
		}

		[U] public void Write_AzureDelegate_FlattensSettingsWithDelegateType()
		{
			ISourceOnlyRepository repo = new SourceOnlyRepository(
				new AzureRepository(new AzureRepositorySettings { Container = "foocontainer" }));
			var root = Write(repo);
			root.GetProperty("settings").GetProperty("delegate_type").GetString().Should().Be("azure");
			root.GetProperty("settings").GetProperty("container").GetString().Should().Be("foocontainer");
		}

		[U] public void Read_NonObject_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
			Deserialize("42").Should().BeNull();
		}

		[U] public void Read_NoSettings_ReturnsNull()
		{
			Deserialize(@"{""type"":""source""}").Should().BeNull();
		}

		[U] public void Read_FsDelegate_DispatchesSettings()
		{
			var repo = Deserialize(@"{""type"":""source"",""settings"":{""delegate_type"":""fs"",""location"":""some/location""}}");
			repo.Should().NotBeNull();
			repo.DelegateType.Should().Be("fs");
			var settings = repo.DelegateSettings.Should().BeAssignableTo<IFileSystemRepositorySettings>().Subject;
			settings.Location.Should().Be("some/location");
		}

		[U] public void Read_S3Delegate_DispatchesSettings()
		{
			var repo = Deserialize(@"{""type"":""source"",""settings"":{""delegate_type"":""s3"",""bucket"":""foobucket""}}");
			repo.Should().NotBeNull();
			repo.DelegateType.Should().Be("s3");
			repo.DelegateSettings.Should().BeAssignableTo<IS3RepositorySettings>()
				.Which.Bucket.Should().Be("foobucket");
		}

		[U] public void Read_UnknownDelegate_ReturnsRepositoryWithNullSettings()
		{
			var repo = Deserialize(@"{""type"":""source"",""settings"":{""delegate_type"":""unknown"",""foo"":""bar""}}");
			repo.Should().NotBeNull();
			repo.DelegateType.Should().Be("unknown");
			repo.DelegateSettings.Should().BeNull();
		}

		[U] public void RoundTrip_FileSystemDelegate_PreservesTypeAndSettings()
		{
			var options = Options();
			ISourceOnlyRepository repo = new SourceOnlyRepository(
				new FileSystemRepository(new FileSystemRepositorySettings("some/location") { ChunkSize = "64mb" }));
			var json = JsonSerializer.Serialize(repo, options);
			var back = JsonSerializer.Deserialize<ISourceOnlyRepository>(json, options);
			back.Should().NotBeNull();
			back.DelegateType.Should().Be("fs");
			back.DelegateSettings.Should().BeAssignableTo<IFileSystemRepositorySettings>()
				.Which.Location.Should().Be("some/location");
		}
	}
}
