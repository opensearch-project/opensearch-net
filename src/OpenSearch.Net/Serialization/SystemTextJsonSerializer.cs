/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.Serialization.Converters;

namespace OpenSearch.Net
{
	/// <summary>
	/// A serializer implementation using System.Text.Json, replacing the legacy Utf8Json-based
	/// <see cref="LowLevelRequestResponseSerializer"/>.
	/// </summary>
	public class SystemTextJsonSerializer : IOpenSearchSerializer
	{
		public static readonly SystemTextJsonSerializer Instance = new SystemTextJsonSerializer();

		private readonly JsonSerializerOptions _options;
		private readonly JsonSerializerOptions _indentedOptions;

		public SystemTextJsonSerializer()
		{
			_options = CreateOptions(writeIndented: false);
			_indentedOptions = CreateOptions(writeIndented: true);
		}

		private static JsonSerializerOptions CreateOptions(bool writeIndented)
		{
			var options = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				WriteIndented = writeIndented
			};

			options.Converters.Add(new DynamicDictionaryConverter());
			options.Converters.Add(new NullableStringIntConverter());
			options.Converters.Add(new StringEnumConverterFactory());
			// ISO 8601 date/time parsing matching the legacy engine (basic-format offsets, >7 fractional digits);
			// the built-in Utf8JsonReader rejects these (GitHub issue #4876).
			options.Converters.Add(new Iso8601DateTimeConverter());
			options.Converters.Add(new NullableIso8601DateTimeConverter());
			options.Converters.Add(new Iso8601DateTimeOffsetConverter());
			options.Converters.Add(new NullableIso8601DateTimeOffsetConverter());
			// ErrorConverter must precede ErrorCauseConverter: Error derives from ErrorCause, and
			// System.Text.Json selects the first converter whose type is assignable from the target.
			options.Converters.Add(new ErrorConverter());
			options.Converters.Add(new ErrorCauseConverter());
			options.Converters.Add(new ExceptionConverterFactory());

			return options;
		}

		private JsonSerializerOptions GetOptions(SerializationFormatting formatting) =>
			formatting == SerializationFormatting.Indented ? _indentedOptions : _options;

		/// <inheritdoc />
		public object Deserialize(Type type, Stream stream)
		{
			var bytes = ReadToArray(stream, out var blank);
			if (blank)
				return type.IsValueType ? Activator.CreateInstance(type) : null;

			return JsonSerializer.Deserialize(bytes, type, _options);
		}

		/// <inheritdoc />
		public T Deserialize<T>(Stream stream)
		{
			var bytes = ReadToArray(stream, out var blank);
			if (blank)
				return default;

			return JsonSerializer.Deserialize<T>(bytes, _options);
		}

		/// <inheritdoc />
		public async Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
		{
			var bytes = await ReadToArrayAsync(stream, cancellationToken).ConfigureAwait(false);
			if (IsBlank(bytes))
				return type.IsValueType ? Activator.CreateInstance(type) : null;

			return JsonSerializer.Deserialize(bytes, type, _options);
		}

		/// <inheritdoc />
		public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
		{
			var bytes = await ReadToArrayAsync(stream, cancellationToken).ConfigureAwait(false);
			if (IsBlank(bytes))
				return default;

			return JsonSerializer.Deserialize<T>(bytes, _options);
		}

		// A response body can be empty or whitespace-only (e.g. the HEAD used by Ping, or a 200 with no body). The
		// CanSeek/Length==0 check alone misses non-seekable network streams and whitespace-only payloads, on which
		// System.Text.Json throws "The input does not contain any JSON tokens". Read the stream fully and treat a blank
		// payload as default/null, matching the legacy Utf8Json engine (which returned default for empty input).
		private static byte[] ReadToArray(Stream stream, out bool blank)
		{
			if (stream == null || stream == Stream.Null)
			{
				blank = true;
				return Array.Empty<byte>();
			}

			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			var bytes = ms.ToArray();
			blank = IsBlank(bytes);
			return bytes;
		}

		private static async Task<byte[]> ReadToArrayAsync(Stream stream, CancellationToken cancellationToken)
		{
			if (stream == null || stream == Stream.Null)
				return Array.Empty<byte>();

			using var ms = new MemoryStream();
			await stream.CopyToAsync(ms, 81920, cancellationToken).ConfigureAwait(false);
			return ms.ToArray();
		}

		private static bool IsBlank(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
				return true;

			foreach (var b in bytes)
			{
				if (b != (byte)' ' && b != (byte)'\t' && b != (byte)'\n' && b != (byte)'\r')
					return false;
			}

			return true;
		}

		/// <inheritdoc />
		public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
		{
			if (data == null) return;

			JsonSerializer.Serialize(stream, data, GetOptions(formatting));
		}

		/// <inheritdoc />
		public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None,
			CancellationToken cancellationToken = default)
		{
			if (data == null) return Task.CompletedTask;

			return JsonSerializer.SerializeAsync(stream, data, GetOptions(formatting), cancellationToken);
		}
	}
}
