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
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// Normalizers are similar to analyzers except that they may only emit a single token.
/// As a consequence, they do not have a tokenizer and only accept a subset of the available
/// char filters and token filters. Only the filters that work on a per-character basis are
/// allowed. For instance a lowercasing filter would be allowed, but not a stemming filter,
/// which needs to look at the keyword as a whole.
/// <para>OpenSearch does not ship with built-in normalizers so far, so the only way to create one is through composing a custom one</para>
/// </summary>
[InterfaceDataContract]
public interface ICustomNormalizer : INormalizer
{
    /// <summary>
    /// Char filters to normalize the keyword
    /// </summary>
    [DataMember(Name = "char_filter")]
    [JsonFormatter(typeof(SingleOrEnumerableFormatter<string>))]
    IEnumerable<string> CharFilter { get; set; }

    /// <summary>
    /// An optional list of logical / registered name of token filters.
    /// </summary>
    [DataMember(Name = "filter")]
    [JsonFormatter(typeof(SingleOrEnumerableFormatter<string>))]
    IEnumerable<string> Filter { get; set; }
}

/// <inheritdoc />
public class CustomNormalizer : NormalizerBase, ICustomNormalizer
{
    public CustomNormalizer() : base("custom") { }

    /// <inheritdoc />
    public IEnumerable<string> CharFilter { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> Filter { get; set; }
}

/// <inheritdoc />
public class CustomNormalizerDescriptor
    : NormalizerDescriptorBase<CustomNormalizerDescriptor, ICustomNormalizer>, ICustomNormalizer
{
    protected override string Type => "custom";

    IEnumerable<string> ICustomNormalizer.CharFilter { get; set; }
    IEnumerable<string> ICustomNormalizer.Filter { get; set; }

    /// <inheritdoc />
    public CustomNormalizerDescriptor Filters(params string[] filters) => Assign(filters, (a, v) => a.Filter = v);

    /// <inheritdoc />
    public CustomNormalizerDescriptor Filters(IEnumerable<string> filters) => Assign(filters, (a, v) => a.Filter = v);

    /// <inheritdoc />
    public CustomNormalizerDescriptor CharFilters(params string[] charFilters) => Assign(charFilters, (a, v) => a.CharFilter = v);

    /// <inheritdoc />
    public CustomNormalizerDescriptor CharFilters(IEnumerable<string> charFilters) => Assign(charFilters, (a, v) => a.CharFilter = v);
}
