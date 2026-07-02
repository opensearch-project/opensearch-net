/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce
{
	// Regression protection for the System.Text.Json migration infrastructure (#388) exercised by
	// the first fully-integrated namespace (analysis tokenizers): the client declares its
	// [DataMember]/[StringEnum] attributes on interfaces, and the wire format uses minimal escaping.
	// These use self-contained types that reproduce that model pattern with public APIs only.
	public class SystemTextJsonAttributeTests
	{
		[StringEnum]
		public enum Category
		{
			[EnumMember(Value = "letter")] Letter,
			[EnumMember(Value = "white_space")] WhiteSpace,
		}

		// Attributes live on the interface; the concrete class implements them implicitly with none.
		public interface IThing
		{
			[DataMember(Name = "type")] string Type { get; }
			[DataMember(Name = "max_token_length")] int? MaxTokenLength { get; set; }
			[DataMember(Name = "pattern")] string Pattern { get; set; }
			[DataMember(Name = "token_chars")] IEnumerable<Category> Categories { get; set; }
		}

		public abstract class ThingBase : IThing
		{
			public string Type { get; protected set; }
			public int? MaxTokenLength { get; set; }
			public string Pattern { get; set; }
			public IEnumerable<Category> Categories { get; set; }
		}

		public class Thing : ThingBase
		{
			public Thing() => Type = "thing";
		}

		public class OtherThing : ThingBase
		{
			public OtherThing() => Type = "other";
		}

		// A concrete family converter, exactly as a per-namespace converter would look.
		private sealed class ThingConverter : PolymorphicInterfaceConverter<IThing>
		{
			public ThingConverter() : base(new Dictionary<string, Type>(StringComparer.Ordinal)
			{
				{ "thing", typeof(Thing) },
				{ "other", typeof(OtherThing) },
			}) { }
		}

		// A family converter with a fallback rule (as analyzers use for custom vs. language).
		private sealed class FallbackThingConverter : PolymorphicInterfaceConverter<IThing>
		{
			public FallbackThingConverter() : base(new Dictionary<string, Type>(StringComparer.Ordinal)
			{
				{ "thing", typeof(Thing) },
			}) { }

			protected override Type ResolveType(string discriminator, JsonElement document) =>
				base.ResolveType(discriminator, document)
				?? (document.TryGetProperty("pattern", out _) ? typeof(OtherThing) : typeof(Thing));
		}

		// A [DataMember] property whose only setter is non-public and whose value is data-driven
		// (not fixed by the constructor), mirroring LanguageAnalyzer.Type.
		public class ProtectedSetterDoc
		{
			[DataMember(Name = "type")] public string Type { get; protected set; }
			[DataMember(Name = "n")] public int Number { get; set; }
		}

		// Only a parameterized constructor whose parameter does not bind to a property, mirroring
		// InlineScript(string script) (property is Source). STJ cannot construct this on its own.
		public class ConstructorOnlyDoc
		{
			public ConstructorOnlyDoc(string unbound) => Ignored = unbound;

			[IgnoreDataMember] public string Ignored { get; }

			[DataMember(Name = "a")] public string A { get; set; }
		}

		private static string Serialize<T>(T value, bool withEnum = false)
		{
			IOpenSearchSerializer serializer;
			if (withEnum)
			{
				var options = new JsonSerializerOptions
				{
					// Fully qualified to disambiguate from System.Runtime.Serialization.DataContractResolver.
					TypeInfoResolver = OpenSearch.Net.DataContractResolver.Instance,
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				};
				options.Converters.Add(StringEnumConverterFactory.Instance);
				serializer = new SystemTextJsonSerializer(options);
			}
			else
			{
				serializer = new SystemTextJsonSerializer();
			}

			using var ms = new MemoryStream();
			serializer.Serialize(value, ms);
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		[U]
		public void HonorsDataMemberNamesDeclaredOnInterface()
		{
			var json = Serialize(new Thing { MaxTokenLength = 5, Pattern = "a" });

			json.Should().Contain("\"type\":\"thing\"")
				.And.Contain("\"max_token_length\":5")
				.And.Contain("\"pattern\":\"a\"");
		}

		[U]
		public void UsesMinimalEscapingToMatchUtf8Json()
		{
			// Utf8Json writes '+' literally; STJ's default HTML-safe encoder would emit \u002B.
			var json = Serialize(new Thing { Pattern = "\\W+" });

			json.Should().Contain("\"pattern\":\"\\\\W+\"");
			json.Should().NotContain("u002B");
		}

		[U]
		public void SerializesStringEnumMembersByEnumMemberValue()
		{
			var json = Serialize(
				new Thing { Categories = new[] { Category.Letter, Category.WhiteSpace } },
				withEnum: true);

			json.Should().Contain("\"token_chars\":[\"letter\",\"white_space\"]");
		}

		[U]
		public void RoundtripsInterfaceDeclaredNames()
		{
			IOpenSearchSerializer serializer = new SystemTextJsonSerializer();
			using var input = new MemoryStream(Encoding.UTF8.GetBytes("{\"type\":\"thing\",\"max_token_length\":7,\"pattern\":\"a\"}"));

			var thing = serializer.Deserialize<Thing>(input);

			thing.MaxTokenLength.Should().Be(7);
			thing.Pattern.Should().Be("a");
		}

		[U]
		public void PolymorphicConverterDispatchesOnDiscriminator()
		{
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = OpenSearch.Net.DataContractResolver.Instance,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			};
			options.Converters.Add(new ThingConverter());

			// Write: declared type is the interface; the concrete type's discriminator is emitted.
			IThing thing = new OtherThing { MaxTokenLength = 3 };
			var json = JsonSerializer.Serialize(thing, options);
			json.Should().Contain("\"type\":\"other\"").And.Contain("\"max_token_length\":3");

			// Read: the discriminator selects the concrete type.
			var roundTripped = JsonSerializer.Deserialize<IThing>(json, options);
			roundTripped.Should().BeOfType<OtherThing>();
			roundTripped.MaxTokenLength.Should().Be(3);
		}

		[U]
		public void WritesNonPublicSetterForDataMemberOnDeserialize()
		{
			IOpenSearchSerializer serializer = new SystemTextJsonSerializer();
			using var input = new MemoryStream(Encoding.UTF8.GetBytes("{\"type\":\"english\",\"n\":5}"));

			var doc = serializer.Deserialize<ProtectedSetterDoc>(input);

			doc.Type.Should().Be("english"); // protected setter wired because [DataMember] is present
			doc.Number.Should().Be(5);
		}

		[U]
		public void DeserializesTypeWithoutParameterlessConstructor()
		{
			IOpenSearchSerializer serializer = new SystemTextJsonSerializer();
			using var input = new MemoryStream(Encoding.UTF8.GetBytes("{\"a\":\"x\"}"));

			var doc = serializer.Deserialize<ConstructorOnlyDoc>(input);

			doc.A.Should().Be("x");
		}

		[U]
		public void FormatsIntegralDoublesWithTrailingDecimalLikeUtf8Json()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(DoubleFormatConverter.Instance);
			options.Converters.Add(SingleFormatConverter.Instance);

			JsonSerializer.Serialize(2.0, options).Should().Be("2.0");
			JsonSerializer.Serialize(100.0, options).Should().Be("100.0");
			JsonSerializer.Serialize(1.5, options).Should().Be("1.5");
			JsonSerializer.Serialize(0.1, options).Should().Be("0.1");
			// nullable picks up the same converter via STJ's built-in wrapping
			JsonSerializer.Serialize((double?)3.0, options).Should().Be("3.0");
		}

		public class ShouldSerializeDoc
		{
			[DataMember(Name = "kept")] public string Kept { get; set; }
			[DataMember(Name = "maybe")] public string Maybe { get; set; }
			public bool ShouldSerializeMaybe() => false;
		}

		[U]
		public void HonorsShouldSerializeConvention()
		{
			var json = Serialize(new ShouldSerializeDoc { Kept = "a", Maybe = "b" });
			json.Should().Contain("\"kept\":\"a\"").And.NotContain("maybe");
		}

		[U]
		public void DeserializesObjectToUtf8JsonClrShapes()
		{
			IOpenSearchSerializer serializer = new SystemTextJsonSerializer();

			object Read(string json)
			{
				using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
				return serializer.Deserialize<object>(ms);
			}

			Read("5").Should().BeOfType<long>().And.Be(5L);
			Read("1.5").Should().BeOfType<double>();
			Read("\"hi\"").Should().BeOfType<string>();
			Read("true").Should().BeOfType<bool>();
			Read("[1,2]").Should().BeOfType<List<object>>();

			var dict = Read("{\"a\":1,\"b\":[true]}").Should().BeOfType<Dictionary<string, object>>().Subject;
			dict["a"].Should().BeOfType<long>();
			dict["b"].Should().BeOfType<List<object>>();
		}

		[U]
		public void PolymorphicConverterUsesFallbackWhenDiscriminatorUnknown()
		{
			var options = new JsonSerializerOptions { TypeInfoResolver = OpenSearch.Net.DataContractResolver.Instance };
			options.Converters.Add(new FallbackThingConverter());

			JsonSerializer.Deserialize<IThing>("{\"type\":\"zzz\",\"pattern\":\"x\"}", options)
				.Should().BeOfType<OtherThing>();
			JsonSerializer.Deserialize<IThing>("{\"type\":\"zzz\"}", options)
				.Should().BeOfType<Thing>();
		}
	}
}
