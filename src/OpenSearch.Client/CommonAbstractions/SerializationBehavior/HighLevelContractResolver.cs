/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using OpenSearch.Net.Serialization.Converters;

namespace OpenSearch.Client
{
	/// <summary>
	/// PROTOTYPE (spike) — the System.Text.Json <see cref="IJsonTypeInfoResolver"/> that reproduces the
	/// runtime-config-driven serialization of the legacy Utf8Json <c>InnerResolver.GetMapping</c>:
	/// <list type="bullet">
	/// <item>applies the connection settings' <c>DefaultFieldNameInferrer</c> to property names;</item>
	/// <item>applies per-member <c>PropertyMappings</c> (explicit name / ignore) configured at runtime;</item>
	/// <item>inherits the interface-data-contract behaviour from <see cref="InterfaceDataContractResolver"/>.</item>
	/// </list>
	/// This proves the key claim of the spike: a STJ resolver can be parameterised by
	/// <see cref="IConnectionSettingsValues"/> the same way the old engine was.
	/// </summary>
	internal class HighLevelContractResolver : InterfaceDataContractResolver
	{
		private readonly IConnectionSettingsValues _settings;

		public HighLevelContractResolver(IConnectionSettingsValues settings) => _settings = settings;

		public override JsonTypeInfo GetTypeInfo(System.Type type, JsonSerializerOptions options)
		{
			var typeInfo = base.GetTypeInfo(type, options);

			if (typeInfo.Kind != JsonTypeInfoKind.Object)
				return typeInfo;

			foreach (var property in typeInfo.Properties)
			{
				var member = property.AttributeProvider as MemberInfo;

				// Per-member runtime property mapping (explicit name / ignore) takes precedence.
				if (member != null && _settings.PropertyMappings.TryGetValue(member, out var mapping))
				{
					if (mapping.Ignore)
					{
						property.ShouldSerialize = (_, __) => false;
						continue;
					}

					if (!string.IsNullOrEmpty(mapping.Name))
					{
						property.Name = mapping.Name;
						continue;
					}
				}

				// Otherwise apply the default field-name inferrer (e.g. camelCase) from the settings.
				if (_settings.DefaultFieldNameInferrer != null)
					property.Name = _settings.DefaultFieldNameInferrer(property.Name);
			}

			return typeInfo;
		}
	}
}
