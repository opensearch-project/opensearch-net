/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(TrackTotalHitsFormatter))]
public class TrackTotalHits : Union<bool, long>
{
    public TrackTotalHits(bool item) : base(item) { }

    public TrackTotalHits(long item) : base(item) { }

    public static implicit operator TrackTotalHits(bool trackTotalHits) => new(trackTotalHits);
    public static implicit operator TrackTotalHits(bool? trackTotalHits) => trackTotalHits is { } b ? new TrackTotalHits(b) : null;

    public static implicit operator TrackTotalHits(long trackTotalHitsUpTo) => new(trackTotalHitsUpTo);
    public static implicit operator TrackTotalHits(long? trackTotalHitsUpTo) => trackTotalHitsUpTo is { } l ? new TrackTotalHits(l) : null;

    public override string ToString() => Tag switch
    {
        0 => Item1.ToString(),
        1 => Item2.ToString(),
        _ => null
    };
}
