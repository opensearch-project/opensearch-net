/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Validates the System.Text.Json "try-read" union converters <see cref="SlicesConverter"/> and
	/// <see cref="UnionListConverter{TCollection, TFirst, TSecond}"/>. These read a value that may be one of
	/// several JSON shapes and must probe each candidate type against a buffered <see cref="JsonDocument"/>.
	/// </summary>
	public class UnionTryReadConverterTests
	{
		// Concrete list type for exercising UnionListConverter<TCollection, long, string>.
		private class LongOrStringList : List<Union<long, string>> { }

		private static readonly JsonSerializerOptions SlicesOptions = new()
		{
			Converters = { new SlicesConverter() }
		};

		private static readonly JsonSerializerOptions ListOptions = new()
		{
			Converters = { new UnionListConverter<LongOrStringList, long, string>() }
		};

		// ---- SlicesConverter --------------------------------------------------

		[U] public void Slices_ReadsNumericBranch()
		{
			var slices = JsonSerializer.Deserialize<Slices>("5", SlicesOptions);
			slices.Should().NotBeNull();
			slices.Tag.Should().Be(0);
			slices.Item1.Should().Be(5L);
		}

		[U] public void Slices_ReadsStringBranch()
		{
			var slices = JsonSerializer.Deserialize<Slices>("\"auto\"", SlicesOptions);
			slices.Should().NotBeNull();
			slices.Tag.Should().Be(1);
			slices.Item2.Should().Be("auto");
		}

		[U] public void Slices_ReadsNull()
		{
			var slices = JsonSerializer.Deserialize<Slices>("null", SlicesOptions);
			slices.Should().BeNull();
		}

		[U] public void Slices_WritesNumericBranch()
		{
			var json = JsonSerializer.Serialize(new Slices(7L), SlicesOptions);
			json.Should().Be("7");
		}

		[U] public void Slices_WritesStringBranch()
		{
			var json = JsonSerializer.Serialize(new Slices("auto"), SlicesOptions);
			json.Should().Be("\"auto\"");
		}

		[U] public void Slices_WritesNull()
		{
			var json = JsonSerializer.Serialize<Slices>(null, SlicesOptions);
			json.Should().Be("null");
		}

		[U] public void Slices_RoundTripsNumeric()
		{
			var json = JsonSerializer.Serialize(new Slices(42L), SlicesOptions);
			var back = JsonSerializer.Deserialize<Slices>(json, SlicesOptions);
			back.Tag.Should().Be(0);
			back.Item1.Should().Be(42L);
		}

		[U] public void Slices_RoundTripsString()
		{
			var json = JsonSerializer.Serialize(new Slices("auto"), SlicesOptions);
			var back = JsonSerializer.Deserialize<Slices>(json, SlicesOptions);
			back.Tag.Should().Be(1);
			back.Item2.Should().Be("auto");
		}

		// ---- UnionListConverter ----------------------------------------------

		[U] public void List_ReadsNull()
		{
			var list = JsonSerializer.Deserialize<LongOrStringList>("null", ListOptions);
			list.Should().BeNull();
		}

		[U] public void List_ReadsEmptyArray()
		{
			var list = JsonSerializer.Deserialize<LongOrStringList>("[]", ListOptions);
			list.Should().NotBeNull().And.BeEmpty();
		}

		[U] public void List_ReadsMixedBranches()
		{
			var list = JsonSerializer.Deserialize<LongOrStringList>("[1,\"two\",3,\"four\"]", ListOptions);
			list.Should().NotBeNull();
			list.Should().HaveCount(4);

			list[0].Tag.Should().Be(0);
			list[0].Item1.Should().Be(1L);

			list[1].Tag.Should().Be(1);
			list[1].Item2.Should().Be("two");

			list[2].Tag.Should().Be(0);
			list[2].Item1.Should().Be(3L);

			list[3].Tag.Should().Be(1);
			list[3].Item2.Should().Be("four");
		}

		[U] public void List_WritesNull()
		{
			var json = JsonSerializer.Serialize<LongOrStringList>(null, ListOptions);
			json.Should().Be("null");
		}

		[U] public void List_WritesMixedBranches()
		{
			var list = new LongOrStringList
			{
				new Union<long, string>(1L),
				new Union<long, string>("two"),
				new Union<long, string>(3L)
			};
			var json = JsonSerializer.Serialize(list, ListOptions);
			json.Should().Be("[1,\"two\",3]");
		}

		[U] public void List_RoundTripsMixedBranches()
		{
			var list = new LongOrStringList
			{
				new Union<long, string>(10L),
				new Union<long, string>("ten")
			};
			var json = JsonSerializer.Serialize(list, ListOptions);
			var back = JsonSerializer.Deserialize<LongOrStringList>(json, ListOptions);

			back.Should().HaveCount(2);
			back[0].Tag.Should().Be(0);
			back[0].Item1.Should().Be(10L);
			back[1].Tag.Should().Be(1);
			back[1].Item2.Should().Be("ten");
		}
	}
}
