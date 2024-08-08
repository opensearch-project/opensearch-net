/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Xunit;

namespace Tests.QueryDsl.Container
{
    public class QueryContainerTests
    {
        [TU]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void StrictAndVerbatimAttributesAreRecursivelySetCorrectly(bool targetStrict, bool targetVerbatim)
        {
            // Arrange
            var query0 = new TermQuery { Field = "field", Value = 1, IsStrict = !targetStrict, IsVerbatim = !targetVerbatim };
            var query1 = new BoolQuery { MustNot = new QueryContainer[] { query0 }, IsStrict = !targetStrict, IsVerbatim = !targetVerbatim };
            var query2 = new TermQuery { Field = "field2", Value = 7, IsStrict = !targetStrict, IsVerbatim = !targetVerbatim };
            var query3 = new BoolQuery { Must = new QueryContainer[] { query1, query2 }, IsStrict = !targetStrict, IsVerbatim = !targetVerbatim };
            var queryContainer = new QueryContainer(query3);

            // Act
            queryContainer.Strict(targetStrict, recurse: true);
            queryContainer.Verbatim(targetVerbatim, recurse: true);

            // Assert
            query0.IsStrict.Should().Be(targetStrict);
            query0.IsVerbatim.Should().Be(targetVerbatim);
            query1.IsStrict.Should().Be(targetStrict);
            query1.IsVerbatim.Should().Be(targetVerbatim);
            query2.IsStrict.Should().Be(targetStrict);
            query2.IsVerbatim.Should().Be(targetVerbatim);
            query3.IsStrict.Should().Be(targetStrict);
            query3.IsVerbatim.Should().Be(targetVerbatim);
        }

        [TU]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void StrictAndVerbatimAttributesAreSetCorrectly(bool targetStrict, bool targetVerbatim)
        {
            // Arrange
            var query0 = new TermQuery { Field = "field", Value = 1, IsStrict = !targetStrict, IsVerbatim = !targetVerbatim };
            var query1 = new BoolQuery { MustNot = new QueryContainer[] { query0 }, IsStrict = !targetStrict, IsVerbatim = !targetVerbatim };
            var query2 = new TermQuery { Field = "field2", Value = 7, IsStrict = !targetStrict, IsVerbatim = !targetVerbatim };
            var query3 = new BoolQuery { Must = new QueryContainer[] { query1, query2 }, IsStrict = !targetStrict, IsVerbatim = !targetVerbatim };
            var queryContainer = new QueryContainer(query3);

            // Act
            queryContainer.Strict(targetStrict, recurse: false);
            queryContainer.Verbatim(targetVerbatim, recurse: false);

            // Assert
            query0.IsStrict.Should().Be(!targetStrict);
            query0.IsVerbatim.Should().Be(!targetVerbatim);
            query1.IsStrict.Should().Be(!targetStrict);
            query1.IsVerbatim.Should().Be(!targetVerbatim);
            query2.IsStrict.Should().Be(!targetStrict);
            query2.IsVerbatim.Should().Be(!targetVerbatim);

            query3.IsStrict.Should().Be(targetStrict);
            query3.IsVerbatim.Should().Be(targetVerbatim);
        }

        [TU]
        [InlineData("name1")]
        [InlineData("a name")]
        [InlineData(null)]
        public void SettingTheNameOnTheQueryContainerSetTheNameOnTheContainedQuery(string name)
        {
            // Arrange
            var query0 = new TermQuery { Name = "a", Field = "field", Value = 1 };
            var query1 = new BoolQuery { Name = "b", MustNot = new QueryContainer[] { query0 } };
            var query2 = new TermQuery { Name = "c", Field = "field2", Value = 7 };
            var query3 = new BoolQuery { Name = "d", Must = new QueryContainer[] { query1, query2 } };
            var queryContainer = new QueryContainer(query3);

            // Act
            queryContainer.Name(name);

            // Assert
            query3.Name.Should().Be(name);
            queryContainer.ContainedQuery.Name.Should().Be(name);
            query0.Name.Should().Be("a");
            query1.Name.Should().Be("b");
            query2.Name.Should().Be("c");
        }


    }
}
