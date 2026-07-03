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
				var terms = value.Terms?.Where(t => t != null).ToList();
				if (terms is { Count: > 0 })
				{
					writer.WritePropertyName(field);
					writer.WriteStartArray();
					foreach (var term in terms)
						JsonSerializer.Serialize(writer, term, typeof(object), options);
					writer.WriteEndArray();
				}
				else if (value.TermsLookup != null)
				{
					writer.WritePropertyName(field);
					JsonSerializer.Serialize(writer, value.TermsLookup, typeof(IFieldLookup), options);
				}
			}

			writer.WriteEndObject();
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
