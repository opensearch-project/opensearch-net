/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Runtime.Serialization;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="StringEnumConverterFactory"/>, which serializes enums decorated with
	/// <see cref="StringEnumAttribute"/> as strings (camelCase, or the <see cref="EnumMemberAttribute"/> value
	/// when present) and deserializes case-insensitively, also accepting the raw field name and numeric values.
	/// </summary>
	public class StringEnumConverterFactoryTests
	{
		[StringEnum]
		public enum Color
		{
			Red,
			[EnumMember(Value = "bright-green")] Green,
			DarkBlue
		}

		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		[U] public void Write_CamelCasesName()
		{
			JsonSerializer.Serialize(Color.DarkBlue, Options()).Should().Be(@"""darkBlue""");
		}

		[U] public void Write_UsesEnumMemberValue()
		{
			JsonSerializer.Serialize(Color.Green, Options()).Should().Be(@"""bright-green""");
		}

		[U] public void Read_SerializedName()
		{
			JsonSerializer.Deserialize<Color>(@"""darkBlue""", Options()).Should().Be(Color.DarkBlue);
		}

		[U] public void Read_EnumMemberValue()
		{
			JsonSerializer.Deserialize<Color>(@"""bright-green""", Options()).Should().Be(Color.Green);
		}

		[U] public void Read_IsCaseInsensitive()
		{
			JsonSerializer.Deserialize<Color>(@"""DARKBLUE""", Options()).Should().Be(Color.DarkBlue);
		}

		[U] public void Read_RawFieldName()
		{
			JsonSerializer.Deserialize<Color>(@"""Red""", Options()).Should().Be(Color.Red);
		}

		[U] public void Read_NumericValue()
		{
			// Green is the second member (index 1).
			JsonSerializer.Deserialize<Color>("1", Options()).Should().Be(Color.Green);
		}

		[U] public void Read_UnknownString_Throws()
		{
			var act = () => JsonSerializer.Deserialize<Color>(@"""purple""", Options());
			act.Should().Throw<JsonException>();
		}

		[U] public void Nullable_Null_ReturnsNull()
		{
			JsonSerializer.Deserialize<Color?>("null", Options()).Should().BeNull();
		}

		[U] public void Nullable_Value_RoundTrips()
		{
			JsonSerializer.Serialize<Color?>(Color.Red, Options()).Should().Be(@"""red""");
			JsonSerializer.Deserialize<Color?>(@"""red""", Options()).Should().Be(Color.Red);
		}
	}
}
