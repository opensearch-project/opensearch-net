/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Runtime.Serialization;
using System.Text;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce
{
	// Regression protection for the System.Text.Json contract resolver (#388):
	// SystemTextJsonSerializer must honor the client's existing
	// System.Runtime.Serialization attributes (DataMember/IgnoreDataMember/DataContract)
	// so wire property names match without re-annotating models.
	public class DataContractResolverTests
	{
		public class Doc
		{
			[DataMember(Name = "user_name")] public string UserName { get; set; }
			public int Count { get; set; }
			[IgnoreDataMember] public string Secret { get; set; }
		}

		[DataContract]
		public class OptIn
		{
			[DataMember(Name = "kept")] public string Kept { get; set; }
			public string Dropped { get; set; }
		}

		private static string Serialize<T>(T value)
		{
			IOpenSearchSerializer serializer = new SystemTextJsonSerializer();
			using var ms = new MemoryStream();
			serializer.Serialize(value, ms);
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		[U]
		public void HonorsDataMemberNameAndIgnoreDataMember() =>
			Serialize(new Doc { UserName = "bob", Count = 3, Secret = "x" })
				.Should().Be("{\"user_name\":\"bob\",\"Count\":3}");

		[U]
		public void DataContractIsOptIn() =>
			Serialize(new OptIn { Kept = "a", Dropped = "b" })
				.Should().Be("{\"kept\":\"a\"}");

		[U]
		public void RoundtripsRenamedMember()
		{
			IOpenSearchSerializer serializer = new SystemTextJsonSerializer();
			using var input = new MemoryStream(Encoding.UTF8.GetBytes("{\"user_name\":\"alice\",\"Count\":9}"));

			var doc = serializer.Deserialize<Doc>(input);

			doc.UserName.Should().Be("alice");
			doc.Count.Should().Be(9);
		}
	}
}
