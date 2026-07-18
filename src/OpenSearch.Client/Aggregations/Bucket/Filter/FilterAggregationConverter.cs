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
