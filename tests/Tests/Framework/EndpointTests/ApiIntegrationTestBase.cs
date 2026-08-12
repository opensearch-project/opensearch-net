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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Framework.EndpointTests
{
	public abstract class ApiIntegrationTestBase<TCluster, TResponse, TInterface, TDescriptor, TInitializer>
		: ApiTestBase<TCluster, TResponse, TInterface, TDescriptor, TInitializer>
		where TCluster : IEphemeralCluster<EphemeralClusterConfiguration>, IOpenSearchClientTestCluster, new()
		where TResponse : class, IResponse
		where TDescriptor : class, TInterface
		where TInitializer : class, TInterface
		where TInterface : class
	{
		protected ApiIntegrationTestBase(TCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		public override IOpenSearchClient Client => Cluster.Client;
		protected abstract bool ExpectIsValid { get; }
		protected abstract int ExpectStatusCode { get; }
		protected override TInitializer Initializer => Activator.CreateInstance<TInitializer>();

		protected virtual void ExpectResponse(TResponse response) { }

		/// <summary>
		/// Returns true when the running OpenSearch version is below the minimum version declared
		/// by <c>TInitializer.MinimumServerVersion</c>, signalling that all [I] tests should be skipped.
		/// The <paramref name="reason"/> string is set to a human-readable explanation.
		/// </summary>
		protected static bool SkipForMinimumVersion(out string reason)
		{
			var field = typeof(TInitializer).GetField(
				"MinimumServerVersion",
				BindingFlags.Public | BindingFlags.Static);

			if (field == null || field.GetValue(null) is not string minVersion)
			{
				reason = null;
				return false;
			}

			// x-version-added in the spec uses 2-component versions (e.g. "2.7", "1.3").
			// OpenSearchVersion requires 3-component semver (e.g. "2.7.0").
			// Normalize by appending ".0" when needed.
			if (minVersion.Count(c => c == '.') < 2)
				minVersion += ".0";

			var serverVersion = TestClient.Configuration.OpenSearchVersion;
			if (serverVersion != null && serverVersion < minVersion)
			{
				reason = $"{typeof(TInitializer).Name} requires OpenSearch >= {minVersion}; " +
				         $"running against {serverVersion}";
				return true;
			}

			reason = null;
			return false;
		}

		// https://youtrack.jetbrains.com/issue/RIDER-19912
		[U] protected override Task HitsTheCorrectUrl() => base.HitsTheCorrectUrl();

		[U] protected override Task UsesCorrectHttpMethod() => base.UsesCorrectHttpMethod();

		[U] protected override void SerializesInitializer() => base.SerializesInitializer();

		[U] protected override void SerializesFluent() => base.SerializesFluent();

		[I] public virtual async Task ReturnsExpectedStatusCode()
		{
			if (SkipForMinimumVersion(out var reason)) return;
			await AssertOnAllResponses(r => r.ApiCall.HttpStatusCode.Should().Be(ExpectStatusCode));
		}

		[I] public virtual async Task ReturnsExpectedIsValid()
		{
			if (SkipForMinimumVersion(out var reason)) return;
			await AssertOnAllResponses(r => r.ShouldHaveExpectedIsValid(ExpectIsValid));
		}

		[I] public virtual async Task ReturnsExpectedResponse()
		{
			if (SkipForMinimumVersion(out var reason)) return;
			await AssertOnAllResponses(ExpectResponse);
		}

		protected override Task AssertOnAllResponses(Action<TResponse> assert) =>
			base.AssertOnAllResponses((r) =>
			{
				if (TestClient.Configuration.RunIntegrationTests && !r.IsValid && r.ApiCall.OriginalException != null
					&& !(r.ApiCall.OriginalException is OpenSearchClientException))
				{
					var e = ExceptionDispatchInfo.Capture(r.ApiCall.OriginalException.Demystify());
					throw new ResponseAssertionException(e.SourceException, r).Demystify();
				}

				try
				{
					assert(r);
				}
				catch (Exception e)
				{
					var ex = ExceptionDispatchInfo.Capture(e.Demystify());
					throw new ResponseAssertionException(ex.SourceException, r).Demystify();
				}
			});
	}

	public class ResponseAssertionException : Exception
	{

		public ResponseAssertionException(Exception innerException, IResponse response)
			: base(ResponseInMessage(innerException.Message, response), innerException) { }

		private static string ResponseInMessage(string innerExceptionMessage, IResponse r) => $@"{innerExceptionMessage}
Response Under Test:
{r.DebugInformation}";
	}
}
