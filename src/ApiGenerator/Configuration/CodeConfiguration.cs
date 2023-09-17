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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GlobExpressions;

namespace ApiGenerator.Configuration
{
    public static class CodeConfiguration
	{
		private static readonly Glob[] OperationsToInclude =
		{
			new("{create,delete}_pit"),
			new("{delete,get}_all_pits"),
            
			new("cat.aliases"),
			new("cat.allocation"),
			new("cat.count"),
			new("cat.fielddata"),
			new("cat.health"),
			new("cat.help"),
			new("cat.indices"),

			new("cluster.*"),
			new("dangling_indices.*"),

			new("indices.{delete,exists,get,put}_index_template"),

			new("ingest.*"),
            new("nodes.*"),
			new("snapshot.*"),
			new("tasks.*")
		};

		public static bool IncludeOperation(string name) => OperationsToInclude.Any(g => g.IsMatch(name));

		/// <summary>
        /// Map API default names for API's we are only supporting on the low level client first
        /// </summary>
        private static readonly Dictionary<string, string> LowLevelApiNameMapping = new()
		{
        };

        /// <summary>
        /// Scan all OSC source code files for Requests and look for the [MapsApi(filename)] attribute.
        /// The class name minus Request is used as the canonical .NET name for the API.
        /// </summary>
        public static readonly Dictionary<string, string> HighLevelApiNameMapping =
            (from f in new DirectoryInfo(GeneratorLocations.OpenSearchClientFolder).GetFiles("*.cs", SearchOption.AllDirectories)
                let contents = File.ReadAllText(f.FullName)
                let c = Regex.Replace(contents, @"^.+\[MapsApi\(""([^ \r\n]+)""\)\].*$", "$1", RegexOptions.Singleline)
                where !c.Contains(" ") //filter results that did not match
                select new { Value = f.Name.Replace("Request", ""), Key = c.Replace(".json", "") })
            .DistinctBy(v => v.Key)
            .ToDictionary(k => k.Key, v => v.Value.Replace(".cs", ""));

        public static readonly HashSet<string> EnableHighLevelCodeGen = new HashSet<string>();

        public static bool IsNewHighLevelApi(string apiFileName) =>
            // no requests with [MapsApi("filename.json")] found
            !HighLevelApiNameMapping.ContainsKey(apiFileName.Replace(".json", ""));

        public static bool IgnoreHighLevelApi(string apiFileName)
        {
			//always generate already mapped requests

            if (HighLevelApiNameMapping.ContainsKey(apiFileName.Replace(".json", ""))) return false;

            return !EnableHighLevelCodeGen.Contains(apiFileName);
        }

        private static Dictionary<string, string> _apiNameMapping;

        public static Dictionary<string, string> ApiNameMapping
        {
            get
            {
                if (_apiNameMapping != null) return _apiNameMapping;
                lock (LowLevelApiNameMapping)
                {
                    if (_apiNameMapping == null)
                    {
                        var mapping = new Dictionary<string, string>(HighLevelApiNameMapping);
                        foreach (var (k, v) in LowLevelApiNameMapping)
                            mapping[k] = v;
                        _apiNameMapping = mapping;
                    }
                    return _apiNameMapping;
                }
            }
        }

        private static readonly string ResponseBuilderAttributeRegex = @"^.+\[ResponseBuilderWithGeneric\(""([^ \r\n]+)""\)\].*$";
        /// <summary>
        /// Scan all OSC source code files for Requests and look for the [MapsApi(filename)] attribute.
        /// The class name minus Request is used as the canonical .NET name for the API.
        /// </summary>
        public static readonly Dictionary<string, string> ResponseBuilderInClientCalls =
            (from f in new DirectoryInfo(GeneratorLocations.OpenSearchClientFolder).GetFiles("*.cs", SearchOption.AllDirectories)
                from l in File.ReadLines(f.FullName)
                where Regex.IsMatch(l, ResponseBuilderAttributeRegex)
                let c = Regex.Replace(l, @"^.+\[ResponseBuilderWithGeneric\(""([^ \r\n]+)""\)\].*$", "$1", RegexOptions.Singleline)
                select new { Key = f.Name.Replace(".cs", ""), Value = c })
            .DistinctBy(v => v.Key)
            .ToDictionary(k => k.Key, v => v.Value);

        public static readonly Dictionary<string, string> DescriptorGenericsLookup =
            (from f in new DirectoryInfo(GeneratorLocations.OpenSearchClientFolder).GetFiles("*Request.cs", SearchOption.AllDirectories)
                let name = Path.GetFileNameWithoutExtension(f.Name).Replace("Request", "")
                let contents = File.ReadAllText(f.FullName)
                let c = Regex.Replace(contents, $@"^.+class ({name}Descriptor(?:<[^>\r\n]+>)?[^ \r\n]*).*$", "$1", RegexOptions.Singleline)
                let key = $"{name}Descriptor"
                select new { Key = key, Value = Regex.Replace(c, @"^.*?(?:(\<.+>).*?)?$", "$1") })
            .DistinctBy(v => v.Key)
            .OrderBy(v => v.Key)
            .ToDictionary(k => k.Key, v => v.Value);

