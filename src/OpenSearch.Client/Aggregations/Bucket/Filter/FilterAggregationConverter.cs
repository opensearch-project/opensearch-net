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
	/// System.Text.Json replacement for the legacy Utf8Json <c>FilterAggregationFormatter</c>.
	///
	/// An <see cref="IFilterAggregation"/> is written as the bare body of its <c>filter</c> query — i.e. the same JSON
	/// a <see cref="QueryContainer"/> produces — with no additional wrapper. Mirroring the legacy formatter:
	/// <list type="bullet">
	/// <item><description>a null filter, or one that is not writable (conditionless and not verbatim), serializes as an
	/// empty object <c>{}</c>;</description></item>
	/// <item><description>otherwise the <see cref="IFilterAggregation.Filter"/> query container is serialized
	/// directly.</description></item>
	/// </list>
	/// On read a non-object token yields <c>null</c>; an object is parsed as a <see cref="QueryContainer"/> and wrapped
	/// in a <see cref="FilterAggregation"/>. The sub-aggregation dictionary is handled by the surrounding container
	/// serialization, exactly as in the legacy engine — this converter only concerns itself with the filter query.
	/// </summary>
	internal class FilterAggregationConverter : JsonConverter<IFilterAggregation>
	{
		public override IFilterAggregation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			var container = JsonSerializer.Deserialize<QueryContainer>(ref reader, options);
			return new FilterAggregation { Filter = container };
		}

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
	}
}
