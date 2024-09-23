/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace OpenSearch.Net
{
	[Flags, StringEnum]
	public enum ClusterStateMetric
	{
		[EnumMember(Value = "blocks")]
		Blocks = 1 << 0,
		[EnumMember(Value = "metadata")]
		Metadata = 1 << 1,
		[EnumMember(Value = "nodes")]
		Nodes = 1 << 2,
		[EnumMember(Value = "routing_table")]
		RoutingTable = 1 << 3,
		[EnumMember(Value = "routing_nodes")]
		RoutingNodes = 1 << 4,
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerNode"/> instead</remarks>
		[EnumMember(Value = "master_node")]
		MasterNode = 1 << 5,
		[EnumMember(Value = "version")]
		Version = 1 << 6,
		[EnumMember(Value = "_all")]
		All = 1 << 7,
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterNode"/></remarks>
		[EnumMember(Value = "cluster_manager_node")]
		ClusterManagerNode = 1 << 8,
	}

	[Flags, StringEnum]
	public enum NodesInfoMetric
	{
		[EnumMember(Value = "settings")]
		Settings = 1 << 0,
		[EnumMember(Value = "os")]
		Os = 1 << 1,
		[EnumMember(Value = "process")]
		Process = 1 << 2,
		[EnumMember(Value = "jvm")]
		Jvm = 1 << 3,
		[EnumMember(Value = "thread_pool")]
		ThreadPool = 1 << 4,
		[EnumMember(Value = "transport")]
		Transport = 1 << 5,
		[EnumMember(Value = "http")]
		Http = 1 << 6,
		[EnumMember(Value = "plugins")]
		Plugins = 1 << 7,
		[EnumMember(Value = "ingest")]
		Ingest = 1 << 8
	}

	[Flags, StringEnum]
	public enum NodesStatsMetric
	{
		[EnumMember(Value = "breaker")]
		Breaker = 1 << 0,
		[EnumMember(Value = "fs")]
		Fs = 1 << 1,
		[EnumMember(Value = "http")]
		Http = 1 << 2,
		[EnumMember(Value = "indices")]
		Indices = 1 << 3,
		[EnumMember(Value = "jvm")]
		Jvm = 1 << 4,
		[EnumMember(Value = "os")]
		Os = 1 << 5,
		[EnumMember(Value = "process")]
		Process = 1 << 6,
		[EnumMember(Value = "thread_pool")]
		ThreadPool = 1 << 7,
		[EnumMember(Value = "transport")]
		Transport = 1 << 8,
		[EnumMember(Value = "discovery")]
		Discovery = 1 << 9,
		[EnumMember(Value = "indexing_pressure")]
		IndexingPressure = 1 << 10,
		[EnumMember(Value = "_all")]
		All = 1 << 11
	}

	[Flags, StringEnum]
	public enum NodesStatsIndexMetric
	{
		[EnumMember(Value = "store")]
		Store = 1 << 0,
		[EnumMember(Value = "indexing")]
		Indexing = 1 << 1,
		[EnumMember(Value = "get")]
		Get = 1 << 2,
		[EnumMember(Value = "search")]
		Search = 1 << 3,
		[EnumMember(Value = "merge")]
		Merge = 1 << 4,
		[EnumMember(Value = "flush")]
		Flush = 1 << 5,
		[EnumMember(Value = "refresh")]
		Refresh = 1 << 6,
		[EnumMember(Value = "query_cache")]
		QueryCache = 1 << 7,
		[EnumMember(Value = "fielddata")]
		Fielddata = 1 << 8,
		[EnumMember(Value = "docs")]
		Docs = 1 << 9,
		[EnumMember(Value = "warmer")]
		Warmer = 1 << 10,
		[EnumMember(Value = "completion")]
		Completion = 1 << 11,
		[EnumMember(Value = "segments")]
		Segments = 1 << 12,
		[EnumMember(Value = "translog")]
		Translog = 1 << 13,
		[EnumMember(Value = "request_cache")]
		RequestCache = 1 << 14,
		[EnumMember(Value = "recovery")]
		Recovery = 1 << 15,
		[EnumMember(Value = "_all")]
		All = 1 << 16
	}

	[Flags, StringEnum]
	public enum NodesUsageMetric
	{
		[EnumMember(Value = "rest_actions")]
		RestActions = 1 << 0,
		[EnumMember(Value = "_all")]
		All = 1 << 1
	}

	[StringEnum]
	public enum DefaultOperator
	{
		[EnumMember(Value = "AND")]
		And,
		[EnumMember(Value = "OR")]
		Or
	}

	[StringEnum]
	public enum SearchType
	{
		[EnumMember(Value = "query_then_fetch")]
		QueryThenFetch,
		[EnumMember(Value = "dfs_query_then_fetch")]
		DfsQueryThenFetch
	}

	[StringEnum]
	public enum SuggestMode
	{
		[EnumMember(Value = "missing")]
		Missing,
		[EnumMember(Value = "popular")]
		Popular,
		[EnumMember(Value = "always")]
		Always
	}

	[StringEnum]
	public enum Refresh
	{
		[EnumMember(Value = "true")]
		True,
		[EnumMember(Value = "false")]
		False,
		[EnumMember(Value = "wait_for")]
		WaitFor
	}

	[StringEnum]
	public enum Health
	{
		[EnumMember(Value = "green")]
		Green,
		[EnumMember(Value = "yellow")]
		Yellow,
		[EnumMember(Value = "red")]
		Red
	}

	[StringEnum]
	public enum Size
	{
		[EnumMember(Value = "")]
		Raw,
		[EnumMember(Value = "k")]
		K,
		[EnumMember(Value = "m")]
		M,
		[EnumMember(Value = "g")]
		G,
		[EnumMember(Value = "t")]
		T,
		[EnumMember(Value = "p")]
		P
	}

	[StringEnum]
	public enum WaitForEvents
	{
		[EnumMember(Value = "immediate")]
		Immediate,
		[EnumMember(Value = "urgent")]
		Urgent,
		[EnumMember(Value = "high")]
		High,
		[EnumMember(Value = "normal")]
		Normal,
		[EnumMember(Value = "low")]
		Low,
		[EnumMember(Value = "languid")]
		Languid
	}

	[StringEnum]
	public enum WaitForStatus
	{
		[EnumMember(Value = "green")]
		Green,
		[EnumMember(Value = "yellow")]
		Yellow,
		[EnumMember(Value = "red")]
		Red
	}

	[Flags, StringEnum]
	public enum ClusterRerouteMetric
	{
		[EnumMember(Value = "blocks")]
		Blocks = 1 << 0,
		[EnumMember(Value = "metadata")]
		Metadata = 1 << 1,
		[EnumMember(Value = "nodes")]
		Nodes = 1 << 2,
		[EnumMember(Value = "routing_table")]
		RoutingTable = 1 << 3,
		[EnumMember(Value = "master_node")]
		MasterNode = 1 << 4,
		[EnumMember(Value = "version")]
		Version = 1 << 5,
		[EnumMember(Value = "_all")]
		All = 1 << 6
	}

	[StringEnum]
	public enum VersionType
	{
		[EnumMember(Value = "internal")]
		Internal,
		[EnumMember(Value = "external")]
		External,
		[EnumMember(Value = "external_gte")]
		ExternalGte,
	}

	[StringEnum]
	public enum Conflicts
	{
		[EnumMember(Value = "abort")]
		Abort,
		[EnumMember(Value = "proceed")]
		Proceed
	}

	[StringEnum]
	public enum OpType
	{
		[EnumMember(Value = "index")]
		Index,
		[EnumMember(Value = "create")]
		Create
	}

	[StringEnum]
	public enum IndicesShardStoresStatus
	{
		[EnumMember(Value = "green")]
		Green,
		[EnumMember(Value = "yellow")]
		Yellow,
		[EnumMember(Value = "red")]
		Red,
		[EnumMember(Value = "all")]
		All
	}

	[StringEnum]
	public enum ThreadType
	{
		[EnumMember(Value = "cpu")]
		Cpu,
		[EnumMember(Value = "wait")]
		Wait,
		[EnumMember(Value = "block")]
		Block
	}

	[StringEnum]
	public enum GroupBy
	{
		[EnumMember(Value = "nodes")]
		Nodes,
		[EnumMember(Value = "parents")]
		Parents,
		[EnumMember(Value = "none")]
		None
	}

	public static partial class KnownEnums
	{
		private static readonly ConcurrentDictionary<Type, Func<Enum, string>> EnumStringResolvers = new();
		static KnownEnums()
		{
			AddEnumStringResolver<ClusterStateMetric>(GetStringValue);
			AddEnumStringResolver<NodesInfoMetric>(GetStringValue);
			AddEnumStringResolver<NodesStatsMetric>(GetStringValue);
			AddEnumStringResolver<NodesStatsIndexMetric>(GetStringValue);
			AddEnumStringResolver<NodesUsageMetric>(GetStringValue);
			AddEnumStringResolver<DefaultOperator>(GetStringValue);
			AddEnumStringResolver<SearchType>(GetStringValue);
			AddEnumStringResolver<SuggestMode>(GetStringValue);
			AddEnumStringResolver<Refresh>(GetStringValue);
			AddEnumStringResolver<Health>(GetStringValue);
			AddEnumStringResolver<Size>(GetStringValue);
			AddEnumStringResolver<WaitForEvents>(GetStringValue);
			AddEnumStringResolver<WaitForStatus>(GetStringValue);
			AddEnumStringResolver<ClusterRerouteMetric>(GetStringValue);
			AddEnumStringResolver<VersionType>(GetStringValue);
			AddEnumStringResolver<Conflicts>(GetStringValue);
			AddEnumStringResolver<OpType>(GetStringValue);
			AddEnumStringResolver<IndicesShardStoresStatus>(GetStringValue);
			AddEnumStringResolver<ThreadType>(GetStringValue);
			AddEnumStringResolver<GroupBy>(GetStringValue);
			RegisterEnumStringResolvers();
		}

        private static void AddEnumStringResolver<T>(Func<T, string> resolver) where T : Enum =>
            EnumStringResolvers.TryAdd(typeof(T), e => resolver((T) e));

		static partial void RegisterEnumStringResolvers();

		public static string GetStringValue(this ClusterStateMetric enumValue)
		{
			if ((enumValue & ClusterStateMetric.All) != 0)
				return "_all";
			var list = new List<string>();
			if ((enumValue & ClusterStateMetric.Blocks) != 0)
				list.Add("blocks");
			if ((enumValue & ClusterStateMetric.Metadata) != 0)
				list.Add("metadata");
			if ((enumValue & ClusterStateMetric.Nodes) != 0)
				list.Add("nodes");
			if ((enumValue & ClusterStateMetric.RoutingTable) != 0)
				list.Add("routing_table");
			if ((enumValue & ClusterStateMetric.RoutingNodes) != 0)
				list.Add("routing_nodes");
			if ((enumValue & ClusterStateMetric.MasterNode) != 0)
				list.Add("master_node");
			if ((enumValue & ClusterStateMetric.ClusterManagerNode) != 0)
				list.Add("cluster_manager_node");
			if ((enumValue & ClusterStateMetric.Version) != 0)
				list.Add("version");
			return string.Join(",", list);
		}

		public static string GetStringValue(this NodesInfoMetric enumValue)
		{
			var list = new List<string>();
			if ((enumValue & NodesInfoMetric.Settings) != 0)
				list.Add("settings");
			if ((enumValue & NodesInfoMetric.Os) != 0)
				list.Add("os");
			if ((enumValue & NodesInfoMetric.Process) != 0)
				list.Add("process");
			if ((enumValue & NodesInfoMetric.Jvm) != 0)
				list.Add("jvm");
			if ((enumValue & NodesInfoMetric.ThreadPool) != 0)
				list.Add("thread_pool");
			if ((enumValue & NodesInfoMetric.Transport) != 0)
				list.Add("transport");
			if ((enumValue & NodesInfoMetric.Http) != 0)
				list.Add("http");
			if ((enumValue & NodesInfoMetric.Plugins) != 0)
				list.Add("plugins");
			if ((enumValue & NodesInfoMetric.Ingest) != 0)
				list.Add("ingest");
			return string.Join(",", list);
		}

		public static string GetStringValue(this NodesStatsMetric enumValue)
		{
			if ((enumValue & NodesStatsMetric.All) != 0)
				return "_all";
			var list = new List<string>();
			if ((enumValue & NodesStatsMetric.Breaker) != 0)
				list.Add("breaker");
			if ((enumValue & NodesStatsMetric.Fs) != 0)
				list.Add("fs");
			if ((enumValue & NodesStatsMetric.Http) != 0)
				list.Add("http");
			if ((enumValue & NodesStatsMetric.Indices) != 0)
				list.Add("indices");
			if ((enumValue & NodesStatsMetric.Jvm) != 0)
				list.Add("jvm");
			if ((enumValue & NodesStatsMetric.Os) != 0)
				list.Add("os");
			if ((enumValue & NodesStatsMetric.Process) != 0)
				list.Add("process");
			if ((enumValue & NodesStatsMetric.ThreadPool) != 0)
				list.Add("thread_pool");
			if ((enumValue & NodesStatsMetric.Transport) != 0)
				list.Add("transport");
			if ((enumValue & NodesStatsMetric.Discovery) != 0)
				list.Add("discovery");
			if ((enumValue & NodesStatsMetric.IndexingPressure) != 0)
				list.Add("indexing_pressure");
			return string.Join(",", list);
		}

		public static string GetStringValue(this NodesStatsIndexMetric enumValue)
		{
			if ((enumValue & NodesStatsIndexMetric.All) != 0)
				return "_all";
			var list = new List<string>();
			if ((enumValue & NodesStatsIndexMetric.Store) != 0)
				list.Add("store");
			if ((enumValue & NodesStatsIndexMetric.Indexing) != 0)
				list.Add("indexing");
			if ((enumValue & NodesStatsIndexMetric.Get) != 0)
				list.Add("get");
			if ((enumValue & NodesStatsIndexMetric.Search) != 0)
				list.Add("search");
			if ((enumValue & NodesStatsIndexMetric.Merge) != 0)
				list.Add("merge");
			if ((enumValue & NodesStatsIndexMetric.Flush) != 0)
				list.Add("flush");
			if ((enumValue & NodesStatsIndexMetric.Refresh) != 0)
				list.Add("refresh");
			if ((enumValue & NodesStatsIndexMetric.QueryCache) != 0)
				list.Add("query_cache");
			if ((enumValue & NodesStatsIndexMetric.Fielddata) != 0)
				list.Add("fielddata");
			if ((enumValue & NodesStatsIndexMetric.Docs) != 0)
				list.Add("docs");
			if ((enumValue & NodesStatsIndexMetric.Warmer) != 0)
				list.Add("warmer");
			if ((enumValue & NodesStatsIndexMetric.Completion) != 0)
				list.Add("completion");
			if ((enumValue & NodesStatsIndexMetric.Segments) != 0)
				list.Add("segments");
			if ((enumValue & NodesStatsIndexMetric.Translog) != 0)
				list.Add("translog");
			if ((enumValue & NodesStatsIndexMetric.RequestCache) != 0)
				list.Add("request_cache");
			if ((enumValue & NodesStatsIndexMetric.Recovery) != 0)
				list.Add("recovery");
			return string.Join(",", list);
		}

		public static string GetStringValue(this NodesUsageMetric enumValue)
		{
			if ((enumValue & NodesUsageMetric.All) != 0)
				return "_all";
			var list = new List<string>();
			if ((enumValue & NodesUsageMetric.RestActions) != 0)
				list.Add("rest_actions");
			return string.Join(",", list);
		}

		public static string GetStringValue(this DefaultOperator enumValue) =>
            enumValue switch
            {
                DefaultOperator.And => "AND",
                DefaultOperator.Or => "OR",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'DefaultOperator'")
            };

        public static string GetStringValue(this SearchType enumValue) =>
            enumValue switch
            {
                SearchType.QueryThenFetch => "query_then_fetch",
                SearchType.DfsQueryThenFetch => "dfs_query_then_fetch",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'SearchType'")
            };

        public static string GetStringValue(this SuggestMode enumValue) =>
            enumValue switch
            {
                SuggestMode.Missing => "missing",
                SuggestMode.Popular => "popular",
                SuggestMode.Always => "always",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'SuggestMode'")
            };

        public static string GetStringValue(this Refresh enumValue) =>
            enumValue switch
            {
                Refresh.True => "true",
                Refresh.False => "false",
                Refresh.WaitFor => "wait_for",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'Refresh'")
            };

        public static string GetStringValue(this Health enumValue) =>
            enumValue switch
            {
                Health.Green => "green",
                Health.Yellow => "yellow",
                Health.Red => "red",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'Health'")
            };

        public static string GetStringValue(this Size enumValue) =>
            enumValue switch
            {
                Size.Raw => "",
                Size.K => "k",
                Size.M => "m",
                Size.G => "g",
                Size.T => "t",
                Size.P => "p",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'Size'")
            };

        public static string GetStringValue(this WaitForEvents enumValue) =>
            enumValue switch
            {
                WaitForEvents.Immediate => "immediate",
                WaitForEvents.Urgent => "urgent",
                WaitForEvents.High => "high",
                WaitForEvents.Normal => "normal",
                WaitForEvents.Low => "low",
                WaitForEvents.Languid => "languid",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'WaitForEvents'")
            };

        public static string GetStringValue(this WaitForStatus enumValue) =>
            enumValue switch
            {
                WaitForStatus.Green => "green",
                WaitForStatus.Yellow => "yellow",
                WaitForStatus.Red => "red",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'WaitForStatus'")
            };

        public static string GetStringValue(this ClusterRerouteMetric enumValue)
		{
			if ((enumValue & ClusterRerouteMetric.All) != 0)
				return "_all";
			var list = new List<string>();
			if ((enumValue & ClusterRerouteMetric.Blocks) != 0)
				list.Add("blocks");
			if ((enumValue & ClusterRerouteMetric.Metadata) != 0)
				list.Add("metadata");
			if ((enumValue & ClusterRerouteMetric.Nodes) != 0)
				list.Add("nodes");
			if ((enumValue & ClusterRerouteMetric.RoutingTable) != 0)
				list.Add("routing_table");
			if ((enumValue & ClusterRerouteMetric.MasterNode) != 0)
				list.Add("master_node");
			if ((enumValue & ClusterRerouteMetric.Version) != 0)
				list.Add("version");
			return string.Join(",", list);
		}

		public static string GetStringValue(this VersionType enumValue) =>
            enumValue switch
            {
                VersionType.Internal => "internal",
                VersionType.External => "external",
                VersionType.ExternalGte => "external_gte",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'VersionType'")
            };

        public static string GetStringValue(this Conflicts enumValue) =>
            enumValue switch
            {
                Conflicts.Abort => "abort",
                Conflicts.Proceed => "proceed",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'Conflicts'")
            };

        public static string GetStringValue(this OpType enumValue) =>
            enumValue switch
            {
                OpType.Index => "index",
                OpType.Create => "create",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'OpType'")
            };

        public static string GetStringValue(this IndicesShardStoresStatus enumValue) =>
            enumValue switch
            {
                IndicesShardStoresStatus.Green => "green",
                IndicesShardStoresStatus.Yellow => "yellow",
                IndicesShardStoresStatus.Red => "red",
                IndicesShardStoresStatus.All => "all",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'IndicesShardStoresStatus'")
            };

        public static string GetStringValue(this ThreadType enumValue) =>
            enumValue switch
            {
                ThreadType.Cpu => "cpu",
                ThreadType.Wait => "wait",
                ThreadType.Block => "block",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'ThreadType'")
            };

        public static string GetStringValue(this GroupBy enumValue) =>
            enumValue switch
            {
                GroupBy.Nodes => "nodes",
                GroupBy.Parents => "parents",
                GroupBy.None => "none",
                _ => throw new ArgumentException($"'{enumValue.ToString()}' is not a valid value for enum 'GroupBy'")
            };

        public static string GetStringValue(this Enum e)
		{
			var type = e.GetType();
			var resolver = EnumStringResolvers.GetOrAdd(type, GetEnumStringResolver);
			return resolver(e);
		}

		private static Func<Enum, string> GetEnumStringResolver(Type type)
		{
			var values = Enum.GetValues(type);
			var dictionary = new Dictionary<Enum, string>(values.Length);
			for (var index = 0; index < values.Length; index++)
			{
				var value = values.GetValue(index);
				var info = type.GetField(value.ToString());
				var da = (EnumMemberAttribute[])info.GetCustomAttributes(typeof(EnumMemberAttribute), false);
				var stringValue = da.Length > 0 ? da[0].Value : Enum.GetName(type, value);
				dictionary.Add((Enum)value, stringValue);
			}

			var isFlag = type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
			return e => !isFlag
                ? dictionary[e]
                : string.Join(",", dictionary.Where(kv => e.HasFlag(kv.Key)).Select(kv => kv.Value));
		}
	}
}
