/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="RangeQueryConverter"/>: dispatches an <see cref="IRangeQuery"/> to the
	/// concrete variant inferred from the inner bound values (date / floating-point / integral / term) and the
	/// date-only <c>format</c> / <c>time_zone</c> options.
	/// </summary>
	public class RangeQueryConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new RangeQueryConverter());
			return options;
		}

		private static IRangeQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<IRangeQuery>(json, Options());

		[U] public void Deserialize_TermRange_ByStringBounds()
		{
			var query = Deserialize(@"{""field"":{""gte"":""alpha"",""lt"":""omega""}}");
			query.Should().BeOfType<TermRangeQuery>();
			query.Field.Name.Should().Be("field");
		}

		[U] public void Deserialize_LongRange_ByIntegralBounds()
		{
			var query = Deserialize(@"{""field"":{""gte"":1,""lte"":10}}");
			query.Should().BeOfType<LongRangeQuery>();
		}

		[U] public void Deserialize_NumericRange_ByFloatingPointBounds()
		{
			var query = Deserialize(@"{""field"":{""gte"":1.5,""lte"":10.25}}");
			query.Should().BeOfType<NumericRangeQuery>();
		}

		[U] public void Deserialize_DateRange_ByIsoDateBounds()
		{
			var query = Deserialize(@"{""field"":{""gte"":""2020-01-01T00:00:00"",""lt"":""2021-01-01T00:00:00""}}");
			query.Should().BeOfType<DateRangeQuery>();
		}

		[U] public void Deserialize_DateRange_ByDateMathBounds()
		{
			var query = Deserialize(@"{""field"":{""gte"":""2020-01-01||+1M""}}");
			query.Should().BeOfType<DateRangeQuery>();
		}

		[U] public void Deserialize_DateRange_ByFormatOption()
		{
			// The presence of "format" forces the date variant even without a parseable bound.
			var query = Deserialize(@"{""field"":{""gte"":""alpha"",""format"":""yyyy""}}");
			query.Should().BeOfType<DateRangeQuery>();
		}

		[U] public void Deserialize_DateRange_ByTimeZoneOption()
		{
			var query = Deserialize(@"{""field"":{""gte"":""alpha"",""time_zone"":""+01:00""}}");
			query.Should().BeOfType<DateRangeQuery>();
		}

		[U] public void Deserialize_ReadsCommonOptions_BoostAndName()
		{
			// _name and boost come from QueryBase and apply to every range variant; the legacy formatter preserved
			// them by delegating to the full type formatter, so the converter must read them too.
			var query = Deserialize(@"{""field"":{""gte"":1,""lte"":10,""boost"":2.5,""_name"":""my_range""}}");
			query.Should().BeOfType<LongRangeQuery>();
			query.Boost.Should().Be(2.5);
			query.Name.Should().Be("my_range");
		}

		[U] public void Deserialize_ReadsCommonOptions_OnDateVariant()
		{
			var query = Deserialize(@"{""field"":{""gte"":""2020-01-01T00:00:00"",""boost"":1.5,""_name"":""d""}}");
			query.Should().BeOfType<DateRangeQuery>();
			query.Boost.Should().Be(1.5);
			query.Name.Should().Be("d");
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<IRangeQuery>(null, Options()).Should().Be("null");
		}

		[U] public void Serialize_ByRuntimeType()
		{
			// Serialization dispatches on the runtime type; the field-name wrapper itself is produced by the
			// (not-yet-migrated) FieldNameQuery serialization, so we only assert the bound values are written.
			IRangeQuery query = new NumericRangeQuery { GreaterThanOrEqualTo = 1.5, LessThanOrEqualTo = 10.25 };
			var json = JsonSerializer.Serialize(query, Options());
			json.Should().Contain("1.5").And.Contain("10.25");
		}
	}
}
