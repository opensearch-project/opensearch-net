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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApiGenerator.Configuration;

public static class CodeConfiguration
{
    /// <summary>
    /// Map API default names for API's we are only supporting on the low level client first
    /// </summary>
    private static readonly Dictionary<string, string> LowLevelApiNameMapping = new()
    {
        { "indices.delete_index_template", "DeleteIndexTemplateV2" },
        { "indices.get_index_template", "GetIndexTemplateV2" },
        { "indices.put_index_template", "PutIndexTemplateV2" }
    };

    private static readonly FileInfo[] ClientCsharpFiles = new DirectoryInfo(GeneratorLocations.OpenSearchClientFolder)
        .GetFiles("*.cs", SearchOption.AllDirectories);
    private static readonly FileInfo[] ClientRequestCsharpFiles = ClientCsharpFiles.Where(f => f.Name.EndsWith("Request.cs")).ToArray();
    private static readonly FileInfo[] ClientResponseCsharpFiles = ClientCsharpFiles.Where(f => f.Name.EndsWith("Response.cs")).ToArray();

    private static readonly Regex ResponseBuilderAttributeRegex = new(@"^.+\[ResponseBuilderWithGeneric\(""([^ \r\n]+)""\)\].*$", RegexOptions.Singleline);
    private static readonly Regex GenericsRegex = new(@"^.*?(?:(\<.+>).*?)?$");
    private static readonly Regex GenericsRemovalRegex = new(@"<.*$");
    private static readonly Regex RequestInterfaceRegex = new(@"^.+interface ([^ \r\n]+Request(?:<[^>\r\n]+>)?[^ \r\n]*).*$", RegexOptions.Singleline);
    private static readonly Regex ResponseClassRegex = new(@"^.+public class ([^ \r\n]+Response(?:<[^>\r\n]+>)?[^ \r\n]*).*$", RegexOptions.Singleline);

    /// <summary>
    /// Scan all OSC source code files for Requests and look for the [MapsApi(filename)] attribute.
    /// The class name minus Request is used as the canonical .NET name for the API.
    /// </summary>
    public static readonly Dictionary<string, string> HighLevelApiNameMapping =
        ClientCsharpFiles.Select(f => new
            {
                File = f,
                MappedApi = Regex.Replace(
                    File.ReadAllText(f.FullName),
                    @"^.+\[MapsApi\(""([^ \r\n]+)""\)\].*$",
                    "$1",
                    RegexOptions.Singleline
                )
            })
            .Where(f => !f.MappedApi.Contains(' '))
            .Select(f => new
            {
                MappedApi = f.MappedApi.Replace(".json", ""),
                ApiName = f.File.Name.Replace("Request", "").Replace(".cs", "")
            })
            .DistinctBy(v => v.MappedApi)
            .ToDictionary(k => k.MappedApi, v => v.ApiName);

    public static readonly Dictionary<string, string> ApiNameMapping =
        new Dictionary<string, string>(HighLevelApiNameMapping).OverrideWith(LowLevelApiNameMapping);

    /// <summary>
    /// Scan all OSC source code files for Requests and look for the [MapsApi(filename)] attribute.
    /// The class name minus Request is used as the canonical .NET name for the API.
    /// </summary>
    public static readonly Dictionary<string, string> ResponseBuilderInClientCalls =
        ClientCsharpFiles
            .SelectMany(f => File.ReadLines(f.FullName)
                .Where(l => ResponseBuilderAttributeRegex.IsMatch(l))
                .Select(l => new
                {
                    Key = f.Name.Replace(".cs", ""),
                    Value = ResponseBuilderAttributeRegex.Replace(l, "$1")
                }))
            .DistinctBy(v => v.Key)
            .ToDictionary(v => v.Key, v => v.Value);


    public static readonly IDictionary<string, string> DescriptorGenericsLookup =
        ClientRequestCsharpFiles
            .Select(f =>
            {
                var descriptor = Path.GetFileNameWithoutExtension(f.Name).Replace("Request", "Descriptor");
                var c = Regex.Replace(
                    File.ReadAllText(f.FullName),
                    $@"^.+class ({descriptor}(?:<[^>\r\n]+>)?[^ \r\n]*).*$",
                    "$1",
                    RegexOptions.Singleline
                );
                return new { Key = descriptor, Value = GenericsRegex.Replace(c, "$1") };
            })
            .DistinctBy(v => v.Key)
            .ToImmutableSortedDictionary(v => v.Key, v => v.Value);


