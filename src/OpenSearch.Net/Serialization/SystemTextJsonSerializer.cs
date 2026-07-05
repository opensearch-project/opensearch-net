/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSearch.Net
{
	/// <summary>
	/// An <see cref="IOpenSearchSerializer"/> implementation backed by the
	/// Microsoft <c>System.Text.Json</c> library.
	/// <para>
	/// This is the foundation for migrating the client away from the vendored,
	/// unmaintained Utf8Json serializer (see GitHub issue #388). It is a standalone
	/// serializer that plugs into the existing <see cref="IOpenSearchSerializer"/>
	/// seam; it does not implement the internal Utf8Json formatter-resolver
	/// interface, so it can be used independently of that layer.
	/// </para>
	/// </summary>
	/// <remarks>
	/// WARNING: EXPERIMENTAL / NOT WIRE-COMPATIBLE WITH THE HIGH-LEVEL CLIENT YET.
	/// <para>
	/// With default options this performs raw <c>System.Text.Json</c> serialization:
	/// it uses PascalCase property names and ignores the high-level client's
	/// <c>[DataMember]</c>, <c>[ReadAs]</c> and <c>[StringEnum]</c> attributes and
	/// field-name inference. Using it directly for <c>OpenSearch.Client</c> request
	/// or response types will therefore produce JSON that does NOT match the wire
	/// format the server expects. It is intended for low-level/custom payloads and
	/// as the base for the converter work tracked by #388. The API surface and
	/// default behavior are not yet stable.
	/// </para>
	/// </remarks>
	public class SystemTextJsonSerializer : IOpenSearchSerializer
	{
		private readonly JsonSerializerOptions _options;

		/// <summary>
		/// Creates a new <see cref="SystemTextJsonSerializer"/>.
		/// </summary>
		/// <param name="options">
		/// The <see cref="JsonSerializerOptions"/> to use. When <c>null</c>, a new
		/// default instance is used. The provided options instance carries any
		/// custom converters that thread client/connection state into serialization.
		/// </param>
		public SystemTextJsonSerializer(JsonSerializerOptions options = null)
		{
			// When no options are supplied, default to honoring the client's existing
			// System.Runtime.Serialization attributes (see DataContractResolver / #388) and to
			// Utf8Json's minimal escaping (it does not HTML-escape '+', '<', '&', etc.), which the
			// server-facing wire format expects.
			_options = options ?? new JsonSerializerOptions
			{
				TypeInfoResolver = DataContractResolver.Instance,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				// Deserialize dynamic `object` payloads (params, meta, _source, dictionary values) to
				// the Utf8Json CLR shapes (Dictionary/List/long/double/string/bool) rather than the
				// STJ default of JsonElement.
				Converters = { ObjectConverter.Instance },
			};
		}

		// The SerializationFormatting.Indented hint is intentionally ignored: the vendored
		// DefaultHighLevelSerializer also ignored it and always wrote compact JSON, so request/response
		// bodies stay compact even when PrettyJson/EnableDebugMode is set (the `pretty` query string still
		// asks the server to format its response). Preserving this keeps the wire format unchanged (#388).
		private JsonSerializerOptions OptionsFor(SerializationFormatting formatting) => _options;

		/// <summary>
		/// The <see cref="JsonSerializerOptions"/> backing this serializer. Exposed so the high-level
		/// client can reach the connection settings threaded into the options' converters (#388).
		/// </summary>
		internal JsonSerializerOptions Options => _options;

		/// <inheritdoc />
		/// <remarks>
		/// Buffers the entire stream into memory before deserializing; for very large
		/// responses this trades memory for simplicity. The async overload streams.
		/// </remarks>
		public object Deserialize(Type type, Stream stream)
		{
			var bytes = ReadAllBytes(stream);
			// An empty or whitespace-only body (e.g. 204/HEAD/empty 200) yields default, matching the
			// vendored serializer; System.Text.Json would otherwise throw on missing tokens.
			return IsBlank(bytes) ? null : JsonSerializer.Deserialize(bytes, type, _options);
		}

		/// <inheritdoc cref="Deserialize(Type, Stream)" />
		public T Deserialize<T>(Stream stream)
		{
			var bytes = ReadAllBytes(stream);
			return IsBlank(bytes) ? default : JsonSerializer.Deserialize<T>(bytes, _options);
		}

		/// <inheritdoc />
		public async Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
		{
			var bytes = await ReadAllBytesAsync(stream, cancellationToken).ConfigureAwait(false);
			return IsBlank(bytes) ? null : JsonSerializer.Deserialize(bytes, type, _options);
		}

		/// <inheritdoc />
		public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
		{
			var bytes = await ReadAllBytesAsync(stream, cancellationToken).ConfigureAwait(false);
			return IsBlank(bytes) ? default : JsonSerializer.Deserialize<T>(bytes, _options);
		}

		/// <inheritdoc />
		public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
		{
			// NDJSON bodies (bulk/msearch) cannot go through the single-root Utf8JsonWriter; let them
			// write directly to the stream (#388).
			if (data is ISystemTextJsonSelfSerializable selfSerializable)
			{
				selfSerializable.Write(stream, this, formatting);
				return;
			}

			JsonSerializer.Serialize(stream, data, OptionsFor(formatting));
		}

		/// <inheritdoc />
		public async Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None,
			CancellationToken cancellationToken = default
		)
		{
			if (data is ISystemTextJsonSelfSerializable selfSerializable)
			{
				using var buffer = new MemoryStream();
				selfSerializable.Write(buffer, this, formatting);
				buffer.Position = 0;
				await buffer.CopyToAsync(stream, 81920, cancellationToken).ConfigureAwait(false);
				return;
			}

			await JsonSerializer.SerializeAsync(stream, data, OptionsFor(formatting), cancellationToken).ConfigureAwait(false);
		}

		private static byte[] ReadAllBytes(Stream stream)
		{
			if (stream is MemoryStream existing) return existing.ToArray();
			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			return ms.ToArray();
		}

		private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken cancellationToken)
		{
			if (stream is MemoryStream existing) return existing.ToArray();
			using var ms = new MemoryStream();
			await stream.CopyToAsync(ms, 81920, cancellationToken).ConfigureAwait(false);
			return ms.ToArray();
		}

		private static bool IsBlank(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0) return true;
			foreach (var b in bytes)
				if (b != (byte)' ' && b != (byte)'\t' && b != (byte)'\n' && b != (byte)'\r')
					return false;
			return true;
		}
	}
}
