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

		// A constructor-bound immutable type with a single public parameterized ctor and get-only properties (no
		// parameterless ctor at all). STJ binds via the parameterized ctor; the resolver must NOT inject a
		// parameterless CreateObject fallback (there is none to inject, and doing so would deserialize X/Y as 0).
		public class ImmutablePoint
		{
			public ImmutablePoint(int x, int y)
			{
				X = x;
				Y = y;
			}

			public int X { get; }
			public int Y { get; }
		}

		[U] public void Deserializes_ConstructorBoundType_ViaParameterizedCtor()
		{
			var p = JsonSerializer.Deserialize<ImmutablePoint>(@"{""X"":3,""Y"":7}", Options());

			p.X.Should().Be(3);
			p.Y.Should().Be(7);
		}

		// A [JsonConstructor]-marked parameterized ctor even when a private parameterless ctor exists: the explicit
		// attribute expresses intent, so the resolver must respect constructor binding rather than the fallback.
		public class ExplicitCtorBound
		{
			private ExplicitCtorBound() { }

			[System.Text.Json.Serialization.JsonConstructor]
			public ExplicitCtorBound(string label)
			{
				Label = label;
			}

			public string Label { get; }
		}

		[U] public void Deserializes_JsonConstructorMarkedType_ViaParameterizedCtor()
		{
			var obj = JsonSerializer.Deserialize<ExplicitCtorBound>(@"{""Label"":""hi""}", Options());

			obj.Label.Should().Be("hi");
		}

		// A type with only a non-public parameterless ctor (no parameterized ctor): the resolver's fallback SHOULD
		// still let it be constructed, matching what the legacy Utf8Json engine could do.
		public class NonPublicParameterlessOnly
		{
			internal NonPublicParameterlessOnly() { }

			public string Value { get; set; }
		}

		[U] public void Deserializes_TypeWithOnlyNonPublicParameterlessCtor()
		{
			var obj = JsonSerializer.Deserialize<NonPublicParameterlessOnly>(@"{""Value"":""ok""}", Options());

			obj.Should().NotBeNull();
			obj.Value.Should().Be("ok");
		}

		// A surfaced property renamed via [DataMember(Name)] to "x", plus an explicit-interface member whose own
		// [DataMember(Name)] is also "x". AddInterfaceDataMembers must not synthesize a second property with the JSON
		// name "x" (System.Text.Json throws on a duplicate JSON name) — it seeds its dedup set with the already-present
		// property names, so the interface member is skipped.
		public interface IDup
		{
			[DataMember(Name = "x")]
			string FromInterface { get; }
		}

		public class DupNames : IDup
		{
			[DataMember(Name = "x")]
			public string Surfaced { get; set; }

			// Explicit interface implementation: STJ does not surface it, so AddInterfaceDataMembers would try to add
			// it under the interface's [DataMember(Name="x")] — colliding with Surfaced's renamed "x".
			string IDup.FromInterface => "iface";
		}

		[U] public void InterfaceDataMember_JsonNameCollision_DoesNotThrow()
		{
			var act = () => JsonSerializer.Serialize(new DupNames { Surfaced = "s" }, Options());

			act.Should().NotThrow();
			// The surfaced property wins the "x" name; the colliding interface member is skipped.
			JsonSerializer.Serialize(new DupNames { Surfaced = "s" }, Options()).Should().Contain(@"""x"":""s""");
		}
	}
}
