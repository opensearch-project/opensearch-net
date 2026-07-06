/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// Behavioural-equivalence tests for <see cref="SingleOrManyCollectionConverter{T}"/>, the
	/// System.Text.Json replacement for the legacy Utf8Json
	/// <c>InterfaceReadOnlyCollectionSingleOrEnumerableFormatter&lt;T&gt;</c>.
	/// </summary>
	public class SingleOrManyCollectionConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new SingleOrManyCollectionConverter<string>());
			return options;
		}

		[U] public void Deserialize_Array_ReadsAllElements()
		{
			var result = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(@"[""a"",""b"",""c""]", Options());
			result.Should().BeEquivalentTo(new[] { "a", "b", "c" });
		}

		[U] public void Deserialize_SingleValue_WrapsInOneElementCollection()
		{
			var result = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(@"""only""", Options());
			result.Should().ContainSingle().Which.Should().Be("only");
		}

		[U] public void Deserialize_EmptyArray_ReturnsEmptyCollection()
		{
			var result = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(@"[]", Options());
			result.Should().NotBeNull().And.BeEmpty();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var result = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(@"null", Options());
			result.Should().BeNull();
		}

		[U] public void Serialize_Collection_WritesArray()
		{
			var json = JsonSerializer.Serialize<IReadOnlyCollection<string>>(new[] { "a", "b" }, Options());
			json.Should().Be(@"[""a"",""b""]");
		}

		[U] public void Serialize_SingleElement_StillWritesArray()
		{
			// Legacy behaviour always serialises as an array, even for a single element.
			var json = JsonSerializer.Serialize<IReadOnlyCollection<string>>(new[] { "solo" }, Options());
			json.Should().Be(@"[""solo""]");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<IReadOnlyCollection<string>>(null, Options());
			json.Should().Be("null");
		}
	}
}
