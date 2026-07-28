/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// Tests for the low-level serializer's handling of object-typed dynamic request bodies (e.g.
	/// <c>Dictionary&lt;string, object&gt;</c> built from a parsed document and sent via <c>PostData.Serializable</c>).
	/// System.Text.Json writes such values through the declared <see cref="object"/> type, which would bypass the
	/// double formatting and emit an integral double like 3.0 as 3 — changing the value the server stores (the YAML
	/// runner's search.backpressure heap_variance and flat_object failures).
	/// </summary>
	public class ObjectConverterTests
	{
		private static readonly SystemTextJsonSerializer Serializer = new SystemTextJsonSerializer();

		[U] public void IntegralDouble_InDictionary_PreservesTrailingZero()
		{
			var body = new Dictionary<string, object>
			{
				{ "transient", new Dictionary<object, object> { { "heap_variance", (object)3.0 } } }
			};

			Serializer.SerializeToString(body).Should().Contain("\"heap_variance\":3.0");
		}

		[U] public void FractionalDouble_InDictionary_Preserved()
		{
			var body = new Dictionary<string, object> { { "n", (object)1.5 } };
			Serializer.SerializeToString(body).Should().Contain("\"n\":1.5");
		}

		[U] public void IntegralDouble_InNestedArray_PreservesTrailingZero()
		{
			var body = new Dictionary<string, object>
			{
				{ "review", new List<object> { new List<object> { "ok", 80.0 } } }
			};

			Serializer.SerializeToString(body).Should().Contain("80.0");
		}

		[U] public void Integer_InDictionary_StaysInteger()
		{
			var body = new Dictionary<string, object> { { "n", (object)50 } };
			Serializer.SerializeToString(body).Should().Contain("\"n\":50").And.NotContain("50.0");
		}

		[U] public void RoundTrips_ObjectValues_AsNativePrimitives()
		{
			const string json = @"{""d"":3.0,""i"":50,""s"":""x"",""b"":true}";
			var dict = Serializer.Deserialize<Dictionary<string, object>>(
				new MemoryStream(Encoding.UTF8.GetBytes(json)));

			dict["d"].Should().BeOfType(typeof(double));
			dict["i"].Should().BeOfType(typeof(long));
			dict["s"].Should().Be("x");
			dict["b"].Should().Be(true);
		}
	}
}
