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

using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// Helpers for reaching the connection state of the built-in request/response serializer, whether it
	/// is the vendored Utf8Json serializer or the <c>System.Text.Json</c> serializer (#388). The serializer
	/// handed to custom response builders is the <see cref="DiagnosticsSerializerProxy"/>, which always
	/// implements <see cref="IInternalSerializer"/>; only the Utf8Json serializer actually yields a
	/// formatter resolver, so callers must probe <see cref="IInternalSerializer.TryGetJsonFormatter"/>
	/// rather than test the interface.
	/// </summary>
	internal static class BuiltInSerializerState
	{
		/// <summary>
		/// True when the serializer participates in the Utf8Json formatter layer (i.e. the stateful
		/// <see cref="StatefulSerializerExtensions.CreateStateful{T}"/> path is available).
		/// </summary>
		public static bool UsesUtf8JsonFormatter(IOpenSearchSerializer builtInSerializer) =>
			builtInSerializer is IInternalSerializer internalSerializer && internalSerializer.TryGetJsonFormatter(out _);

		/// <summary>
		/// Resolves the connection settings behind the built-in serializer: from the Utf8Json formatter
		/// resolver when available, otherwise from the <see cref="SystemTextJsonSerializer"/> options.
		/// </summary>
		public static IConnectionSettingsValues GetConnectionSettings(IOpenSearchSerializer builtInSerializer)
		{
			if (builtInSerializer is IInternalSerializer internalSerializer && internalSerializer.TryGetJsonFormatter(out var formatter))
				return formatter.GetConnectionSettings();

			var stj = Unwrap(builtInSerializer);
			return stj != null ? SourceSerializerProviderConverter.Find(stj.Options)?.Settings : null;
		}

		private static SystemTextJsonSerializer Unwrap(IOpenSearchSerializer serializer)
		{
			if (serializer is DiagnosticsSerializerProxy proxy)
				serializer = proxy.InnerSerializer;
			return serializer as SystemTextJsonSerializer;
		}
	}
}
