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

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>FieldsFormatter</c>. A <see cref="Fields"/> is a
	/// collection of <see cref="Field"/> serialized as a JSON array; each element is delegated to
	/// <see cref="FieldConverter"/> so individual fields keep their string/object serialization semantics and are
	/// resolved through the runtime <c>Inferrer</c> (hence a <see cref="SettingsAwareConverter{T}"/>). On read a
	/// non-array token yields <c>null</c> and any <c>null</c> elements are dropped — matching the legacy formatter.
	/// </summary>
	internal class FieldsConverter : SettingsAwareConverter<Fields>
	{
		private readonly FieldConverter _fieldConverter;

		public FieldsConverter(IConnectionSettingsValues settings) : base(settings) =>
			_fieldConverter = new FieldConverter(settings);

		public override Fields Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartArray)
			{
				reader.Skip();
				return null;
			}

			var fields = new List<Field>();
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray)
					break;

				var field = _fieldConverter.Read(ref reader, typeof(Field), options);
				if (field != null)
					fields.Add(field);
			}

			return new Fields(fields);
		}

		public override void Write(Utf8JsonWriter writer, Fields value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var fields = value.ListOfFields;
			writer.WriteStartArray();
			for (var i = 0; i < fields.Count; i++)
				_fieldConverter.Write(writer, fields[i], options);
			writer.WriteEndArray();
		}
	}
}
