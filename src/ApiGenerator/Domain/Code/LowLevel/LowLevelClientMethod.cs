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
using System.Text.RegularExpressions;
using ApiGenerator.Domain.Specification;
using SemanticVersioning;

namespace ApiGenerator.Domain.Code.LowLevel
{
    public class LowLevelClientMethod
    {
        public CsharpNames CsharpNames { get; set; }

        public string Arguments { get; set; }
        public string OfficialDocumentationLink { get; set; }

        public Stability Stability { get; set; }
        public string PerPathMethodName { get; set; }
        public string HttpMethod { get; set; }

        public Deprecation Deprecation { get; set; }
        public UrlInformation Url { get; set; }
        public bool HasBody { get; set; }
        public IEnumerable<UrlPart> Parts { get; set; }
        public string Path { get; set; }

        public Version VersionAdded { get; set; }

        public string UrlInCode
        {
            get
            {
                var url = Path.TrimStart('/');
                var options = Url.AllPaths.SelectMany(p => p.Parts).Select(p => p.Name).Distinct();

                var pattern = string.Join("|", options);
                var urlCode = $"\"{url}\"";
                if (!Path.Contains('{')) return urlCode;

                var patchedUrl = Regex.Replace(url, "{(" + pattern + ")}", m =>
                {
                    var arg = m.Groups[^1].Value.ToCamelCase();
                    return $"{{{arg}:{arg}}}";
                });
                return $"Url($\"{patchedUrl}\")";
            }
        }

        public string MapsApiArguments { get; set; }
    }
}
