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
	/// System.Text.Json replacement for the legacy Utf8Json <c>FieldCapabilitiesFields.Converter</c> nested formatter
	/// (a <c>ResolvableDictionaryFormatterBase&lt;FieldCapabilitiesFields, Field, FieldTypes&gt;</c>). The wire shape is
	/// a flat JSON object whose property names are field names and whose values are <see cref="FieldTypes"/> maps.
	///
	/// This is a <em>non-generic</em> formatter, so the open-generic <see cref="DictionaryResponseConverterFactory"/>
	/// does not construct it; a dedicated converter is needed instead. Keys resolve through the runtime
	/// <c>Inferrer</c> (<see cref="Field"/> is an <c>IUrlParameter</c>), so it derives from
	/// <see cref="SettingsAwareConverter{T}"/> — matching the legacy formatter which read settings from the resolver.
	///
	/// <see cref="Utf8JsonReader"/> is forward-only, so the body is buffered into a <see cref="JsonDocument"/> and read
	/// from the DOM; each key is materialized through the registered <see cref="Field"/> converter and each value is
	/// handed to <see cref="JsonSerializer"/> as <see cref="FieldTypes"/> so registered converters apply. Unlike the
	/// response-dictionary converters this legacy formatter did NOT strip the server-error envelope (that is only done
	/// by the response formatters), so every property is treated as a data entry.
	/// </summary>
	internal class FieldCapabilitiesFieldsConverter : SettingsAwareConverter<FieldCapabilitiesFields>
	{
		public FieldCapabilitiesFieldsConverter(IConnectionSettingsValues settings) : base(settings) { }

		// The legacy formatter wrapped the deserialized dictionary in a proxy even for a JSON null (the Utf8Json
		// dictionary formatter returned null, which the proxy treats as empty); opt into null handling to match.
		public override bool HandleNull => true;

		public override FieldCapabilitiesFields Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var dictionary = new Dictionary<Field, FieldTypes>();

			if (reader.TokenType != JsonTokenType.Null)
			{
				using var document = JsonDocument.ParseValue(ref reader);
				var root = document.RootElement;

				if (root.ValueKind == JsonValueKind.Object)
				{
					foreach (var property in root.EnumerateObject())
					{
						var key = DictionaryResponseConverterHelpers.DeserializeKey<Field>(property.Name, options);
						var value = property.Value.Deserialize<FieldTypes>(options);
						dictionary.Add(key, value);
					}
				}
			}

			return new FieldCapabilitiesFields(Settings, dictionary);
		}

		public override void Write(Utf8JsonWriter writer, FieldCapabilitiesFields value, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}
}
