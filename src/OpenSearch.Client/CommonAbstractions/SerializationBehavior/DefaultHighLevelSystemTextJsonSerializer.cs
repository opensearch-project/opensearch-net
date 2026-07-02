/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

#nullable enable

using System;
#nullable enable

using System.IO;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using System.Threading;
#nullable enable

using System.Threading.Tasks;
#nullable enable

using OpenSearch.Net;
#nullable enable

using OpenSearch.Net.Serialization.Converters;

namespace OpenSearch.Client
{
	/// <summary>
	/// The built-in internal serializer that the high level client OpenSearch.Client uses,
	/// based on System.Text.Json. Replaces the legacy Utf8Json-based <see cref="DefaultHighLevelSerializer"/>.
	/// </summary>
	internal class DefaultHighLevelSystemTextJsonSerializer : IOpenSearchSerializer
	{
		private readonly JsonSerializerOptions _options;

		/// <summary>
		/// The connection settings values used by converters that need access to property mappings,
		/// field name inferrer, and other client configuration.
		/// </summary>
		public IConnectionSettingsValues Settings { get; }

		/// <summary>
		/// The source serializer used for user document (POCO) handling.
		/// This is set after construction since there may be a circular dependency
		/// between the request serializer and the source serializer.
		/// </summary>
		public IOpenSearchSerializer SourceSerializer { get; set; }

		public DefaultHighLevelSystemTextJsonSerializer(IConnectionSettingsValues settings)
		{
			Settings = settings ?? throw new ArgumentNullException(nameof(settings));

			_options = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				NumberHandling = JsonNumberHandling.AllowReadingFromString
			};

			// Register converters in priority order.
			// OpenSearchClientConverterFactory handles domain-specific types (ReadAs, polymorphism, etc.)
			_options.Converters.Add(new OpenSearchClientConverterFactory(settings));

			// Low-level converters from OpenSearch.Net
			_options.Converters.Add(new DynamicDictionaryConverter());
			_options.Converters.Add(new NullableStringIntConverter());
			_options.Converters.Add(new StringEnumConverterFactory());
		}

		/// <summary>
		/// Returns the configured <see cref="JsonSerializerOptions"/> for use by converters
		/// that need to perform sub-serialization.
		/// </summary>
		public JsonSerializerOptions GetJsonSerializerOptions() => _options;

		/// <inheritdoc />
		public T Deserialize<T>(Stream stream)
		{
			if (stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0))
				return default;

			return JsonSerializer.Deserialize<T>(stream, _options);
		}

		/// <inheritdoc />
		public object Deserialize(Type type, Stream stream)
		{
			if (stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0))
				return null;

			return JsonSerializer.Deserialize(stream, type, _options);
		}

		/// <inheritdoc />
		public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
		{
			if (stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0))
				return default;

			return await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
		{
			if (stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0))
				return null;

			return await JsonSerializer.DeserializeAsync(stream, type, _options, cancellationToken).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));

			using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
			{
				Indented = formatting == SerializationFormatting.Indented
			});
			JsonSerializer.Serialize(writer, data, _options);
			writer.Flush();
		}

		/// <inheritdoc />
		public async Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None,
			CancellationToken cancellationToken = default)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));

			// System.Text.Json's SerializeAsync writes directly to the stream with buffering
			await JsonSerializer.SerializeAsync(stream, data, _options, cancellationToken).ConfigureAwait(false);
		}
	}
}
