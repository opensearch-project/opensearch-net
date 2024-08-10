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

namespace OpenSearch.Stack.ArtifactsApi.Products;

public class SubProduct
{
    private readonly Func<OpenSearchVersion, string> _getExistsMoniker;

    private readonly Func<OpenSearchVersion, bool> _isValid;

    public SubProduct(string subProject, Func<OpenSearchVersion, bool> isValid = null,
        Func<OpenSearchVersion, string> listName = null)
    {
        SubProductName = subProject;
        _isValid = isValid ?? (v => true);
        _getExistsMoniker = listName ?? (v => subProject);
    }

    public string SubProductName { get; }

    public OpenSearchVersion ShippedByDefaultAsOf { get; set; }

    public bool PlatformDependent { get; protected set; }

    /// <summary> what moniker to use when asserting the sub product is already present</summary>
    public string GetExistsMoniker(OpenSearchVersion version) => _getExistsMoniker(version);

    /// <summary>Whether the sub project is included in the distribution out of the box for the given version</summary>
    public bool IsIncludedOutOfTheBox(OpenSearchVersion version) =>
        ShippedByDefaultAsOf != null && version >= ShippedByDefaultAsOf;

    /// <summary>Whether the subProject is valid for the given version</summary>
    public bool IsValid(OpenSearchVersion version) => IsIncludedOutOfTheBox(version) || _isValid(version);

    public override string ToString() => SubProductName;
}
