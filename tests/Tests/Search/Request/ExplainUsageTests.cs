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
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.Request;

public class ExplainUsageTests : SearchUsageTestBase
{
    /**
		 * Enables explanation for each hit on how its score was computed.
		 *
		 * See the OpenSearch documentation on {ref_current}/search-explain.html[Explain] for more detail.
		 */
    public ExplainUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object ExpectJson =>
        new { explain = true };

    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .Explain();

    protected override SearchRequest<Project> Initializer =>
        new SearchRequest<Project> { Explain = true };

    [I]
    protected async Task ExplanationIsSetOnHits() => await AssertOnAllResponses(r =>
    {
        r.Hits.Should().NotBeEmpty();
        r.Hits.Should().NotContain(hit => hit.Explanation == null);
        foreach (var explanation in r.Hits.Select(h => h.Explanation))
        {
            explanation.Description.Should().NotBeNullOrEmpty();
            explanation.Value.Should().BeGreaterThan(0);
        }
    });
}
