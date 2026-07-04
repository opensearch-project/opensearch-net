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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IMovingAverageAggregation"/>
	/// (the <c>moving_avg</c> pipeline aggregation), replacing the vendored Utf8Json
	/// <c>MovingAverageAggregationFormatter</c> as part of #388. The <c>model</c> discriminator and
	/// its <c>settings</c> object are declared on the interface without <c>[DataMember]</c>, so the
	/// default resolver drops them; this converter writes the full shape explicitly, mirroring the
	/// vendored formatter's property order and null-omission, and reads the concrete model back from
	/// its discriminator. Like the vendored formatter, it writes only the pipeline members and the
	/// model/settings — <c>moving_avg</c> is a pipeline aggregation and carries no nested
	/// sub-aggregations; <c>meta</c> is emitted by the enclosing aggregation container.
	/// </summary>
	internal sealed class MovingAverageAggregationConverter : JsonConverter<IMovingAverageAggregation>
	{
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
				JsonSerializer.Serialize(writer, value.GapPolicy.Value, options);
			}

			if (!value.Format.IsNullOrEmpty())
				writer.WriteString("format", value.Format);

			if (value.Window != null)
				writer.WriteNumber("window", value.Window.Value);

			if (value.Minimize != null)
				writer.WriteBoolean("minimize", value.Minimize.Value);

			if (value.Predict != null)
				writer.WriteNumber("predict", value.Predict.Value);

			if (value.Model != null)
			{
				writer.WriteString("model", value.Model.Name);
				writer.WritePropertyName("settings");
				// Serialize by the model's runtime type so the DataContractResolver honors the
				// concrete model's [DataMember] settings members (alpha/beta/gamma/period/pad/type),
				// matching the vendored formatter which serialized each model via its own formatter.
				JsonSerializer.Serialize(writer, value.Model, value.Model.GetType(), options);
			}

			writer.WriteEndObject();
		}

		public override IMovingAverageAggregation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			var aggregation = new MovingAverageAggregation();
			string modelName = null;
			JsonElement? settings = null;

			while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
			{
				var propertyName = reader.GetString();
				reader.Read();
				switch (propertyName)
				{
					case "format":
						aggregation.Format = reader.GetString();
						break;
					case "gap_policy":
						aggregation.GapPolicy = JsonSerializer.Deserialize<GapPolicy?>(ref reader, options);
						break;
					case "minimize":
						aggregation.Minimize = reader.GetBoolean();
						break;
					case "predict":
						aggregation.Predict = reader.GetInt32();
						break;
					case "window":
						aggregation.Window = reader.GetInt32();
						break;
					case "settings":
						using (var document = JsonDocument.ParseValue(ref reader))
							settings = document.RootElement.Clone();
						break;
					case "model":
						modelName = reader.GetString();
						break;
					case "buckets_path":
						var path = reader.GetString();
						if (!string.IsNullOrEmpty(path))
							aggregation.BucketsPath = new SingleBucketsPath(path);
						break;
					default:
						reader.Skip();
						break;
				}
			}

			if (modelName != null && settings.HasValue)
			{
				var json = settings.Value.GetRawText();
				switch (modelName)
				{
					case "linear":
						aggregation.Model = JsonSerializer.Deserialize<LinearModel>(json, options);
						break;
					case "simple":
						aggregation.Model = JsonSerializer.Deserialize<SimpleModel>(json, options);
						break;
					case "ewma":
						aggregation.Model = JsonSerializer.Deserialize<EwmaModel>(json, options);
						break;
					case "holt":
						aggregation.Model = JsonSerializer.Deserialize<HoltLinearModel>(json, options);
						break;
					case "holt_winters":
						aggregation.Model = JsonSerializer.Deserialize<HoltWintersModel>(json, options);
						break;
				}
			}

			return aggregation;
		}
	}
}
