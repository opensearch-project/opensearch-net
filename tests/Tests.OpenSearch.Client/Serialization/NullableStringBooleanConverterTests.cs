/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Unit tests for <see cref="NullableStringBooleanConverter"/>, the System.Text.Json replacement for the
	/// legacy Utf8Json <c>NullableStringBooleanFormatter</c>.
	/// </summary>
	public class NullableStringBooleanConverterTests
	{
		private static readonly NullableStringBooleanConverter Converter = new();

		private static bool? Read(string json)
		{
			var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
			reader.Read(); // advance to first token
			return Converter.Read(ref reader, typeof(bool?), new JsonSerializerOptions());
		}

		private static string Write(bool? value)
		{
			using var ms = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(ms))
			{
				Converter.Write(writer, value, new JsonSerializerOptions());
			}
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		[U] public void Read_BooleanTrue() => Read("true").Should().BeTrue();

		[U] public void Read_BooleanFalse() => Read("false").Should().BeFalse();

		[U] public void Read_StringTrue() => Read("\"true\"").Should().BeTrue();

		[U] public void Read_StringFalse() => Read("\"false\"").Should().BeFalse();

		[U] public void Read_StringMixedCase() => Read("\"True\"").Should().BeTrue();

		[U] public void Read_Null() => Read("null").Should().BeNull();

		[U] public void Read_InvalidString_Throws()
		{
			Action act = () => Read("\"notabool\"");
			act.Should().Throw<JsonException>();
		}

		[U] public void Read_UnexpectedNumberToken_Throws()
		{
			Action act = () => Read("123");
			act.Should().Throw<JsonException>();
		}

		[U] public void Write_True() => Write(true).Should().Be("true");

		[U] public void Write_False() => Write(false).Should().Be("false");

		[U] public void Write_Null() => Write(null).Should().Be("null");

		[U] public void RoundTrips_True() => Read(Write(true)).Should().BeTrue();

		[U] public void RoundTrips_Null() => Read(Write(null)).Should().BeNull();
	}
}
