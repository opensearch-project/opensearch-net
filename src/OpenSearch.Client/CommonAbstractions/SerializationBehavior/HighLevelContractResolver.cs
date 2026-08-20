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
	/// The System.Text.Json <see cref="IJsonTypeInfoResolver"/> that reproduces the runtime-config-driven
	/// serialization of the legacy Utf8Json <c>InnerResolver.GetMapping</c>:
	/// <list type="bullet">
	/// <item>applies the connection settings' <c>DefaultFieldNameInferrer</c> to property names;</item>
	/// <item>applies per-member <c>PropertyMappings</c> (explicit name / ignore) configured at runtime;</item>
	/// <item>inherits the interface-data-contract behaviour from <see cref="InterfaceDataContractResolver"/>.</item>
	/// </list>
	/// The resolver is parameterised by <see cref="IConnectionSettingsValues"/> the same way the old engine was.
	/// </summary>
	internal class HighLevelContractResolver : InterfaceDataContractResolver
	{
		private readonly IConnectionSettingsValues _settings;
		// A settings-carrying formatter resolver so a type-level ShouldSerialize(IJsonFormatterResolver) (e.g.
		// Routing's, which resolves the routing via the Inferrer) can be invoked. Built once, lazily.
		private OpenSearch.Net.Utf8Json.IJsonFormatterResolver _formatterResolver;

		public HighLevelContractResolver(IConnectionSettingsValues settings) => _settings = settings;

		// Keep a non-[DataMember] member of a data-contract type when it carries an OSC serialization-directing
		// attribute the base resolver cannot see — [PropertyName] / an OSC mapping attribute ([Text(Name=)], …) — so a
		// user IProperty implementation's [PropertyName]-decorated members are not dropped (matching the legacy engine,
		// which did not treat a class as a data-contract merely for implementing an [InterfaceDataContract] interface).
		protected override bool KeepNonDataMember(MemberInfo member)
		{
			if (member == null)
				return false;
			if (OpenSearchPropertyAttributeBase.From(member) != null)
				return true;
			var mapping = _settings.PropertyMappingProvider?.CreatePropertyMapping(member);
			return mapping != null && !string.IsNullOrEmpty(mapping.Name);
		}

		// TODO(utf8json-decoupling): this holds a Utf8Json IJsonFormatterResolver so that type-level
		// ShouldSerialize(IJsonFormatterResolver) conventions carried over from the legacy engine (e.g. Routing's,
		// which resolves the routing via the Inferrer) can still be invoked during STJ serialization. It is a
		// deliberate reuse of the legacy contract machinery rather than a rewrite; fully decoupling from Utf8Json is a
		// follow-up tied to removing the vendored library (see dev-docs/system-text-json-migration.md).
		private OpenSearch.Net.Utf8Json.IJsonFormatterResolver FormatterResolver =>
			_formatterResolver ?? (_formatterResolver = new OpenSearchClientFormatterResolver(_settings));

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
				// must/should/must_not/filter arrays). System.Text.Json does not call these methods, so wire them into
				// JsonPropertyInfo.ShouldSerialize.
				if (member != null && property.ShouldSerialize == null)
				{
					var predicate = FindShouldSerialize(member);
					if (predicate != null)
						property.ShouldSerialize = predicate;
				}

				// Also honour the type-level ShouldSerialize(IJsonFormatterResolver) convention the legacy engine
				// applied to every member whose declared TYPE defines it (Routing omits an empty inferred routing;
				// QueryContainer omits a conditionless query). Without this a non-null-but-empty Routing emits
				// "routing": null and a conditionless standalone query emits an empty object.
				if (member != null && property.ShouldSerialize == null)
				{
					var predicate = FindTypeShouldSerialize(property.PropertyType);
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

		// Reproduces the legacy type-level ShouldSerialize convention: a member is omitted when its declared TYPE
		// defines `bool ShouldSerialize(IJsonFormatterResolver)` returning false. Used by Routing (empty inferred
		// routing) and QueryContainer (conditionless query). Returns null when the type has no such method.
		private Func<object, object, bool> FindTypeShouldSerialize(Type propertyType)
		{
			if (propertyType == null)
				return null;

			var method = propertyType.GetMethod("ShouldSerialize",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null, new[] { typeof(OpenSearch.Net.Utf8Json.IJsonFormatterResolver) }, null);
			if (method == null || method.ReturnType != typeof(bool))
				return null;

			// The predicate's second arg is the property VALUE; ShouldSerialize is an instance method on that value's
			// type (e.g. QueryContainer / Routing), so invoke it on the value, not on the containing object.
			return (_, value) => value != null && (bool)method.Invoke(value, new object[] { FormatterResolver });
		}

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
