/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="ErrorCauseConverter"/> / <see cref="ErrorConverter"/>, the System.Text.Json
	/// replacement for the legacy Utf8Json ErrorCause/Error formatters.
	/// </summary>
	public class ErrorCauseConverterTests
	{
		private static JsonSerializerOptions ErrorCauseOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new ErrorCauseConverter());
			return options;
		}

		private static JsonSerializerOptions ErrorOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new ErrorConverter());
			return options;
		}

		[U] public void Read_BareString_BecomesReason()
		{
			var ec = JsonSerializer.Deserialize<ErrorCause>(@"""something broke""", ErrorCauseOptions());
			ec.Reason.Should().Be("something broke");
		}

		[U] public void Read_Object_ReadsKnownFields()
		{
			var json = @"{""type"":""parse_exception"",""reason"":""bad"",""line"":3,""col"":5,""index"":""i1""}";
			var ec = JsonSerializer.Deserialize<ErrorCause>(json, ErrorCauseOptions());

			ec.Type.Should().Be("parse_exception");
			ec.Reason.Should().Be("bad");
			ec.Line.Should().Be(3);
			ec.Column.Should().Be(5);
			ec.Index.Should().Be("i1");
		}

		[U] public void Read_CausedBy_IsRecursive()
		{
			var json = @"{""type"":""outer"",""caused_by"":{""type"":""inner"",""reason"":""root""}}";
			var ec = JsonSerializer.Deserialize<ErrorCause>(json, ErrorCauseOptions());

			ec.Type.Should().Be("outer");
			ec.CausedBy.Should().NotBeNull();
			ec.CausedBy.Type.Should().Be("inner");
			ec.CausedBy.Reason.Should().Be("root");
		}

		[U] public void Read_UnknownFields_GoToAdditionalProperties()
		{
			var json = @"{""type"":""x"",""weird_field"":123}";
			var ec = JsonSerializer.Deserialize<ErrorCause>(json, ErrorCauseOptions());

			ec.AdditionalProperties.Should().ContainKey("weird_field");
			ec.AdditionalProperties["weird_field"].Should().Be(123L);
			// An integral unknown value must be a long, not a double: the conditional in ReadDynamicValue would
			// otherwise unify to double and box 123 as 123.0, unlike the legacy engine's Int64.
			ec.AdditionalProperties["weird_field"].Should().BeOfType<long>();
		}

		[U] public void Read_NestedUnknownFields_UseClrCollections()
		{
			// Nested unknown objects/arrays must materialise as Dictionary<string, object> / List<object> with native
			// scalar values (long, not double), matching the legacy Utf8Json dynamic reads. Buffering into a JsonElement
			// would leak an engine-specific DOM type that callers cannot index or enumerate as they did before.
			var json = @"{""nested"":{""number"":1},""items"":[1,2]}";
			var ec = JsonSerializer.Deserialize<ErrorCause>(json, ErrorCauseOptions());

			var nested = ec.AdditionalProperties["nested"].Should()
				.BeOfType<Dictionary<string, object>>().Which;
			nested["number"].Should().Be(1L);

			var items = ec.AdditionalProperties["items"].Should()
				.BeOfType<List<object>>().Which;
			items.Should().Equal(1L, 2L);
		}

		[U] public void Read_ResourceId_AcceptsSingleValue()
		{
			// resource.id uses the single-or-array converter: a bare string becomes a one-element collection.
			var ec = JsonSerializer.Deserialize<ErrorCause>(@"{""resource.id"":""idx""}", ErrorCauseOptions());
			ec.ResourceId.Should().ContainSingle().Which.Should().Be("idx");
		}

		[U] public void Read_ShardAsString_UsesNullableStringInt()
		{
			// shard uses the NullableStringInt converter: accepts a numeric string.
			var ec = JsonSerializer.Deserialize<ErrorCause>(@"{""shard"":""7""}", ErrorCauseOptions());
			ec.Shard.Should().Be(7);
		}

		[U] public void RoundTrip_KnownFields()
		{
			var json = @"{""type"":""t"",""reason"":""r"",""line"":1}";
			var ec = JsonSerializer.Deserialize<ErrorCause>(json, ErrorCauseOptions());
			var back = JsonSerializer.Serialize(ec, ErrorCauseOptions());

			// Re-parse to compare structurally (field order is deterministic here but compare via round-trip).
			var ec2 = JsonSerializer.Deserialize<ErrorCause>(back, ErrorCauseOptions());
			ec2.Type.Should().Be("t");
			ec2.Reason.Should().Be("r");
			ec2.Line.Should().Be(1);
		}

		[U] public void Error_ReadsHeadersAndRootCause()
		{
			var json = @"{""type"":""e"",""headers"":{""h1"":""v1""},""root_cause"":[{""type"":""rc""}]}";
			var err = JsonSerializer.Deserialize<Error>(json, ErrorOptions());

			err.Type.Should().Be("e");
			err.Headers.Should().ContainKey("h1");
			err.Headers["h1"].Should().Be("v1");
			err.RootCause.Should().ContainSingle().Which.Type.Should().Be("rc");
		}

		[U] public void Error_WritesHeadersAndRootCause()
		{
			var json = @"{""type"":""e"",""headers"":{""h1"":""v1""},""root_cause"":[{""type"":""rc""}]}";
			var err = JsonSerializer.Deserialize<Error>(json, ErrorOptions());
			var back = JsonSerializer.Serialize(err, ErrorOptions());

			back.Should().Contain(@"""headers""").And.Contain(@"""root_cause""").And.Contain(@"""rc""");
		}
	}
}
