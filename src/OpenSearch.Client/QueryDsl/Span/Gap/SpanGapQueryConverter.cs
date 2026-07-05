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
	/// A <see cref="System.Text.Json"/> converter for <see cref="ISpanGapQuery"/>, replacing the vendored
	/// Utf8Json <c>SpanGapQueryFormatter</c> as part of #388. A span gap is written as
	/// <c>{ "&lt;field&gt;": &lt;width&gt; }</c> (the inferred field name mapped to the integer width).
	/// <see cref="CanConvert"/> matches the concrete implementations so it applies when a span_near clause
	/// is serialized by its runtime type. Constructed with the connection settings for field-name inference.
	/// </summary>
	internal sealed class SpanGapQueryConverter : JsonConverter<ISpanGapQuery>
	{
		private readonly IConnectionSettingsValues _settings;

		public SpanGapQueryConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override bool CanConvert(Type typeToConvert) => typeof(ISpanGapQuery).IsAssignableFrom(typeToConvert);

		public override void Write(Utf8JsonWriter writer, ISpanGapQuery value, JsonSerializerOptions options)
		{
			if (value == null || SpanGapQuery.IsConditionless(value))
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(_settings.Inferrer.Field(value.Field));
			writer.WriteNumberValue(value.Width.Value);
			writer.WriteEndObject();
		}

		public override ISpanGapQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var query = new SpanGapQuery();
			foreach (var member in root.EnumerateObject())
			{
				query.Field = member.Name;
				if (member.Value.ValueKind == JsonValueKind.Number)
					query.Width = member.Value.GetInt32();
				break;
			}

			return query;
		}
	}
}
