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
using System.Text.Json;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>JoinFieldFormatter</c>. Serializes a
	/// <see cref="JoinField"/> as either a bare JSON string (a parent relation) or an object with
	/// <c>name</c>/<c>parent</c> (a child relation), resolving the <see cref="RelationName"/> and parent
	/// <see cref="Id"/> through the runtime <c>Inferrer</c> — hence a <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class JoinFieldConverter : SettingsAwareConverter<JoinField>
	{
		public JoinFieldConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override JoinField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				var parent = reader.GetString();
				return new JoinField(new JoinField.Parent(parent));
			}

			Id parentId = null;
			string name = null;
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var propertyName = reader.GetString();
				reader.Read();
				switch (propertyName)
				{
					case "parent":
						parentId = JsonSerializer.Deserialize<Id>(ref reader, options);
						break;
					case "name":
						name = reader.GetString();
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return parentId != null
				? new JoinField(new JoinField.Child(name, parentId))
				: new JoinField(new JoinField.Parent(name));
		}

		public override void Write(Utf8JsonWriter writer, JoinField value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
				{
					WriteRelationName(writer, value.ParentOption.Name);
					break;
				}
				case 1:
				{
					var child = value.ChildOption;
					writer.WriteStartObject();
					writer.WritePropertyName("name");
					WriteRelationName(writer, child.Name);
					writer.WritePropertyName("parent");
					var id = (child.ParentId as IUrlParameter)?.GetString(Settings);
					if (id == null)
						writer.WriteNullValue();
					else
						writer.WriteStringValue(id);
					writer.WriteEndObject();
					break;
				}
			}
		}

		private void WriteRelationName(Utf8JsonWriter writer, RelationName value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(Settings.Inferrer.RelationName(value));
		}
	}
}
