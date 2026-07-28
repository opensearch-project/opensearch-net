/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// The high-level serializer engine defaults to the legacy Utf8Json engine; System.Text.Json is opt-in via
	/// <c>UseSystemTextJson()</c> (or the OSC_USE_STJ environment variable). These tests cover the programmatic switch
	/// and the default, without depending on process environment variables.
	/// </summary>
	public class EngineSelectionTests
	{
		// DiagnosticsSerializerProxy.InnerSerializer is internal to OpenSearch.Net; read the engine's type name via
		// reflection to avoid an InternalsVisibleTo dependency.
		private static string EngineTypeName(IConnectionSettingsValues settings)
		{
			var serializer = ((IConnectionConfigurationValues)settings).RequestResponseSerializer;
			var innerProp = serializer.GetType().GetProperty("InnerSerializer",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
			var inner = innerProp?.GetValue(serializer) ?? serializer;
			return inner.GetType().Name;
		}

		[U] public void DefaultsToUtf8Json()
		{
			var settings = new ConnectionSettings(new SingleNodeConnectionPool(new System.Uri("http://localhost:9200")));
			EngineTypeName(settings).Should().Be(nameof(DefaultHighLevelSerializer));
		}

		[U] public void UseSystemTextJson_OptsIntoStj()
		{
			var settings = new ConnectionSettings(new SingleNodeConnectionPool(new System.Uri("http://localhost:9200")))
				.UseSystemTextJson();
			EngineTypeName(settings).Should().Be("SystemTextJsonHighLevelSerializer");
		}

		[U] public void UseSystemTextJson_False_ForcesUtf8Json()
		{
			var settings = new ConnectionSettings(new SingleNodeConnectionPool(new System.Uri("http://localhost:9200")))
				.UseSystemTextJson(false);
			EngineTypeName(settings).Should().Be(nameof(DefaultHighLevelSerializer));
		}
	}
}
