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

using OpenSearch.Net.Extensions;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Net
{
	public static class OpenSearchSerializerExtensions
	{
		internal static void SerializeUsingWriter<T>(this IOpenSearchSerializer serializer, ref JsonWriter writer, T body, IConnectionConfigurationValues settings, SerializationFormatting formatting)
		{
			if (serializer is IInternalSerializer s && s.TryGetJsonFormatter(out var formatterResolver))
			{
				JsonSerializer.Serialize(ref writer, body, formatterResolver);
				return;
			}

			var memoryStreamFactory = settings.MemoryStreamFactory;
			var bodyBytes = serializer.SerializeToBytes(body, memoryStreamFactory, formatting);
			writer.WriteRaw(bodyBytes);
		}

		/// <summary>
		/// Extension method that serializes an instance of <typeparamref name="T"/> to a byte array.
		/// </summary>
		public static byte[] SerializeToBytes<T>(
			this IOpenSearchSerializer serializer,
			T data,
			SerializationFormatting formatting = SerializationFormatting.None) =>
			SerializeToBytes(serializer, data, RecyclableMemoryStreamFactory.Default, formatting);

		/// <summary>
		/// Extension method that serializes an instance of <typeparamref name="T"/> to a byte array.
		/// </summary>
		/// <param name="memoryStreamFactory">
		/// A factory yielding MemoryStream instances, defaults to <see cref="RecyclableMemoryStreamFactory"/>
		/// that yields memory streams backed by pooled byte arrays.
		/// </param>
		public static byte[] SerializeToBytes<T>(
			this IOpenSearchSerializer serializer,
			T data,
			IMemoryStreamFactory memoryStreamFactory,
			SerializationFormatting formatting = SerializationFormatting.None
		)
		{
			memoryStreamFactory ??= RecyclableMemoryStreamFactory.Default;
			using (var ms = memoryStreamFactory.Create())
			{
				serializer.Serialize(data, ms, formatting);
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Extension method that serializes an instance of <typeparamref name="T"/> to a string.
		/// </summary>
		public static string SerializeToString<T>(
			this IOpenSearchSerializer serializer,
			T data,
			SerializationFormatting formatting = SerializationFormatting.None) =>
			SerializeToString(serializer, data, RecyclableMemoryStreamFactory.Default, formatting);

		/// <summary>
		/// Extension method that serializes an instance of <typeparamref name="T"/> to a string.
		/// </summary>
		/// <param name="memoryStreamFactory">
		/// A factory yielding MemoryStream instances, defaults to <see cref="RecyclableMemoryStreamFactory"/>
		/// that yields memory streams backed by pooled byte arrays.
		/// </param>
		public static string SerializeToString<T>(
			this IOpenSearchSerializer serializer,
			T data,
			IMemoryStreamFactory memoryStreamFactory,
			SerializationFormatting formatting = SerializationFormatting.None
		)
		{
			memoryStreamFactory ??= RecyclableMemoryStreamFactory.Default;
			using (var ms = memoryStreamFactory.Create())
			{
				serializer.Serialize(data, ms, formatting);
				return ms.Utf8String();
			}
		}

	}
}
