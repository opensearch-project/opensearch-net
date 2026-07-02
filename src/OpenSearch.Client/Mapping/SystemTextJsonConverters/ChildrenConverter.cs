/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Linq;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Net;

namespace OpenSearch.Client.Mapping.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="Children"/>.
	/// Children is a list of child relation names for join field mappings.
	/// Can be serialized as a single string or array of strings.
	/// </summary>
	internal sealed class ChildrenConverter : JsonConverter<Children>
	{
		private readonly IConnectionSettingsValues _settings;

		public ChildrenConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override Children Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			var children = new Children();

			if (reader.TokenType == JsonTokenType.String)
			{
				var value = reader.GetString();
				if (!string.IsNullOrEmpty(value))
					children.Add(value);
				return children;
			}

			if (reader.TokenType == JsonTokenType.StartArray)
			{
				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndArray)
						break;

					if (reader.TokenType == JsonTokenType.String)
					{
						var value = reader.GetString();
						if (!string.IsNullOrEmpty(value))
							children.Add(value);
					}
					else
					{
						reader.Skip();
					}
				}
				return children;
			}

			reader.Skip();
			return children;
		}

		public override void Write(Utf8JsonWriter writer, Children value, JsonSerializerOptions options)
		{
			if (value == null || value.Count == 0)
			{
				writer.WriteNullValue();
				return;
			}

			var resolved = value.Cast<IUrlParameter>().ToList();

			if (resolved.Count == 1)
			{
				var name = _settings != null
					? resolved[0].GetString(_settings)
					: value[0]?.Name;
				writer.WriteStringValue(name);
				return;
			}

			writer.WriteStartArray();
			foreach (var item in resolved)
			{
				var name = _settings != null
					? item.GetString(_settings)
					: item.ToString();
				writer.WriteStringValue(name);
			}
			writer.WriteEndArray();
		}
	}
}
