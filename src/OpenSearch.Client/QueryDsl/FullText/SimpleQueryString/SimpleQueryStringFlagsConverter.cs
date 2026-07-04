/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// STJ converter for the <c>[Flags]</c> <see cref="SimpleQueryStringFlags"/> (#388), replacing the
	/// vendored <c>SimpleQueryStringFlagsFormatter</c>. Emits the set members joined by <c>|</c> using
	/// their uppercase <c>[EnumMember]</c> names, in the fixed order the formatter used.
	/// </summary>
	internal sealed class SimpleQueryStringFlagsConverter : JsonConverter<SimpleQueryStringFlags?>
	{
		private static readonly (SimpleQueryStringFlags Flag, string Name)[] Order =
		{
			(SimpleQueryStringFlags.All, "ALL"),
			(SimpleQueryStringFlags.None, "NONE"),
			(SimpleQueryStringFlags.And, "AND"),
			(SimpleQueryStringFlags.Or, "OR"),
			(SimpleQueryStringFlags.Not, "NOT"),
			(SimpleQueryStringFlags.Prefix, "PREFIX"),
			(SimpleQueryStringFlags.Phrase, "PHRASE"),
			(SimpleQueryStringFlags.Precedence, "PRECEDENCE"),
			(SimpleQueryStringFlags.Escape, "ESCAPE"),
			(SimpleQueryStringFlags.Whitespace, "WHITESPACE"),
			(SimpleQueryStringFlags.Fuzzy, "FUZZY"),
			(SimpleQueryStringFlags.Near, "NEAR"),
			(SimpleQueryStringFlags.Slop, "SLOP"),
		};

		public override void Write(Utf8JsonWriter writer, SimpleQueryStringFlags? value, JsonSerializerOptions options)
		{
			if (value == null) { writer.WriteNullValue(); return; }

			var parts = new List<string>(Order.Length);
			foreach (var (flag, name) in Order)
				if (value.Value.HasFlag(flag)) parts.Add(name);
			writer.WriteStringValue(string.Join("|", parts));
		}

		public override SimpleQueryStringFlags? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			var raw = reader.GetString();
			if (string.IsNullOrEmpty(raw)) return default(SimpleQueryStringFlags);

			var result = default(SimpleQueryStringFlags);
			foreach (var token in raw.Split('|'))
				foreach (var (flag, name) in Order)
					if (string.Equals(token, name, StringComparison.OrdinalIgnoreCase))
						result |= flag;
			return result;
		}
	}
}
