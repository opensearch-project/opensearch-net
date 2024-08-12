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
*   http://www.apache.org/licenses/LICENSE-2.0
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

namespace ApiGenerator.Domain.Specification;


// ReSharper disable once ClassNeverInstantiated.Global
public class UrlInformation
{
    public IDictionary<string, QueryParameters> Params { get; set; } = new SortedDictionary<string, QueryParameters>();

    public IEnumerable<UrlPath> Paths => AllPaths.Where(p => p.Deprecation == null);
    public IEnumerable<UrlPath> DeprecatedPaths => AllPaths.Where(p => p.Deprecation != null);

    public IList<UrlPath> AllPaths = new List<UrlPath>();

    public IReadOnlyCollection<UrlPart> Parts => Paths
            .SelectMany(p => p.Parts)
            .DistinctBy(p => p.Name)
            .OrderBy(p => p.Name)
            .ToList();

    public bool IsPartless => !Parts.Any();

    private static readonly string[] DocumentApiParts = { "index", "id" };

    public bool IsDocumentApi => IsADocumentRoute(Parts);

    public static bool IsADocumentRoute(IReadOnlyCollection<UrlPart> parts) =>
        parts.Count == DocumentApiParts.Length
        && parts.All(p => DocumentApiParts.Contains(p.Name));


    public bool TryGetDocumentApiPath(out UrlPath path)
    {
        path = null;
        if (!IsDocumentApi) return false;

        var mostVerbosePath = Paths.OrderByDescending(p => p.Parts.Count).First();
        path = new UrlPath(mostVerbosePath.Path, mostVerbosePath.Parts, mostVerbosePath.Deprecation, mostVerbosePath.VersionAdded, mostVerbosePath.Parts);
        return true;
    }
}
