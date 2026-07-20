/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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

				// Member-level [JsonFormatter(typeof(XxxFormatter))]: reproduce the legacy engine's per-member
				// formatter override by binding the migrated converter to this property. Only member-specific
				// formatters that are NOT registered as a global type-level default belong here (a global
				// registration already covers the type). This runs independently of the name logic below (a member
				// can have both a converter and an inferred/explicit name), so it is applied first and does not
				// short-circuit the naming branches.
				if (member != null && property.CustomConverter == null)
				{
					var memberConverter = MemberFormatterConverters.TryCreate(member, _settings);
					if (memberConverter != null)
						property.CustomConverter = memberConverter;
				}

				// Honour the legacy ShouldSerialize{Member}() convention (used by e.g. IBoolQuery to omit empty
				// must/should/must_not/filter arrays and by Routing/QueryContainer). System.Text.Json does not call
				// these methods, so wire them into JsonPropertyInfo.ShouldSerialize.
				if (member != null && property.ShouldSerialize == null)
				{
					var predicate = FindShouldSerialize(member);
					if (predicate != null)
						property.ShouldSerialize = predicate;
				}

				// Wire name / ignore, reproducing the legacy OpenSearchClientFormatterResolver.GetMapping precedence:
				//   fluent PropertyMappings  >  OSC mapping attribute ([Text(Name=)], ...)  >  property-mapping provider
				//   ([PropertyName], [DataMember], or a custom IPropertyMappingProvider).
				// The STJ resolver previously only consulted PropertyMappings + [DataMember], so [PropertyName], OSC
				// attributes and custom providers were ignored and the member fell through to the camelCase inferrer.
				if (member != null)
				{
					_settings.PropertyMappings.TryGetValue(member, out var fluentMapping);
					var oscAttr = OpenSearchPropertyAttributeBase.From(member);
					var providerMapping = _settings.PropertyMappingProvider?.CreatePropertyMapping(member);

					var ignore = fluentMapping?.Ignore ?? oscAttr?.Ignore ?? providerMapping?.Ignore;
					if (ignore == true)
					{
						property.ShouldSerialize = (_, __) => false;
						continue;
					}

					var name = fluentMapping?.Name ?? oscAttr?.Name ?? providerMapping?.Name;
					if (!string.IsNullOrEmpty(name))
					{
						property.Name = name;
						continue;
					}
				}

				// An explicit [DataMember(Name)] (already applied by the base resolver, on the property or a matching
				// interface property) is authoritative — do not run the field-name inferrer over it, or a snake_case
				// name like "is_write_index" would be re-mangled to camelCase.
				if (HasExplicitDataMemberName(member))
					continue;

				// Otherwise apply the default field-name inferrer (e.g. camelCase) from the settings.
				if (_settings.DefaultFieldNameInferrer != null)
					property.Name = _settings.DefaultFieldNameInferrer(property.Name);
			}

			return typeInfo;
		}

		// True when the member (or a matching interface property) carries a [DataMember(Name=...)] with a non-empty
		// Name — that explicit wire name is authoritative and must not be overwritten by the field-name inferrer.
		// Locates a bool ShouldSerialize{Member}() method (public on the declaring type, or an explicit-interface
		// implementation on an implemented interface) and returns a predicate that invokes it, or null if none.
		// Mirrors the JSON.NET/Utf8Json ShouldSerialize convention the legacy engine honoured.
		private static Func<object, object, bool> FindShouldSerialize(MemberInfo member)
		{
			if (!(member is PropertyInfo) || member.DeclaringType == null)
				return null;

			var methodName = "ShouldSerialize" + member.Name;

			var method = member.DeclaringType.GetMethod(methodName,
				BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

			if (method == null)
			{
				foreach (var i in member.DeclaringType.GetInterfaces())
				{
					var m = i.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
					if (m != null)
					{
						method = m;
						break;
					}
				}
			}

			if (method == null || method.ReturnType != typeof(bool))
				return null;

			return (obj, _) => obj != null && (bool)method.Invoke(obj, null);
		}

		private static bool HasExplicitDataMemberName(MemberInfo member)
		{
			if (member == null)
				return false;

			var dm = member.GetCustomAttribute<DataMemberAttribute>(true);
			if (dm != null && !string.IsNullOrEmpty(dm.Name))
				return true;

			if (member.DeclaringType == null)
				return false;

			foreach (var i in member.DeclaringType.GetInterfaces())
			{
				var p = i.GetProperty(member.Name, BindingFlags.Public | BindingFlags.Instance);
				var idm = p?.GetCustomAttribute<DataMemberAttribute>(true);
				if (idm != null && !string.IsNullOrEmpty(idm.Name))
					return true;
			}

			return false;
		}
	}
}
