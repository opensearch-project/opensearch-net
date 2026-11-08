/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>MultiGetRequestFormatter</c>.
	///
	/// Unlike the bulk/multi-search request bodies, a multi-get request is a <em>single</em> JSON object — either
	/// <c>{ "ids": [ … ] }</c> (when every document can be flattened to a bare id) or <c>{ "docs": [ … ] }</c> — so
	/// there is no newline-delimited output here; the converter writes straight to the supplied
	/// <see cref="Utf8JsonWriter"/>.
	///
	/// Settings-aware: reproduces the legacy request-level-index flattening (if a request-level index is set and a
	/// document resolves to the same index, the document's index is cleared) which the old formatter performed via
	/// <c>formatterResolver.GetConnectionSettings()</c>.
	/// </summary>
	internal class MultiGetRequestConverter : SettingsAwareConverter<IMultiGetRequest>
	{
		public MultiGetRequestConverter(IConnectionSettingsValues settings) : base(settings) { }

		// The legacy Deserialize threw NotSupportedException — a multi-get request body is never read back.
		public override IMultiGetRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, IMultiGetRequest value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			if (!(value?.Documents.HasAny()).GetValueOrDefault(false))
			{
				writer.WriteEndObject();
				return;
			}

			List<IMultiGetOperation> docs;

			// If an index is specified at the request level and a document has the same index, remove the index.
			if (value.Index != null)
			{
				var resolvedIndex = value.Index.GetString(Settings);
				docs = value.Documents.Select(d =>
					{
						if (d.Index == null)
							return d;

						var docIndex = d.Index.GetString(Settings);
						if (string.Equals(resolvedIndex, docIndex)) d.Index = null;
						return d;
					})
					.ToList();
			}
			else
				docs = value.Documents.ToList();

			var flatten = docs.All(p => p.CanBeFlattened);

			writer.WritePropertyName(flatten ? "ids" : "docs");

			writer.WriteStartArray();
			for (var index = 0; index < docs.Count; index++)
			{
				var doc = docs[index];
				if (flatten)
					// The legacy IdFormatter serialized a bare Id (string or number); delegate to the Id converter.
					JsonSerializer.Serialize(writer, doc.Id, options);
				else
					// Serialize with the runtime type so all [DataMember] members resolve.
					JsonSerializer.Serialize(writer, doc, doc.GetType(), options);
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
	}
}
