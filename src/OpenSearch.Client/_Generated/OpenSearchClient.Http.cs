/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
// ███╗   ██╗ ██████╗ ████████╗██╗ ██████╗███████╗
// ████╗  ██║██╔═══██╗╚══██╔══╝██║██╔════╝██╔════╝
// ██╔██╗ ██║██║   ██║   ██║   ██║██║     █████╗
// ██║╚██╗██║██║   ██║   ██║   ██║██║     ██╔══╝
// ██║ ╚████║╚██████╔╝   ██║   ██║╚██████╗███████╗
// ╚═╝  ╚═══╝ ╚═════╝    ╚═╝   ╚═╝ ╚═════╝╚══════╝
// -----------------------------------------------
//
// This file is automatically generated
// Please do not edit these files manually
// Run the following in the root of the repos:
//
//      *NIX        :   ./build.sh codegen
//      Windows     :   build.bat codegen
//
// -----------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace OpenSearch.Client.Specification.HttpApi;

/// <summary>
/// Http APIs.
/// <para>Not intended to be instantiated directly. Use the <see cref="IOpenSearchClient.Http"/> property
/// on <see cref="IOpenSearchClient"/>.
/// </para>
/// </summary>
public class HttpNamespace : NamespacedClientProxy
{
    internal HttpNamespace(OpenSearchClient client)
        : base(client) { }

    public TResponse Delete<TResponse>(
        string path,
        Func<HttpDeleteDescriptor, IHttpDeleteRequest> selector = null
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        Delete<TResponse>(selector.InvokeOrDefault(new HttpDeleteDescriptor(path)));

    public Task<TResponse> DeleteAsync<TResponse>(
        string path,
        Func<HttpDeleteDescriptor, IHttpDeleteRequest> selector = null,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        DeleteAsync<TResponse>(selector.InvokeOrDefault(new HttpDeleteDescriptor(path)), ct);

    public TResponse Delete<TResponse>(IHttpDeleteRequest request)
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequest<IHttpDeleteRequest, TResponse>(request, request.RequestParameters, _ => null);

    public Task<TResponse> DeleteAsync<TResponse>(
        IHttpDeleteRequest request,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequestAsync<IHttpDeleteRequest, TResponse>(
            request,
            request.RequestParameters,
            _ => null,
            ct
        );

    public TResponse Get<TResponse>(
        string path,
        Func<HttpGetDescriptor, IHttpGetRequest> selector = null
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        Get<TResponse>(selector.InvokeOrDefault(new HttpGetDescriptor(path)));

    public Task<TResponse> GetAsync<TResponse>(
        string path,
        Func<HttpGetDescriptor, IHttpGetRequest> selector = null,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        GetAsync<TResponse>(selector.InvokeOrDefault(new HttpGetDescriptor(path)), ct);

    public TResponse Get<TResponse>(IHttpGetRequest request)
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequest<IHttpGetRequest, TResponse>(request, request.RequestParameters, _ => null);

    public Task<TResponse> GetAsync<TResponse>(
        IHttpGetRequest request,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequestAsync<IHttpGetRequest, TResponse>(
            request,
            request.RequestParameters,
            _ => null,
            ct
        );

    public TResponse Head<TResponse>(
        string path,
        Func<HttpHeadDescriptor, IHttpHeadRequest> selector = null
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        Head<TResponse>(selector.InvokeOrDefault(new HttpHeadDescriptor(path)));

    public Task<TResponse> HeadAsync<TResponse>(
        string path,
        Func<HttpHeadDescriptor, IHttpHeadRequest> selector = null,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        HeadAsync<TResponse>(selector.InvokeOrDefault(new HttpHeadDescriptor(path)), ct);

    public TResponse Head<TResponse>(IHttpHeadRequest request)
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequest<IHttpHeadRequest, TResponse>(request, request.RequestParameters, _ => null);

    public Task<TResponse> HeadAsync<TResponse>(
        IHttpHeadRequest request,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequestAsync<IHttpHeadRequest, TResponse>(
            request,
            request.RequestParameters,
            _ => null,
            ct
        );

    public TResponse Patch<TResponse>(
        string path,
        Func<HttpPatchDescriptor, IHttpPatchRequest> selector = null
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        Patch<TResponse>(selector.InvokeOrDefault(new HttpPatchDescriptor(path)));

    public Task<TResponse> PatchAsync<TResponse>(
        string path,
        Func<HttpPatchDescriptor, IHttpPatchRequest> selector = null,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        PatchAsync<TResponse>(selector.InvokeOrDefault(new HttpPatchDescriptor(path)), ct);

    public TResponse Patch<TResponse>(IHttpPatchRequest request)
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequest<IHttpPatchRequest, TResponse>(request, request.RequestParameters, r => r.Body);

    public Task<TResponse> PatchAsync<TResponse>(
        IHttpPatchRequest request,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequestAsync<IHttpPatchRequest, TResponse>(
            request,
            request.RequestParameters,
            r => r.Body,
            ct
        );

    public TResponse Post<TResponse>(
        string path,
        Func<HttpPostDescriptor, IHttpPostRequest> selector = null
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        Post<TResponse>(selector.InvokeOrDefault(new HttpPostDescriptor(path)));

    public Task<TResponse> PostAsync<TResponse>(
        string path,
        Func<HttpPostDescriptor, IHttpPostRequest> selector = null,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        PostAsync<TResponse>(selector.InvokeOrDefault(new HttpPostDescriptor(path)), ct);

    public TResponse Post<TResponse>(IHttpPostRequest request)
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequest<IHttpPostRequest, TResponse>(request, request.RequestParameters, r => r.Body);

    public Task<TResponse> PostAsync<TResponse>(
        IHttpPostRequest request,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequestAsync<IHttpPostRequest, TResponse>(
            request,
            request.RequestParameters,
            r => r.Body,
            ct
        );

    public TResponse Put<TResponse>(
        string path,
        Func<HttpPutDescriptor, IHttpPutRequest> selector = null
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        Put<TResponse>(selector.InvokeOrDefault(new HttpPutDescriptor(path)));

    public Task<TResponse> PutAsync<TResponse>(
        string path,
        Func<HttpPutDescriptor, IHttpPutRequest> selector = null,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        PutAsync<TResponse>(selector.InvokeOrDefault(new HttpPutDescriptor(path)), ct);

    public TResponse Put<TResponse>(IHttpPutRequest request)
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequest<IHttpPutRequest, TResponse>(request, request.RequestParameters, r => r.Body);

    public Task<TResponse> PutAsync<TResponse>(
        IHttpPutRequest request,
        CancellationToken ct = default
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        DoRequestAsync<IHttpPutRequest, TResponse>(
            request,
            request.RequestParameters,
            r => r.Body,
            ct
        );
}
