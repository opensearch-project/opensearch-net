/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
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
		private readonly JsonSerializerOptions _indentedOptions;

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
			_options = options ?? new JsonSerializerOptions();
			// A parallel options instance that only differs by indentation, so the
			// SerializationFormatting hint can be honored without mutating state.
			_indentedOptions = new JsonSerializerOptions(_options) { WriteIndented = true };
		}

		private JsonSerializerOptions OptionsFor(SerializationFormatting formatting) =>
			formatting == SerializationFormatting.Indented ? _indentedOptions : _options;

		/// <inheritdoc />
		/// <remarks>
		/// Buffers the entire stream into memory before deserializing; for very large
		/// responses this trades memory for simplicity. The async overload streams.
		/// </remarks>
		public object Deserialize(Type type, Stream stream) =>
			JsonSerializer.Deserialize(ReadAllBytes(stream), type, _options);

		/// <inheritdoc cref="Deserialize(Type, Stream)" />
		public T Deserialize<T>(Stream stream) =>
			JsonSerializer.Deserialize<T>(ReadAllBytes(stream), _options);

		/// <inheritdoc />
		public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default) =>
			JsonSerializer.DeserializeAsync(stream, type, _options, cancellationToken).AsTask();

		/// <inheritdoc />
		public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default) =>
			JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken).AsTask();

		/// <inheritdoc />
		public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None) =>
			JsonSerializer.Serialize(stream, data, OptionsFor(formatting));

		/// <inheritdoc />
		public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None,
			CancellationToken cancellationToken = default
		) =>
			JsonSerializer.SerializeAsync(stream, data, OptionsFor(formatting), cancellationToken);

		private static byte[] ReadAllBytes(Stream stream)
		{
			if (stream is MemoryStream existing) return existing.ToArray();
			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			return ms.ToArray();
		}
	}
}
