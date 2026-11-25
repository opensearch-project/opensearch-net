/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.QueryDsl.TermLevel
{
    public class FloatingPointQueryIntegrationTests : IClusterFixture<WritableCluster>
    {
        private readonly WritableCluster _cluster;

        public FloatingPointQueryIntegrationTests(WritableCluster cluster) => _cluster = cluster;

        [I]
        public async Task QueriesDistinguishFloatingPointZeros()
        {
            var client = _cluster.Client;
            var index = "floating-point-zeros-" + Guid.NewGuid().ToString("N");
            var create = await client.LowLevel.Indices.CreateAsync<StringResponse>(index,
                PostData.String(@"{""mappings"":{""properties"":{""amount"":{""type"":""double""}}}}"));
            create.Success.Should().BeTrue("{0}", create.DebugInformation);

            try
            {
                // Seed from explicit JSON so the serializer under test cannot normalize the source documents.
                var negative = await client.LowLevel.IndexAsync<StringResponse>(index, "negative-zero",
                    PostData.String(@"{""amount"":-0.0}"));
                var positive = await client.LowLevel.IndexAsync<StringResponse>(index, "positive-zero",
                    PostData.String(@"{""amount"":0.0}"));
                negative.Success.Should().BeTrue("{0}", negative.DebugInformation);
                positive.Success.Should().BeTrue("{0}", positive.DebugInformation);
                (await client.Indices.RefreshAsync(index)).ShouldBeValid();

                var negativeZero = BitConverter.Int64BitsToDouble(long.MinValue);
                var cases = new (QueryContainer Query, string[] Expected)[]
                {
                    (new TermQuery { Field = "amount", Value = negativeZero, IsVerbatim = true }, new[] { "negative-zero" }),
                    (new TermQuery { Field = "amount", Value = 0d, IsVerbatim = true }, new[] { "positive-zero" }),
                    (new TermQuery { Field = "amount", Value = 0L, IsVerbatim = true }, new[] { "positive-zero" }),
                    (new TermsQuery { Field = "amount", Terms = new object[] { negativeZero }, IsVerbatim = true }, new[] { "negative-zero" }),
                    (new TermsQuery { Field = "amount", Terms = new object[] { negativeZero, 0d }, IsVerbatim = true }, new[] { "negative-zero", "positive-zero" }),
                    (new NumericRangeQuery { Field = "amount", GreaterThan = negativeZero, IsVerbatim = true }, new[] { "positive-zero" }),
                    (new NumericRangeQuery { Field = "amount", GreaterThanOrEqualTo = negativeZero, IsVerbatim = true }, new[] { "negative-zero", "positive-zero" }),
                    (new NumericRangeQuery { Field = "amount", LessThan = negativeZero, IsVerbatim = true }, Array.Empty<string>()),
                    (new NumericRangeQuery { Field = "amount", LessThanOrEqualTo = negativeZero, IsVerbatim = true }, new[] { "negative-zero" }),
                    (new NumericRangeQuery { Field = "amount", GreaterThanOrEqualTo = negativeZero, LessThan = 0d, IsVerbatim = true }, new[] { "negative-zero" }),
                };

                foreach (var (query, expected) in cases)
                {
                    var response = await client.SearchAsync<object>(new SearchRequest(index) { Query = query });

                    response.ShouldBeValid();
                    response.Hits.Select(hit => hit.Id).Should().BeEquivalentTo(expected);
                }
            }
            finally
            {
                (await client.Indices.DeleteAsync(index)).ShouldBeValid();
            }
        }
    }
}
