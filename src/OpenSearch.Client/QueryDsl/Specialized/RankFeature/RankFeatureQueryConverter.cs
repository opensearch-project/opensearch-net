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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IRankFeatureQuery"/>, replacing the
	/// vendored Utf8Json formatter as part of #388. The <c>field</c> is written as a value (not a key)
	/// alongside <c>_name</c>/<c>boost</c> and, optionally, one of the polymorphic scoring functions
	/// (<c>saturation</c>/<c>log</c>/<c>sigmoid</c>/<c>linear</c>). Constructed with the connection
	/// settings for field-name inference.
	/// </summary>
	internal sealed class RankFeatureQueryConverter : JsonConverter<IRankFeatureQuery>
	{
		private readonly IConnectionSettingsValues _settings;

		public RankFeatureQueryConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

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
			{
				writer.WritePropertyName("boost");
				JsonSerializer.Serialize(writer, value.Boost.Value, options);
			}

			writer.WriteString("field", value.Field == null ? null : _settings.Inferrer.Field(value.Field));

			switch (value.Function)
			{
				case IRankFeatureSigmoidFunction sigmoid:
					WriteFunction(writer, "sigmoid", sigmoid, options);
					break;
				case IRankFeatureSaturationFunction saturation:
					WriteFunction(writer, "saturation", saturation, options);
					break;
				case IRankFeatureLogarithmFunction logarithm:
					WriteFunction(writer, "log", logarithm, options);
					break;
				case IRankFeatureLinearFunction linear:
					WriteFunction(writer, "linear", linear, options);
					break;
			}

			writer.WriteEndObject();
		}

		private static void WriteFunction(Utf8JsonWriter writer, string name, IRankFeatureFunction function, JsonSerializerOptions options)
		{
			writer.WritePropertyName(name);
			JsonSerializer.Serialize(writer, function, function.GetType(), options);
		}

		public override IRankFeatureQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var query = new RankFeatureQuery();
			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "_name":
						query.Name = member.Value.GetString();
						break;
					case "boost":
						query.Boost = member.Value.GetDouble();
						break;
					case "field":
						query.Field = member.Value.GetString();
						break;
					case "saturation":
						query.Function = member.Value.Deserialize<RankFeatureSaturationFunction>(options);
						break;
					case "log":
						query.Function = member.Value.Deserialize<RankFeatureLogarithmFunction>(options);
						break;
					case "sigmoid":
						query.Function = member.Value.Deserialize<RankFeatureSigmoidFunction>(options);
						break;
					case "linear":
						query.Function = member.Value.Deserialize<RankFeatureLinearFunction>(options);
						break;
				}
			}

			return query;
		}
	}
}
