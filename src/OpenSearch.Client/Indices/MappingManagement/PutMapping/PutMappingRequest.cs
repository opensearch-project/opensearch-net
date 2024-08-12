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
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[MapsApi("indices.put_mapping.json")]
[ReadAs(typeof(PutMappingRequest))]
public partial interface IPutMappingRequest : ITypeMapping { }

[InterfaceDataContract]
[ReadAs(typeof(PutMappingRequest<object>))]
// ReSharper disable once UnusedTypeParameter
public partial interface IPutMappingRequest<TDocument> where TDocument : class { }

[DataContract]
public partial class PutMappingRequest
{
    /// <inheritdoc />
    public bool? DateDetection { get; set; }

    /// <inheritdoc />
    public Union<bool, DynamicMapping> Dynamic { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> DynamicDateFormats { get; set; }

    /// <inheritdoc />
    public IDynamicTemplateContainer DynamicTemplates { get; set; }

    /// <inheritdoc />
    public IFieldNamesField FieldNamesField { get; set; }

    /// <inheritdoc />
    public IDictionary<string, object> Meta { get; set; }

    /// <inheritdoc />
    public bool? NumericDetection { get; set; }

    /// <inheritdoc />
    public IProperties Properties { get; set; }

    /// <inheritdoc />
    public IRoutingField RoutingField { get; set; }

    /// <inheritdoc />
    public IRuntimeFields RuntimeFields { get; set; }

    /// <inheritdoc />
    public ISizeField SizeField { get; set; }

    /// <inheritdoc />
    public ISourceField SourceField { get; set; }
}

// ReSharper disable once UnusedTypeParameter
public partial class PutMappingRequest<TDocument> where TDocument : class { }

[DataContract]
public partial class PutMappingDescriptor<TDocument> where TDocument : class
{
    bool? ITypeMapping.DateDetection { get; set; }
    Union<bool, DynamicMapping> ITypeMapping.Dynamic { get; set; }
    IEnumerable<string> ITypeMapping.DynamicDateFormats { get; set; }
    IDynamicTemplateContainer ITypeMapping.DynamicTemplates { get; set; }
    IFieldNamesField ITypeMapping.FieldNamesField { get; set; }
    IDictionary<string, object> ITypeMapping.Meta { get; set; }
    bool? ITypeMapping.NumericDetection { get; set; }
    IProperties ITypeMapping.Properties { get; set; }
    IRoutingField ITypeMapping.RoutingField { get; set; }
    IRuntimeFields ITypeMapping.RuntimeFields { get; set; }
    ISizeField ITypeMapping.SizeField { get; set; }
    ISourceField ITypeMapping.SourceField { get; set; }

    protected PutMappingDescriptor<TDocument> Assign<TValue>(TValue value, Action<ITypeMapping, TValue> assigner) =>
        Fluent.Assign(this, value, assigner);

    /// <summary>
    /// Convenience method to map as much as it can based on OpenSearchType attributes set on the type.
    /// <para>This method also automatically sets up mappings for primitive values types (e.g. int, long, double, DateTime...)</para>
    /// <para>Class types default to object and Enums to int</para>
    /// <para>Later calls can override whatever is set by this call.</para>
    /// </summary>
    public PutMappingDescriptor<TDocument> AutoMap(IPropertyVisitor visitor = null, int maxRecursion = 0)
    {
        Self.Properties = Self.Properties.AutoMap<TDocument>(visitor, maxRecursion);
        return this;
    }

    /// <inheritdoc cref="AutoMap(IPropertyVisitor,int)" />
    public PutMappingDescriptor<TDocument> AutoMap(int maxRecursion) => AutoMap(null, maxRecursion);

    /// <inheritdoc cref="ITypeMapping.Dynamic" />
    public PutMappingDescriptor<TDocument> Dynamic(Union<bool, DynamicMapping> dynamic) => Assign(dynamic, (a, v) => a.Dynamic = v);

    /// <inheritdoc cref="ITypeMapping.Dynamic" />
    public PutMappingDescriptor<TDocument> Dynamic(bool? dynamic = true) => Assign(dynamic, (a, v) => a.Dynamic = v);

    /// <inheritdoc cref="ITypeMapping.SizeField" />
    public PutMappingDescriptor<TDocument> SizeField(Func<SizeFieldDescriptor, ISizeField> sizeFieldSelector) =>
        Assign(sizeFieldSelector, (a, v) => a.SizeField = v?.Invoke(new SizeFieldDescriptor()));

