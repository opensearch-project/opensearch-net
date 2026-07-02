/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.CommonOptions.SystemTextJsonConverters;

internal sealed class FuzzinessInterfaceConverter : JsonConverter<IFuzziness>
{
	private readonly IConnectionSettingsValues _settings;

	public FuzzinessInterfaceConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override IFuzziness? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var converter = new FuzzinessConverter(_settings);
		return converter.Read(ref reader, typeof(Fuzziness), options);
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
		{
			writer.WriteNumberValue(value.EditDistance.Value);
		}
		else if (value.Ratio.HasValue)
		{
			writer.WriteNumberValue(value.Ratio.Value);
		}
		else
		{
			writer.WriteNullValue();
		}
	}
}

internal sealed class FuzzinessConverter : JsonConverter<Fuzziness>
{
	private readonly IConnectionSettingsValues _settings;

	public FuzzinessConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override Fuzziness? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			if (value == null)
				return null;

			if (string.Equals(value, "AUTO", StringComparison.OrdinalIgnoreCase))
				return Fuzziness.Auto;

			if (value.StartsWith("AUTO:", StringComparison.OrdinalIgnoreCase))
			{
				var parts = value.Substring(5).Split(',');
				if (parts.Length == 2
					&& int.TryParse(parts[0], out var low)
					&& int.TryParse(parts[1], out var high))
				{
					return Fuzziness.AutoLength(low, high);
				}
			}

			return null;
		}

		if (reader.TokenType == JsonTokenType.Number)
		{
			if (reader.TryGetDouble(out var doubleValue))
			{
				if (Math.Abs(doubleValue % 1) < double.Epsilon)
					return Fuzziness.EditDistance((int)doubleValue);

				return Fuzziness.Ratio(doubleValue);
			}
		}

		return null;
	}

	public override void Write(Utf8JsonWriter writer, Fuzziness value, JsonSerializerOptions options)
	{
		var interfaceConverter = new FuzzinessInterfaceConverter(_settings);
		interfaceConverter.Write(writer, value, options);
	}
}
