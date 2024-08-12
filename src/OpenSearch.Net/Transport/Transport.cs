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
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.Extensions;

namespace OpenSearch.Net;

public class Transport<TConnectionSettings> : ITransport<TConnectionSettings>
    where TConnectionSettings : class, IConnectionConfigurationValues
{
    /// <summary>
    /// Transport coordinates the client requests over the connection pool nodes and is in charge of falling over on different nodes
    /// </summary>
    /// <param name="configurationValues">The connection settings to use for this transport</param>
    public Transport(TConnectionSettings configurationValues) : this(configurationValues, null, null, null) { }

    /// <summary>
    /// Transport coordinates the client requests over the connection pool nodes and is in charge of falling over on different nodes
    /// </summary>
    /// <param name="configurationValues">The connection settings to use for this transport</param>
    /// <param name="pipelineProvider">In charge of create a new pipeline, safe to pass null to use the default</param>
    /// <param name="dateTimeProvider">The date time proved to use, safe to pass null to use the default</param>
    /// <param name="memoryStreamFactory">The memory stream provider to use, safe to pass null to use the default</param>
    public Transport(
        TConnectionSettings configurationValues,
        IRequestPipelineFactory pipelineProvider,
        IDateTimeProvider dateTimeProvider,
        IMemoryStreamFactory memoryStreamFactory
    )
    {
        configurationValues.ThrowIfNull(nameof(configurationValues));
        configurationValues.ConnectionPool.ThrowIfNull(nameof(configurationValues.ConnectionPool));
        configurationValues.Connection.ThrowIfNull(nameof(configurationValues.Connection));
        configurationValues.RequestResponseSerializer.ThrowIfNull(nameof(configurationValues.RequestResponseSerializer));

        Settings = configurationValues;
        PipelineProvider = pipelineProvider ?? new RequestPipelineFactory();
        DateTimeProvider = dateTimeProvider ?? Net.DateTimeProvider.Default;
        MemoryStreamFactory = memoryStreamFactory ?? configurationValues.MemoryStreamFactory;
    }

    public TConnectionSettings Settings { get; }

    private IDateTimeProvider DateTimeProvider { get; }
    private IMemoryStreamFactory MemoryStreamFactory { get; }
    private IRequestPipelineFactory PipelineProvider { get; }

    public TResponse Request<TResponse>(HttpMethod method, string path, PostData data = null, IRequestParameters requestParameters = null)
        where TResponse : class, IOpenSearchResponse, new()
    {
        using (var pipeline = PipelineProvider.Create(Settings, DateTimeProvider, MemoryStreamFactory, requestParameters))
        {
            pipeline.FirstPoolUsage(Settings.BootstrapLock);

            var requestData = new RequestData(method, path, data, Settings, requestParameters, MemoryStreamFactory);
            Settings.OnRequestDataCreated?.Invoke(requestData);
            TResponse response = null;

            var seenExceptions = new List<PipelineException>();
            foreach (var node in pipeline.NextNode())
            {
                requestData.Node = node;
                try
                {
                    pipeline.SniffOnStaleCluster();
                    Ping(pipeline, node);
                    response = pipeline.CallOpenSearch<TResponse>(requestData);
                    if (!response.ApiCall.SuccessOrKnownError)
                    {
                        pipeline.MarkDead(node);
                        pipeline.SniffOnConnectionFailure();
                    }
                }
                catch (PipelineException pipelineException) when (!pipelineException.Recoverable)
                {
                    HandlePipelineException(ref response, pipelineException, pipeline, node, seenExceptions);
                    break;
                }
                catch (PipelineException pipelineException)
                {
                    HandlePipelineException(ref response, pipelineException, pipeline, node, seenExceptions);
                }
                catch (Exception killerException)
                {
                    throw new UnexpectedOpenSearchClientException(killerException, seenExceptions)
                    {
                        Request = requestData,
                        Response = response?.ApiCall,
                        AuditTrail = pipeline.AuditTrail
                    };
                }
                if (response == null || !response.ApiCall.SuccessOrKnownError) continue;

                pipeline.MarkAlive(node);
                break;
            }
            return FinalizeResponse(requestData, pipeline, seenExceptions, response);
        }
    }

    public async Task<TResponse> RequestAsync<TResponse>(HttpMethod method, string path, CancellationToken cancellationToken,
        PostData data = null, IRequestParameters requestParameters = null
    )
        where TResponse : class, IOpenSearchResponse, new()
    {
        using (var pipeline = PipelineProvider.Create(Settings, DateTimeProvider, MemoryStreamFactory, requestParameters))
        {
            await pipeline.FirstPoolUsageAsync(Settings.BootstrapLock, cancellationToken).ConfigureAwait(false);

            var requestData = new RequestData(method, path, data, Settings, requestParameters, MemoryStreamFactory);
            Settings.OnRequestDataCreated?.Invoke(requestData);
            TResponse response = null;

            var seenExceptions = new List<PipelineException>();
            foreach (var node in pipeline.NextNode())
            {
                requestData.Node = node;
                try
                {
                    await pipeline.SniffOnStaleClusterAsync(cancellationToken).ConfigureAwait(false);
                    await PingAsync(pipeline, node, cancellationToken).ConfigureAwait(false);
                    response = await pipeline.CallOpenSearchAsync<TResponse>(requestData, cancellationToken).ConfigureAwait(false);
                    if (!response.ApiCall.SuccessOrKnownError)
                    {
                        pipeline.MarkDead(node);
                        await pipeline.SniffOnConnectionFailureAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (PipelineException pipelineException) when (!pipelineException.Recoverable)
                {
                    HandlePipelineException(ref response, pipelineException, pipeline, node, seenExceptions);
                    break;
                }
                catch (PipelineException pipelineException)
                {
                    HandlePipelineException(ref response, pipelineException, pipeline, node, seenExceptions);
                }
                catch (Exception killerException)
                {
                    if (killerException is OperationCanceledException && cancellationToken.IsCancellationRequested)
                        pipeline.AuditCancellationRequested();

                    throw new UnexpectedOpenSearchClientException(killerException, seenExceptions)
                    {
                        Request = requestData,
                        Response = response?.ApiCall,
                        AuditTrail = pipeline.AuditTrail
                    };
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    pipeline.AuditCancellationRequested();
                    break;
                }
                if (response == null || !response.ApiCall.SuccessOrKnownError) continue;

                pipeline.MarkAlive(node);
                break;
            }
            return FinalizeResponse(requestData, pipeline, seenExceptions, response);
        }
    }

    private static void HandlePipelineException<TResponse>(
        ref TResponse response, PipelineException ex, IRequestPipeline pipeline, Node node, List<PipelineException> seenExceptions
    )
        where TResponse : class, IOpenSearchResponse, new()
    {
        if (response == null) response = ex.Response as TResponse;
        pipeline.MarkDead(node);
        seenExceptions.Add(ex);
    }

    private TResponse FinalizeResponse<TResponse>(RequestData requestData, IRequestPipeline pipeline, List<PipelineException> seenExceptions,
        TResponse response
    ) where TResponse : class, IOpenSearchResponse, new()
    {
        if (requestData.Node == null) //foreach never ran
            pipeline.ThrowNoNodesAttempted(requestData, seenExceptions);

        var callDetails = GetMostRecentCallDetails(response, seenExceptions);
        var clientException = pipeline.CreateClientException(response, callDetails, requestData, seenExceptions);

        if (response?.ApiCall == null)
            pipeline.BadResponse(ref response, callDetails, requestData, clientException);

        HandleOpenSearchClientException(requestData, clientException, response);
        return response;
    }

    private static IApiCallDetails GetMostRecentCallDetails<TResponse>(TResponse response, IEnumerable<PipelineException> seenExceptions)
        where TResponse : class, IOpenSearchResponse, new()
    {
        var callDetails = response?.ApiCall ?? seenExceptions.LastOrDefault(e => e.ApiCall != null)?.ApiCall;
        return callDetails;
    }


    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private void HandleOpenSearchClientException(RequestData data, Exception clientException, IOpenSearchResponse response)
    {
        if (response.ApiCall is ApiCallDetails a)
        {
            //if original exception was not explicitly set during the pipeline
            //set it to the OpenSearchClientException we created for the bad response
            if (clientException != null && a.OriginalException == null)
                a.OriginalException = clientException;
        }

        Settings.OnRequestCompleted?.Invoke(response.ApiCall);
        if (data != null && (clientException != null && data.ThrowExceptions)) throw clientException;
    }

    private static void Ping(IRequestPipeline pipeline, Node node)
    {
        try
        {
            pipeline.Ping(node);
        }
        catch (PipelineException e) when (e.Recoverable)
        {
            pipeline.SniffOnConnectionFailure();
            throw;
        }
    }

    private static async Task PingAsync(IRequestPipeline pipeline, Node node, CancellationToken cancellationToken)
    {
        try
        {
            await pipeline.PingAsync(node, cancellationToken).ConfigureAwait(false);
        }
        catch (PipelineException e) when (e.Recoverable)
        {
            await pipeline.SniffOnConnectionFailureAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
