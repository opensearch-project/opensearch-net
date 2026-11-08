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
	/// Behavioural tests for <see cref="ScriptConverter"/>: dispatches to <see cref="InlineScript"/> when a
	/// source/inline field is present and to <see cref="IndexedScript"/> when an id field is present, carrying the
	/// shared lang and params.
	/// </summary>
	public class ScriptConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new ScriptConverter());
			return options;
		}

		[U] public void Deserialize_Source_AsInlineScript()
		{
			var script = JsonSerializer.Deserialize<IScript>(
				@"{""source"":""doc['x'].value"",""lang"":""painless""}", Options());

			script.Should().BeOfType<InlineScript>();
			((IInlineScript)script).Source.Should().Be("doc['x'].value");
			script.Lang.Should().Be("painless");
		}

		[U] public void Deserialize_Inline_AsInlineScript()
		{
			var script = JsonSerializer.Deserialize<IScript>(@"{""inline"":""1 + 1""}", Options());

			script.Should().BeOfType<InlineScript>();
			((IInlineScript)script).Source.Should().Be("1 + 1");
		}

		[U] public void Deserialize_Id_AsIndexedScript()
		{
			var script = JsonSerializer.Deserialize<IScript>(@"{""id"":""my-stored-script""}", Options());

			script.Should().BeOfType<IndexedScript>();
			((IIndexedScript)script).Id.Should().Be("my-stored-script");
		}

		[U] public void Deserialize_WithParams()
		{
			var script = JsonSerializer.Deserialize<IScript>(
				@"{""source"":""doc['x'].value * p"",""params"":{""p"":2}}", Options());

			script.Should().BeOfType<InlineScript>();
			script.Params.Should().ContainKey("p");
		}

		[U] public void Deserialize_NoKnownField_ReturnsNull()
		{
			JsonSerializer.Deserialize<IScript>(@"{""foo"":""bar""}", Options()).Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			JsonSerializer.Deserialize<IScript>("null", Options()).Should().BeNull();
		}

		[U] public void Serialize_InlineScript()
		{
			IScript script = new InlineScript("Math.log(2)") { Lang = "painless" };

			var json = JsonSerializer.Serialize(script, Options());

			json.Should().Contain(@"""source""").And.Contain("Math.log(2)");
			json.Should().Contain(@"""lang""").And.Contain("painless");
		}

		[U] public void Serialize_IndexedScript()
		{
			IScript script = new IndexedScript("my-stored-script");

			var json = JsonSerializer.Serialize(script, Options());

			json.Should().Contain(@"""id""").And.Contain("my-stored-script");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<IScript>(null, Options()).Should().Be("null");
		}
	}
}
