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
using ApiGenerator.Domain.Specification;

namespace ApiGenerator.Domain.Code.HighLevel.Requests;

public class DescriptorPartialImplementation
{
    public CsharpNames CsharpNames { get; set; }
    public string OfficialDocumentationLink { get; set; }
    public IReadOnlyCollection<Constructor> Constructors { get; set; }
    public IReadOnlyCollection<UrlPart> Parts { get; set; }
    public IReadOnlyCollection<UrlPath> Paths { get; set; }
    public IReadOnlyCollection<QueryParameters> Params { get; set; }
    public bool HasBody { get; set; }

    public IEnumerable<FluentRouteSetter> GetFluentRouteSetters()
    {
        var setters = new List<FluentRouteSetter>();
        if (!Parts.Any()) return setters;

        var alwaysGenerate = new[] { "index" };
        var parts = Parts
            .Where(p => !p.Required || alwaysGenerate.Contains(p.Name))
            .Where(p => !string.IsNullOrEmpty(p.Name))
            .ToList();
        var returnType = CsharpNames.GenericOrNonGenericDescriptorName;
        foreach (var part in parts)
        {
            var paramName = part.Name.ToPascalCase();
            paramName = paramName.Length > 1
                ? paramName[..1].ToLowerInvariant() + paramName[1..]
                : paramName.ToLowerInvariant();

            var routeSetter = part.Required ? "Required" : "Optional";

            var code =
                $"public {returnType} {part.InterfaceName}({part.HighLevelTypeName} {paramName}) => Assign({paramName}, (a,v)=>a.RouteValues.{routeSetter}(\"{part.Name}\", v));";
            var xmlDoc = $"///<summary>{part.Description}</summary>";
            setters.Add(new FluentRouteSetter { Code = code, XmlDoc = xmlDoc });
            switch (paramName)
            {
                case "index":
                {
                    code = $"public {returnType} {part.InterfaceName}<TOther>() where TOther : class ";
                    code += $"=> Assign(typeof(TOther), (a,v)=>a.RouteValues.{routeSetter}(\"{part.Name}\", ({part.HighLevelTypeName})v));";
                    xmlDoc = $"///<summary>a shortcut into calling {part.InterfaceName}(typeof(TOther))</summary>";
                    setters.Add(new FluentRouteSetter { Code = code, XmlDoc = xmlDoc });

                    if (part.Type == "list")
                    {
                        code = $"public {returnType} AllIndices() => Index(Indices.All);";
                        xmlDoc = "///<summary>A shortcut into calling Index(Indices.All)</summary>";
                        setters.Add(new FluentRouteSetter { Code = code, XmlDoc = xmlDoc });
                    }
                    break;
                }
                case "fields" when part.Type == "list":
                    code = $"public {returnType} Fields<T>(params Expression<Func<T, object>>[] fields) ";
                    code += $"=> Assign(fields, (a,v)=>a.RouteValues.{routeSetter}(\"fields\", (Fields)v));";
                    xmlDoc = $"///<summary>{part.Description}</summary>";
                    setters.Add(new FluentRouteSetter { Code = code, XmlDoc = xmlDoc });
                    break;
            }
        }
        return setters;
    }
}
