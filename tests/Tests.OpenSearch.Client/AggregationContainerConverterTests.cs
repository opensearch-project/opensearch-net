/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="AggregationContainerConverter"/>, the System.Text.Json replacement for the
	/// legacy Utf8Json <c>AggregationDictionaryFormatter</c>. Reads/writes the request-side <c>aggs</c> object: a
	/// verbatim string-keyed dictionary of <see cref="IAggregationContainer"/>.
	///
	/// The individual <see cref="IAggregationContainer"/> value serialization is delegated to
	/// <see cref="JsonSerializer"/>; to keep these tests independent of the (attribute-driven) container contract, a
	/// small stub converter for <see cref="IAggregationContainer"/> is registered that reads/writes the single
	/// <c>terms.field</c> shape.
	/// </summary>
	public class AggregationContainerConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new StubAggregationContainerConverter());
			options.Converters.Add(new AggregationContainerConverter());
			return options;
		}

		private static string Serialize(AggregationDictionary value)
		{
			using var ms = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(ms))
				JsonSerializer.Serialize(writer, value, Options());
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		private static AggregationDictionary Deserialize(string json) =>
			JsonSerializer.Deserialize<AggregationDictionary>(Encoding.UTF8.GetBytes(json), Options());

		[U] public void Reads_MultipleNamedAggregations()
		{
			var dict = Deserialize(@"{""states"":{""terms"":{""field"":""state""}},""cities"":{""terms"":{""field"":""city""}}}");
			Count(dict).Should().Be(2);
			((AggregationContainer)dict["states"]).Terms.Field.Name.Should().Be("state");
			((AggregationContainer)dict["cities"]).Terms.Field.Name.Should().Be("city");
		}

		[U] public void Reads_EmptyObject()
		{
			var dict = Deserialize(@"{}");
			Count(dict).Should().Be(0);
		}

		[U] public void Writes_MultipleNamedAggregations()
		{
			var dict = new AggregationDictionary
			{
				{ "states", MakeTerms("state") },
				{ "cities", MakeTerms("city") }
			};
			var json = Serialize(dict);
			json.Should().Contain(@"""states"":").And.Contain(@"""cities"":");
			json.Should().Contain(@"""field"":""state""").And.Contain(@"""field"":""city""");
		}

		[U] public void Writes_Null_AsJsonNull() =>
			Serialize(null).Should().Be("null");

		[U] public void RoundTrips()
		{
			var options = Options();
			var dict = new AggregationDictionary { { "states", MakeTerms("state") } };
			var json = Serialize(dict);
			var back = JsonSerializer.Deserialize<AggregationDictionary>(Encoding.UTF8.GetBytes(json), options);
			Count(back).Should().Be(1);
			((AggregationContainer)back["states"]).Terms.Field.Name.Should().Be("state");
		}

		[U] public void ReservedAggregationName_Throws()
		{
			// Constructing the AggregationDictionary from the read entries runs its reserved-keyword validation.
			Action act = () => Deserialize(@"{""buckets"":{""terms"":{""field"":""state""}}}");
			act.Should().Throw<ArgumentException>();
		}

		private static int Count(AggregationDictionary dict) =>
			((System.Collections.Generic.IDictionary<string, IAggregationContainer>)dict).Count;

		private static AggregationContainer MakeTerms(string field) =>
			new AggregationContainer { Terms = new TermsAggregation("x") { Field = field } };

		// Minimal IAggregationContainer converter that reads/writes only { "terms": { "field": <string> } }.
		private class StubAggregationContainerConverter : JsonConverter<IAggregationContainer>
		{
			public override IAggregationContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var container = new AggregationContainer();
				if (reader.TokenType != JsonTokenType.StartObject)
				{
					reader.Skip();
					return container;
				}

				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndObject)
						break;

					var name = reader.GetString();
					reader.Read();
					if (name == "terms" && reader.TokenType == JsonTokenType.StartObject)
					{
						string field = null;
						while (reader.Read())
						{
							if (reader.TokenType == JsonTokenType.EndObject)
								break;
							var inner = reader.GetString();
							reader.Read();
							if (inner == "field")
								field = reader.GetString();
							else
								reader.Skip();
						}
						container.Terms = new TermsAggregation("x") { Field = field };
					}
					else
						reader.Skip();
				}

				return container;
			}

			public override void Write(Utf8JsonWriter writer, IAggregationContainer value, JsonSerializerOptions options)
			{
				writer.WriteStartObject();
				if (value.Terms?.Field != null)
				{
					writer.WritePropertyName("terms");
					writer.WriteStartObject();
					writer.WriteString("field", value.Terms.Field.Name);
					writer.WriteEndObject();
				}
				writer.WriteEndObject();
			}
		}
	}
}
