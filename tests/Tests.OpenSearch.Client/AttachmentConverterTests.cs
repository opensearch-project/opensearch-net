/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="AttachmentConverter"/>, the System.Text.Json replacement for the legacy
	/// Utf8Json <c>AttachmentFormatter</c>. An attachment reads from a bare string (→ content) or an object whose
	/// fields accept both plain and underscore-prefixed aliases; writing emits a bare string when there is no
	/// metadata, otherwise an object with the populated fields.
	/// </summary>
	public class AttachmentConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new AttachmentConverter());
			return options;
		}

		private static Attachment Deserialize(string json) =>
			JsonSerializer.Deserialize<Attachment>(json, Options());

		[U] public void Read_String_BecomesContent()
		{
			var attachment = Deserialize(@"""base64content""");
			attachment.Content.Should().Be("base64content");
			attachment.ContainsMetadata.Should().BeFalse();
		}

		[U] public void Read_Object_MapsAllFields()
		{
			var attachment = Deserialize(
				@"{""content"":""c"",""author"":""a"",""content_length"":10,""content_type"":""text/plain"",""detect_language"":true,""indexed_chars"":100,""keywords"":""k"",""language"":""en"",""name"":""n"",""title"":""t""}");

			attachment.Content.Should().Be("c");
			attachment.Author.Should().Be("a");
			attachment.ContentLength.Should().Be(10);
			attachment.ContentType.Should().Be("text/plain");
			attachment.DetectLanguage.Should().BeTrue();
			attachment.IndexedCharacters.Should().Be(100);
			attachment.Keywords.Should().Be("k");
			attachment.Language.Should().Be("en");
			attachment.Name.Should().Be("n");
			attachment.Title.Should().Be("t");
		}

		[U] public void Read_Object_HonorsUnderscoreAliases()
		{
			var attachment = Deserialize(
				@"{""_content"":""c"",""_name"":""n"",""_content_type"":""ct"",""_content_length"":5,""contentlength"":5,""_language"":""en"",""_detect_language"":false,""_indexed_chars"":7}");

			attachment.Content.Should().Be("c");
			attachment.Name.Should().Be("n");
			attachment.ContentType.Should().Be("ct");
			attachment.ContentLength.Should().Be(5);
			attachment.Language.Should().Be("en");
			attachment.DetectLanguage.Should().BeFalse();
			attachment.IndexedCharacters.Should().Be(7);
		}

		[U] public void Read_Object_SkipsUnknownFields()
		{
			var attachment = Deserialize(@"{""content"":""c"",""unknown"":{""nested"":[1,2]}}");
			attachment.Content.Should().Be("c");
		}

		[U] public void Read_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Write_NoMetadata_WritesBareString()
		{
			var json = JsonSerializer.Serialize(new Attachment { Content = "just-content" }, Options());
			json.Should().Be(@"""just-content""");
		}

		[U] public void Write_WithMetadata_WritesObject()
		{
			var attachment = new Attachment { Content = "c", Author = "a", ContentLength = 10, Name = "n" };
			var json = JsonSerializer.Serialize(attachment, Options());
			json.Should().Be(@"{""content"":""c"",""author"":""a"",""content_length"":10,""name"":""n""}");
		}

		[U] public void Write_Null_WritesNull() =>
			JsonSerializer.Serialize<Attachment>(null, Options()).Should().Be("null");

		[U] public void Roundtrip_ObjectForm()
		{
			var original = new Attachment { Content = "c", Author = "a", Date = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
			var json = JsonSerializer.Serialize(original, Options());
			var back = Deserialize(json);
			back.Content.Should().Be("c");
			back.Author.Should().Be("a");
			back.Date.Should().Be(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		}
	}
}
