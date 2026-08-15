/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the request-stateful <see cref="MultiGetResponseConverter"/> (the System.Text.Json
	/// replacement for the legacy Utf8Json <c>MultiGetResponseFormatter</c>, which was declared in the file named
	/// <c>MultiGetHitJsonConverter.cs</c>).
	///
	/// The <c>docs</c> array carries no per-document type discriminator, so the converter recovers each hit's concrete
	/// document type by positionally zipping the array against the originating request's
	/// <see cref="IMultiGetRequest.Documents"/> and deserializing each hit as <c>MultiGetHit&lt;operation.ClrType&gt;</c>.
	/// The converter is therefore constructed with the request (mirroring the legacy per-request
	/// <c>CreateStateful</c> installation), and these tests instantiate it directly with a hand-built request.
	///
	/// Assertions target the dispatch outcome (hit count + concrete <c>MultiGetHit&lt;T&gt;</c> type) which is the
	/// converter's responsibility; the hit metadata members have <c>internal set</c> accessors not populated by the
	/// shared contract resolver.
	/// </summary>
	public class MultiGetResponseConverterTests
	{
		private class DocA { public string Name { get; set; } }
		private class DocB { public string Name { get; set; } }

		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			return new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
		}

		private static IMultiGetRequest Request(params System.Type[] clrTypes)
		{
			var ops = clrTypes.Select<System.Type, IMultiGetOperation>(t =>
				t == typeof(DocA)
					? new MultiGetOperation<DocA>("1")
					: new MultiGetOperation<DocB>("1")).ToList();
			return new MultiGetRequest { Documents = ops };
		}

		private static MultiGetResponse Deserialize(string json, IMultiGetRequest request)
		{
			var options = Options();
			var converter = new MultiGetResponseConverter(request);
			var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
			reader.Read();
			return converter.Read(ref reader, typeof(MultiGetResponse), options);
		}

		[U] public void Read_SingleHit_DispatchesConcreteDocumentType()
		{
			var response = Deserialize(
				@"{""docs"":[{""_index"":""i"",""_id"":""1"",""found"":true,""_source"":{""name"":""a""}}]}",
				Request(typeof(DocA)));

			response.InternalHits.Should().HaveCount(1);
			response.InternalHits.Single().Should().BeOfType<MultiGetHit<DocA>>();
		}

		[U] public void Read_MultipleHits_ZippedPositionallyByType()
		{
			var response = Deserialize(
				@"{""docs"":[{""_id"":""1"",""found"":true},{""_id"":""2"",""found"":true}]}",
				Request(typeof(DocA), typeof(DocB)));

			response.InternalHits.Should().HaveCount(2);
			response.InternalHits.First().Should().BeOfType<MultiGetHit<DocA>>();
			response.InternalHits.Last().Should().BeOfType<MultiGetHit<DocB>>();
		}

		[U] public void Read_NullRequest_ReturnsNull()
		{
			// Legacy returned null when the formatter had no request.
			Deserialize(@"{""docs"":[]}", null).Should().BeNull();
		}

		[U] public void Read_NoDocsProperty_ReturnsEmptyResponse()
		{
			var response = Deserialize(@"{""other"":123}", Request(typeof(DocA)));
			response.Should().NotBeNull();
			response.InternalHits.Should().BeEmpty();
		}

		[U] public void Read_EmptyDocsArray_ReturnsEmptyResponse()
		{
			var response = Deserialize(@"{""docs"":[]}", Request(typeof(DocA)));
			response.InternalHits.Should().BeEmpty();
		}

		[U] public void Read_Null_ReturnsEmptyResponse()
		{
			var response = Deserialize("null", Request(typeof(DocA)));
			response.Should().NotBeNull();
			response.InternalHits.Should().BeEmpty();
		}

		[U] public void Read_MoreDocsThanRequestDocuments_DropsExtras()
		{
			// Zip stops at the shorter sequence: one request document => only the first hit is taken.
			var response = Deserialize(
				@"{""docs"":[{""_id"":""1""},{""_id"":""2""}]}",
				Request(typeof(DocA)));
			response.InternalHits.Should().HaveCount(1);
			response.InternalHits.Single().Should().BeOfType<MultiGetHit<DocA>>();
		}

		[U] public void RoundTrip_PreservesHitConcreteTypes()
		{
			var options = Options();
			var request = Request(typeof(DocA), typeof(DocB));
			var response = Deserialize(
				@"{""docs"":[{""_id"":""1""},{""_id"":""2""}]}", request);

			var buffer = new System.Buffers.ArrayBufferWriter<byte>();
			using (var writer = new Utf8JsonWriter(buffer))
				new MultiGetResponseConverter(request).Write(writer, response, options);

			var json = System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan);
			json.Should().Contain("\"docs\"");

			var back = Deserialize(json, request);
			back.InternalHits.Should().HaveCount(2);
			back.InternalHits.First().Should().BeOfType<MultiGetHit<DocA>>();
			back.InternalHits.Last().Should().BeOfType<MultiGetHit<DocB>>();
		}
	}
}
