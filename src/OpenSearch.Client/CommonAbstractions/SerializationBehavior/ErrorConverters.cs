/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="ErrorCause"/>, replacing the vendored
	/// Utf8Json <c>ErrorCauseFormatter</c> as part of #388. Reads the OpenSearch server <c>error</c>
	/// cause object: a bare string becomes <see cref="ErrorCause.Reason"/>; an object is switched on the
	/// exact server wire property names, with any unmapped property collected into
	/// <see cref="ErrorCause.AdditionalProperties"/>.
	/// </summary>
	internal sealed class ErrorCauseConverter : JsonConverter<ErrorCause>
	{
		public override ErrorCause Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			ReadErrorCause(ref reader, options);

		public override void Write(Utf8JsonWriter writer, ErrorCause value, JsonSerializerOptions options) =>
			WriteErrorCause(writer, value, options);

		/// <summary>Reads an <see cref="ErrorCause"/> from the reader's current token.</summary>
		internal static ErrorCause ReadErrorCause(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new ErrorCause { Reason = reader.GetString() };
				case JsonTokenType.StartObject:
					var errorCause = new ErrorCause();
					ReadObject(ref reader, errorCause, options);
					return errorCause;
				default:
					reader.Skip();
					return null;
			}
		}

		/// <summary>Writes an <see cref="ErrorCause"/> (or <see cref="Error"/>) mirroring the vendored serializer.</summary>
		internal static void WriteErrorCause(Utf8JsonWriter writer, ErrorCause value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.BytesLimit.HasValue)
				writer.WriteNumber("bytes_limit", value.BytesLimit.Value);

			if (value.BytesWanted.HasValue)
				writer.WriteNumber("bytes_wanted", value.BytesWanted.Value);

			if (value.CausedBy != null)
			{
				writer.WritePropertyName("caused_by");
				WriteErrorCause(writer, value.CausedBy, options);
			}

			if (value.Column.HasValue)
				writer.WriteNumber("col", value.Column.Value);

			if (value.FailedShards != null && value.FailedShards.Any())
			{
				writer.WritePropertyName("failed_shards");
				JsonSerializer.Serialize(writer, value.FailedShards, options);
			}

			if (value.Grouped.HasValue)
				writer.WriteBoolean("grouped", value.Grouped.Value);

			if (value.Index != null)
				writer.WriteString("index", value.Index);

			if (value.IndexUUID != null)
				writer.WriteString("index_uuid", value.IndexUUID);

			if (value.Language != null)
				writer.WriteString("lang", value.Language);

			if (value.Line.HasValue)
				writer.WriteNumber("line", value.Line.Value);

			if (value.Phase != null)
				writer.WriteString("phase", value.Phase);

			if (value.Reason != null)
				writer.WriteString("reason", value.Reason);

			if (value.ResourceId != null && value.ResourceId.Any())
			{
				writer.WritePropertyName("resource.id");
				JsonSerializer.Serialize(writer, value.ResourceId, options);
			}

			if (value.ResourceType != null)
				writer.WriteString("resource.type", value.ResourceType);

			if (value.Script != null)
				writer.WriteString("script", value.Script);

			if (value.ScriptStack != null && value.ScriptStack.Any())
			{
				writer.WritePropertyName("script_stack");
				JsonSerializer.Serialize(writer, value.ScriptStack, options);
			}

			if (value.Shard.HasValue)
				writer.WriteNumber("shard", value.Shard.Value);

			if (value.StackTrace != null)
				writer.WriteString("stack_trace", value.StackTrace);

			if (value.Type != null)
				writer.WriteString("type", value.Type);

			// Error-only extras (headers, root_cause) are emitted between the mapped
			// fields and the additional properties, matching the vendored ErrorFormatter.
			if (value is Error error)
				WriteErrorExtras(writer, error, options);

			if (value.AdditionalProperties != null && value.AdditionalProperties.Any())
			{
				foreach (var additionalProperty in value.AdditionalProperties)
				{
					writer.WritePropertyName(additionalProperty.Key);
					JsonSerializer.Serialize(writer, additionalProperty.Value, options);
				}
			}

			writer.WriteEndObject();
		}

		private static void WriteErrorExtras(Utf8JsonWriter writer, Error error, JsonSerializerOptions options)
		{
			if (error.Headers != null && error.Headers.Any())
			{
				writer.WritePropertyName("headers");
				JsonSerializer.Serialize(writer, error.Headers, options);
			}

			if (error.RootCause != null && error.RootCause.Any())
			{
				writer.WritePropertyName("root_cause");
				writer.WriteStartArray();
				foreach (var rootCause in error.RootCause)
					WriteErrorCause(writer, rootCause, options);
				writer.WriteEndArray();
			}
		}

		/// <summary>Reads the body of a top-level <see cref="Error"/> object (including its extra fields).</summary>
		internal static void ReadErrorObject(ref Utf8JsonReader reader, Error error, JsonSerializerOptions options) =>
			ReadObject(ref reader, error, options);

		/// <summary>
		/// Reads the body of an <c>error</c> object into <paramref name="target"/>. When the target is an
		/// <see cref="Error"/>, the <c>headers</c> and <c>root_cause</c> fields are also honoured. Unmapped
		/// properties are collected into <see cref="ErrorCause.AdditionalProperties"/>.
		/// </summary>
		private static void ReadObject<T>(ref Utf8JsonReader reader, T target, JsonSerializerOptions options)
			where T : ErrorCause
		{
			var additionalProperties = new Dictionary<string, object>();
			var error = target as Error;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				var propertyName = reader.GetString();
				reader.Read(); // advance to the value token

				if (error != null && ReadErrorField(ref reader, propertyName, error, options))
					continue;

				if (ReadErrorCauseField(ref reader, propertyName, target, options))
					continue;

				additionalProperties[propertyName] = JsonSerializer.Deserialize<object>(ref reader, options);
			}

			target.AdditionalProperties = additionalProperties;
		}

		private static bool ReadErrorCauseField(ref Utf8JsonReader reader, string name, ErrorCause target, JsonSerializerOptions options)
		{
			switch (name)
			{
				case "bytes_limit":
					target.BytesLimit = ReadNullableInt64(ref reader);
					return true;
				case "bytes_wanted":
					target.BytesWanted = ReadNullableInt64(ref reader);
					return true;
				case "caused_by":
					target.CausedBy = ReadErrorCause(ref reader, options);
					return true;
				case "col":
					target.Column = ReadNullableInt32(ref reader);
					return true;
				case "failed_shards":
					target.FailedShards = JsonSerializer.Deserialize<List<ShardFailure>>(ref reader, options);
					return true;
				case "grouped":
					target.Grouped = ReadNullableBoolean(ref reader);
					return true;
				case "index":
					target.Index = reader.GetString();
					return true;
				case "index_uuid":
					target.IndexUUID = reader.GetString();
					return true;
				case "lang":
					target.Language = reader.GetString();
					return true;
				case "line":
					target.Line = ReadNullableInt32(ref reader);
					return true;
				case "phase":
					target.Phase = reader.GetString();
					return true;
				case "reason":
					target.Reason = reader.GetString();
					return true;
				case "resource.id":
					target.ResourceId = ReadSingleOrEnumerableString(ref reader, options);
					return true;
				case "resource.type":
					target.ResourceType = reader.GetString();
					return true;
				case "script":
					target.Script = reader.GetString();
					return true;
				case "script_stack":
					target.ScriptStack = ReadSingleOrEnumerableString(ref reader, options);
					return true;
				case "shard":
					target.Shard = ReadStringOrInt32(ref reader);
					return true;
				case "stack_trace":
					target.StackTrace = reader.GetString();
					return true;
				case "type":
					target.Type = reader.GetString();
					return true;
				default:
					return false;
			}
		}

		private static bool ReadErrorField(ref Utf8JsonReader reader, string name, Error target, JsonSerializerOptions options)
		{
			switch (name)
			{
				case "headers":
					target.Headers = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
					return true;
				case "root_cause":
					target.RootCause = ReadErrorCauseList(ref reader, options);
					return true;
				default:
					return false;
			}
		}

		private static IReadOnlyCollection<ErrorCause> ReadErrorCauseList(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.StartArray:
					var list = new List<ErrorCause>();
					while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
						list.Add(ReadErrorCause(ref reader, options));
					return list;
				default:
					return new List<ErrorCause> { ReadErrorCause(ref reader, options) };
			}
		}

		private static IReadOnlyCollection<string> ReadSingleOrEnumerableString(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
				return JsonSerializer.Deserialize<List<string>>(ref reader, options);

			return new ReadOnlyCollection<string>(new List<string>(1) { reader.GetString() });
		}

		private static int? ReadNullableInt32(ref Utf8JsonReader reader) =>
			reader.TokenType == JsonTokenType.Number ? reader.GetInt32() : (int?)null;

		private static long? ReadNullableInt64(ref Utf8JsonReader reader) =>
			reader.TokenType == JsonTokenType.Number ? reader.GetInt64() : (long?)null;

		private static bool? ReadNullableBoolean(ref Utf8JsonReader reader) =>
			reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False
				? reader.GetBoolean()
				: (bool?)null;

		/// <summary>
		/// Reads the <c>shard</c> field which the server may send either as a number or as a string.
		/// Mirrors the vendored <c>NullableStringIntFormatter</c>.
		/// </summary>
		private static int? ReadStringOrInt32(ref Utf8JsonReader reader)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					return reader.GetInt32();
				case JsonTokenType.String:
					var s = reader.GetString();
					if (!int.TryParse(s, out var i))
						throw new JsonException($"Cannot parse {typeof(int).FullName} from: {s}");
					return i;
				default:
					throw new JsonException($"Cannot parse {typeof(int).FullName} from: {reader.TokenType}");
			}
		}
	}

	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="Error"/>, replacing the vendored Utf8Json
	/// <c>ErrorFormatter</c> as part of #388. Behaves like <see cref="ErrorCauseConverter"/> but additionally
	/// reads/writes the <c>headers</c> and <c>root_cause</c> fields that only exist on the top-level error.
	/// </summary>
	internal sealed class ErrorConverter : JsonConverter<Error>
	{
		public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			ReadError(ref reader, options);

		public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options) =>
			ErrorCauseConverter.WriteErrorCause(writer, value, options);

		internal static Error ReadError(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new Error { Reason = reader.GetString() };
				case JsonTokenType.StartObject:
					var error = new Error();
					ErrorCauseConverter.ReadErrorObject(ref reader, error, options);
					return error;
				default:
					reader.Skip();
					return null;
			}
		}
	}
}
