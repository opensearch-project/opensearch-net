/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="JoinField"/>, replacing the vendored
	/// Utf8Json <c>JoinFieldFormatter</c> as part of #388. A parent is written as a bare relation name
	/// (string), a child is written as an object with <c>name</c> and <c>parent</c> keys. On read, a
	/// string is treated as a parent; an object with a <c>parent</c> value becomes a child. Constructed
	/// with the connection settings for id inference on the parent id.
	/// </summary>
	internal sealed class JoinFieldConverter : JsonConverter<JoinField>
	{
		private readonly IConnectionSettingsValues _settings;

		public JoinFieldConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

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
					JsonSerializer.Serialize(writer, value.ParentOption.Name, options);
					break;
				case 1:
				{
					var child = value.ChildOption;
					writer.WriteStartObject();
					writer.WritePropertyName("name");
					JsonSerializer.Serialize(writer, child.Name, options);
					var id = (child.ParentId as IUrlParameter)?.GetString(_settings);
					writer.WriteString("parent", id);
					writer.WriteEndObject();
					break;
				}
			}
		}

		public override JoinField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			if (reader.TokenType == JsonTokenType.String)
			{
				var parent = reader.GetString();
				return new JoinField(new JoinField.Parent(parent));
			}

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			Id parentId = null;
			string name = null;
			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "parent":
						parentId = member.Value.Deserialize<Id>(options);
						break;
					case "name":
						name = member.Value.GetString();
						break;
				}
			}

			return parentId != null
				? new JoinField(new JoinField.Child(name, parentId))
				: new JoinField(new JoinField.Parent(name));
		}
	}
}
