/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// The <c>sparse_encoding</c> processor generates sparse vector (token-weight) representations
/// of text fields for <a href="https://opensearch.org/docs/latest/vector-search/ai-search/neural-sparse-search/">neural sparse search</a>.
/// </summary>
[InterfaceDataContract]
public interface ISparseEncodingProcessor : IInferenceProcessor { }

/// <inheritdoc cref="ISparseEncodingProcessor"/>
public class SparseEncodingProcessor : InferenceProcessorBase, ISparseEncodingProcessor
{
    protected override string Name => "sparse_encoding";
}

/// <inheritdoc cref="ISparseEncodingProcessor"/>
public class SparseEncodingProcessorDescriptor<TDocument>
    : InferenceProcessorDescriptorBase<TDocument, SparseEncodingProcessorDescriptor<TDocument>, ISparseEncodingProcessor>, ISparseEncodingProcessor
    where TDocument : class
{
    protected override string Name => "sparse_encoding";
}
