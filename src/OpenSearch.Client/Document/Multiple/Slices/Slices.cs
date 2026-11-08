/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(SlicesFormatter))]
public class Slices : Union<long, string>
{
	public Slices(long value) : base(value) { }

	public Slices(string value) : base(value) { }

	public static implicit operator Slices(long value) => new(value);
	public static implicit operator Slices(long? value) => value is { } v ? new Slices(v) : null;
	public static implicit operator Slices(string value) => value is { } v ? new Slices(value) : null;

	public override string ToString() => Tag switch
	{
		0 => Item1.ToString(),
		1 => Item2,
		_ => null
	};
}
