/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="MovingAverageAggregationConverter"/>, the System.Text.Json replacement for the
	/// legacy Utf8Json <c>MovingAverageAggregationFormatter</c>. Exercises the polymorphic model dispatch
	/// (linear / simple / ewma / holt / holt_winters) via the paired <c>model</c> name + <c>settings</c> object, the
	/// scalar fields, buckets_path and gap_policy, and null handling.
	/// </summary>
	public class MovingAverageAggregationConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new MovingAverageAggregationConverter());
			// GapPolicy and HoltWintersType are [StringEnum]; BucketsPath is its own converter.
			options.Converters.Add(new StringEnumConverterFactory());
			options.Converters.Add(new BucketsPathConverter());
			return options;
		}

		private static IMovingAverageAggregation Deserialize(string json) =>
			JsonSerializer.Deserialize<IMovingAverageAggregation>(json, Options());

		private static string Serialize(IMovingAverageAggregation value) =>
			JsonSerializer.Serialize(value, Options());

		[U] public void Read_LinearModel()
		{
			var agg = Deserialize(@"{""model"":""linear"",""settings"":{}}");
			agg.Model.Should().BeOfType<LinearModel>();
			agg.Model.Name.Should().Be("linear");
		}

		[U] public void Read_SimpleModel()
		{
			var agg = Deserialize(@"{""model"":""simple"",""settings"":{}}");
			agg.Model.Should().BeOfType<SimpleModel>();
		}

		[U] public void Read_EwmaModel_WithSettings()
		{
			var agg = Deserialize(@"{""model"":""ewma"",""settings"":{""alpha"":0.5}}");
			agg.Model.Should().BeOfType<EwmaModel>();
			((EwmaModel)agg.Model).Alpha.Should().Be(0.5f);
		}

		[U] public void Read_HoltModel_WithSettings()
		{
			var agg = Deserialize(@"{""model"":""holt"",""settings"":{""alpha"":0.3,""beta"":0.1}}");
			agg.Model.Should().BeOfType<HoltLinearModel>();
			var holt = (HoltLinearModel)agg.Model;
			holt.Alpha.Should().Be(0.3f);
			holt.Beta.Should().Be(0.1f);
		}

		[U] public void Read_HoltWintersModel_WithSettings()
		{
			var agg = Deserialize(@"{""model"":""holt_winters"",""settings"":{""alpha"":0.3,""beta"":0.1,""gamma"":0.2,""period"":7,""pad"":true,""type"":""mult""}}");
			agg.Model.Should().BeOfType<HoltWintersModel>();
			var hw = (HoltWintersModel)agg.Model;
			hw.Alpha.Should().Be(0.3f);
			hw.Period.Should().Be(7);
			hw.Pad.Should().Be(true);
			hw.Type.Should().Be(HoltWintersType.Multiplicative);
		}

		[U] public void Read_ScalarFields()
		{
			var agg = Deserialize(@"{""format"":""0.00"",""gap_policy"":""insert_zeros"",""minimize"":true,""predict"":10,""window"":5,""buckets_path"":""the_sum""}");
			agg.Format.Should().Be("0.00");
			agg.GapPolicy.Should().Be(GapPolicy.InsertZeros);
			agg.Minimize.Should().Be(true);
			agg.Predict.Should().Be(10);
			agg.Window.Should().Be(5);
			agg.BucketsPath.Should().BeOfType<SingleBucketsPath>();
			((SingleBucketsPath)agg.BucketsPath).BucketsPath.Should().Be("the_sum");
		}

		[U] public void Read_ModelWithoutSettings_LeavesModelNull()
		{
			// The legacy formatter only assigned the model when BOTH the name and the settings segment were present.
			var agg = Deserialize(@"{""model"":""ewma""}");
			agg.Model.Should().BeNull();
		}

		[U] public void Read_NonObject_ReturnsNull() =>
			Deserialize("null").Should().BeNull();

		[U] public void Write_Null_WritesJsonNull() =>
			Serialize(null).Should().Be("null");

		[U] public void Write_ScalarFields_InLegacyOrder()
		{
			IMovingAverageAggregation agg = new MovingAverageAggregation("ma", "the_sum")
			{
				GapPolicy = GapPolicy.Skip,
				Format = "0.00",
				Window = 5,
				Minimize = true,
				Predict = 10
			};
			var json = Serialize(agg);
			json.Should().Contain(@"""buckets_path"":""the_sum""");
			json.Should().Contain(@"""gap_policy"":""skip""");
			json.Should().Contain(@"""format"":""0.00""");
			json.Should().Contain(@"""window"":5");
			json.Should().Contain(@"""minimize"":true");
			json.Should().Contain(@"""predict"":10");
		}

		[U] public void Write_EwmaModel_WritesModelNameAndSettings()
		{
			IMovingAverageAggregation agg = new MovingAverageAggregation("ma", "the_sum")
			{
				Model = new EwmaModel { Alpha = 0.5f }
			};
			var json = Serialize(agg);
			json.Should().Contain(@"""model"":""ewma""");
			json.Should().Contain(@"""settings"":{""alpha"":0.5}");
		}

		[U] public void Write_LinearModel_WritesEmptySettings()
		{
			IMovingAverageAggregation agg = new MovingAverageAggregation("ma", "the_sum")
			{
				Model = new LinearModel()
			};
			var json = Serialize(agg);
			json.Should().Contain(@"""model"":""linear""");
			json.Should().Contain(@"""settings"":{}");
		}

		[U] public void RoundTrip_HoltWinters()
		{
			var options = Options();
			IMovingAverageAggregation agg = new MovingAverageAggregation("ma", "the_sum")
			{
				GapPolicy = GapPolicy.Skip,
				Window = 30,
				Model = new HoltWintersModel { Alpha = 0.3f, Beta = 0.1f, Gamma = 0.2f, Period = 7 }
			};
			var json = JsonSerializer.Serialize(agg, options);
			var back = JsonSerializer.Deserialize<IMovingAverageAggregation>(json, options);
			back.Model.Should().BeOfType<HoltWintersModel>();
			back.Window.Should().Be(30);
			back.GapPolicy.Should().Be(GapPolicy.Skip);
			((HoltWintersModel)back.Model).Period.Should().Be(7);
		}
	}
}
