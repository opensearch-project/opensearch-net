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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSearch.Net.VirtualizedCluster.Extensions;

namespace OpenSearch.Net.VirtualizedCluster.Audit;

public class Auditor
{
    private VirtualizedCluster _cluster;
    private VirtualizedCluster _clusterAsync;

    public Auditor(Func<VirtualizedCluster> setup) => Cluster = setup;

    private Auditor(VirtualizedCluster cluster, VirtualizedCluster clusterAsync)
    {
        _cluster = cluster;
        _clusterAsync = clusterAsync;
        StartedUp = true;
    }

    public Action<IConnectionPool> AssertPoolAfterCall { get; set; }
    public Action<IConnectionPool> AssertPoolAfterStartup { get; set; }

    public Action<IConnectionPool> AssertPoolBeforeCall { get; set; }
    public Action<IConnectionPool> AssertPoolBeforeStartup { get; set; }

    public List<OpenSearch.Net.Audit> AsyncAuditTrail { get; set; }
    public List<OpenSearch.Net.Audit> AuditTrail { get; set; }
    public Func<VirtualizedCluster> Cluster { get; set; }

    public IOpenSearchResponse Response { get; internal set; }
    public IOpenSearchResponse ResponseAsync { get; internal set; }

    private bool StartedUp { get; }


    public void ChangeTime(Func<DateTime, DateTime> selector)
    {
        _cluster ??= Cluster();
        _clusterAsync ??= Cluster();

        _cluster.ChangeTime(selector);
        _clusterAsync.ChangeTime(selector);
    }

    public async Task<Auditor> TraceStartup(ClientCall callTrace = null)
    {
        //synchronous code path
        _cluster = _cluster ?? Cluster();
        if (!StartedUp) AssertPoolBeforeStartup?.Invoke(_cluster.ConnectionPool);
        AssertPoolBeforeCall?.Invoke(_cluster.ConnectionPool);
        Response = _cluster.ClientCall(callTrace?.RequestOverrides);
        AuditTrail = Response.ApiCall.AuditTrail;
        if (!StartedUp) AssertPoolAfterStartup?.Invoke(_cluster.ConnectionPool);
        AssertPoolAfterCall?.Invoke(_cluster.ConnectionPool);

        //async code path
        _clusterAsync = _clusterAsync ?? Cluster();
        if (!StartedUp) AssertPoolBeforeStartup?.Invoke(_clusterAsync.ConnectionPool);
        AssertPoolBeforeCall?.Invoke(_clusterAsync.ConnectionPool);
        ResponseAsync = await _clusterAsync.ClientCallAsync(callTrace?.RequestOverrides).ConfigureAwait(false);
        AsyncAuditTrail = ResponseAsync.ApiCall.AuditTrail;
        if (!StartedUp) AssertPoolAfterStartup?.Invoke(_clusterAsync.ConnectionPool);
        AssertPoolAfterCall?.Invoke(_clusterAsync.ConnectionPool);
        return new Auditor(_cluster, _clusterAsync);
    }

    public async Task<Auditor> TraceCall(ClientCall callTrace, int nthCall = 0)
    {
        await TraceStartup(callTrace).ConfigureAwait(false);
        return AssertAuditTrails(callTrace, nthCall);
    }

#pragma warning disable 1998 // Async method lacks 'await' operators and will run synchronously
    private async Task TraceException<TException>(ClientCall callTrace, Action<TException> assert)
#pragma warning restore 1998 // Async method lacks 'await' operators and will run synchronously
        where TException : OpenSearchClientException
    {
        _cluster = _cluster ?? Cluster();
        _cluster.ClientThrows(true);
        AssertPoolBeforeCall?.Invoke(_cluster.ConnectionPool);

        Action call = () => Response = _cluster.ClientCall(callTrace?.RequestOverrides);
        var exception = TryCall(call, assert);
        assert(exception);

        AuditTrail = exception.AuditTrail;
        AssertPoolAfterCall?.Invoke(_cluster.ConnectionPool);

        _clusterAsync = _clusterAsync ?? Cluster();
        _clusterAsync.ClientThrows(true);
        Func<Task> callAsync = async () => ResponseAsync = await _clusterAsync.ClientCallAsync(callTrace?.RequestOverrides).ConfigureAwait(false);
        exception = await TryCallAsync(callAsync, assert).ConfigureAwait(false);
        assert(exception);

        AsyncAuditTrail = exception.AuditTrail;
        AssertPoolAfterCall?.Invoke(_clusterAsync.ConnectionPool);
    }

