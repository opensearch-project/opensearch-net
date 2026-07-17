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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>BulkResponseItemFormatter</c>.
	///
	/// A bulk response item is polymorphic: it is a single-key object whose outer property name is the
	/// operation type and selects the concrete <see cref="BulkResponseItemBase"/> subtype —
	/// <c>{ "index": { ... } }</c> → <see cref="BulkIndexResponseItem"/>, <c>{ "create": { ... } }</c> →
	/// <see cref="BulkCreateResponseItem"/>, <c>{ "update": { ... } }</c> → <see cref="BulkUpdateResponseItem"/>,
	/// <c>{ "delete": { ... } }</c> → <see cref="BulkDeleteResponseItem"/>. The item body (the inner object value)
	/// is then deserialized as that concrete type, exactly as the legacy formatter dispatched to the concrete
	/// formatter after reading the discriminating key.
	///
	/// Parity notes with the legacy formatter:
	/// <list type="bullet">
	/// <item>If the token is not the start of an object, the legacy skipped the value and returned <c>null</c>;
	/// here we buffer with <see cref="JsonDocument.ParseValue"/> and return <c>null</c> for any non-object value
	/// (this also covers the JSON <c>null</c> token).</item>
	/// <item>An unknown operation key (not index/create/update/delete) yielded <c>null</c> in the legacy
	/// (the switch left <c>bulkResponseItem</c> null); preserved here.</item>
	/// <item>Only the first property is considered, matching the legacy which read exactly one property name and
	/// then the matching end-object.</item>
	/// </list>
	///
	/// <see cref="Utf8JsonReader"/> is forward-only, so the value is buffered into a <see cref="JsonDocument"/>
	/// and the discriminating key + inner body are read from the DOM.
	///
	/// The legacy <c>Serialize</c> threw <see cref="NotSupportedException"/> (bulk items are only ever read from
	/// the server, never written); that behaviour is preserved.
	/// </summary>
	internal class BulkResponseItemConverter : JsonConverter<BulkResponseItemBase>
	{
		public override BulkResponseItemBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			foreach (var property in root.EnumerateObject())
			{
				switch (property.Name)
				{
					case "delete": return property.Value.Deserialize<BulkDeleteResponseItem>(options);
					case "update": return property.Value.Deserialize<BulkUpdateResponseItem>(options);
					case "index": return property.Value.Deserialize<BulkIndexResponseItem>(options);
					case "create": return property.Value.Deserialize<BulkCreateResponseItem>(options);
					default: return null;
				}
			}

			return null;
		}

		public override void Write(Utf8JsonWriter writer, BulkResponseItemBase value, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}
}
