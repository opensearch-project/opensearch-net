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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenSearch.OpenSearch.Managed.Configuration;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;
using OpenSearch.OpenSearch.Managed.FileSystem;
using OpenSearch.Stack.ArtifactsApi;
using ProcNet;
using ProcNet.Std;

namespace OpenSearch.OpenSearch.Managed
{
	public class OpenSearchNode : ObservableProcess
	{
		private readonly ManualResetEvent _startedHandle = new ManualResetEvent(false);

		public OpenSearchNode(OpenSearchVersion version, string openSearchHome = null)
			: this(new NodeConfiguration(new ClusterConfiguration(version, (v, s) => new NodeFileSystem(v, openSearchHome)))) { }

		public OpenSearchNode(NodeConfiguration config) : base(StartArgs(config)) => NodeConfiguration = config;

		public string Version { get; private set; }
		public int? Port { get; private set; }
		public bool NodeStarted { get; private set; }
		public NodeConfiguration NodeConfiguration { get; }

		private int? JavaProcessId { get; set; }
		public override int? ProcessId => JavaProcessId ?? base.ProcessId;
		public int? HostProcessId => base.ProcessId;

		/// <summary>
		///     Set this true if you want the node to go into assumed started state as soon as its waiting for more nodes to start
		///     doing the election.
		///     <para>Useful to speed up starting multi node clusters</para>
		/// </summary>
		public bool AssumeStartedOnNotEnoughMasterPing { get; set; }

		internal IConsoleLineHandler Writer { get; private set; }

		public Exception LastSeenException { get; set; }
		public WaitHandle StartedHandle => _startedHandle;

		private static StartArguments StartArgs(NodeConfiguration config)
		{
			//var args = new[] {config.FileSystem.Binary}.Concat(config.CommandLineArguments);

			var startArguments = new StartArguments(config.FileSystem.Binary, config.CommandLineArguments)
			{
				SendControlCFirst = true,
				Environment = EnvVars(config),
				WaitForExit = config.WaitForShutdown,
				WaitForStreamReadersTimeout = config.WaitForShutdown
			};
			config.ModifyStartArguments(startArguments);
			return startArguments;
		}

		private static Dictionary<string, string> EnvVars(NodeConfiguration config)
		{
			var environmentVariables = new Dictionary<string, string> { { "OPENSEARCH_JAVA_OPTS", "-Xms1g -Xmx1g" } };
			if (!string.IsNullOrWhiteSpace(config.FileSystem.ConfigPath))
				environmentVariables.Add(config.FileSystem.ConfigEnvironmentVariableName, config.FileSystem.ConfigPath);

			if (!string.IsNullOrWhiteSpace(config.FileSystem.OpenSearchHome))
				environmentVariables.Add("OPENSEARCH_HOME", config.FileSystem.OpenSearchHome);

			return environmentVariables;
		}

		private bool AssumedStartedStateChecker(string section, string message)
		{
			if (AssumeStartedOnNotEnoughMasterPing
				&& section.Contains("ZenDiscovery")
				&& message.Contains("not enough master nodes discovered during pinging"))
				return true;

			return false;
		}

		public IDisposable Start() => Start(TimeSpan.FromMinutes(2));

		public IDisposable Start(TimeSpan waitForStarted) => Start(new LineHighlightWriter(), waitForStarted);

		public IDisposable Start(IConsoleLineHandler writer, TimeSpan waitForStarted)
		{
			var node = NodeConfiguration.DesiredNodeName;
			var subscription = SubscribeLines(writer);
			if (WaitForStarted(waitForStarted)) return subscription;

			subscription.Dispose();
			throw new OpenSearchCleanExitException(
				$"Failed to start node: {node} before the configured timeout of: {waitForStarted}");
		}

		public IDisposable SubscribeLines() => SubscribeLines(new LineHighlightWriter());

		public IDisposable SubscribeLines(IConsoleLineHandler writer) =>
			SubscribeLines(writer, delegate { }, delegate { }, delegate { });

		public IDisposable SubscribeLines(IConsoleLineHandler writer, Action<LineOut> onNext) =>
			SubscribeLines(writer, onNext, delegate { }, delegate { });

