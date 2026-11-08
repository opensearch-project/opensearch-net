/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ClusterRerouteCommandFormatter</c>.
	///
	/// An <see cref="IClusterRerouteCommand"/> is polymorphic and serialized as a single-key wrapper object whose
	/// property name is the command name and whose value is the command body:
	/// <c>{ "allocate_replica": { ... } }</c> → <see cref="AllocateReplicaClusterRerouteCommand"/>,
	/// <c>{ "allocate_empty_primary": { ... } }</c> → <see cref="AllocateEmptyPrimaryRerouteCommand"/>,
	/// <c>{ "allocate_stale_primary": { ... } }</c> → <see cref="AllocateStalePrimaryRerouteCommand"/>,
	/// <c>{ "move": { ... } }</c> → <see cref="MoveClusterRerouteCommand"/>,
	/// <c>{ "cancel": { ... } }</c> → <see cref="CancelClusterRerouteCommand"/>.
	///
	/// Unlike the alias action wrapper, the command-name key is NOT a member of the concrete type (it carries an
	/// <c>[IgnoreDataMember] Name</c>), so the converter itself writes the wrapping object and, on read, deserializes
	/// the inner body (the value of the command-name property) as the resolved concrete type.
	///
	/// System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound — unlike the Utf8Json
	/// version which read the property name in place — so we buffer the value into a <see cref="JsonDocument"/> and
	/// inspect the DOM to choose the concrete type.
	/// </summary>
	internal class ClusterRerouteCommandConverter : JsonConverter<IClusterRerouteCommand>
	{
		public override IClusterRerouteCommand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			IClusterRerouteCommand command = null;

			// The legacy formatter iterated every property, matched the first recognised command name, and ignored
			// any others; mirror that (last recognised wins, unrecognised keys skipped).
			foreach (var property in root.EnumerateObject())
			{
				switch (property.Name)
				{
					case "allocate_replica":
						command = property.Value.Deserialize<AllocateReplicaClusterRerouteCommand>(options);
						break;
					case "allocate_empty_primary":
						command = property.Value.Deserialize<AllocateEmptyPrimaryRerouteCommand>(options);
						break;
					case "allocate_stale_primary":
						command = property.Value.Deserialize<AllocateStalePrimaryRerouteCommand>(options);
						break;
					case "move":
						command = property.Value.Deserialize<MoveClusterRerouteCommand>(options);
						break;
					case "cancel":
						command = property.Value.Deserialize<CancelClusterRerouteCommand>(options);
						break;
				}
			}

			return command;
		}

		public override void Write(Utf8JsonWriter writer, IClusterRerouteCommand value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(value.Name);

			switch (value.Name)
			{
				case "allocate_replica":
					Serialize<IAllocateReplicaClusterRerouteCommand>(writer, value, options);
					break;
				case "allocate_empty_primary":
					Serialize<IAllocateEmptyPrimaryRerouteCommand>(writer, value, options);
					break;
				case "allocate_stale_primary":
					Serialize<IAllocateStalePrimaryRerouteCommand>(writer, value, options);
					break;
				case "move":
					Serialize<IMoveClusterRerouteCommand>(writer, value, options);
					break;
				case "cancel":
					Serialize<ICancelClusterRerouteCommand>(writer, value, options);
					break;
				default:
					// Fallback for custom commands: serialize by runtime type.
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}

			writer.WriteEndObject();
		}

		private static void Serialize<TCommand>(Utf8JsonWriter writer, IClusterRerouteCommand value, JsonSerializerOptions options)
			where TCommand : class, IClusterRerouteCommand =>
			JsonSerializer.Serialize(writer, value as TCommand, options);
	}
}
