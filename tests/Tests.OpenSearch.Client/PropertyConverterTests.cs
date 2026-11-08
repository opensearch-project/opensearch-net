/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="PropertyConverter"/> and <see cref="PropertiesConverter"/>: an
	/// <see cref="IProperty"/> is dispatched to a concrete mapping type by the <c>type</c> discriminator
	/// (<c>text</c>/<c>keyword</c>/<c>date</c>/<c>long</c>/<c>nested</c>, …), a mapping with no <c>type</c> but with a
	/// <c>properties</c> field falls back to an object mapping, and <see cref="IProperties"/> is a
	/// <see cref="PropertyName"/> → <see cref="IProperty"/> dictionary. Mirrors the legacy Utf8Json
	/// <c>PropertyFormatter</c> / <c>PropertiesFormatter</c>.
	/// </summary>
	public class PropertyConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new global::OpenSearch.Net.Serialization.Converters.StringEnumConverterFactory());
			options.Converters.Add(new PropertyNameConverter(settings));
			options.Converters.Add(new PropertyConverter());
			options.Converters.Add(new PropertiesConverter(settings));
			return options;
		}

		private static IProperty DeserializeProperty(string json) =>
			JsonSerializer.Deserialize<IProperty>(json, Options());

		private static IProperties DeserializeProperties(string json) =>
			JsonSerializer.Deserialize<IProperties>(json, Options());

		// --- Property dispatch ---

		[U] public void Deserialize_Text_DispatchesTextProperty()
		{
			var property = DeserializeProperty(@"{""type"":""text"",""analyzer"":""standard"",""boost"":2.0}");
			property.Should().BeOfType<TextProperty>();
			property.Type.Should().Be("text");
			((TextProperty)property).Analyzer.Should().Be("standard");
			((ITextProperty)property).Boost.Should().Be(2.0);
		}

		[U] public void Deserialize_Keyword_DispatchesKeywordProperty()
		{
			var property = DeserializeProperty(@"{""type"":""keyword"",""ignore_above"":256}");
			property.Should().BeOfType<KeywordProperty>();
			property.Type.Should().Be("keyword");
			((IKeywordProperty)property).IgnoreAbove.Should().Be(256);
		}

		[U] public void Deserialize_Date_DispatchesDateProperty()
		{
			var property = DeserializeProperty(@"{""type"":""date"",""format"":""yyyy-MM-dd""}");
			property.Should().BeOfType<DateProperty>();
			property.Type.Should().Be("date");
			((IDateProperty)property).Format.Should().Be("yyyy-MM-dd");
		}

		[U] public void Deserialize_Long_DispatchesNumberProperty_AndPreservesTypeString()
		{
			// All numeric field types map to NumberProperty; the exact type string must be preserved on the instance.
			var property = DeserializeProperty(@"{""type"":""long"",""index"":true}");
			property.Should().BeOfType<NumberProperty>();
			property.Type.Should().Be("long");
			((INumberProperty)property).Index.Should().BeTrue();
		}

		[U] public void Deserialize_Integer_PreservesTypeString()
		{
			var property = DeserializeProperty(@"{""type"":""integer""}");
			property.Should().BeOfType<NumberProperty>();
			property.Type.Should().Be("integer");
		}

		[U] public void Deserialize_GeoPoint_DispatchesGeoPointProperty()
		{
			// Snake_case discriminator value.
			var property = DeserializeProperty(@"{""type"":""geo_point""}");
			property.Should().BeOfType<GeoPointProperty>();
			property.Type.Should().Be("geo_point");
		}

		[U] public void Deserialize_Nested_DispatchesNestedProperty()
		{
			var property = DeserializeProperty(@"{""type"":""nested"",""properties"":{""name"":{""type"":""text""}}}");
			property.Should().BeOfType<NestedProperty>();
			property.Type.Should().Be("nested");
			var nested = (INestedProperty)property;
			nested.Properties.Should().NotBeNull();
			nested.Properties.ContainsKey("name").Should().BeTrue();
			nested.Properties["name"].Should().BeOfType<TextProperty>();
		}

		[U] public void Deserialize_Object_DispatchesObjectProperty()
		{
			var property = DeserializeProperty(@"{""type"":""object"",""properties"":{""name"":{""type"":""keyword""}}}");
			property.Should().BeOfType<ObjectProperty>();
			var obj = (IObjectProperty)property;
			obj.Properties.Should().NotBeNull();
			obj.Properties["name"].Should().BeOfType<KeywordProperty>();
		}

		[U] public void Deserialize_NoTypeButProperties_FallsBackToObject()
		{
			// No "type" field, but "properties" present -> object mapping (legacy fallback).
			var property = DeserializeProperty(@"{""properties"":{""name"":{""type"":""text""}}}");
			property.Should().BeOfType<ObjectProperty>();
			((IObjectProperty)property).Properties.ContainsKey("name").Should().BeTrue();
		}

		[U] public void Deserialize_NoTypeAtAll_FallsBackToObject()
		{
			// Neither "type" nor "properties" -> FieldType.None -> ObjectProperty.
			var property = DeserializeProperty(@"{""enabled"":false}");
			property.Should().BeOfType<ObjectProperty>();
			((IObjectProperty)property).Enabled.Should().BeFalse();
		}

		[U] public void Deserialize_UnknownType_FallsBackToObject()
		{
			// A type string that is not a known FieldType cannot be parsed -> stays None -> ObjectProperty.
			var property = DeserializeProperty(@"{""type"":""definitely_not_a_real_type""}");
			property.Should().BeOfType<ObjectProperty>();
		}

		[U] public void Deserialize_Null_ReturnsNull() => DeserializeProperty("null").Should().BeNull();

		// --- Property write dispatch ---

		[U] public void Serialize_Text_WritesTypeText()
		{
			IProperty property = new TextProperty { Analyzer = "standard" };
			var json = JsonSerializer.Serialize(property, Options());
			json.Should().Contain(@"""type"":""text""");
			json.Should().Contain(@"""analyzer"":""standard""");
		}

		[U] public void Serialize_Number_WritesConcreteTypeString()
		{
			IProperty property = new NumberProperty(NumberType.Long);
			var json = JsonSerializer.Serialize(property, Options());
			json.Should().Contain(@"""type"":""long""");
		}

		[U] public void Serialize_Null_WritesNull() =>
			JsonSerializer.Serialize<IProperty>(null, Options()).Should().Be("null");

		// --- Round trips ---

		[U] public void RoundTrip_Keyword_PreservesMembers()
		{
			IProperty property = new KeywordProperty { IgnoreAbove = 100, Boost = 1.5, Index = false };
			var back = JsonSerializer.Deserialize<IProperty>(JsonSerializer.Serialize(property, Options()), Options());
			back.Should().BeOfType<KeywordProperty>();
			var kw = (IKeywordProperty)back;
			kw.IgnoreAbove.Should().Be(100);
			kw.Boost.Should().Be(1.5);
			kw.Index.Should().BeFalse();
		}

		[U] public void RoundTrip_Number_PreservesTypeString()
		{
			IProperty property = new NumberProperty(NumberType.Integer) { Coerce = true };
			var back = JsonSerializer.Deserialize<IProperty>(JsonSerializer.Serialize(property, Options()), Options());
			back.Should().BeOfType<NumberProperty>();
			back.Type.Should().Be("integer");
			((INumberProperty)back).Coerce.Should().BeTrue();
		}

		// --- Properties dictionary ---

		[U] public void Deserialize_Properties_ReadsDictionary()
		{
			var properties = DeserializeProperties(
				@"{""title"":{""type"":""text""},""age"":{""type"":""integer""},""location"":{""type"":""geo_point""}}");
			properties.Should().NotBeNull();
			properties.Count.Should().Be(3);
			properties["title"].Should().BeOfType<TextProperty>();
			properties["age"].Should().BeOfType<NumberProperty>();
			properties["age"].Type.Should().Be("integer");
			properties["location"].Should().BeOfType<GeoPointProperty>();
			// Property names are propagated onto each value.
			properties["title"].Name.Should().Be((PropertyName)"title");
		}

		[U] public void Deserialize_Properties_SkipsNonObjectValues()
		{
			// Legacy skips any value that is not itself an object.
			var properties = DeserializeProperties(@"{""title"":{""type"":""text""},""bogus"":123,""tags"":[1,2]}");
			properties.Count.Should().Be(1);
			properties.ContainsKey("title").Should().BeTrue();
		}

		[U] public void Deserialize_Properties_Null_ReturnsNull() =>
			DeserializeProperties("null").Should().BeNull();

		[U] public void Serialize_Properties_WritesDictionary()
		{
			var properties = new Properties
			{
				{ "title", new TextProperty() },
				{ "age", new NumberProperty(NumberType.Integer) }
			};
			var json = JsonSerializer.Serialize<IProperties>(properties, Options());
			json.Should().Contain(@"""title"":{");
			json.Should().Contain(@"""type"":""text""");
			json.Should().Contain(@"""age"":{");
			json.Should().Contain(@"""type"":""integer""");
		}

		[U] public void Serialize_Properties_Null_WritesNull() =>
			JsonSerializer.Serialize<IProperties>(null, Options()).Should().Be("null");

		[U] public void RoundTrip_Properties_PreservesEntries()
		{
			var properties = new Properties
			{
				{ "title", new TextProperty { Analyzer = "standard" } },
				{ "count", new NumberProperty(NumberType.Long) },
				{ "created", new DateProperty { Format = "yyyy-MM-dd" } }
			};
			var back = JsonSerializer.Deserialize<IProperties>(JsonSerializer.Serialize<IProperties>(properties, Options()), Options());
			back.Count.Should().Be(3);
			((ITextProperty)back["title"]).Analyzer.Should().Be("standard");
			back["count"].Should().BeOfType<NumberProperty>();
			back["count"].Type.Should().Be("long");
			((IDateProperty)back["created"]).Format.Should().Be("yyyy-MM-dd");
		}

		[U] public void RoundTrip_NestedObjectProperties()
		{
			var properties = new Properties
			{
				{
					"user", new ObjectProperty
					{
						Properties = new Properties
						{
							{ "name", new TextProperty() },
							{ "email", new KeywordProperty() }
						}
					}
				}
			};
			var back = JsonSerializer.Deserialize<IProperties>(JsonSerializer.Serialize<IProperties>(properties, Options()), Options());
			back["user"].Should().BeOfType<ObjectProperty>();
			var inner = ((IObjectProperty)back["user"]).Properties;
			inner.Count.Should().Be(2);
			inner["name"].Should().BeOfType<TextProperty>();
			inner["email"].Should().BeOfType<KeywordProperty>();
		}
	}
}
