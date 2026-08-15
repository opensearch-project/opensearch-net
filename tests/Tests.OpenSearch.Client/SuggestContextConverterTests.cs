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
	/// Behavioural tests for <see cref="SuggestContextConverter"/>, the System.Text.Json replacement for the legacy
	/// Utf8Json <c>SuggestContextFormatter</c>. The concrete <see cref="ISuggestContext"/> variant is dispatched from
	/// the <c>type</c> discriminator: <c>geo</c> → <see cref="GeoSuggestContext"/>, <c>category</c> (or anything
	/// else / missing) → <see cref="CategorySuggestContext"/>.
	/// </summary>
	public class SuggestContextConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new SuggestContextConverter());
			options.Converters.Add(new FieldConverter(settings));
			return options;
		}

		private static ISuggestContext Deserialize(string json) =>
			JsonSerializer.Deserialize<ISuggestContext>(json, Options());

		[U] public void Read_GeoType_BecomesGeoSuggestContext()
		{
			var context = Deserialize(@"{""type"":""geo"",""name"":""location""}");
			context.Should().BeOfType<GeoSuggestContext>();
			context.Name.Should().Be("location");
		}

		[U] public void Read_CategoryType_BecomesCategorySuggestContext()
		{
			var context = Deserialize(@"{""type"":""category"",""name"":""cat""}");
			context.Should().BeOfType<CategorySuggestContext>();
			context.Name.Should().Be("cat");
		}

		[U] public void Read_UnknownType_FallsBackToCategory()
		{
			var context = Deserialize(@"{""type"":""bogus"",""name"":""x""}");
			context.Should().BeOfType<CategorySuggestContext>();
		}

		[U] public void Read_MissingType_FallsBackToCategory()
		{
			var context = Deserialize(@"{""name"":""x""}");
			context.Should().BeOfType<CategorySuggestContext>();
		}

		[U] public void Read_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Write_Null_WritesNull() =>
			JsonSerializer.Serialize<ISuggestContext>(null, Options()).Should().Be("null");

		[U] public void Write_ByRuntimeType_EmitsType()
		{
			ISuggestContext context = new CategorySuggestContext { Name = "cat" };
			var json = JsonSerializer.Serialize(context, Options());
			json.Should().Contain(@"""type""").And.Contain(@"""category""");
		}
	}
}
