/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ChildrenFormatter</c>. Serializes a
	/// <see cref="Children"/> collection either as a single JSON string (when there is one child) or as a JSON
	/// array of strings, resolving each <see cref="RelationName"/> through the runtime <c>Inferrer</c> — hence a
	/// <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class ChildrenConverter : SettingsAwareConverter<Children>
	{
		public ChildrenConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override Children Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var children = new Children();
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
				{
					RelationName type = reader.GetString();
					children.Add(type);
					return children;
				}
				case JsonTokenType.StartArray:
				{
					var types = new List<RelationName>();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndArray)
							break;

						RelationName type = reader.GetString();
						types.Add(type);
					}

					children.AddRange(types);
					return children;
				}
				default:
					reader.Skip();
					return children;
			}
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
				writer.WriteStringValue(resolved[0].GetString(Settings));
				return;
			}

			writer.WriteStartArray();
			foreach (var r in resolved)
				writer.WriteStringValue(r.GetString(Settings));
			writer.WriteEndArray();
		}
	}
}
