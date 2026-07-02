/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Net;

namespace OpenSearch.Client.Mapping.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="JoinField"/>.
	/// JoinField can be:
	/// - A simple string representing a parent relation name
	/// - An object {"name": "child_name", "parent": "parent_id"} representing a child
	/// </summary>
	internal sealed class JoinFieldConverter : JsonConverter<JoinField>
	{
		private readonly IConnectionSettingsValues _settings;

		public JoinFieldConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override JoinField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType == JsonTokenType.String)
			{
				var parentName = reader.GetString();
				return new JoinField(new JoinField.Parent(parentName));
			}

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected String or StartObject for JoinField but got {reader.TokenType}");

			string name = null;
			string parentId = null;

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
					case "name":
						name = reader.GetString();
						break;
					case "parent":
						parentId = reader.TokenType == JsonTokenType.Number
							? reader.GetInt64().ToString()
							: reader.GetString();
						break;
					default:
						reader.Skip();
						break;
				}
			}

			if (parentId != null)
				return new JoinField(new JoinField.Child(name, new Id(parentId)));

			return new JoinField(new JoinField.Parent(name));
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
					var parentName = _settings != null
						? _settings.Inferrer.RelationName(value.ParentOption.Name)
						: value.ParentOption.Name?.Name;
					writer.WriteStringValue(parentName);
					break;
				}
				case 1:
				{
					var child = value.ChildOption;
					writer.WriteStartObject();

					writer.WritePropertyName("name");
					var childName = _settings != null
						? _settings.Inferrer.RelationName(child.Name)
						: child.Name?.Name;
					writer.WriteStringValue(childName);

					writer.WritePropertyName("parent");
					var id = child.ParentId is IUrlParameter urlParam && _settings != null
						? urlParam.GetString(_settings)
						: child.ParentId?.ToString();
					writer.WriteStringValue(id);

					writer.WriteEndObject();
					break;
				}
			}
		}
	}
}
