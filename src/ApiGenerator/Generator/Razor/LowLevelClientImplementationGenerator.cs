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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiGenerator.Configuration;
using ApiGenerator.Domain;
using ApiGenerator.Domain.Code;
using ShellProgressBar;

namespace ApiGenerator.Generator.Razor;

public class LowLevelClientImplementationGenerator : RazorGeneratorBase
{
    public override string Title => "OpenSearch.Net client implementation";

    public override async Task Generate(RestApiSpec spec, ProgressBar progressBar, CancellationToken token)
    {
        await DoRazor(spec, View(), Target(), token);

        await DoRazor(HttpMethod.All, View("Http"), Target("Http"), token);

        await DoRazorDependantFiles(
            progressBar,
            spec.EndpointsPerNamespaceLowLevel.Where(kv => kv.Key != CsharpNames.RootNamespace).ToList(),
            View("Namespace"),
            kv => kv.Key,
            Target,
            token
        );

        return;

        string View(string id = null) =>
            ViewLocations.LowLevel("Client", "Implementation", $"OpenSearchLowLevelClient{(id != null ? $".{id}" : string.Empty)}.cshtml");

        string Target(string id = null) => GeneratorLocations.LowLevel($"OpenSearchLowLevelClient{(id != null ? $".{id}" : string.Empty)}.cs");
    }
}
