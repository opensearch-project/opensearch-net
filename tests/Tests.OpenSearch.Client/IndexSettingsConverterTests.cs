/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="IndexSettingsConverter"/> / <see cref="DynamicIndexSettingsConverter"/>.
	/// Index settings are exchanged as dotted keys (e.g. <c>index.number_of_shards</c>): known strongly-typed
	/// settings are projected to their dotted key on write, and nested <c>{ index: { … } }</c> objects are
	/// flattened to dotted keys on read (except the <c>analysis</c>/<c>similarity</c> sub-objects, kept verbatim).
	/// </summary>
	public class IndexSettingsConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new IndexSettingsConverter());
			options.Converters.Add(new DynamicIndexSettingsConverter());
			options.Converters.Add(new TimeConverter());
			options.Converters.Add(new AutoExpandReplicasConverter());
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		private static IIndexSettings Deserialize(string json) =>
			JsonSerializer.Deserialize<IIndexSettings>(json, Options());

		private static string Serialize(IIndexSettings settings) =>
			JsonSerializer.Serialize(settings, Options());

		[U] public void Write_KnownSettings_AsDottedKeys()
		{
			var settings = new IndexSettings { NumberOfShards = 1, NumberOfReplicas = 2 };
			var json = Serialize(settings);

			json.Should().Contain(@"""index.number_of_shards"":1");
			json.Should().Contain(@"""index.number_of_replicas"":2");
		}

		[U] public void Write_CustomSetting_VerbatimKey()
		{
			var settings = new IndexSettings(new Dictionary<string, object> { { "any.setting", "can be set" } })
			{
				NumberOfShards = 1
			};
			var json = Serialize(settings);

			json.Should().Contain(@"""any.setting"":""can be set""");
			json.Should().Contain(@"""index.number_of_shards"":1");
		}

		[U] public void Read_DottedKeys_PopulatesKnownSettings()
		{
			var settings = Deserialize(@"{""index.number_of_shards"":3,""index.number_of_replicas"":5}");

			settings.NumberOfShards.Should().Be(3);
			settings.NumberOfReplicas.Should().Be(5);
		}

		[U] public void Read_NestedIndexObject_IsFlattened()
		{
			// The server nests dotted keys under the "index" object; the converter must unflatten them.
			var settings = Deserialize(@"{""index"":{""number_of_shards"":4,""number_of_replicas"":6}}");

			settings.NumberOfShards.Should().Be(4);
			settings.NumberOfReplicas.Should().Be(6);
		}

		[U] public void Read_CustomSetting_KeptInDictionary()
		{
			var settings = Deserialize(@"{""index.number_of_shards"":1,""any.setting"":""hello"",""doubles"":1.5,""bools"":false}");

			settings.NumberOfShards.Should().Be(1);
			IDictionary<string, object> dict = settings;
			dict.Should().ContainKey("any.setting");
			dict["any.setting"].Should().Be("hello");
			dict.Should().ContainKey("doubles");
			dict.Should().ContainKey("bools");
		}

		[U] public void Read_AutoExpandReplicas()
		{
			var settings = Deserialize(@"{""index.auto_expand_replicas"":""1-3""}");
			settings.AutoExpandReplicas.Should().NotBeNull();
			settings.AutoExpandReplicas.ToString().Should().Be("1-3");
		}

		[U] public void Read_StoreType_Enum()
		{
			var settings = Deserialize(@"{""index.store.type"":""mmapfs""}");
			settings.FileSystemStorageImplementation.Should().Be(FileSystemStorageImplementation.MMap);
		}

		[U] public void Read_AnalysisBlock_IsCapturedAndNotFlattened()
		{
			// The "analysis" sub-object is preserved verbatim (not flattened into dotted keys) and reserialized
			// into the strong Analysis type.
			var settings = Deserialize(@"{""index.number_of_shards"":1,""analysis"":{}}");

			settings.NumberOfShards.Should().Be(1);
			settings.Analysis.Should().NotBeNull();
			// The analysis object must not have been flattened into a dotted key.
			IDictionary<string, object> dict = settings;
			dict.Should().NotContainKey("analysis.analyzer");
		}

		[U] public void Read_Null_ReturnsEmptySettings()
		{
			// A JSON null yields a fresh IndexSettings (mirrors the legacy formatter constructing a new instance).
			var settings = Deserialize("null");
			settings.Should().NotBeNull();
			settings.NumberOfShards.Should().BeNull();
		}

		[U] public void Write_Null_WritesNull()
		{
			JsonSerializer.Serialize<IIndexSettings>(null, Options()).Should().Be("null");
		}

		[U] public void RoundTrip_KnownAndCustomSettings()
		{
			var options = Options();
			var original = new IndexSettings(new Dictionary<string, object> { { "any.setting", "can be set" } })
			{
				NumberOfShards = 2,
				NumberOfReplicas = 3,
				AutoExpandReplicas = "0-5",
				FileSystemStorageImplementation = FileSystemStorageImplementation.MMap
			};

			var json = JsonSerializer.Serialize<IIndexSettings>(original, options);
			var back = JsonSerializer.Deserialize<IIndexSettings>(json, options);

			back.NumberOfShards.Should().Be(2);
			back.NumberOfReplicas.Should().Be(3);
			back.AutoExpandReplicas.ToString().Should().Be("0-5");
			back.FileSystemStorageImplementation.Should().Be(FileSystemStorageImplementation.MMap);
			((IDictionary<string, object>)back).Should().ContainKey("any.setting");
		}
	}
}
