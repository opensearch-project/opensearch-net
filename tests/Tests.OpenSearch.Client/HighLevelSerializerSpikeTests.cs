/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// SPIKE validation for <see cref="SystemTextJsonHighLevelSerializer"/> — proves a System.Text.Json based
	/// high-level serializer can be driven by the runtime <see cref="IConnectionSettingsValues"/> configuration,
	/// which is the hardest capability of the legacy Utf8Json engine to reproduce.
	/// </summary>
	public class HighLevelSerializerSpikeTests
	{
		public class Doc
		{
			public string FirstName { get; set; }
			public int ItemCount { get; set; }
		}

		private static string Serialize<T>(IOpenSearchSerializer serializer, T value)
		{
			using var ms = new MemoryStream();
			serializer.Serialize(value, ms);
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		[U] public void UsesDefaultCamelCaseFieldInference()
		{
			var settings = new ConnectionSettings(); // default DefaultFieldNameInferrer = camelCase
			var serializer = new SystemTextJsonHighLevelSerializer(settings);

			var json = Serialize(serializer, new Doc { FirstName = "bob", ItemCount = 3 });

			json.Should().Contain(@"""firstName"":""bob""");
			json.Should().Contain(@"""itemCount"":3");
		}

		[U] public void HonoursCustomFieldNameInferrer()
		{
			// Prove the resolver is driven by runtime settings: switch inference to UPPER-case.
			var settings = new ConnectionSettings().DefaultFieldNameInferrer(f => f.ToUpperInvariant());
			var serializer = new SystemTextJsonHighLevelSerializer(settings);

			var json = Serialize(serializer, new Doc { FirstName = "bob", ItemCount = 3 });

			json.Should().Contain(@"""FIRSTNAME"":""bob""");
			json.Should().Contain(@"""ITEMCOUNT"":3");
		}

		[U] public void RoundTrips()
		{
			var settings = new ConnectionSettings();
			var serializer = new SystemTextJsonHighLevelSerializer(settings);

			var json = Serialize(serializer, new Doc { FirstName = "alice", ItemCount = 7 });
			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
			var back = serializer.Deserialize<Doc>(ms);

			back.FirstName.Should().Be("alice");
			back.ItemCount.Should().Be(7);
		}

		[U] public void ReadAs_DeserializesInterfaceAsConcreteType()
		{
			// ITextIndexPrefixes is marked [ReadAs(typeof(TextIndexPrefixes))]; deserializing the interface must
			// produce the concrete TextIndexPrefixes via ReadAsConverterFactory.
			var settings = new ConnectionSettings();
			var serializer = new SystemTextJsonHighLevelSerializer(settings);

			// Field names come from [DataMember(Name=...)] on the interface: min_chars / max_chars.
			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(@"{""min_chars"":2,""max_chars"":5}"));
			var result = serializer.Deserialize<ITextIndexPrefixes>(ms);

			result.Should().BeOfType<TextIndexPrefixes>();
			result.MinCharacters.Should().Be(2);
			result.MaxCharacters.Should().Be(5);
		}
	}
}
