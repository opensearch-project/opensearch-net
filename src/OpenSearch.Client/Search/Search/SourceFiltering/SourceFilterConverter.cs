/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="ISourceFilter"/>, replacing the
	/// vendored Utf8Json <c>SourceFilterFormatter</c> as part of #388. On read a bare string or array
	/// is treated as <c>includes</c>, and an object supplies <c>includes</c>/<c>excludes</c>. On write
	/// the (non-null) <c>includes</c>/<c>excludes</c> field lists are emitted as an object.
	/// </summary>
	internal sealed class SourceFilterConverter : JsonConverter<ISourceFilter>
	{
		public override void Write(Utf8JsonWriter writer, ISourceFilter value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			// Member order mirrors the ISourceFilter declaration (excludes before includes), which the
			// original DynamicObjectResolver-based formatter emitted.
			if (value.Excludes != null)
			{
				writer.WritePropertyName("excludes");
				JsonSerializer.Serialize(writer, value.Excludes, options);
			}
			if (value.Includes != null)
			{
				writer.WritePropertyName("includes");
				JsonSerializer.Serialize(writer, value.Includes, options);
			}
			writer.WriteEndObject();
		}

		public override ISourceFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			var filter = new SourceFilter();
			switch (root.ValueKind)
			{
				case JsonValueKind.String:
					filter.Includes = new[] { root.GetString() };
					break;
				case JsonValueKind.Array:
					filter.Includes = root.Deserialize<string[]>(options);
					break;
				case JsonValueKind.Object:
					foreach (var member in root.EnumerateObject())
					{
						switch (member.Name)
						{
							case "includes":
								filter.Includes = member.Value.Deserialize<Fields>(options);
								break;
							case "excludes":
								filter.Excludes = member.Value.Deserialize<Fields>(options);
								break;
						}
					}
					break;
			}

			return filter;
		}
	}
}
