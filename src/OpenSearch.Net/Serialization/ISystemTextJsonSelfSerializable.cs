/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;

namespace OpenSearch.Net
{
	/// <summary>
	/// Implemented by request types whose body is not a single JSON document — for example the
	/// newline-delimited (NDJSON) bodies of <c>_bulk</c> and <c>_msearch</c> — and therefore cannot be
	/// written through <c>System.Text.Json</c>'s single-root <c>Utf8JsonWriter</c> (see GitHub issue #388).
	/// <para>
	/// When <see cref="SystemTextJsonSerializer"/> is asked to serialize such a value it delegates to
	/// <see cref="Write"/>, passing itself so the implementation can reach the connection settings (via the
	/// options it carries) and reuse the request/response and source serializers to emit each line.
	/// </para>
	/// </summary>
	internal interface ISystemTextJsonSelfSerializable
	{
		void Write(Stream stream, IOpenSearchSerializer builtInSerializer, SerializationFormatting formatting);
	}
}
