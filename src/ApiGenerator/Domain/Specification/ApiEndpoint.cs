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
using SemanticVersioning;

namespace ApiGenerator.Domain.Specification
{
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

        private IEnumerable<QueryParameters> ParamsToGenerate => Url.Params.Values.Where(p => !p.Skip).OrderBy(p => p.ClsName);

        public RequestInterface RequestInterface => new()
        {
            CsharpNames = CsharpNames,
            UrlParts = Url.Parts,
            PartialParameters =
                Body == null ? Enumerable.Empty<QueryParameters>().ToList() : ParamsToGenerate.Where(p => p.RenderPartial).ToList(),
            OfficialDocumentationLink = OfficialDocumentationLink?.Url
        };

        public RequestPartialImplementation RequestPartialImplementation => new()
        {
            CsharpNames = CsharpNames,
            OfficialDocumentationLink = OfficialDocumentationLink?.Url,
            Stability = Stability,
            Paths = Url.Paths.ToList(),
            Parts = Url.Parts,
            Params = ParamsToGenerate.ToList(),
            Constructors = Constructor.RequestConstructors(CsharpNames, Url, inheritsFromPlainRequestBase: true).ToList(),
            GenericConstructors = Constructor.RequestConstructors(CsharpNames, Url, inheritsFromPlainRequestBase: false).ToList(),
            HasBody = Body != null,
        };

        public DescriptorPartialImplementation DescriptorPartialImplementation => new()
        {
            CsharpNames = CsharpNames,
            OfficialDocumentationLink = OfficialDocumentationLink?.Url,
            Constructors = Constructor.DescriptorConstructors(CsharpNames, Url).ToList(),
            Paths = Url.Paths.ToList(),
            Parts = Url.Parts,
            Params = ParamsToGenerate.ToList(),
            HasBody = Body != null,
        };

        public RequestParameterImplementation RequestParameterImplementation => new()
        {
            CsharpNames = CsharpNames,
            OfficialDocumentationLink = OfficialDocumentationLink?.Url,
            Params = ParamsToGenerate.ToList(),
            HttpMethod = PreferredHttpMethod
        };

        public string PreferredHttpMethod =>
            HttpMethods.OrderByDescending(m => m switch
            {
                "GET" => 0,
                "POST" => 1,
                "PUT" or "DELETE" or "PATCH" or "HEAD" => 2, // Prefer "resource" methods over GET/POST methods
                _ => -1
            }).First();

        public string HighLevelMethodXmlDocDescription =>
            $"<c>{PreferredHttpMethod}</c> request to the <c>{Name}</c> API, read more about this API online:";

        private bool BodyIsOptional => Body is not { Required: true } || HttpMethods.Contains("GET");

        private Deprecation Deprecated =>
            !Url.Paths.Any() && Url.AllPaths.Count > 0
                ? Url.DeprecatedPaths
                    .Select(p => p.Deprecation)
                    .MaxBy(d => d.Version)
                : null;

        private Version VersionAdded =>
            Url.AllPaths
                .Select(p => p.VersionAdded)
                .Where(v => v != null)
                .Min();

        public HighLevelModel HighLevelModel => new()
        {
            CsharpNames = CsharpNames,
            Fluent = new FluentMethod(CsharpNames, Url.Parts,
                selectorIsOptional: BodyIsOptional,
                link: OfficialDocumentationLink?.Url,
                summary: HighLevelMethodXmlDocDescription,
                deprecated: Deprecated,
                versionAdded: VersionAdded
            ),
            FluentBound = !CsharpNames.DescriptorBindsOverMultipleDocuments
                ? null
                : new BoundFluentMethod(CsharpNames, Url.Parts,
                    selectorIsOptional: BodyIsOptional,
                    link: OfficialDocumentationLink?.Url,
                    summary: HighLevelMethodXmlDocDescription,
                    deprecated: Deprecated,
                    versionAdded: VersionAdded
                ),
            Initializer = new InitializerMethod(CsharpNames,
                link: OfficialDocumentationLink?.Url,
                summary: HighLevelMethodXmlDocDescription,
                deprecated: Deprecated,
                versionAdded: VersionAdded
            )
        };

        private List<LowLevelClientMethod> _lowLevelClientMethods;

        public IReadOnlyCollection<LowLevelClientMethod> LowLevelClientMethods
        {
            get
            {
                if (_lowLevelClientMethods != null && _lowLevelClientMethods.Count > 0) return _lowLevelClientMethods;

                // enumerate once and cache
                _lowLevelClientMethods = new List<LowLevelClientMethod>();

                if (OfficialDocumentationLink == null)
                    Generator.ApiGenerator.Warnings.Add($"API '{Name}' has no documentation");

                var httpMethod = PreferredHttpMethod;
                foreach (var path in Url.AllPaths)
                {
                    var methodName = CsharpNames.PerPathMethodName(path.Path);
                    var parts = new List<UrlPart>(path.Parts);
                    var mapsApiArgumentHints = parts.Select(p => p.Name).ToList();
                    // TODO This is hack until we stop transforming the new spec format into the old
                    if (Name == "index" && !mapsApiArgumentHints.Contains("id"))
                        httpMethod = "POST";
                    else if (Name == "index") httpMethod = PreferredHttpMethod;

                    if (Body != null)
                    {
                        parts.Add(new UrlPart { Name = "body", Type = "PostData", Description = Body.Description });
                        mapsApiArgumentHints.Add("body");
                    }

                    var args = parts
                        .Select(p => p.Argument)
                        .Concat(new[] { CsharpNames.ParametersName + " requestParameters = null" })
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
                        Deprecation = path.Deprecation,
                        Path = path.Path,
                        Parts = parts,
                        Url = Url,
                        HasBody = Body != null,
                        VersionAdded = path.VersionAdded,
                    };
                    _lowLevelClientMethods.Add(apiMethod);
                }
                return _lowLevelClientMethods;
            }
        }
    }
}
