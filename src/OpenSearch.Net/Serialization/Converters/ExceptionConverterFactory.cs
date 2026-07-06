/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// A <see cref="JsonConverterFactory"/> that serializes any <see cref="Exception"/> (and derived
	/// types) as a JSON array, one object per exception in the <see cref="Exception.InnerException"/>
	/// chain. This replaces the legacy Utf8Json <c>ExceptionFormatter&lt;TException&gt;</c> and its
	/// resolver.
	///
	/// Unlike the legacy formatter — which read exception state via the now-obsolete
	/// <c>ISerializable.GetObjectData</c> API — this reads the public properties of
	/// <see cref="Exception"/>. As a result the internal <c>RemoteStackTraceString</c>,
	/// <c>RemoteStackIndex</c> and structured <c>ExceptionMethod</c> fields are no longer emitted.
	/// Serialization is one-way; deserialization is not supported, matching the legacy behaviour.
	/// </summary>
	public class ExceptionConverterFactory : JsonConverterFactory
	{
		private const int MaxExceptionDepth = 20;

		/// <inheritdoc />
		public override bool CanConvert(Type typeToConvert) => typeof(Exception).IsAssignableFrom(typeToConvert);

		/// <inheritdoc />
		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var converterType = typeof(ExceptionConverter<>).MakeGenericType(typeToConvert);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}

		private class ExceptionConverter<TException> : JsonConverter<TException> where TException : Exception
		{
			public override TException Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
				throw new NotSupportedException();

			public override void Write(Utf8JsonWriter writer, TException value, JsonSerializerOptions options)
			{
				if (value == null)
				{
					writer.WriteNullValue();
					return;
				}

				writer.WriteStartArray();

				Exception current = value;
				var depth = 0;
				while (current != null && depth < MaxExceptionDepth)
				{
					WriteException(writer, current, depth);
					current = current.InnerException;
					depth++;
				}

				writer.WriteEndArray();
			}

			private static void WriteException(Utf8JsonWriter writer, Exception e, int depth)
			{
				writer.WriteStartObject();

				writer.WriteNumber("Depth", depth);
				writer.WriteString("ClassName", e.GetType().FullName);
				writer.WriteString("Message", e.Message);
				writer.WriteString("Source", e.Source);
				writer.WriteString("StackTraceString", e.StackTrace);
				writer.WriteNumber("HResult", e.HResult);
				writer.WriteString("HelpURL", e.HelpLink);

				writer.WriteEndObject();
			}
		}
	}
}
