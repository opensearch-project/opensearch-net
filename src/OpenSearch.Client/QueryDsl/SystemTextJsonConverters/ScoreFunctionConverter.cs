/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IScoreFunction"/>.
	/// Polymorphic: WeightFunction, ScriptScoreFunction, FieldValueFactorFunction,
	/// RandomScoreFunction, DecayFunctions (linear, exp, gauss).
	/// </summary>
	internal sealed class ScoreFunctionConverter : JsonConverter<IScoreFunction>
	{
		public override IScoreFunction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IScoreFunction but got {reader.TokenType}");

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			QueryContainer filter = null;
			double? weight = null;
			IScoreFunction function = null;

			foreach (var prop in root.EnumerateObject())
			{
				switch (prop.Name)
				{
					case "filter":
						filter = JsonSerializer.Deserialize<QueryContainer>(prop.Value.GetRawText(), options);
						break;
					case "weight":
						weight = prop.Value.GetDouble();
						break;
					case "script_score":
						function = ReadScriptScore(prop.Value, options);
						break;
					case "field_value_factor":
						function = ReadFieldValueFactor(prop.Value);
						break;
					case "random_score":
						function = ReadRandomScore(prop.Value);
						break;
					case "exp":
					case "gauss":
					case "linear":
						function = ReadDecay(prop.Name, prop.Value, options);
						break;
				}
			}

			if (function == null)
			{
				if (weight.HasValue)
					function = new WeightFunction();
				else
					return null;
			}

			function.Weight = weight;
			function.Filter = filter;
			return function;
		}

		public override void Write(Utf8JsonWriter writer, IScoreFunction value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.Filter != null)
			{
				writer.WritePropertyName("filter");
				JsonSerializer.Serialize(writer, value.Filter, options);
			}

			switch (value)
			{
				case IDecayFunction decay:
					WriteDecay(writer, decay, options);
					break;
				case IFieldValueFactorFunction fvf:
					WriteFieldValueFactor(writer, fvf);
					break;
				case IRandomScoreFunction random:
					WriteRandomScore(writer, random);
					break;
				case IScriptScoreFunction script:
					WriteScriptScore(writer, script, options);
					break;
			}

			if (value.Weight.HasValue)
			{
				writer.WritePropertyName("weight");
				writer.WriteNumberValue(value.Weight.Value);
			}

			writer.WriteEndObject();
		}

		private static IScoreFunction ReadScriptScore(JsonElement element, JsonSerializerOptions options)
		{
			var fn = new ScriptScoreFunction();
			if (element.TryGetProperty("script", out var s))
				fn.Script = JsonSerializer.Deserialize<IScript>(s.GetRawText(), options);
			return fn;
		}

		private static IScoreFunction ReadFieldValueFactor(JsonElement element)
		{
			var fn = new FieldValueFactorFunction();
			if (element.TryGetProperty("field", out var f))
				fn.Field = new Field(f.GetString());
			if (element.TryGetProperty("factor", out var fac))
				fn.Factor = fac.GetDouble();
			if (element.TryGetProperty("modifier", out var m))
				fn.Modifier = ParseModifier(m.GetString());
			if (element.TryGetProperty("missing", out var mi))
				fn.Missing = mi.GetDouble();
			return fn;
		}

		private static IScoreFunction ReadRandomScore(JsonElement element)
		{
			var fn = new RandomScoreFunction();
			if (element.TryGetProperty("seed", out var seed))
			{
				if (seed.ValueKind == JsonValueKind.Number)
					fn.Seed = new Union<long, string>(seed.GetInt64());
				else
					fn.Seed = new Union<long, string>(seed.GetString());
			}
			if (element.TryGetProperty("field", out var f))
				fn.Field = new Field(f.GetString());
			return fn;
		}

		private static IScoreFunction ReadDecay(string decayType, JsonElement element, JsonSerializerOptions options)
		{
			string fieldName = null;
			MultiValueMode? multiValueMode = null;
			JsonElement fieldBody = default;

			foreach (var prop in element.EnumerateObject())
			{
				if (prop.Name == "multi_value_mode")
					multiValueMode = ParseMultiValueMode(prop.Value.GetString());
				else
				{
					fieldName = prop.Name;
					fieldBody = prop.Value;
				}
			}

			if (fieldName == null || fieldBody.ValueKind == JsonValueKind.Undefined)
				return null;

			var subType = "numeric";
			if (fieldBody.TryGetProperty("origin", out var origin))
			{
				if (origin.ValueKind == JsonValueKind.String) subType = "date";
				else if (origin.ValueKind == JsonValueKind.Object) subType = "geo";
			}

			IDecayFunction decay = CreateDecay(decayType, subType, fieldBody, options);
			if (decay != null)
			{
				decay.Field = new Field(fieldName);
				decay.MultiValueMode = multiValueMode;
			}
			return decay;
		}

		private static IDecayFunction CreateDecay(string type, string subType, JsonElement body, JsonSerializerOptions options)
		{
			return (type, subType) switch
			{
				("exp", "numeric") => ReadNumericDecay<ExponentialDecayFunction>(body),
				("exp", "date") => ReadDateDecay<ExponentialDateDecayFunction>(body),
				("exp", "geo") => ReadGeoDecay<ExponentialGeoDecayFunction>(body, options),
				("gauss", "numeric") => ReadNumericDecay<GaussDecayFunction>(body),
				("gauss", "date") => ReadDateDecay<GaussDateDecayFunction>(body),
				("gauss", "geo") => ReadGeoDecay<GaussGeoDecayFunction>(body, options),
				("linear", "numeric") => ReadNumericDecay<LinearDecayFunction>(body),
				("linear", "date") => ReadDateDecay<LinearDateDecayFunction>(body),
				("linear", "geo") => ReadGeoDecay<LinearGeoDecayFunction>(body, options),
				_ => null
			};
		}

		private static T ReadNumericDecay<T>(JsonElement el) where T : DecayFunctionBase<double?, double?>, new()
		{
			var fn = new T();
			if (el.TryGetProperty("origin", out var o)) fn.Origin = o.GetDouble();
			if (el.TryGetProperty("scale", out var s)) fn.Scale = s.GetDouble();
			if (el.TryGetProperty("offset", out var off)) fn.Offset = off.GetDouble();
			if (el.TryGetProperty("decay", out var d)) fn.Decay = d.GetDouble();
			return fn;
		}

		private static T ReadDateDecay<T>(JsonElement el) where T : DecayFunctionBase<DateMath, Time>, new()
		{
			var fn = new T();
			if (el.TryGetProperty("origin", out var o)) fn.Origin = DateMath.FromString(o.GetString());
			if (el.TryGetProperty("scale", out var s)) fn.Scale = new Time(s.GetString());
			if (el.TryGetProperty("offset", out var off)) fn.Offset = new Time(off.GetString());
			if (el.TryGetProperty("decay", out var d)) fn.Decay = d.GetDouble();
			return fn;
		}

		private static T ReadGeoDecay<T>(JsonElement el, JsonSerializerOptions options)
			where T : DecayFunctionBase<GeoLocation, Distance>, new()
		{
			var fn = new T();
			if (el.TryGetProperty("origin", out var o))
			{
				var raw = o.GetRawText();
				var r2 = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(raw));
				r2.Read();
				fn.Origin = new GeoLocationConverter().Read(ref r2, typeof(GeoLocation), options);
			}
			if (el.TryGetProperty("scale", out var s)) fn.Scale = s.GetString();
			if (el.TryGetProperty("offset", out var off)) fn.Offset = off.GetString();
			if (el.TryGetProperty("decay", out var d)) fn.Decay = d.GetDouble();
			return fn;
		}

		private static void WriteDecay(Utf8JsonWriter writer, IDecayFunction decay, JsonSerializerOptions options)
		{
			writer.WritePropertyName(decay.DecayType);
			writer.WriteStartObject();

			var fieldName = decay.Field?.ToString();
			if (!string.IsNullOrEmpty(fieldName))
			{
				writer.WritePropertyName(fieldName);
				writer.WriteStartObject();

				switch (decay)
				{
					case IDecayFunction<double?, double?> n:
						if (n.Origin.HasValue) { writer.WritePropertyName("origin"); writer.WriteNumberValue(n.Origin.Value); }
						if (n.Scale.HasValue) { writer.WritePropertyName("scale"); writer.WriteNumberValue(n.Scale.Value); }
						if (n.Offset.HasValue) { writer.WritePropertyName("offset"); writer.WriteNumberValue(n.Offset.Value); }
						break;
					case IDecayFunction<DateMath, Time> d:
						if (d.Origin != null) { writer.WritePropertyName("origin"); writer.WriteStringValue(d.Origin.ToString()); }
						if (d.Scale != null) { writer.WritePropertyName("scale"); writer.WriteStringValue(d.Scale.ToString()); }
						if (d.Offset != null) { writer.WritePropertyName("offset"); writer.WriteStringValue(d.Offset.ToString()); }
						break;
					case IDecayFunction<GeoLocation, Distance> g:
						if (g.Origin != null) { writer.WritePropertyName("origin"); new GeoLocationConverter().Write(writer, g.Origin, options); }
						if (g.Scale != null) { writer.WritePropertyName("scale"); writer.WriteStringValue(g.Scale.ToString()); }
						if (g.Offset != null) { writer.WritePropertyName("offset"); writer.WriteStringValue(g.Offset.ToString()); }
						break;
				}

				if (decay.Decay.HasValue) { writer.WritePropertyName("decay"); writer.WriteNumberValue(decay.Decay.Value); }
				writer.WriteEndObject();
			}

			if (decay.MultiValueMode.HasValue)
			{
				writer.WritePropertyName("multi_value_mode");
				writer.WriteStringValue(GetMultiValueModeString(decay.MultiValueMode.Value));
			}

			writer.WriteEndObject();
		}

		private static void WriteFieldValueFactor(Utf8JsonWriter writer, IFieldValueFactorFunction value)
		{
			writer.WritePropertyName("field_value_factor");
			writer.WriteStartObject();
			if (value.Field != null) { writer.WritePropertyName("field"); writer.WriteStringValue(value.Field.ToString()); }
			if (value.Factor.HasValue) { writer.WritePropertyName("factor"); writer.WriteNumberValue(value.Factor.Value); }
			if (value.Modifier.HasValue) { writer.WritePropertyName("modifier"); writer.WriteStringValue(GetModifierString(value.Modifier.Value)); }
			if (value.Missing.HasValue) { writer.WritePropertyName("missing"); writer.WriteNumberValue(value.Missing.Value); }
			writer.WriteEndObject();
		}

		private static void WriteRandomScore(Utf8JsonWriter writer, IRandomScoreFunction value)
		{
			writer.WritePropertyName("random_score");
			writer.WriteStartObject();
			if (value.Seed != null)
			{
				writer.WritePropertyName("seed");
				if (value.Seed.Tag == 0) writer.WriteNumberValue(value.Seed.Item1);
				else writer.WriteStringValue(value.Seed.Item2);
			}
			if (value.Field != null) { writer.WritePropertyName("field"); writer.WriteStringValue(value.Field.ToString()); }
			writer.WriteEndObject();
		}

		private static void WriteScriptScore(Utf8JsonWriter writer, IScriptScoreFunction value, JsonSerializerOptions options)
		{
			writer.WritePropertyName("script_score");
			writer.WriteStartObject();
			if (value.Script != null) { writer.WritePropertyName("script"); JsonSerializer.Serialize(writer, value.Script, options); }
			writer.WriteEndObject();
		}

		private static FieldValueFactorModifier? ParseModifier(string v) => v?.ToLowerInvariant() switch
		{
			"none" => FieldValueFactorModifier.None, "log" => FieldValueFactorModifier.Log,
			"log1p" => FieldValueFactorModifier.Log1P, "log2p" => FieldValueFactorModifier.Log2P,
			"ln" => FieldValueFactorModifier.Ln, "ln1p" => FieldValueFactorModifier.Ln1P,
			"ln2p" => FieldValueFactorModifier.Ln2P, "square" => FieldValueFactorModifier.Square,
			"sqrt" => FieldValueFactorModifier.SquareRoot, "reciprocal" => FieldValueFactorModifier.Reciprocal,
			_ => null
		};

		private static string GetModifierString(FieldValueFactorModifier v) => v switch
		{
			FieldValueFactorModifier.None => "none", FieldValueFactorModifier.Log => "log",
			FieldValueFactorModifier.Log1P => "log1p", FieldValueFactorModifier.Log2P => "log2p",
			FieldValueFactorModifier.Ln => "ln", FieldValueFactorModifier.Ln1P => "ln1p",
			FieldValueFactorModifier.Ln2P => "ln2p", FieldValueFactorModifier.Square => "square",
			FieldValueFactorModifier.SquareRoot => "sqrt", FieldValueFactorModifier.Reciprocal => "reciprocal",
			_ => v.ToString().ToLowerInvariant()
		};

		private static MultiValueMode? ParseMultiValueMode(string v) => v?.ToLowerInvariant() switch
		{
			"min" => MultiValueMode.Min, "max" => MultiValueMode.Max,
			"avg" => MultiValueMode.Average, "sum" => MultiValueMode.Sum, _ => null
		};

		private static string GetMultiValueModeString(MultiValueMode v) => v switch
		{
			MultiValueMode.Min => "min", MultiValueMode.Max => "max",
			MultiValueMode.Average => "avg", MultiValueMode.Sum => "sum",
			_ => v.ToString().ToLowerInvariant()
		};
	}
}
