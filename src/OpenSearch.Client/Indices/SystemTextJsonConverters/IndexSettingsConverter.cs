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

namespace OpenSearch.Client.Indices.SystemTextJsonConverters;

/// <summary>
/// System.Text.Json converter for <see cref="IIndexSettings"/>.
/// Delegates to <see cref="DynamicIndexSettingsConverter"/> for the actual work.
/// </summary>
internal sealed class IndexSettingsConverter : JsonConverter<IIndexSettings>
{
	public override IIndexSettings? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var dynamicConverter = new DynamicIndexSettingsConverter();
		return (IIndexSettings?)dynamicConverter.Read(ref reader, typeof(IDynamicIndexSettings), options);
	}

	public override void Write(Utf8JsonWriter writer, IIndexSettings value, JsonSerializerOptions options)
	{
		var dynamicConverter = new DynamicIndexSettingsConverter();
		dynamicConverter.Write(writer, value, options);
	}
}

/// <summary>
/// System.Text.Json converter for <see cref="IDynamicIndexSettings"/>.
/// Serializes known properties as regular fields and any dynamic/unknown settings from the dictionary.
/// On deserialization, parses all properties, assigns known ones to typed properties, and collects
/// unknown entries into the dictionary backing store.
/// </summary>
internal sealed class DynamicIndexSettingsConverter : JsonConverter<IDynamicIndexSettings>
{
	// Known setting keys matching UpdatableIndexSettings / FixedIndexSettings constants
	private const string NumberOfShards = "index.number_of_shards";
	private const string NumberOfReplicas = "index.number_of_replicas";
	private const string NumberOfRoutingShards = "index.number_of_routing_shards";
	private const string RefreshInterval = "index.refresh_interval";
	private const string BlocksReadOnly = "index.blocks.read_only";
	private const string BlocksRead = "index.blocks.read";
	private const string BlocksWrite = "index.blocks.write";
	private const string BlocksMetadata = "index.blocks.metadata";
	private const string BlocksReadOnlyAllowDelete = "index.blocks.read_only_allow_delete";
	private const string Priority = "index.priority";
	private const string AutoExpandReplicasKey = "index.auto_expand_replicas";
	private const string RequestsCacheEnable = "index.requests.cache.enable";
	private const string RoutingAllocationTotalShardsPerNode = "index.routing.allocation.total_shards_per_node";
	private const string UnassignedNodeLeftDelayedTimeout = "index.unassigned.node_left.delayed_timeout";
	private const string DefaultPipeline = "index.default_pipeline";
	private const string FinalPipeline = "index.final_pipeline";
	private const string RoutingPartitionSize = "index.routing_partition_size";
	private const string Hidden = "index.hidden";
	private const string NumberOfShardsShort = "number_of_shards";
	private const string NumberOfReplicasShort = "number_of_replicas";
	private const string RefreshIntervalShort = "refresh_interval";

	public override IDynamicIndexSettings? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected StartObject for IDynamicIndexSettings but got {reader.TokenType}");

		using var doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;

		var indexSettings = new IndexSettings();
		var dictionary = (IDictionary<string, object>)indexSettings;

		foreach (var property in root.EnumerateObject())
		{
			var key = property.Name;
			var element = property.Value;

			if (!TrySetKnownProperty(indexSettings, key, element, options))
			{
				// Store unknown properties in the dictionary
				dictionary[key] = ConvertJsonElement(element);
			}
		}

