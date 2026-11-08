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
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>MultiSearchTemplateFormatter</c>. This is the
	/// <em>template</em> variant of <see cref="MultiSearchConverter"/>: the request body is <em>newline-delimited
	/// JSON</em> (ndjson) where, for each search-template operation, a header object
	/// (index/search_type/preference/routing/ignore_unavailable) is written followed by a raw <c>'\n'</c>, then the
	/// search-template body followed by another raw <c>'\n'</c>.
	///
	/// A <see cref="Utf8JsonWriter"/> forbids more than one JSON value at the document root and cannot emit a bare
	/// newline between values, so the whole payload is built into a buffer (using nested writers that inherit the
	/// serializer's encoder) and emitted verbatim with a single
	/// <see cref="Utf8JsonWriter.WriteRawValue(System.ReadOnlySpan{byte}, bool)"/> call
	/// (<c>skipInputValidation: true</c>) — reproducing the legacy <c>writer.WriteRaw((byte)'\n')</c> bytes exactly.
	///
	/// Settings-aware: the header values are resolved through the runtime request parameters / Inferrer, which the
	/// legacy formatter obtained via <c>formatterResolver.GetConnectionSettings()</c>.
	/// </summary>
	internal class MultiSearchTemplateConverter : SettingsAwareConverter<IMultiSearchTemplateRequest>
	{
		private const byte Newline = (byte)'\n';

		public MultiSearchTemplateConverter(IConnectionSettingsValues settings) : base(settings) { }

		// The legacy Deserialize delegated to the concrete request-type formatter; the closest faithful
		// System.Text.Json equivalent is to deserialize into the concrete request type over the same options.
		public override IMultiSearchTemplateRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			JsonSerializer.Deserialize<MultiSearchTemplateRequest>(ref reader, options);

		public override void Write(Utf8JsonWriter writer, IMultiSearchTemplateRequest value, JsonSerializerOptions options)
		{
			if (value?.Operations == null)
				return;

			var writerOptions = new JsonWriterOptions { Encoder = options.Encoder, Indented = options.WriteIndented };

			using var ms = new MemoryStream();

			foreach (var operation in value.Operations.Values)
			{
				var p = operation.RequestParameters;

				string GetString(string key) => p.GetResolvedQueryStringValue(key, Settings);

				IUrlParameter indices = value.Index == null || !value.Index.Equals(operation.Index)
					? operation.Index
					: null;

				var searchType = GetString("search_type");
				if (searchType == "query_then_fetch")
					searchType = null;

				var header = new
				{
					index = indices?.GetString(Settings),
					search_type = searchType,
					preference = GetString("preference"),
					routing = GetString("routing"),
					ignore_unavailable = GetString("ignore_unavailable")
				};

				using (var hw = new Utf8JsonWriter(ms, writerOptions))
					JsonSerializer.Serialize(hw, header, options);
				ms.WriteByte(Newline);

				using (var bw = new Utf8JsonWriter(ms, writerOptions))
					JsonSerializer.Serialize(bw, operation, operation.GetType(), options);
				ms.WriteByte(Newline);
			}

			if (ms.Length == 0)
				return;

			// skipInputValidation: the buffer is newline-delimited JSON — multiple root values separated by raw '\n' —
			// which is deliberately NOT a single valid JSON document, so validation must be bypassed to emit it verbatim.
			writer.WriteRawValue(ms.ToArray(), skipInputValidation: true);
		}
	}
}
