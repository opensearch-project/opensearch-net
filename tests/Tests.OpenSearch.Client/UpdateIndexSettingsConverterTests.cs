/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="UpdateIndexSettingsConverter"/>. An <see cref="IUpdateIndexSettingsRequest"/>
	/// is serialized as just its <see cref="IUpdateIndexSettingsRequest.IndexSettings"/> body (no wrapping object) and
	/// delegates both directions to the <see cref="DynamicIndexSettingsConverter"/>, matching the legacy
	/// <c>UpdateIndexSettingsRequestFormatter</c> which forwarded to the shared <c>DynamicIndexSettingsFormatter</c>.
	/// </summary>
	public class UpdateIndexSettingsConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new UpdateIndexSettingsConverter());
			options.Converters.Add(new IndexSettingsConverter());
			options.Converters.Add(new DynamicIndexSettingsConverter());
			options.Converters.Add(new TimeConverter());
			options.Converters.Add(new AutoExpandReplicasConverter());
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		private static string Serialize(IUpdateIndexSettingsRequest request) =>
			JsonSerializer.Serialize(request, Options());

		private static IUpdateIndexSettingsRequest Deserialize(string json) =>
			JsonSerializer.Deserialize<IUpdateIndexSettingsRequest>(json, Options());

		[U] public void Write_DelegatesToDynamicSettings_DottedKeys()
		{
			var request = new UpdateIndexSettingsRequest
			{
				IndexSettings = new DynamicIndexSettings { NumberOfReplicas = 2 }
			};
			var json = Serialize(request);

			// No wrapping object: the body is the settings dictionary itself.
			json.Should().Contain(@"""index.number_of_replicas"":2");
			json.Should().StartWith("{").And.EndWith("}");
		}

		[U] public void Write_CustomSetting_VerbatimKey()
		{
			var request = new UpdateIndexSettingsRequest
			{
				IndexSettings = new DynamicIndexSettings(new Dictionary<string, object> { { "any.setting", "value" } })
				{
					NumberOfReplicas = 1
				}
			};
			var json = Serialize(request);

			json.Should().Contain(@"""any.setting"":""value""");
			json.Should().Contain(@"""index.number_of_replicas"":1");
		}

		[U] public void Read_PopulatesIndexSettings()
		{
			var request = Deserialize(@"{""index.number_of_replicas"":5}");

			request.Should().NotBeNull();
			request.IndexSettings.Should().NotBeNull();
			request.IndexSettings.NumberOfReplicas.Should().Be(5);
		}

		[U] public void Read_NestedIndexObject_IsFlattened()
		{
			var request = Deserialize(@"{""index"":{""number_of_replicas"":3}}");
			request.IndexSettings.NumberOfReplicas.Should().Be(3);
		}

		[U] public void Write_Null_WritesNull()
		{
			JsonSerializer.Serialize<IUpdateIndexSettingsRequest>(null, Options()).Should().Be("null");
		}

		[U] public void Read_Null_ReturnsRequestWithEmptySettings()
		{
			var request = Deserialize("null");
			request.Should().NotBeNull();
			request.IndexSettings.Should().NotBeNull();
			request.IndexSettings.NumberOfReplicas.Should().BeNull();
		}

		[U] public void RoundTrip_KnownAndCustomSettings()
		{
			var options = Options();
			var original = new UpdateIndexSettingsRequest
			{
				IndexSettings = new DynamicIndexSettings(new Dictionary<string, object> { { "any.setting", "kept" } })
				{
					NumberOfReplicas = 4
				}
			};

			var json = JsonSerializer.Serialize<IUpdateIndexSettingsRequest>(original, options);
			var back = JsonSerializer.Deserialize<IUpdateIndexSettingsRequest>(json, options);

			back.IndexSettings.NumberOfReplicas.Should().Be(4);
			((IDictionary<string, object>)back.IndexSettings).Should().ContainKey("any.setting");
		}
	}
}
