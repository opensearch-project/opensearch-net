/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Collections.Generic;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="ITermsQuery"/>.
	/// JSON format:
	/// <code>
	/// {"field_name": ["val1", "val2"]}
	/// </code>
	/// or terms lookup:
	/// <code>
	/// {"field_name": {"index": "...", "id": "...", "path": "..."}}
	/// </code>
	/// </summary>
	internal sealed class TermsQueryConverter : JsonConverter<ITermsQuery>
	{
		public override ITermsQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for ITermsQuery but got {reader.TokenType}");

			var query = new TermsQuery();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName");

				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "_name":
						query.Name = reader.GetString();
						break;
					default:
						// This is the field name
						query.Field = new Field(prop);
						if (reader.TokenType == JsonTokenType.StartArray)
							query.Terms = ReadTermsArray(ref reader);
						else if (reader.TokenType == JsonTokenType.StartObject)
							query.TermsLookup = ReadTermsLookup(ref reader);
						else
							reader.Skip();
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, ITermsQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			var fieldName = value.Field?.ToString();
			if (!string.IsNullOrEmpty(fieldName))
			{
				writer.WritePropertyName(fieldName);

				if (value.TermsLookup != null)
					WriteTermsLookup(writer, value.TermsLookup);
				else
					WriteTermsArray(writer, value.Terms);
			}

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
		}

		private static List<object> ReadTermsArray(ref Utf8JsonReader reader)
		{
			var terms = new List<object>();
			while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.String:
						terms.Add(reader.GetString());
						break;
					case JsonTokenType.Number:
						if (reader.TryGetInt64(out var l))
							terms.Add(l);
						else
							terms.Add(reader.GetDouble());
						break;
					case JsonTokenType.True:
						terms.Add(true);
						break;
					case JsonTokenType.False:
						terms.Add(false);
						break;
					case JsonTokenType.Null:
						terms.Add(null);
						break;
					default:
						reader.Skip();
						break;
				}
			}
			return terms;
		}

		private static IFieldLookup ReadTermsLookup(ref Utf8JsonReader reader)
		{
			var lookup = new FieldLookup();
			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var p = reader.GetString();
				reader.Read();

				switch (p)
				{
					case "index":
						lookup.Index = reader.GetString();
						break;
					case "id":
						lookup.Id = new Id(reader.GetString());
						break;
					case "path":
						lookup.Path = new Field(reader.GetString());
						break;
					case "routing":
						lookup.Routing = reader.GetString();
						break;
					default:
						reader.Skip();
						break;
				}
			}
			return lookup;
		}

		private static void WriteTermsArray(Utf8JsonWriter writer, IEnumerable<object> terms)
		{
			writer.WriteStartArray();
			if (terms != null)
			{
				foreach (var term in terms)
				{
					switch (term)
					{
						case string s: writer.WriteStringValue(s); break;
						case int i: writer.WriteNumberValue(i); break;
						case long l: writer.WriteNumberValue(l); break;
						case double d: writer.WriteNumberValue(d); break;
						case float f: writer.WriteNumberValue(f); break;
						case decimal dec: writer.WriteNumberValue(dec); break;
						case bool b: writer.WriteBooleanValue(b); break;
						case null: writer.WriteNullValue(); break;
						default: writer.WriteStringValue(term.ToString()); break;
					}
				}
			}
			writer.WriteEndArray();
		}

		private static void WriteTermsLookup(Utf8JsonWriter writer, IFieldLookup lookup)
		{
			writer.WriteStartObject();
			if (lookup.Index != null)
			{
				writer.WritePropertyName("index");
				writer.WriteStringValue(lookup.Index.ToString());
			}
			if (lookup.Id != null)
			{
				writer.WritePropertyName("id");
				writer.WriteStringValue(lookup.Id.ToString());
			}
			if (lookup.Path != null)
			{
				writer.WritePropertyName("path");
				writer.WriteStringValue(lookup.Path.ToString());
			}
			if (lookup.Routing != null)
			{
				writer.WritePropertyName("routing");
				writer.WriteStringValue(lookup.Routing.ToString());
			}
			writer.WriteEndObject();
		}
	}
}
