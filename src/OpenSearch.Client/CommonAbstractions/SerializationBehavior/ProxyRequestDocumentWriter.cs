/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Text.Json;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// Writes a proxy-request document body (index / create) through the connection's
	/// <see cref="IConnectionSettingsValues.SourceSerializer"/>, mirroring the legacy
	/// <c>IProxyRequest.WriteJson(sourceSerializer, …)</c>. This is what lets a user-supplied source serializer (e.g.
	/// the Newtonsoft-based JsonNetSerializer) govern the document shape. The serialized bytes are spliced into the
	/// output via <c>WriteRawValue</c>. In the default configuration the source serializer is the same System.Text.Json
	/// high-level serializer, so this is an equivalent (non-recursive) round through it.
	/// </summary>
	internal static class ProxyRequestDocumentWriter
	{
		public static void Write<TDocument>(Utf8JsonWriter writer, TDocument document, IConnectionSettingsValues settings,
			JsonSerializerOptions options)
		{
			if (document == null)
			{
				writer.WriteNullValue();
				return;
			}

			var sourceSerializer = settings?.SourceSerializer;
			if (sourceSerializer == null)
			{
				JsonSerializer.Serialize(writer, document, options);
				return;
			}

			using var ms = settings.MemoryStreamFactory.Create();
			sourceSerializer.Serialize(document, ms, SerializationFormatting.None);
			writer.WriteRawValue(ms.ToArray(), skipInputValidation: true);
		}
	}
}
