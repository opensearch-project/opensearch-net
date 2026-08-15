/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="FilterAggregationConverter"/>, the System.Text.Json replacement for the legacy
	/// Utf8Json <c>FilterAggregationFormatter</c>. An <see cref="IFilterAggregation"/> is written as the bare body of
	/// its <c>filter</c> query (a <see cref="QueryContainer"/>) with no wrapper; a null / non-writable filter writes an
	/// empty object; a non-object token reads as null.
	/// </summary>
	public class FilterAggregationConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new FilterAggregationConverter());
			// The filter body is a QueryContainer; register the container + per-query converters it delegates to.
			options.Converters.Add(new QueryContainerConverter());
			options.Converters.Add(new QueryContainerInterfaceConverter());
			options.Converters.Add(new QueryContainerCollectionConverter());
			options.Converters.Add(new ReadAsConverterFactory());
			options.Converters.Add(new FieldNameQueryConverterFactory(settings));
			return options;
		}

		private static IFilterAggregation Deserialize(string json) =>
			JsonSerializer.Deserialize<IFilterAggregation>(json, Options());

		private static string Serialize(IFilterAggregation value) =>
			JsonSerializer.Serialize(value, Options());

		[U] public void Write_Filter_WritesBareQueryBody()
		{
			IFilterAggregation agg = new FilterAggregation("f")
			{
				Filter = new TermQuery { Field = "state", Value = "active" }
			};
			var json = Serialize(agg);
			json.Should().StartWith(@"{""term"":");
			json.Should().Contain("state").And.Contain("active");
		}

		[U] public void Write_NullFilter_WritesEmptyObject()
		{
			IFilterAggregation agg = new FilterAggregation("f") { Filter = null };
			Serialize(agg).Should().Be("{}");
		}

		[U] public void Write_NonWritableFilter_WritesEmptyObject()
		{
			// An empty QueryContainer (no query set) is conditionless and not verbatim => not writable.
			IFilterAggregation agg = new FilterAggregation("f") { Filter = new QueryContainer() };
			Serialize(agg).Should().Be("{}");
		}

		[U] public void Read_Object_PopulatesFilter()
		{
			var agg = Deserialize(@"{""term"":{""state"":{""value"":""active""}}}");
			agg.Should().NotBeNull();
			agg.Filter.Should().NotBeNull();
			((IQueryContainer)agg.Filter).Term.Should().NotBeNull();
			((IQueryContainer)agg.Filter).Term.Field.Name.Should().Be("state");
		}

		[U] public void Read_EmptyObject_ReturnsAggregationWithEmptyFilter()
		{
			var agg = Deserialize("{}");
			agg.Should().NotBeNull();
			// An empty object yields a (non-null) empty QueryContainer, matching the legacy formatter.
			agg.Filter.Should().NotBeNull();
		}

		[U] public void Read_NonObject_ReturnsNull() =>
			Deserialize("null").Should().BeNull();

		[U] public void RoundTrip_TermFilter()
		{
			var options = Options();
			IFilterAggregation agg = new FilterAggregation("f")
			{
				Filter = new TermQuery { Field = "state", Value = "active" }
			};
			var json = JsonSerializer.Serialize(agg, options);
			var back = JsonSerializer.Deserialize<IFilterAggregation>(json, options);
			back.Should().NotBeNull();
			((IQueryContainer)back.Filter).Term.Field.Name.Should().Be("state");
		}
	}
}
