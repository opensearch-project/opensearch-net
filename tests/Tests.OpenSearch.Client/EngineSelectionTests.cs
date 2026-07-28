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
	/// System.Text.Json is opt-in via <c>UseSystemTextJson()</c> (or the OSC_USE_STJ / OSC_USE_UTF8JSON environment
	/// variables, which the unit CI matrix sets per leg). These tests assert the PROGRAMMATIC switch, which takes
	/// precedence over the environment variables, so they are deterministic regardless of which engine the CI leg
	/// selects. The plain default (no method call, no env var) is Utf8Json, but that is not asserted here because the
	/// process environment cannot be assumed clean under the test matrix.
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

		private static ConnectionSettings NewSettings() =>
			new ConnectionSettings(new SingleNodeConnectionPool(new System.Uri("http://localhost:9200")));

		[U] public void UseSystemTextJson_OptsIntoStj()
		{
			var settings = NewSettings().UseSystemTextJson();
			EngineTypeName(settings).Should().Be("SystemTextJsonHighLevelSerializer");
		}

		[U] public void UseSystemTextJson_False_ForcesUtf8Json()
		{
			// Explicit false overrides any OSC_USE_STJ=true set by the CI matrix.
			var settings = NewSettings().UseSystemTextJson(false);
			EngineTypeName(settings).Should().Be(nameof(DefaultHighLevelSerializer));
		}

		[U] public void UseSystemTextJson_IsRepeatableAndLastCallWins()
		{
			var settings = NewSettings().UseSystemTextJson().UseSystemTextJson(false);
			EngineTypeName(settings).Should().Be(nameof(DefaultHighLevelSerializer));

			settings.UseSystemTextJson();
			EngineTypeName(settings).Should().Be("SystemTextJsonHighLevelSerializer");
		}
	}
}
