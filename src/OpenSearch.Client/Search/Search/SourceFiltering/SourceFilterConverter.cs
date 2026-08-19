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
	/// System.Text.Json replacement for the legacy Utf8Json <c>SourceFilterFormatter</c>.
	///
	/// <see cref="ISourceFilter"/> is a union of several JSON shapes:
	/// <list type="bullet">
	/// <item><description>a single field <c>string</c> becomes the sole <see cref="ISourceFilter.Includes"/>;</description></item>
	/// <item><description>an array of field strings becomes <see cref="ISourceFilter.Includes"/>;</description></item>
	/// <item><description>an object with <c>includes</c>/<c>excludes</c> members maps to the respective
	/// <see cref="Fields"/> properties (any other member is skipped);</description></item>
	/// <item><description><c>null</c> yields <c>null</c>.</description></item>
	/// </list>
	/// The boolean <c>_source</c> shape (<c>true</c>/<c>false</c>) is not handled here — as in the legacy formatter it
	/// is dispatched at the enclosing <c>Union&lt;bool, ISourceFilter&gt;</c> level.
	///
	/// Serialization mirrors the legacy <c>DynamicObjectResolver.ExcludeNullCamelCase</c> object output: an object
	/// with the non-null <c>includes</c>/<c>excludes</c> members. The <see cref="Fields"/> members are delegated to
	/// the (settings-aware) <see cref="FieldsConverter"/> registered on the options so field-name inference is
	/// preserved.
	/// </summary>
	internal class SourceFilterConverter : JsonConverter<ISourceFilter>
	{
		public override ISourceFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new SourceFilter { Includes = new[] { reader.GetString() } };
				case JsonTokenType.StartArray:
					var includes = JsonSerializer.Deserialize<string[]>(ref reader, options);
					return new SourceFilter { Includes = includes };
				default:
					var filter = new SourceFilter();
					if (reader.TokenType != JsonTokenType.StartObject)
					{
						// A scalar (number/bool) never carries includes/excludes; the legacy formatter still
						// returned a (non-null) empty SourceFilter for these, so consume and do the same.
						reader.Skip();
						return filter;
					}

					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject)
							break;

						if (reader.TokenType != JsonTokenType.PropertyName)
							continue;

						var property = reader.GetString();
						reader.Read();
						switch (property)
						{
							case "includes":
								filter.Includes = JsonSerializer.Deserialize<Fields>(ref reader, options);
								break;
							case "excludes":
								filter.Excludes = JsonSerializer.Deserialize<Fields>(ref reader, options);
								break;
							default:
								reader.Skip();
								break;
						}
					}

					return filter;
			}
		}

		public override void Write(Utf8JsonWriter writer, ISourceFilter value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			if (value.Includes != null)
			{
				writer.WritePropertyName("includes");
				JsonSerializer.Serialize(writer, value.Includes, options);
			}

			if (value.Excludes != null)
			{
				writer.WritePropertyName("excludes");
				JsonSerializer.Serialize(writer, value.Excludes, options);
			}

			writer.WriteEndObject();
		}
	}
}
