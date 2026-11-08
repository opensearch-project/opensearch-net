/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Runtime.Serialization;
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
	/// Like the legacy formatter, this reads exception state via <c>ISerializable.GetObjectData</c>, so
	/// <c>RemoteStackTraceString</c> and <c>RemoteStackIndex</c> are still emitted and the output matches the legacy
	/// engine field-for-field. The one field the legacy formatter emitted that this does not is the structured
	/// <c>ExceptionMethod</c> object; on modern .NET <c>GetObjectData</c> no longer populates the underlying
	/// <c>ExceptionMethod</c> serialization entry, so the legacy formatter also produces nothing for it in practice
	/// (verified byte-for-byte across several exception shapes). Serialization is one-way; deserialization is not
	/// supported, matching the legacy behaviour.
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
				// Read the exception's serialized state via ISerializable so the output matches the legacy Utf8Json
				// ExceptionFormatter field-for-field (including RemoteStackTraceString / RemoteStackIndex, which are
				// only reachable through SerializationInfo). A field-count mismatch fails the Json.NET parity test.
				string className = e.GetType().FullName;
				string source = e.Source;
				string stackTrace = e.StackTrace;
				string remoteStackTrace = null;
				var remoteStackIndex = 0;
				var hResult = e.HResult;
				string helpUrl = e.HelpLink;

				try
				{
#pragma warning disable SYSLIB0050, SYSLIB0051 // Formatter-based serialization APIs are obsolete
					var si = new SerializationInfo(e.GetType(), new FormatterConverter());
					e.GetObjectData(si, new StreamingContext());
#pragma warning restore SYSLIB0050, SYSLIB0051
					className = si.GetString("ClassName") ?? className;
					source = si.GetString("Source");
					stackTrace = si.GetString("StackTraceString");
					remoteStackTrace = si.GetString("RemoteStackTraceString");
					remoteStackIndex = si.GetInt32("RemoteStackIndex");
					hResult = si.GetInt32("HResult");
					helpUrl = si.GetString("HelpURL");
				}
				catch
				{
					// GetObjectData can throw for exotic exception types; fall back to the property values above.
				}

				writer.WriteStartObject();

				writer.WriteNumber("Depth", depth);
				writer.WriteString("ClassName", className);
				writer.WriteString("Message", e.Message);
				writer.WriteString("Source", source);
				writer.WriteString("StackTraceString", stackTrace);
				writer.WriteString("RemoteStackTraceString", remoteStackTrace);
				writer.WriteNumber("RemoteStackIndex", remoteStackIndex);
				writer.WriteNumber("HResult", hResult);
				writer.WriteString("HelpURL", helpUrl);

				writer.WriteEndObject();
			}
		}
	}
}
