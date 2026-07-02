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

			return options;
		}

		private JsonSerializerOptions GetOptions(SerializationFormatting formatting) =>
			formatting == SerializationFormatting.Indented ? _indentedOptions : _options;

		/// <inheritdoc />
		public object Deserialize(Type type, Stream stream)
		{
			if (stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0))
				return type.IsValueType ? Activator.CreateInstance(type) : null;

			return JsonSerializer.Deserialize(stream, type, _options);
		}

		/// <inheritdoc />
		public T Deserialize<T>(Stream stream)
		{
			if (stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0))
				return default;

			return JsonSerializer.Deserialize<T>(stream, _options);
		}

		/// <inheritdoc />
		public async Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
		{
			if (stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0))
				return type.IsValueType ? Activator.CreateInstance(type) : null;

			return await JsonSerializer.DeserializeAsync(stream, type, _options, cancellationToken).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
		{
			if (stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0))
				return default;

			return await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken).ConfigureAwait(false);
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
