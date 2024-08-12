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
using ApiGenerator.Domain.Code.HighLevel.Requests;
using Version = SemanticVersioning.Version;

namespace ApiGenerator.Generator;

//TODO this should be in views and models
public static class CodeGenerator
{
    public static string CatFormatPropertyGenerator(string type, string name, string key, string setter) =>
          $"public {type} {name} {{ "
        + $"    get => Q<{type}>(\"{key}\");"
        + $"    set {{ Q(\"{key}\", {setter}); SetAcceptHeader({setter}); }}"
        + $"}}";

    public static string PropertyGenerator(string type, string name, string key, string setter) =>
        $"public {type} {name} {{ get => Q<{type}>(\"{key}\"); set => Q(\"{key}\", {setter}); }}";

    public static string Property(string @namespace, string type, string name, string key, string setter, string obsolete, Version versionAdded, params string[] doc)
    {
        var components = new List<string>();
        foreach (var d in RenderDocumentation(doc)) A(d);
        if (versionAdded != null) A($"/// <remarks>Supported by OpenSearch servers of version {versionAdded} or greater.</remarks>");
        if (!string.IsNullOrWhiteSpace(obsolete)) A($"[Obsolete(\"{obsolete}\")]");

        var generated = @namespace != null && @namespace == "Cat" && name == "Format"
            ? CatFormatPropertyGenerator(type, name, key, setter)
            : PropertyGenerator(type, name, key, setter);

        A(generated);
        return string.Join($"{Environment.NewLine}\t\t", components);

        void A(string s)
        {
            components.Add(s);
        }
    }

    public static string Constructor(Constructor c)
    {
        var components = new List<string>();
        if (!c.Description.IsNullOrEmpty()) A(c.Description);
        var generated = c.Generated;
        if (c.Body.IsNullOrEmpty()) generated += "{}";
        A(generated);
        if (!c.Body.IsNullOrEmpty()) A(c.Body);
        if (!c.AdditionalCode.IsNullOrEmpty()) A(c.AdditionalCode);
        return string.Join($"{Environment.NewLine}\t\t", components);

        void A(string s)
        {
            components.Add(s);
        }
    }

    private static IEnumerable<string> RenderDocumentation(params string[] doc)
    {
        doc = (doc?.SelectMany(WrapDocumentation) ?? Enumerable.Empty<string>()).ToArray();
        switch (doc.Length)
        {
            case 0: yield break;
            case 1:
                yield return $"/// <summary>{doc[0]}</summary>";

                yield break;
            default:
                yield return "/// <summary>";

                foreach (var d in doc) yield return $"/// {d}";

                yield return "/// </summary>";

                yield break;
        }
    }

    private static string[] WrapDocumentation(string documentation)
    {
        if (string.IsNullOrWhiteSpace(documentation)) return Array.Empty<string>();
        const int max = 140;
        var lines = documentation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var charCount = 0;
        return lines.GroupBy(Wrap).Select(l => string.Join(" ", l.ToArray())).ToArray();

        int Wrap(string w)
        {
            var increase = charCount % max + w.Length + 1 >= max ? max - charCount % max : 0;
            return (charCount += increase + w.Length + 1) / max;
        }
    }
}
