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
using OpenSearch.Net;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the settings-aware <see cref="ScoreFunctionConverter"/>: the polymorphic dispatch of an
	/// <see cref="IScoreFunction"/> to its concrete type (decay exp/gauss/linear in numeric/date/geo variants,
	/// random_score, field_value_factor, script_score, and the weight-only WeightFunction), the co-existing common
	/// fields (<c>filter</c>/<c>weight</c>), null handling and round-tripping. Field names are resolved through the
	/// runtime Inferrer, so the converter is constructed with the connection settings.
	/// </summary>
	public class ScoreFunctionConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				// Mirror the real serializer's relaxed encoder so script source (e.g. doc['x'].value) is not escaped.
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			// The converter under test.
			options.Converters.Add(new ScoreFunctionConverter(settings));
			// Sub-converters the decay/random/field-value/script bodies rely on.
			options.Converters.Add(new StringEnumConverterFactory()); // MultiValueMode, FieldValueFactorModifier
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new TimeConverter());
			options.Converters.Add(new DistanceConverter());
			options.Converters.Add(new GeoLocationConverter());
			options.Converters.Add(new DateMathConverter());
			options.Converters.Add(new DateMathExpressionConverter());
			options.Converters.Add(new ScriptConverter());
			// RandomScoreFunction.Seed is a Union<long, string>; the union open generic is not registered globally,
			// so provide the closed converter here for the read path (write serializes the seed inline).
			options.Converters.Add(new UnionConverter<long, string>());
			return options;
		}

		private static IScoreFunction Deserialize(string json) =>
			JsonSerializer.Deserialize<IScoreFunction>(json, Options());

		private static string Serialize(IScoreFunction value) =>
			JsonSerializer.Serialize(value, Options());

		// --- Dispatch: field_value_factor ---

		[U] public void Deserialize_FieldValueFactor()
		{
			var fn = Deserialize(@"{""field_value_factor"":{""field"":""popularity"",""factor"":1.2,""modifier"":""log1p"",""missing"":0.5}}");
			fn.Should().BeOfType<FieldValueFactorFunction>();
			var fvf = (IFieldValueFactorFunction)fn;
			fvf.Field.Name.Should().Be("popularity");
			fvf.Factor.Should().Be(1.2);
			fvf.Modifier.Should().Be(FieldValueFactorModifier.Log1P);
			fvf.Missing.Should().Be(0.5);
		}

		[U] public void Serialize_FieldValueFactor()
		{
			IScoreFunction fn = new FieldValueFactorFunction
			{
				Field = "popularity", Factor = 1.2, Modifier = FieldValueFactorModifier.Log1P, Missing = 0.5
			};
			var json = Serialize(fn);
			json.Should().Be(@"{""field_value_factor"":{""field"":""popularity"",""factor"":1.2,""modifier"":""log1p"",""missing"":0.5}}");
		}

		// --- Dispatch: random_score ---

		[U] public void Deserialize_RandomScore_WithSeedAndField()
		{
			var fn = Deserialize(@"{""random_score"":{""seed"":42,""field"":""_seq_no""}}");
			fn.Should().BeOfType<RandomScoreFunction>();
			var rsf = (IRandomScoreFunction)fn;
			rsf.Field.Name.Should().Be("_seq_no");
			rsf.Seed.Should().NotBeNull();
		}

		[U] public void Serialize_RandomScore_SeedLong()
		{
			IScoreFunction fn = new RandomScoreFunction { Seed = 42, Field = "_seq_no" };
			var json = Serialize(fn);
			json.Should().Be(@"{""random_score"":{""seed"":42,""field"":""_seq_no""}}");
		}

		[U] public void Serialize_RandomScore_SeedString()
		{
			IScoreFunction fn = new RandomScoreFunction { Seed = "my-seed" };
			var json = Serialize(fn);
			json.Should().Be(@"{""random_score"":{""seed"":""my-seed""}}");
		}

		// --- Dispatch: script_score ---

		[U] public void Deserialize_ScriptScore()
		{
			var fn = Deserialize(@"{""script_score"":{""script"":{""source"":""doc['x'].value""}}}");
			fn.Should().BeOfType<ScriptScoreFunction>();
			var ssf = (IScriptScoreFunction)fn;
			((IInlineScript)ssf.Script).Source.Should().Be("doc['x'].value");
		}

		[U] public void Serialize_ScriptScore()
		{
			IScoreFunction fn = new ScriptScoreFunction { Script = new InlineScript("doc['x'].value") };
			var json = Serialize(fn);
			json.Should().Contain(@"""script_score""").And.Contain(@"""source"":""doc['x'].value""");
		}

		// --- Dispatch: weight-only WeightFunction ---

		[U] public void Deserialize_WeightOnly_ReturnsWeightFunction()
		{
			var fn = Deserialize(@"{""weight"":2.0}");
			fn.Should().BeOfType<WeightFunction>();
			fn.Weight.Should().Be(2.0);
		}

		[U] public void Serialize_WeightFunction()
		{
			IScoreFunction fn = new WeightFunction { Weight = 2.0 };
			var json = Serialize(fn);
			json.Should().Be(@"{""weight"":2}");
		}

		// --- Dispatch: numeric decay (gauss/linear/exp) ---

		[U] public void Deserialize_GaussNumericDecay()
		{
			var fn = Deserialize(@"{""gauss"":{""age"":{""origin"":0,""scale"":10,""offset"":2,""decay"":0.33},""multi_value_mode"":""avg""}}");
			fn.Should().BeOfType<GaussDecayFunction>();
			var decay = (IDecayFunction<double?, double?>)fn;
			decay.Field.Name.Should().Be("age");
			decay.Origin.Should().Be(0);
			decay.Scale.Should().Be(10);
			decay.Offset.Should().Be(2);
			((IDecayFunction)decay).Decay.Should().Be(0.33);
			((IDecayFunction)decay).MultiValueMode.Should().Be(MultiValueMode.Average);
		}

		[U] public void Serialize_LinearNumericDecay()
		{
			IScoreFunction fn = new LinearDecayFunction
			{
				Field = "age", Origin = 0, Scale = 10, Offset = 2, Decay = 0.33, MultiValueMode = MultiValueMode.Average
			};
			var json = Serialize(fn);
			json.Should().Be(@"{""linear"":{""age"":{""origin"":0,""scale"":10,""offset"":2,""decay"":0.33},""multi_value_mode"":""avg""}}");
		}

		[U] public void Deserialize_ExpNumericDecay()
		{
			var fn = Deserialize(@"{""exp"":{""age"":{""origin"":5,""scale"":1}}}");
			fn.Should().BeOfType<ExponentialDecayFunction>();
			var decay = (IDecayFunction<double?, double?>)fn;
			decay.Field.Name.Should().Be("age");
			decay.Origin.Should().Be(5);
			decay.Scale.Should().Be(1);
		}

		// --- Dispatch: date decay (string origin) ---

		[U] public void Deserialize_GaussDateDecay_ByStringOrigin()
		{
			var fn = Deserialize(@"{""gauss"":{""date"":{""origin"":""2020-01-01"",""scale"":""10d"",""offset"":""2d""}}}");
			fn.Should().BeOfType<GaussDateDecayFunction>();
			var decay = (IDecayFunction<DateMath, Time>)fn;
			decay.Field.Name.Should().Be("date");
			decay.Scale.Should().NotBeNull();
		}

		// --- Dispatch: geo decay (object origin) ---

		[U] public void Deserialize_GaussGeoDecay_ByObjectOrigin()
		{
			var fn = Deserialize(@"{""gauss"":{""location"":{""origin"":{""lat"":40.0,""lon"":-70.0},""scale"":""1km""}}}");
			fn.Should().BeOfType<GaussGeoDecayFunction>();
			var decay = (IDecayFunction<GeoLocation, Distance>)fn;
			decay.Field.Name.Should().Be("location");
			decay.Scale.Should().NotBeNull();
		}

		// --- Common fields co-existing with a function body ---

		[U] public void Deserialize_CommonWeight_CoexistsWithFunctionBody()
		{
			var fn = Deserialize(@"{""field_value_factor"":{""field"":""popularity""},""weight"":3.0}");
			fn.Should().BeOfType<FieldValueFactorFunction>();
			fn.Weight.Should().Be(3.0);
		}

		[U] public void Serialize_FieldValueFactor_WithWeight()
		{
			IScoreFunction fn = new FieldValueFactorFunction { Field = "popularity", Weight = 3.0 };
			var json = Serialize(fn);
			json.Should().Be(@"{""field_value_factor"":{""field"":""popularity""},""weight"":3}");
		}

		// --- null ---

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Deserialize_EmptyObject_ReturnsNull()
		{
			// No function body and no weight => null, matching the legacy formatter.
			Deserialize("{}").Should().BeNull();
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<IScoreFunction>(null, Options()).Should().Be("null");
		}

		// --- Round-trip ---

		[U] public void RoundTrip_FieldValueFactor()
		{
			IScoreFunction original = new FieldValueFactorFunction { Field = "popularity", Factor = 1.2, Weight = 2.0 };
			var back = Deserialize(Serialize(original));
			back.Should().BeOfType<FieldValueFactorFunction>();
			var fvf = (IFieldValueFactorFunction)back;
			fvf.Field.Name.Should().Be("popularity");
			fvf.Factor.Should().Be(1.2);
			back.Weight.Should().Be(2.0);
		}

		[U] public void RoundTrip_NumericDecay()
		{
			IScoreFunction original = new GaussDecayFunction { Field = "age", Origin = 0, Scale = 10, Decay = 0.5 };
			var back = Deserialize(Serialize(original));
			back.Should().BeOfType<GaussDecayFunction>();
			var decay = (IDecayFunction<double?, double?>)back;
			decay.Origin.Should().Be(0);
			decay.Scale.Should().Be(10);
		}
	}
}
