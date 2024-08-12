/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Concurrent;
using System.Reflection;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Formatters;
using OpenSearch.Net.Utf8Json.Resolvers;

namespace OpenSearch.Client;

internal interface IJsonFormatterResolverWithSettings
{
    IConnectionSettingsValues Settings { get; }
}

internal class OpenSearchClientFormatterResolver : IJsonFormatterResolver, IJsonFormatterResolverWithSettings
{
    private readonly IJsonFormatter<object> _fallbackFormatter;
    private readonly InnerResolver _innerFormatterResolver;

    public OpenSearchClientFormatterResolver(IConnectionSettingsValues settings)
    {
        Settings = settings;
        _innerFormatterResolver = new InnerResolver(settings);
        _fallbackFormatter = new DynamicObjectTypeFallbackFormatter(_innerFormatterResolver);
    }

    public IConnectionSettingsValues Settings { get; }

    public IJsonFormatter<T> GetFormatter<T>() =>
        typeof(T) == typeof(object)
            ? (IJsonFormatter<T>)_fallbackFormatter
            : _innerFormatterResolver.GetFormatter<T>();

    internal sealed class InnerResolver : IJsonFormatterResolver
    {
        private static readonly IJsonFormatterResolver[] Resolvers =
        {
				// IL emit a resolver that registers formatters
				DynamicCompositeResolver.Create(new IJsonFormatter[]
            {
                new QueryContainerCollectionFormatter(),
                new SimpleQueryStringFlagsFormatter(),
                new TimeSpanTicksFormatter(),
                new NullableTimeSpanTicksFormatter(),
                new JsonNetCompatibleUriFormatter(),
                new GeoOrientationFormatter(),
                new NullableGeoOrientationFormatter(),
                new ShapeOrientationFormatter(),
                new NullableShapeOrientationFormatter(),
            }, new IJsonFormatterResolver[0]),
            BuiltinResolver.Instance, // Builtin primitives
				OpenSearchNetEnumResolver.Instance, // Specialized Enum handling
				AttributeFormatterResolver.Instance, // [JsonFormatter]
				ReadAsFormatterResolver.Instance, // [ReadAs]
				IsADictionaryFormatterResolver.Instance, // IsADictionaryBase<TKey, TValue>
				DynamicGenericResolver.Instance, // T[], List<T>, etc...
				InterfaceGenericDictionaryResolver.Instance,
            InterfaceGenericReadOnlyDictionaryResolver.Instance
        };

        private readonly IJsonFormatterResolver _finalFormatter;
        private readonly ConcurrentDictionary<Type, object> _formatters = new ConcurrentDictionary<Type, object>();
        private readonly IConnectionSettingsValues _settings;

        internal InnerResolver(IConnectionSettingsValues settings)
        {
            _settings = settings;
            _finalFormatter =
                DynamicObjectResolver.Create(GetMapping, new Lazy<Func<string, string>>(() => settings.DefaultFieldNameInferrer), true);
        }

        public IJsonFormatter<T> GetFormatter<T>() =>
            (IJsonFormatter<T>)_formatters.GetOrAdd(typeof(T), type =>
            {
                foreach (var item in Resolvers)
                {
                    var formatter = item.GetFormatter<T>();
                    if (formatter != null)
                        return formatter;
                }

                return _finalFormatter.GetFormatter<T>();
            });

        private JsonProperty GetMapping(MemberInfo member)
        {
            // TODO: Skip calling this method for OSC and OpenSearch.Net types, at the type level
            if (!_settings.PropertyMappings.TryGetValue(member, out var propertyMapping))
                propertyMapping = OpenSearchPropertyAttributeBase.From(member);

            var serializerMapping = _settings.PropertyMappingProvider?.CreatePropertyMapping(member);
            var nameOverride = propertyMapping?.Name ?? serializerMapping?.Name;
            var property = new JsonProperty(nameOverride);

            var overrideIgnore = propertyMapping?.Ignore ?? serializerMapping?.Ignore;
            if (overrideIgnore.HasValue)
                property.Ignore = overrideIgnore.Value;

            if (propertyMapping != null || serializerMapping != null)
                property.AllowPrivate = true;

            if (member.GetCustomAttribute<StringEnumAttribute>() != null)
                CreateEnumFormatterForProperty(member, property);
            else if (member.GetCustomAttribute<StringTimeSpanAttribute>() != null)
            {
                switch (member)
                {
                    case PropertyInfo propertyInfo:
                        property.JsonFormatter =
                            BuiltinResolver.BuiltinResolverGetFormatterHelper.GetFormatter(propertyInfo.PropertyType);
                        break;
                    case FieldInfo fieldInfo:
                        property.JsonFormatter =
                            BuiltinResolver.BuiltinResolverGetFormatterHelper.GetFormatter(fieldInfo.FieldType);
                        break;
                }
            }
            else if (member.GetCustomAttribute<EpochDateTimeAttribute>() != null)
                property.JsonFormatter = EpochDateTimeFormatter.Instance;

            return property;
        }

        private static void CreateEnumFormatterForType(Type type, JsonProperty property)
        {
            if (type.IsEnum)
                property.JsonFormatter = typeof(EnumFormatter<>).MakeGenericType(type).CreateInstance(true);
            else if (type.IsNullable())
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType.IsEnum)
                {
                    var innerFormatter = typeof(EnumFormatter<>).MakeGenericType(underlyingType).CreateInstance(true);
                    property.JsonFormatter = typeof(StaticNullableFormatter<>).MakeGenericType(underlyingType).CreateInstance(innerFormatter);
                }
            }
        }

        private static void CreateEnumFormatterForProperty(MemberInfo member, JsonProperty property)
        {
            switch (member)
            {
                case PropertyInfo propertyInfo:
                    {
                        CreateEnumFormatterForType(propertyInfo.PropertyType, property);
                        break;
                    }
                case FieldInfo fieldInfo:
                    {
                        CreateEnumFormatterForType(fieldInfo.FieldType, property);
                        break;
                    }
            }
        }
    }
}
