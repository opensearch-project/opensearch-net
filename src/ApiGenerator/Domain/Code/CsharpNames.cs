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
using System.Linq;
using ApiGenerator.Configuration;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Generator;

namespace ApiGenerator.Domain.Code
{
    public class CsharpNames
    {
        public CsharpNames(string name, string endpointMethodName, string endpointNamespace)
        {
            RestSpecName = string.IsNullOrWhiteSpace(endpointNamespace) ? endpointMethodName : $"{endpointNamespace}.{endpointMethodName}";
            Namespace = CreateCSharpNamespace(endpointNamespace);
            if (CodeConfiguration.ApiNameMapping.TryGetValue(name, out var mapsApiMethodName))
                ApiName = mapsApiMethodName;
            else ApiName = endpointMethodName.ToPascalCase();

            //if the api name starts with the namespace do not repeat it in the method name
            string Replace(string original, string ns, string find, string replace, string[] exceptions)
            {
                if (ns != null && Namespace != ns) return original;
                if (exceptions.Contains(original)) return original;

                var replaced = original.Replace(find, replace);
                if (string.IsNullOrEmpty(replaced)) return original;

                return replaced;
            }

            MethodName = Replace(ApiName, null, Namespace, "", new string[0]);

            var namespaceRenames = new Dictionary<string, (string find, string replace, string[] exceptions)>
            {
                { "Indices", (find: "Index", replace: "", exceptions: new [] { "SimulateIndexTemplate" }) },
            };
            foreach (var (ns, (find, replace, exceptions)) in namespaceRenames)
                MethodName = Replace(MethodName, ns, find, replace, exceptions);
        }

        /// <summary> Pascal cased version of the namespace from the specification </summary>
        public string Namespace { get; }

        public string RestSpecName { get; }

        /// <summary>
        /// The pascal cased method name as loaded by <see cref="ApiEndpointFactory.From"/>
        /// <pre>Uses <see cref="CodeConfiguration.ApiNameMapping"/> mapping of request implementations in the OSC code base</pre>
        /// </summary>
        public string MethodName { get; }

        public string ApiName { get; }

        public string RequestName => $"{ApiName}Request";

        public string ResponseName
        {
            get
            {
                if (Namespace == "Cat") return $"CatResponse<{ApiName}Record>";
                else if (ApiName.EndsWith("Exists")) return $"ExistsResponse";

                var generatedName = $"{ApiName}Response";
                var name = CodeConfiguration.ResponseLookup.TryGetValue(generatedName, out var lookup) ? lookup.Item1 : generatedName;
                return name;
            }
        }
        public string RequestInterfaceName => $"I{RequestName}";
        public string ParametersName => $"{RequestName}Parameters";
        public string DescriptorName => $"{ApiName}Descriptor";

        public const string ApiNamespace = "Specification";
        public const string ApiNamespaceSuffix = "Api";
        public const string RootNamespace = "NoNamespace";
        public const string LowLevelClientNamespacePrefix = "LowLevel";
        public const string HighLevelClientNamespacePrefix = "";
        public const string ClientNamespaceSuffix = "Namespace";

        private static string CreateCSharpNamespace(string endpointNamespace)
        {
            switch (endpointNamespace)
            {
                case null:
                case "": return RootNamespace;
                default: return endpointNamespace.ToPascalCase();
            }
        }

        public string PerPathMethodName(string path)
        {
            Func<string, bool> ms = s => Namespace != null && Namespace.StartsWith(s);
            Func<string, bool> pc = path.Contains;

            var method = MethodName;
            // This is temporary for transition period
            // TODO: remove in branch once it in opensearch is scrubbed
            if (path.Contains("{type}") && !method.Contains("Type")) method += "UsingType";

            if (ms("Indices") && !pc("{index}"))
                return (method + "ForAll").Replace("AsyncForAll", "ForAllAsync");

            if (ms("Nodes") && !pc("{node_id}"))
                return (method + "ForAll").Replace("AsyncForAll", "ForAllAsync");

            return method;
        }


        public string GenericsDeclaredOnRequest =>
            CodeConfiguration.RequestInterfaceGenericsLookup.TryGetValue(RequestInterfaceName, out var requestGeneric) ? requestGeneric : null;

