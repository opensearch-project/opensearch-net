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
	/// System.Text.Json replacement for the legacy Utf8Json <c>MovingAverageAggregationFormatter</c>.
	///
	/// An <see cref="IMovingAverageAggregation"/> carries an optional polymorphic moving-average model whose <c>model</c>
	/// name (<c>linear</c> / <c>simple</c> / <c>ewma</c> / <c>holt</c> / <c>holt_winters</c>) and separate <c>settings</c>
	/// object are two distinct wire properties. The legacy formatter read the raw <c>settings</c> segment and the
	/// <c>model</c> name independently and, after the loop, parsed the settings with the model type selected by the name.
	/// <see cref="Utf8JsonReader"/> is forward-only and cannot rewind, so we buffer the object into a
	/// <see cref="JsonDocument"/> and inspect the DOM, reproducing that name→settings pairing regardless of property
	/// order.
	///
	/// <para>Read maps: <c>format</c>, <c>gap_policy</c>, <c>minimize</c>, <c>predict</c>, <c>window</c>, and a
	/// <c>buckets_path</c> string (wrapped in a <see cref="SingleBucketsPath"/> when non-empty, matching the legacy
	/// case which only handled the single-path string form).</para>
	///
	/// <para>Write emits (in the legacy order) <c>buckets_path</c>, <c>gap_policy</c> (as its <c>GetStringValue()</c>
	/// wire form), <c>format</c>, <c>window</c>, <c>minimize</c>, <c>predict</c>, then — when a model is present — the
	/// <c>model</c> name followed by a <c>settings</c> object serialized from the model's concrete runtime type
	/// (empty <c>{}</c> for the field-less linear/simple models).</para>
	///
	/// The legacy formatter did not use connection settings, so this is a plain (non settings-aware) converter.
	/// </summary>
	internal class MovingAverageAggregationConverter : JsonConverter<IMovingAverageAggregation>
	{
		public override IMovingAverageAggregation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var aggregation = new MovingAverageAggregation();
			string modelName = null;
			JsonElement settings = default;
			var haveSettings = false;

			foreach (var property in root.EnumerateObject())
			{
				switch (property.Name)
				{
					case "format":
						aggregation.Format = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
						break;
					case "gap_policy":
						aggregation.GapPolicy = JsonSerializer.Deserialize<GapPolicy?>(property.Value.GetRawText(), options);
						break;
					case "minimize":
						if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
							aggregation.Minimize = property.Value.GetBoolean();
						break;
					case "predict":
						if (property.Value.ValueKind == JsonValueKind.Number)
							aggregation.Predict = property.Value.GetInt32();
						break;
					case "window":
						if (property.Value.ValueKind == JsonValueKind.Number)
							aggregation.Window = property.Value.GetInt32();
						break;
					case "settings":
						settings = property.Value.Clone();
						haveSettings = true;
						break;
					case "model":
						modelName = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
						break;
					case "buckets_path":
						var path = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
						if (!string.IsNullOrEmpty(path))
							aggregation.BucketsPath = new SingleBucketsPath(path);
						break;
				}
			}

			if (modelName != null && haveSettings)
			{
				var raw = settings.GetRawText();
				switch (modelName)
				{
					case "linear":
						aggregation.Model = JsonSerializer.Deserialize<LinearModel>(raw, options);
						break;
					case "simple":
						aggregation.Model = JsonSerializer.Deserialize<SimpleModel>(raw, options);
						break;
					case "ewma":
						aggregation.Model = JsonSerializer.Deserialize<EwmaModel>(raw, options);
						break;
					case "holt":
						aggregation.Model = JsonSerializer.Deserialize<HoltLinearModel>(raw, options);
						break;
					case "holt_winters":
						aggregation.Model = JsonSerializer.Deserialize<HoltWintersModel>(raw, options);
						break;
				}
			}

			return aggregation;
		}

		public override void Write(Utf8JsonWriter writer, IMovingAverageAggregation value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.BucketsPath != null)
			{
				writer.WritePropertyName("buckets_path");
				JsonSerializer.Serialize(writer, value.BucketsPath, options);
			}

			if (value.GapPolicy != null)
			{
				writer.WritePropertyName("gap_policy");
				// GapPolicy is an [EnumMember]-annotated enum; serialize through the registered StringEnum converter.
				JsonSerializer.Serialize(writer, value.GapPolicy.Value, options);
			}

			if (!string.IsNullOrEmpty(value.Format))
			{
				writer.WritePropertyName("format");
				writer.WriteStringValue(value.Format);
			}

			if (value.Window != null)
			{
				writer.WritePropertyName("window");
				writer.WriteNumberValue(value.Window.Value);
			}

			if (value.Minimize != null)
			{
				writer.WritePropertyName("minimize");
				writer.WriteBooleanValue(value.Minimize.Value);
			}

			if (value.Predict != null)
			{
				writer.WritePropertyName("predict");
				writer.WriteNumberValue(value.Predict.Value);
			}

			if (value.Model != null)
			{
				writer.WritePropertyName("model");
				writer.WriteStringValue(value.Model.Name);
				writer.WritePropertyName("settings");
				// Serialize the model by its concrete runtime type so the [DataMember]-annotated interface members are
				// emitted (the field-less linear/simple models produce an empty object). Serializing by the declared
				// IMovingAverageModel type would only expose the ignored Name member.
				JsonSerializer.Serialize(writer, value.Model, value.Model.GetType(), options);
			}

			writer.WriteEndObject();
		}
	}
}
