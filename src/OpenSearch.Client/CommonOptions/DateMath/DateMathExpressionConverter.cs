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
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>DateMathExpressionFormatter</c>. A
	/// <see cref="DateMathExpression"/> is serialized as its string representation. On read, a string
	/// that does not contain the date math separator (<c>||</c>) and parses as a date/time is
	/// anchored as a new <see cref="DateMathExpression"/> from the <see cref="DateTime"/>; otherwise
	/// a <see cref="DateMathExpression"/> is constructed from the raw string. Any non-string token is
	/// skipped and yields <c>null</c>.
	/// </summary>
	internal class DateMathExpressionConverter : JsonConverter<DateMathExpression>
	{
		public override DateMathExpression Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
			{
				reader.Skip();
				return null;
			}

			var value = reader.GetString();
			if (value == null)
				return null;

			if (!ContainsDateMathSeparator(value) && IsDateTime(value, out var dateTime))
				return new DateMathExpression(dateTime);

			return new DateMathExpression(value);
		}

		public override void Write(Utf8JsonWriter writer, DateMathExpression value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(value.ToString());
		}

		private static bool IsDateTime(string value, out DateTime dateTime)
		{
			dateTime = default;
			return value != null &&
				DateTime.TryParse(value, CultureInfo.InvariantCulture,
					DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out dateTime);
		}

		private static bool ContainsDateMathSeparator(string value) =>
			value != null && value.IndexOf("||", StringComparison.Ordinal) >= 0;
	}
}
