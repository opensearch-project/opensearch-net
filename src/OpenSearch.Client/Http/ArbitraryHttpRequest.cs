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

public abstract class ArbitraryHttpRequest<TParams>
	: PlainRequestBase<TParams>, IArbitraryHttpRequest<TParams>
	where TParams : ArbitraryHttpRequestParameters<TParams>, new()
{
	private string _path;

	protected ArbitraryHttpRequest() { }

	protected ArbitraryHttpRequest(string path) => Path = path;

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

public abstract class ArbitraryBodyHttpRequest<TParams>
	: ArbitraryHttpRequest<TParams>, IArbitraryBodyHttpRequest<TParams>
	where TParams : ArbitraryHttpRequestParameters<TParams>, new()
{
	protected ArbitraryBodyHttpRequest() { }

	protected ArbitraryBodyHttpRequest(string path) : base(path) { }

	public PostData Body { get; set; }
}

public abstract class ArbitraryHttpRequestDescriptor<TSelf, TParams, TInterface>
	: RequestDescriptorBase<TSelf, TParams, TInterface>, IArbitraryHttpRequest<TParams>
	where TSelf : ArbitraryHttpRequestDescriptor<TSelf, TParams, TInterface>, TInterface
	where TParams : ArbitraryHttpRequestParameters<TParams>, new()
	where TInterface : IArbitraryHttpRequest<TParams>
{
	private string _path;

	protected ArbitraryHttpRequestDescriptor(string path) => Path = path;

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

public abstract class ArbitraryBodyHttpRequestDescriptor<TSelf, TParams, TInterface>
	: ArbitraryHttpRequestDescriptor<TSelf, TParams, TInterface>, IArbitraryBodyHttpRequest<TParams>
	where TSelf : ArbitraryBodyHttpRequestDescriptor<TSelf, TParams, TInterface>, TInterface
	where TParams : ArbitraryHttpRequestParameters<TParams>, new()
	where TInterface : IArbitraryBodyHttpRequest<TParams>
{
	protected ArbitraryBodyHttpRequestDescriptor(string path) : base(path) { }

	PostData IArbitraryBodyHttpRequest<TParams>.Body { get; set; }

	public TSelf Body(PostData body) => Assign(body, (a, v) => a.Body = v);

	public TSelf Body(byte[] bytes) => Body(PostData.Bytes(bytes));

	public TSelf Body(string body) => Body(PostData.String(body));

#if NETSTANDARD2_1
	public TSelf Body(ReadOnlyMemory<byte> bytes) => Body(PostData.ReadOnlyMemory(bytes));
#endif

	public TSelf MultiJsonBody(IEnumerable<string> items) => Body(PostData.MultiJson(items));

	public TSelf MultiJsonBody(IEnumerable<object> items) => Body(PostData.MultiJson(items));

	public TSelf StreamableBody<T>(T state, Action<T, Stream> syncWriter, Func<T, Stream, CancellationToken, Task> asyncWriter) =>
		Body(PostData.StreamHandler(state, syncWriter, asyncWriter));

	public TSelf SerializableBody<T>(T o) => Body(PostData.Serializable(o));
}
