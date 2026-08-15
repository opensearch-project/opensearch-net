/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="ProcessorConverter"/>: dispatches an <see cref="IProcessor"/> to the concrete
	/// processor type selected by the single wrapping processor-name key (e.g. <c>set</c>, <c>grok</c>, <c>rename</c>),
	/// reads the whole nested body, and writes by runtime type wrapping the body under the processor's
	/// <see cref="IProcessor.Name"/>. Mirrors the legacy Utf8Json <c>ProcessorFormatter</c>.
	///
	/// Multi-word body members (e.g. <c>target_field</c>) belong to plain processor classes whose member naming is a
	/// resolver-level concern under the shared contract resolver, so they are verified via round-trip rather than by
	/// asserting a literal field name.
	/// </summary>
	public class ProcessorConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			// Processor bodies contain Field members which need the settings-aware Field converter.
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new ProcessorConverter());
			return options;
		}

		private static IProcessor Deserialize(string json) =>
			JsonSerializer.Deserialize<IProcessor>(json, Options());

		[U] public void Deserialize_Set_DispatchesConcreteType()
		{
			var processor = Deserialize(@"{""set"":{""field"":""my-field"",""value"":42}}");
			processor.Should().BeOfType<SetProcessor>();
			((SetProcessor)processor).Field.Should().Be((Field)"my-field");
		}

		[U] public void Deserialize_Grok_DispatchesConcreteType()
		{
			var processor = Deserialize(@"{""grok"":{""field"":""message""}}");
			processor.Should().BeOfType<GrokProcessor>();
			((GrokProcessor)processor).Field.Should().Be((Field)"message");
		}

		[U] public void Deserialize_Rename_DispatchesConcreteType()
		{
			var processor = Deserialize(@"{""rename"":{""field"":""old"",""target_field"":""new""}}");
			processor.Should().BeOfType<RenameProcessor>();
			((RenameProcessor)processor).Field.Should().Be((Field)"old");
		}

		[U] public void Deserialize_TextEmbedding_DispatchesLastRegisteredBranch()
		{
			var processor = Deserialize(@"{""text_embedding"":{}}");
			processor.Should().BeOfType<TextEmbeddingProcessor>();
		}

		[U] public void Deserialize_UnknownKey_ReturnsNull()
		{
			Deserialize(@"{""unknown"":{}}").Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Deserialize_NonObject_ReturnsNull()
		{
			Deserialize("42").Should().BeNull();
		}

		[U] public void Serialize_Set_WrapsInSetKey()
		{
			IProcessor processor = new SetProcessor { Field = "my-field", Value = 42 };
			var json = JsonSerializer.Serialize(processor, Options());
			json.Should().Contain(@"""set""");
			json.Should().StartWith(@"{""set"":{");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<IProcessor>(null, Options()).Should().Be("null");
		}

		[U] public void RoundTrip_Set_PreservesCommonAndSpecificMembers()
		{
			IProcessor processor = new SetProcessor
			{
				Field = "my-field",
				Value = "hello",
				Override = false,
				Description = "sets a field",
				Tag = "tag-1",
				If = "ctx.foo != null",
				IgnoreFailure = true
			};
			var back = (SetProcessor)JsonSerializer.Deserialize<IProcessor>(JsonSerializer.Serialize(processor, Options()), Options());
			back.Field.Should().Be((Field)"my-field");
			back.Override.Should().BeFalse();
			back.Description.Should().Be("sets a field");
			back.Tag.Should().Be("tag-1");
			back.If.Should().Be("ctx.foo != null");
			back.IgnoreFailure.Should().BeTrue();
		}

		[U] public void RoundTrip_Rename_PreservesMembers()
		{
			IProcessor processor = new RenameProcessor { Field = "old", TargetField = "new" };
			var back = (RenameProcessor)JsonSerializer.Deserialize<IProcessor>(JsonSerializer.Serialize(processor, Options()), Options());
			back.Field.Should().Be((Field)"old");
			back.TargetField.Should().Be((Field)"new");
		}
	}
}
