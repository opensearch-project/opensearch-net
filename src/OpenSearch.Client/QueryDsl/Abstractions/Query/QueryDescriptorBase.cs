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

namespace OpenSearch.Client;

public abstract class QueryDescriptorBase<TDescriptor, TInterface>
    : DescriptorBase<TDescriptor, TInterface>, IQuery
    where TDescriptor : QueryDescriptorBase<TDescriptor, TInterface>, TInterface
    where TInterface : class, IQuery
{
    /// <inheritdoc cref="IQuery.Conditionless"/>
    protected abstract bool Conditionless { get; }

    double? IQuery.Boost { get; set; }

    bool IQuery.Conditionless => Conditionless;

    bool IQuery.IsStrict { get; set; }

    bool IQuery.IsVerbatim { get; set; }

    bool IQuery.IsWritable => Self.IsVerbatim || !Self.Conditionless;
    string IQuery.Name { get; set; }

    /// <inheritdoc cref="IQuery.Name"/>
    public TDescriptor Name(string name) => Assign(name, (a, v) => a.Name = v);

    /// <inheritdoc cref="IQuery.Boost"/>
    public TDescriptor Boost(double? boost) => Assign(boost, (a, v) => a.Boost = v);

    /// <inheritdoc cref="IQuery.IsVerbatim"/>
    public TDescriptor Verbatim(bool verbatim = true) => Assign(verbatim, (a, v) => a.IsVerbatim = v);

    /// <inheritdoc cref="IQuery.IsStrict"/>
    public TDescriptor Strict(bool strict = true) => Assign(strict, (a, v) => a.IsStrict = v);
}
