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

using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;

namespace Tests.Reproduce;

public class GithubIssue2323 : ClusterTestClassBase<ReadOnlyCluster>
{
    public GithubIssue2323(ReadOnlyCluster cluster) : base(cluster) { }

    [I]
    public void NestedInnerHitsShouldIncludedNestedProperty()
    {
        var client = Client;
        var response = client.Search<Project>(s => s
            .Query(q => q
                .Nested(n => n
                    .Path(p => p.Tags)
                    .Query(nq => nq
                        .MatchAll()
                    )
                    .InnerHits(i => i
                        .Source(false)
                    )
                )
            )
        );

        response.ShouldBeValid();

        var innerHits = response.Hits.Select(h => h.InnerHits).ToList();

        innerHits.Should().NotBeNullOrEmpty();

        var innerHit = innerHits.First();
        innerHit.Should().ContainKey("tags");
        var hitMetadata = innerHit["tags"].Hits.Hits.First();

        hitMetadata.Nested.Should().NotBeNull();
        hitMetadata.Nested.Field.Should().Be(new Field("tags"));
        hitMetadata.Nested.Offset.Should().BeGreaterOrEqualTo(0);
    }
}
