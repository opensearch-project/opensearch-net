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

using System;
using System.Collections.Generic;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(VerbatimDictionaryKeysFormatter<Normalizers, INormalizers, string, INormalizer>))]
public interface INormalizers : IIsADictionary<string, INormalizer> { }

public class Normalizers : IsADictionaryBase<string, INormalizer>, INormalizers
{
    public Normalizers() { }

    public Normalizers(IDictionary<string, INormalizer> container) : base(container) { }

    public Normalizers(Dictionary<string, INormalizer> container) : base(container) { }

    public void Add(string name, INormalizer analyzer) => BackingDictionary.Add(name, analyzer);
}

public class NormalizersDescriptor : IsADictionaryDescriptorBase<NormalizersDescriptor, INormalizers, string, INormalizer>
{
    public NormalizersDescriptor() : base(new Normalizers()) { }

    public NormalizersDescriptor UserDefined(string name, INormalizer analyzer) => Assign(name, analyzer);

    /// <summary>
    /// OpenSearch does not ship with built-in normalizers so far, so the only way to
    /// get one is by building a custom one. Custom normalizers take a list of char character
    /// filters and a list of token filters.
    /// </summary>
    public NormalizersDescriptor Custom(string name, Func<CustomNormalizerDescriptor, ICustomNormalizer> selector) =>
        Assign(name, selector?.Invoke(new CustomNormalizerDescriptor()));
}
