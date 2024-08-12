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

using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// BM25 Similarity. Introduced in Stephen E. Robertson, Steve Walker, Susan Jones, Micheline Hancock-Beaulieu,
/// and Mike Gatford. Okapi at TREC-3. In Proceedings of the Third Text Retrieval Conference (TREC 1994). Gaithersburg,
/// USA, November 1994.
/// </summary>
public interface IBM25Similarity : ISimilarity
{
    /// <summary>
    /// Controls to what degree document length normalizes tf values.
    /// </summary>
    [DataMember(Name = "b")]
    [JsonFormatter(typeof(NullableStringDoubleFormatter))]
    double? B { get; set; }

    /// <summary>
    /// Sets whether overlap tokens (Tokens with 0 position increment) are ignored when computing norm.
    /// </summary>
    [DataMember(Name = "discount_overlaps")]
    [JsonFormatter(typeof(NullableStringBooleanFormatter))]
    bool? DiscountOverlaps { get; set; }

    /// <summary>
    /// Controls non-linear term frequency normalization (saturation).
    /// </summary>
    [DataMember(Name = "k1")]
    [JsonFormatter(typeof(NullableStringDoubleFormatter))]
    double? K1 { get; set; }
}

/// <inheritdoc />
public class BM25Similarity : IBM25Similarity
{
    /// <inheritdoc />
    public double? B { get; set; }

    /// <inheritdoc />
    public bool? DiscountOverlaps { get; set; }

    /// <inheritdoc />
    public double? K1 { get; set; }

    public string Type => "BM25";
}

/// <inheritdoc />
public class BM25SimilarityDescriptor
    : DescriptorBase<BM25SimilarityDescriptor, IBM25Similarity>, IBM25Similarity
{
    double? IBM25Similarity.B { get; set; }
    bool? IBM25Similarity.DiscountOverlaps { get; set; }
    double? IBM25Similarity.K1 { get; set; }
    string ISimilarity.Type => "BM25";

    /// <inheritdoc />
    public BM25SimilarityDescriptor DiscountOverlaps(bool? discount = true) => Assign(discount, (a, v) => a.DiscountOverlaps = v);

    /// <inheritdoc />
    public BM25SimilarityDescriptor K1(double? k1) => Assign(k1, (a, v) => a.K1 = v);

    /// <inheritdoc />
    public BM25SimilarityDescriptor B(double? b) => Assign(b, (a, v) => a.B = v);
}
