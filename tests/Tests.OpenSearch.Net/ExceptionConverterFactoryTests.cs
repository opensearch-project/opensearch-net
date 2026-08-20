/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="ExceptionConverterFactory"/>, the System.Text.Json replacement
	/// for the legacy Utf8Json <c>ExceptionFormatter&lt;TException&gt;</c>.
	/// </summary>
	public class ExceptionConverterFactoryTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new ExceptionConverterFactory());
			return options;
		}

		[U] public void Serialize_SingleException_WritesOneElementArray()
		{
			var ex = new InvalidOperationException("boom") { Source = "unit-test", HelpLink = "http://help" };

			using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ex, Options()));

			doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
			doc.RootElement.GetArrayLength().Should().Be(1);

			var first = doc.RootElement[0];
			first.GetProperty("Depth").GetInt32().Should().Be(0);
			first.GetProperty("ClassName").GetString().Should().Be("System.InvalidOperationException");
			first.GetProperty("Message").GetString().Should().Be("boom");
			first.GetProperty("Source").GetString().Should().Be("unit-test");
			first.GetProperty("HelpURL").GetString().Should().Be("http://help");
		}

		[U] public void Serialize_InnerExceptionChain_FlattensWithIncreasingDepth()
		{
			var inner = new ArgumentException("inner");
			var outer = new InvalidOperationException("outer", inner);

			using var doc = JsonDocument.Parse(JsonSerializer.Serialize(outer, Options()));

			doc.RootElement.GetArrayLength().Should().Be(2);
			doc.RootElement[0].GetProperty("Depth").GetInt32().Should().Be(0);
			doc.RootElement[0].GetProperty("Message").GetString().Should().Be("outer");
			doc.RootElement[1].GetProperty("Depth").GetInt32().Should().Be(1);
			doc.RootElement[1].GetProperty("Message").GetString().Should().Be("inner");
		}

		[U] public void Serialize_DeepChain_IsCappedAt20()
		{
			// Build a chain deeper than the 20-exception cap.
			Exception ex = new Exception("level-0");
			for (var i = 1; i < 30; i++)
				ex = new Exception($"level-{i}", ex);

			using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ex, Options()));

			doc.RootElement.GetArrayLength().Should().Be(20);
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<Exception>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_IsNotSupported()
		{
			Action act = () => JsonSerializer.Deserialize<Exception>(@"[{""Message"":""x""}]", Options());
			act.Should().Throw<NotSupportedException>();
		}
	}
}
