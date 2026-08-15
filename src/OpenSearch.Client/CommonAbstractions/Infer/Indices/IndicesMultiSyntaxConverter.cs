/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>IndicesMultiSyntaxFormatter</c>. Serializes an
	/// <see cref="Indices"/> as a single JSON string, resolving it through the runtime <c>Inferrer</c> — hence a
	/// <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class IndicesMultiSyntaxConverter : SettingsAwareConverter<Indices>
	{
		public IndicesMultiSyntaxConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override Indices Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				Indices indices = reader.GetString();
				return indices;
			}

			reader.Skip();
			return null;
		}

		public override void Write(Utf8JsonWriter writer, Indices value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					writer.WriteStringValue("_all");
					break;
				case 1:
					writer.WriteStringValue(((IUrlParameter)value).GetString(Settings));
					break;
			}
		}
	}
}
