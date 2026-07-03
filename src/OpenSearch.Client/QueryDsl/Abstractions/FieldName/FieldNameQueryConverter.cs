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
	/// A <see cref="System.Text.Json"/> converter for the field-name-keyed query family (term,
	/// prefix, wildcard, regexp, fuzzy, match, …), replacing the vendored Utf8Json
	/// <c>FieldNameQueryFormatter&lt;T, TInterface&gt;</c> as part of #388.
	/// <para>
	/// These queries serialize as <c>{ "&lt;field&gt;": { …body… } }</c>: the inferred field name is
	/// the object key (the <see cref="IFieldNameQuery.Field"/> property is <c>[IgnoreDataMember]</c>,
	/// so it is excluded from the body). Constructed with the connection settings (decision D1) for
	/// field-name inference. The body is (de)serialized as the concrete
	/// <typeparamref name="TConcrete"/> — which is not <typeparamref name="TInterface"/>, so it does
	/// not recurse back into this converter.
	/// </para>
	/// </summary>
	/// <typeparam name="TConcrete">The concrete query type, e.g. <c>TermQuery</c>.</typeparam>
	/// <typeparam name="TInterface">The query interface, e.g. <c>ITermQuery</c>.</typeparam>
	internal sealed class FieldNameQueryConverter<TConcrete, TInterface> : JsonConverter<TInterface>
		where TConcrete : class, TInterface, IFieldNameQuery, new()
		where TInterface : class, IFieldNameQuery
	{
		private readonly IConnectionSettingsValues _settings;

		public FieldNameQueryConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();

			var field = value?.Field == null ? null : _settings.Inferrer.Field(value.Field);
			if (!string.IsNullOrEmpty(field))
			{
				writer.WritePropertyName(field);
				// Serialize the body as the concrete type; Field is [IgnoreDataMember] so it is excluded.
				JsonSerializer.Serialize(writer, value, typeof(TConcrete), options);
			}

			writer.WriteEndObject();
		}

		public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			foreach (var member in root.EnumerateObject())
			{
				// The oracle always writes the object body form; the scalar short form is read-only
				// input we do not need for round-tripping our own output.
				if (member.Value.ValueKind != JsonValueKind.Object) continue;

				var query = member.Value.Deserialize<TConcrete>(options);
				if (query != null)
					query.Field = member.Name;
				return query;
			}

			return null;
		}
	}
}