    public async Task<Auditor> TraceOpenSearchException(ClientCall callTrace, Action<OpenSearchClientException> assert)
    {
        await TraceException(callTrace, assert).ConfigureAwait(false);
        var audit = new Auditor(_cluster, _clusterAsync);
        return await audit.TraceOpenSearchExceptionOnResponse(callTrace, assert).ConfigureAwait(false);
    }

    public async Task<Auditor> TraceUnexpectedOpenSearchException(ClientCall callTrace, Action<UnexpectedOpenSearchClientException> assert)
    {
        await TraceException(callTrace, assert).ConfigureAwait(false);
        return new Auditor(_cluster, _clusterAsync);
    }

#pragma warning disable 1998
    public async Task<Auditor> TraceOpenSearchExceptionOnResponse(ClientCall callTrace, Action<OpenSearchClientException> assert)
#pragma warning restore 1998
    {
        _cluster = _cluster ?? Cluster();
        _cluster.ClientThrows(false);
        AssertPoolBeforeCall?.Invoke(_cluster.ConnectionPool);

        Action call = () => { Response = _cluster.ClientCall(callTrace?.RequestOverrides); };
        call();

        if (Response.ApiCall.Success) throw new Exception("Expected call to not be valid");

        var exception = Response.ApiCall.OriginalException as OpenSearchClientException ?? throw new Exception("OriginalException on response is not expected OpenSearchClientException");
        assert(exception);

        AuditTrail = exception.AuditTrail;
        AssertPoolAfterCall?.Invoke(_cluster.ConnectionPool);

        _clusterAsync = _clusterAsync ?? Cluster();
        _clusterAsync.ClientThrows(false);
        Func<Task> callAsync = async () => { ResponseAsync = await _clusterAsync.ClientCallAsync(callTrace?.RequestOverrides).ConfigureAwait(false); };
        await callAsync().ConfigureAwait(false);
        if (Response.ApiCall.Success) throw new Exception("Expected call to not be valid");
        exception = ResponseAsync.ApiCall.OriginalException as OpenSearchClientException;
        if (exception == null) throw new Exception("OriginalException on response is not expected OpenSearchClientException");
        assert(exception);

        AsyncAuditTrail = exception.AuditTrail;
        AssertPoolAfterCall?.Invoke(_clusterAsync.ConnectionPool);
        var audit = new Auditor(_cluster, _clusterAsync);

        return audit;
    }

#pragma warning disable 1998
    public async Task<Auditor> TraceUnexpectedException(ClientCall callTrace, Action<UnexpectedOpenSearchClientException> assert)
#pragma warning restore 1998
    {
        _cluster = _cluster ?? Cluster();
        AssertPoolBeforeCall?.Invoke(_cluster.ConnectionPool);

        Action call = () => Response = _cluster.ClientCall(callTrace?.RequestOverrides);
        var exception = TryCall(call, assert);
        assert(exception);

        AuditTrail = exception.AuditTrail;
        AssertPoolAfterCall?.Invoke(_cluster.ConnectionPool);

        _clusterAsync = _clusterAsync ?? Cluster();
        Func<Task> callAsync = async () => ResponseAsync = await _clusterAsync.ClientCallAsync(callTrace?.RequestOverrides).ConfigureAwait(false);
        exception = await TryCallAsync(callAsync, assert).ConfigureAwait(false);
        assert(exception);

        AsyncAuditTrail = exception.AuditTrail;
        AssertPoolAfterCall?.Invoke(_clusterAsync.ConnectionPool);
        return new Auditor(_cluster, _clusterAsync);
    }

