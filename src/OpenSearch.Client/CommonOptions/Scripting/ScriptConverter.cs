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

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ScriptFormatter</c>.
	///
	/// A script is deserialized into a concrete <see cref="IScript"/> shape based on which field is present:
	/// <c>inline</c>/<c>source</c> produce an <see cref="InlineScript"/>, <c>id</c> produces an
	/// <see cref="IndexedScript"/>. The shared <c>lang</c> and <c>params</c> fields are applied afterwards. On write
	/// the concrete type is dispatched to the matching contract (<see cref="IInlineScript"/> /
	/// <see cref="IIndexedScript"/>), falling back to the base <see cref="IScript"/> contract.
	/// </summary>
	internal class ScriptConverter : JsonConverter<IScript>
	{
		public override IScript Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			IScript script = null;
			string language = null;
			Dictionary<string, object> parameters = null;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name while reading IScript.");

				var field = reader.GetString();
				reader.Read(); // advance to value

				switch (field)
				{
					case "inline":
					case "source":
						script = new InlineScript(reader.GetString());
						break;
					case "id":
						script = new IndexedScript(reader.GetString());
						break;
					case "lang":
						language = reader.GetString();
						break;
					case "params":
						parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
						break;
					default:
						reader.Skip();
						break;
				}
			}

			if (script == null)
				return null;

			script.Lang = language;
			script.Params = parameters;
			return script;
		}

		public override void Write(Utf8JsonWriter writer, IScript value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value)
			{
				case IInlineScript inlineScript:
					JsonSerializer.Serialize(writer, inlineScript, options);
					break;
				case IIndexedScript indexedScript:
					JsonSerializer.Serialize(writer, indexedScript, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, options);
					break;
			}
		}
	}
}
