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
///  The path_hierarchy tokenizer takes something like this:
/// <para>/something/something/else</para>
/// <para>And produces tokens:</para>
/// <para></para>
/// <para>/something</para>
/// <para>/something/something</para>
/// <para>/something/something/else</para>
/// </summary>
public interface IPathHierarchyTokenizer : ITokenizer
{
    /// <summary>
    /// The buffer size to use, defaults to 1024.
    /// </summary>
    [DataMember(Name = "buffer_size")]
    [JsonFormatter(typeof(NullableStringIntFormatter))]
    int? BufferSize { get; set; }

    /// <summary>
    /// The character delimiter to use, defaults to /.
    /// </summary>
    [DataMember(Name = "delimiter")]
    char? Delimiter { get; set; }

    /// <summary>
    /// An optional replacement character to use. Defaults to the delimiter
    /// </summary>
    [DataMember(Name = "replacement")]
    char? Replacement { get; set; }

    /// <summary>
    /// Generates tokens in reverse order, defaults to false.
    /// </summary>
    [DataMember(Name = "reverse")]
    [JsonFormatter(typeof(NullableStringBooleanFormatter))]
    bool? Reverse { get; set; }

    /// <summary>
    /// Controls initial tokens to skip, defaults to 0.
    /// </summary>
    [DataMember(Name = "skip")]
    [JsonFormatter(typeof(NullableStringIntFormatter))]
    int? Skip { get; set; }
}

/// <inheritdoc />
public class PathHierarchyTokenizer : TokenizerBase, IPathHierarchyTokenizer
{
    public PathHierarchyTokenizer() => Type = "path_hierarchy";

    /// <summary />
    public int? BufferSize { get; set; }

    /// <summary />
    public char? Delimiter { get; set; }

    /// <summary />
    public char? Replacement { get; set; }

    /// <summary />
    public bool? Reverse { get; set; }

    /// <summary />
    public int? Skip { get; set; }
}

/// <inheritdoc />
public class PathHierarchyTokenizerDescriptor
    : TokenizerDescriptorBase<PathHierarchyTokenizerDescriptor, IPathHierarchyTokenizer>, IPathHierarchyTokenizer
{
    protected override string Type => "path_hierarchy";

    int? IPathHierarchyTokenizer.BufferSize { get; set; }
    char? IPathHierarchyTokenizer.Delimiter { get; set; }
    char? IPathHierarchyTokenizer.Replacement { get; set; }
    bool? IPathHierarchyTokenizer.Reverse { get; set; }
    int? IPathHierarchyTokenizer.Skip { get; set; }

    /// <inheritdoc />
    public PathHierarchyTokenizerDescriptor BufferSize(int? bufferSize) => Assign(bufferSize, (a, v) => a.BufferSize = v);

    /// <inheritdoc />
    public PathHierarchyTokenizerDescriptor Skip(int? skip) => Assign(skip, (a, v) => a.Skip = v);

    /// <inheritdoc />
    public PathHierarchyTokenizerDescriptor Reverse(bool? reverse = true) => Assign(reverse, (a, v) => a.Reverse = v);

    /// <inheritdoc />
    public PathHierarchyTokenizerDescriptor Delimiter(char? delimiter) => Assign(delimiter, (a, v) => a.Delimiter = v);

    /// <inheritdoc />
    public PathHierarchyTokenizerDescriptor Replacement(char? replacement) =>
        Assign(replacement, (a, v) => a.Replacement = v);
}