        /// <summary> Scan all OSC files for request interfaces and note any generics declared on them </summary>
        private static readonly List<Tuple<string, string>> AllKnownRequestInterfaces = (
            // find all files in OSC ending with Request.cs
            from f in new DirectoryInfo(GeneratorLocations.OpenSearchClientFolder).GetFiles("*Request.cs", SearchOption.AllDirectories)
            from l in File.ReadLines(f.FullName)
            // attempt to locate all Request interfaces lines
            where Regex.IsMatch(l, @"^.+interface [^ \r\n]+Request")
            //grab the interface name including any generics declared on it
            let c = Regex.Replace(l, @"^.+interface ([^ \r\n]+Request(?:<[^>\r\n]+>)?[^ \r\n]*).*$", "$1", RegexOptions.Singleline)
            where c.StartsWith("I") && c.Contains("Request")
            let request = Regex.Replace(c, "<.*$", "")
            let generics = Regex.Replace(c, @"^.*?(?:(\<.+>).*?)?$", "$1")
            select Tuple.Create(request,  generics)
            )
            .OrderBy(v=>v.Item1)
            .ToList();

        public static readonly HashSet<string> GenericOnlyInterfaces = new HashSet<string>(AllKnownRequestInterfaces
            .GroupBy(v => v.Item1)
            .Where(g => g.All(v => !string.IsNullOrEmpty(v.Item2)))
            .Select(g => g.Key)
            .ToList());

        public static readonly HashSet<string> DocumentRequests = new HashSet<string>((
            // find all files in OSC ending with Request.cs
            from f in new DirectoryInfo(GeneratorLocations.OpenSearchClientFolder).GetFiles("*Request.cs", SearchOption.AllDirectories)
            from l in File.ReadLines(f.FullName)
            // attempt to locate all Request interfaces lines
            where Regex.IsMatch(l, @"^.+interface [^ \r\n]+Request")
            where l.Contains("IDocumentRequest")
            let c = Regex.Replace(l, @"^.+interface ([^ \r\n]+Request(?:<[^>\r\n]+>)?[^ \r\n]*).*$", "$1", RegexOptions.Singleline)
            //grab the interface name including any generics declared on it
            let request = Regex.Replace(c, "<.*$", "")
            select request
            )
            .ToList());

        public static readonly Dictionary<string, string> DescriptorConstructors = (
            // find all files in OSC ending with Request.cs
            from f in new DirectoryInfo(GeneratorLocations.OpenSearchClientFolder).GetFiles("*Request.cs", SearchOption.AllDirectories)
            let descriptor = Path.GetFileNameWithoutExtension(f.Name).Replace("Request", "Descriptor")
            let re = $@"^.+public {descriptor}\(([^\r\n\)]+?)\).*$"
            from l in File.ReadLines(f.FullName)
            where Regex.IsMatch(l, re)
            let args = Regex.Replace(l, re, "$1", RegexOptions.Singleline)
            where !string.IsNullOrWhiteSpace(args) && !args.Contains(": base")
            select (Descriptor: descriptor, Args: args)
            )
            .ToDictionary(r => r.Descriptor, r => r.Args);

        public static readonly Dictionary<string, string> RequestInterfaceGenericsLookup =
            AllKnownRequestInterfaces
            .GroupBy(v=>v.Item1)
            .Select(g=>g.Last())
            .ToDictionary(k => k.Item1, v => v.Item2);

        /// <summary>
        /// Some API's reuse response this is a hardcoded map of these cases
        /// </summary>
        private static Dictionary<string, (string, string)> ResponseReroute = new Dictionary<string, (string, string)>
        {
            {"UpdateByQueryRethrottleResponse", ("ListTasksResponse", "")},
            {"DeleteByQueryRethrottleResponse", ("ListTasksResponse", "")},
            {"MultiSearchTemplateResponse", ("MultiSearchResponse", "")},
            {"ScrollResponse", ("SearchResponse", "<TDocument>")},
            {"SearchTemplateResponse", ("SearchResponse", "<TDocument>")},

        };


        /// <summary> Create a dictionary lookup of all responses and their generics </summary>
        public static readonly SortedDictionary<string, (string, string)> ResponseLookup = new SortedDictionary<string, (string, string)>(
        (
            // find all files in OSC ending with Request.cs
            from f in new DirectoryInfo(GeneratorLocations.OpenSearchClientFolder).GetFiles("*Response.cs", SearchOption.AllDirectories)
            from l in File.ReadLines(f.FullName)
            // attempt to locate all Response class lines
            where Regex.IsMatch(l, @"^.+public class [^ \r\n]+Response")
            //grab the response name including any generics declared on it
            let c = Regex.Replace(l, @"^.+public class ([^ \r\n]+Response(?:<[^>\r\n]+>)?[^ \r\n]*).*$", "$1", RegexOptions.Singleline)
            where c.Contains("Response")
            let response = Regex.Replace(c, "<.*$", "")
            let generics = Regex.Replace(c, @"^.*?(?:(\<.+>).*?)?$", "$1")
            select (response,  (response, generics))
        )
            .Concat(ResponseReroute.Select(kv=>(kv.Key, (kv.Value.Item1, kv.Value.Item2))))
            .ToDictionary(t=>t.Item1, t=>t.Item2));

    }
}
