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
	/// Behavioural tests for <see cref="TermsIncludeConverter"/> and <see cref="TermsExcludeConverter"/>:
	/// each value may be a string array (exact terms), a regex string (pattern), or — for include only —
	/// a partition object.
	/// </summary>
	public class TermsIncludeExcludeConverterTests
	{
		private static JsonSerializerOptions IncludeOptions()
		{
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(new ConnectionSettings())
			};
			options.Converters.Add(new TermsIncludeConverter());
			return options;
		}

		private static JsonSerializerOptions ExcludeOptions()
		{
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(new ConnectionSettings())
			};
			options.Converters.Add(new TermsExcludeConverter());
			return options;
		}

		// ---- TermsInclude ----

		[U] public void Include_Deserialize_StringArray()
		{
			var include = JsonSerializer.Deserialize<TermsInclude>(@"[""foo"",""bar""]", IncludeOptions());

			include.Values.Should().Equal("foo", "bar");
			include.Pattern.Should().BeNull();
			include.Partition.Should().BeNull();
		}

		[U] public void Include_Deserialize_Pattern()
		{
			var include = JsonSerializer.Deserialize<TermsInclude>(@"""foo.*""", IncludeOptions());

			include.Pattern.Should().Be("foo.*");
			include.Values.Should().BeNull();
		}

		[U] public void Include_Deserialize_Partition()
		{
			var include = JsonSerializer.Deserialize<TermsInclude>(
				@"{""partition"":1,""num_partitions"":20}", IncludeOptions());

			include.Partition.Should().Be(1);
			include.NumberOfPartitions.Should().Be(20);
			include.Values.Should().BeNull();
			include.Pattern.Should().BeNull();
		}

		[U] public void Include_Deserialize_Null()
		{
			JsonSerializer.Deserialize<TermsInclude>("null", IncludeOptions()).Should().BeNull();
		}

		[U] public void Include_Serialize_StringArray()
		{
			var json = JsonSerializer.Serialize(new TermsInclude(new[] { "foo", "bar" }), IncludeOptions());
			json.Should().Be(@"[""foo"",""bar""]");
		}

		[U] public void Include_Serialize_Pattern()
		{
			var json = JsonSerializer.Serialize(new TermsInclude("foo.*"), IncludeOptions());
			json.Should().Be(@"""foo.*""");
		}

		[U] public void Include_Serialize_Partition()
		{
			var json = JsonSerializer.Serialize(new TermsInclude(1, 20), IncludeOptions());
			json.Should().Be(@"{""partition"":1,""num_partitions"":20}");
		}

		[U] public void Include_Serialize_Null()
		{
			JsonSerializer.Serialize<TermsInclude>(null, IncludeOptions()).Should().Be("null");
		}

		[U] public void Include_RoundTrip_Partition()
		{
			var options = IncludeOptions();
			var json = JsonSerializer.Serialize(new TermsInclude(3, 10), options);
			var back = JsonSerializer.Deserialize<TermsInclude>(json, options);

			back.Partition.Should().Be(3);
			back.NumberOfPartitions.Should().Be(10);
		}

		[U] public void Include_RoundTrip_StringArray()
		{
			var options = IncludeOptions();
			var json = JsonSerializer.Serialize(new TermsInclude(new[] { "a", "b", "c" }), options);
			var back = JsonSerializer.Deserialize<TermsInclude>(json, options);

			back.Values.Should().Equal("a", "b", "c");
		}

		// ---- TermsExclude ----

		[U] public void Exclude_Deserialize_StringArray()
		{
			var exclude = JsonSerializer.Deserialize<TermsExclude>(@"[""foo"",""bar""]", ExcludeOptions());

			exclude.Values.Should().Equal("foo", "bar");
			exclude.Pattern.Should().BeNull();
		}

		[U] public void Exclude_Deserialize_Pattern()
		{
			var exclude = JsonSerializer.Deserialize<TermsExclude>(@"""foo.*""", ExcludeOptions());

			exclude.Pattern.Should().Be("foo.*");
			exclude.Values.Should().BeNull();
		}

		[U] public void Exclude_Deserialize_Null()
		{
			JsonSerializer.Deserialize<TermsExclude>("null", ExcludeOptions()).Should().BeNull();
		}

		[U] public void Exclude_Serialize_StringArray()
		{
			var json = JsonSerializer.Serialize(new TermsExclude(new[] { "foo", "bar" }), ExcludeOptions());
			json.Should().Be(@"[""foo"",""bar""]");
		}

		[U] public void Exclude_Serialize_Pattern()
		{
			var json = JsonSerializer.Serialize(new TermsExclude("foo.*"), ExcludeOptions());
			json.Should().Be(@"""foo.*""");
		}

		[U] public void Exclude_Serialize_Null()
		{
			JsonSerializer.Serialize<TermsExclude>(null, ExcludeOptions()).Should().Be("null");
		}

		[U] public void Exclude_RoundTrip_StringArray()
		{
			var options = ExcludeOptions();
			var json = JsonSerializer.Serialize(new TermsExclude(new[] { "a", "b" }), options);
			var back = JsonSerializer.Deserialize<TermsExclude>(json, options);

			back.Values.Should().Equal("a", "b");
		}
	}
}
