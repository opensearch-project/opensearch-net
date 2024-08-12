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

using System.Diagnostics;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A field that can index numbers so that they can later be used
/// to boost documents in queries with a rank_feature query.
/// </summary>
[InterfaceDataContract]
public interface IRankFeatureProperty : IProperty
{
    /// <summary>
    /// Rank features that correlate negatively with the score should set <see cref="PositiveScoreImpact"/>
    /// to false (defaults to true). This will be used by the rank_feature query to modify the scoring
    /// formula in such a way that the score decreases with the value of the feature instead of
    /// increasing. For instance in web search, the url length is a commonly used feature
    /// which correlates negatively with scores.
    /// </summary>
    [DataMember(Name = "positive_score_impact")]
    bool? PositiveScoreImpact { get; set; }
}

/// <inheritdoc cref="IRankFeatureProperty" />
public class RankFeatureProperty : PropertyBase, IRankFeatureProperty
{
    public RankFeatureProperty() : base(FieldType.RankFeature) { }

    /// <inheritdoc />
    public bool? PositiveScoreImpact { get; set; }
}

/// <inheritdoc cref="IRankFeatureProperty" />
[DebuggerDisplay("{DebugDisplay}")]
public class RankFeaturePropertyDescriptor<T>
    : PropertyDescriptorBase<RankFeaturePropertyDescriptor<T>, IRankFeatureProperty, T>, IRankFeatureProperty
    where T : class
{
    public RankFeaturePropertyDescriptor() : base(FieldType.RankFeature) { }

    bool? IRankFeatureProperty.PositiveScoreImpact { get; set; }

    /// <inheritdoc cref="IRankFeatureProperty.PositiveScoreImpact" />
    public RankFeaturePropertyDescriptor<T> PositiveScoreImpact(bool? positiveScoreImpact = true) =>
        Assign(positiveScoreImpact, (a, v) => a.PositiveScoreImpact = v);
}
