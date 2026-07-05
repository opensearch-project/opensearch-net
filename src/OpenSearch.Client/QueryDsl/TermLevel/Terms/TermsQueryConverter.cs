/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="ITermsQuery"/>, replacing the
	/// vendored Utf8Json <c>TermsQueryFormatter</c> as part of #388. Unlike the standard field-name
	/// query, <c>_name</c>/<c>boost</c> sit at the same level as the field key (not inside its body):
	/// <c>{ "_name": …, "boost": …, "&lt;field&gt;": [ …terms… ] }</c> — or a terms-lookup object in
	/// place of the array. Constructed with the connection settings for field-name inference.
	/// </summary>
	internal sealed class TermsQueryConverter : JsonConverter<ITermsQuery>
	{
		private static readonly Type LookupConcreteType =
			typeof(IFieldLookup).GetCustomAttribute<ReadAsAttribute>()?.Type ?? typeof(IFieldLookup);

		private readonly IConnectionSettingsValues _settings;

		public TermsQueryConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, ITermsQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (!string.IsNullOrEmpty(value.Name))
				writer.WriteString("_name", value.Name);
			if (value.Boost.HasValue)
			{
				writer.WritePropertyName("boost");
				JsonSerializer.Serialize(writer, value.Boost.Value, options);
			}

			var field = value.Field == null ? null : _settings.Inferrer.Field(value.Field);
			if (!string.IsNullOrEmpty(field))
			{
				// Mirror the vendored TermsQueryFormatter: when verbatim, a non-null Terms array is written
				// even when empty (the caller means "match nothing"); otherwise an empty array is omitted and
				// a terms-lookup object may take its place.
				if (value.IsVerbatim)
				{
					if (value.TermsLookup != null)
						WriteLookup(writer, field, value.TermsLookup, options);
					else if (value.Terms != null)
						WriteTermsArray(writer, field, value.Terms, options);
				}
				else if (value.Terms?.Any() == true)
					WriteTermsArray(writer, field, value.Terms, options);
				else if (value.TermsLookup != null)
					WriteLookup(writer, field, value.TermsLookup, options);
			}

			writer.WriteEndObject();
		}

		private void WriteTermsArray(Utf8JsonWriter writer, string field, IEnumerable<object> terms, JsonSerializerOptions options)
		{
			writer.WritePropertyName(field);
			writer.WriteStartArray();
			// Term values are user document values, so they are written through the source serializer
			// (mirroring the vendored SourceWriteFormatter) to apply the client's field/value inference.
			var sourceSerializer = SourceSerializerProviderConverter.Resolve(options);
			foreach (var term in terms)
				WriteTerm(writer, term, sourceSerializer, options);
			writer.WriteEndArray();
		}

		private static void WriteLookup(Utf8JsonWriter writer, string field, IFieldLookup lookup, JsonSerializerOptions options)
		{
			writer.WritePropertyName(field);
			JsonSerializer.Serialize(writer, lookup, typeof(IFieldLookup), options);
		}

		private static void WriteTerm(Utf8JsonWriter writer, object term, IOpenSearchSerializer sourceSerializer, JsonSerializerOptions options)
		{
			if (term == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (sourceSerializer == null)
			{
				JsonSerializer.Serialize(writer, term, term.GetType(), options);
				return;
			}

			using var stream = new System.IO.MemoryStream();
			sourceSerializer.Serialize(term, stream);
			stream.Position = 0;
			using var document = JsonDocument.Parse(stream);
			document.RootElement.WriteTo(writer);
		}

		public override ITermsQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var query = new TermsQuery();
			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "_name":
						query.Name = member.Value.GetString();
						break;
					case "boost":
						query.Boost = member.Value.GetDouble();
						break;
					default:
						query.Field = member.Name;
						if (member.Value.ValueKind == JsonValueKind.Array)
							query.Terms = member.Value.Deserialize<List<object>>(options);
						else if (member.Value.ValueKind == JsonValueKind.Object)
							query.TermsLookup = (IFieldLookup)member.Value.Deserialize(LookupConcreteType, options);
						break;
				}
			}

			return query;
		}
	}
}
