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
using System.IO;
using System.Linq;
using System.Text;

namespace OpenSearch.Net.Extensions
{
	internal static class UtilExtensions
	{
		private const long MillisecondsInAWeek = MillisecondsInADay * 7;
		private const long MillisecondsInADay = MillisecondsInAnHour * 24;
		private const long MillisecondsInAnHour = MillisecondsInAMinute * 60;
		private const long MillisecondsInAMinute = MillisecondsInASecond * 60;
		private const long MillisecondsInASecond = 1000;

		internal static string Utf8String(this byte[] bytes) => bytes == null ? null : Encoding.UTF8.GetString(bytes, 0, bytes.Length);

		internal static string Utf8String(this MemoryStream ms)
		{
			if (ms is null)
				return null;

			if (!ms.TryGetBuffer(out var buffer) || buffer.Array is null)
				return ms.ToArray().Utf8String();

			return buffer.Utf8String();
		}

		internal static byte[] Utf8Bytes(this string s) => s.IsNullOrEmpty() ? null : Encoding.UTF8.GetBytes(s);

		internal static void ThrowIfEmpty<T>(this IEnumerable<T> @object, string parameterName)
		{
			var enumerated = @object == null ? null : (@object as T[] ?? @object.ToArray());
			enumerated.ThrowIfNull(parameterName);
			if (!enumerated!.Any())
				throw new ArgumentException("Argument can not be an empty collection", parameterName);
		}

		internal static bool HasAny<T>(this IEnumerable<T> list) => list != null && list.Any();

		internal static bool HasAny<T>(this IEnumerable<T> list, out T[] enumerated)
		{
			enumerated = list == null ? null : (list as T[] ?? list.ToArray());
			return enumerated.HasAny();
		}

		internal static Exception AsAggregateOrFirst(this IEnumerable<Exception> exceptions)
		{
			var opensearch = exceptions as Exception[] ?? exceptions?.ToArray();
			if (opensearch == null || opensearch.Length == 0) return null;

			return opensearch.Length == 1 ? opensearch[0] : new AggregateException(opensearch);
		}

		internal static void ThrowIfNull<T>(this T value, string name) where T : class
		{
			if (value == null)
				throw new ArgumentNullException(name);
		}

		internal static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

		internal static IEnumerable<T> DistinctByInternal<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property) =>
			items.GroupBy(property).Select(x => x.First());

		internal static string ToTimeUnit(this TimeSpan timeSpan)
		{
			var ms = timeSpan.TotalMilliseconds;
			string interval;
			double factor;

			if (ms >= MillisecondsInAWeek)
			{
				factor = ms / MillisecondsInAWeek;
				interval = "w";
			}
			else if (ms >= MillisecondsInADay)
			{
				factor = ms / MillisecondsInADay;
				interval = "d";
			}
			else if (ms >= MillisecondsInAnHour)
			{
				factor = ms / MillisecondsInAnHour;
				interval = "h";
			}
			else if (ms >= MillisecondsInAMinute)
			{
				factor = ms / MillisecondsInAMinute;
				interval = "m";
			}
			else if (ms >= MillisecondsInASecond)
			{
				factor = ms / MillisecondsInASecond;
				interval = "s";
			}
			else
			{
				factor = ms;
				interval = "ms";
			}

			return factor.ToString("0.##", CultureInfo.InvariantCulture) + interval;
		}
	}
}
