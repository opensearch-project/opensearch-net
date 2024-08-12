/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;
using OpenSearch.Net.Specification.HttpApi;

namespace OpenSearch.Client;

public interface IArbitraryHttpRequest<out T> : IRequest<T>
    where T : ArbitraryHttpRequestParameters<T>, new()
{
    [IgnoreDataMember]
    string Path { get; set; }
}

public interface IArbitraryBodyHttpRequest<out T> : IArbitraryHttpRequest<T>
    where T : ArbitraryHttpRequestParameters<T>, new()
{
    [IgnoreDataMember]
    PostData Body { get; set; }
}

public abstract class ArbitraryHttpRequestBase<TParams>
    : PlainRequestBase<TParams>, IArbitraryHttpRequest<TParams>
    where TParams : ArbitraryHttpRequestParameters<TParams>, new()
{
    private string _path;

    protected ArbitraryHttpRequestBase() { }

    protected ArbitraryHttpRequestBase(string path) => Path = path;

    public string Path
    {
        get => _path;
        set
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Path), "Path cannot be null or empty");
            _path = value;
        }
    }

    public Dictionary<string, object> QueryString
    {
        get => RequestState.RequestParameters.QueryString;
        set => RequestState.RequestParameters.QueryString = value;
    }

    internal override ApiUrls ApiUrls => throw new NotImplementedException();

    protected override string ResolveUrl(RouteValues routeValues, IConnectionSettingsValues settings) => throw new NotImplementedException();

    string IRequest.GetUrl(IConnectionSettingsValues settings) => Path;
}

public abstract class ArbitraryBodyHttpRequestBase<TParams>
    : ArbitraryHttpRequestBase<TParams>, IArbitraryBodyHttpRequest<TParams>
    where TParams : ArbitraryHttpRequestParameters<TParams>, new()
{
    protected ArbitraryBodyHttpRequestBase() { }

    protected ArbitraryBodyHttpRequestBase(string path) : base(path) { }

    public PostData Body { get; set; }
}

public abstract class ArbitraryHttpRequestDescriptorBase<TSelf, TParams, TInterface>
    : RequestDescriptorBase<TSelf, TParams, TInterface>, IArbitraryHttpRequest<TParams>
    where TSelf : ArbitraryHttpRequestDescriptorBase<TSelf, TParams, TInterface>, TInterface
    where TParams : ArbitraryHttpRequestParameters<TParams>, new()
    where TInterface : IArbitraryHttpRequest<TParams>
{
    private string _path;

    protected ArbitraryHttpRequestDescriptorBase(string path) => Path = path;

    public string Path
    {
        get => _path;
        set
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Path), "Path cannot be null or empty");
            _path = value;
        }
    }

    public TSelf QueryString(Dictionary<string, object> queryString) =>
        Assign(queryString, (a, v) => a.RequestParameters.QueryString = v);

    public TSelf QueryString(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> selector) =>
        Assign(selector, (a, v) => a.RequestParameters.QueryString = v?.Invoke(new FluentDictionary<string, object>()));

    internal override ApiUrls ApiUrls => throw new NotImplementedException();

    protected override string ResolveUrl(RouteValues routeValues, IConnectionSettingsValues settings) => throw new NotImplementedException();

    string IRequest.GetUrl(IConnectionSettingsValues settings) => Path;
}

public abstract class ArbitraryBodyHttpRequestDescriptorBase<TSelf, TParams, TInterface>
    : ArbitraryHttpRequestDescriptorBase<TSelf, TParams, TInterface>, IArbitraryBodyHttpRequest<TParams>
    where TSelf : ArbitraryBodyHttpRequestDescriptorBase<TSelf, TParams, TInterface>, TInterface
    where TParams : ArbitraryHttpRequestParameters<TParams>, new()
    where TInterface : IArbitraryBodyHttpRequest<TParams>
{
    protected ArbitraryBodyHttpRequestDescriptorBase(string path) : base(path) { }

    PostData IArbitraryBodyHttpRequest<TParams>.Body { get; set; }

    public TSelf Body(PostData body) => Assign(body, (a, v) => a.Body = v);

    private TSelf Body<T>(T data, Func<T, PostData> factory) where T : class => Body(data != null ? factory(data) : null);

    private TSelf Body<T>(T? data, Func<T, PostData> factory) where T : struct => Body(data.HasValue ? factory(data.Value) : null);

    public TSelf Body(byte[] bytes) => Body(bytes, PostData.Bytes);

    public TSelf Body(string body) => Body(body, PostData.String);

#if NETSTANDARD2_1
	public TSelf Body(ReadOnlyMemory<byte>? bytes) => Body(bytes, PostData.ReadOnlyMemory);
#endif

    public TSelf MultiJsonBody(IEnumerable<string> items) => Body(items, PostData.MultiJson);

    public TSelf MultiJsonBody(IEnumerable<object> items) => Body(items, PostData.MultiJson);

    public TSelf StreamableBody<T>(T state, Action<T, Stream> syncWriter, Func<T, Stream, CancellationToken, Task> asyncWriter) =>
        Body(PostData.StreamHandler(state, syncWriter, asyncWriter));

    public TSelf SerializableBody<T>(T o) => Body(PostData.Serializable(o));
}
