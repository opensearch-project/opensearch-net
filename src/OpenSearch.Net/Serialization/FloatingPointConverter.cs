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

namespace OpenSearch.Net
{
	/// <summary>
	/// <see cref="System.Text.Json"/> converters for <see cref="double"/> and <see cref="float"/> that
	/// match the vendored Utf8Json number formatting (#388): an integral value in fixed notation is
	/// written with a trailing <c>.0</c> (for example <c>2</c> → <c>2.0</c>), which STJ's shortest form
	/// omits. Values already carrying a decimal point or an exponent (e.g. <c>1.5</c>, <c>1E+20</c>)
	/// are unchanged and already match. <c>boost</c> and many scored/float fields depend on this.
	/// <para>
	/// STJ applies these to the corresponding nullable types (<c>double?</c>/<c>float?</c>)
	/// automatically via its built-in nullable wrapping.
	/// </para>
	/// </summary>
	public sealed class DoubleFormatConverter : JsonConverter<double>
	{
		/// <summary> A shared instance. </summary>
		public static readonly DoubleFormatConverter Instance = new();

		/// <inheritdoc />
		public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.GetDouble();

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) =>
			FloatingPointConverter.WriteWithDecimal(writer, value, !double.IsNaN(value) && !double.IsInfinity(value),
				value.ToString("R", CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// <see cref="float"/> counterpart of <see cref="DoubleFormatConverter"/>.
	/// </summary>
	public sealed class SingleFormatConverter : JsonConverter<float>
	{
		/// <summary> A shared instance. </summary>
		public static readonly SingleFormatConverter Instance = new();

		/// <inheritdoc />
		public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.GetSingle();

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options) =>
			FloatingPointConverter.WriteWithDecimal(writer, value, !float.IsNaN(value) && !float.IsInfinity(value),
				value.ToString("R", CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// <see cref="decimal"/> counterpart of <see cref="DoubleFormatConverter"/>: a whole value keeps a
	/// decimal point (e.g. <c>1.0</c> rather than <c>1</c>), matching the vendored serializer.
	/// </summary>
	public sealed class DecimalFormatConverter : JsonConverter<decimal>
	{
		/// <summary> A shared instance. </summary>
		public static readonly DecimalFormatConverter Instance = new();

		/// <inheritdoc />
		public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.GetDecimal();

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
			FloatingPointConverter.WriteWithDecimal(writer, (double)value, true, value.ToString(CultureInfo.InvariantCulture));
	}

	internal static class FloatingPointConverter
	{
		internal static void WriteWithDecimal(Utf8JsonWriter writer, double value, bool isFinite, string text)
		{
			if (!isFinite)
			{
				// Non-finite is not valid JSON; defer to STJ (throws unless configured), preserving default behavior.
				writer.WriteNumberValue(value);
				return;
			}

			if (text.IndexOf('.') < 0 && text.IndexOf('E') < 0 && text.IndexOf('e') < 0)
				text += ".0";

			writer.WriteRawValue(text);
		}
	}
}
