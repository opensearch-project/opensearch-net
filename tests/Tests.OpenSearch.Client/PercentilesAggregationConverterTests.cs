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
	/// Behavioural tests for the settings-aware <see cref="PercentilesAggregationConverter"/>, the System.Text.Json
	/// replacement for the legacy Utf8Json <c>PercentilesAggregationFormatter</c>. Covers the polymorphic
	/// <see cref="IPercentilesMethod"/> (tdigest / hdr), the metric fields, <c>percents</c>, <c>keyed</c>,
	/// field-name inference and null handling.
	/// </summary>
	public class PercentilesAggregationConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new PercentilesAggregationConverter(settings));
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new ScriptConverter());
			return options;
		}

		private static IPercentilesAggregation Deserialize(string json) =>
			JsonSerializer.Deserialize<IPercentilesAggregation>(json, Options());

		private static string Serialize(IPercentilesAggregation value) =>
			JsonSerializer.Serialize(value, Options());

		[U] public void Read_Field_Percents_Keyed()
		{
			var agg = Deserialize(@"{""field"":""load"",""percents"":[25.0,50.0,99.0],""keyed"":true}");
			agg.Field.Name.Should().Be("load");
			agg.Percents.Should().BeEquivalentTo(new[] { 25.0, 50.0, 99.0 });
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

		[U] public void Read_MissingAndFormat()
		{
			var agg = Deserialize(@"{""field"":""load"",""missing"":0.0,""format"":""0.0""}");
			agg.Missing.Should().Be(0.0);
			agg.Format.Should().Be("0.0");
		}

		[U] public void Write_Field_IsInferred()
		{
			IPercentilesAggregation agg = new PercentilesAggregation("p", "load");
			var json = Serialize(agg);
			json.Should().Contain(@"""field"":""load""");
		}

		[U] public void Write_TDigestMethod()
		{
			IPercentilesAggregation agg = new PercentilesAggregation("p", "load")
			{
				Method = new TDigestMethod { Compression = 200.0 }
			};
			var json = Serialize(agg);
			json.Should().Contain(@"""tdigest"":{""compression"":200.0}");
		}

		[U] public void Write_HdrMethod()
		{
			IPercentilesAggregation agg = new PercentilesAggregation("p", "load")
			{
				Method = new HDRHistogramMethod { NumberOfSignificantValueDigits = 2 }
			};
			var json = Serialize(agg);
			json.Should().Contain(@"""hdr"":{""number_of_significant_value_digits"":2}");
		}

		[U] public void Write_PercentsAndKeyed()
		{
			IPercentilesAggregation agg = new PercentilesAggregation("p", "load")
			{
				Percents = new[] { 50.0, 95.0 },
				Keyed = false
			};
			var json = Serialize(agg);
			json.Should().Contain(@"""percents"":[50,95]");
			json.Should().Contain(@"""keyed"":false");
		}

		[U] public void Write_Null_WritesJsonNull() =>
			Serialize(null).Should().Be("null");

		[U] public void RoundTrip_WithTDigest()
		{
			var options = Options();
			IPercentilesAggregation agg = new PercentilesAggregation("p", "load")
			{
				Method = new TDigestMethod { Compression = 100.0 },
				Percents = new[] { 25.0, 50.0 },
				Keyed = true,
				Format = "0.0"
			};
			var json = JsonSerializer.Serialize(agg, options);
			var back = JsonSerializer.Deserialize<IPercentilesAggregation>(json, options);
			back.Field.Name.Should().Be("load");
			back.Method.Should().BeOfType<TDigestMethod>();
			back.Percents.ToArray().Should().BeEquivalentTo(new[] { 25.0, 50.0 });
			back.Keyed.Should().Be(true);
			back.Format.Should().Be("0.0");
		}
	}
}
