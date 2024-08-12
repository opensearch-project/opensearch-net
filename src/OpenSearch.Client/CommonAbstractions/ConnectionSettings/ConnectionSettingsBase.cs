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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenSearch.Net;

namespace OpenSearch.Client;

/// <inheritdoc cref="IConnectionSettingsValues" />
public class ConnectionSettings : ConnectionSettingsBase<ConnectionSettings>
{
    /// <summary> The default user agent for OpenSearch.Client </summary>
    public static readonly string DefaultUserAgent =
        $"opensearch-net/{typeof(IConnectionSettingsValues).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion} ({RuntimeInformation.OSDescription}; {RuntimeInformation.FrameworkDescription}; OpenSearch.Client)";

    /// <summary>
    /// A delegate used to construct a serializer to serialize CLR types representing documents and other types related to
    /// documents.
    /// By default, the internal serializer will be used to serializer all types.
    /// </summary>
    public delegate IOpenSearchSerializer SourceSerializerFactory(IOpenSearchSerializer builtIn, IConnectionSettingsValues values);

    /// <summary>
    /// Creates a new instance of connection settings, if <paramref name="uri"/> is not specified will default to connecting to http://localhost:9200
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="connection"></param>
    public ConnectionSettings(Uri uri = null, IConnection connection = null) : this(new SingleNodeConnectionPool(uri ?? new Uri("http://localhost:9200")), connection) { }

    /// <summary>
    /// Sets up the client to communicate to OpenSearch Cloud using <paramref name="cloudId"/>,
    /// <para><see cref="CloudConnectionPool"/> documentation for more information on how to obtain your Cloud Id</para>
    /// </summary>
    public ConnectionSettings(string cloudId, BasicAuthenticationCredentials credentials) : this(new CloudConnectionPool(cloudId, credentials)) { }

    /// <summary>
    /// Sets up the client to communicate to OpenSearch Cloud using <paramref name="cloudId"/>,
    /// <para><see cref="CloudConnectionPool"/> documentation for more information on how to obtain your Cloud Id</para>
    /// </summary>
    public ConnectionSettings(string cloudId, ApiKeyAuthenticationCredentials credentials) : this(new CloudConnectionPool(cloudId, credentials)) { }

    /// <summary>
    /// Instantiate connection settings using a <see cref="SingleNodeConnectionPool" /> using the provided
    /// <see cref="InMemoryConnection" /> that never uses any IO.
    /// </summary>
    public ConnectionSettings(InMemoryConnection connection)
        : this(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection) { }

    public ConnectionSettings(IConnectionPool connectionPool) : this(connectionPool, null, null) { }

    public ConnectionSettings(IConnectionPool connectionPool, SourceSerializerFactory sourceSerializer)
        : this(connectionPool, null, sourceSerializer) { }

    public ConnectionSettings(IConnectionPool connectionPool, IConnection connection) : this(connectionPool, connection, null) { }

    public ConnectionSettings(IConnectionPool connectionPool, IConnection connection, SourceSerializerFactory sourceSerializer)
        : this(connectionPool, connection, sourceSerializer, null) { }

    public ConnectionSettings(
        IConnectionPool connectionPool,
        IConnection connection,
        SourceSerializerFactory sourceSerializer,
        IPropertyMappingProvider propertyMappingProvider
    ) : base(connectionPool, connection, sourceSerializer, propertyMappingProvider) { }
}

