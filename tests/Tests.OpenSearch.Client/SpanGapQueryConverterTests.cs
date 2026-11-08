/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the settings-aware <see cref="SpanGapQueryConverter"/>: a span-gap query is a single-field
	/// object <c>{ "&lt;field&gt;": &lt;width&gt; }</c>. Covers the field wrapper on write, read of the field/width pair,
	/// the conditionless/null-writes-null behaviour and round-trip.
	/// </summary>
	public class SpanGapQueryConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new SpanGapQueryConverter(settings));
			return options;
		}

		private static ISpanGapQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<ISpanGapQuery>(json, Options());

		private static string Serialize(ISpanGapQuery value) =>
			JsonSerializer.Serialize(value, Options());

		[U] public void Deserialize_FieldWidth()
		{
			var query = Deserialize(@"{""description"":2}");
			query.Should().BeOfType<SpanGapQuery>();
			query.Field.Name.Should().Be("description");
			query.Width.Should().Be(2);
		}

		[U] public void Deserialize_OnlyFirstPairIsSignificant()
		{
			// The legacy formatter ignored any pair beyond the first.
			var query = Deserialize(@"{""description"":2,""ignored"":5}");
			query.Field.Name.Should().Be("description");
			query.Width.Should().Be(2);
		}

		[U] public void Serialize_EmitsFieldWrapper()
		{
			ISpanGapQuery query = new SpanGapQuery { Field = "description", Width = 2 };
			Serialize(query).Should().Be(@"{""description"":2}");
		}

		[U] public void Serialize_Conditionless_NoWidth_WritesNull()
		{
			ISpanGapQuery query = new SpanGapQuery { Field = "description" };
			Serialize(query).Should().Be("null");
		}

		[U] public void Serialize_Conditionless_NoField_WritesNull()
		{
			ISpanGapQuery query = new SpanGapQuery { Width = 2 };
			Serialize(query).Should().Be("null");
		}

		[U] public void Deserialize_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Serialize_Null_WritesNull() =>
			JsonSerializer.Serialize<ISpanGapQuery>(null, Options()).Should().Be("null");

		[U] public void RoundTrip()
		{
			ISpanGapQuery original = new SpanGapQuery { Field = "description", Width = 3 };
			var back = Deserialize(Serialize(original));
			back.Field.Name.Should().Be("description");
			back.Width.Should().Be(3);
		}
	}
}
