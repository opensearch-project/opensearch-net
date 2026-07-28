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
	/// System.Text.Json replacement for the legacy Utf8Json <c>UpdateIndexSettingsRequestFormatter</c>. An
	/// <see cref="IUpdateIndexSettingsRequest"/> is serialized on the wire as just its
	/// <see cref="IUpdateIndexSettingsRequest.IndexSettings"/> body (no wrapping object), so the converter delegates
	/// both directions to the <see cref="DynamicIndexSettingsConverter"/> read/write helpers — mirroring the legacy
	/// formatter which forwarded to a shared <c>DynamicIndexSettingsFormatter</c> instance.
	///
	/// A <c>null</c> request is written as a JSON null (matching the legacy <c>writer.WriteNull()</c>), so
	/// <see cref="HandleNull"/> is opted in on write. On read the whole body materializes into a fresh
	/// <see cref="UpdateIndexSettingsRequest"/> whose <see cref="IUpdateIndexSettingsRequest.IndexSettings"/> is the
	/// dynamic settings parsed from the body.
	/// </summary>
	internal class UpdateIndexSettingsConverter : JsonConverter<IUpdateIndexSettingsRequest>
	{
		// The legacy formatter always produced a request (even for a JSON null the DynamicIndexSettingsFormatter
		// returned an empty settings instance); opt into null handling so STJ routes null through Read/Write instead
		// of short-circuiting.
		public override bool HandleNull => true;

		public override IUpdateIndexSettingsRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var dynamicSettings = DynamicIndexSettingsConverter.ReadIndexSettings(ref reader, options);
			return new UpdateIndexSettingsRequest { IndexSettings = dynamicSettings };
		}

		public override void Write(Utf8JsonWriter writer, IUpdateIndexSettingsRequest value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			DynamicIndexSettingsConverter.WriteIndexSettings(writer, value.IndexSettings, options);
		}
	}
}
