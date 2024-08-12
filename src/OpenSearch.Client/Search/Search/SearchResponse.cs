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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client;

/// <summary>
/// A response to a search request
/// </summary>
/// <typeparam name="TDocument">The document type</typeparam>
public interface ISearchResponse<out TDocument> : IResponse where TDocument : class
{
    /// <summary>
    /// Gets the collection of aggregations
    /// </summary>
    AggregateDictionary Aggregations { get; }

    /// <summary>
    /// Gets the statistics about the clusters on which the search query was executed.
    /// </summary>
    ClusterStatistics Clusters { get; }

    /// <summary>
    /// Gets the documents inside the hits, by deserializing <see cref="IHitMetadata{T}.Source" />
    /// into <typeparamref name="TDocument" />
    /// <para>
    /// NOTE: if you use <see cref="ISearchRequest.StoredFields" /> on the search request,
    /// <see cref="Documents" /> will be empty and you should use <see cref="Fields" />
    /// instead to get the field values. As an alternative to
    /// <see cref="Fields" />, try source filtering using <see cref="ISearchRequest.Source" /> on the
    /// search request to return <see cref="Documents" /> with partial fields selected
    /// </para>
    /// </summary>
    IReadOnlyCollection<TDocument> Documents { get; }

    /// <summary>
    /// Gets the field values inside the hits, when the search request uses
    /// <see cref="SearchRequest.StoredFields" />.
    /// </summary>
    IReadOnlyCollection<FieldValues> Fields { get; }

    /// <summary>
    /// Gets the collection of hits that matched the query
    /// </summary>
    /// <value>
    /// The hits.
    /// </value>
    IReadOnlyCollection<IHit<TDocument>> Hits { get; }

    /// <summary>
    /// Gets the meta data about the hits that match the search query criteria.
    /// </summary>
    IHitsMetadata<TDocument> HitsMetadata { get; }

    /// <summary>
    /// Gets the maximum score for documents matching the search query criteria
    /// </summary>
    double MaxScore { get; }

    /// <summary>
    /// Number of times the server performed an incremental reduce phase
    /// </summary>
    long NumberOfReducePhases { get; }

    /// <summary>
    /// Gets the results of profiling the search query. Has a value only when
    /// <see cref="ISearchRequest.Profile" /> is set to <c>true</c> on the search request.
    /// </summary>
    Profile Profile { get; }

    /// <summary>
    /// Gets the scroll id which can be passed to the Scroll API in order to retrieve the next batch
    /// of results. Has a value only when <see cref="SearchRequest.Scroll" /> is specified on the
    /// search request
    /// </summary>
    string ScrollId { get; }

    /// <summary>
    /// Gets the statistics about the shards on which the search query was executed.
    /// </summary>
    ShardStatistics Shards { get; }

    /// <summary>
    /// Gets the suggester results.
    /// </summary>
    ISuggestDictionary<TDocument> Suggest { get; }

    /// <summary>
    /// Gets a value indicating whether the search was terminated early
    /// </summary>
    bool TerminatedEarly { get; }

    /// <summary>
    /// Gets a value indicating whether the search timed out or not
    /// </summary>
    bool TimedOut { get; }

    /// <summary>
    /// Time in milliseconds for OpenSearch to execute the search
    /// </summary>
    long Took { get; }

    /// <summary>
    /// Gets the total number of documents matching the search query criteria
    /// </summary>
    long Total { get; }
}

public class SearchResponse<TDocument> : ResponseBase, ISearchResponse<TDocument> where TDocument : class
{
    private IReadOnlyCollection<TDocument> _documents;

    private IReadOnlyCollection<FieldValues> _fields;

    private IReadOnlyCollection<IHit<TDocument>> _hits;

    /// <inheritdoc />
    [DataMember(Name = "aggregations")]
    public AggregateDictionary Aggregations { get; internal set; } = AggregateDictionary.Default;

    /// <inheritdoc />
    [DataMember(Name = "_clusters")]
    public ClusterStatistics Clusters { get; internal set; }

    /// <inheritdoc />
    [IgnoreDataMember]
    public IReadOnlyCollection<TDocument> Documents =>
        _documents ?? (_documents = Hits
            .Select(h => h.Source)
            .ToList());

    /// <inheritdoc />
    [IgnoreDataMember]
    public IReadOnlyCollection<FieldValues> Fields =>
        _fields ?? (_fields = Hits
            .Select(h => h.Fields)
            .ToList());

    /// <inheritdoc />
    [IgnoreDataMember]
    public IReadOnlyCollection<IHit<TDocument>> Hits =>
        _hits ?? (_hits = HitsMetadata?.Hits ?? EmptyReadOnly<IHit<TDocument>>.Collection);

    /// <inheritdoc />
    [DataMember(Name = "hits")]
    public IHitsMetadata<TDocument> HitsMetadata { get; internal set; }

    /// <inheritdoc />
    [IgnoreDataMember]
    public double MaxScore => HitsMetadata?.MaxScore ?? 0;

    /// <inheritdoc />
    [DataMember(Name = "num_reduce_phases")]
    public long NumberOfReducePhases { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "profile")]
    public Profile Profile { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "_scroll_id")]
    public string ScrollId { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "_shards")]
    public ShardStatistics Shards { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "suggest")]
    public ISuggestDictionary<TDocument> Suggest { get; internal set; } = SuggestDictionary<TDocument>.Default;

    /// <inheritdoc />
    [DataMember(Name = "terminated_early")]
    public bool TerminatedEarly { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "timed_out")]
    public bool TimedOut { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "took")]
    public long Took { get; internal set; }

    /// <inheritdoc />
    [IgnoreDataMember]
    public long Total => HitsMetadata?.Total?.Value ?? -1;
}
