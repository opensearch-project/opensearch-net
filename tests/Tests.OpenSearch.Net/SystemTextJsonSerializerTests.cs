/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Text;
using System.Threading.Tasks;
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

		// A response body can be empty, whitespace-only, or an absent (null/Stream.Null) stream — the HEAD used by
		// Ping, or a 200 with no body. The built-in System.Text.Json reader throws "The input does not contain any
		// JSON tokens" on such input; the serializer reads the stream fully and treats a blank payload as
		// default/null (matching the legacy Utf8Json engine). These tests pin that behaviour so the empty-body path
		// cannot silently regress into a throw.

		[U] public void Deserialize_EmptyStream_ReturnsNullForReferenceType()
		{
			using var stream = new MemoryStream();
			Serializer.Deserialize<ErrorCause>(stream).Should().BeNull();
		}

		[U] public void Deserialize_WhitespaceOnlyStream_ReturnsNullForReferenceType()
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(" \t\r\n "));
			Serializer.Deserialize<ErrorCause>(stream).Should().BeNull();
		}

		[U] public void Deserialize_NullStream_ReturnsNullForReferenceType() =>
			Serializer.Deserialize<ErrorCause>(Stream.Null).Should().BeNull();

		[U] public void Deserialize_EmptyStream_ReturnsDefaultForValueType()
		{
			using var stream = new MemoryStream();
			Serializer.Deserialize<int>(stream).Should().Be(0);
		}

		[U] public void DeserializeNonGeneric_EmptyStream_ReturnsNullForReferenceType()
		{
			using var stream = new MemoryStream();
			Serializer.Deserialize(typeof(ErrorCause), stream).Should().BeNull();
		}

		[U] public void DeserializeNonGeneric_EmptyStream_ReturnsBoxedDefaultForValueType()
		{
			using var stream = new MemoryStream();
			Serializer.Deserialize(typeof(int), stream).Should().Be(0);
		}

		[U] public async Task DeserializeAsync_EmptyStream_ReturnsNullForReferenceType()
		{
			using var stream = new MemoryStream();
			(await Serializer.DeserializeAsync<ErrorCause>(stream)).Should().BeNull();
		}

		[U] public async Task DeserializeAsync_WhitespaceOnlyStream_ReturnsDefaultForValueType()
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes("   "));
			(await Serializer.DeserializeAsync<int>(stream)).Should().Be(0);
		}

		[U] public void Serialize_NullData_WritesNothing()
		{
			using var stream = new MemoryStream();
			Serializer.Serialize<ErrorCause>(null, stream);
			stream.ToArray().Should().BeEmpty();
		}
	}
}
