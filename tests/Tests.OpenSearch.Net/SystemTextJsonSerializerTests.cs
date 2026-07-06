/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// End-to-end tests for <see cref="SystemTextJsonSerializer"/> — the low-level serializer that
	/// <see cref="ConnectionConfiguration"/> now uses by default. Verifies the registered converters
	/// (in particular ErrorCause/Error) work through the real serializer, not just when newed directly.
	/// </summary>
	public class SystemTextJsonSerializerTests
	{
		private static readonly SystemTextJsonSerializer Serializer = new SystemTextJsonSerializer();

		private static T Deserialize<T>(string json)
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
			return Serializer.Deserialize<T>(stream);
		}

		private static string Serialize<T>(T value)
		{
			using var stream = new MemoryStream();
			Serializer.Serialize(value, stream);
			return Encoding.UTF8.GetString(stream.ToArray());
		}

		[U] public void Deserializes_ErrorCause_ThroughSerializer()
		{
			var ec = Deserialize<ErrorCause>(@"{""type"":""parse_exception"",""reason"":""bad"",""line"":3}");

			ec.Type.Should().Be("parse_exception");
			ec.Reason.Should().Be("bad");
			ec.Line.Should().Be(3);
		}

		[U] public void Deserializes_Error_WithRootCause_ThroughSerializer()
		{
			var err = Deserialize<Error>(@"{""type"":""e"",""reason"":""r"",""root_cause"":[{""type"":""rc""}]}");

			err.Type.Should().Be("e");
			err.RootCause.Should().ContainSingle().Which.Type.Should().Be("rc");
		}

		[U] public void Serializes_ErrorCause_WithSnakeCaseFields()
		{
			var ec = Deserialize<ErrorCause>(@"{""type"":""t"",""index_uuid"":""abc""}");
			var json = Serialize(ec);

			// The custom converter must win over reflection: fields stay snake_case, not PascalCase.
			json.Should().Contain(@"""index_uuid""").And.Contain(@"""type""");
			json.Should().NotContain("IndexUUID");
		}
	}
}
