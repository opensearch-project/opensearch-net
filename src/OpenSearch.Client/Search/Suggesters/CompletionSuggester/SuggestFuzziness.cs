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
/// Fuzziness options for a suggester
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(SuggestFuzziness))]
public interface ISuggestFuzziness
{
    /// <summary>
    /// The fuzziness factor. defaults to AUTO
    /// </summary>
    [DataMember(Name = "fuzziness")]
    IFuzziness Fuzziness { get; set; }

    /// <summary>
    /// Minimum length of the input before fuzzy suggestions are returned. Defaults to 3
    /// </summary>
    [DataMember(Name = "min_length")]
    int? MinLength { get; set; }

    /// <summary>
    /// Minimum length of the input, which is not checked for fuzzy alternatives. Defaults to 1
    /// </summary>
    [DataMember(Name = "prefix_length")]
    int? PrefixLength { get; set; }

    /// <summary>
    /// if set to true, transpositions are counted as one change instead of two. Defaults to true
    /// </summary>
    [DataMember(Name = "transpositions")]
    bool? Transpositions { get; set; }

    /// <summary>
    /// If true, all measurements (like fuzzy edit distance, transpositions, and lengths) are measured in Unicode code
    /// points instead of in bytes. This is slightly slower than raw bytes, so it is set to false by default.
    /// </summary>
    [DataMember(Name = "unicode_aware")]
    bool? UnicodeAware { get; set; }
}

/// <inheritdoc />
public class SuggestFuzziness : ISuggestFuzziness
{
    /// <inheritdoc />
    public IFuzziness Fuzziness { get; set; }
    /// <inheritdoc />
    public int? MinLength { get; set; }
    /// <inheritdoc />
    public int? PrefixLength { get; set; }
    /// <inheritdoc />
    public bool? Transpositions { get; set; }
    /// <inheritdoc />
    public bool? UnicodeAware { get; set; }
}

/// <inheritdoc cref="ISuggestFuzziness" />
public class SuggestFuzzinessDescriptor<T> : DescriptorBase<SuggestFuzzinessDescriptor<T>, ISuggestFuzziness>, ISuggestFuzziness
    where T : class
{
    IFuzziness ISuggestFuzziness.Fuzziness { get; set; }
    int? ISuggestFuzziness.MinLength { get; set; }
    int? ISuggestFuzziness.PrefixLength { get; set; }
    bool? ISuggestFuzziness.Transpositions { get; set; }
    bool? ISuggestFuzziness.UnicodeAware { get; set; }

    /// <inheritdoc cref="ISuggestFuzziness.Fuzziness" />
    public SuggestFuzzinessDescriptor<T> Fuzziness(Fuzziness fuzziness) => Assign(fuzziness, (a, v) => a.Fuzziness = v);
    /// <inheritdoc cref="ISuggestFuzziness.UnicodeAware" />
    public SuggestFuzzinessDescriptor<T> UnicodeAware(bool? aware = true) => Assign(aware, (a, v) => a.UnicodeAware = v);
    /// <inheritdoc cref="ISuggestFuzziness.Transpositions" />
    public SuggestFuzzinessDescriptor<T> Transpositions(bool? transpositions = true) => Assign(transpositions, (a, v) => a.Transpositions = v);
    /// <inheritdoc cref="ISuggestFuzziness.MinLength" />
    public SuggestFuzzinessDescriptor<T> MinLength(int? length) => Assign(length, (a, v) => a.MinLength = v);
    /// <inheritdoc cref="ISuggestFuzziness.PrefixLength" />
    public SuggestFuzzinessDescriptor<T> PrefixLength(int? length) => Assign(length, (a, v) => a.PrefixLength = v);
}
