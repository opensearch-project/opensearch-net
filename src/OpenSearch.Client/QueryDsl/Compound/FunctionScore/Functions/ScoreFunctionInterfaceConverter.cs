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
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="IScoreFunction"/>, replacing the
	/// vendored <c>ScoreFunctionJsonFormatter</c> Utf8Json formatter as part of #388. A score function
	/// is written as <c>{ "filter": &lt;QueryContainer&gt;, &lt;body&gt;, "weight": &lt;double&gt; }</c>
	/// where the body depends on the runtime function type:
	/// <list type="bullet">
	/// <item><see cref="IDecayFunction"/> → <c>{ "&lt;exp|gauss|linear&gt;": { "&lt;field&gt;": { origin/scale/offset/decay }, "multi_value_mode": … } }</c></item>
	/// <item><see cref="IFieldValueFactorFunction"/> → <c>"field_value_factor": { … }</c></item>
	/// <item><see cref="IRandomScoreFunction"/> → <c>"random_score": { … }</c></item>
	/// <item><see cref="IScriptScoreFunction"/> → <c>"script_score": { "script": &lt;IScript&gt; }</c></item>
	/// <item><see cref="IWeightFunction"/> → nothing extra</item>
	/// </list>
	/// The decay body has numeric/date/geo variants; reading sniffs the <c>origin</c> token kind to pick
	/// the concrete decay type. Constructed with the connection settings for field-name inference.
	/// </summary>
	internal sealed class ScoreFunctionInterfaceConverter : JsonConverter<IScoreFunction>
	{
		private readonly IConnectionSettingsValues _settings;

		public ScoreFunctionInterfaceConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

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
				// Route doubles through the registered DoubleFormatConverter so whole values keep a decimal
				// (e.g. 3.0 rather than 3), matching the vendored formatter.
				writer.WritePropertyName("weight");
				JsonSerializer.Serialize(writer, value.Weight.Value, options);
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
				JsonSerializer.Serialize(writer, value.Seed, options);
			}

			if (value.Field != null)
			{
				writer.WritePropertyName("field");
				JsonSerializer.Serialize(writer, value.Field, options);
			}
			writer.WriteEndObject();
		}

		private void WriteFieldValueFactor(Utf8JsonWriter writer, IFieldValueFactorFunction value, JsonSerializerOptions options)
		{
			writer.WritePropertyName("field_value_factor");
			writer.WriteStartObject();

			writer.WriteString("field", _settings.Inferrer.Field(value.Field));

			if (value.Factor.HasValue)
			{
				writer.WritePropertyName("factor");
				JsonSerializer.Serialize(writer, value.Factor.Value, options);
			}

			if (value.Modifier.HasValue)
			{
				writer.WritePropertyName("modifier");
				JsonSerializer.Serialize(writer, value.Modifier.Value, options);
			}

			if (value.Missing.HasValue)
			{
				writer.WritePropertyName("missing");
				JsonSerializer.Serialize(writer, value.Missing.Value, options);
			}

			writer.WriteEndObject();
		}

		private void WriteDecay(Utf8JsonWriter writer, IDecayFunction decay, JsonSerializerOptions options)
		{
			writer.WritePropertyName(decay.DecayType);
			writer.WriteStartObject();

			writer.WritePropertyName(_settings.Inferrer.Field(decay.Field));
			writer.WriteStartObject();

			switch (decay)
			{
				case IDecayFunction<double?, double?> numericDecay:
					WriteNumericDecay(writer, numericDecay, options);
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
				JsonSerializer.Serialize(writer, decay.Decay.Value, options);
			}

			writer.WriteEndObject();

			if (decay.MultiValueMode.HasValue)
			{
				writer.WritePropertyName("multi_value_mode");
				JsonSerializer.Serialize(writer, decay.MultiValueMode.Value, options);
			}

			writer.WriteEndObject();
		}

		private static void WriteNumericDecay(Utf8JsonWriter writer, IDecayFunction<double?, double?> value, JsonSerializerOptions options)
		{
			if (value.Origin.HasValue)
			{
				writer.WritePropertyName("origin");
				JsonSerializer.Serialize(writer, value.Origin.Value, options);
			}

			if (value.Scale.HasValue)
			{
				writer.WritePropertyName("scale");
				JsonSerializer.Serialize(writer, value.Scale.Value, options);
			}

			if (value.Offset.HasValue)
			{
				writer.WritePropertyName("offset");
				JsonSerializer.Serialize(writer, value.Offset.Value, options);
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

		public override IScoreFunction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object)
				return null;

			QueryContainer filter = null;
			double? weight = null;
			IScoreFunction function = null;

			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "filter":
						filter = member.Value.Deserialize<QueryContainer>(options);
						break;
					case "weight":
						weight = member.Value.GetDouble();
						break;
					case "exp":
					case "gauss":
					case "linear":
						function = ReadDecay(member.Name, member.Value, options);
						break;
					case "random_score":
						function = member.Value.Deserialize<RandomScoreFunction>(options);
						break;
					case "field_value_factor":
						function = member.Value.Deserialize<FieldValueFactorFunction>(options);
						break;
					case "script_score":
						function = member.Value.Deserialize<ScriptScoreFunction>(options);
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

		private static IDecayFunction ReadDecay(string decayType, JsonElement element, JsonSerializerOptions options)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			MultiValueMode? multiValueMode = null;
			string field = null;
			JsonElement body = default;
			var hasBody = false;

			foreach (var member in element.EnumerateObject())
			{
				if (member.Name == "multi_value_mode")
					multiValueMode = member.Value.Deserialize<MultiValueMode>(options);
				else
				{
					field = member.Name;
					body = member.Value;
					hasBody = true;
				}
			}

			if (!hasBody)
				return null;

			var decayFunction = ReadDecayFunction(decayType, body, options);
			if (decayFunction != null)
			{
				decayFunction.Field = field;
				decayFunction.MultiValueMode = multiValueMode;
			}

			return decayFunction;
		}

		private static IDecayFunction ReadDecayFunction(string type, JsonElement body, JsonSerializerOptions options)
		{
			var subType = "numeric";

			if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("origin", out var origin))
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

			switch (type)
			{
				case "exp":
					switch (subType)
					{
						case "numeric": return body.Deserialize<ExponentialDecayFunction>(options);
						case "date": return body.Deserialize<ExponentialDateDecayFunction>(options);
						case "geo": return body.Deserialize<ExponentialGeoDecayFunction>(options);
						default: return null;
					}
				case "gauss":
					switch (subType)
					{
						case "numeric": return body.Deserialize<GaussDecayFunction>(options);
						case "date": return body.Deserialize<GaussDateDecayFunction>(options);
						case "geo": return body.Deserialize<GaussGeoDecayFunction>(options);
						default: return null;
					}
				case "linear":
					switch (subType)
					{
						case "numeric": return body.Deserialize<LinearDecayFunction>(options);
						case "date": return body.Deserialize<LinearDateDecayFunction>(options);
						case "geo": return body.Deserialize<LinearGeoDecayFunction>(options);
						default: return null;
					}
				default: return null;
			}
		}
	}
}
