/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="RankFeatureQueryConverter"/>: a rank-feature query is a flat object with literal
	/// keys <c>_name</c>/<c>boost</c>/<c>field</c> plus at most one polymorphic function sub-object dispatched by key
	/// (<c>saturation</c>/<c>log</c>/<c>sigmoid</c>/<c>linear</c>). Covers each function dispatch branch on read and
	/// write, the no-function case, common options, null and round-trip.
	/// </summary>
	public class RankFeatureQueryConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new RankFeatureQueryConverter());
			options.Converters.Add(new FieldConverter(settings));
			return options;
		}

		private static IRankFeatureQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<IRankFeatureQuery>(json, Options());

		private static string Serialize(IRankFeatureQuery value) =>
			JsonSerializer.Serialize(value, Options());

		// --- Dispatch: saturation ---

		[U] public void Deserialize_Saturation()
		{
			var query = Deserialize(@"{""_name"":""named_query"",""boost"":1.1,""field"":""rank"",""saturation"":{}}");
			query.Should().BeOfType<RankFeatureQuery>();
			query.Name.Should().Be("named_query");
			query.Boost.Should().Be(1.1);
			query.Field.Name.Should().Be("rank");
			query.Function.Should().BeOfType<RankFeatureSaturationFunction>();
		}

		[U] public void Serialize_Saturation()
		{
			IRankFeatureQuery query = new RankFeatureQuery
			{
				Name = "named_query", Boost = 1.1, Field = "rank", Function = new RankFeatureSaturationFunction()
			};
			Serialize(query).Should().Be(@"{""_name"":""named_query"",""boost"":1.1,""field"":""rank"",""saturation"":{}}");
		}

		[U] public void Deserialize_Saturation_WithPivot()
		{
			var query = Deserialize(@"{""field"":""rank"",""saturation"":{""pivot"":8.0}}");
			var fn = query.Function.Should().BeOfType<RankFeatureSaturationFunction>().Subject;
			fn.Pivot.Should().Be(8.0f);
		}

		// --- Dispatch: log ---

		[U] public void Deserialize_Logarithm()
		{
			var query = Deserialize(@"{""field"":""rank"",""log"":{""scaling_factor"":4.0}}");
			var fn = query.Function.Should().BeOfType<RankFeatureLogarithmFunction>().Subject;
			fn.ScalingFactor.Should().Be(4.0f);
		}

		[U] public void Serialize_Logarithm()
		{
			IRankFeatureQuery query = new RankFeatureQuery
			{
				Field = "rank", Function = new RankFeatureLogarithmFunction { ScalingFactor = 4.0f }
			};
			Serialize(query).Should().Be(@"{""field"":""rank"",""log"":{""scaling_factor"":4}}");
		}

		// --- Dispatch: sigmoid ---

		[U] public void Deserialize_Sigmoid()
		{
			var query = Deserialize(@"{""field"":""rank"",""sigmoid"":{""pivot"":8.0,""exponent"":0.6}}");
			var fn = query.Function.Should().BeOfType<RankFeatureSigmoidFunction>().Subject;
			fn.Pivot.Should().Be(8.0f);
			fn.Exponent.Should().Be(0.6f);
		}

		[U] public void Serialize_Sigmoid()
		{
			IRankFeatureQuery query = new RankFeatureQuery
			{
				Field = "rank", Function = new RankFeatureSigmoidFunction { Pivot = 8.0f, Exponent = 0.6f }
			};
			var json = Serialize(query);
			json.Should().Contain(@"""sigmoid""").And.Contain(@"""pivot"":8").And.Contain(@"""exponent"":0.6");
		}

		// --- Dispatch: linear ---

		[U] public void Deserialize_Linear()
		{
			var query = Deserialize(@"{""field"":""rank"",""linear"":{}}");
			query.Function.Should().BeOfType<RankFeatureLinearFunction>();
		}

		[U] public void Serialize_Linear()
		{
			IRankFeatureQuery query = new RankFeatureQuery { Field = "rank", Function = new RankFeatureLinearFunction() };
			Serialize(query).Should().Be(@"{""field"":""rank"",""linear"":{}}");
		}

		// --- No function ---

		[U] public void Deserialize_NoFunction()
		{
			var query = Deserialize(@"{""_name"":""named_query"",""boost"":1.1,""field"":""rank""}");
			query.Field.Name.Should().Be("rank");
			query.Function.Should().BeNull();
		}

		[U] public void Serialize_NoFunction()
		{
			IRankFeatureQuery query = new RankFeatureQuery { Name = "named_query", Boost = 1.1, Field = "rank" };
			Serialize(query).Should().Be(@"{""_name"":""named_query"",""boost"":1.1,""field"":""rank""}");
		}

		// --- null ---

		[U] public void Deserialize_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Serialize_Null_WritesNull() =>
			JsonSerializer.Serialize<IRankFeatureQuery>(null, Options()).Should().Be("null");

		// --- round-trip ---

		[U] public void RoundTrip_Sigmoid()
		{
			IRankFeatureQuery original = new RankFeatureQuery
			{
				Name = "n", Boost = 2.0, Field = "rank", Function = new RankFeatureSigmoidFunction { Pivot = 8.0f, Exponent = 0.6f }
			};
			var back = Deserialize(Serialize(original));
			back.Field.Name.Should().Be("rank");
			back.Name.Should().Be("n");
			back.Boost.Should().Be(2.0);
			var fn = back.Function.Should().BeOfType<RankFeatureSigmoidFunction>().Subject;
			fn.Pivot.Should().Be(8.0f);
			fn.Exponent.Should().Be(0.6f);
		}
	}
}
