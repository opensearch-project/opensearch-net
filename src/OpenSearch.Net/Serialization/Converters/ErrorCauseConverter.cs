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

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ErrorCauseFormatter&lt;T&gt;</c>.
	///
	/// Reads an <see cref="ErrorCause"/> that OpenSearch may return either as a bare string (treated as the
	/// <see cref="ErrorCause.Reason"/>) or as an object with a fixed set of snake_case fields; unrecognised fields
	/// are collected into <see cref="ErrorCause.AdditionalProperties"/>. <c>caused_by</c> is recursive. Derived
	/// types extend the known-field set via the <see cref="ReadExtraField"/> / <see cref="WriteExtraFields"/> hooks.
	/// </summary>
	public class ErrorCauseConverter<TErrorCause> : JsonConverter<TErrorCause>
		where TErrorCause : ErrorCause, new()
	{
		// Sub-converters reused for special fields; mirrors the legacy ErrorCauseFormatterStatics.
		private static readonly NullableStringIntConverter NullableStringInt = new NullableStringIntConverter();
		private static readonly SingleOrManyCollectionConverter<string> SingleOrMany = new SingleOrManyCollectionConverter<string>();

		/// <summary>Hook for derived types to read a field not handled by the base set. Returns true if handled.</summary>
		protected virtual bool ReadExtraField(ref Utf8JsonReader reader, string field, TErrorCause value, JsonSerializerOptions options) => false;

		/// <summary>Hook for derived types to write their additional fields.</summary>
		protected virtual void WriteExtraFields(Utf8JsonWriter writer, TErrorCause value, JsonSerializerOptions options) { }

		public override TErrorCause Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new TErrorCause { Reason = reader.GetString() };
				case JsonTokenType.StartObject:
					return ReadObject(ref reader, options);
				default:
					reader.Skip();
					return null;
			}
		}

		private TErrorCause ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var value = new TErrorCause();
			var additional = new Dictionary<string, object>();
			value.AdditionalProperties = additional;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					return value;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name while reading ErrorCause.");

				var field = reader.GetString();
				reader.Read(); // advance to the value

				switch (field)
				{
					case "bytes_limit": value.BytesLimit = reader.GetInt64(); break;
					case "bytes_wanted": value.BytesWanted = reader.GetInt64(); break;
					case "caused_by": value.CausedBy = ReadCausedBy(ref reader, options); break;
					case "col": value.Column = reader.GetInt32(); break;
					case "failed_shards": value.FailedShards = JsonSerializer.Deserialize<List<ShardFailure>>(ref reader, options); break;
					case "grouped": value.Grouped = reader.GetBoolean(); break;
					case "index": value.Index = reader.GetString(); break;
					case "index_uuid": value.IndexUUID = reader.GetString(); break;
					case "lang": value.Language = reader.GetString(); break;
					case "line": value.Line = reader.GetInt32(); break;
					case "phase": value.Phase = reader.GetString(); break;
					case "reason": value.Reason = reader.GetString(); break;
					case "resource.id": value.ResourceId = SingleOrMany.Read(ref reader, typeof(IReadOnlyCollection<string>), options); break;
					case "resource.type": value.ResourceType = reader.GetString(); break;
					case "script": value.Script = reader.GetString(); break;
					case "script_stack": value.ScriptStack = SingleOrMany.Read(ref reader, typeof(IReadOnlyCollection<string>), options); break;
					case "shard": value.Shard = NullableStringInt.Read(ref reader, typeof(int?), options); break;
					case "stack_trace": value.StackTrace = reader.GetString(); break;
					case "type": value.Type = reader.GetString(); break;
					default:
						if (!ReadExtraField(ref reader, field, value, options))
							additional.Add(field, ReadDynamicValue(ref reader, options));
						break;
				}
			}

			throw new JsonException("Unexpected end of JSON while reading ErrorCause.");
		}

		private static ErrorCause ReadCausedBy(ref Utf8JsonReader reader, JsonSerializerOptions options) =>
			new ErrorCauseConverter<ErrorCause>().Read(ref reader, typeof(ErrorCause), options);

		// Reads an unknown value into a plain CLR object (mirrors the legacy object formatter fallback).
		private static object ReadDynamicValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: return null;
				case JsonTokenType.True: return true;
				case JsonTokenType.False: return false;
				case JsonTokenType.String: return reader.GetString();
				case JsonTokenType.Number:
					return reader.TryGetInt64(out var l) ? l : reader.GetDouble();
				case JsonTokenType.StartObject:
				case JsonTokenType.StartArray:
					using (var doc = JsonDocument.ParseValue(ref reader))
						return doc.RootElement.Clone();
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, TErrorCause value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.BytesLimit.HasValue) writer.WriteNumber("bytes_limit", value.BytesLimit.Value);
			if (value.BytesWanted.HasValue) writer.WriteNumber("bytes_wanted", value.BytesWanted.Value);
			if (value.CausedBy != null)
			{
				writer.WritePropertyName("caused_by");
				new ErrorCauseConverter<ErrorCause>().Write(writer, value.CausedBy, options);
			}
			if (value.Column.HasValue) writer.WriteNumber("col", value.Column.Value);
			if (Any(value.FailedShards))
			{
				writer.WritePropertyName("failed_shards");
				JsonSerializer.Serialize(writer, value.FailedShards, options);
			}
			if (value.Grouped.HasValue) writer.WriteBoolean("grouped", value.Grouped.Value);
			if (value.Index != null) writer.WriteString("index", value.Index);
			if (value.IndexUUID != null) writer.WriteString("index_uuid", value.IndexUUID);
			if (value.Language != null) writer.WriteString("lang", value.Language);
			if (value.Line.HasValue) writer.WriteNumber("line", value.Line.Value);
			if (value.Phase != null) writer.WriteString("phase", value.Phase);
			if (value.Reason != null) writer.WriteString("reason", value.Reason);
			if (Any(value.ResourceId))
			{
				writer.WritePropertyName("resource.id");
				SingleOrMany.Write(writer, value.ResourceId, options);
			}
			if (value.ResourceType != null) writer.WriteString("resource.type", value.ResourceType);
			if (value.Script != null) writer.WriteString("script", value.Script);
			if (Any(value.ScriptStack))
			{
				writer.WritePropertyName("script_stack");
				SingleOrMany.Write(writer, value.ScriptStack, options);
			}
			if (value.Shard.HasValue) writer.WriteNumber("shard", value.Shard.Value);
			if (value.StackTrace != null) writer.WriteString("stack_trace", value.StackTrace);
			if (value.Type != null) writer.WriteString("type", value.Type);

			WriteExtraFields(writer, value, options);

			if (value.AdditionalProperties != null)
			{
				foreach (var kv in value.AdditionalProperties)
				{
					writer.WritePropertyName(kv.Key);
					JsonSerializer.Serialize(writer, kv.Value, options);
				}
			}

			writer.WriteEndObject();
		}

		private static bool Any<T>(IEnumerable<T> items)
		{
			if (items == null) return false;
			foreach (var _ in items) return true;
			return false;
		}
	}

	/// <summary>Non-generic converter for the base <see cref="ErrorCause"/> type.</summary>
	public class ErrorCauseConverter : ErrorCauseConverter<ErrorCause> { }
}
