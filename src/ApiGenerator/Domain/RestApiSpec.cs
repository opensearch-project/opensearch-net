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
using System.Collections.ObjectModel;
using System.Linq;
using ApiGenerator.Domain.Specification;

namespace ApiGenerator.Domain;

public class EnumDescription
{
    public string Name { get; set; }
    public IEnumerable<string> Options { get; set; }
}

public class RestApiSpec
{
    public IDictionary<string, ApiEndpoint> Endpoints { get; set; }

    public ImmutableSortedDictionary<string, ReadOnlyCollection<ApiEndpoint>> EndpointsPerNamespaceLowLevel =>
        Endpoints.Values.GroupBy(e => e.CsharpNames.Namespace)
            .ToImmutableSortedDictionary(kv => kv.Key, kv => kv.ToList().AsReadOnly());

    public ImmutableSortedDictionary<string, ReadOnlyCollection<ApiEndpoint>> EndpointsPerNamespaceHighLevel =>
        Endpoints.Values
            .GroupBy(e => e.CsharpNames.Namespace)
            .ToImmutableSortedDictionary(kv => kv.Key, kv => kv.ToList().AsReadOnly());


    private IEnumerable<EnumDescription> _enumDescriptions;

    public IEnumerable<EnumDescription> EnumsInTheSpec
    {
        get
        {
            if (_enumDescriptions != null) return _enumDescriptions;

            string CreateName(string name, string methodName, string @namespace)
            {
                if (!name.ToLowerInvariant().Contains("metric")
                    && name.ToLowerInvariant() != "status"
                    && name.ToLowerInvariant() != "format") return name;

                return methodName.StartsWith(@namespace) ? methodName + name : @namespace + methodName + name;
            }

            var urlParameterEnums = Endpoints.Values
                .SelectMany(e => e.Url.Params.Values, (e, param) => new { e, param })
                .Where(t => t.param.Options?.Any() ?? false)
                .Select(t => new { t.param, name = CreateName(t.param.ClsName, t.e.CsharpNames.MethodName, t.e.CsharpNames.Namespace) })
                .Where(t => t.name != "Time")
                .Select(t => new EnumDescription { Name = t.name, Options = t.param.Options }).ToList();

            var urlPartEnums = Endpoints.Values
                .SelectMany(e => e.Url.Parts, (e, part) => new { e, part })
                .Where(t => t.part.Options?.Any() ?? false)
                .Select(t => new EnumDescription
                {
                    Name = CreateName(t.part.Name.ToPascalCase(), t.e.CsharpNames.MethodName, t.e.CsharpNames.Namespace),
                    Options = t.part.Options
                }).ToList();

            _enumDescriptions = urlPartEnums
                .Concat(urlParameterEnums)
                .DistinctBy(e => e.Name)
                .ToList();

            return _enumDescriptions;
        }
    }
}
