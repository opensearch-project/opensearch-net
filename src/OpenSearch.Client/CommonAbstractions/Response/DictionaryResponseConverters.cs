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
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// Shared read helpers for the System.Text.Json replacements of the legacy Utf8Json
	/// <c>DictionaryResponseFormatter</c>, <c>ResolvableDictionaryResponseFormatter</c> and
	/// <c>DynamicResponseFormatter</c>.
	///
	/// Every one of those legacy formatters parses a response whose JSON body is a flat object: most keys are
	/// data entries, but the standard server-error envelope fields <c>error</c> and <c>status</c> are pulled off
	/// into <see cref="ResponseBase.Error"/> / <see cref="ResponseBase.StatusCode"/> rather than the dictionary,
	/// exactly as the <c>ResponseFormatterHelpers.ServerErrorFields</c> automaton did on the old engine.
	/// </summary>
	internal static class DictionaryResponseConverterHelpers
	{
		/// <summary>
		/// If <paramref name="property"/> is one of the server-error envelope fields (<c>error</c>/<c>status</c>)
		/// it is consumed into <paramref name="response"/> and <c>true</c> is returned; otherwise <c>false</c>,
		/// meaning the caller should treat it as a data entry. Mirrors the legacy formatter branch-for-branch:
		/// a string <c>error</c> becomes <c>new Error { Reason = ... }</c>; any other shape is deserialized as an
		/// <see cref="Error"/>; a numeric <c>status</c> sets the status code; a non-numeric <c>status</c> is skipped.
		/// </summary>
		public static bool TryReadServerErrorField(ResponseBase response, JsonProperty property, JsonSerializerOptions options)
		{
			switch (property.Name)
			{
				case "error":
					if (property.Value.ValueKind == JsonValueKind.String)
						response.Error = new Error { Reason = property.Value.GetString() };
					else
						response.Error = property.Value.Deserialize<Error>(options);
					return true;
				case "status":
					if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out var status))
						response.StatusCode = status;
					// A non-numeric status is skipped, matching the legacy formatter's ReadNextBlock().
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Deserializes a JSON object property name into a strongly-typed dictionary key. On the old engine the key
		/// formatter deserialized the (quoted) property name; here we re-emit the name as a JSON string so that any
		/// registered key converter (e.g. the settings-aware <see cref="IndexNameConverter"/>) is applied identically.
		/// The common <c>string</c> key short-circuits without a serializer round-trip.
		/// </summary>
		public static TKey DeserializeKey<TKey>(string propertyName, JsonSerializerOptions options)
		{
			if (typeof(TKey) == typeof(string))
				return (TKey)(object)propertyName;

			var quoted = JsonSerializer.Serialize(propertyName);
			return JsonSerializer.Deserialize<TKey>(quoted, options);
		}
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>DictionaryResponseFormatter&lt;TResponse, TKey, TValue&gt;</c>.
	/// Parses a response whose body is a JSON object into a strongly-typed <typeparamref name="TResponse"/> whose
	/// <see cref="IDictionaryResponse{TKey,TValue}.BackingDictionary"/> holds every non-envelope entry.
	///
	/// <see cref="Utf8JsonReader"/> is forward-only, so the body is buffered into a <see cref="JsonDocument"/> and read
	/// from the DOM. Each value is handed to <see cref="JsonSerializer"/> as <typeparamref name="TValue"/> so registered
	/// converters apply. This is an open generic and cannot be registered directly on
	/// <c>JsonSerializerOptions.Converters</c>; <see cref="DictionaryResponseConverterFactory"/> constructs the closed
	/// converter from the legacy <c>[JsonFormatter]</c> attribute.
	/// </summary>
	internal class DictionaryResponseConverter<TResponse, TKey, TValue> : JsonConverter<TResponse>
		where TResponse : ResponseBase, IDictionaryResponse<TKey, TValue>, new()
	{
		// A JSON null must yield an (empty) response, matching the legacy formatter which simply never entered its
		// object-reading loop. STJ skips the converter for a null reference type unless HandleNull is opted in.
		public override bool HandleNull => true;

		public override TResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var response = new TResponse();
			var dictionary = new Dictionary<TKey, TValue>();

			if (reader.TokenType == JsonTokenType.Null)
			{
				response.BackingDictionary = dictionary;
				return response;
			}

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			if (root.ValueKind == JsonValueKind.Object)
			{
				foreach (var property in root.EnumerateObject())
				{
					if (DictionaryResponseConverterHelpers.TryReadServerErrorField(response, property, options))
						continue;

					var key = DictionaryResponseConverterHelpers.DeserializeKey<TKey>(property.Name, options);
					var value = property.Value.Deserialize<TValue>(options);
					dictionary.Add(key, value);
				}
			}

			response.BackingDictionary = dictionary;
			return response;
		}

		public override void Write(Utf8JsonWriter writer, TResponse value, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json
	/// <c>ResolvableDictionaryResponseFormatter&lt;TResponse, TKey, TValue&gt;</c>. Behaves like
	/// <see cref="DictionaryResponseConverter{TResponse,TKey,TValue}"/> but wraps the parsed entries in a
	/// <see cref="ResolvableDictionaryProxy{TKey,TValue}"/> so keys can be resolved through the runtime
	/// <c>Inferrer</c> — hence a <see cref="SettingsAwareConverter{T}"/> whose <see cref="IConnectionSettingsValues"/>
	/// is injected by <see cref="DictionaryResponseConverterFactory"/> (the old engine read settings from the
	/// resolver via <c>formatterResolver.GetConnectionSettings()</c>).
	/// </summary>
	internal class ResolvableDictionaryResponseConverter<TResponse, TKey, TValue> : SettingsAwareConverter<TResponse>
		where TResponse : ResponseBase, IDictionaryResponse<TKey, TValue>, new()
		where TKey : IUrlParameter
	{
		public ResolvableDictionaryResponseConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override bool HandleNull => true;

		public override TResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var response = new TResponse();
			var dictionary = new Dictionary<TKey, TValue>();

			if (reader.TokenType != JsonTokenType.Null)
			{
				using var document = JsonDocument.ParseValue(ref reader);
				var root = document.RootElement;

				if (root.ValueKind == JsonValueKind.Object)
				{
					foreach (var property in root.EnumerateObject())
					{
						if (DictionaryResponseConverterHelpers.TryReadServerErrorField(response, property, options))
							continue;

						var key = DictionaryResponseConverterHelpers.DeserializeKey<TKey>(property.Name, options);
						var value = property.Value.Deserialize<TValue>(options);
						dictionary.Add(key, value);
					}
				}
			}

			response.BackingDictionary = new ResolvableDictionaryProxy<TKey, TValue>(Settings, dictionary);
			return response;
		}

		public override void Write(Utf8JsonWriter writer, TResponse value, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>DynamicResponseFormatter&lt;TResponse&gt;</c>. Captures the
	/// whole response body into a <see cref="DynamicDictionary"/> (minus the server-error envelope fields).
	///
	/// The legacy formatter read each value through Utf8Json's <c>object</c> formatter, which produced boxed CLR
	/// primitives and nested <c>Dictionary&lt;string, object&gt;</c>/<c>List&lt;object&gt;</c>. Under System.Text.Json a
	/// plain <c>Deserialize&lt;object&gt;</c> would instead yield <see cref="JsonElement"/>, which
	/// <see cref="DynamicDictionary"/> cannot navigate — so values are materialized explicitly here, mirroring the
	/// existing <c>DynamicDictionaryConverter</c>.
	/// </summary>
	internal class DynamicResponseConverter<TResponse> : JsonConverter<TResponse>
		where TResponse : ResponseBase, IDynamicResponse, new()
	{
		public override bool HandleNull => true;

		public override TResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var response = new TResponse();
			var dictionary = new Dictionary<string, object>();

			if (reader.TokenType != JsonTokenType.Null)
			{
				using var document = JsonDocument.ParseValue(ref reader);
				var root = document.RootElement;

				if (root.ValueKind == JsonValueKind.Object)
				{
					foreach (var property in root.EnumerateObject())
					{
						if (DictionaryResponseConverterHelpers.TryReadServerErrorField(response, property, options))
							continue;

						dictionary[property.Name] = ReadValue(property.Value);
					}
				}
			}

			response.BackingDictionary = DynamicDictionary.Create(dictionary);
			return response;
		}

		public override void Write(Utf8JsonWriter writer, TResponse value, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		// Materializes a JsonElement into boxed CLR values matching the legacy object formatter and the existing
		// DynamicDictionaryConverter: long/double for numbers, nested Dictionary<string, object>/List<object>.
		private static object ReadValue(JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Null:
				case JsonValueKind.Undefined:
					return null;
				case JsonValueKind.True:
					return true;
				case JsonValueKind.False:
					return false;
				case JsonValueKind.Number:
					return element.TryGetInt64(out var l) ? l : (object)element.GetDouble();
				case JsonValueKind.String:
					return element.GetString();
				case JsonValueKind.Object:
				{
					var nested = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					foreach (var property in element.EnumerateObject())
						nested[property.Name] = ReadValue(property.Value);
					return nested;
				}
				case JsonValueKind.Array:
				{
					var list = new List<object>();
					foreach (var item in element.EnumerateArray())
						list.Add(ReadValue(item));
					return list;
				}
				default:
					return null;
			}
		}
	}
}
