/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="SourceFilterConverter"/>. An <see cref="ISourceFilter"/> is a union read from
	/// a single field string, an array of field strings, or an object with <c>includes</c>/<c>excludes</c> members;
	/// it is written back as an object carrying only the non-null members. Null yields/writes null. The boolean
	/// <c>_source</c> form is handled one level up at the <c>Union&lt;bool, ISourceFilter&gt;</c> and is out of scope
	/// here.
	/// </summary>
	public class SourceFilterConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new SourceFilterConverter());
			options.Converters.Add(new FieldsConverter(settings));
			options.Converters.Add(new FieldConverter(settings));
			return options;
		}

		private static ISourceFilter Deserialize(string json) =>
			JsonSerializer.Deserialize<ISourceFilter>(json, Options());

		// ---- read ----

		[U] public void Read_SingleString_BecomesIncludes()
		{
			var filter = Deserialize(@"""field1""");
			filter.Should().NotBeNull();
			filter.Includes.Select(f => f.Name).Should().Equal("field1");
			filter.Excludes.Should().BeNull();
		}

		[U] public void Read_Array_BecomesIncludes()
		{
			var filter = Deserialize(@"[""field1"",""field2""]");
			filter.Should().NotBeNull();
			filter.Includes.Select(f => f.Name).Should().Equal("field1", "field2");
			filter.Excludes.Should().BeNull();
		}

		[U] public void Read_Object_IncludesAndExcludes()
		{
			var filter = Deserialize(@"{""includes"":[""a"",""b""],""excludes"":[""c""]}");
			filter.Should().NotBeNull();
			filter.Includes.Select(f => f.Name).Should().Equal("a", "b");
			filter.Excludes.Select(f => f.Name).Should().Equal("c");
		}

		[U] public void Read_Object_OnlyExcludes()
		{
			var filter = Deserialize(@"{""excludes"":[""c""]}");
			filter.Should().NotBeNull();
			filter.Includes.Should().BeNull();
			filter.Excludes.Select(f => f.Name).Should().Equal("c");
		}

		[U] public void Read_Object_UnknownMemberSkipped()
		{
			var filter = Deserialize(@"{""includes"":[""a""],""unknown"":{""nested"":1}}");
			filter.Should().NotBeNull();
			filter.Includes.Select(f => f.Name).Should().Equal("a");
		}

		[U] public void Read_EmptyObject_ReturnsEmptyFilter()
		{
			var filter = Deserialize(@"{}");
			filter.Should().NotBeNull();
			filter.Includes.Should().BeNull();
			filter.Excludes.Should().BeNull();
		}

		[U] public void Read_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		// ---- write ----

		[U] public void Write_IncludesAndExcludes()
		{
			var filter = new SourceFilter { Includes = new[] { "a", "b" }, Excludes = new[] { "c" } };
			var json = JsonSerializer.Serialize<ISourceFilter>(filter, Options());
			json.Should().Be(@"{""includes"":[""a"",""b""],""excludes"":[""c""]}");
		}

		[U] public void Write_OnlyIncludes_OmitsNullExcludes()
		{
			var filter = new SourceFilter { Includes = new[] { "a" } };
			var json = JsonSerializer.Serialize<ISourceFilter>(filter, Options());
			json.Should().Be(@"{""includes"":[""a""]}");
		}

		[U] public void Write_Empty_WritesEmptyObject()
		{
			var json = JsonSerializer.Serialize<ISourceFilter>(new SourceFilter(), Options());
			json.Should().Be("{}");
		}

		[U] public void Write_Null_WritesNull() =>
			JsonSerializer.Serialize<ISourceFilter>(null, Options()).Should().Be("null");

		// ---- round trip ----

		[U] public void RoundTrip_Object()
		{
			var options = Options();
			var filter = new SourceFilter { Includes = new[] { "a", "b" }, Excludes = new[] { "c" } };
			var json = JsonSerializer.Serialize<ISourceFilter>(filter, options);
			var back = JsonSerializer.Deserialize<ISourceFilter>(json, options);
			back.Includes.Select(f => f.Name).Should().Equal("a", "b");
			back.Excludes.Select(f => f.Name).Should().Equal("c");
		}
	}
}
