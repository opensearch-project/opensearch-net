/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using OpenSearch.Net.Utf8Json;
using NetConverters = OpenSearch.Net.Serialization.Converters;

namespace OpenSearch.Client
{
	/// <summary>
	/// Maps a member's legacy Utf8Json <c>[JsonFormatter(typeof(XxxFormatter))]</c> to the migrated
	/// System.Text.Json converter, so <see cref="HighLevelContractResolver"/> can bind it as the property's
	/// <c>CustomConverter</c>.
	///
	/// Only <b>member-specific</b> formatters belong here — ones the legacy engine attached per member rather than
	/// as a type-level default. Type-level defaults (e.g. GeoShape, Time, TrackTotalHits, Union, the IsADictionary
	/// family) are already registered globally on the serializer options and must NOT be duplicated here, or the
	/// per-member binding would shadow / conflict with the global one. The classic member-level cases are the
	/// string/number coercions and the epoch/ticks date encodings, which deliberately are not global because the
	/// same CLR type (int?, DateTimeOffset, long, ...) is serialized differently depending on the member.
	/// </summary>
	internal static class MemberFormatterConverters
	{
		// Legacy formatter type (open generic definition for generic formatters) -> factory for the STJ converter.
		private static readonly Dictionary<Type, Func<Type, Type, IConnectionSettingsValues, JsonConverter>> Map =
			new Dictionary<Type, Func<Type, Type, IConnectionSettingsValues, JsonConverter>>
			{
				// string <-> number coercions
				{ typeof(NullableStringBooleanFormatter), (_, __, ___) => new NullableStringBooleanConverter() },
				{ typeof(NullableStringIntFormatter), (_, __, ___) => new NetConverters.NullableStringIntConverter() },
				{ typeof(NullableStringLongFormatter), (_, __, ___) => new NullableStringLongConverter() },
				{ typeof(NullableStringDoubleFormatter), (_, __, ___) => new NullableStringDoubleConverter() },
				{ typeof(StringLongFormatter), (_, __, ___) => new StringLongConverter() },
				{ typeof(StringIntFormatter), (_, __, ___) => new StringIntConverter() },
				{ typeof(IntStringFormatter), (_, __, ___) => new IntStringConverter() },

				// epoch / ticks date-time encodings (member-specific; the ISO 8601 form is the type-level default)
				{ typeof(DateTimeOffsetEpochMillisecondsFormatter), (_, __, ___) => new DateTimeOffsetEpochMillisecondsConverter() },
				{ typeof(NullableDateTimeOffsetEpochMillisecondsFormatter), (_, __, ___) => new NullableDateTimeOffsetEpochMillisecondsConverter() },
				{ typeof(NullableDateTimeEpochMillisecondsFormatter), (_, __, ___) => new NullableDateTimeEpochMillisecondsConverter() },
				{ typeof(TimeSpanTicksFormatter), (_, __, ___) => new TimeSpanTicksConverter() },
				{ typeof(NullableTimeSpanTicksFormatter), (_, __, ___) => new NullableTimeSpanTicksConverter() },

				// indices_boost: serializes as an array of single-key objects [{index:boost}] and reads both the array
				// and object forms. Settings-aware (resolves index names through the Inferrer).
				{ typeof(IndicesBoostFormatter), (_, __, settings) => new IndicesBoostConverter(settings) },

				// A member marked [JsonFormatter(typeof(IndicesFormatter))] (e.g. alias add/remove "indices") always
				// serializes as an ARRAY, overriding the IndicesMultiSyntax type-level default (which renders a single
				// index as a bare string).
				{ typeof(IndicesFormatter), (_, __, settings) => new IndicesConverter(settings) },

				// Document bodies ([JsonFormatter(typeof(SourceFormatter<>))] and the Collapsed/Write variants) are
				// (de)serialized through the connection's SourceSerializer, so a user-supplied source serializer
				// governs the shape. The formatter is a closed generic (SourceFormatter<T>); build SourceConverter<T>.
				{ typeof(SourceFormatter<>), (_, memberType, settings) => MakeSource(typeof(SourceConverter<>), memberType, settings) },
				{ typeof(CollapsedSourceFormatter<>), (_, memberType, settings) => MakeSource(typeof(CollapsedSourceConverter<>), memberType, settings) },
				{ typeof(SourceWriteFormatter<>), (_, memberType, settings) => MakeSource(typeof(SourceWriteConverter<>), memberType, settings) },

				// get-field-mapping "mapping" dictionary: keyed by field name, values are polymorphic IFieldMapping
				// (meta fields + property mappings). STJ cannot deserialize the abstract IFieldMapping on its own, so
				// this member-level converter reads the discriminating key and routes to the concrete type. Settings-aware
				// (resolves field-name keys through the Inferrer).
				{ typeof(FieldMappingFormatter), (_, __, settings) => new FieldMappingConverter(settings) },

				// resolvable read-only dictionaries keyed by an inferred key type (Field / IndexName): e.g.
				// TermVectorsResponse.TermVectors, ClusterHealthResponse.Indices, TypeFieldMappings.Mappings. The legacy
				// formatter wrapped the map in a ResolvableDictionaryProxy so lookups resolve through the inferrer; STJ's
				// default handling cannot build the inferred key type from a JSON property name, so bridge it. Close the
				// converter with the formatter's own <TKey,TValue>.
				{ typeof(ResolvableReadOnlyDictionaryFormatter<,>), (ft, _, settings) => MakeResolvableDict(ft, settings) },

				// single-or-array coercion: a member typed IEnumerable<T> that also accepts a bare scalar. Close the
				// converter with the FORMATTER's element type T (SingleOrEnumerableFormatter<T>), not the member type.
				{ typeof(SingleOrEnumerableFormatter<>), (ft, _, __) => MakeSingle(typeof(SingleOrEnumerableConverter<>), ft) },
				{ typeof(SerializeAsSingleFormatter<>), (ft, _, __) => MakeSingle(typeof(SerializeAsSingleConverter<>), ft) },
			};

