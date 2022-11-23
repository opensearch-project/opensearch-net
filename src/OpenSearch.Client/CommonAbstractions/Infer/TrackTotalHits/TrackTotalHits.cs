/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Diagnostics;
using System.Globalization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	[JsonFormatter(typeof(TrackTotalHitsFormatter))]
	[DebuggerDisplay("{DebugDisplay,nq}")]
	public class TrackTotalHits : IEquatable<TrackTotalHits>, IUrlParameter
	{
		public TrackTotalHits(bool trackTotalHits)
		{
			Tag = 0;
			BoolValue = trackTotalHits;
		}

		public TrackTotalHits(long trackTotalHitsUpTo)
		{
			Tag = 1;
			LongValue = trackTotalHitsUpTo;
		}

		public bool Equals(TrackTotalHits other)
		{
			if (Tag != other?.Tag) return false;

			return Tag switch
			{
				0 => BoolValue == other.BoolValue,
				1 => LongValue == other.LongValue,
				_ => false
			};
		}

		private byte Tag { get; }

		internal bool? BoolValue { get; }

		internal long? LongValue { get; }

		private string BoolOrLongValue => BoolValue?.ToString(CultureInfo.InvariantCulture) ?? LongValue?.ToString(CultureInfo.InvariantCulture);

		private string DebugDisplay => BoolOrLongValue;

		public override string ToString() => BoolOrLongValue;

		public string GetString(IConnectionConfigurationValues settings) => BoolOrLongValue;

		public static implicit operator TrackTotalHits(bool trackTotalHits) => new(trackTotalHits);
		public static implicit operator TrackTotalHits(bool? trackTotalHits) => trackTotalHits is {} b ? new TrackTotalHits(b) : null;

		public static implicit operator TrackTotalHits(long trackTotalHitsUpTo) => new(trackTotalHitsUpTo);
		public static implicit operator TrackTotalHits(long? trackTotalHitsUpTo) => trackTotalHitsUpTo is {} l ? new TrackTotalHits(l) : null;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;

			return obj switch
			{
				TrackTotalHits t => Equals(t),
				bool b => Equals(b),
				long l => Equals(l),
				_ => false
			};
		}

		private static int TypeHashCode { get; } = typeof(TrackTotalHits).GetHashCode();

		public override int GetHashCode()
		{
			unchecked
			{
				var result = TypeHashCode;
				result = (result * 397) ^ (BoolValue?.GetHashCode() ?? 0);
				result = (result * 397) ^ (LongValue?.GetHashCode() ?? 0);
				return result;
			}
		}

		public static bool operator ==(TrackTotalHits left, TrackTotalHits right) => Equals(left, right);

		public static bool operator !=(TrackTotalHits left, TrackTotalHits right) => !Equals(left, right);
	}
}
