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
	/// Behavioural tests for <see cref="FuzzinessConverter"/>: fuzziness values round-trip as either a string
	/// (AUTO / AUTO:low,high), an integer edit distance, or a floating point ratio.
	/// </summary>
	public class FuzzinessConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new FuzzinessConverter());
			return options;
		}

		[U] public void Deserialize_Auto()
		{
			var f = JsonSerializer.Deserialize<IFuzziness>(@"""AUTO""", Options());
			f.Auto.Should().BeTrue();
			f.Low.Should().BeNull();
			f.High.Should().BeNull();
		}

		[U] public void Deserialize_AutoLength()
		{
			var f = JsonSerializer.Deserialize<IFuzziness>(@"""AUTO:3,6""", Options());
			f.Auto.Should().BeTrue();
			f.Low.Should().Be(3);
			f.High.Should().Be(6);
		}

		[U] public void Deserialize_EditDistance()
		{
			var f = JsonSerializer.Deserialize<IFuzziness>("2", Options());
			f.Auto.Should().BeFalse();
			f.EditDistance.Should().Be(2);
			f.Ratio.Should().BeNull();
		}

		[U] public void Deserialize_Ratio()
		{
			var f = JsonSerializer.Deserialize<IFuzziness>("0.5", Options());
			f.Auto.Should().BeFalse();
			f.EditDistance.Should().BeNull();
			f.Ratio.Should().Be(0.5);
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			JsonSerializer.Deserialize<IFuzziness>("null", Options()).Should().BeNull();
		}

		[U] public void Serialize_Auto()
		{
			JsonSerializer.Serialize<IFuzziness>(Fuzziness.Auto, Options()).Should().Be(@"""AUTO""");
		}

		[U] public void Serialize_AutoLength()
		{
			JsonSerializer.Serialize<IFuzziness>(Fuzziness.AutoLength(3, 6), Options()).Should().Be(@"""AUTO:3,6""");
		}

		[U] public void Serialize_EditDistance()
		{
			JsonSerializer.Serialize<IFuzziness>(Fuzziness.EditDistance(2), Options()).Should().Be("2");
		}

		[U] public void Serialize_Ratio()
		{
			JsonSerializer.Serialize<IFuzziness>(Fuzziness.Ratio(0.5), Options()).Should().Be("0.5");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<IFuzziness>(null, Options()).Should().Be("null");
		}
	}
}
