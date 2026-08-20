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
	/// Behavioural tests for <see cref="ClusterRerouteCommandConverter"/>: dispatches an
	/// <see cref="IClusterRerouteCommand"/> to the concrete variant selected by the wrapping command-name key
	/// (<c>allocate_replica</c> / <c>allocate_empty_primary</c> / <c>allocate_stale_primary</c> / <c>move</c> /
	/// <c>cancel</c>), reading the inner body and writing the wrapper by <c>Name</c>. Mirrors the legacy Utf8Json
	/// <c>ClusterRerouteCommandFormatter</c>.
	///
	/// The command-name wrapper key is produced by the converter itself, so it is asserted directly. The command
	/// interfaces are NOT marked <c>[InterfaceDataContract]</c>, so under the current shared resolver their
	/// interface-declared <c>[DataMember]</c> snake_case names (<c>from_node</c>, <c>accept_data_loss</c>,
	/// <c>allow_primary</c>) are not yet honoured — those multi-word members are therefore verified via round-trip
	/// (internally consistent) rather than by asserting a literal snake_case field name. Single-word members
	/// (<c>index</c>, <c>node</c>, <c>shard</c>) are stable and asserted from raw JSON.
	/// </summary>
	public class ClusterRerouteCommandConverterTests
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
			// Bodies contain IndexName members which need the settings-aware member converter.
			options.Converters.Add(new IndexNameConverter(settings));
			options.Converters.Add(new ClusterRerouteCommandConverter());
			return options;
		}

		private static IClusterRerouteCommand Deserialize(string json) =>
			JsonSerializer.Deserialize<IClusterRerouteCommand>(json, Options());

		[U] public void Deserialize_AllocateReplica_DispatchesConcreteType()
		{
			var command = Deserialize(@"{""allocate_replica"":{""index"":""my-index"",""shard"":0,""node"":""node-1""}}");
			command.Should().BeOfType<AllocateReplicaClusterRerouteCommand>();
			var c = (AllocateReplicaClusterRerouteCommand)command;
			c.Index.Should().Be((IndexName)"my-index");
			c.Shard.Should().Be(0);
			c.Node.Should().Be("node-1");
		}

		[U] public void Deserialize_AllocateEmptyPrimary_DispatchesConcreteType()
		{
			var command = Deserialize(@"{""allocate_empty_primary"":{""index"":""my-index"",""shard"":1,""node"":""node-1""}}");
			command.Should().BeOfType<AllocateEmptyPrimaryRerouteCommand>();
			((AllocateEmptyPrimaryRerouteCommand)command).Node.Should().Be("node-1");
		}

		[U] public void Deserialize_AllocateStalePrimary_DispatchesConcreteType()
		{
			var command = Deserialize(@"{""allocate_stale_primary"":{""index"":""my-index"",""shard"":2,""node"":""node-1""}}");
			command.Should().BeOfType<AllocateStalePrimaryRerouteCommand>();
			((AllocateStalePrimaryRerouteCommand)command).Shard.Should().Be(2);
		}

		[U] public void Deserialize_Move_DispatchesConcreteType()
		{
			var command = Deserialize(@"{""move"":{""index"":""my-index"",""shard"":3}}");
			command.Should().BeOfType<MoveClusterRerouteCommand>();
			var c = (MoveClusterRerouteCommand)command;
			c.Index.Should().Be((IndexName)"my-index");
			c.Shard.Should().Be(3);
		}

		[U] public void Deserialize_Cancel_DispatchesConcreteType()
		{
			var command = Deserialize(@"{""cancel"":{""index"":""my-index"",""shard"":4,""node"":""node-1""}}");
			command.Should().BeOfType<CancelClusterRerouteCommand>();
			((CancelClusterRerouteCommand)command).Node.Should().Be("node-1");
		}

		[U] public void Deserialize_UnknownKey_ReturnsNull()
		{
			Deserialize(@"{""unknown"":{}}").Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Serialize_AllocateReplica_WrapsInCommandNameKey()
		{
			IClusterRerouteCommand command = new AllocateReplicaClusterRerouteCommand { Index = "my-index", Shard = 0, Node = "node-1" };
			var json = JsonSerializer.Serialize(command, Options());
			json.Should().StartWith(@"{""allocate_replica"":{")
				.And.Contain(@"""index"":""my-index""")
				.And.Contain(@"""node"":""node-1""")
				.And.EndWith("}");
		}

		[U] public void Serialize_Move_WrapsInMoveKey()
		{
			IClusterRerouteCommand command = new MoveClusterRerouteCommand { Index = "my-index", Shard = 3 };
			var json = JsonSerializer.Serialize(command, Options());
			json.Should().StartWith(@"{""move"":{").And.Contain(@"""index"":""my-index""");
		}

		[U] public void Serialize_Cancel_WrapsInCancelKey()
		{
			IClusterRerouteCommand command = new CancelClusterRerouteCommand { Index = "my-index", Shard = 4, Node = "node-1" };
			var json = JsonSerializer.Serialize(command, Options());
			json.Should().StartWith(@"{""cancel"":{").And.Contain(@"""node"":""node-1""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<IClusterRerouteCommand>(null, Options()).Should().Be("null");
		}

		[U] public void RoundTrip_Move_PreservesAllMembers()
		{
			IClusterRerouteCommand command = new MoveClusterRerouteCommand { Index = "my-index", Shard = 3, FromNode = "node-1", ToNode = "node-2" };
			var back = JsonSerializer.Deserialize<IClusterRerouteCommand>(JsonSerializer.Serialize(command, Options()), Options());
			back.Should().BeOfType<MoveClusterRerouteCommand>();
			var c = (MoveClusterRerouteCommand)back;
			c.Index.Should().Be((IndexName)"my-index");
			c.Shard.Should().Be(3);
			c.FromNode.Should().Be("node-1");
			c.ToNode.Should().Be("node-2");
		}

		[U] public void RoundTrip_AllocateEmptyPrimary_PreservesAcceptDataLoss()
		{
			IClusterRerouteCommand command = new AllocateEmptyPrimaryRerouteCommand { Index = "my-index", Shard = 1, Node = "node-1", AcceptDataLoss = true };
			var back = JsonSerializer.Deserialize<IClusterRerouteCommand>(JsonSerializer.Serialize(command, Options()), Options());
			back.Should().BeOfType<AllocateEmptyPrimaryRerouteCommand>();
			((AllocateEmptyPrimaryRerouteCommand)back).AcceptDataLoss.Should().BeTrue();
		}

		[U] public void RoundTrip_AllocateStalePrimary_PreservesAcceptDataLoss()
		{
			IClusterRerouteCommand command = new AllocateStalePrimaryRerouteCommand { Index = "my-index", Shard = 2, Node = "node-1", AcceptDataLoss = true };
			var back = JsonSerializer.Deserialize<IClusterRerouteCommand>(JsonSerializer.Serialize(command, Options()), Options());
			back.Should().BeOfType<AllocateStalePrimaryRerouteCommand>();
			((AllocateStalePrimaryRerouteCommand)back).AcceptDataLoss.Should().BeTrue();
		}

		[U] public void RoundTrip_Cancel_PreservesAllowPrimary()
		{
			IClusterRerouteCommand command = new CancelClusterRerouteCommand { Index = "my-index", Shard = 4, Node = "node-1", AllowPrimary = true };
			var back = JsonSerializer.Deserialize<IClusterRerouteCommand>(JsonSerializer.Serialize(command, Options()), Options());
			back.Should().BeOfType<CancelClusterRerouteCommand>();
			((CancelClusterRerouteCommand)back).AllowPrimary.Should().BeTrue();
		}
	}
}
