/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.QueryDsl.FullText.CombinedFields
{
	[SkipVersion("<3.2.0", "combined_fields query was added in OpenSearch 3.2.0")]
	public class CombinedFieldsUsageTests : QueryDslUsageTestsBase
	{
		public CombinedFieldsUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<ICombinedFieldsQuery>(a => a.CombinedFields)
		{
			q => q.Query = null,
			q => q.Query = string.Empty
		};

		protected override QueryContainer QueryInitializer => new CombinedFieldsQuery
		{
			Fields = Field<Project>(p => p.Description).And("myOtherField"),
			Query = "hello world",
			Boost = 1.1,
			Operator = Operator.And,
			MinimumShouldMatch = 2,
			Name = "named_query"
		};

		protected override object QueryJson => new
		{
			combined_fields = new
			{
				_name = "named_query",
				boost = 1.1,
				query = "hello world",
				minimum_should_match = 2,
				@operator = "and",
				fields = new[]
				{
					"description",
					"myOtherField"
				}
			}
		};

		protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
			.CombinedFields(c => c
				.Fields(f => f.Field(p => p.Description).Field("myOtherField"))
				.Query("hello world")
				.Boost(1.1)
				.Operator(Operator.And)
				.MinimumShouldMatch(2)
				.Name("named_query")
			);
	}

	/**[float]
	 * === Combined fields with boost usage
	 */
	[SkipVersion("<3.2.0", "combined_fields query was added in OpenSearch 3.2.0")]
	public class CombinedFieldsWithBoostUsageTests : QueryDslUsageTestsBase
	{
		public CombinedFieldsWithBoostUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override QueryContainer QueryInitializer => new CombinedFieldsQuery
		{
			Fields = Field<Project>(p => p.Description, 2.2).And("myOtherField^0.3"),
			Query = "hello world",
		};

		protected override object QueryJson => new
		{
			combined_fields = new
			{
				query = "hello world",
				fields = new[]
				{
					"description^2.2",
					"myOtherField^0.3"
				}
			}
		};

		protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
			.CombinedFields(c => c
				.Fields(Field<Project>(p => p.Description, 2.2).And("myOtherField^0.3"))
				.Query("hello world")
			);
	}
}