/// <inheritdoc cref="IConnectionSettingsValues" />
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class ConnectionSettingsBase<TConnectionSettings> : ConnectionConfiguration<TConnectionSettings>, IConnectionSettingsValues
    where TConnectionSettings : ConnectionSettingsBase<TConnectionSettings>, IConnectionSettingsValues
{
    private readonly FluentDictionary<Type, string> _defaultIndices;
    private readonly FluentDictionary<Type, string> _defaultRelationNames;
    private readonly HashSet<Type> _disableIdInference = new HashSet<Type>();
    private readonly FluentDictionary<Type, string> _idProperties = new FluentDictionary<Type, string>();
    private readonly Inferrer _inferrer;
    private readonly IPropertyMappingProvider _propertyMappingProvider;
    private readonly FluentDictionary<MemberInfo, IPropertyMapping> _propertyMappings = new FluentDictionary<MemberInfo, IPropertyMapping>();
    private readonly FluentDictionary<Type, string> _routeProperties = new FluentDictionary<Type, string>();
    private readonly IOpenSearchSerializer _sourceSerializer;

    private bool _defaultDisableAllInference;
    private Func<string, string> _defaultFieldNameInferrer;
    private string _defaultIndex;

    protected ConnectionSettingsBase(
        IConnectionPool connectionPool,
        IConnection connection,
        ConnectionSettings.SourceSerializerFactory sourceSerializerFactory,
        IPropertyMappingProvider propertyMappingProvider
    )
        : base(connectionPool, connection, null)
    {
        var formatterResolver = new OpenSearchClientFormatterResolver(this);
        var defaultSerializer = new DefaultHighLevelSerializer(formatterResolver);
        var sourceSerializer = sourceSerializerFactory?.Invoke(defaultSerializer, this) ?? defaultSerializer;
        var serializerAsMappingProvider = sourceSerializer as IPropertyMappingProvider;

        _propertyMappingProvider = propertyMappingProvider ?? serializerAsMappingProvider ?? new PropertyMappingProvider();

        //We wrap these in an internal proxy to facilitate serialization diagnostics
        _sourceSerializer = new DiagnosticsSerializerProxy(sourceSerializer, "source");
        UseThisRequestResponseSerializer = new DiagnosticsSerializerProxy(defaultSerializer);
        _defaultFieldNameInferrer = p => p.ToCamelCase();
        _defaultIndices = new FluentDictionary<Type, string>();
        _defaultRelationNames = new FluentDictionary<Type, string>();
        _inferrer = new Inferrer(this);

        UserAgent(ConnectionSettings.DefaultUserAgent);
    }

    bool IConnectionSettingsValues.DefaultDisableIdInference => _defaultDisableAllInference;
    Func<string, string> IConnectionSettingsValues.DefaultFieldNameInferrer => _defaultFieldNameInferrer;
    string IConnectionSettingsValues.DefaultIndex => _defaultIndex;
    FluentDictionary<Type, string> IConnectionSettingsValues.DefaultIndices => _defaultIndices;
    HashSet<Type> IConnectionSettingsValues.DisableIdInference => _disableIdInference;
    FluentDictionary<Type, string> IConnectionSettingsValues.DefaultRelationNames => _defaultRelationNames;
    FluentDictionary<Type, string> IConnectionSettingsValues.IdProperties => _idProperties;
    Inferrer IConnectionSettingsValues.Inferrer => _inferrer;
    IPropertyMappingProvider IConnectionSettingsValues.PropertyMappingProvider => _propertyMappingProvider;
    FluentDictionary<MemberInfo, IPropertyMapping> IConnectionSettingsValues.PropertyMappings => _propertyMappings;
    FluentDictionary<Type, string> IConnectionSettingsValues.RouteProperties => _routeProperties;
    IOpenSearchSerializer IConnectionSettingsValues.SourceSerializer => _sourceSerializer;

    /// <summary>
    /// The default index to use for a request when no index has been explicitly specified
    /// and no default indices are specified for the given CLR type specified for the request.
    /// </summary>
    public TConnectionSettings DefaultIndex(string defaultIndex) => Assign(defaultIndex, (a, v) => a._defaultIndex = v);

    /// <summary>
    /// Specifies how field names are inferred from CLR property names.
    /// <para></para>
    /// By default, OpenSearch.Client camel cases property names.
    /// </summary>
    /// <example>
    /// CLR property EmailAddress will be inferred as "emailAddress" OpenSearch document field name
    /// </example>
    public TConnectionSettings DefaultFieldNameInferrer(Func<string, string> fieldNameInferrer) =>
        Assign(fieldNameInferrer, (a, v) => a._defaultFieldNameInferrer = v);

    /// <summary>
    /// Disables automatic Id inference for given CLR types.
    /// <para></para>
    /// OpenSearch.Client by default will use the value of a property named Id on a CLR type as the _id to send to OpenSearch. Adding a type
    /// will disable this behaviour for that CLR type. If Id inference should be disabled for all CLR types, use
    /// <see cref="DefaultDisableIdInference"/>
    /// </summary>
    public TConnectionSettings DefaultDisableIdInference(bool disable = true) => Assign(disable, (a, v) => a._defaultDisableAllInference = v);

    private void MapIdPropertyFor<TDocument>(Expression<Func<TDocument, object>> objectPath)
    {
        objectPath.ThrowIfNull(nameof(objectPath));

        var memberInfo = new MemberInfoResolver(objectPath);
        var fieldName = memberInfo.Members.Single().Name;

        if (_idProperties.TryGetValue(typeof(TDocument), out var idPropertyFieldName))
        {
            if (idPropertyFieldName.Equals(fieldName)) return;

            throw new ArgumentException(
                $"Cannot map '{fieldName}' as the id property for type '{typeof(TDocument).Name}': it already has '{_idProperties[typeof(TDocument)]}' mapped.");
        }

        _idProperties.Add(typeof(TDocument), fieldName);
    }

    /// <inheritdoc cref="IConnectionSettingsValues.RouteProperties"/>
    private void MapRoutePropertyFor<TDocument>(Expression<Func<TDocument, object>> objectPath)
    {
        objectPath.ThrowIfNull(nameof(objectPath));

        var memberInfo = new MemberInfoResolver(objectPath);
        var fieldName = memberInfo.Members.Single().Name;

        if (_routeProperties.TryGetValue(typeof(TDocument), out var routePropertyFieldName))
        {
            if (routePropertyFieldName.Equals(fieldName)) return;

            throw new ArgumentException(
                $"Cannot map '{fieldName}' as the route property for type '{typeof(TDocument).Name}': it already has '{_routeProperties[typeof(TDocument)]}' mapped.");
        }

        _routeProperties.Add(typeof(TDocument), fieldName);
    }

    private void ApplyPropertyMappings<TDocument>(IList<IClrPropertyMapping<TDocument>> mappings)
        where TDocument : class
    {
        foreach (var mapping in mappings)
        {
            var e = mapping.Property;
            var memberInfoResolver = new MemberInfoResolver(e);
            if (memberInfoResolver.Members.Count > 1)
                throw new ArgumentException($"{nameof(ApplyPropertyMappings)} can only map direct properties");

            if (memberInfoResolver.Members.Count == 0)
                throw new ArgumentException($"Expression {e} does contain any member access");

            var memberInfo = memberInfoResolver.Members[0];

            if (_propertyMappings.TryGetValue(memberInfo, out var propertyMapping))
            {
                var newName = mapping.NewName;
                var mappedAs = propertyMapping.Name;
                var typeName = typeof(TDocument).Name;
                if (mappedAs.IsNullOrEmpty() && newName.IsNullOrEmpty())
                    throw new ArgumentException($"Property mapping '{e}' on type is already ignored");
                if (mappedAs.IsNullOrEmpty())
                    throw new ArgumentException(
                        $"Property mapping '{e}' on type {typeName} can not be mapped to '{newName}' it already has an ignore mapping");
                if (newName.IsNullOrEmpty())
                    throw new ArgumentException(
                        $"Property mapping '{e}' on type {typeName} can not be ignored it already has a mapping to '{mappedAs}'");

                throw new ArgumentException(
                    $"Property mapping '{e}' on type {typeName} can not be mapped to '{newName}' already mapped as '{mappedAs}'");
            }
            _propertyMappings[memberInfo] = mapping.ToPropertyMapping();
        }
    }

    /// <summary>
    /// Specify how the mapping is inferred for a given CLR type.
    /// The mapping can infer the index, id and relation name for a given CLR type, as well as control
    /// serialization behaviour for CLR properties.
    /// </summary>
    public TConnectionSettings DefaultMappingFor<TDocument>(Func<ClrTypeMappingDescriptor<TDocument>, IClrTypeMapping<TDocument>> selector)
        where TDocument : class
    {
        var inferMapping = selector(new ClrTypeMappingDescriptor<TDocument>());
        if (!inferMapping.IndexName.IsNullOrEmpty())
            _defaultIndices[inferMapping.ClrType] = inferMapping.IndexName;

        if (!inferMapping.RelationName.IsNullOrEmpty())
            _defaultRelationNames[inferMapping.ClrType] = inferMapping.RelationName;

        if (!string.IsNullOrWhiteSpace(inferMapping.IdPropertyName))
            _idProperties[inferMapping.ClrType] = inferMapping.IdPropertyName;

        if (inferMapping.IdProperty != null)
            MapIdPropertyFor(inferMapping.IdProperty);

        if (inferMapping.RoutingProperty != null)
            MapRoutePropertyFor(inferMapping.RoutingProperty);

        if (inferMapping.Properties != null)
            ApplyPropertyMappings(inferMapping.Properties);

        if (inferMapping.DisableIdInference) _disableIdInference.Add(inferMapping.ClrType);
        else _disableIdInference.Remove(inferMapping.ClrType);

        return (TConnectionSettings)this;
    }

    /// <summary>
    /// Specify how the mapping is inferred for a given CLR type.
    /// The mapping can infer the index and relation name for a given CLR type.
    /// </summary>
    public TConnectionSettings DefaultMappingFor(Type documentType, Func<ClrTypeMappingDescriptor, IClrTypeMapping> selector)
    {
        var inferMapping = selector(new ClrTypeMappingDescriptor(documentType));
        if (!inferMapping.IndexName.IsNullOrEmpty())
            _defaultIndices[inferMapping.ClrType] = inferMapping.IndexName;

        if (!inferMapping.RelationName.IsNullOrEmpty())
            _defaultRelationNames[inferMapping.ClrType] = inferMapping.RelationName;

        if (!string.IsNullOrWhiteSpace(inferMapping.IdPropertyName))
            _idProperties[inferMapping.ClrType] = inferMapping.IdPropertyName;

        return (TConnectionSettings)this;
    }

    /// <summary>
    /// Specify how the mapping is inferred for a given CLR type.
    /// The mapping can infer the index and relation name for a given CLR type.
    /// </summary>
    public TConnectionSettings DefaultMappingFor(IEnumerable<IClrTypeMapping> typeMappings)
    {
        if (typeMappings == null) return (TConnectionSettings)this;

        foreach (var inferMapping in typeMappings)
        {
            if (!inferMapping.IndexName.IsNullOrEmpty())
                _defaultIndices[inferMapping.ClrType] = inferMapping.IndexName;

            if (!inferMapping.RelationName.IsNullOrEmpty())
                _defaultRelationNames[inferMapping.ClrType] = inferMapping.RelationName;
        }

        return (TConnectionSettings)this;
    }

    /// <summary>
    /// OpenSearch.Client handles 404 in its <see cref="ResponseBase.IsValid"/>, we do not want the low level client throwing exceptions
    /// when <see cref="IConnectionConfigurationValues.ThrowExceptions"/> is enabled for 404's. The client is in charge of composing paths
    /// so a 404 never signals a wrong url but a missing entity.
    /// </summary>
    protected override bool HttpStatusCodeClassifier(HttpMethod method, int statusCode) =>
        statusCode >= 200 && statusCode < 300
        || statusCode == 404;
}