		return indexSettings;
	}

	public override void Write(Utf8JsonWriter writer, IDynamicIndexSettings value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();

		// Write known properties
		WriteIfNotNull(writer, NumberOfReplicasShort, value.NumberOfReplicas, options);
		WriteIfNotNull(writer, RefreshIntervalShort, value.RefreshInterval, options);
		WriteIfNotNull(writer, DefaultPipeline, value.DefaultPipeline, options);
		WriteIfNotNull(writer, FinalPipeline, value.FinalPipeline, options);

		if (value.AutoExpandReplicas != null)
		{
			writer.WritePropertyName(AutoExpandReplicasKey);
			if (!value.AutoExpandReplicas.Enabled)
				writer.WriteBooleanValue(false);
			else
				writer.WriteStringValue(value.AutoExpandReplicas.ToString());
		}

		WriteIfNotNull(writer, BlocksReadOnly, value.BlocksReadOnly, options);
		WriteIfNotNull(writer, BlocksRead, value.BlocksRead, options);
		WriteIfNotNull(writer, BlocksWrite, value.BlocksWrite, options);
		WriteIfNotNull(writer, BlocksMetadata, value.BlocksMetadata, options);
		WriteIfNotNull(writer, BlocksReadOnlyAllowDelete, value.BlocksReadOnlyAllowDelete, options);
		WriteIfNotNull(writer, Priority, value.Priority, options);
		WriteIfNotNull(writer, RequestsCacheEnable, value.RequestsCacheEnabled, options);
		WriteIfNotNull(writer, RoutingAllocationTotalShardsPerNode, value.RoutingAllocationTotalShardsPerNode, options);

		if (value.Analysis != null)
		{
			writer.WritePropertyName("analysis");
			JsonSerializer.Serialize(writer, value.Analysis, options);
		}

		if (value.Similarity != null)
		{
			writer.WritePropertyName("similarity");
			JsonSerializer.Serialize(writer, value.Similarity, options);
		}

		// Write IIndexSettings-specific properties
		if (value is IIndexSettings indexSettings)
		{
			WriteIfNotNull(writer, NumberOfShardsShort, indexSettings.NumberOfShards, options);
			WriteIfNotNull(writer, NumberOfRoutingShards, indexSettings.NumberOfRoutingShards, options);
			WriteIfNotNull(writer, RoutingPartitionSize, indexSettings.RoutingPartitionSize, options);
			WriteIfNotNull(writer, Hidden, indexSettings.Hidden, options);

			if (indexSettings.Sorting != null)
			{
				writer.WritePropertyName("sort");
				JsonSerializer.Serialize(writer, indexSettings.Sorting, options);
			}

			if (indexSettings.SoftDeletes != null)
			{
				writer.WritePropertyName("soft_deletes");
				JsonSerializer.Serialize(writer, indexSettings.SoftDeletes, options);
			}
		}

		// Write any additional dictionary entries (custom/unknown settings)
		IDictionary<string, object> dict = value;
		if (dict != null)
		{
			foreach (var kvp in dict)
			{
				if (IsKnownKey(kvp.Key))
					continue;

				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}
		}

		writer.WriteEndObject();
	}

	private static bool TrySetKnownProperty(IndexSettings settings, string key, JsonElement element, JsonSerializerOptions options)
	{
		switch (key)
		{
			case NumberOfShards:
			case NumberOfShardsShort:
				settings.NumberOfShards = GetNullableInt(element);
				return true;
			case NumberOfReplicas:
			case NumberOfReplicasShort:
				settings.NumberOfReplicas = GetNullableInt(element);
				return true;
			case NumberOfRoutingShards:
				settings.NumberOfRoutingShards = GetNullableInt(element);
				return true;
			case RefreshInterval:
			case RefreshIntervalShort:
				settings.RefreshInterval = GetTimeValue(element);
				return true;
			case BlocksReadOnly:
				settings.BlocksReadOnly = GetNullableBool(element);
				return true;
			case BlocksRead:
				settings.BlocksRead = GetNullableBool(element);
				return true;
			case BlocksWrite:
				settings.BlocksWrite = GetNullableBool(element);
				return true;
			case BlocksMetadata:
				settings.BlocksMetadata = GetNullableBool(element);
				return true;
			case BlocksReadOnlyAllowDelete:
				settings.BlocksReadOnlyAllowDelete = GetNullableBool(element);
				return true;
			case Priority:
				settings.Priority = GetNullableInt(element);
				return true;
			case AutoExpandReplicasKey:
				settings.AutoExpandReplicas = GetAutoExpandReplicas(element);
				return true;
			case RequestsCacheEnable:
				settings.RequestsCacheEnabled = GetNullableBool(element);
				return true;
			case RoutingAllocationTotalShardsPerNode:
				settings.RoutingAllocationTotalShardsPerNode = GetNullableInt(element);
				return true;
			case UnassignedNodeLeftDelayedTimeout:
				settings.UnassignedNodeLeftDelayedTimeout = GetTimeValue(element);
				return true;
			case DefaultPipeline:
				settings.DefaultPipeline = element.ValueKind == JsonValueKind.String ? element.GetString() : null;
				return true;
			case FinalPipeline:
				settings.FinalPipeline = element.ValueKind == JsonValueKind.String ? element.GetString() : null;
				return true;
			case RoutingPartitionSize:
				settings.RoutingPartitionSize = GetNullableInt(element);
				return true;
			case Hidden:
				settings.Hidden = GetNullableBool(element);
				return true;
			case "analysis":
				settings.Analysis = JsonSerializer.Deserialize<IAnalysis>(element.GetRawText(), options);
				return true;
			case "similarity":
				settings.Similarity = JsonSerializer.Deserialize<ISimilarities>(element.GetRawText(), options);
				return true;
			default:
				return false;
		}
	}

	private static bool IsKnownKey(string key)
	{
		switch (key)
		{
			case NumberOfShards:
			case NumberOfShardsShort:
			case NumberOfReplicas:
			case NumberOfReplicasShort:
			case NumberOfRoutingShards:
			case RefreshInterval:
			case RefreshIntervalShort:
			case BlocksReadOnly:
			case BlocksRead:
			case BlocksWrite:
			case BlocksMetadata:
			case BlocksReadOnlyAllowDelete:
			case Priority:
			case AutoExpandReplicasKey:
			case RequestsCacheEnable:
			case RoutingAllocationTotalShardsPerNode:
			case UnassignedNodeLeftDelayedTimeout:
			case DefaultPipeline:
			case FinalPipeline:
			case RoutingPartitionSize:
			case Hidden:
			case "analysis":
			case "similarity":
				return true;
			default:
				return false;
		}
	}

	private static void WriteIfNotNull(Utf8JsonWriter writer, string propertyName, int? value, JsonSerializerOptions options)
	{
		if (value.HasValue)
		{
			writer.WritePropertyName(propertyName);
			writer.WriteNumberValue(value.Value);
		}
	}

	private static void WriteIfNotNull(Utf8JsonWriter writer, string propertyName, bool? value, JsonSerializerOptions options)
	{
		if (value.HasValue)
		{
			writer.WritePropertyName(propertyName);
			writer.WriteBooleanValue(value.Value);
		}
	}

	private static void WriteIfNotNull(Utf8JsonWriter writer, string propertyName, string? value, JsonSerializerOptions options)
	{
		if (value != null)
		{
			writer.WritePropertyName(propertyName);
			writer.WriteStringValue(value);
		}
	}

	private static void WriteIfNotNull(Utf8JsonWriter writer, string propertyName, Time? value, JsonSerializerOptions options)
	{
		if (value != null)
		{
			writer.WritePropertyName(propertyName);
			writer.WriteStringValue(value.ToString());
		}
	}

	private static int? GetNullableInt(JsonElement element)
	{
		if (element.ValueKind == JsonValueKind.Number)
			return element.GetInt32();
		if (element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out var v))
			return v;
		return null;
	}

	private static bool? GetNullableBool(JsonElement element)
	{
		return element.ValueKind switch
		{
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			JsonValueKind.String => bool.TryParse(element.GetString(), out var b) ? b : null,
			_ => null
		};
	}

	private static Time? GetTimeValue(JsonElement element)
	{
		if (element.ValueKind == JsonValueKind.String)
			return new Time(element.GetString());
		if (element.ValueKind == JsonValueKind.Number)
			return new Time(element.GetInt64());
		return null;
	}

	private static AutoExpandReplicas? GetAutoExpandReplicas(JsonElement element)
	{
		if (element.ValueKind == JsonValueKind.False)
			return AutoExpandReplicas.Disabled;
		if (element.ValueKind == JsonValueKind.String)
		{
			var str = element.GetString();
			return string.IsNullOrEmpty(str) ? null : AutoExpandReplicas.Create(str);
		}
		return null;
	}

	private static object? ConvertJsonElement(JsonElement element)
	{
		return element.ValueKind switch
		{
			JsonValueKind.String => element.GetString(),
			JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			JsonValueKind.Null => null,
			JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText()),
			JsonValueKind.Array => JsonSerializer.Deserialize<List<object>>(element.GetRawText()),
			_ => element.GetRawText()
		};
	}
}
