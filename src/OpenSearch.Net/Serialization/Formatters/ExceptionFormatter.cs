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
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using OpenSearch.Net.Extensions;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Net
{
	internal class ExceptionFormatterResolver : IJsonFormatterResolver
	{
		public static readonly ExceptionFormatterResolver Instance = new ExceptionFormatterResolver();

		private ExceptionFormatterResolver() { }

		public IJsonFormatter<T> GetFormatter<T>()
		{
			if (typeof(Exception).IsAssignableFrom(typeof(T)))
			{
				var type = typeof(ExceptionFormatter<>).MakeGenericType(typeof(T));
				return (IJsonFormatter<T>)type.CreateInstance();
			}

			return null;
		}
	}

	internal class ExceptionFormatter<TException> : IJsonFormatter<TException> where TException : Exception
	{
		private static List<Dictionary<string, object>> FlattenExceptions(Exception e)
		{
			var maxExceptions = 20;
			var exceptions = new List<Dictionary<string, object>>(maxExceptions);
			var depth = 0;
			do
			{
				var o = ToDictionary(e, depth);
				exceptions.Add(o);
				depth++;
				e = e.InnerException;
			} while (depth < maxExceptions && e != null);

			return exceptions;
		}

		private static Dictionary<string, object> ToDictionary(Exception e, int depth)
		{
			var o = new Dictionary<string, object>(10);

#if NET8_0_OR_GREATER
    //NOTE: This is a workaround for the obsolete warning in .NET 8.0. We need to find a work around and remove this pragma
#pragma warning disable SYSLIB0050 // Type or member is obsolete
			var si = new SerializationInfo(e.GetType(), new FormatterConverter());
#pragma warning restore SYSLIB0050 // Type or member is obsolete
			var sc = new StreamingContext();
#pragma warning disable SYSLIB0051 // Type or member is obsolete
			e.GetObjectData(si, sc);
#pragma warning restore SYSLIB0051 // Type or member is obsolete
#else
			var si = new SerializationInfo(e.GetType(), new FormatterConverter());
			var sc = new StreamingContext();
			e.GetObjectData(si, sc);
#endif
			var helpUrl = si.GetString("HelpURL");
			var stackTrace = si.GetString("StackTraceString");
			var remoteStackTrace = si.GetString("RemoteStackTraceString");
			var remoteStackIndex = si.GetInt32("RemoteStackIndex");
			var exceptionMethod = si.GetString("ExceptionMethod");
			var hresult = si.GetInt32("HResult");
			var source = si.GetString("Source");
			var className = si.GetString("ClassName");

			o.Add("Depth", depth);
			o.Add("ClassName", className);
			o.Add("Message", e.Message);
			o.Add("Source", source);
			o.Add("StackTraceString", stackTrace);
			o.Add("RemoteStackTraceString", remoteStackTrace);
			o.Add("RemoteStackIndex", remoteStackIndex);
			o.Add("HResult", hresult);
			o.Add("HelpURL", helpUrl);

			WriteStructuredExceptionMethod(o, exceptionMethod);
			return o;
		}

		private static void WriteStructuredExceptionMethod(Dictionary<string,object> o, string exceptionMethodString)
		{
			if (string.IsNullOrWhiteSpace(exceptionMethodString)) return;

			var args = exceptionMethodString.Split('\0', '\n');

			if (args.Length != 5) return;

			var memberType = int.Parse(args[0], CultureInfo.InvariantCulture);
			var name = args[1];
			var assemblyName = args[2];
			var className = args[3];
			var signature = args[4];
			var an = new AssemblyName(assemblyName);
			var exceptionMethod = new Dictionary<string, object>(7)
			{
				{ "Name", name },
				{ "AssemblyName", an.Name },
				{ "AssemblyVersion", an.Version.ToString() },
				{ "AssemblyCulture", an.CultureName },
				{ "ClassName", className },
				{ "Signature", signature },
				{ "MemberType", memberType }
			};

			o.Add("ExceptionMethod", exceptionMethod);
		}

		public void Serialize(ref JsonWriter writer, TException value, IJsonFormatterResolver formatterResolver)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			var flattenedExceptions = FlattenExceptions(value);
			var valueFormatter = formatterResolver.GetFormatter<object>();

			writer.WriteBeginArray();
			for (var i = 0; i < flattenedExceptions.Count; i++)
			{
				if (i > 0)
					writer.WriteValueSeparator();

				var flattenedException = flattenedExceptions[i];
				writer.WriteBeginObject();
				var count = 0;
				foreach (var kv in flattenedException)
				{
					if (count > 0)
						writer.WriteValueSeparator();

					writer.WritePropertyName(kv.Key);
					valueFormatter.Serialize(ref writer, kv.Value, formatterResolver);
					count++;
				}
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		public TException Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver) =>
			throw new NotSupportedException();
	}
}
