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

using System;
using System.Collections.Generic;
using System.Linq;
using ApiGenerator.Configuration;
using ApiGenerator.Domain.Specification;

namespace ApiGenerator.Domain.Code.HighLevel.Requests;

public class Constructor
{
    private static readonly string Indent = $"{Environment.NewLine}\t\t";
    public string AdditionalCode { get; set; } = string.Empty;
    public bool Parameterless { get; set; }
    public string Body { get; set; }
    public string Description { get; set; }
    public string Generated { get; set; }

    public static IEnumerable<Constructor> DescriptorConstructors(CsharpNames names, UrlInformation url)
    {
        var m = names.DescriptorName;
        var generic = FirstGeneric(names.GenericsDeclaredOnDescriptor);
        var generateGeneric = !string.IsNullOrEmpty(generic);
        return GenerateConstructors(url, true, generateGeneric, m, generic);
    }

    public static IEnumerable<Constructor> RequestConstructors(CsharpNames names, UrlInformation url, bool inheritsFromPlainRequestBase)
    {
        var generic = FirstGeneric(names.GenericsDeclaredOnRequest);
        var generateGeneric = CodeConfiguration.GenericOnlyInterfaces.Contains(names.RequestInterfaceName) || !inheritsFromPlainRequestBase;
        return GenerateConstructors(url, inheritsFromPlainRequestBase, generateGeneric, names.RequestName, generic);
    }

    private static string FirstGeneric(string fullGenericString) =>
        fullGenericString?.Replace("<", "").Replace(">", "").Split(",").First().Trim();

    private static IEnumerable<Constructor> GenerateConstructors(
        UrlInformation url,
        bool inheritsFromPlainRequestBase,
        bool generateGeneric,
        string typeName,
        string generic
    )
    {
        var ctors = new List<Constructor>();

        var paths = url.Paths.ToList();

        if (url.IsPartless) return ctors;

        ctors.AddRange(from path in paths
                       let baseArgs = inheritsFromPlainRequestBase ? path.RequestBaseArguments : path.TypedSubClassBaseArguments
                       let constParams = path.ConstructorArguments
                       let generated = $"public {typeName}({constParams}) : base({baseArgs})"
                       select new Constructor
                       {
                           Parameterless = string.IsNullOrEmpty(constParams),
                           Generated = generated,
                           Description = path.GetXmlDocs(Indent),
                           //Body = isDocumentApi ? $" => Q(\"routing\", new Routing(() => AutoRouteDocument()));" : string.Empty
                           Body = string.Empty
                       });

        if (generateGeneric && !string.IsNullOrWhiteSpace(generic))
        {
            ctors.AddRange(from path in paths.Where(path => path.HasResolvableArguments)
                           let baseArgs = path.AutoResolveBaseArguments(generic)
                           let constructorArgs = path.AutoResolveConstructorArguments
                           let baseOrThis = inheritsFromPlainRequestBase
                               ? "this"
                               : "base"
                           let generated = $"public {typeName}({constructorArgs}) : {baseOrThis}({baseArgs})"
                           select new Constructor
                           {
                               Parameterless = string.IsNullOrEmpty(constructorArgs),
                               Generated = generated,
                               Description = path.GetXmlDocs(Indent, skipResolvable: true),
                               Body = string.Empty
                           });

            if (url.TryGetDocumentApiPath(out var docPath))
            {
                var docPathBaseArgs = docPath.DocumentPathBaseArgument(generic);
                var docPathConstArgs = docPath.DocumentPathConstructorArgument(generic);
                ctors.Add(new Constructor
                {
                    Parameterless = string.IsNullOrEmpty(docPathConstArgs),
                    Generated = $"public {typeName}({docPathConstArgs}) : this({docPathBaseArgs})",

                    AdditionalCode = $"partial void DocumentFromPath({generic} document);",
                    Description = docPath.GetXmlDocs(Indent, skipResolvable: true, documentConstructor: true),
                    Body = "=> DocumentFromPath(documentWithId);"
                });
            }
        }
        var constructors = ctors.GroupBy(c => c.Generated.Split(new[] { ':' }, 2)[0]).Select(g => g.Last()).ToList();
        if (!constructors.Any(c => c.Parameterless))
            constructors.Add(new Constructor
            {
                Parameterless = true,
                Generated = $"protected {typeName}() : base()",
                Description =
                    $"/// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>{Indent}[SerializationConstructor]",
            });
        return constructors;
    }
}
