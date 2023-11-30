/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

namespace ApiGenerator.Domain.Code;

public class HttpMethod
{
	public static readonly HttpMethod Delete = new("Delete", false);
	public static readonly HttpMethod Get = new("Get", false);
	public static readonly HttpMethod Head = new("Head", false);
	public static readonly HttpMethod Patch = new("Patch", true);
	public static readonly HttpMethod Post = new("Post", true);
	public static readonly HttpMethod Put = new("Put", true);
	public static readonly HttpMethod[] All = { Delete, Get, Head, Patch, Post, Put };

	private readonly string _method;

	private HttpMethod(string method, bool takesBody)
	{
		_method = method;
		TakesBody = takesBody;
	}

	public bool TakesBody { get; private set; }

	// ReSharper disable once InconsistentNaming
	public string IRequest => $"I{Request}";

	public string Request => $"Http{_method}Request";

	public string RequestParameters => $"Http{_method}RequestParameters";

	public string Descriptor => $"Http{_method}Descriptor";

	public string MethodEnum => $"HttpMethod.{_method.ToUpperInvariant()}";

	public override string ToString() => _method;
}
