/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ScoreFunctionJsonFormatter</c>.
	///
	/// <see cref="IScoreFunction"/> is polymorphic. A single score-function object carries an optional common
	/// <c>filter</c> (a <see cref="QueryContainer"/>) and an optional common <c>weight</c>, plus at most one
	/// function body identified by a well-known key:
	/// <list type="bullet">
	/// <item><description><c>exp</c> / <c>gauss</c> / <c>linear</c> — a decay function. The key names the decay type;
	/// the single inner key (other than <c>multi_value_mode</c>) is the field, and the field body's <c>origin</c>
	/// token selects the numeric / date / geo variant (a string origin ⇒ date, an object origin ⇒ geo, otherwise
	/// numeric);</description></item>
	/// <item><description><c>random_score</c> ⇒ <see cref="RandomScoreFunction"/>;</description></item>
	/// <item><description><c>field_value_factor</c> ⇒ <see cref="FieldValueFactorFunction"/>;</description></item>
	/// <item><description><c>script_score</c> ⇒ <see cref="ScriptScoreFunction"/>;</description></item>
	/// <item><description>no function body but a <c>weight</c> present ⇒ <see cref="WeightFunction"/>.</description></item>
	/// </list>
	/// <see cref="Utf8JsonReader"/> is forward-only, so — unlike the Utf8Json version which peeked at byte segments —
	/// we buffer the value into a <see cref="JsonDocument"/>, inspect which known key(s) are present to choose the
	/// concrete type, then <see cref="JsonSerializer"/>.Deserialize the relevant portion so no members are dropped.
	/// On write we dispatch on the runtime type exactly as the legacy Serialize switch did, resolving decay/field
	/// factor field names through the runtime <c>Inferrer</c> (hence <see cref="SettingsAwareConverter{T}"/>).
	/// </summary>
	internal class ScoreFunctionConverter : SettingsAwareConverter<IScoreFunction>
	{
		public ScoreFunctionConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IScoreFunction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			QueryContainer filter = null;
			double? weight = null;
			IScoreFunction function = null;

			foreach (var property in root.EnumerateObject())
			{
				switch (property.Name)
				{
					case "filter":
						filter = JsonSerializer.Deserialize<QueryContainer>(property.Value.GetRawText(), options);
						break;
					case "weight":
						weight = property.Value.GetDouble();
						break;
					case "exp":
					case "gauss":
					case "linear":
						function = ReadDecay(property.Name, property.Value, options);
						break;
					case "random_score":
						function = JsonSerializer.Deserialize<RandomScoreFunction>(property.Value.GetRawText(), options);
						break;
					case "field_value_factor":
						function = JsonSerializer.Deserialize<FieldValueFactorFunction>(property.Value.GetRawText(), options);
						break;
					case "script_score":
						function = JsonSerializer.Deserialize<ScriptScoreFunction>(property.Value.GetRawText(), options);
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

		private static IScoreFunction ReadDecay(string type, JsonElement element, JsonSerializerOptions options)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			MultiValueMode? multiValueMode = null;
			string field = null;
			var fieldBody = default(JsonElement);
			var haveField = false;

			foreach (var property in element.EnumerateObject())
			{
				if (property.Name == "multi_value_mode")
					multiValueMode = JsonSerializer.Deserialize<MultiValueMode>(property.Value.GetRawText(), options);
				else
				{
					field = property.Name;
					fieldBody = property.Value;
					haveField = true;
				}
			}

			if (!haveField)
				return null;

			var decayFunction = DeserializeDecay(type, fieldBody, options);
			if (decayFunction == null)
				return null;

			decayFunction.Field = field;
			decayFunction.MultiValueMode = multiValueMode;
			return decayFunction;
		}

		private static IDecayFunction DeserializeDecay(string type, JsonElement fieldBody, JsonSerializerOptions options)
		{
			// The origin token discriminates the variant, mirroring the legacy ReadDecayFunction:
			// a string origin => date, an object origin => geo, otherwise numeric.
			var subType = "numeric";
			if (fieldBody.ValueKind == JsonValueKind.Object && fieldBody.TryGetProperty("origin", out var origin))
			{
				switch (origin.ValueKind)
				{
					case JsonValueKind.String:
						subType = "date";
						break;
					case JsonValueKind.Object:
						subType = "geo";
						break;
				}
			}

			var raw = fieldBody.GetRawText();
			switch (type)
			{
				case "exp":
					switch (subType)
					{
						case "numeric": return JsonSerializer.Deserialize<ExponentialDecayFunction>(raw, options);
						case "date": return JsonSerializer.Deserialize<ExponentialDateDecayFunction>(raw, options);
						case "geo": return JsonSerializer.Deserialize<ExponentialGeoDecayFunction>(raw, options);
						default: return null;
					}
				case "gauss":
					switch (subType)
					{
						case "numeric": return JsonSerializer.Deserialize<GaussDecayFunction>(raw, options);
						case "date": return JsonSerializer.Deserialize<GaussDateDecayFunction>(raw, options);
						case "geo": return JsonSerializer.Deserialize<GaussGeoDecayFunction>(raw, options);
						default: return null;
					}
				case "linear":
					switch (subType)
					{
						case "numeric": return JsonSerializer.Deserialize<LinearDecayFunction>(raw, options);
						case "date": return JsonSerializer.Deserialize<LinearDateDecayFunction>(raw, options);
						case "geo": return JsonSerializer.Deserialize<LinearGeoDecayFunction>(raw, options);
						default: return null;
					}
				default: return null;
			}
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
				case IDecayFunction decayFunction:
					WriteDecay(writer, decayFunction, options);
					break;
				case IFieldValueFactorFunction fieldValueFactorFunction:
					WriteFieldValueFactor(writer, fieldValueFactorFunction, options);
					break;
				case IRandomScoreFunction randomScoreFunction:
					WriteRandomScore(writer, randomScoreFunction, options);
					break;
				case IScriptScoreFunction scriptScoreFunction:
					WriteScriptScore(writer, scriptScoreFunction, options);
					break;
				case IWeightFunction _:
					break;
				default:
					throw new Exception($"Can not write function score json for {value.GetType().Name}");
			}

			if (value.Weight.HasValue)
			{
				writer.WritePropertyName("weight");
				OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, value.Weight.Value);
			}

			writer.WriteEndObject();
		}

		private static void WriteScriptScore(Utf8JsonWriter writer, IScriptScoreFunction value, JsonSerializerOptions options)
		{
			writer.WritePropertyName("script_score");
			writer.WriteStartObject();
			writer.WritePropertyName("script");
			JsonSerializer.Serialize(writer, value?.Script, options);
			writer.WriteEndObject();
		}

		private static void WriteRandomScore(Utf8JsonWriter writer, IRandomScoreFunction value, JsonSerializerOptions options)
		{
			writer.WritePropertyName("random_score");
			writer.WriteStartObject();

			if (value.Seed != null)
			{
				writer.WritePropertyName("seed");
				WriteSeed(writer, value.Seed);
			}

			if (value.Field != null)
			{
				writer.WritePropertyName("field");
				JsonSerializer.Serialize(writer, value.Field, options);
			}

			writer.WriteEndObject();
		}

		// Seed is a Union<long, string>; the legacy engine delegated to the Union<long, string> formatter. That
		// formatter is not registered as a global STJ converter, so we write the encapsulated value directly by tag
		// (tag 0 => long, tag 1 => string) to reproduce the exact output.
		private static void WriteSeed(Utf8JsonWriter writer, Union<long, string> seed)
		{
			switch (seed.Tag)
			{
				case 0:
					writer.WriteNumberValue(seed.Item1);
					break;
				case 1:
					writer.WriteStringValue(seed.Item2);
					break;
				default:
					throw new Exception($"Unrecognized tag value: {seed.Tag}");
			}
		}

		private void WriteFieldValueFactor(Utf8JsonWriter writer, IFieldValueFactorFunction value, JsonSerializerOptions options)
		{
			writer.WritePropertyName("field_value_factor");
			writer.WriteStartObject();

			writer.WritePropertyName("field");
			writer.WriteStringValue(Settings.Inferrer.Field(value.Field));

			if (value.Factor.HasValue)
			{
				writer.WritePropertyName("factor");
				OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, value.Factor.Value);
			}

			if (value.Modifier.HasValue)
			{
				writer.WritePropertyName("modifier");
				JsonSerializer.Serialize(writer, value.Modifier.Value, options);
			}

			if (value.Missing.HasValue)
			{
				writer.WritePropertyName("missing");
				OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, value.Missing.Value);
			}

			writer.WriteEndObject();
		}

		private void WriteDecay(Utf8JsonWriter writer, IDecayFunction decay, JsonSerializerOptions options)
		{
			writer.WritePropertyName(decay.DecayType);
			writer.WriteStartObject();

			writer.WritePropertyName(Settings.Inferrer.Field(decay.Field));
			writer.WriteStartObject();

			switch (decay)
			{
				case IDecayFunction<double?, double?> numericDecay:
					WriteNumericDecay(writer, numericDecay);
					break;
				case IDecayFunction<DateMath, Time> dateDecay:
					WriteDateDecay(writer, dateDecay, options);
					break;
				case IDecayFunction<GeoLocation, Distance> geoDecay:
					WriteGeoDecay(writer, geoDecay, options);
					break;
				default:
					throw new Exception($"Can not write decay function json for {decay.GetType().Name}");
			}

			if (decay.Decay.HasValue)
			{
				writer.WritePropertyName("decay");
				OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, decay.Decay.Value);
			}

			writer.WriteEndObject();

			if (decay.MultiValueMode.HasValue)
			{
				writer.WritePropertyName("multi_value_mode");
				JsonSerializer.Serialize(writer, decay.MultiValueMode.Value, options);
			}

			writer.WriteEndObject();
		}

		private static void WriteNumericDecay(Utf8JsonWriter writer, IDecayFunction<double?, double?> value)
		{
			if (value.Origin.HasValue)
			{
				writer.WritePropertyName("origin");
				OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, value.Origin.Value);
			}

			if (value.Scale.HasValue)
			{
				writer.WritePropertyName("scale");
				OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, value.Scale.Value);
			}

			if (value.Offset != null)
			{
				writer.WritePropertyName("offset");
				OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, value.Offset.Value);
			}
		}

		private static void WriteDateDecay(Utf8JsonWriter writer, IDecayFunction<DateMath, Time> value, JsonSerializerOptions options)
		{
			if (value == null || value.Field.IsConditionless())
				return;

			if (value.Origin != null)
			{
				writer.WritePropertyName("origin");
				JsonSerializer.Serialize(writer, value.Origin, options);
			}

			writer.WritePropertyName("scale");
			JsonSerializer.Serialize(writer, value.Scale, options);

			if (value.Offset != null)
			{
				writer.WritePropertyName("offset");
				JsonSerializer.Serialize(writer, value.Offset, options);
			}
		}

		private static void WriteGeoDecay(Utf8JsonWriter writer, IDecayFunction<GeoLocation, Distance> value, JsonSerializerOptions options)
		{
			if (value == null || value.Field.IsConditionless())
				return;

			writer.WritePropertyName("origin");
			JsonSerializer.Serialize(writer, value.Origin, options);

			writer.WritePropertyName("scale");
			JsonSerializer.Serialize(writer, value.Scale, options);

			if (value.Offset != null)
			{
				writer.WritePropertyName("offset");
				JsonSerializer.Serialize(writer, value.Offset, options);
			}
		}
	}
}
