/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>TermsQueryFormatter</c>.
	///
	/// <see cref="ITermsQuery"/> is a field-name query serialized as a single-field object whose key is the query's
	/// <see cref="IFieldNameQuery.Field"/> (resolved through the runtime <c>Inferrer</c>, hence a
	/// <see cref="SettingsAwareConverter{T}"/>) alongside the common <c>_name</c>/<c>boost</c> options from
	/// <c>QueryBase</c>. The value under the field key is one of two shapes:
	/// <list type="bullet">
	/// <item><description>a JSON array — a verbatim terms list (<c>{ "field": [ ... ] }</c>);</description></item>
	/// <item><description>a JSON object — a terms-lookup (<c>{ "field": { "index":.., "id":.., "path":.., "routing":.. } }</c>).</description></item>
	/// </list>
	/// On write the legacy formatter dispatched on <c>IsVerbatim</c>: when verbatim it preferred <c>TermsLookup</c> then
	/// <c>Terms</c>; otherwise it preferred a non-empty <c>Terms</c> array then <c>TermsLookup</c>. That dispatch is
	/// reproduced exactly here, and the field wrapper is emitted by this converter (Write produces the wrapper and Read
	/// consumes it symmetrically). The reader is forward-only, matching the legacy sequential read.
	/// </summary>
	internal class TermsQueryConverter : SettingsAwareConverter<ITermsQuery>
	{
		public TermsQueryConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override ITermsQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
			{
				reader.Read();
				return null;
			}

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			ITermsQuery query = new TermsQuery();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var property = reader.GetString();
				reader.Read(); // advance to value

				switch (property)
				{
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "_name":
						query.Name = reader.GetString();
						break;
					default:
						query.Field = property;
						ReadTerms(ref reader, query, options);
						break;
				}
			}

			return query;
		}

		private static void ReadTerms(ref Utf8JsonReader reader, ITermsQuery query, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartObject:
					query.TermsLookup = JsonSerializer.Deserialize<FieldLookup>(ref reader, options);
					break;
				case JsonTokenType.StartArray:
					var terms = new List<object>();
					while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
						terms.Add(ReadTermValue(ref reader, options));
					query.Terms = terms;
					break;
				default:
					reader.Skip();
					break;
			}
		}

		private static object ReadTermValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return reader.GetString();
				case JsonTokenType.Number:
					return reader.TryGetInt64(out var l) ? l : (object)reader.GetDouble();
				case JsonTokenType.True:
				case JsonTokenType.False:
					return reader.GetBoolean();
				default:
					// Object / array terms are uncommon; buffer the DOM so the value round-trips faithfully.
					using (var doc = JsonDocument.ParseValue(ref reader))
						return doc.RootElement.Clone();
			}
		}

		public override void Write(Utf8JsonWriter writer, ITermsQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var field = Settings.Inferrer.Field(value.Field);

			writer.WriteStartObject();

			if (!value.Name.IsNullOrEmpty())
				writer.WriteString("_name", value.Name);

			if (value.Boost.HasValue)
				writer.WriteNumber("boost", value.Boost.Value);

			if (value.IsVerbatim)
			{
				if (value.TermsLookup != null)
				{
					writer.WritePropertyName(field);
					JsonSerializer.Serialize(writer, value.TermsLookup, options);
				}
				else if (value.Terms != null)
				{
					writer.WritePropertyName(field);
					WriteTerms(writer, value.Terms, options);
				}
			}
			else
			{
				if (value.Terms.HasAny())
				{
					writer.WritePropertyName(field);
					WriteTerms(writer, value.Terms, options);
				}
				else if (value.TermsLookup != null)
				{
					writer.WritePropertyName(field);
					JsonSerializer.Serialize(writer, value.TermsLookup, options);
				}
			}

			writer.WriteEndObject();
		}

		private void WriteTerms(Utf8JsonWriter writer, IEnumerable<object> terms, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (var term in terms)
				WriteTermValue(writer, term, options);
			writer.WriteEndArray();
		}

		private void WriteTermValue(Utf8JsonWriter writer, object term, JsonSerializerOptions options)
		{
			switch (term)
			{
				case null:
					writer.WriteNullValue();
					break;
				case string s:
					writer.WriteStringValue(s);
					break;
				case bool b:
					writer.WriteBooleanValue(b);
					break;
				case long l:
					writer.WriteNumberValue(l);
					break;
				case int i:
					writer.WriteNumberValue(i);
					break;
				case double d:
					writer.WriteNumberValue(d);
					break;
				case JsonElement e:
					e.WriteTo(writer);
					break;
				default:
					// A boxed value of some other runtime type. Mirror the legacy SourceWriteFormatter: an
					// OpenSearch.Client type is written through the request options (its registered converters apply),
					// while any other value (e.g. a user enum) goes through the configured SourceSerializer so a custom
					// source serializer governs it.
					if (term.GetType().IsOpenSearchClientType())
						JsonSerializer.Serialize(writer, term, term.GetType(), options);
					else
						ProxyRequestDocumentWriter.Write(writer, term, Settings, options);
					break;
			}
		}
	}
}
