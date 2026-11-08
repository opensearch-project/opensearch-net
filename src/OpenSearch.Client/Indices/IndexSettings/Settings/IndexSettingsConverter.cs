/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static OpenSearch.Client.FixedIndexSettings;
using static OpenSearch.Client.IndexSortSettings;
using static OpenSearch.Client.UpdatableIndexSettings;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>IndexSettingsFormatter</c> /
	/// <c>DynamicIndexSettingsFormatter</c>.
	///
	/// Index settings are exchanged on the wire as a mixture of a strongly-typed known-settings surface
	/// (e.g. <see cref="IIndexSettings.NumberOfShards"/>) and an arbitrary "bag" of custom settings, all keyed by
	/// dotted names such as <c>index.number_of_shards</c>. On the wire the server nests these keys under objects
	/// (<c>{ "index": { "number_of_shards": 1 } }</c>) while the client models them flat, so:
	/// <list type="bullet">
	/// <item><description><b>Read</b> materializes the JSON into a boxed object tree (long/double/string/bool/
	/// <c>Dictionary&lt;string,object&gt;</c>/<c>List&lt;object&gt;</c>, matching the legacy <c>PrimitiveObjectFormatter</c>),
	/// then <em>flattens</em> nested objects into dotted keys — except the <c>analysis</c> and <c>similarity</c>
	/// sub-objects which are kept verbatim and reserialized into their strong types.</description></item>
	/// <item><description><b>Write</b> copies the settings dictionary, projects every known strongly-typed property
	/// back into its dotted key, then writes the merged dictionary (skipping <c>null</c> values, except
	/// <c>index.refresh_interval</c> which is always emitted, mirroring the legacy dictionary formatter).</description></item>
	/// </list>
	/// System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only, so the value is buffered into a
	/// <see cref="JsonDocument"/> and materialized from the DOM (unlike the Utf8Json version which read directly).
	/// This converter delegates value (de)serialization to <see cref="JsonSerializer"/> using the ambient
	/// <see cref="JsonSerializerOptions"/>, so nested settings-aware converters (Field, Time, enums, Analysis, …)
	/// behave exactly as they do everywhere else.
	/// </summary>
	internal class DynamicIndexSettingsConverter : JsonConverter<IDynamicIndexSettings>
	{
		// The legacy formatter always materialized a (possibly empty) IndexSettings instance, even for a JSON null;
		// opt into null handling so STJ routes null through Read instead of short-circuiting to a null reference.
		public override bool HandleNull => true;

		public override IDynamicIndexSettings Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			ReadIndexSettings(ref reader, options);

		public override void Write(Utf8JsonWriter writer, IDynamicIndexSettings value, JsonSerializerOptions options) =>
			WriteIndexSettings(writer, value, options);

		internal static IndexSettings ReadIndexSettings(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var indexSettings = new IndexSettings();
			SetKnownIndexSettings(ref reader, options, indexSettings);
			return indexSettings;
		}

		internal static void WriteIndexSettings(Utf8JsonWriter writer, IDynamicIndexSettings value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// Copy the existing bag (custom settings + refresh_interval) so we don't mutate the caller's instance,
			// then project the known strongly-typed settings back into their dotted keys — identical wire output.
			var d = new Dictionary<string, object>((IDictionary<string, object>)value);

			void Set(string knownKey, object newValue)
			{
				if (newValue != null) d[knownKey] = newValue;
			}

			Set(NumberOfReplicas, value.NumberOfReplicas);
			Set(RefreshInterval, value.RefreshInterval);
			Set(DefaultPipeline, value.DefaultPipeline);
			Set(FinalPipeline, value.FinalPipeline);
			Set(BlocksReadOnly, value.BlocksReadOnly);
			Set(BlocksRead, value.BlocksRead);
			Set(BlocksWrite, value.BlocksWrite);
			Set(BlocksMetadata, value.BlocksMetadata);
			Set(BlocksReadOnlyAllowDelete, value.BlocksReadOnlyAllowDelete);
			Set(Priority, value.Priority);
			Set(UpdatableIndexSettings.AutoExpandReplicas, value.AutoExpandReplicas);
			Set(UpdatableIndexSettings.RecoveryInitialShards, value.RecoveryInitialShards);
			Set(RequestsCacheEnable, value.RequestsCacheEnabled);
			Set(RoutingAllocationTotalShardsPerNode, value.RoutingAllocationTotalShardsPerNode);
			Set(UnassignedNodeLeftDelayedTimeout, value.UnassignedNodeLeftDelayedTimeout);

			var translog = value.Translog;
			Set(TranslogSyncInterval, translog?.SyncInterval);
			Set(UpdatableIndexSettings.TranslogDurability, translog?.Durability);

			var flush = value.Translog?.Flush;
			Set(TranslogFlushThresholdSize, flush?.ThresholdSize);
			Set(TranslogFlushThresholdPeriod, flush?.ThresholdPeriod);

			Set(MergePolicyExpungeDeletesAllowed, value.Merge?.Policy?.ExpungeDeletesAllowed);
			Set(MergePolicyFloorSegment, value.Merge?.Policy?.FloorSegment);
			Set(MergePolicyMaxMergeAtOnce, value.Merge?.Policy?.MaxMergeAtOnce);
			Set(MergePolicyMaxMergeAtOnceExplicit, value.Merge?.Policy?.MaxMergeAtOnceExplicit);
			Set(MergePolicyMaxMergedSegment, value.Merge?.Policy?.MaxMergedSegment);
			Set(MergePolicySegmentsPerTier, value.Merge?.Policy?.SegmentsPerTier);
			Set(MergePolicyReclaimDeletesWeight, value.Merge?.Policy?.ReclaimDeletesWeight);

			Set(MergeSchedulerMaxThreadCount, value.Merge?.Scheduler?.MaxThreadCount);
			Set(MergeSchedulerAutoThrottle, value.Merge?.Scheduler?.AutoThrottle);

			var log = value.SlowLog;
			var search = log?.Search;
			var indexing = log?.Indexing;

			Set(SlowlogSearchThresholdQueryWarn, search?.Query?.ThresholdWarn);
			Set(SlowlogSearchThresholdQueryInfo, search?.Query?.ThresholdInfo);
			Set(SlowlogSearchThresholdQueryDebug, search?.Query?.ThresholdDebug);
			Set(SlowlogSearchThresholdQueryTrace, search?.Query?.ThresholdTrace);

			Set(SlowlogSearchThresholdFetchWarn, search?.Fetch?.ThresholdWarn);
			Set(SlowlogSearchThresholdFetchInfo, search?.Fetch?.ThresholdInfo);
			Set(SlowlogSearchThresholdFetchDebug, search?.Fetch?.ThresholdDebug);
			Set(SlowlogSearchThresholdFetchTrace, search?.Fetch?.ThresholdTrace);
			Set(SlowlogSearchLevel, search?.LogLevel);

			Set(SlowlogIndexingThresholdFetchWarn, indexing?.ThresholdWarn);
			Set(SlowlogIndexingThresholdFetchInfo, indexing?.ThresholdInfo);
			Set(SlowlogIndexingThresholdFetchDebug, indexing?.ThresholdDebug);
			Set(SlowlogIndexingThresholdFetchTrace, indexing?.ThresholdTrace);
			Set(SlowlogIndexingLevel, indexing?.LogLevel);
			Set(SlowlogIndexingSource, indexing?.Source);

			Set(UpdatableIndexSettings.Analysis, value.Analysis);
			Set(Similarity, value.Similarity);

			if (value is IIndexSettings indexSettings)
			{
				Set(StoreType, indexSettings.FileSystemStorageImplementation);
				Set(QueriesCacheEnabled, indexSettings.Queries?.Cache?.Enabled);
				Set(NumberOfShards, indexSettings.NumberOfShards);
				Set(NumberOfRoutingShards, indexSettings.NumberOfRoutingShards);
				Set(RoutingPartitionSize, indexSettings.RoutingPartitionSize);
				Set(Hidden, indexSettings.Hidden);

				if (indexSettings.SoftDeletes != null)
					Set(SoftDeletesRetentionOperations, indexSettings.SoftDeletes.Retention?.Operations);

				if (indexSettings.Sorting != null)
				{
					Set(IndexSortSettings.Fields, AsArrayOrSingleItem(indexSettings.Sorting.Fields));
					Set(Order, AsArrayOrSingleItem(indexSettings.Sorting.Order));
					Set(Mode, AsArrayOrSingleItem(indexSettings.Sorting.Mode));
					Set(IndexSortSettings.Missing, AsArrayOrSingleItem(indexSettings.Sorting.Missing));
				}
			}

			// Reproduce IndexSettingsDictionaryFormatter: write every entry, skipping null values except the
			// index.refresh_interval key which is always emitted.
			writer.WriteStartObject();
			foreach (var entry in d)
			{
				if (entry.Value == null && entry.Key != RefreshInterval)
					continue;

				writer.WritePropertyName(entry.Key);
				if (entry.Value == null)
					writer.WriteNullValue();
				else
					JsonSerializer.Serialize(writer, entry.Value, entry.Value.GetType(), options);
			}
			writer.WriteEndObject();
		}

		private static object AsArrayOrSingleItem<T>(IEnumerable<T> items)
		{
			if (items == null || !items.Any())
				return null;

			if (items.Count() == 1)
				return items.First();

			return items;
		}

		private static Dictionary<string, object> Flatten(Dictionary<string, object> original, string prefix = "",
			Dictionary<string, object> current = null
		)
		{
			current ??= new Dictionary<string, object>();
			foreach (var property in original)
			{
				if (property.Value is Dictionary<string, object> objects &&
					property.Key != UpdatableIndexSettings.Analysis &&
					property.Key != Similarity)
					Flatten(objects, prefix + property.Key + ".", current);
				else current.Add(prefix + property.Key, property.Value);
			}
			return current;
		}

		private static void SetKnownIndexSettings(ref Utf8JsonReader reader, JsonSerializerOptions options, IIndexSettings s)
		{
			var settings = Flatten(ReadDictionary(ref reader));

			Set<int?>(s, settings, NumberOfReplicas, v => s.NumberOfReplicas = v, options);
			Set<AutoExpandReplicas>(s, settings, UpdatableIndexSettings.AutoExpandReplicas, v => s.AutoExpandReplicas = v, options);
			Set<Time>(s, settings, RefreshInterval, v => s.RefreshInterval = v, options);
			Set<bool?>(s, settings, BlocksReadOnly, v => s.BlocksReadOnly = v, options);
			Set<bool?>(s, settings, BlocksRead, v => s.BlocksRead = v, options);
			Set<bool?>(s, settings, BlocksWrite, v => s.BlocksWrite = v, options);
			Set<bool?>(s, settings, BlocksMetadata, v => s.BlocksMetadata = v, options);
			Set<bool?>(s, settings, BlocksReadOnlyAllowDelete, v => s.BlocksReadOnlyAllowDelete = v, options);
			Set<int?>(s, settings, Priority, v => s.Priority = v, options);
			Set<string>(s, settings, DefaultPipeline, v => s.DefaultPipeline = v, options);
			Set<string>(s, settings, FinalPipeline, v => s.FinalPipeline = v, options);

			Set<Union<int, RecoveryInitialShards>>(s, settings, UpdatableIndexSettings.RecoveryInitialShards,
				v => s.RecoveryInitialShards = v, options);
			Set<bool?>(s, settings, RequestsCacheEnable, v => s.RequestsCacheEnabled = v, options);
			Set<int?>(s, settings, RoutingAllocationTotalShardsPerNode,
				v => s.RoutingAllocationTotalShardsPerNode = v, options);
			Set<Time>(s, settings, UnassignedNodeLeftDelayedTimeout,
				v => s.UnassignedNodeLeftDelayedTimeout = v, options);

			var t = s.Translog = new TranslogSettings();
			Set<Time>(s, settings, TranslogSyncInterval, v => t.SyncInterval = v, options);
			Set<TranslogDurability?>(s, settings, UpdatableIndexSettings.TranslogDurability, v => t.Durability = v, options);

			var tf = s.Translog.Flush = new TranslogFlushSettings();
			Set<string>(s, settings, TranslogFlushThresholdSize, v => tf.ThresholdSize = v, options);
			Set<Time>(s, settings, TranslogFlushThresholdPeriod, v => tf.ThresholdPeriod = v, options);

			s.Merge = new MergeSettings();
			var p = s.Merge.Policy = new MergePolicySettings();
			Set<int?>(s, settings, MergePolicyExpungeDeletesAllowed, v => p.ExpungeDeletesAllowed = v, options);
			Set<string>(s, settings, MergePolicyFloorSegment, v => p.FloorSegment = v, options);
			Set<int?>(s, settings, MergePolicyMaxMergeAtOnce, v => p.MaxMergeAtOnce = v, options);
			Set<int?>(s, settings, MergePolicyMaxMergeAtOnceExplicit, v => p.MaxMergeAtOnceExplicit = v, options);
			Set<string>(s, settings, MergePolicyMaxMergedSegment, v => p.MaxMergedSegment = v, options);
			Set<int?>(s, settings, MergePolicySegmentsPerTier, v => p.SegmentsPerTier = v, options);
			Set<double?>(s, settings, MergePolicyReclaimDeletesWeight, v => p.ReclaimDeletesWeight = v, options);

			var ms = s.Merge.Scheduler = new MergeSchedulerSettings();
			Set<int?>(s, settings, MergeSchedulerMaxThreadCount, v => ms.MaxThreadCount = v, options);
			Set<bool?>(s, settings, MergeSchedulerAutoThrottle, v => ms.AutoThrottle = v, options);

			s.SlowLog = new SlowLog();
			var search = s.SlowLog.Search = new SlowLogSearch();
			Set<LogLevel?>(s, settings, SlowlogSearchLevel, v => search.LogLevel = v, options);
			var query = s.SlowLog.Search.Query = new SlowLogSearchQuery();
			Set<Time>(s, settings, SlowlogSearchThresholdQueryWarn, v => query.ThresholdWarn = v, options);
			Set<Time>(s, settings, SlowlogSearchThresholdQueryInfo, v => query.ThresholdInfo = v, options);
			Set<Time>(s, settings, SlowlogSearchThresholdQueryDebug, v => query.ThresholdDebug = v, options);
			Set<Time>(s, settings, SlowlogSearchThresholdQueryTrace, v => query.ThresholdTrace = v, options);

			var fetch = s.SlowLog.Search.Fetch = new SlowLogSearchFetch();
			Set<Time>(s, settings, SlowlogSearchThresholdFetchWarn, v => fetch.ThresholdWarn = v, options);
			Set<Time>(s, settings, SlowlogSearchThresholdFetchInfo, v => fetch.ThresholdInfo = v, options);
			Set<Time>(s, settings, SlowlogSearchThresholdFetchDebug, v => fetch.ThresholdDebug = v, options);
			Set<Time>(s, settings, SlowlogSearchThresholdFetchTrace, v => fetch.ThresholdTrace = v, options);

			var indexing = s.SlowLog.Indexing = new SlowLogIndexing();
			Set<Time>(s, settings, SlowlogIndexingThresholdFetchWarn, v => indexing.ThresholdWarn = v, options);
			Set<Time>(s, settings, SlowlogIndexingThresholdFetchInfo, v => indexing.ThresholdInfo = v, options);
			Set<Time>(s, settings, SlowlogIndexingThresholdFetchDebug, v => indexing.ThresholdDebug = v, options);
			Set<Time>(s, settings, SlowlogIndexingThresholdFetchTrace, v => indexing.ThresholdTrace = v, options);
			Set<LogLevel?>(s, settings, SlowlogIndexingLevel, v => indexing.LogLevel = v, options);
			Set<int?>(s, settings, SlowlogIndexingSource, v => indexing.Source = v, options);
			Set<int?>(s, settings, NumberOfShards, v => s.NumberOfShards = v, options);
			Set<int?>(s, settings, NumberOfRoutingShards, v => s.NumberOfRoutingShards = v, options);
			Set<int?>(s, settings, RoutingPartitionSize, v => s.RoutingPartitionSize = v, options);
			Set<bool?>(s, settings, Hidden, v => s.Hidden = v, options);
			Set<FileSystemStorageImplementation?>(s, settings, StoreType, v => s.FileSystemStorageImplementation = v, options);

			var sorting = s.Sorting = new SortingSettings();
			SetArray<string[], string>(s, settings, IndexSortSettings.Fields, v => sorting.Fields = v, v => sorting.Fields = new[] { v },
				options);
			SetArray<IndexSortOrder[], IndexSortOrder>(s, settings, Order, v => sorting.Order = v, v => sorting.Order = new[] { v },
				options);
			SetArray<IndexSortMode[], IndexSortMode>(s, settings, Mode, v => sorting.Mode = v, v => sorting.Mode = new[] { v }, options);
			SetArray<IndexSortMissing[], IndexSortMissing>(s, settings, IndexSortSettings.Missing, v => sorting.Missing = v,
				v => sorting.Missing = new[] { v }, options);

			s.Queries = new QueriesSettings();
			var queriesCache = s.Queries.Cache = new QueriesCacheSettings();
			Set<bool?>(s, settings, QueriesCacheEnabled, v => queriesCache.Enabled = v, options);

			var softDeletes = s.SoftDeletes = new SoftDeleteSettings();
			var softDeletesRetention = s.SoftDeletes.Retention = new SoftDeleteRetentionSettings();
			Set<long?>(s, settings, SoftDeletesEnabled, v => softDeletesRetention.Operations = v, options);

			IDictionary<string, object> dict = s;
			foreach (var kv in settings)
			{
				var setting = kv.Value;
				// TODO: Find a nicer way to avoid the serialization/deserialization roundtrip
				if (kv.Key == UpdatableIndexSettings.Analysis || kv.Key == "index.analysis")
					s.Analysis = ReserializeAndDeserialize<Analysis>(setting, options);
				if (kv.Key == Similarity || kv.Key == "index.similarity")
					s.Similarity = ReserializeAndDeserialize<Similarities>(setting, options);
				else
					dict.Add(kv.Key, setting);
			}
		}

		// Buffer the forward-only reader into the DOM and materialize a boxed object tree that matches the legacy
		// Utf8Json PrimitiveObjectFormatter (long for integral numbers, double otherwise, nested Dictionary/List).
		private static Dictionary<string, object> ReadDictionary(ref Utf8JsonReader reader)
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			return MaterializeValue(doc.RootElement) as Dictionary<string, object> ?? new Dictionary<string, object>();
		}

		private static object MaterializeValue(JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
					var dict = new Dictionary<string, object>();
					foreach (var property in element.EnumerateObject())
						dict[property.Name] = MaterializeValue(property.Value);
					return dict;
				case JsonValueKind.Array:
					var list = new List<object>();
					foreach (var item in element.EnumerateArray())
						list.Add(MaterializeValue(item));
					return list;
				case JsonValueKind.String:
					return element.GetString();
				case JsonValueKind.Number:
					if (element.TryGetInt64(out var l))
						return l;
					return element.GetDouble();
				case JsonValueKind.True:
					return true;
				case JsonValueKind.False:
					return false;
				default:
					return null;
			}
		}

		private static T ReserializeAndDeserialize<T>(object setting, JsonSerializerOptions options)
		{
			if (setting == null)
				return default;

			var json = JsonSerializer.Serialize(setting, setting.GetType(), options);
			return JsonSerializer.Deserialize<T>(json, options);
		}

		private static void Set<T>(IIndexSettings s, IDictionary<string, object> settings, string key, Action<T> assign,
			JsonSerializerOptions options
		)
		{
			if (!settings.TryGetValue(key, out var setting))
				return;

			var value = ConvertToValue<T>(setting, options);
			assign(value);
			s.Add(key, value);
			settings.Remove(key);
		}

		// TODO: Optimize this
		private static T ConvertToValue<T>(object setting, JsonSerializerOptions options)
		{
			if (setting is T t)
				return t;

			if (setting == null)
				return default;

			if (setting is IConvertible)
			{
				var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

				try
				{
					return (T)Convert.ChangeType(setting, type);
				}
				catch
				{
					// swallow exception and fall through to reserializing
				}
			}

			var json = JsonSerializer.Serialize(setting, setting.GetType(), options);
			return JsonSerializer.Deserialize<T>(json, options);
		}

		private static void SetArray<TArray, TItem>(IIndexSettings s, IDictionary<string, object> settings, string key,
			Action<TArray> assign, Action<TItem> assign2, JsonSerializerOptions options
		)
			where TArray : IEnumerable<TItem>
		{
			if (!settings.TryGetValue(key, out var v)) return;

			if (!(v is string) && v is IEnumerable)
			{
				var value = ConvertToValue<TArray>(v, options);
				assign(value);
				s.Add(key, value);
			}
			else
			{
				var value = ConvertToValue<TItem>(v, options);
				assign2(value);
				s.Add(key, value);
			}
			settings.Remove(key);
		}
	}

	/// <inheritdoc />
	internal class IndexSettingsConverter : JsonConverter<IIndexSettings>
	{
		public override bool HandleNull => true;

		public override IIndexSettings Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			DynamicIndexSettingsConverter.ReadIndexSettings(ref reader, options);

		public override void Write(Utf8JsonWriter writer, IIndexSettings value, JsonSerializerOptions options) =>
			DynamicIndexSettingsConverter.WriteIndexSettings(writer, value, options);
	}
}