    /// <inheritdoc cref="ITypeMapping.SizeField" />
    public PutMappingDescriptor<TDocument> DisableSizeField(bool? disabled = true) =>
        Assign(disabled, (a, v) => a.SizeField = new SizeField { Enabled = !v });

    /// <inheritdoc cref="ITypeMapping.DynamicDateFormats" />
    public PutMappingDescriptor<TDocument> DynamicDateFormats(IEnumerable<string> dateFormats) =>
        Assign(dateFormats, (a, v) => a.DynamicDateFormats = v);

    /// <inheritdoc cref="ITypeMapping.DateDetection" />
    public PutMappingDescriptor<TDocument> DateDetection(bool? detect = true) => Assign(detect, (a, v) => a.DateDetection = v);

    /// <inheritdoc cref="ITypeMapping.NumericDetection" />
    public PutMappingDescriptor<TDocument> NumericDetection(bool? detect = true) => Assign(detect, (a, v) => a.NumericDetection = v);

    /// <inheritdoc cref="ITypeMapping.SourceField" />
    public PutMappingDescriptor<TDocument> SourceField(Func<SourceFieldDescriptor, ISourceField> sourceFieldSelector) =>
        Assign(sourceFieldSelector, (a, v) => a.SourceField = v?.Invoke(new SourceFieldDescriptor()));

    /// <inheritdoc cref="ITypeMapping.RoutingField" />
    public PutMappingDescriptor<TDocument> RoutingField(Func<RoutingFieldDescriptor<TDocument>, IRoutingField> routingFieldSelector) =>
        Assign(routingFieldSelector, (a, v) => a.RoutingField = v?.Invoke(new RoutingFieldDescriptor<TDocument>()));

    /// <inheritdoc cref="ITypeMapping.RuntimeFields" />
    public PutMappingDescriptor<TDocument> RuntimeFields(Func<RuntimeFieldsDescriptor<TDocument>, IPromise<IRuntimeFields>> runtimeFieldsSelector) =>
        Assign(runtimeFieldsSelector, (a, v) => a.RuntimeFields = v?.Invoke(new RuntimeFieldsDescriptor<TDocument>())?.Value);

    /// <inheritdoc cref="ITypeMapping.RuntimeFields" />
    public PutMappingDescriptor<TDocument> RuntimeFields<TSource>(Func<RuntimeFieldsDescriptor<TSource>, IPromise<IRuntimeFields>> runtimeFieldsSelector) where TSource : class =>
        Assign(runtimeFieldsSelector, (a, v) => a.RuntimeFields = v?.Invoke(new RuntimeFieldsDescriptor<TSource>())?.Value);

    /// <inheritdoc cref="ITypeMapping.FieldNamesField" />
    public PutMappingDescriptor<TDocument> FieldNamesField(Func<FieldNamesFieldDescriptor<TDocument>, IFieldNamesField> fieldNamesFieldSelector) =>
        Assign(fieldNamesFieldSelector, (a, v) => a.FieldNamesField = v.Invoke(new FieldNamesFieldDescriptor<TDocument>()));

    /// <inheritdoc cref="ITypeMapping.Meta" />
    public PutMappingDescriptor<TDocument> Meta(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> metaSelector) =>
        Assign(metaSelector, (a, v) => a.Meta = v(new FluentDictionary<string, object>()));

    /// <inheritdoc cref="ITypeMapping.Meta" />
    public PutMappingDescriptor<TDocument> Meta(Dictionary<string, object> metaDictionary) => Assign(metaDictionary, (a, v) => a.Meta = v);

    /// <inheritdoc cref="ITypeMapping.Properties" />
    public PutMappingDescriptor<TDocument> Properties(Func<PropertiesDescriptor<TDocument>, IPromise<IProperties>> propertiesSelector) =>
        Assign(propertiesSelector, (a, v) => a.Properties = v?.Invoke(new PropertiesDescriptor<TDocument>(a.Properties))?.Value);

    /// <inheritdoc cref="ITypeMapping.DynamicTemplates" />
    public PutMappingDescriptor<TDocument> DynamicTemplates(Func<DynamicTemplateContainerDescriptor<TDocument>, IPromise<IDynamicTemplateContainer>> dynamicTemplatesSelector) =>
        Assign(dynamicTemplatesSelector, (a, v) => a.DynamicTemplates = v?.Invoke(new DynamicTemplateContainerDescriptor<TDocument>())?.Value);
}