    private static readonly List<string> RequestInterfaceDeclarations =
        ClientRequestCsharpFiles
            .SelectMany(f => File.ReadLines(f.FullName))
            .Where(l => RequestInterfaceRegex.IsMatch(l))
            .ToList();

    /// <summary> Scan all OSC files for request interfaces and note any generics declared on them </summary>
    private static readonly List<(string Request, string Generics)> AllKnownRequestInterfaces =
        RequestInterfaceDeclarations
            .Select(l => RequestInterfaceRegex.Replace(l, "$1"))
            .Where(c => c.StartsWith("I") && c.Contains("Request"))
            .Select(c => (
                Request: GenericsRemovalRegex.Replace(c, ""),
                Generics: GenericsRegex.Replace(c, "$1")
            ))
            .OrderBy(v => v.Request)
            .ToList();

    public static readonly HashSet<string> GenericOnlyInterfaces = AllKnownRequestInterfaces
        .GroupBy(v => v.Item1)
        .Where(g => g.All(v => !string.IsNullOrEmpty(v.Item2)))
        .Select(g => g.Key)
        .ToHashSet();

    public static readonly HashSet<string> DocumentRequests =
        RequestInterfaceDeclarations
            .Where(l => l.Contains("IDocumentRequest"))
            .Select(l => RequestInterfaceRegex.Replace(l, "$1"))
            .Select(c => GenericsRemovalRegex.Replace(c, ""))
            .ToHashSet();

    public static readonly Dictionary<string, string> DescriptorConstructors =
        ClientRequestCsharpFiles
            .SelectMany(f =>
            {
                var descriptor = Path.GetFileNameWithoutExtension(f.Name).Replace("Request", "Descriptor");
                var re = new Regex($@"^.+public {descriptor}\(([^\r\n\)]+?)\).*$", RegexOptions.Singleline);
                return File.ReadLines(f.FullName)
                    .Where(l => re.IsMatch(l))
                    .Select(l => re.Replace(l, "$1"))
                    .Where(args => !string.IsNullOrWhiteSpace(args) && !args.Contains(": base"))
                    .Select(args => new
                    {
                        Descriptor = descriptor,
                        Args = args
                    });
            })
            .ToDictionary(r => r.Descriptor, r => r.Args);

    public static readonly Dictionary<string, string> RequestInterfaceGenericsLookup =
        AllKnownRequestInterfaces
            .GroupBy(v => v.Request)
            .Select(g => g.Last())
            .ToDictionary(k => k.Request, v => v.Generics);

    /// <summary>
    /// Some API's reuse response this is a hardcoded map of these cases
    /// </summary>
    private static readonly Dictionary<string, (string, string)> ResponseReroute = new()
    {
        { "UpdateByQueryRethrottleResponse", ("ListTasksResponse", "") },
        { "DeleteByQueryRethrottleResponse", ("ListTasksResponse", "") },
        { "MultiSearchTemplateResponse", ("MultiSearchResponse", "") },
        { "ScrollResponse", ("SearchResponse", "<TDocument>") },
        { "SearchTemplateResponse", ("SearchResponse", "<TDocument>") }
    };

    /// <summary> Create a dictionary lookup of all responses and their generics </summary>
    public static readonly IDictionary<string, (string, string)> ResponseLookup =
        ClientResponseCsharpFiles
            .SelectMany(f => File.ReadLines(f.FullName))
            .Where(l => ResponseClassRegex.IsMatch(l))
            .Select(l =>
            {
                var c = ResponseClassRegex.Replace(l, "$1");
                var response = GenericsRemovalRegex.Replace(c, "");
                var generics = GenericsRegex.Replace(c, "$1");
                return (response, (response, generics));
            })
            .Concat(ResponseReroute.Select(kv => (kv.Key, (kv.Value.Item1, kv.Value.Item2))))
            .ToImmutableSortedDictionary(t => t.Item1, t => t.Item2);
}
