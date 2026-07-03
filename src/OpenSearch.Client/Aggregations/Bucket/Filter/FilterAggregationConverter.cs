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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IFilterAggregation"/>, replacing the
	/// vendored Utf8Json <c>FilterAggregationFormatter</c> as part of #388. The aggregation body IS the
	/// <see cref="IFilterAggregation.Filter"/> query container written inline; sub-aggregations and
	/// meta are emitted as siblings by the enclosing aggregation container.
	/// </summary>
	internal sealed class FilterAggregationConverter : JsonConverter<IFilterAggregation>
	{
		public override void Write(Utf8JsonWriter writer, IFilterAggregation value, JsonSerializerOptions options)
		{
			if (value?.Filter == null || !value.Filter.IsWritable)
			{
				writer.WriteStartObject();
				writer.WriteEndObject();
				return;
			}

			JsonSerializer.Serialize(writer, value.Filter, options);
		}

		public override IFilterAggregation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) return null;

			var container = JsonSerializer.Deserialize<QueryContainer>(ref reader, options);
			return new FilterAggregation { Filter = container };
		}
	}
}
