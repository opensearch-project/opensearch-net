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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net.Extensions;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>QueryContainerFormatter</c> /
	/// <c>QueryContainerInterfaceFormatter</c>.
	///
	/// <see cref="QueryContainer"/> exposes ~80 query-type properties (<c>bool</c>, <c>match</c>, <c>term</c>,
	/// <c>range</c>, <c>nested</c>, …), at most one of which is non-null. The legacy engine used
	/// <c>DynamicObjectResolver</c> reflection to write the single set property (by its <see cref="DataMemberAttribute"/>
	/// wire name) and, on read, to read the single key and populate the matching property.
	///
	/// A plain <c>JsonSerializer</c> delegation is NOT possible here: every <see cref="IQueryContainer"/> member is
	/// implemented on <see cref="QueryContainer"/> as an <em>explicit interface implementation</em>, so
	/// <see cref="System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver"/> discovers no public properties
	/// and would produce/consume an empty object. This converter therefore dispatches generically: it reflects the
	/// <c>[DataMember]</c> names declared on the <see cref="IQueryContainer"/> interface once, then writes/reads the
	/// single query through those, delegating each query <em>body</em> to <c>JsonSerializer</c> so the per-query
	/// converters already registered (<c>FieldNameQueryConverterFactory</c>, <c>RangeQueryConverter</c>,
	/// <c>ReadAsConverterFactory</c>, …) handle the individual wire shapes.
	///
	/// It also preserves the legacy passthroughs:
	/// <list type="bullet">
	/// <item>the raw-query passthrough on write (<see cref="IQueryContainer.RawQuery"/> + <c>IsWritable</c>) — a verbatim
	/// JSON string written directly;</item>
	/// <item>the string-shape read (a JSON <em>string</em> whose content is itself a query object);</item>
	/// <item>returning <c>null</c> for any other token shape.</item>
	/// </list>
	/// Because bodies are delegated through <c>options</c>, this converter needs no settings of its own.
	/// </summary>
	internal class QueryContainerConverter : JsonConverter<QueryContainer>
	{
		// The [DataMember(Name)] mapping declared on the IQueryContainer interface (e.g. "bool" -> IQueryContainer.Bool).
		// [IgnoreDataMember] members (RawQuery, IsWritable, …) carry no DataMemberAttribute name and are excluded.
		internal static readonly (string Name, PropertyInfo Property)[] QueryProperties =
			typeof(IQueryContainer)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Select(p => (Property: p, DataMember: p.GetCustomAttribute<DataMemberAttribute>(true)))
				.Where(x => x.DataMember != null && !string.IsNullOrEmpty(x.DataMember.Name))
				.Select(x => (x.DataMember.Name, x.Property))
				.ToArray();

		private static readonly Dictionary<string, PropertyInfo> NameToProperty =
			QueryProperties.ToDictionary(x => x.Name, x => x.Property);

		public override QueryContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// Utf8JsonReader is forward-only; buffer the value so we can inspect its shape (object / string / other).
			using var doc = JsonDocument.ParseValue(ref reader);
			return ReadElement(doc.RootElement, options);
		}

		// Shared read entry point used by the container, interface and collection converters. Mirrors the legacy
		// QueryContainerFormatter.Deserialize switch: object -> populate; string -> parse-then-populate; else null.
		internal static QueryContainer ReadElement(JsonElement element, JsonSerializerOptions options)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
					return ReadObject(element, options);
				case JsonValueKind.String:
					var raw = element.GetString();
					if (string.IsNullOrEmpty(raw))
						return null;
					using (var inner = JsonDocument.Parse(raw))
					{
						return inner.RootElement.ValueKind == JsonValueKind.Object
							? ReadObject(inner.RootElement, options)
							: null;
					}
				default:
					return null;
			}
		}

		private static QueryContainer ReadObject(JsonElement root, JsonSerializerOptions options)
		{
			var container = new QueryContainer();
			foreach (var member in root.EnumerateObject())
			{
				if (!NameToProperty.TryGetValue(member.Name, out var property))
					continue;

				// Delegate the body to the per-query converter registered for the property's interface type.
				var query = member.Value.Deserialize(property.PropertyType, options);
				if (query != null)
					property.SetValue(container, query);
			}

			return container;
		}

		public override void Write(Utf8JsonWriter writer, QueryContainer value, JsonSerializerOptions options) =>
			WriteContainer(writer, value, options);

		// Shared write entry point (legacy QueryContainerInterfaceFormatter.Serialize): raw-query passthrough first,
		// otherwise emit the single non-null query property as { "<wire name>": <body> }.
		internal static void WriteContainer(Utf8JsonWriter writer, IQueryContainer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var rawQuery = value.RawQuery;
			if ((!rawQuery?.Raw.IsNullOrEmpty() ?? false) && rawQuery.IsWritable)
			{
				writer.WriteRawValue(rawQuery.Raw);
				return;
			}

			writer.WriteStartObject();
			foreach (var (name, property) in QueryProperties)
			{
				var query = property.GetValue(value);
				if (query == null)
					continue;

				writer.WritePropertyName(name);
				// Delegate the body to the per-query converter for the declared interface type.
				JsonSerializer.Serialize(writer, query, property.PropertyType, options);
			}
			writer.WriteEndObject();
		}
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>QueryContainerInterfaceFormatter</c>. Reading delegates
	/// to <see cref="QueryContainerConverter"/> (produces a concrete <see cref="QueryContainer"/>); writing shares the
	/// same raw-passthrough + single-property dispatch logic.
	/// </summary>
	internal class QueryContainerInterfaceConverter : JsonConverter<IQueryContainer>
	{
		public override IQueryContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			return QueryContainerConverter.ReadElement(doc.RootElement, options);
		}

		public override void Write(Utf8JsonWriter writer, IQueryContainer value, JsonSerializerOptions options) =>
			QueryContainerConverter.WriteContainer(writer, value, options);
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>QueryContainerCollectionFormatter</c>. On read accepts an
	/// array (or a single object treated as a one-element list); on write emits an array, skipping null and
	/// non-<c>IsWritable</c> entries, mirroring the legacy collection formatter.
	/// </summary>
	internal class QueryContainerCollectionConverter : JsonConverter<IEnumerable<QueryContainer>>
	{
		public override IEnumerable<QueryContainer> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;
			switch (root.ValueKind)
			{
				case JsonValueKind.Array:
					var list = new List<QueryContainer>();
					foreach (var element in root.EnumerateArray())
						list.Add(QueryContainerConverter.ReadElement(element, options));
					return list;
				case JsonValueKind.Object:
					return new List<QueryContainer> { QueryContainerConverter.ReadElement(root, options) };
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, IEnumerable<QueryContainer> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var container in value)
			{
				if (container == null || !container.IsWritable)
					continue;

				QueryContainerConverter.WriteContainer(writer, container, options);
			}
			writer.WriteEndArray();
		}
	}
}
