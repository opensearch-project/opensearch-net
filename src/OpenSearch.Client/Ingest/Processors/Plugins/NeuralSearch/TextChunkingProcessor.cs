/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

// ──────────────────────────────────────────────────────────────────────────
// Algorithm configuration
// ──────────────────────────────────────────────────────────────────────────

/// <summary>
/// Fixed-token-length chunking algorithm configuration.
/// Splits text into chunks of at most <see cref="TokenLimit"/> tokens.
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(FixedTokenLengthChunkingAlgorithm))]
public interface IFixedTokenLengthChunkingAlgorithm
{
    /// <summary>Maximum number of tokens per chunk.</summary>
    [DataMember(Name = "token_limit")]
    int? TokenLimit { get; set; }

    /// <summary>
    /// Fraction of tokens from the previous chunk to repeat at the start of the next chunk,
    /// providing context overlap. Value between 0 and 0.5.
    /// </summary>
    [DataMember(Name = "overlap_rate")]
    double? OverlapRate { get; set; }

    /// <summary>The tokenizer to use (e.g. <c>"standard"</c>).</summary>
    [DataMember(Name = "tokenizer")]
    string Tokenizer { get; set; }

    /// <summary>Maximum number of characters in the output field. Chunks exceeding
    /// this limit are further split.</summary>
    [DataMember(Name = "max_chunk_limit")]
    int? MaxChunkLimit { get; set; }
}

/// <inheritdoc cref="IFixedTokenLengthChunkingAlgorithm"/>
public class FixedTokenLengthChunkingAlgorithm : IFixedTokenLengthChunkingAlgorithm
{
    /// <inheritdoc />
    public int? TokenLimit { get; set; }
    /// <inheritdoc />
    public double? OverlapRate { get; set; }
    /// <inheritdoc />
    public string Tokenizer { get; set; }
    /// <inheritdoc />
    public int? MaxChunkLimit { get; set; }
}

/// <summary>
/// Delimiter-based chunking algorithm configuration.
/// Splits text on a delimiter string (e.g. <c>"\n\n"</c>).
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(DelimiterChunkingAlgorithm))]
public interface IDelimiterChunkingAlgorithm
{
    /// <summary>The delimiter string to split on.</summary>
    [DataMember(Name = "delimiter")]
    string Delimiter { get; set; }

    /// <summary>Maximum number of characters in the output field.</summary>
    [DataMember(Name = "max_chunk_limit")]
    int? MaxChunkLimit { get; set; }
}

/// <inheritdoc cref="IDelimiterChunkingAlgorithm"/>
public class DelimiterChunkingAlgorithm : IDelimiterChunkingAlgorithm
{
    /// <inheritdoc />
    public string Delimiter { get; set; }
    /// <inheritdoc />
    public int? MaxChunkLimit { get; set; }
}

/// <summary>Algorithm configuration for the <c>text_chunking</c> processor.</summary>
[InterfaceDataContract]
[ReadAs(typeof(ChunkingAlgorithm))]
public interface IChunkingAlgorithm
{
    /// <summary>Fixed-token-length algorithm configuration.</summary>
    [DataMember(Name = "fixed_token_length")]
    IFixedTokenLengthChunkingAlgorithm FixedTokenLength { get; set; }

    /// <summary>Delimiter-based algorithm configuration.</summary>
    [DataMember(Name = "delimiter")]
    IDelimiterChunkingAlgorithm Delimiter { get; set; }
}

/// <inheritdoc cref="IChunkingAlgorithm"/>
public class ChunkingAlgorithm : IChunkingAlgorithm
{
    /// <inheritdoc />
    public IFixedTokenLengthChunkingAlgorithm FixedTokenLength { get; set; }
    /// <inheritdoc />
    public IDelimiterChunkingAlgorithm Delimiter { get; set; }
}