    private Auditor AssertAuditTrails(ClientCall callTrace, int nthCall)
    {
        var nl = Environment.NewLine;
        if (AuditTrail.Count != AsyncAuditTrail.Count)
            throw new Exception($"{nthCall} has a mismatch between sync and async. {nl}async:{AuditTrail}{nl}sync:{AsyncAuditTrail}");

        AssertTrailOnResponse(callTrace, AuditTrail, true, nthCall);
        AssertTrailOnResponse(callTrace, AuditTrail, false, nthCall);

        callTrace.AssertResponse?.Invoke(Response);
        callTrace.AssertResponse?.Invoke(ResponseAsync);

        callTrace?.AssertPoolAfterCall?.Invoke(_cluster.ConnectionPool);
        callTrace?.AssertPoolAfterCall?.Invoke(_clusterAsync.ConnectionPool);
        return new Auditor(_cluster, _clusterAsync);
    }


    public void VisualizeCalls(int numberOfCalls)
    {
        var cluster = _cluster ?? Cluster();
        var messages = new List<string>(numberOfCalls * 2);
        for (var i = 0; i < numberOfCalls; i++)
        {
            var call = cluster.ClientCall();
            var d = call.ApiCall;
            var actualAuditTrail = AuditTrailToString(d.AuditTrail);
            messages.Add($"{d.HttpMethod.GetStringValue()} ({d.Uri.Port}) (status: {d.HttpStatusCode})");
            messages.Add(actualAuditTrail);
        }
        throw new Exception(string.Join(Environment.NewLine, messages));
    }

    private static string AuditTrailToString(List<OpenSearch.Net.Audit> auditTrail)
    {
        var actualAuditTrail = auditTrail.Aggregate(new StringBuilder(),
            (sb, a) => sb.AppendLine($"-> {a}"),
            sb => sb.ToString());
        return actualAuditTrail;
    }

    public async Task<Auditor> TraceCalls(params ClientCall[] audits)
    {
        var auditor = this;
        foreach (var a in audits.Select((a, i) => new { a, i })) auditor = await auditor.TraceCall(a.a, a.i).ConfigureAwait(false);
        return auditor;
    }

    private static void AssertTrailOnResponse(ClientCall callTrace, List<OpenSearch.Net.Audit> auditTrail, bool sync, int nthCall)
    {
        var typeOfTrail = (sync ? "synchronous" : "asynchronous") + " audit trail";
        var nthClientCall = (nthCall + 1).ToOrdinal();

        var actualAuditTrail = auditTrail.Aggregate(new StringBuilder(Environment.NewLine),
            (sb, a) => sb.AppendLine($"-> {a}"),
            sb => sb.ToString());

        var traceEvents = callTrace.Select(c => c.Event).ToList();
        var auditEvents = auditTrail.Select(a => a.Event).ToList();
        if (!traceEvents.SequenceEqual(auditEvents))
            throw new Exception($"the {nthClientCall} client call's {typeOfTrail} should assert ALL audit trail items{actualAuditTrail}");

        foreach (var t in auditTrail.Select((a, i) => new { a, i }))
        {
            var i = t.i;
            var audit = t.a;
            var nthAuditTrailItem = (i + 1).ToOrdinal();
            var because = $"thats the {{0}} specified on the {nthAuditTrailItem} item in the {nthClientCall} client call's {typeOfTrail}";
            var c = callTrace[i];
            if (audit.Event != c.Event)
                throw new Exception(string.Format(because, "event"));
            if (c.Port.HasValue && audit.Node.Uri.Port != c.Port.Value)
                throw new Exception(string.Format(because, "port"));

            c.SimpleAssert?.Invoke(audit);
            c.AssertWithBecause?.Invoke(string.Format(because, "custom assertion"), audit);
        }

        if (callTrace.Count != auditTrail.Count)
            throw new Exception($"callTrace has {callTrace.Count} items. Actual auditTrail {actualAuditTrail}");
    }

    private static TException TryCall<TException>(Action call, Action<TException> assert) where TException : OpenSearchClientException
    {
        TException exception = null;
        try
        {
            call();
        }
        catch (TException ex)
        {
            exception = ex;
            assert(ex);
        }
        if (exception is null) throw new Exception("No exception happened while one was expected");

        return exception;
    }
    private static async Task<TException> TryCallAsync<TException>(Func<Task> call, Action<TException> assert) where TException : OpenSearchClientException
    {
        TException exception = null;
        try
        {
            await call().ConfigureAwait(false);
        }
        catch (TException ex)
        {
            exception = ex;
            assert(ex);
        }
        if (exception is null) throw new Exception("No exception happened while one was expected");

        return exception;
    }

}