		public IDisposable SubscribeLines(IConsoleLineHandler writer, Action<LineOut> onNext,
			Action<Exception> onError
		) =>
			SubscribeLines(writer, onNext, onError, delegate { });

		public IDisposable SubscribeLines(IConsoleLineHandler writer, Action<LineOut> onNext, Action<Exception> onError,
			Action onCompleted
		)
		{
			Writer = writer;
			var node = NodeConfiguration.DesiredNodeName;
			writer?.WriteDiagnostic($"OpenSearch location: [{Binary}]", node);
			writer?.WriteDiagnostic($"Settings: {{{string.Join(" ", NodeConfiguration.CommandLineArguments)}}}", node);

			var envVarName = "JAVA_HOME";
			var javaHome = Environment.GetEnvironmentVariable(envVarName);
			writer?.WriteDiagnostic($"{envVarName}: {{{javaHome}}}", node);
			Process.StartInfo.Environment[envVarName] = javaHome;

			return SubscribeLines(
				l =>
				{
					writer?.Handle(l);
					onNext?.Invoke(l);
				},
				e =>
				{
					LastSeenException = e;
					writer?.Handle(e);
					onError?.Invoke(e);
					_startedHandle.Set();
				},
				() =>
				{
					onCompleted?.Invoke();
					_startedHandle.Set();
				});
		}

		public bool WaitForStarted(TimeSpan timeout) => _startedHandle.WaitOne(timeout);

		protected override void OnBeforeSetCompletedHandle()
		{
			_startedHandle.Set();
			base.OnBeforeSetCompletedHandle();
		}

		protected override void OnBeforeWaitForEndOfStreamsError(TimeSpan waited)
		{
			// The wait for streams finished before streams were fully read.
			// this usually indicates the process is still running.
			// Proc will successfully kill the host but will leave the JavaProcess the bat file starts running
			// The opensearch jar is closing down so won't leak but might prevent EphemeralClusterComposer to do its clean up.
			// We do a hard kill on both here to make sure both processes are gone.
			HardKill(HostProcessId);
			HardKill(JavaProcessId);
		}

		private static void HardKill(int? processId)
		{
			if (!processId.HasValue) return;

			try
			{
				var p = Process.GetProcessById(processId.Value);
				p.Kill();
			}
			catch (Exception) { }
		}

		protected override bool ContinueReadingFromProcessReaders()
		{
			if (!NodeStarted) return true;

			return true;

			// some how if we return false here it leads to Task starvation in Proc and tests in e.g will OpenSearch.OpenSearch.Xunit will start
			// to timeout. This makes little sense to me now, so leaving this performance optimization out for now. Hopefully another fresh look will yield
			// to (not so) obvious.
			//return this.NodeConfiguration.ShowOpenSearchOutputAfterStarted;
		}

		protected override bool KeepBufferingLines(LineOut c)
		{
			var lineOutParser = LineOutParser.OpenSearch;
			//if the node is already started only keep buffering lines while we have a writer and the nodeconfiguration wants output after started
			if (NodeStarted)
			{
				var keepBuffering = Writer != null && NodeConfiguration.ShowOpenSearchOutputAfterStarted;
				if (!keepBuffering) CancelAsyncReads();
				return keepBuffering;
			}

			var parsed = lineOutParser.TryParse(c?.Line, out _, out _, out var section, out _, out var message,
				out var started);

			if (!parsed) return Writer != null;

			if (JavaProcessId == null && lineOutParser.TryParseNodeInfo(section, message, out var version, out var pid))
			{
				JavaProcessId = pid;
				Version = version;
			}

			else if (lineOutParser.TryGetPortNumber(section, message, out var port))
			{
				Port = port;
				var dp = NodeConfiguration.DesiredPort;
				if (dp.HasValue && Port != dp.Value)
					throw new OpenSearchCleanExitException(
						$"Node started on port {port} but {dp.Value} was requested");
			}

			if (!started) started = AssumedStartedStateChecker(section, message);
			if (started)
			{
				if (!Port.HasValue)
					throw new OpenSearchCleanExitException(
						$"Node started but OpenSearchNode did not grab its port number");

				NodeStarted = true;
				_startedHandle.Set();
			}

			// if we have dont a writer always return true
			if (Writer != null) return true;

			//otherwise only keep buffering if we are not started
			return !started;
		}
	}
}
