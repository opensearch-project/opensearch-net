/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>DateMathFormatter</c>. A
	/// <see cref="DateMath"/> is serialized as its string representation. On read, a string that
	/// does not contain the date math separator (<c>||</c>) and parses as a date/time is anchored
	/// via <see cref="DateMath.Anchored(DateTime)"/>; otherwise the string is parsed via
	/// <see cref="DateMath.FromString"/>. Any non-string token is skipped and yields <c>null</c>.
	/// </summary>
	internal class DateMathConverter : JsonConverter<DateMath>
	{
		public override DateMath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
				return DateMath.Anchored(dateTime);

			return DateMath.FromString(value);
		}

		public override void Write(Utf8JsonWriter writer, DateMath value, JsonSerializerOptions options)
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
