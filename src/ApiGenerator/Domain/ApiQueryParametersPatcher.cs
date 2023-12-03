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
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Domain.Specification;

namespace ApiGenerator.Domain
{
    public static class ApiQueryParametersPatcher
    {
        public static SortedDictionary<string, QueryParameters> Patch(
            string endpointName,
            IDictionary<string, QueryParameters> source,
            IEndpointOverrides overrides
        )
        {
            if (source == null) return null;

            var globalOverrides = new GlobalOverrides();
            var declaredKeys = source.Keys;
            var skipList = CreateSkipList(globalOverrides, overrides, declaredKeys);
            var partialList = CreatePartialList(globalOverrides, overrides, declaredKeys);

            var renameLookup = CreateRenameLookup(globalOverrides, overrides, declaredKeys);
            var obsoleteLookup = CreateObsoleteLookup(globalOverrides, overrides, declaredKeys);

            var patchedParams = new SortedDictionary<string, QueryParameters>();
            foreach (var (queryStringKey, value) in source)
            {
				value.QueryStringKey = queryStringKey;

                if (!renameLookup.TryGetValue(queryStringKey, out var preferredName)) preferredName = queryStringKey;

                value.ClsName ??= CreateCSharpName(preferredName, endpointName);

                if (skipList.Contains(queryStringKey)) value.Skip = true;

                if (partialList.Contains(queryStringKey)) value.RenderPartial = true;

                if (obsoleteLookup.TryGetValue(queryStringKey, out var obsolete)) value.Obsolete = obsolete;

                //make sure source_enabled takes a boolean only
                if (preferredName == "source_enabled") value.Type = "boolean";

				patchedParams[preferredName] = value;
            }

            return patchedParams;
        }

        private static string CreateCSharpName(string queryStringKey, string endpointName)
        {
            if (string.IsNullOrWhiteSpace(queryStringKey)) return "UNKNOWN";

            if (queryStringKey == "format" && endpointName == "text_structure.find_structure")
                return "TextStructureFindStructureFormat";

            var cased = queryStringKey.ToPascalCase();
            switch (cased)
            {
                case "Type":
                case "Index":
                case "Source":
                case "Script":
                    return cased + "QueryString";
                default:
                    return cased;
            }
        }

        private static IList<string> CreateSkipList(IEndpointOverrides global, IEndpointOverrides local, ICollection<string> declaredKeys) =>
            CreateList(global, local, "skip", e => e.SkipQueryStringParams, declaredKeys);

        private static IList<string> CreatePartialList(IEndpointOverrides global, IEndpointOverrides local, ICollection<string> declaredKeys) =>
            CreateList(global, local, "partial", e => e.RenderPartial, declaredKeys);

        private static IDictionary<string, string> CreateLookup(IEndpointOverrides global, IEndpointOverrides local, string type,
            Func<IEndpointOverrides, IDictionary<string, string>> @from, ICollection<string> declaredKeys
        )
        {
            var d = new SortedDictionary<string, string>();
            foreach (var kv in from(global)) d[kv.Key] = kv.Value;

            if (local == null) return d;

            var localDictionary = from(local);
            foreach (var kv in localDictionary) d[kv.Key] = kv.Value;

            var name = local.GetType().Name;
            foreach (var p in localDictionary.Keys.Except(declaredKeys))
                Generator.ApiGenerator.Warnings.Add($"On {name} {type} key '{p}' is not found in spec");

            return d;
        }

        private static IList<string> CreateList(IEndpointOverrides global, IEndpointOverrides local, string type,
            Func<IEndpointOverrides, IEnumerable<string>> @from, ICollection<string> declaredKeys
        )
        {
            var list = new List<string>();
            if (global != null) list.AddRange(from(global));
            if (local != null)
            {
                var localList = from(local).ToList();
                list.AddRange(localList);
                var name = local.GetType().Name;
                foreach (var p in localList.Except(declaredKeys))
                    Generator.ApiGenerator.Warnings.Add($"On {name} {type} key '{p}' is not found in spec");
            }
            return list.Distinct().ToList();
        }

        private static IDictionary<string, string> CreateRenameLookup(IEndpointOverrides global, IEndpointOverrides local,
            ICollection<string> declaredKeys
        ) =>
            CreateLookup(global, local, "rename", e => e.RenameQueryStringParams, declaredKeys);

        private static IDictionary<string, string> CreateObsoleteLookup(IEndpointOverrides global, IEndpointOverrides local,
            ICollection<string> declaredKeys
        ) =>
            CreateLookup(global, local, "obsolete", e => e.ObsoleteQueryStringParams, declaredKeys);
    }
}
