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
	/// Tests for <see cref="InterfaceDataContractResolver"/>: verifies that data-contract metadata declared on an
	/// <em>interface</em> marked with <c>[InterfaceDataContract]</c> drives serialization of the concrete class —
	/// the key behaviour of the legacy Utf8Json mechanism that has no built-in System.Text.Json equivalent.
	/// </summary>
	public class InterfaceDataContractResolverTests
	{
		[OpenSearchContract]
		public interface IThing
		{
			[DataMember(Name = "the_name")]
			string Name { get; }

			[DataMember(Name = "the_count")]
			int Count { get; }

			// No [DataMember] -> must NOT be serialized under the data-contract opt-in model.
			string Internal { get; }

			[IgnoreDataMember]
			string Secret { get; }
		}

		public class Thing : IThing
		{
			public string Name { get; set; }
			public int Count { get; set; }
			public string Internal { get; set; }
			public string Secret { get; set; }
		}

		private static JsonSerializerOptions Options() =>
			new JsonSerializerOptions { TypeInfoResolver = new InterfaceDataContractResolver() };

		[U] public void UsesInterfaceDataMemberNames()
		{
			var json = JsonSerializer.Serialize(new Thing { Name = "n", Count = 5, Internal = "x", Secret = "s" }, Options());

			json.Should().Contain(@"""the_name"":""n""");
			json.Should().Contain(@"""the_count"":5");
		}

		[U] public void OmitsMembersWithoutDataMember()
		{
			var json = JsonSerializer.Serialize(new Thing { Name = "n", Count = 5, Internal = "x", Secret = "s" }, Options());

			// Internal has no [DataMember]; Secret is [IgnoreDataMember]. Neither should appear.
			json.Should().NotContain("Internal").And.NotContain("nternal");
			json.Should().NotContain("Secret").And.NotContain("ecret");
		}

		[U] public void Deserializes_UsingInterfaceNames()
		{
			var thing = JsonSerializer.Deserialize<Thing>(@"{""the_name"":""n"",""the_count"":9}", Options());

			thing.Name.Should().Be("n");
			thing.Count.Should().Be(9);
		}
	}
}
