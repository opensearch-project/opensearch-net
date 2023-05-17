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
*   http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System.Collections.Generic;
using System.Linq;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Domain.Code;
using ApiGenerator.Domain.Code.HighLevel.Methods;
using ApiGenerator.Domain.Code.HighLevel.Requests;
using ApiGenerator.Domain.Code.LowLevel;

namespace ApiGenerator.Domain.Specification;

public class ApiEndpoint
{
    /// <summary> The original name as declared in the spec </summary>
    public string Name { get; set; }

    /// <summary> The original namespace as declared in the spec </summary>
    public string Namespace { get; set; }

    /// <summary> The original method name as declared in the spec </summary>
    public string MethodName { get; set; }

    /// <summary> Computed Csharp identifier names </summary>
    public CsharpNames CsharpNames { get; set; }

    public Stability Stability { get; set; }

    public Documentation OfficialDocumentationLink { get; set; }

    public UrlInformation Url { get; set; }

    public Body Body { get; set; }

    public IReadOnlyCollection<string> HttpMethods { get; set; }

    public IEndpointOverrides Overrides { get; internal set; }

    public RequestInterface RequestInterface => new()
    {
        CsharpNames = CsharpNames,
        UrlParts = Url.Parts,
        PartialParameters =
            Body == null ? Enumerable.Empty<QueryParameters>().ToList() : Url.Params.Values.Where(p => p.RenderPartial && !p.Skip).ToList(),
        OfficialDocumentationLink = OfficialDocumentationLink?.Url
    };

    public RequestPartialImplementation RequestPartialImplementation => new()
    {
        CsharpNames = CsharpNames,
        OfficialDocumentationLink = OfficialDocumentationLink?.Url,
        Stability = Stability,
        Paths = Url.Paths,
        Parts = Url.Parts,
        Params = Url.Params.Values.Where(p => !p.Skip).ToList(),
        Constructors = Constructor.RequestConstructors(CsharpNames, Url, true).ToList(),
        GenericConstructors = Constructor.RequestConstructors(CsharpNames, Url, false).ToList(),
        HasBody = Body != null
    };

    public DescriptorPartialImplementation DescriptorPartialImplementation => new()
    {
        CsharpNames = CsharpNames,
        OfficialDocumentationLink = OfficialDocumentationLink?.Url,
        Constructors = Constructor.DescriptorConstructors(CsharpNames, Url).ToList(),
        Paths = Url.Paths,
        Parts = Url.Parts,
        Params = Url.Params.Values.Where(p => !p.Skip).ToList(),
        HasBody = Body != null
    };

    public RequestParameterImplementation RequestParameterImplementation => new()
    {
        CsharpNames = CsharpNames,
        OfficialDocumentationLink = OfficialDocumentationLink?.Url,
        Params = Url.Params.Values.Where(p => !p.Skip).ToList(),
        HttpMethod = PreferredHttpMethod
    };

    public string PreferredHttpMethod
    {
        get
        {
            var first = HttpMethods.First();
            if (HttpMethods.Count > 1 && first.ToUpperInvariant() == "GET")
                return HttpMethods.Last();

            return first;
        }
    }

    public string HighLevelMethodXmlDocDescription =>
        $"<c>{PreferredHttpMethod}</c> request to the <c>{Name}</c> API, read more about this API online:";

    public HighLevelModel HighLevelModel => new()
    {
        CsharpNames = CsharpNames,
        Fluent = new FluentMethod(CsharpNames, Url.Parts,
            Body is not { Required: true } || HttpMethods.Contains("GET"),
            OfficialDocumentationLink?.Url,
            HighLevelMethodXmlDocDescription
        ),
        FluentBound = !CsharpNames.DescriptorBindsOverMultipleDocuments
            ? null
            : new BoundFluentMethod(CsharpNames, Url.Parts,
                Body is not { Required: true } || HttpMethods.Contains("GET"),
                OfficialDocumentationLink?.Url,
                HighLevelMethodXmlDocDescription
            ),
        Initializer = new InitializerMethod(CsharpNames,
            OfficialDocumentationLink?.Url,
            HighLevelMethodXmlDocDescription
        )
    };

    private List<LowLevelClientMethod> _lowLevelClientMethods;

    public IReadOnlyCollection<LowLevelClientMethod> LowLevelClientMethods
    {
        get
        {
            if (_lowLevelClientMethods is { Count: > 0 }) return _lowLevelClientMethods;

            // enumerate once and cache
            _lowLevelClientMethods = new List<LowLevelClientMethod>();

            if (OfficialDocumentationLink == null)
                Generator.ApiGenerator.Warnings.Add($"API '{Name}' has no documentation");

            var httpMethod = PreferredHttpMethod;
            foreach (var path in Url.PathsWithDeprecations)
            {
                var methodName = CsharpNames.PerPathMethodName(path.Path);
                var parts = new List<UrlPart>(path.Parts);
                var mapsApiArgumentHints = parts.Select(p => p.Name).ToList();
                httpMethod = Name switch
                {
                    // TODO This is hack until we stop transforming the new spec format into the old
                    "index" when !mapsApiArgumentHints.Contains("id") => "POST",
                    "index" => PreferredHttpMethod,
                    _ => httpMethod
                };

                if (Body != null)
                {
                    parts.Add(new UrlPart { Name = "body", Type = "PostData", Description = Body.Description });
                    mapsApiArgumentHints.Add("body");
                }

                var args = parts
                    .Select(p => p.Argument)
                    .Concat(new[] { $"{CsharpNames.ParametersName} requestParameters = null" })
                    .ToList();

                var apiMethod = new LowLevelClientMethod
                {
                    Arguments = string.Join(", ", args),
                    MapsApiArguments = string.Join(", ", mapsApiArgumentHints),
                    CsharpNames = CsharpNames,
                    PerPathMethodName = methodName,
                    HttpMethod = httpMethod,
                    OfficialDocumentationLink = OfficialDocumentationLink?.Url,
                    Stability = Stability,
                    DeprecatedPath = path.Deprecation,
                    Path = path.Path,
                    Parts = parts,
                    Url = Url,
                    HasBody = Body != null
                };
                _lowLevelClientMethods.Add(apiMethod);
            }
            return _lowLevelClientMethods;
        }
    }
}