		// Closes the open-generic single-or-enumerable converter with the formatter's element type argument.
		private static JsonConverter MakeSingle(Type openConverter, Type closedFormatter)
		{
			if (!closedFormatter.IsGenericType)
				return null;
			var arg = closedFormatter.GetGenericArguments()[0];
			return (JsonConverter)Activator.CreateInstance(openConverter.MakeGenericType(arg));
		}

		// Closes ResolvableReadOnlyDictionaryConverter<TKey,TValue> with the formatter's own <TKey,TValue> and
		// constructs it with the runtime settings.
		private static JsonConverter MakeResolvableDict(Type closedFormatter, IConnectionSettingsValues settings)
		{
			if (!closedFormatter.IsGenericType)
				return null;
			var args = closedFormatter.GetGenericArguments();
			var converterType = typeof(ResolvableReadOnlyDictionaryConverter<,>).MakeGenericType(args);
			return (JsonConverter)Activator.CreateInstance(converterType, settings);
		}

		// Closes the open-generic source converter with the formatter's own type argument (SourceFormatter<T> -> ...<T>)
		// and constructs it with the runtime settings.
		private static JsonConverter MakeSource(Type openConverter, Type memberType, IConnectionSettingsValues settings)
		{
			if (memberType == null)
				return null;
			var converterType = openConverter.MakeGenericType(memberType);
			return (JsonConverter)Activator.CreateInstance(converterType, settings);
		}

		/// <summary>
		/// Returns the migrated converter for the member's legacy <c>[JsonFormatter]</c>, or <c>null</c> when the
		/// member has no formatter attribute or it is not one of the member-specific formatters handled here.
		/// </summary>
		public static JsonConverter TryCreate(MemberInfo member, IConnectionSettingsValues settings)
		{
			// [StringTimeSpan] overrides the ticks-number type-level default with the string form. It is its own
			// attribute (not a [JsonFormatter]); handle it explicitly, picking the (non-)nullable variant.
			if (member.GetCustomAttribute<StringTimeSpanAttribute>(true) != null)
			{
				var t = (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo)?.FieldType;
				if (t == typeof(TimeSpan?))
					return new NullableStringTimeSpanConverter();
				if (t == typeof(TimeSpan))
					return new StringTimeSpanConverter();
			}

			// A member marked [StringEnum] serializes its enum as a string even when the enum type itself is not
			// [StringEnum]-marked (e.g. HttpStatusCode). Build the string-enum converter for the member's enum type.
			if (member.GetCustomAttribute<OpenSearch.Net.StringEnumAttribute>(true) != null)
			{
				var t = (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo)?.FieldType;
				var converter = NetConverters.StringEnumConverterFactory.CreateForType(t);
				if (converter != null)
					return converter;
			}

			var formatterType = GetFormatterType(member);
			if (formatterType == null)
				return null;

			var key = formatterType.IsGenericType ? formatterType.GetGenericTypeDefinition() : formatterType;
			if (!Map.TryGetValue(key, out var factory))
				return null;

			// For the source-body formatters the [JsonFormatter] may be the OPEN generic (SourceFormatter<>), whose
			// type argument is an unbound T; the factory closes its converter with the member's own declared type.
			var memberType = (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo)?.FieldType;
			return factory(formatterType, memberType, settings);
		}

		// Reads [JsonFormatter] from the member itself or, failing that, from the matching property on an interface
		// the declaring type implements (mirrors how the legacy engine discovered member attributes through
		// explicit-interface implementations).
		private static Type GetFormatterType(MemberInfo member)
		{
			var attr = member.GetCustomAttribute<JsonFormatterAttribute>(true);
			if (attr != null)
				return attr.FormatterType;

			if (member is PropertyInfo && member.DeclaringType != null)
			{
				foreach (var i in member.DeclaringType.GetInterfaces())
				{
					var ip = i.GetProperty(member.Name, BindingFlags.Public | BindingFlags.Instance);
					var ia = ip?.GetCustomAttribute<JsonFormatterAttribute>(true);
					if (ia != null)
						return ia.FormatterType;
				}
			}

			return null;
		}
	}
}
