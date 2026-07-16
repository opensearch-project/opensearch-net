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
	/// Behavioural tests for <see cref="AliasActionConverter"/>: dispatches an <see cref="IAliasAction"/> to the
	/// concrete variant selected by the single wrapping key (<c>add</c> / <c>remove</c> / <c>remove_index</c>), reads
	/// the whole nested body, and writes by runtime type. Mirrors the legacy Utf8Json <c>AliasActionFormatter</c>.
	///
	/// The wrapping keys (<c>add</c>/<c>remove</c>/<c>remove_index</c>) come from the action interfaces which carry
	/// <c>[InterfaceDataContract]</c>, so they are parity-preserved and asserted directly. The inner operation body
	/// types (<c>AliasAddOperation</c> etc.) are plain classes NOT opted into the data-contract model, so under the
	/// current shared resolver their multi-word members (e.g. <c>is_write_index</c>, <c>must_exist</c>) currently
	/// serialize as camelCase rather than legacy snake_case; that is a resolver-level concern, so multi-word members
	/// are verified via round-trip (internally consistent) rather than by asserting a literal field name.
	/// </summary>
	public class AliasActionConverterTests
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
			// Bodies contain IndexName / Indices members which need the settings-aware member converters.
			options.Converters.Add(new IndexNameConverter(settings));
			options.Converters.Add(new IndicesMultiSyntaxConverter(settings));
			options.Converters.Add(new AliasActionConverter());
			return options;
		}

		private static IAliasAction Deserialize(string json) =>
			JsonSerializer.Deserialize<IAliasAction>(json, Options());

		[U] public void Deserialize_Add_DispatchesConcreteType()
		{
			var action = Deserialize(@"{""add"":{""index"":""my-index"",""alias"":""my-alias""}}");
			action.Should().BeOfType<AliasAddAction>();
			var add = ((AliasAddAction)action).Add;
			add.Should().NotBeNull();
			add.Alias.Should().Be("my-alias");
			add.Index.Should().Be((IndexName)"my-index");
		}

		[U] public void Deserialize_Remove_DispatchesConcreteType()
		{
			var action = Deserialize(@"{""remove"":{""index"":""my-index"",""alias"":""my-alias""}}");
			action.Should().BeOfType<AliasRemoveAction>();
			var remove = ((AliasRemoveAction)action).Remove;
			remove.Should().NotBeNull();
			remove.Alias.Should().Be("my-alias");
			remove.Index.Should().Be((IndexName)"my-index");
		}

		[U] public void Deserialize_RemoveIndex_DispatchesConcreteType()
		{
			var action = Deserialize(@"{""remove_index"":{""index"":""my-index""}}");
			action.Should().BeOfType<AliasRemoveIndexAction>();
			((AliasRemoveIndexAction)action).RemoveIndex.Index.Should().Be((IndexName)"my-index");
		}

		[U] public void Deserialize_UnknownKey_ReturnsNull()
		{
			Deserialize(@"{""unknown"":{}}").Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Serialize_Add_WrapsInAddKey()
		{
			IAliasAction action = new AliasAddAction { Add = new AliasAddOperation { Index = "my-index", Alias = "my-alias" } };
			var json = JsonSerializer.Serialize(action, Options());
			json.Should().Be(@"{""add"":{""alias"":""my-alias"",""index"":""my-index""}}");
		}

		[U] public void Serialize_Remove_WrapsInRemoveKey()
		{
			IAliasAction action = new AliasRemoveAction { Remove = new AliasRemoveOperation { Index = "my-index", Alias = "my-alias" } };
			var json = JsonSerializer.Serialize(action, Options());
			json.Should().Be(@"{""remove"":{""alias"":""my-alias"",""index"":""my-index""}}");
		}

		[U] public void Serialize_RemoveIndex_WrapsInRemoveIndexKey()
		{
			IAliasAction action = new AliasRemoveIndexAction { RemoveIndex = new AliasRemoveIndexOperation { Index = "my-index" } };
			var json = JsonSerializer.Serialize(action, Options());
			json.Should().Be(@"{""remove_index"":{""index"":""my-index""}}");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<IAliasAction>(null, Options()).Should().Be("null");
		}

		[U] public void RoundTrip_Add_PreservesAllMembers()
		{
			IAliasAction action = new AliasAddAction
			{
				Add = new AliasAddOperation { Index = "my-index", Alias = "my-alias", IsWriteIndex = true, IsHidden = false }
			};
			var back = JsonSerializer.Deserialize<IAliasAction>(JsonSerializer.Serialize(action, Options()), Options());
			back.Should().BeOfType<AliasAddAction>();
			var add = ((AliasAddAction)back).Add;
			add.Alias.Should().Be("my-alias");
			add.Index.Should().Be((IndexName)"my-index");
			add.IsWriteIndex.Should().BeTrue();
			add.IsHidden.Should().BeFalse();
		}

		[U] public void RoundTrip_Remove_PreservesMustExist()
		{
			IAliasAction action = new AliasRemoveAction
			{
				Remove = new AliasRemoveOperation { Index = "my-index", Alias = "my-alias", MustExist = true }
			};
			var back = JsonSerializer.Deserialize<IAliasAction>(JsonSerializer.Serialize(action, Options()), Options());
			back.Should().BeOfType<AliasRemoveAction>();
			((AliasRemoveAction)back).Remove.MustExist.Should().BeTrue();
		}

		[U] public void RoundTrip_RemoveIndex_PreservesIndex()
		{
			IAliasAction action = new AliasRemoveIndexAction { RemoveIndex = new AliasRemoveIndexOperation { Index = "my-index" } };
			var back = JsonSerializer.Deserialize<IAliasAction>(JsonSerializer.Serialize(action, Options()), Options());
			back.Should().BeOfType<AliasRemoveIndexAction>();
			((AliasRemoveIndexAction)back).RemoveIndex.Index.Should().Be((IndexName)"my-index");
		}
	}
}
