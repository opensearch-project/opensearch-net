/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// Shared <see cref="System.Text.Json"/> write helper for the <c>tdigest</c>/<c>hdr</c> percentiles
	/// method object, used by both the percentiles and percentile-ranks converters (#388).
	/// </summary>
	internal static class PercentilesMethodConverter
	{
		public static void Write(Utf8JsonWriter writer, IPercentilesMethod method)
		{
			switch (method)
			{
				case ITDigestMethod tdigest:
					writer.WritePropertyName("tdigest");
					writer.WriteStartObject();
					if (tdigest.Compression.HasValue)
						writer.WriteNumber("compression", tdigest.Compression.Value);
					writer.WriteEndObject();
					break;
				case IHDRHistogramMethod hdr:
					writer.WritePropertyName("hdr");
					writer.WriteStartObject();
					if (hdr.NumberOfSignificantValueDigits.HasValue)
						writer.WriteNumber("number_of_significant_value_digits", hdr.NumberOfSignificantValueDigits.Value);
					writer.WriteEndObject();
					break;
			}
		}
	}
}
