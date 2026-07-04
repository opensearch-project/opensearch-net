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
	/// A <see cref="System.Text.Json"/> converter for <see cref="DateMath"/>, replacing the vendored
	/// Utf8Json <c>DateMathFormatter</c> as part of #388. A date-math value is written as its string
	/// form; on read, a plain date-time (no date-math separator) is anchored, otherwise the expression
	/// is parsed.
	/// </summary>
	internal sealed class DateMathConverter : JsonConverter<DateMath>
	{
		public override DateMath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				return null;

			var value = reader.GetString();
			if (value == null) return null;

			// Only anchor a value that is STRICTLY an ISO-8601 date/time (mirroring the vendored
			// DateMathFormatter, which used the strict ISO reader). A value in a custom `format`
			// (e.g. "01/01/2016") must be preserved verbatim as a string anchor, not reparsed to ISO.
			if (!value.Contains("||") && TryParseIso8601(value, out var dateTime))
				return DateMath.Anchored(dateTime);

			return DateMath.FromString(value);
		}

		private static readonly string[] Iso8601Formats =
		{
			"yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
			"yyyy-MM-ddTHH:mm:ssK",
			"yyyy-MM-ddTHH:mm:ss.FFFFFFF",
			"yyyy-MM-ddTHH:mm:ss",
			"yyyy-MM-dd",
			"yyyy-MM",
			"yyyy",
		};

		private static bool TryParseIso8601(string value, out DateTime dateTime) =>
			DateTime.TryParseExact(value, Iso8601Formats, CultureInfo.InvariantCulture,
				DateTimeStyles.RoundtripKind, out dateTime);

		public override void Write(Utf8JsonWriter writer, DateMath value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(value.ToString());
		}
	}
}
