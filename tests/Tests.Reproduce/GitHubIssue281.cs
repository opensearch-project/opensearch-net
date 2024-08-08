/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Xunit;

namespace Tests.Reproduce
{
    public class GithubIssue281
    {
        public class SampleDomainObject
        {
            [JsonPropertyName("first_name")]
            public string FirstName { get; set; }

            [JsonPropertyName("last_name")]
            public string LastName { get; set; }
        }

        [U]
        public void GithubIssu281_MustNotWithTermQueryAndVerbatimEmptyValueShouldBeInRequestBody()
        {
            var connectionSettings = new ConnectionSettings(new InMemoryConnection()).DisableDirectStreaming();
            var client = new OpenSearchClient(connectionSettings);

            var action = () =>
                client.Search<SampleDomainObject>(s => s
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m.Exists(e => e.Field("last_name")))
                            .MustNot(m => m.Term(t => t.Verbatim().Field("last_name.keyword").Value(string.Empty)))
                        )
                    )
                    .Index("index")
                    .Source(sfd => null)
                );

            var response = action.Should().NotThrow().Subject;

            var json = Encoding.UTF8.GetString(response.ApiCall.RequestBodyInBytes);
            json.Should()
                .Be(
                    @"{""query"":{""bool"":{""must"":[{""exists"":{""field"":""last_name""}}],""must_not"":[{""term"":{""last_name.keyword"":{""value"":""""}}}]}}}");
        }

        [U]
        public void GithubIssue281_MustNotTermQueryAndVerbatimEmptyValueShouldBeRegisteredAsNonNull()
        {
            Func<SearchDescriptor<SampleDomainObject>, ISearchRequest> selector = s => s
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m.Exists(e => e.Field("last_name")))
                        .MustNot(m => m.Term(t => t.Verbatim().Field("last_name.keyword").Value(string.Empty)))
                    )
                )
                .Index("index")
                .Source(sfd => null);

            var searchRequest = selector.Invoke(new SearchDescriptor<SampleDomainObject>());
            var query = searchRequest.Query as IQueryContainer;

            // this is fine
            query.Bool.Must.Should().NotBeEmpty();
            query.Bool.Must.First().Should().NotBeNull("Must");

            // this too...
            query.Bool.MustNot.Should().NotBeEmpty();
            // ... and this passes so long as `.Verbatim()` is used in the `TermQuery`
            query.Bool.MustNot.First().Should().NotBeNull("MustNot");
        }

        [U]
        public void GithubIssue281_MustNotTermQueryAndNonVerbatimNonEmptyValueShouldBeRegisteredAsNonNull()
        {
            Func<SearchDescriptor<SampleDomainObject>, ISearchRequest> selector = s => s
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m.Exists(e => e.Field("last_name")))
                        .MustNot(m => m.Term(t => t.Verbatim().Field("last_name.keyword").Value("mal")))
                    )
                )
                .Index("index")
                .Source(sfd => null);

            var searchRequest = selector.Invoke(new SearchDescriptor<SampleDomainObject>());
            var query = searchRequest.Query as IQueryContainer;

            // this is fine
            query.Bool.Must.Should().NotBeEmpty();
            query.Bool.Must.First().Should().NotBeNull("Must");

            // this too...
            query.Bool.MustNot.Should().NotBeEmpty();
            // and so is this when the Term value is non-empty
            query.Bool.MustNot.First().Should().NotBeNull("MustNot");
        }

        [U]
        public void GithubIssue281_MustNotExistsClauseShouldNotBeNull()
        {
            Func<SearchDescriptor<SampleDomainObject>, ISearchRequest> selector = s => s
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m.Exists(e => e.Field("last_name")))
                        .MustNot(m => m.Exists(e => e.Field("last_name")))
                    )
                )
                .Index("index")
                .Source(sfd => null);

            var searchRequest = selector.Invoke(new SearchDescriptor<SampleDomainObject>());
            var query = searchRequest.Query as IQueryContainer;

            // this is fine
            query.Bool.Must.Should().NotBeEmpty();
            query.Bool.Must.First().Should().NotBeNull("Must");

            // MustNot ... Exists seems to work
            query.Bool.MustNot.Should().NotBeEmpty();
            query.Bool.MustNot.First().Should().NotBeNull("MustNot");
        }

        [U]
        public void GithubIssue281_TermQueryWithNonEmptyValueSerializesToNonNullResult()
        {
            Func<QueryContainerDescriptor<SampleDomainObject>, QueryContainer> termQuery =
                m => m.Term(t => t.Field("last_name.keyword").Value("doe"));

            var result = termQuery.Invoke(new QueryContainerDescriptor<SampleDomainObject>());

            result.Should().NotBeNull();
        }

        [U]
        public void GithubIssue281_TermQueryWithVerbatimEmptyValueSerializesToNonNullResult()
        {
            Func<QueryContainerDescriptor<SampleDomainObject>, QueryContainer> termQuery =
                m => m.Term(t => t.Verbatim().Field("last_name.keyword").Value(string.Empty));

            var result = termQuery.Invoke(new QueryContainerDescriptor<SampleDomainObject>());

            result.Should().NotBeNull();
        }

        [TU]
        [InlineData("null", null, true)]
        [InlineData("non-empty string", "doe", false)]
        [InlineData("empty string", "", true)]
        public void GithubIssue281_TermQueryIsConditionless(string scenario, string val, bool expected)
        {
            bool SimulateIsConditionless(ITermQuery q)
            {
                return q.Value == null || string.IsNullOrEmpty(q.Value.ToString());
            }

            var temrQuery = new TermQuery { Value = val };

            var result = SimulateIsConditionless(temrQuery);

            result.Should().Be(expected, $"{scenario} should be conditionless: ${expected}");
        }
    }
}
