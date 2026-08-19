/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;

namespace Tests.Reproduce;

public class GitHubIssue998
{
	/// <summary>
	/// MaxRetriesReached and FailedOverAllNodes audit events should have a valid Ended timestamp.
	/// Previously the Auditable returned by Audit() was discarded without Dispose(), leaving
	/// Ended at default(DateTime).
	/// </summary>
	[U]
	public void MaxRetriesReachedAuditShouldHaveValidEndedTimestamp()
	{
		var pool = new StaticConnectionPool(new[] { new Uri("http://node1:9200"), new Uri("http://node2:9200") });
		var settings = new ConnectionConfiguration(pool, new AlwaysFailsConnection())
			.DisablePing()
			.MaximumRetries(1);
		var client = new OpenSearchLowLevelClient(settings);

		var response = client.Search<StringResponse>("test-index",
			PostData.Serializable(new { query = new { match_all = new { } } }));

		var trail = response.ApiCall.AuditTrail;
		trail.Should().NotBeNullOrEmpty();

		foreach (var audit in trail)
		{
			audit.Ended.Should().NotBe(default(DateTime),
				$"audit event {audit.Event} should have a valid Ended timestamp");
			audit.Ended.Should().BeOnOrAfter(audit.Started,
				$"audit event {audit.Event} Ended should be >= Started");
		}

		var maxRetries = trail.FirstOrDefault(a => a.Event == AuditEvent.MaxRetriesReached);
		maxRetries.Should().NotBeNull("expected MaxRetriesReached in audit trail");
		maxRetries!.Ended.Should().NotBe(default(DateTime));

		var failedOverAll = trail.FirstOrDefault(a => a.Event == AuditEvent.FailedOverAllNodes);
		failedOverAll.Should().NotBeNull("expected FailedOverAllNodes in audit trail");
		failedOverAll!.Ended.Should().NotBe(default(DateTime));
	}

	/// <summary>
	/// MaxTimeoutReached audit event should have a valid Ended timestamp.
	/// </summary>
	[U]
	public void MaxTimeoutReachedAuditShouldHaveValidEndedTimestamp()
	{
		var pool = new StaticConnectionPool(new[] { new Uri("http://node1:9200"), new Uri("http://node2:9200") });
		var settings = new ConnectionConfiguration(pool, new SlowConnection(TimeSpan.FromSeconds(10)))
			.DisablePing()
			.RequestTimeout(TimeSpan.FromSeconds(1))
			.MaxRetryTimeout(TimeSpan.FromSeconds(2));
		var client = new OpenSearchLowLevelClient(settings);

		var response = client.Search<StringResponse>("test-index",
			PostData.Serializable(new { query = new { match_all = new { } } }));

		var trail = response.ApiCall.AuditTrail;
		trail.Should().NotBeNullOrEmpty();

		var maxTimeout = trail.FirstOrDefault(a => a.Event == AuditEvent.MaxTimeoutReached);
		maxTimeout.Should().NotBeNull("expected MaxTimeoutReached in audit trail");
		maxTimeout!.Ended.Should().NotBe(default(DateTime),
			"MaxTimeoutReached audit event should have a valid Ended timestamp");
		maxTimeout.Ended.Should().BeOnOrAfter(maxTimeout.Started);
	}

	private sealed class AlwaysFailsConnection : IConnection
	{
		public TResponse Request<TResponse>(RequestData requestData)
			where TResponse : class, IOpenSearchResponse, new() =>
			ResponseBuilder.ToResponse<TResponse>(requestData, new HttpRequestException("simulated failure"), null, null, Stream.Null, RequestData.MimeType);

		public Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
			where TResponse : class, IOpenSearchResponse, new() =>
			Task.FromResult(Request<TResponse>(requestData));

		public void Dispose() { }
	}

	private sealed class SlowConnection : IConnection
	{
		private readonly TimeSpan _delay;
		public SlowConnection(TimeSpan delay) => _delay = delay;

		public TResponse Request<TResponse>(RequestData requestData)
			where TResponse : class, IOpenSearchResponse, new()
		{
			Thread.Sleep(_delay);
			return ResponseBuilder.ToResponse<TResponse>(requestData, new HttpRequestException("timeout"), null, null, Stream.Null, RequestData.MimeType);
		}

		public Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
			where TResponse : class, IOpenSearchResponse, new() =>
			Task.FromResult(Request<TResponse>(requestData));

		public void Dispose() { }
	}
}
