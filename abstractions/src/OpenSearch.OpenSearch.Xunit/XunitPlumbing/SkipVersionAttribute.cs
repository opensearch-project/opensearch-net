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
using System.Linq;
using SemanticVersioning;

namespace OpenSearch.OpenSearch.Xunit.XunitPlumbing;

/// <summary>
///     An Xunit test that should be skipped for given OpenSearch versions, and a reason why.
/// </summary>
public class SkipVersionAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local
    // reason is used to allow the test its used on to self document why its been put in place
    public SkipVersionAttribute(string skipVersionRangesSeparatedByComma, string reason)
    {
        Reason = reason;
        Ranges = string.IsNullOrEmpty(skipVersionRangesSeparatedByComma)
            ? new List<Range>()
            : skipVersionRangesSeparatedByComma.Split(',')
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => new Range(r))
                .ToList();
    }

    /// <summary>
    ///     The reason why the test should be skipped
    /// </summary>
    public string Reason { get; }

    /// <summary>
    ///     The version ranges for which the test should be skipped
    /// </summary>
    public IList<Range> Ranges { get; }
}
