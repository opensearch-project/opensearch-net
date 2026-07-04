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

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// The <see cref="System.Text.Json"/> counterpart of the vendored Utf8Json <c>SourceFormatter&lt;T&gt;</c>
	/// (see GitHub issue #388). Members that carry <c>[JsonFormatter(typeof(SourceFormatter&lt;&gt;))]</c>
	/// (a document body such as a hit's <c>_source</c>, an update <c>doc</c>/<c>upsert</c>, or a
	/// term-vector/percolate document) are (de)serialized through the connection's
	/// <see cref="IConnectionSettingsValues.SourceSerializer"/> rather than the request/response
	/// serializer, mirroring the Utf8Json behavior. This is what applies the client's document
	/// field-name inference to user POCOs, which the request/response contract resolver does not do.
	/// <para>
	/// The converter is stateless (instantiated once per closed type and cached) and resolves the
	/// source serializer from the ambient options at call time via
	/// <see cref="SourceSerializerProviderConverter"/>, which the options factory registers with the
	/// active connection settings.
	/// </para>
	/// </summary>
	internal class SourceConverter<T> : JsonConverter<T>
	{
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return default;

			using var document = JsonDocument.ParseValue(ref reader);
			var sourceSerializer = SourceSerializerProviderConverter.Resolve(options);
			if (sourceSerializer == null)
				return document.Deserialize<T>(options);

			var bytes = System.Text.Encoding.UTF8.GetBytes(document.RootElement.GetRawText());
			using var stream = new MemoryStream(bytes);
			return (T)sourceSerializer.Deserialize(typeToConvert, stream);
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var sourceSerializer = SourceSerializerProviderConverter.Resolve(options);
			if (sourceSerializer == null)
			{
				JsonSerializer.Serialize(writer, value, typeof(T), options);
				return;
			}

			using var stream = new MemoryStream();
			sourceSerializer.Serialize(value, stream);
			stream.Position = 0;
			using var document = JsonDocument.Parse(stream);
			document.RootElement.WriteTo(writer);
		}
	}

	/// <summary>
	/// A carrier converter that exposes the active connection's source serializer to the otherwise
	/// stateless, parameterless per-property converters (e.g. <see cref="SourceConverter{T}"/>). It is
	/// registered in the options solely so it can be located via <see cref="Resolve"/>; it never
	/// (de)serializes its marker type.
	/// </summary>
	internal sealed class SourceSerializerProviderConverter : JsonConverter<SourceSerializerProviderConverter.Marker>
	{
		internal sealed class Marker { }

		private readonly IConnectionSettingsValues _settings;

		public SourceSerializerProviderConverter(IConnectionSettingsValues settings) => _settings = settings;

		// Resolved lazily: when the request/response options are built during connection-settings
		// construction, the source serializer has not been assigned yet (it is created afterwards).
		public IOpenSearchSerializer SourceSerializer => _settings?.SourceSerializer;

		public IConnectionSettingsValues Settings => _settings;

		public static SourceSerializerProviderConverter Find(JsonSerializerOptions options)
		{
			if (options == null) return null;
			for (var i = 0; i < options.Converters.Count; i++)
				if (options.Converters[i] is SourceSerializerProviderConverter provider)
					return provider;
			return null;
		}

		public static IOpenSearchSerializer Resolve(JsonSerializerOptions options) => Find(options)?.SourceSerializer;

		public override Marker Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, Marker value, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}
}