        public string GenericsDeclaredOnResponse =>
            CodeConfiguration.ResponseLookup.TryGetValue(ResponseName, out var requestGeneric) ? requestGeneric.Item2 : null;

        public string GenericsDeclaredOnDescriptor =>
            CodeConfiguration.DescriptorGenericsLookup.TryGetValue(DescriptorName, out var generic) ? generic : null;

        public List<string> ResponseGenerics =>
            !CodeConfiguration.ResponseLookup.TryGetValue(ResponseName, out var responseGeneric)
            || string.IsNullOrEmpty(responseGeneric.Item2)
                ? new List<string>()
                : SplitGeneric(responseGeneric.Item2);

        public List<string> DescriptorGenerics =>
            CodeConfiguration.DescriptorGenericsLookup.TryGetValue(DescriptorName, out var generic) ? SplitGeneric(generic) : new List<string>();

        public bool DescriptorBindsOverMultipleDocuments =>
            HighLevelDescriptorMethodGenerics.Count == 2 && HighLevelDescriptorMethodGenerics.All(g => g.Contains("Document"));
        //&& ResponseGenerics.FirstOrDefault() == DescriptorBoundDocumentGeneric ;

        public string DescriptorBoundDocumentGeneric =>
            HighLevelDescriptorMethodGenerics.FirstOrDefault(g => g == "TDocument") ?? HighLevelDescriptorMethodGenerics.Last();

        public List<string> HighLevelDescriptorMethodGenerics => DescriptorGenerics
            .Concat(ResponseGenerics)
            .Distinct()
            .ToList();

        public static List<string> SplitGeneric(string generic) => (generic ?? string.Empty)
            .Replace("<", "")
            .Replace(">", "")
            .Split(",")
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct()
            .ToList();


        public bool DescriptorNotFoundInCodebase => !CodeConfiguration.DescriptorGenericsLookup.TryGetValue(DescriptorName, out _);

        public string GenericDescriptorName => GenericsDeclaredOnDescriptor.IsNullOrEmpty() ? null : $"{DescriptorName}{GenericsDeclaredOnDescriptor}";
        public string GenericRequestName => GenericsDeclaredOnRequest.IsNullOrEmpty() ? null : $"{RequestName}{GenericsDeclaredOnRequest}";
        public string GenericInterfaceName => GenericsDeclaredOnRequest.IsNullOrEmpty() ? null : $"I{GenericRequestName}";
        public string GenericResponseName => GenericsDeclaredOnResponse.IsNullOrEmpty() ? null : $"{ResponseName}{GenericsDeclaredOnResponse}";

        public string GenericOrNonGenericDescriptorName => GenericDescriptorName ?? DescriptorName;
        public string GenericOrNonGenericInterfaceName => GenericInterfaceName ?? RequestInterfaceName;
        public string GenericOrNonGenericResponseName
        {
            get
            {
                var full = GenericResponseName ?? ResponseName;
                if (full.StartsWith("SearchResponse<"))
                    full = "I" + full;
                return full;
            }
        }

        /// <summary> If matching Request.cs only defined generic interface make the client method only accept said interface </summary>
        public string GenericOrNonGenericInterfacePreference => CodeConfiguration.GenericOnlyInterfaces.Contains(RequestInterfaceName)
            ? GenericInterfaceName
            : RequestInterfaceName;

        /// <summary> If matching Request.cs only defined generic interface make the client method only accept said interface </summary>
        public string GenericOrNonGenericRequestPreference => CodeConfiguration.GenericOnlyInterfaces.Contains(RequestInterfaceName)
            ? GenericRequestName
            : RequestName;

        public bool CustomResponseBuilderPerRequestOverride(out string call) => CodeConfiguration.ResponseBuilderInClientCalls.TryGetValue(RequestName, out call);

        public static string GetEnumName(string schemaKey)
        {
            var enumName = schemaKey.Replace("_common", "").SplitPascalCase().ToPascalCase();
            if (GlobalOverrides.Instance.RenameEnums.TryGetValue(enumName, out var renamed)) enumName = renamed;
            return enumName;
        }
    }
}
