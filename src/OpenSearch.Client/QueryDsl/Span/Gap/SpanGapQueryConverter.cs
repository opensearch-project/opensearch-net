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
	/// System.Text.Json replacement for the legacy Utf8Json <c>SpanGapQueryFormatter</c>.
	///
	/// A span-gap query is a single-field object <c>{ "&lt;field&gt;": &lt;width&gt; }</c> where the key is the query's
	/// <see cref="ISpanGapQuery.Field"/> resolved through the runtime <c>Inferrer</c> (hence a
	/// <see cref="SettingsAwareConverter{T}"/>) and the value is the integer <see cref="ISpanGapQuery.Width"/>.
	/// A null value — or a conditionless query (no width / no field) — writes JSON <c>null</c>, matching the legacy
	/// formatter. On read only the first property is significant (the legacy formatter ignored any beyond the first);
	/// this converter emits the field wrapper on write and consumes it symmetrically on read.
	/// </summary>
	internal class SpanGapQueryConverter : SettingsAwareConverter<ISpanGapQuery>
	{
		public SpanGapQueryConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override ISpanGapQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

			var query = new SpanGapQuery();
			var read = false;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var field = reader.GetString();
				reader.Read(); // advance to value

				// Mirror the legacy formatter: only the first field/width pair is significant.
				if (read)
				{
					reader.Skip();
					continue;
				}

				query.Field = field;
				query.Width = reader.GetInt32();
				read = true;
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, ISpanGapQuery value, JsonSerializerOptions options)
		{
			if (value == null || SpanGapQuery.IsConditionless(value))
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WriteNumber(Settings.Inferrer.Field(value.Field), value.Width.Value);
			writer.WriteEndObject();
		}
	}
}
