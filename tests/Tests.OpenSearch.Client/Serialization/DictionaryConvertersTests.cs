/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Unit tests for <see cref="VerbatimDictionaryKeysConverter{TDictionary,TInterface,TKey,TValue}"/> and
	/// <see cref="AggregateDictionaryConverter"/>, the System.Text.Json replacements for the legacy Utf8Json
	/// <c>VerbatimDictionaryKeysFormatter</c> and <c>AggregateDictionaryFormatter</c>.
	/// </summary>
	public class DictionaryConvertersTests
	{
		// A concrete string-keyed IIsADictionary used to exercise the verbatim converter without depending on
		// production dictionary types (which carry their own attributes/converters).
		public interface ITestDictionary : IIsADictionary<string, string> { }

		public class TestDictionary : IsADictionaryBase<string, string>, ITestDictionary
		{
			public TestDictionary() { }
			public TestDictionary(IDictionary<string, string> backing) : base(backing) { }
		}

		private static JsonSerializerOptions VerbatimOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new VerbatimDictionaryKeysConverter<TestDictionary, ITestDictionary, string, string>());
			return options;
		}

		private static JsonSerializerOptions PreservingNullOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(
				new VerbatimDictionaryKeysPreservingNullConverter<TestDictionary, ITestDictionary, string, string>());
			return options;
		}

		private static string Serialize(ITestDictionary value, JsonSerializerOptions options)
		{
			using var ms = new MemoryStream();
			using (var writer = new Utf8JsonWriter(ms))
				JsonSerializer.Serialize(writer, value, options);
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		private static ITestDictionary Deserialize(string json, JsonSerializerOptions options) =>
			JsonSerializer.Deserialize<ITestDictionary>(Encoding.UTF8.GetBytes(json), options);

		// ---- VerbatimDictionaryKeysConverter ----

		[U] public void Verbatim_WritesEmptyObject()
		{
			var value = new TestDictionary();
			Serialize(value, VerbatimOptions()).Should().Be("{}");
		}

		[U] public void Verbatim_WritesMultipleEntries()
		{
			var value = new TestDictionary { ["a"] = "1", ["b"] = "2" };
			var json = Serialize(value, VerbatimOptions());
			json.Should().Contain("\"a\":\"1\"").And.Contain("\"b\":\"2\"");
			json.Should().StartWith("{").And.EndWith("}");
		}

		[U] public void Verbatim_SkipsNullValuesByDefault()
		{
			var value = new TestDictionary { ["a"] = "1", ["b"] = null };
			var json = Serialize(value, VerbatimOptions());
			json.Should().Contain("\"a\":\"1\"").And.NotContain("\"b\"");
		}

		[U] public void Verbatim_WritesNullForNullDictionary() =>
			Serialize(null, VerbatimOptions()).Should().Be("null");

		[U] public void Verbatim_PreservingNull_WritesNullValues()
		{
			var value = new TestDictionary { ["a"] = "1", ["b"] = null };
			var json = Serialize(value, PreservingNullOptions());
			json.Should().Contain("\"a\":\"1\"").And.Contain("\"b\":null");
		}

		[U] public void Verbatim_ReadsEmptyObject()
		{
			var result = Deserialize("{}", VerbatimOptions());
			result.Should().BeOfType<TestDictionary>();
			result.Count.Should().Be(0);
		}

		[U] public void Verbatim_ReadsMultipleEntries()
		{
			var result = Deserialize("{\"a\":\"1\",\"b\":\"2\"}", VerbatimOptions());
			result.Count.Should().Be(2);
			result["a"].Should().Be("1");
			result["b"].Should().Be("2");
		}

		[U] public void Verbatim_ReadsTopLevelNullAsNull()
		{
			// System.Text.Json short-circuits a top-level JSON null for reference types before invoking the
			// converter, so the result is null (standard STJ semantics shared by the other migrated converters).
			var result = Deserialize("null", VerbatimOptions());
			result.Should().BeNull();
		}

		[U] public void Verbatim_RoundTrips()
		{
			var options = VerbatimOptions();
			var value = new TestDictionary { ["x"] = "10", ["y"] = "20" };
			var back = Deserialize(Serialize(value, options), options);
			back["x"].Should().Be("10");
			back["y"].Should().Be("20");
		}

		// ---- AggregateDictionaryConverter ----

		// Minimal IAggregate converter so the dictionary-level converter can be tested independently of the
		// (not-yet-migrated) full AggregateFormatter. Reads {"value": <number>} into a ValueAggregate.
		private class StubAggregateConverter : JsonConverter<IAggregate>
		{
			public override IAggregate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var agg = new ValueAggregate();
				if (reader.TokenType != JsonTokenType.StartObject)
				{
					reader.Skip();
					return agg;
				}

				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndObject)
						break;

					var name = reader.GetString();
					reader.Read();
					if (name == "value" && reader.TokenType == JsonTokenType.Number)
						agg.Value = reader.GetDouble();
					else
						reader.Skip();
				}

				return agg;
			}

			public override void Write(Utf8JsonWriter writer, IAggregate value, JsonSerializerOptions options) =>
				throw new NotSupportedException();
		}

		private static AggregateDictionary DeserializeAggregates(string json)
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new StubAggregateConverter());
			options.Converters.Add(new AggregateDictionaryConverter());
			return JsonSerializer.Deserialize<AggregateDictionary>(Encoding.UTF8.GetBytes(json), options);
		}

		[U] public void Aggregates_ReadsEmptyObject()
		{
			var result = DeserializeAggregates("{}");
			result.Count.Should().Be(0);
		}

		[U] public void Aggregates_NonObjectReturnsEmpty()
		{
			var result = DeserializeAggregates("123");
			result.Count.Should().Be(0);
		}

		[U] public void Aggregates_ReadsMultipleEntries()
		{
			var result = DeserializeAggregates("{\"min_price\":{\"value\":10},\"max_price\":{\"value\":99}}");
			result.Count.Should().Be(2);
			(result["min_price"] as ValueAggregate)!.Value.Should().Be(10);
			(result["max_price"] as ValueAggregate)!.Value.Should().Be(99);
		}

		[U] public void Aggregates_StripsTypedKeyPrefix()
		{
			// typed_keys=true responses return keys as "<type>#<name>"; dictionary should be keyed by the name.
			var result = DeserializeAggregates("{\"avg#my_agg\":{\"value\":42}}");
			result.Count.Should().Be(1);
			(result["my_agg"] as ValueAggregate)!.Value.Should().Be(42);
		}

		[U] public void Aggregates_NestedValueObjectIsParsed()
		{
			var result = DeserializeAggregates("{\"outer\":{\"value\":5,\"meta\":{\"k\":\"v\"}}}");
			(result["outer"] as ValueAggregate)!.Value.Should().Be(5);
		}
	}
}
