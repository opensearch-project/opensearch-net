/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="FieldCapabilitiesFieldsConverter"/>, the dedicated (non-generic)
	/// settings-aware replacement for the legacy Utf8Json <c>FieldCapabilitiesFields.Converter</c>. The wire shape is
	/// a flat object keyed by field name whose values are <see cref="FieldTypes"/> maps; keys resolve through the
	/// runtime <c>Inferrer</c> and the parsed entries are wrapped in a <c>ResolvableDictionaryProxy</c>.
	/// </summary>
	public class FieldCapabilitiesFieldsConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new FieldCapabilitiesFieldsConverter(settings));
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		private static FieldCapabilitiesFields Deserialize(string json) =>
			JsonSerializer.Deserialize<FieldCapabilitiesFields>(Encoding.UTF8.GetBytes(json), Options());

		[U] public void Parses_Fields_AndResolvesKeys()
		{
			var json = @"{
				""name"": { ""keyword"": { ""searchable"": true, ""aggregatable"": true } },
				""_index"": { ""_index"": { ""searchable"": true, ""aggregatable"": true } }
			}";
			var fields = Deserialize(json);

			fields.Should().NotBeNull();
			fields.Count.Should().Be(2);

			// Key resolution: the proxy indexes by the Inferrer-resolved field name.
			fields.ResolvedKeys.Should().Contain("name").And.Contain("_index");
			fields.Keys.Should().Contain((Field)"name");

			// Value path: FieldTypes exposes typed accessors over its inner "keyword"/"_index" entries.
			var keyword = fields["name"].Keyword;
			keyword.Should().NotBeNull();
			keyword.Searchable.Should().BeTrue();
			keyword.Aggregatable.Should().BeTrue();

			fields["_index"].Index.Should().NotBeNull();
		}

		[U] public void Parses_DottedFieldName_AsKey()
		{
			var fields = Deserialize(@"{ ""jobTitle.keyword"": { ""keyword"": { ""searchable"": true } } }");
			fields.Count.Should().Be(1);
			fields.ResolvedKeys.Should().Contain("jobTitle.keyword");
			fields["jobTitle.keyword"].Keyword.Searchable.Should().BeTrue();
		}

		[U] public void Parses_EmptyObject()
		{
			var fields = Deserialize("{}");
			fields.Should().NotBeNull();
			fields.Count.Should().Be(0);
		}

		[U] public void Parses_Null_YieldsEmpty()
		{
			// HandleNull is opted in; a top-level null yields a non-null, empty proxy (matching the legacy formatter,
			// whose dictionary formatter returned null and the proxy treated as empty).
			var fields = Deserialize("null");
			fields.Should().NotBeNull();
			fields.Count.Should().Be(0);
		}

		[U] public void Write_Throws_NotSupported()
		{
			// The legacy formatter's Serialize threw NotSupportedException (a response-only dictionary).
			var fields = Deserialize(@"{ ""name"": { ""keyword"": {} } }");
			var act = () => JsonSerializer.Serialize(fields, Options());
			act.Should().Throw<System.NotSupportedException>();
		}
	}
}
