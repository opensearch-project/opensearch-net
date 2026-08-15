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
	/// Behavioural tests for <see cref="BulkResponseItemConverter"/>: dispatches a <see cref="BulkResponseItemBase"/>
	/// to the concrete variant selected by the single wrapping operation-type key (<c>index</c>/<c>create</c>/
	/// <c>update</c>/<c>delete</c>) and deserializes the inner object body as that concrete type. Mirrors the read path
	/// of the legacy Utf8Json <c>BulkResponseItemFormatter</c> (whose <c>Serialize</c> threw
	/// <see cref="System.NotSupportedException"/> — preserved here).
	///
	/// The tests assert the dispatch outcome (concrete type + <c>Operation</c> discriminator) which is the converter's
	/// responsibility. Inner metadata members (<c>_index</c>, <c>_id</c>, ...) have <c>internal set</c> accessors that
	/// the shared contract resolver does not populate, so they are not asserted here.
	/// </summary>
	public class BulkResponseItemConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new BulkResponseItemConverter());
			return options;
		}

		private static BulkResponseItemBase Deserialize(string json) =>
			JsonSerializer.Deserialize<BulkResponseItemBase>(json, Options());

		[U] public void Deserialize_Index_DispatchesConcreteType()
		{
			var item = Deserialize(@"{""index"":{""_index"":""i"",""_id"":""1"",""status"":201}}");
			item.Should().BeOfType<BulkIndexResponseItem>();
			item.Operation.Should().Be("index");
		}

		[U] public void Deserialize_Create_DispatchesConcreteType()
		{
			var item = Deserialize(@"{""create"":{""_index"":""i"",""_id"":""1"",""status"":201}}");
			item.Should().BeOfType<BulkCreateResponseItem>();
			item.Operation.Should().Be("create");
		}

		[U] public void Deserialize_Update_DispatchesConcreteType()
		{
			var item = Deserialize(@"{""update"":{""_index"":""i"",""_id"":""1"",""status"":200}}");
			item.Should().BeOfType<BulkUpdateResponseItem>();
			item.Operation.Should().Be("update");
		}

		[U] public void Deserialize_Delete_DispatchesConcreteType()
		{
			var item = Deserialize(@"{""delete"":{""_index"":""i"",""_id"":""1"",""status"":200}}");
			item.Should().BeOfType<BulkDeleteResponseItem>();
			item.Operation.Should().Be("delete");
		}

		[U] public void Deserialize_UnknownOperation_ReturnsNull()
		{
			Deserialize(@"{""frobnicate"":{""_index"":""i""}}").Should().BeNull();
		}

		[U] public void Deserialize_NonObject_ReturnsNull()
		{
			// Legacy: any token that was not BeginObject was skipped and null returned.
			Deserialize("123").Should().BeNull();
			Deserialize(@"[1,2,3]").Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Deserialize_EmptyObject_ReturnsNull()
		{
			// No property to dispatch on => null, matching the legacy switch leaving the item null.
			Deserialize("{}").Should().BeNull();
		}

		[U] public void Deserialize_OnlyFirstKeyConsidered()
		{
			// The legacy read exactly one property name; the first key selects the concrete type.
			var item = Deserialize(@"{""delete"":{""status"":200},""index"":{""status"":201}}");
			item.Should().BeOfType<BulkDeleteResponseItem>();
		}

		[U] public void Serialize_Throws()
		{
			var converter = new BulkResponseItemConverter();
			System.Action write = () =>
			{
				var buffer = new System.Buffers.ArrayBufferWriter<byte>();
				using var writer = new Utf8JsonWriter(buffer);
				converter.Write(writer, new BulkIndexResponseItem(), Options());
			};
			write.Should().Throw<System.NotSupportedException>();
		}
	}
}
