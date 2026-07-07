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
	/// System.Text.Json replacement for the legacy Utf8Json <c>FuzzinessFormatter</c> /
	/// <c>FuzzinessInterfaceFormatter</c>.
	///
	/// A fuzziness value is serialized as either a JSON string (<c>"AUTO"</c> or <c>"AUTO:low,high"</c>), a JSON
	/// integer (an edit distance) or a JSON floating point number (a ratio). On read the token type decides the
	/// concrete shape produced.
	/// </summary>
	internal class FuzzinessConverter : JsonConverter<IFuzziness>
	{
		public override IFuzziness Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
				{
					var raw = reader.GetString();
					if (string.Equals(raw, "AUTO", StringComparison.Ordinal))
						return Fuzziness.Auto;

					// Expected form: AUTO:low,high
					var colonIndex = raw.IndexOf(':');
					var commaIndex = raw.IndexOf(',');
					if (colonIndex < 0 || commaIndex < 0 || commaIndex <= colonIndex)
						return Fuzziness.Auto;

					var lowText = raw.Substring(colonIndex + 1, commaIndex - colonIndex - 1);
					var highText = raw.Substring(commaIndex + 1);
					var low = int.Parse(lowText, CultureInfo.InvariantCulture);
					var high = int.Parse(highText, CultureInfo.InvariantCulture);
					return Fuzziness.AutoLength(low, high);
				}
				case JsonTokenType.Number:
				{
					// An integer token is treated as an edit distance; anything with a fractional part is a ratio.
					if (reader.TryGetInt32(out var editDistance))
						return Fuzziness.EditDistance(editDistance);

					return Fuzziness.Ratio(reader.GetDouble());
				}
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, IFuzziness value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (value.Auto)
			{
				if (!value.Low.HasValue || !value.High.HasValue)
					writer.WriteStringValue("AUTO");
				else
					writer.WriteStringValue($"AUTO:{value.Low},{value.High}");
			}
			else if (value.EditDistance.HasValue)
				writer.WriteNumberValue(value.EditDistance.Value);
			else if (value.Ratio.HasValue)
				writer.WriteNumberValue(value.Ratio.Value);
			else
				writer.WriteNullValue();
		}
	}
}
