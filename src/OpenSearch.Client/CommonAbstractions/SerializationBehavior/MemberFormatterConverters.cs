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
		private static readonly Dictionary<Type, Func<Type, IConnectionSettingsValues, JsonConverter>> Map =
			new Dictionary<Type, Func<Type, IConnectionSettingsValues, JsonConverter>>
			{
				// string <-> number coercions
				{ typeof(NullableStringBooleanFormatter), (_, __) => new NullableStringBooleanConverter() },
				{ typeof(NullableStringIntFormatter), (_, __) => new NetConverters.NullableStringIntConverter() },
				{ typeof(NullableStringLongFormatter), (_, __) => new NullableStringLongConverter() },
				{ typeof(NullableStringDoubleFormatter), (_, __) => new NullableStringDoubleConverter() },
				{ typeof(StringLongFormatter), (_, __) => new StringLongConverter() },
				{ typeof(StringIntFormatter), (_, __) => new StringIntConverter() },
				{ typeof(IntStringFormatter), (_, __) => new IntStringConverter() },

				// epoch / ticks date-time encodings (member-specific; the ISO 8601 form is the type-level default)
				{ typeof(DateTimeOffsetEpochMillisecondsFormatter), (_, __) => new DateTimeOffsetEpochMillisecondsConverter() },
				{ typeof(NullableDateTimeOffsetEpochMillisecondsFormatter), (_, __) => new NullableDateTimeOffsetEpochMillisecondsConverter() },
				{ typeof(NullableDateTimeEpochMillisecondsFormatter), (_, __) => new NullableDateTimeEpochMillisecondsConverter() },
				{ typeof(TimeSpanTicksFormatter), (_, __) => new TimeSpanTicksConverter() },
				{ typeof(NullableTimeSpanTicksFormatter), (_, __) => new NullableTimeSpanTicksConverter() },

				// indices_boost: serializes as an array of single-key objects [{index:boost}] and reads both the array
				// and object forms. Settings-aware (resolves index names through the Inferrer).
				{ typeof(IndicesBoostFormatter), (_, settings) => new IndicesBoostConverter(settings) },

				// A member marked [JsonFormatter(typeof(IndicesFormatter))] (e.g. alias add/remove "indices") always
				// serializes as an ARRAY, overriding the IndicesMultiSyntax type-level default (which renders a single
				// index as a bare string).
				{ typeof(IndicesFormatter), (_, settings) => new IndicesConverter(settings) },
			};

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

			var formatterType = GetFormatterType(member);
			if (formatterType == null)
				return null;

			var key = formatterType.IsGenericType ? formatterType.GetGenericTypeDefinition() : formatterType;
			return Map.TryGetValue(key, out var factory) ? factory(formatterType, settings) : null;
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
