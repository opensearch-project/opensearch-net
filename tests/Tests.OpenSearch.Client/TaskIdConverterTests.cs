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
	/// Behavioural tests for <see cref="TaskIdConverter"/>. A <see cref="TaskId"/> serializes as its fully-qualified
	/// <c>node:task</c> string; a JSON string reads back into a <see cref="TaskId"/> and any other token yields null.
	/// The converter also handles <see cref="TaskId"/> as a dictionary key (property name), mirroring the legacy
	/// formatter's <c>IObjectPropertyNameFormatter</c> implementation.
	/// </summary>
	public class TaskIdConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new TaskIdConverter());
			return options;
		}

		[U] public void Read_String()
		{
			var taskId = JsonSerializer.Deserialize<TaskId>(@"""node-1:42""", Options());
			taskId.Should().NotBeNull();
			taskId.NodeId.Should().Be("node-1");
			taskId.TaskNumber.Should().Be(42);
			taskId.FullyQualifiedId.Should().Be("node-1:42");
		}

		[U] public void Read_NonString_ReturnsNull()
		{
			var taskId = JsonSerializer.Deserialize<TaskId>("123", Options());
			taskId.Should().BeNull();
		}

		[U] public void Read_NullToken_ReturnsNull()
		{
			var taskId = JsonSerializer.Deserialize<TaskId>("null", Options());
			taskId.Should().BeNull();
		}

		[U] public void Read_ObjectToken_IsSkipped_ReturnsNull()
		{
			var taskId = JsonSerializer.Deserialize<TaskId>(@"{""a"":1}", Options());
			taskId.Should().BeNull();
		}

		[U] public void Write_Value()
		{
			var json = JsonSerializer.Serialize(new TaskId("node-1:42"), Options());
			json.Should().Be(@"""node-1:42""");
		}

		[U] public void Write_Null()
		{
			var json = JsonSerializer.Serialize<TaskId>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new TaskId("abc:99"), options);
			var taskId = JsonSerializer.Deserialize<TaskId>(json, options);
			taskId.Should().Be(new TaskId("abc:99"));
		}

		[U] public void Write_AsDictionaryKey()
		{
			var dict = new Dictionary<TaskId, int> { { new TaskId("node-1:42"), 7 } };
			var json = JsonSerializer.Serialize(dict, Options());
			json.Should().Be(@"{""node-1:42"":7}");
		}

		[U] public void Read_AsDictionaryKey()
		{
			var dict = JsonSerializer.Deserialize<Dictionary<TaskId, int>>(@"{""node-1:42"":7}", Options());
			dict.Should().ContainKey(new TaskId("node-1:42"));
			dict[new TaskId("node-1:42")].Should().Be(7);
		}
	}
}
