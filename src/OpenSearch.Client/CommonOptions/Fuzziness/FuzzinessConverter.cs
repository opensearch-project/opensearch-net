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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IFuzziness"/>, replacing the vendored
	/// Utf8Json <c>FuzzinessFormatter</c>/<c>FuzzinessInterfaceFormatter</c> as part of #388. Auto
	/// fuzziness is written as the string <c>"AUTO"</c> (or <c>"AUTO:low,high"</c> when a length range
	/// is supplied), an edit distance as an integer number and a ratio as a floating-point number.
	/// Reads accept both the string and number forms.
	/// </summary>
	internal sealed class FuzzinessConverter : JsonConverter<IFuzziness>
	{
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

		public override IFuzziness Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			switch (root.ValueKind)
			{
				case JsonValueKind.String:
				{
					var raw = root.GetString();
					if (string.IsNullOrEmpty(raw) || raw == "AUTO")
						return Fuzziness.Auto;

					var colonIndex = raw.IndexOf(':');
					var commaIndex = raw.IndexOf(',');
					if (colonIndex >= 0 && commaIndex > colonIndex)
					{
						var low = int.Parse(
							raw.Substring(colonIndex + 1, commaIndex - colonIndex - 1),
							CultureInfo.InvariantCulture);
						var high = int.Parse(
							raw.Substring(commaIndex + 1),
							CultureInfo.InvariantCulture);
						return Fuzziness.AutoLength(low, high);
					}

					return Fuzziness.Auto;
				}
				case JsonValueKind.Number:
				{
					if (root.TryGetInt32(out var editDistance))
						return Fuzziness.EditDistance(editDistance);

					return Fuzziness.Ratio(root.GetDouble());
				}
				default:
					return null;
			}
		}
	}
}
