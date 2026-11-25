/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Xunit;

namespace Tests.OpenSearch.Client.Serialization
{
    public class FloatingPointQuerySerializationTests
    {
        private static readonly double NegativeZero = BitConverter.Int64BitsToDouble(long.MinValue);
        private static readonly double[] Values =
        {
            NegativeZero, 0d, -1e-100, 1e-100, 1e100,
            double.Epsilon, -double.Epsilon, double.MinValue, double.MaxValue, 12.34,
        };

        [TU]
        [InlineData(false)]
        [InlineData(true)]
        public void TermPreservesFloatingPointValues(bool useStj)
        {
            foreach (var value in Values)
            {
                using var json = Serialize(new TermQuery
                {
                    Field = "amount",
                    Value = value,
                    IsVerbatim = true,
                }, useStj);

                AssertNumber(json.RootElement.GetProperty("term").GetProperty("amount").GetProperty("value"), value);
            }
        }

        [TU]
        [InlineData(false)]
        [InlineData(true)]
        public void TermsPreservesMixedNumericTypes(bool useStj)
        {
            using var json = Serialize(new TermsQuery
            {
                Field = "amount",
                Terms = new object[] { NegativeZero, 0d, 9007199254740993L, long.MinValue, long.MaxValue, 1e-100, -0.0f, "text", true, null },
                IsVerbatim = true,
            }, useStj);
            var terms = json.RootElement.GetProperty("terms").GetProperty("amount");

            terms.GetArrayLength().Should().Be(10);
            AssertNumber(terms[0], NegativeZero);
            AssertNumber(terms[1], 0d);
            terms[2].GetInt64().Should().Be(9007199254740993L);
            terms[3].GetInt64().Should().Be(long.MinValue);
            terms[4].GetInt64().Should().Be(long.MaxValue);
            AssertNumber(terms[5], 1e-100);
            AssertNumber(terms[6], NegativeZero);
            terms[7].GetString().Should().Be("text");
            terms[8].GetBoolean().Should().BeTrue();
            terms[9].ValueKind.Should().Be(JsonValueKind.Null);
        }

        [TU]
        [InlineData(false, "gt")]
        [InlineData(true, "gt")]
        [InlineData(false, "gte")]
        [InlineData(true, "gte")]
        [InlineData(false, "lt")]
        [InlineData(true, "lt")]
        [InlineData(false, "lte")]
        [InlineData(true, "lte")]
        public void RangePreservesFloatingPointValues(bool useStj, string bound)
        {
            foreach (var value in Values)
            {
                var query = new NumericRangeQuery { Field = "amount", IsVerbatim = true };
                switch (bound)
                {
                    case "gt":
                        query.GreaterThan = value;
                        break;
                    case "gte":
                        query.GreaterThanOrEqualTo = value;
                        break;
                    case "lt":
                        query.LessThan = value;
                        break;
                    case "lte":
                        query.LessThanOrEqualTo = value;
                        break;
                }

                using var json = Serialize(query, useStj);

                AssertNumber(json.RootElement.GetProperty("range").GetProperty("amount").GetProperty(bound), value);
            }
        }

        private static JsonDocument Serialize(QueryContainer query, bool useStj)
        {
            var client = new OpenSearchClient(new ConnectionSettings(new InMemoryConnection()).UseSystemTextJson(useStj));
            using var stream = new MemoryStream();
            client.RequestResponseSerializer.Serialize(query, stream);
            return JsonDocument.Parse(stream.ToArray());
        }

        private static void AssertNumber(JsonElement actual, double expected)
        {
            // Numeric equality alone treats both zero signs as equal. Integer-form -0 also loses the distinction
            // in the server, even if GetDouble preserves its sign bit.
            BitConverter.DoubleToInt64Bits(actual.GetDouble()).Should().Be(BitConverter.DoubleToInt64Bits(expected));
            if (expected == 0d)
                actual.GetRawText().IndexOfAny(new[] { '.', 'e', 'E' }).Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
