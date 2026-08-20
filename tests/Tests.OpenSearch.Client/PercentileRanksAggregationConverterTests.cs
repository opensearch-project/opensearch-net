/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the settings-aware <see cref="PercentileRanksAggregationConverter"/>, the System.Text.Json
	/// replacement for the legacy Utf8Json <c>PercentileRanksAggregationFormatter</c>. Mirrors the percentiles tests,
	/// but the values list is the <c>values</c> property and is only written when non-empty.
	/// </summary>
	public class PercentileRanksAggregationConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new PercentileRanksAggregationConverter(settings));
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new ScriptConverter());
			return options;
		}

		private static IPercentileRanksAggregation Deserialize(string json) =>
			JsonSerializer.Deserialize<IPercentileRanksAggregation>(json, Options());

		private static string Serialize(IPercentileRanksAggregation value) =>
			JsonSerializer.Serialize(value, Options());

		[U] public void Read_Field_Values_Keyed()
		{
			var agg = Deserialize(@"{""field"":""load"",""values"":[100.0,200.0],""keyed"":true}");
			agg.Field.Name.Should().Be("load");
			agg.Values.Should().BeEquivalentTo(new[] { 100.0, 200.0 });
			agg.Keyed.Should().Be(true);
		}

		[U] public void Read_TDigestMethod()
		{
			var agg = Deserialize(@"{""field"":""load"",""tdigest"":{""compression"":100.0}}");
			agg.Method.Should().BeOfType<TDigestMethod>();
			((TDigestMethod)agg.Method).Compression.Should().Be(100.0);
		}

		[U] public void Read_HdrMethod()
		{
			var agg = Deserialize(@"{""field"":""load"",""hdr"":{""number_of_significant_value_digits"":3}}");
			agg.Method.Should().BeOfType<HDRHistogramMethod>();
			((HDRHistogramMethod)agg.Method).NumberOfSignificantValueDigits.Should().Be(3);
		}

		[U] public void Write_Field_IsInferred()
		{
			IPercentileRanksAggregation agg = new PercentileRanksAggregation("p", "load");
			Serialize(agg).Should().Contain(@"""field"":""load""");
		}

		[U] public void Write_ValuesAndKeyed()
		{
			IPercentileRanksAggregation agg = new PercentileRanksAggregation("p", "load")
			{
				Values = new[] { 100.0, 200.0 },
				Keyed = true
			};
			var json = Serialize(agg);
			json.Should().Contain(@"""values"":[100,200]");
			json.Should().Contain(@"""keyed"":true");
		}

		[U] public void Write_EmptyValues_Omitted()
		{
			// The legacy formatter only wrote "values" when the collection was non-empty.
			IPercentileRanksAggregation agg = new PercentileRanksAggregation("p", "load")
			{
				Values = new double[0]
			};
			Serialize(agg).Should().NotContain("values");
		}

		[U] public void Write_HdrMethod()
		{
			IPercentileRanksAggregation agg = new PercentileRanksAggregation("p", "load")
			{
				Method = new HDRHistogramMethod { NumberOfSignificantValueDigits = 2 }
			};
			Serialize(agg).Should().Contain(@"""hdr"":{""number_of_significant_value_digits"":2}");
		}

		[U] public void Write_Null_WritesJsonNull() =>
			Serialize(null).Should().Be("null");

		[U] public void RoundTrip_WithHdr()
		{
			var options = Options();
			IPercentileRanksAggregation agg = new PercentileRanksAggregation("p", "load")
			{
				Method = new HDRHistogramMethod { NumberOfSignificantValueDigits = 3 },
				Values = new[] { 100.0, 200.0 },
				Keyed = true,
				Format = "0.0"
			};
			var json = JsonSerializer.Serialize(agg, options);
			var back = JsonSerializer.Deserialize<IPercentileRanksAggregation>(json, options);
			back.Field.Name.Should().Be("load");
			back.Method.Should().BeOfType<HDRHistogramMethod>();
			back.Values.ToArray().Should().BeEquivalentTo(new[] { 100.0, 200.0 });
			back.Keyed.Should().Be(true);
			back.Format.Should().Be("0.0");
		}
	}
}
