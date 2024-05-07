/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(VerbatimDictionaryKeysFormatter<InferenceFieldMap, IInferenceFieldMap, Field, Field>))]
public interface IInferenceFieldMap : IIsADictionary<Field, Field> { }

[InterfaceDataContract]
public interface IInferenceProcessor : IProcessor
{
    /// <summary>
    /// The ID of the model that will be used to generate the embeddings.
    /// The model must be deployed in OpenSearch before it can be used in neural search.
    /// </summary>
    /// <remarks>
    /// For more information,
    /// see <a href="https://opensearch.org/docs/latest/ml-commons-plugin/using-ml-models/">Using custom models within OpenSearch</a>
    /// and <a href="https://opensearch.org/docs/latest/search-plugins/semantic-search/">Semantic search</a>.
    /// </remarks>
    [DataMember(Name = "model_id")]
    string ModelId { get; set; }

    /// <summary>
    /// Contains key-value pairs that specify the mapping of a text field to a vector field.
    /// <ul>
    /// <li><c>Key</c> being the name of the field from which to generate embeddings.</li>
    /// <li><c>Value</c> being the name of the vector field in which to store the generated embeddings.</li>
    /// </ul>
    /// </summary>
    [DataMember(Name = "field_map")]
    IInferenceFieldMap FieldMap { get; set; }
}

public class InferenceFieldMap : IsADictionaryBase<Field, Field>, IInferenceFieldMap
{
    public InferenceFieldMap() { }
    public InferenceFieldMap(IDictionary<Field, Field> container) : base(container) { }

    public void Add(Field source, Field target) => BackingDictionary.Add(source, target);
}

/// <inheritdoc cref="IInferenceProcessor"/>
public abstract class InferenceProcessorBase : ProcessorBase, IInferenceProcessor
{
    /// <inheritdoc />
    public string ModelId { get; set; }
    /// <inheritdoc />
    public IInferenceFieldMap FieldMap { get; set; }
}

public class InferenceFieldMapDescriptor<TDocument>
    : IsADictionaryDescriptorBase<InferenceFieldMapDescriptor<TDocument>, InferenceFieldMap, Field, Field>
    where TDocument : class
{
    public InferenceFieldMapDescriptor() : base(new InferenceFieldMap()) { }

    public InferenceFieldMapDescriptor<TDocument> Map(Field source, Field target) =>
        Assign(source, target);

    public InferenceFieldMapDescriptor<TDocument> Map<TSourceValue>(
        Expression<Func<TDocument, TSourceValue>> source,
        Field target
    ) =>
        Assign(source, target);

    public InferenceFieldMapDescriptor<TDocument> Map<TTargetValue>(
        Field source,
        Expression<Func<TDocument, TTargetValue>> target
    ) =>
        Assign(source, target);

    public InferenceFieldMapDescriptor<TDocument> Map<TSourceValue, TTargetValue>(
        Expression<Func<TDocument, TSourceValue>> source,
        Expression<Func<TDocument, TTargetValue>> target
    ) =>
        Assign(source, target);
}

/// <inheritdoc cref="IInferenceProcessor"/>
public abstract class InferenceProcessorDescriptorBase<T, TInferenceProcessorDescriptor, TInferenceProcessorInterface>
    : ProcessorDescriptorBase<TInferenceProcessorDescriptor, TInferenceProcessorInterface>, IInferenceProcessor
    where T : class
    where TInferenceProcessorDescriptor : InferenceProcessorDescriptorBase<T, TInferenceProcessorDescriptor, TInferenceProcessorInterface>, TInferenceProcessorInterface
    where TInferenceProcessorInterface : class, IInferenceProcessor
{
    string IInferenceProcessor.ModelId { get; set; }
    IInferenceFieldMap IInferenceProcessor.FieldMap { get; set; }

    /// <inheritdoc cref="IInferenceProcessor.ModelId"/>
    public TInferenceProcessorDescriptor ModelId(string modelId) => Assign(modelId, (a, v) => a.ModelId = v);

    /// <inheritdoc cref="IInferenceProcessor.FieldMap"/>
    public TInferenceProcessorDescriptor FieldMap(IDictionary<Field, Field> fieldMap) =>
        Assign(fieldMap, (a, v) => a.FieldMap = v != null ? new InferenceFieldMap(v) : null);

    /// <inheritdoc cref="IInferenceProcessor.FieldMap"/>
    public TInferenceProcessorDescriptor FieldMap(IInferenceFieldMap fieldMap) =>
        Assign(fieldMap, (a, v) => a.FieldMap = v);

    /// <inheritdoc cref="IInferenceProcessor.FieldMap"/>
    public TInferenceProcessorDescriptor FieldMap(Func<InferenceFieldMapDescriptor<T>, IPromise<IInferenceFieldMap>> selector) =>
        Assign(selector, (a, v) => a.FieldMap = v?.Invoke(new InferenceFieldMapDescriptor<T>())?.Value);

    /// <inheritdoc cref="IInferenceProcessor.FieldMap"/>
    public TInferenceProcessorDescriptor FieldMap<TDocument>(Func<InferenceFieldMapDescriptor<TDocument>, IPromise<IInferenceFieldMap>> selector)
        where TDocument : class =>
        Assign(selector, (a, v) => a.FieldMap = v?.Invoke(new InferenceFieldMapDescriptor<TDocument>())?.Value);
}
