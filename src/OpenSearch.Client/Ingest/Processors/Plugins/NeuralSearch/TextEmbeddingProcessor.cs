/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// The <c>text_embedding</c> processor is used to generate vector embeddings from text fields for <a href="https://opensearch.org/docs/latest/search-plugins/semantic-search/">semantic search</a>.
/// </summary>
[InterfaceDataContract]
public interface ITextEmbeddingProcessor : IInferenceProcessor
{
}

/// <inheritdoc cref="ITextEmbeddingProcessor"/>
public class TextEmbeddingProcessor : InferenceProcessorBase, ITextEmbeddingProcessor
{
    protected override string Name => "text_embedding";
}

/// <inheritdoc cref="ITextEmbeddingProcessor"/>
public class TextEmbeddingProcessorDescriptor<TDocument>
    : InferenceProcessorDescriptorBase<TDocument, TextEmbeddingProcessorDescriptor<TDocument>, ITextEmbeddingProcessor>, ITextEmbeddingProcessor
    where TDocument : class
{
    protected override string Name => "text_embedding";
}
