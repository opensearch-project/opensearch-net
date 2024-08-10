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
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/


using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Stack.ArtifactsApi;
using OpenSearch.Stack.ArtifactsApi.Products;
using OpenSearch.Stack.ArtifactsApi.Resolvers;
using Xunit;
using Xunit.Abstractions;

namespace OpenSearch.Stack.ArtifactsApiTests
{
    public class ReleasedVersionResolverTests
    {
        public ReleasedVersionResolverTests(ITestOutputHelper traceSink) => _traceSink = traceSink ?? throw new NullReferenceException(nameof(traceSink));

        private readonly ITestOutputHelper _traceSink;

        [U]
        public void Does_Resolver_Construct_Valid_DownloadUrl_Test()
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3)
            };

            var testCases = new[]
            {
                new {Product = Product.OpenSearch, Version = "1.2.3", Platform = OSPlatform.Linux, Architecture = Architecture.X64},
                new {Product = Product.OpenSearch, Version = "1.2.4", Platform = OSPlatform.Linux, Architecture = Architecture.Arm64},
                new {Product = Product.OpenSearch, Version = "1.0.0", Platform = OSPlatform.Linux, Architecture = Architecture.Arm64}
            };
            foreach (var testCase in testCases)
            {
                var version = OpenSearchVersion.From(testCase.Version);
                var resolveSucceeded = ReleasedVersionResolver.TryResolve(testCase.Product, version, testCase.Platform,
                    testCase.Architecture, out var artifact);
                Assert.True(resolveSucceeded);
                _traceSink.WriteLine($"Checking URL {artifact.DownloadUrl}");

                using var request = new HttpRequestMessage(HttpMethod.Head, new Uri(artifact.DownloadUrl));
                using var response = httpClient.Send(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
