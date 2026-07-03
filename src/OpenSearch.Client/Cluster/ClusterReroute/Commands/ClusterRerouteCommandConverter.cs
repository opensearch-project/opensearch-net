/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="IClusterRerouteCommand"/>,
	/// replacing the vendored Utf8Json <c>ClusterRerouteCommandFormatter</c> as part of #388. Each command
	/// is a single-property object <c>{ "&lt;name&gt;": { …body… } }</c>; on write the concrete runtime type
	/// is serialized under its <see cref="IClusterRerouteCommand.Name"/>, on read the property name selects
	/// the concrete command type.
	/// </summary>
	internal sealed class ClusterRerouteCommandConverter : JsonConverter<IClusterRerouteCommand>
	{
		private static readonly Dictionary<string, Type> NameToType = new(StringComparer.Ordinal)
		{
			{ "allocate_replica", typeof(AllocateReplicaClusterRerouteCommand) },
			{ "allocate_empty_primary", typeof(AllocateEmptyPrimaryRerouteCommand) },
			{ "allocate_stale_primary", typeof(AllocateStalePrimaryRerouteCommand) },
			{ "move", typeof(MoveClusterRerouteCommand) },
			{ "cancel", typeof(CancelClusterRerouteCommand) },
		};

		public override void Write(Utf8JsonWriter writer, IClusterRerouteCommand value, JsonSerializerOptions options)
		{
			if (value?.Name == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(value.Name);
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
			writer.WriteEndObject();
		}

		public override IClusterRerouteCommand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) { reader.Skip(); return null; }

			using var document = JsonDocument.ParseValue(ref reader);
			foreach (var member in document.RootElement.EnumerateObject())
			{
				if (NameToType.TryGetValue(member.Name, out var type))
					return (IClusterRerouteCommand)member.Value.Deserialize(type, options);
				break;
			}

			return null;
		}
	}
}