/// <inheritdoc cref="IChunkingAlgorithm"/>
public class ChunkingAlgorithmDescriptor
    : DescriptorBase<ChunkingAlgorithmDescriptor, IChunkingAlgorithm>, IChunkingAlgorithm
{
    IFixedTokenLengthChunkingAlgorithm IChunkingAlgorithm.FixedTokenLength { get; set; }
    IDelimiterChunkingAlgorithm IChunkingAlgorithm.Delimiter { get; set; }

    /// <inheritdoc cref="IChunkingAlgorithm.FixedTokenLength"/>
    public ChunkingAlgorithmDescriptor FixedTokenLength(
        Func<FixedTokenLengthChunkingAlgorithmDescriptor, IFixedTokenLengthChunkingAlgorithm> selector) =>
        Assign(selector?.Invoke(new FixedTokenLengthChunkingAlgorithmDescriptor()),
            (a, v) => a.FixedTokenLength = v);

    /// <inheritdoc cref="IChunkingAlgorithm.Delimiter"/>
    public ChunkingAlgorithmDescriptor Delimiter(
        Func<DelimiterChunkingAlgorithmDescriptor, IDelimiterChunkingAlgorithm> selector) =>
        Assign(selector?.Invoke(new DelimiterChunkingAlgorithmDescriptor()),
            (a, v) => a.Delimiter = v);
}

/// <inheritdoc cref="IFixedTokenLengthChunkingAlgorithm"/>
public class FixedTokenLengthChunkingAlgorithmDescriptor
    : DescriptorBase<FixedTokenLengthChunkingAlgorithmDescriptor, IFixedTokenLengthChunkingAlgorithm>,
      IFixedTokenLengthChunkingAlgorithm
{
    int? IFixedTokenLengthChunkingAlgorithm.TokenLimit { get; set; }
    double? IFixedTokenLengthChunkingAlgorithm.OverlapRate { get; set; }
    string IFixedTokenLengthChunkingAlgorithm.Tokenizer { get; set; }
    int? IFixedTokenLengthChunkingAlgorithm.MaxChunkLimit { get; set; }

    /// <inheritdoc cref="IFixedTokenLengthChunkingAlgorithm.TokenLimit"/>
    public FixedTokenLengthChunkingAlgorithmDescriptor TokenLimit(int? limit) =>
        Assign(limit, (a, v) => a.TokenLimit = v);

    /// <inheritdoc cref="IFixedTokenLengthChunkingAlgorithm.OverlapRate"/>
    public FixedTokenLengthChunkingAlgorithmDescriptor OverlapRate(double? rate) =>
        Assign(rate, (a, v) => a.OverlapRate = v);

    /// <inheritdoc cref="IFixedTokenLengthChunkingAlgorithm.Tokenizer"/>
    public FixedTokenLengthChunkingAlgorithmDescriptor Tokenizer(string tokenizer) =>
        Assign(tokenizer, (a, v) => a.Tokenizer = v);

    /// <inheritdoc cref="IFixedTokenLengthChunkingAlgorithm.MaxChunkLimit"/>
    public FixedTokenLengthChunkingAlgorithmDescriptor MaxChunkLimit(int? limit) =>
        Assign(limit, (a, v) => a.MaxChunkLimit = v);
}

/// <inheritdoc cref="IDelimiterChunkingAlgorithm"/>
public class DelimiterChunkingAlgorithmDescriptor
    : DescriptorBase<DelimiterChunkingAlgorithmDescriptor, IDelimiterChunkingAlgorithm>,
      IDelimiterChunkingAlgorithm
{
    string IDelimiterChunkingAlgorithm.Delimiter { get; set; }
    int? IDelimiterChunkingAlgorithm.MaxChunkLimit { get; set; }

    /// <inheritdoc cref="IDelimiterChunkingAlgorithm.Delimiter"/>
    public DelimiterChunkingAlgorithmDescriptor Delimiter(string delimiter) =>
        Assign(delimiter, (a, v) => a.Delimiter = v);

    /// <inheritdoc cref="IDelimiterChunkingAlgorithm.MaxChunkLimit"/>
    public DelimiterChunkingAlgorithmDescriptor MaxChunkLimit(int? limit) =>
        Assign(limit, (a, v) => a.MaxChunkLimit = v);
}

// ──────────────────────────────────────────────────────────────────────────
// TextChunkingProcessor
// ──────────────────────────────────────────────────────────────────────────

