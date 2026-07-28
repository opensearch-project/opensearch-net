/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using OpenSearch.Net.Extensions;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>FieldNameQueryFormatter&lt;T, TInterface&gt;</c>.
	///
	/// Field-name queries serialize as a single-key object <c>{ "&lt;field&gt;": &lt;query body&gt; }</c> where the key is the
	/// query's <see cref="IFieldNameQuery.Field"/> resolved through the runtime <c>Inferrer</c> — hence a
	/// <see cref="SettingsAwareConverter{T}"/>. The query body itself is serialized/deserialized as the concrete
	/// type <typeparamref name="T"/>; because this converter is registered against <typeparamref name="TInterface"/>
	/// (not <typeparamref name="T"/>), delegating to <see cref="JsonSerializer"/> for <typeparamref name="T"/> writes
	/// the body's members without recursing back into this converter.
	///
	/// On read, some query bodies may appear as a bare scalar (e.g. a term written as <c>{ "field": "value" }</c>
	/// or a match written as <c>{ "field": "text" }</c>) rather than a nested object; those short-forms are handled
	/// explicitly, mirroring the legacy formatter.
	/// </summary>
	internal class FieldNameQueryConverter<T, TInterface> : SettingsAwareConverter<TInterface>
		where T : class, TInterface, IFieldNameQuery, new()
		where TInterface : class, IFieldNameQuery
	{
		public FieldNameQueryConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var fieldName = value.Field;
			if (fieldName == null)
			{
				// Mirror the legacy formatter: an unset field writes nothing. STJ requires a value token, so emit null.
				writer.WriteNullValue();
				return;
			}

			var field = Settings.Inferrer.Field(fieldName);
			if (field.IsNullOrEmpty())
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(field);
			// Serialize the body by its RUNTIME type, mirroring the legacy formatter (which serialized through the
			// interface's data contract, never casting to the concrete T). The value may be either the concrete
			// query (e.g. TermQuery) or a Fluent descriptor (e.g. TermQueryDescriptor<T>) that implements TInterface
			// via explicit-interface members but is NOT a T — casting to (T) throws InvalidCastException on the
			// descriptor path. Neither the concrete nor the descriptor type is bound to this interface-keyed
			// converter, so serializing by runtime type does not recurse. Field is [IgnoreDataMember] so it is not
			// written here.
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
			writer.WriteEndObject();
		}

		public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected start of object when reading {typeof(T).Name}.");

			reader.Read(); // property name or end object

			if (reader.TokenType == JsonTokenType.EndObject)
				return null;

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException($"Expected field name when reading {typeof(T).Name}.");

			var fieldName = reader.GetString();
			reader.Read(); // advance to the value

			TInterface query = null;

			switch (reader.TokenType)
			{
				case JsonTokenType.StartObject:
					query = JsonSerializer.Deserialize<T>(ref reader, options);
					reader.Read(); // consume end object of the outer wrapper
					break;
				case JsonTokenType.Null:
					reader.Read(); // consume end object of the outer wrapper
					break;
				default:
					var concrete = new T();
					ReadShortForm(ref reader, concrete);
					query = concrete;
					reader.Read(); // consume end object of the outer wrapper
					break;
			}

			if (query == null)
				return null;

			query.Field = fieldName;
			return query;
		}

		// Handles the scalar short-forms the legacy formatter supported for a subset of query types.
		private static void ReadShortForm(ref Utf8JsonReader reader, TInterface query)
		{
			switch (query)
			{
				case ITermQuery termQuery:
					switch (reader.TokenType)
					{
						case JsonTokenType.String:
							termQuery.Value = reader.GetString();
							break;
						case JsonTokenType.Number:
							termQuery.Value = reader.TryGetInt64(out var l) ? l : (object)reader.GetDouble();
							break;
						case JsonTokenType.True:
						case JsonTokenType.False:
							termQuery.Value = reader.GetBoolean();
							break;
					}
					break;
				case IMatchQuery matchQuery:
					matchQuery.Query = reader.GetString();
					break;
				case IMatchPhraseQuery matchPhraseQuery:
					matchPhraseQuery.Query = reader.GetString();
					break;
				case IMatchPhrasePrefixQuery matchPhrasePrefixQuery:
					matchPhrasePrefixQuery.Query = reader.GetString();
					break;
				case IMatchBoolPrefixQuery matchBoolPrefixQuery:
					matchBoolPrefixQuery.Query = reader.GetString();
					break;
			}
		}
	}
}
