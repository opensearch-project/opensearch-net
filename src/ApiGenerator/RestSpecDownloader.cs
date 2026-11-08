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
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiGenerator.Configuration;

namespace ApiGenerator
{
    public static class RestSpecDownloader
    {
		private static readonly HttpClient Http = new();

		private const string SpecUrl = "https://api-spec.opensearch.org/opensearch-openapi.yaml";

	public static async Task DownloadAsync(CancellationToken token)
		{
			Console.WriteLine($"Downloading OpenAPI spec from {SpecUrl}");
			var spec = await Http.GetStringAsync(SpecUrl, token);
			await File.WriteAllTextAsync(GeneratorLocations.OpenApiSpecFile, spec, token);
            Console.WriteLine("Downloaded OpenAPI spec");
        }
	}
}
