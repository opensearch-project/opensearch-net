/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Abstract base converter for queries that use a field-name wrapper pattern.
	/// These queries serialize as: <c>{"field_name": {&lt;properties&gt;}}</c>
	/// Examples: match, term, prefix, wildcard, fuzzy, regexp, range, etc.
	/// </summary>
	/// <typeparam name="TQuery">The query interface type (e.g. IMatchQuery, ITermQuery)</typeparam>
	/// <typeparam name="TConcrete">The concrete query type (e.g. MatchQuery, TermQuery)</typeparam>
	internal abstract class FieldNameQueryConverterBase<TQuery, TConcrete> : JsonConverter<TQuery>
		where TQuery : class, IFieldNameQuery
		where TConcrete : class, TQuery, new()
	{
		public override TQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject token but got {reader.TokenType}");

			reader.Read(); // Move past StartObject

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected field name property in field-name query");

			// The first property name is the field name
			var fieldName = reader.GetString();
			reader.Read(); // Move past PropertyName

			var query = new TConcrete();
			((IFieldNameQuery)query).Field = new Field(fieldName);

			if (reader.TokenType == JsonTokenType.StartObject)
			{
				// Standard case: {"field_name": {"query": "...", "boost": 1.0}}
				ReadInnerProperties(ref reader, query, options);
			}
			else
			{
				// Short-form case: {"field_name": "value"} (e.g. term query short form)
				ReadShortForm(ref reader, query, options);
			}

			reader.Read(); // Move past the outer EndObject

			return query;
		}

		public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			var field = ((IFieldNameQuery)value).Field;
			var fieldName = field?.ToString() ?? throw new JsonException("Field name cannot be null for a field-name query");

			writer.WritePropertyName(fieldName);
			writer.WriteStartObject();

			WriteInnerProperties(writer, value, options);

			// Write common IQuery properties
			if (value.Boost.HasValue)
			{
				writer.WritePropertyName("boost");
				writer.WriteNumberValue(value.Boost.Value);
			}

			if (!string.IsNullOrEmpty(value.Name))
			{
				writer.WritePropertyName("_name");
				writer.WriteStringValue(value.Name);
			}

			writer.WriteEndObject();
			writer.WriteEndObject();
		}

		/// <summary>
		/// Read the inner properties of the query object (inside the field-name wrapper).
		/// The reader is positioned at the StartObject token of the inner object.
		/// Implementations must consume through the matching EndObject.
		/// </summary>
		protected abstract void ReadInnerProperties(ref Utf8JsonReader reader, TConcrete query, JsonSerializerOptions options);

		/// <summary>
		/// Write the inner properties of the query (inside the field-name wrapper).
		/// The writer is positioned inside the inner object. Implementations should NOT
		/// write StartObject/EndObject for the inner object.
		/// </summary>
		protected abstract void WriteInnerProperties(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options);

		/// <summary>
		/// Handle the short-form of a field-name query (e.g. {"field": "value"} instead of {"field": {"value": "..."}}).
		/// Default implementation throws; override for queries that support short form.
		/// </summary>
		protected virtual void ReadShortForm(ref Utf8JsonReader reader, TConcrete query, JsonSerializerOptions options)
		{
			throw new JsonException(
				$"Unexpected token {reader.TokenType} when reading {typeof(TQuery).Name}. " +
				"Expected an object with query properties.");
		}
	}
}
