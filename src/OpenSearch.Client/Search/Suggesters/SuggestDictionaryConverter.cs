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
	/// A <see cref="System.Text.Json"/> converter for <see cref="ISuggestDictionary{T}"/>, replacing the
	/// vendored Utf8Json <c>SuggestDictionaryFormatter&lt;T&gt;</c> as part of #388. Reads the
	/// <c>suggest</c> map (<c>string</c> → array of <see cref="ISuggest{T}"/>) into a
	/// <see cref="SuggestDictionary{T}"/>; on write the keys are emitted verbatim.
	/// </summary>
	internal sealed class SuggestDictionaryConverter<T> : JsonConverter<ISuggestDictionary<T>>
		where T : class
	{
		public override ISuggestDictionary<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			var dictionary = JsonSerializer.Deserialize<Dictionary<string, ISuggest<T>[]>>(ref reader, options);
			return dictionary == null ? null : new SuggestDictionary<T>(dictionary);
		}

		public override void Write(Utf8JsonWriter writer, ISuggestDictionary<T> value, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.Write(writer, (SuggestDictionary<T>)value, options, skipNull: true);
	}
}
