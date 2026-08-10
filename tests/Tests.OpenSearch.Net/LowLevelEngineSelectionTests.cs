/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Reflection;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// The low-level <see cref="ConnectionConfiguration"/> mirrors the high-level
	/// <c>ConnectionSettings.UseSystemTextJson()</c> switch (see EngineSelectionTests.cs in
	/// Tests.OpenSearch.Client): System.Text.Json is opt-in via <c>UseSystemTextJson()</c> or the
	/// OSC_USE_STJ / OSC_USE_UTF8JSON environment variables, but Utf8Json remains the default. These
	/// tests assert the programmatic switch, which takes precedence over the environment variables, so
	/// they are deterministic regardless of which engine a CI leg's environment selects.
	/// </summary>
	public class LowLevelEngineSelectionTests
	{
		// DiagnosticsSerializerProxy.InnerSerializer is internal; read the engine's type name via
		// reflection to avoid an InternalsVisibleTo dependency, matching EngineSelectionTests.cs's approach.
		private static string EngineTypeName(IConnectionConfigurationValues config)
		{
			var serializer = config.RequestResponseSerializer;
			var innerProp = serializer.GetType().GetProperty("InnerSerializer",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			var inner = innerProp?.GetValue(serializer) ?? serializer;
			return inner.GetType().Name;
		}

		private static ConnectionConfiguration NewConfig() =>
			new ConnectionConfiguration(new SingleNodeConnectionPool(new Uri("http://localhost:9200")));

		[U] public void UseSystemTextJson_OptsIntoStj()
		{
			var config = NewConfig().UseSystemTextJson();
			EngineTypeName(config).Should().Be(nameof(SystemTextJsonSerializer));
		}

		[U] public void UseSystemTextJson_False_ForcesUtf8Json()
		{
			// Explicit false overrides any OSC_USE_STJ=true set in the environment.
			var config = NewConfig().UseSystemTextJson(false);
			EngineTypeName(config).Should().Be(nameof(LowLevelRequestResponseSerializer));
		}

		[U] public void UseSystemTextJson_IsRepeatableAndLastCallWins()
		{
			var config = NewConfig().UseSystemTextJson().UseSystemTextJson(false);
			EngineTypeName(config).Should().Be(nameof(LowLevelRequestResponseSerializer));

			config.UseSystemTextJson();
			EngineTypeName(config).Should().Be(nameof(SystemTextJsonSerializer));
		}

		// An explicitly-supplied serializer always wins over the toggle -- both at construction time and
		// across later UseSystemTextJson() calls, matching the pre-existing constructor contract (a
		// caller-supplied serializer was never overridden by anything before this switch existed).
		[U] public void ExplicitSerializer_AlwaysWins_OverToggle()
		{
			var explicitSerializer = new LowLevelRequestResponseSerializer();
			var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
			var config = new ConnectionConfiguration(pool, explicitSerializer);

			EngineTypeName(config).Should().Be(nameof(LowLevelRequestResponseSerializer));

			// Calling UseSystemTextJson() must not dislodge the explicit serializer.
			config.UseSystemTextJson();
			EngineTypeName(config).Should().Be(nameof(LowLevelRequestResponseSerializer));
		}

		// The default (no method call, no env var) is Utf8Json -- not asserted directly here because the
		// process environment cannot be assumed clean under the CI test matrix (matching
		// EngineSelectionTests.cs's own caveat for the high-level default).

		// Environment-variable-driven selection. These tests set process-wide environment variables, so each
		// clears both variables in a finally block to avoid leaking state into other tests -- including the
		// high-level EngineSelectionTests.cs, since SystemTextJsonEnvironment.ReadOverride() is now shared
		// between the two layers.
		[U] public void EnvironmentVariable_OscUseStjTrue_OptsIntoStj()
		{
			Environment.SetEnvironmentVariable("OSC_USE_STJ", "true");
			try
			{
				EngineTypeName(NewConfig()).Should().Be(nameof(SystemTextJsonSerializer));
			}
			finally
			{
				Environment.SetEnvironmentVariable("OSC_USE_STJ", null);
			}
		}

		[U] public void EnvironmentVariable_OscUseStjFalse_ForcesUtf8Json()
		{
			Environment.SetEnvironmentVariable("OSC_USE_STJ", "false");
			try
			{
				EngineTypeName(NewConfig()).Should().Be(nameof(LowLevelRequestResponseSerializer));
			}
			finally
			{
				Environment.SetEnvironmentVariable("OSC_USE_STJ", null);
			}
		}

		[U] public void EnvironmentVariable_LegacyOscUseUtf8JsonFalse_OptsIntoStj()
		{
			// The legacy env var's sense is inverted relative to OSC_USE_STJ.
			Environment.SetEnvironmentVariable("OSC_USE_UTF8JSON", "false");
			try
			{
				EngineTypeName(NewConfig()).Should().Be(nameof(SystemTextJsonSerializer));
			}
			finally
			{
				Environment.SetEnvironmentVariable("OSC_USE_UTF8JSON", null);
			}
		}

		[U] public void EnvironmentVariable_LegacyOscUseUtf8JsonTrue_ForcesUtf8Json()
		{
			Environment.SetEnvironmentVariable("OSC_USE_UTF8JSON", "true");
			try
			{
				EngineTypeName(NewConfig()).Should().Be(nameof(LowLevelRequestResponseSerializer));
			}
			finally
			{
				Environment.SetEnvironmentVariable("OSC_USE_UTF8JSON", null);
			}
		}

		[U] public void ProgrammaticToggle_TakesPrecedenceOverEnvironmentVariable()
		{
			Environment.SetEnvironmentVariable("OSC_USE_STJ", "true");
			try
			{
				// An explicit UseSystemTextJson(false) call must override OSC_USE_STJ=true.
				var config = NewConfig().UseSystemTextJson(false);
				EngineTypeName(config).Should().Be(nameof(LowLevelRequestResponseSerializer));
			}
			finally
			{
				Environment.SetEnvironmentVariable("OSC_USE_STJ", null);
			}
		}

		[U] public void ExplicitConstructorSerializer_TakesPrecedenceOverEnvironmentVariable()
		{
			Environment.SetEnvironmentVariable("OSC_USE_STJ", "true");
			try
			{
				var explicitSerializer = new LowLevelRequestResponseSerializer();
				var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
				var config = new ConnectionConfiguration(pool, explicitSerializer);

				EngineTypeName(config).Should().Be(nameof(LowLevelRequestResponseSerializer));
			}
			finally
			{
				Environment.SetEnvironmentVariable("OSC_USE_STJ", null);
			}
		}

	}
}
