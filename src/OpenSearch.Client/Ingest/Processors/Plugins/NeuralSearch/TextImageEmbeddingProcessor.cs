/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// The <c>text_image_embedding</c> processor generates multimodal (text and image) embeddings
/// for <a href="https://opensearch.org/docs/latest/vector-search/ai-search/multimodal-search/">multimodal neural search</a>.
/// </summary>
[InterfaceDataContract]
public interface ITextImageEmbeddingProcessor : IInferenceProcessor { }

/// <inheritdoc cref="ITextImageEmbeddingProcessor"/>
public class TextImageEmbeddingProcessor : InferenceProcessorBase, ITextImageEmbeddingProcessor
{
    protected override string Name => "text_image_embedding";
}

/// <inheritdoc cref="ITextImageEmbeddingProcessor"/>
public class TextImageEmbeddingProcessorDescriptor<TDocument>
    : InferenceProcessorDescriptorBase<TDocument, TextImageEmbeddingProcessorDescriptor<TDocument>, ITextImageEmbeddingProcessor>, ITextImageEmbeddingProcessor
    where TDocument : class
{
    protected override string Name => "text_image_embedding";
}
