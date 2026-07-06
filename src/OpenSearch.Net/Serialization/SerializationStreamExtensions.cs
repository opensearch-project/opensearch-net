/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSearch.Net
{
	/// <summary>
	/// Stream helpers shared by the <c>System.Text.Json</c> serializer and the custom multi-response
	/// builders (#388): read a stream fully into a byte array, fast-pathing an existing
	/// <see cref="MemoryStream"/>.
	/// </summary>
	internal static class SerializationStreamExtensions
	{
		private const int CopyBufferSize = 81920;

		public static byte[] ReadAllBytes(this Stream stream)
		{
			if (stream is MemoryStream existing) return existing.ToArray();
			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			return ms.ToArray();
		}

		public static async Task<byte[]> ReadAllBytesAsync(this Stream stream, CancellationToken cancellationToken)
		{
			if (stream is MemoryStream existing) return existing.ToArray();
			using var ms = new MemoryStream();
			await stream.CopyToAsync(ms, CopyBufferSize, cancellationToken).ConfigureAwait(false);
			return ms.ToArray();
		}
	}
}
