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
	/// Behavioural tests for <see cref="FuzzyQueryConverter"/>: dispatches an <see cref="IFuzzyQuery"/> to the
	/// concrete variant inferred from the inner <c>value</c> field (date string, plain string, or number), reading
	/// the common options and the per-variant <c>fuzziness</c> type from the buffered DOM.
	/// </summary>
	public class FuzzyQueryConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new FuzzyQueryConverter(settings));
			// Sub-field converters the fuzziness / rewrite handling delegates to.
			options.Converters.Add(new FuzzinessConverter());
			options.Converters.Add(new TimeConverter());
			options.Converters.Add(new MultiTermQueryRewriteConverter());
			return options;
		}

		private static IFuzzyQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<IFuzzyQuery>(json, Options());

		[U] public void Deserialize_FuzzyString_ByStringValue()
		{
			var query = Deserialize(@"{""field"":{""value"":""ki""}}");
			query.Should().BeOfType<FuzzyQuery>();
			query.Field.Name.Should().Be("field");
			((IFuzzyStringQuery)query).Value.Should().Be("ki");
		}

		[U] public void Deserialize_FuzzyNumeric_ByNumberValue()
		{
			var query = Deserialize(@"{""field"":{""value"":12.5}}");
			query.Should().BeOfType<FuzzyNumericQuery>();
			((IFuzzyNumericQuery)query).Value.Should().Be(12.5);
		}

		[U] public void Deserialize_FuzzyDate_ByIsoDateValue()
		{
			var query = Deserialize(@"{""field"":{""value"":""2020-01-01T00:00:00""}}");
			query.Should().BeOfType<FuzzyDateQuery>();
		}

		[U] public void Deserialize_CommonOptions()
		{
			var query = Deserialize(
				@"{""field"":{""value"":""ki"",""prefix_length"":2,""max_expansions"":50,""transpositions"":true,""boost"":2.0,""_name"":""named""}}");

			query.Should().BeOfType<FuzzyQuery>();
			query.PrefixLength.Should().Be(2);
			query.MaxExpansions.Should().Be(50);
			query.Transpositions.Should().Be(true);
			query.Boost.Should().Be(2.0);
			query.Name.Should().Be("named");
		}

		[U] public void Deserialize_StringFuzziness_EditDistance()
		{
			var query = Deserialize(@"{""field"":{""value"":""ki"",""fuzziness"":2}}");
			var fuzzy = query.Should().BeOfType<FuzzyQuery>().Subject;
			fuzzy.Fuzziness.Should().NotBeNull();
			((IFuzziness)fuzzy.Fuzziness).EditDistance.Should().Be(2);
		}

		[U] public void Deserialize_StringFuzziness_Auto()
		{
			var query = Deserialize(@"{""field"":{""value"":""ki"",""fuzziness"":""AUTO""}}");
			var fuzzy = query.Should().BeOfType<FuzzyQuery>().Subject;
			((IFuzziness)fuzzy.Fuzziness).Auto.Should().BeTrue();
		}

		[U] public void Deserialize_NumericFuzziness()
		{
			var query = Deserialize(@"{""field"":{""value"":12.5,""fuzziness"":1.5}}");
			var fuzzy = query.Should().BeOfType<FuzzyNumericQuery>().Subject;
			fuzzy.Fuzziness.Should().Be(1.5);
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<IFuzzyQuery>(null, Options()).Should().Be("null");
		}
	}
}
