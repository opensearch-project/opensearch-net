/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>RankFeatureQueryFormatter</c>.
	///
	/// A rank-feature query serializes as a flat object with literal keys: the common <c>_name</c>/<c>boost</c> from
	/// <c>QueryBase</c>, a <c>field</c> (delegated to the registered settings-aware <see cref="FieldConverter"/>), and at
	/// most one function sub-object identified by its key. The function is polymorphic (<see cref="IRankFeatureFunction"/>)
	/// and dispatched by which key is present:
	/// <list type="bullet">
	/// <item><description><c>saturation</c> ⇒ <see cref="RankFeatureSaturationFunction"/>;</description></item>
	/// <item><description><c>log</c> ⇒ <see cref="RankFeatureLogarithmFunction"/>;</description></item>
	/// <item><description><c>sigmoid</c> ⇒ <see cref="RankFeatureSigmoidFunction"/>;</description></item>
	/// <item><description><c>linear</c> ⇒ <see cref="RankFeatureLinearFunction"/>.</description></item>
	/// </list>
	/// On write the legacy formatter dispatched on the runtime function type (sigmoid, then saturation, then log, then
	/// linear) and delegated the sub-object body to the function's formatter; that order and delegation are preserved
	/// here. Because it reads/writes literal keys and delegates field resolution to the registered
	/// <see cref="FieldConverter"/>, this converter itself needs no settings.
	/// </summary>
	internal class RankFeatureQueryConverter : JsonConverter<IRankFeatureQuery>
	{
		public override IRankFeatureQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			var query = new RankFeatureQuery();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var property = reader.GetString();
				reader.Read(); // advance to value

				switch (property)
				{
					case "_name":
						query.Name = reader.GetString();
						break;
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "field":
						query.Field = JsonSerializer.Deserialize<Field>(ref reader, options);
						break;
					case "saturation":
						query.Function = JsonSerializer.Deserialize<RankFeatureSaturationFunction>(ref reader, options);
						break;
					case "log":
						query.Function = JsonSerializer.Deserialize<RankFeatureLogarithmFunction>(ref reader, options);
						break;
					case "sigmoid":
						query.Function = JsonSerializer.Deserialize<RankFeatureSigmoidFunction>(ref reader, options);
						break;
					case "linear":
						query.Function = JsonSerializer.Deserialize<RankFeatureLinearFunction>(ref reader, options);
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, IRankFeatureQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (!string.IsNullOrEmpty(value.Name))
				writer.WriteString("_name", value.Name);

			if (value.Boost.HasValue)
				writer.WriteNumber("boost", value.Boost.Value);

			writer.WritePropertyName("field");
			JsonSerializer.Serialize(writer, value.Field, options);

			if (value.Function != null)
			{
				switch (value.Function)
				{
					case IRankFeatureSigmoidFunction sigmoid:
						WriteFunction(writer, "sigmoid", sigmoid, options);
						break;
					case IRankFeatureSaturationFunction saturation:
						WriteFunction(writer, "saturation", saturation, options);
						break;
					case IRankFeatureLogarithmFunction log:
						WriteFunction(writer, "log", log, options);
						break;
					case IRankFeatureLinearFunction linear:
						WriteFunction(writer, "linear", linear, options);
						break;
				}
			}

			writer.WriteEndObject();
		}

		private static void WriteFunction<TFunction>(Utf8JsonWriter writer, string name, TFunction function, JsonSerializerOptions options)
			where TFunction : IRankFeatureFunction
		{
			writer.WritePropertyName(name);
			JsonSerializer.Serialize(writer, function, options);
		}
	}
}
