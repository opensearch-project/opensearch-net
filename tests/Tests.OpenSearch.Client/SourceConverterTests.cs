/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Tests <see cref="SourceConverter{T}"/>'s reader-position contract: a converter's Read must leave the reader on
	/// the LAST token of the value it consumed (here, the Null token), not advance past it. The null branch previously
	/// called an extra reader.Read(); when the null value is followed by a sibling token, over-reading leaves the
	/// reader on that sibling, so the enclosing reader then skips it.
	/// </summary>
	public class SourceConverterTests
	{
		public class Doc
		{
			public string Name { get; set; }
		}

		private static SourceConverter<Doc> Converter()
		{
			var settings = new ConnectionSettings(new SingleNodeConnectionPool(new System.Uri("http://localhost:9200")));
			return new SourceConverter<Doc>(settings);
		}

		[U] public void Read_NullValueFollowedBySibling_LeavesReaderOnNullNotSibling()
		{
			// {"a":null,"b":1} — position the reader on a's null value, call Read, and assert it stays on the Null
			// token. If Read over-reads, the reader lands on the "b" property name and the caller would skip it.
			var bytes = Encoding.UTF8.GetBytes(@"{""a"":null,""b"":1}");
			var reader = new Utf8JsonReader(bytes);
			reader.Read();                       // StartObject
			reader.Read();                       // PropertyName "a"
			reader.Read();                       // Null (a's value) — where the framework hands off to the converter
			reader.TokenType.Should().Be(JsonTokenType.Null);

			var result = Converter().Read(ref reader, typeof(Doc), new JsonSerializerOptions());

			result.Should().BeNull();
			// Contract: still on the Null token, so the outer reader advances to "b" next (not past it).
			reader.TokenType.Should().Be(JsonTokenType.Null,
				"Read must leave the reader on the value's last token, not advance onto the following property");

			reader.Read();
			reader.TokenType.Should().Be(JsonTokenType.PropertyName);
			reader.GetString().Should().Be("b", "the sibling property must still be readable, not skipped");
		}
	}
}