/// <summary>
/// The <c>text_chunking</c> processor splits long text fields into smaller chunks before
/// embedding. Supports fixed-token-length and delimiter-based algorithms.
/// <para>See <a href="https://opensearch.org/docs/latest/ingest-pipelines/processors/text-chunking/">Text chunking</a>.</para>
/// </summary>
[InterfaceDataContract]
public interface ITextChunkingProcessor : IProcessor
{
    /// <summary>The chunking algorithm configuration.</summary>
    [DataMember(Name = "algorithm")]
    IChunkingAlgorithm Algorithm { get; set; }

    /// <summary>
    /// Maps source fields to target fields that will hold the chunked output.
    /// </summary>
    [DataMember(Name = "field_map")]
    IInferenceFieldMap FieldMap { get; set; }

    /// <summary>
    /// Maximum number of characters allowed in a single chunk. Chunks exceeding
    /// this limit are split further. Defaults to <c>10000</c>.
    /// </summary>
    [DataMember(Name = "max_chunk_limit")]
    int? MaxChunkLimit { get; set; }
}

/// <inheritdoc cref="ITextChunkingProcessor"/>
public class TextChunkingProcessor : ProcessorBase, ITextChunkingProcessor
{
    protected override string Name => "text_chunking";

    /// <inheritdoc />
    public IChunkingAlgorithm Algorithm { get; set; }
    /// <inheritdoc />
    public IInferenceFieldMap FieldMap { get; set; }
    /// <inheritdoc />
    public int? MaxChunkLimit { get; set; }
}

/// <inheritdoc cref="ITextChunkingProcessor"/>
public class TextChunkingProcessorDescriptor<TDocument>
    : ProcessorDescriptorBase<TextChunkingProcessorDescriptor<TDocument>, ITextChunkingProcessor>,
      ITextChunkingProcessor
    where TDocument : class
{
    protected override string Name => "text_chunking";

    IChunkingAlgorithm ITextChunkingProcessor.Algorithm { get; set; }
    IInferenceFieldMap ITextChunkingProcessor.FieldMap { get; set; }
    int? ITextChunkingProcessor.MaxChunkLimit { get; set; }

    /// <inheritdoc cref="ITextChunkingProcessor.Algorithm"/>
    public TextChunkingProcessorDescriptor<TDocument> Algorithm(
        IChunkingAlgorithm algorithm) =>
        Assign(algorithm, (a, v) => a.Algorithm = v);

    /// <inheritdoc cref="ITextChunkingProcessor.Algorithm"/>
    public TextChunkingProcessorDescriptor<TDocument> Algorithm(
        Func<ChunkingAlgorithmDescriptor, IChunkingAlgorithm> selector) =>
        Assign(selector?.Invoke(new ChunkingAlgorithmDescriptor()), (a, v) => a.Algorithm = v);

    /// <inheritdoc cref="ITextChunkingProcessor.FieldMap"/>
    public TextChunkingProcessorDescriptor<TDocument> FieldMap(
        IDictionary<Field, Field> fieldMap) =>
        Assign(fieldMap, (a, v) => a.FieldMap = v != null ? new InferenceFieldMap(v) : null);

    /// <inheritdoc cref="ITextChunkingProcessor.FieldMap"/>
    public TextChunkingProcessorDescriptor<TDocument> FieldMap(
        IInferenceFieldMap fieldMap) =>
        Assign(fieldMap, (a, v) => a.FieldMap = v);

    /// <inheritdoc cref="ITextChunkingProcessor.FieldMap"/>
    public TextChunkingProcessorDescriptor<TDocument> FieldMap(
        Func<InferenceFieldMapDescriptor<TDocument>, IPromise<IInferenceFieldMap>> selector) =>
        Assign(selector, (a, v) => a.FieldMap = v?.Invoke(new InferenceFieldMapDescriptor<TDocument>())?.Value);

    /// <inheritdoc cref="ITextChunkingProcessor.MaxChunkLimit"/>
    public TextChunkingProcessorDescriptor<TDocument> MaxChunkLimit(int? limit) =>
        Assign(limit, (a, v) => a.MaxChunkLimit = v);
}
