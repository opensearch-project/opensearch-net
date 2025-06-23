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
using ApiGenerator.Configuration;
using ApiGenerator.Domain.Specification;

namespace ApiGenerator.Domain
{
    public class EnumDescription
    {
        public string Name { get; set; }
		public bool IsFlag { get; set; }
        public IEnumerable<(string Value, string Alias)> Options { get; set; }
    }

    public class RestApiSpec
    {
		public ImmutableSortedDictionary<string, ApiEndpoint> Endpoints { get; set; }

        public ImmutableSortedDictionary<string, ReadOnlyCollection<ApiEndpoint>> EndpointsPerNamespaceLowLevel =>
            Endpoints.Values.GroupBy(e=>e.CsharpNames.Namespace)
                .ToImmutableSortedDictionary(kv => kv.Key, kv => kv.ToList().AsReadOnly());

        public ImmutableSortedDictionary<string, ReadOnlyCollection<ApiEndpoint>> EndpointsPerNamespaceHighLevel =>
            Endpoints.Values
                .Where(v => !CodeConfiguration.IgnoreHighLevelApi(v.Name))
                .GroupBy(e => e.CsharpNames.Namespace)
                .ToImmutableSortedDictionary(kv => kv.Key, kv => kv.ToList().AsReadOnly());

        public ImmutableList<EnumDescription> EnumsInTheSpec { get; set; }
    }
}
