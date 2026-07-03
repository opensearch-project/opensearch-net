/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter factory for the top-level dictionary/dynamic response
	/// types (#388). These response types carry a type-level
	/// <c>[JsonFormatter(typeof(DictionaryResponseFormatter&lt;…&gt;))]</c> (or the resolvable/dynamic
	/// variants); this factory recognizes those attributes and produces the matching read-only
	/// converter, closing it over the same type arguments and threading the connection settings into the
	/// resolvable variant.
	/// </summary>
	internal sealed class ResponseFormatterConverterFactory : JsonConverterFactory
	{
		private readonly IConnectionSettingsValues _settings;

		public ResponseFormatterConverterFactory(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override bool CanConvert(Type typeToConvert) => TryGetFormatter(typeToConvert, out _, out _);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (!TryGetFormatter(typeToConvert, out var definition, out var args))
				return null;

			if (definition == typeof(DictionaryResponseFormatter<,,>))
				return (JsonConverter)Activator.CreateInstance(typeof(DictionaryResponseConverter<,,>).MakeGenericType(args));
			if (definition == typeof(ResolvableDictionaryResponseFormatter<,,>))
				return (JsonConverter)Activator.CreateInstance(typeof(ResolvableDictionaryResponseConverter<,,>).MakeGenericType(args), _settings);
			if (definition == typeof(DynamicResponseFormatter<>))
				return (JsonConverter)Activator.CreateInstance(typeof(DynamicResponseConverter<>).MakeGenericType(args));

			return null;
		}

		private static bool TryGetFormatter(Type type, out Type definition, out Type[] arguments)
		{
			definition = null;
			arguments = null;
			var attribute = type.GetCustomAttribute<OpenSearch.Net.Utf8Json.JsonFormatterAttribute>(false);
			var formatterType = attribute?.FormatterType;
			if (formatterType == null || !formatterType.IsGenericType) return false;

			var d = formatterType.GetGenericTypeDefinition();
			if (d == typeof(DictionaryResponseFormatter<,,>)
				|| d == typeof(ResolvableDictionaryResponseFormatter<,,>)
				|| d == typeof(DynamicResponseFormatter<>))
			{
				definition = d;
				arguments = formatterType.GetGenericArguments();
				return true;
			}

			return false;
		}
	}

	/// <summary> Shared read helpers for the dictionary/dynamic response readers (#388). </summary>
	internal static class ResponseReader
	{
		public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(JsonElement root, JsonSerializerOptions options,
			out Error error, out int? statusCode)
		{
			error = null;
			statusCode = null;
			var dictionary = new Dictionary<TKey, TValue>();

			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "error":
						error = member.Value.ValueKind == JsonValueKind.String
							? new Error { Reason = member.Value.GetString() }
							: member.Value.Deserialize<Error>(options);
						break;
					case "status":
						if (member.Value.ValueKind == JsonValueKind.Number)
							statusCode = member.Value.GetInt32();
						break;
					default:
						dictionary[ConvertKey<TKey>(member.Name, options)] = member.Value.Deserialize<TValue>(options);
						break;
				}
			}

			return dictionary;
		}

		private static TKey ConvertKey<TKey>(string name, JsonSerializerOptions options) =>
			typeof(TKey) == typeof(string)
				? (TKey)(object)name
				: JsonSerializer.Deserialize<TKey>(JsonSerializer.Serialize(name), options);
	}

	/// <summary> Read-only converter for a plain <see cref="IDictionaryResponse{TKey,TValue}"/> (#388). </summary>
	internal sealed class DictionaryResponseConverter<TResponse, TKey, TValue> : JsonConverter<TResponse>
		where TResponse : ResponseBase, IDictionaryResponse<TKey, TValue>, new()
	{
		public override TResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var dictionary = ResponseReader.ReadDictionary<TKey, TValue>(document.RootElement, options, out var error, out var statusCode);
			var response = new TResponse { Error = error, StatusCode = statusCode };
			((IDictionaryResponse<TKey, TValue>)response).BackingDictionary = dictionary;
			return response;
		}

		public override void Write(Utf8JsonWriter writer, TResponse value, JsonSerializerOptions options) =>
			throw new NotSupportedException($"{typeof(TResponse).Name} is a response type and is not serialized.");
	}

	/// <summary> Read-only converter for a settings-resolvable <see cref="IDictionaryResponse{TKey,TValue}"/> (#388). </summary>
	internal sealed class ResolvableDictionaryResponseConverter<TResponse, TKey, TValue> : JsonConverter<TResponse>
		where TResponse : ResponseBase, IDictionaryResponse<TKey, TValue>, new()
		where TKey : IUrlParameter
	{
		private readonly IConnectionSettingsValues _settings;

		public ResolvableDictionaryResponseConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override TResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var dictionary = ResponseReader.ReadDictionary<TKey, TValue>(document.RootElement, options, out var error, out var statusCode);
			var response = new TResponse { Error = error, StatusCode = statusCode };
			((IDictionaryResponse<TKey, TValue>)response).BackingDictionary = new ResolvableDictionaryProxy<TKey, TValue>(_settings, dictionary);
			return response;
		}

		public override void Write(Utf8JsonWriter writer, TResponse value, JsonSerializerOptions options) =>
			throw new NotSupportedException($"{typeof(TResponse).Name} is a response type and is not serialized.");
	}

	/// <summary> Read-only converter for a <see cref="IDynamicResponse"/> (#388). </summary>
	internal sealed class DynamicResponseConverter<TResponse> : JsonConverter<TResponse>
		where TResponse : ResponseBase, IDynamicResponse, new()
	{
		public override TResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var dictionary = ResponseReader.ReadDictionary<string, object>(document.RootElement, options, out var error, out var statusCode);
			var response = new TResponse { Error = error, StatusCode = statusCode };
			((IDynamicResponse)response).BackingDictionary = DynamicDictionary.Create(dictionary);
			return response;
		}

		public override void Write(Utf8JsonWriter writer, TResponse value, JsonSerializerOptions options) =>
			throw new NotSupportedException($"{typeof(TResponse).Name} is a response type and is not serialized.");
	}
}
