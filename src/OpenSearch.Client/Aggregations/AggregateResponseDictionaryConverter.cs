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
	/// A read-only <see cref="System.Text.Json"/> converter for the response-side
	/// <see cref="AggregateDictionary"/> (the <c>aggregations</c> map), replacing the vendored Utf8Json
	/// formatter as part of #388. Reads <c>{ "&lt;name&gt;": &lt;aggregate&gt; }</c>; typed-key
	/// (<c>&lt;type&gt;#&lt;name&gt;</c>) resolution is handled by <see cref="AggregateDictionary"/>.
	/// </summary>
	internal sealed class AggregateResponseDictionaryConverter : JsonConverter<AggregateDictionary>
	{
		public override AggregateDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return new AggregateDictionary(new Dictionary<string, IAggregate>());

			var dictionary = new Dictionary<string, IAggregate>();
			foreach (var member in root.EnumerateObject())
			{
				if (member.Value.ValueKind != JsonValueKind.Object) continue;
				var aggregate = member.Value.Deserialize<IAggregate>(options);
				if (aggregate != null)
					dictionary[member.Name] = aggregate;
			}

			return new AggregateDictionary(dictionary);
		}

		public override void Write(Utf8JsonWriter writer, AggregateDictionary value, JsonSerializerOptions options) =>
			throw new NotSupportedException("AggregateDictionary is a response type and is not serialized.");
	}
}
