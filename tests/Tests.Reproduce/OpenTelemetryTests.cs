/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Diagnostics;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce
{
	public class OpenTelemetryTests
	{
		private const string SourceName = "OpenSearch.Net.RequestPipeline";

		/// <summary>
		/// Subscribes to the client's <see cref="ActivitySource"/> and captures completed <see cref="Activity"/>s
		/// whose <c>url.full</c> contains <paramref name="marker"/>. The marker keeps each test isolated: the
		/// listener is process-global, so without it a test would also capture spans produced by other tests
		/// running in parallel. Mirrors what an OpenTelemetry SDK does when a consumer calls
		/// <c>AddSource(OpenSearchClientActivitySource.ActivitySourceName)</c>.
		/// </summary>
		private static (List<Activity> activities, ActivityListener listener) SubscribeToActivities(string marker)
		{
			var activities = new List<Activity>();
			var listener = new ActivityListener
			{
				ShouldListenTo = source => source.Name == SourceName,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
				ActivityStopped = a =>
				{
					var url = a.TagObjects.FirstOrDefault(t => t.Key == "url.full").Value as string;
					if (url != null && url.Contains(marker))
						activities.Add(a);
				}
			};
			ActivitySource.AddActivityListener(listener);
			return (activities, listener);
		}

		private static OpenSearchLowLevelClient CreateLowLevelClient(int statusCode = 200)
		{
			var connection = new InMemoryConnection(
				responseBody: System.Text.Encoding.UTF8.GetBytes("{}"),
				statusCode: statusCode);
			var config = new ConnectionConfiguration(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection);
			return new OpenSearchLowLevelClient(config);
		}

		[U]
		public void ActivitySourceNameConstantMatchesTheDocumentedName()
		{
			// The source name is the public contract users pass to AddSource(...); guard it against silent change.
			OpenSearchClientActivitySource.ActivitySourceName.Should().Be(SourceName);
		}

		[U]
		public void EmitsActivityWithSemanticConventionTags()
		{
			var (activities, listener) = SubscribeToActivities("otel-tags-index");
			using (listener)
			{
				var client = CreateLowLevelClient();
				client.DoRequest<StringResponse>(HttpMethod.POST, "/otel-tags-index/_search", PostData.Serializable(new { }));

				var activity = activities.Should().ContainSingle().Subject;

				activity.DisplayName.Should().Be("POST");
				activity.Kind.Should().Be(ActivityKind.Client);
				activity.Status.Should().Be(ActivityStatusCode.Ok);

				// Assert against literal strings on purpose: these are the OpenTelemetry semantic
				// convention names that form the public contract with any OTel backend.
				var tags = activity.TagObjects.ToDictionary(t => t.Key, t => t.Value);
				tags.Should().Contain("db.system", "opensearch");
				tags.Should().Contain("http.request.method", "POST");
				tags.Should().Contain("server.address", "localhost");
				tags.Should().Contain("server.port", 9200);
				tags.Should().ContainKey("url.full");
				tags.Should().Contain("http.response.status_code", 200);
			}
		}

		[U]
		public void LowLevelRequestWithoutOperationNameHasNoDbOperationTag()
		{
			// A raw DoRequest carries no RequestParameters, so OperationName is null: the span name
			// falls back to the HTTP method and db.operation must be omitted (it is optional per spec).
			var (activities, listener) = SubscribeToActivities("otel-noop-index");
			using (listener)
			{
				var client = CreateLowLevelClient();
				client.DoRequest<StringResponse>(HttpMethod.GET, "/otel-noop-index/_doc/1", null);

				var activity = activities.Should().ContainSingle().Subject;
				activity.DisplayName.Should().Be("GET");
				activity.TagObjects.Select(t => t.Key).Should().NotContain("db.operation");
			}
		}

		[U]
		public void HighLevelRequestUsesRestOperationNameForSpanAndDbOperation()
		{
			var (activities, listener) = SubscribeToActivities("otel-highlevel-index");
			using (listener)
			{
				var connection = new InMemoryConnection(System.Text.Encoding.UTF8.GetBytes("{}"), 200);
				var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection)
					.DefaultIndex("otel-highlevel-index");
				var client = new OpenSearchClient(settings);

				client.Count<OtelDoc>();

				var activity = activities.Should().ContainSingle().Subject;
				activity.DisplayName.Should().Be("count");
				activity.TagObjects.ToDictionary(t => t.Key, t => t.Value)
					.Should().Contain("db.operation", "count");
			}
		}

		[U]
		public void MarksActivityAsErrorOnFailedResponse()
		{
			// statusCode 500 is a completed HTTP call with an unsuccessful status — exercises the
			// success==false branch of SetActivityEndState (not the exception path).
			var (activities, listener) = SubscribeToActivities("otel-500-index");
			using (listener)
			{
				var client = CreateLowLevelClient(statusCode: 500);
				client.DoRequest<StringResponse>(HttpMethod.POST, "/otel-500-index/_search", PostData.Serializable(new { }));

				var activity = activities.Should().ContainSingle().Subject;
				activity.Status.Should().Be(ActivityStatusCode.Error);
				activity.TagObjects.ToDictionary(t => t.Key, t => t.Value)
					.Should().Contain("http.response.status_code", 500);
			}
		}

		[U]
		public void MarksActivityAsErrorWhenTheConnectionThrows()
		{
			// A connection that throws mid-request exercises the catch-block SetActivityError path,
			// which the status-code cases never reach.
			var (activities, listener) = SubscribeToActivities("otel-throw-index");
			using (listener)
			{
				var config = new ConnectionConfiguration(
					new SingleNodeConnectionPool(new Uri("http://localhost:9200")),
					new ThrowingConnection());
				var client = new OpenSearchLowLevelClient(config);

				Action act = () => client.DoRequest<StringResponse>(HttpMethod.POST, "/otel-throw-index/_search", PostData.Serializable(new { }));

				// The original exception must still surface — instrumentation never swallows it.
				act.Should().Throw<Exception>();

				var activity = activities.Should().ContainSingle().Subject;
				activity.Status.Should().Be(ActivityStatusCode.Error);
				// No response was produced, so there is no status-code tag.
				activity.TagObjects.Select(t => t.Key).Should().NotContain("http.response.status_code");
			}
		}

		[U]
		public async Task AsyncRequestAlsoEmitsActivity()
		{
			var (activities, listener) = SubscribeToActivities("otel-async-index");
			using (listener)
			{
				var client = CreateLowLevelClient();
				await client.DoRequestAsync<StringResponse>(HttpMethod.POST, "/otel-async-index/_search", CancellationToken.None,
					PostData.Serializable(new { }));

				var activity = activities.Should().ContainSingle().Subject;
				activity.Kind.Should().Be(ActivityKind.Client);
				activity.Status.Should().Be(ActivityStatusCode.Ok);
			}
		}

		[U]
		public void EmitsNoActivityWhenNobodyIsListening()
		{
			// No listener subscribed: HasListeners is false, so StartActivity should never create an Activity.
			var (activities, listener) = SubscribeToActivities("otel-nolistener-index");
			listener.Dispose(); // stop listening immediately

			var client = CreateLowLevelClient();
			client.DoRequest<StringResponse>(HttpMethod.POST, "/otel-nolistener-index/_search", PostData.Serializable(new { }));

			activities.Should().BeEmpty();
		}

		private class OtelDoc
		{
			public string Id { get; set; }
		}

		/// <summary>An <see cref="IConnection"/> that always throws, to exercise the failure/catch path.</summary>
		private sealed class ThrowingConnection : IConnection
		{
			public TResponse Request<TResponse>(RequestData requestData)
				where TResponse : class, IOpenSearchResponse, new() =>
				throw new Exception("boom");

			public Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
				where TResponse : class, IOpenSearchResponse, new() =>
				throw new Exception("boom");

			public void Dispose() { }
		}
	}
}
