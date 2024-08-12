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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json.Linq;

namespace Tests.Core.Extensions;

public static class DiffExtensions
{
    public static void DeepSort(this JToken token)
    {
        if (token == null) return;

        if (token is JObject jObj)
        {
            var props = jObj.Properties().ToList();
            foreach (var prop in props) prop.Remove();

            foreach (var prop in props.OrderBy(p => p.Name))
            {
                jObj.Add(prop);
                prop.Value?.DeepSort();
            }
        }
        if (token is JArray jArray)
            foreach (var v in jArray.Values())
                v?.DeepSort();
        if (token is JProperty jProp)
            jProp.Value?.DeepSort();
    }

    public static string CreateCharacterDifference(this string expected, string actual, string message = null)
    {
        var d = new Differ();
        var result = d.CreateCharacterDiffs(expected.Trim('"'), actual.Trim('"'), false);
        if (!result.DiffBlocks.Any()) return string.Empty;

        var builder = new StringBuilder()
            .AppendLine(message)
            .AppendLine($"expect: \"{expected}\"")
            .AppendLine($"actual: \"{actual}\"")
            .AppendLine();

        //TODO come up with a clever way to dispay inline character differences.
        //Looked at the diffplex source but they don't have one either
        return builder.ToString();
    }

    public static string DiffNoApproximation(this string expected, string actual, string message = null) =>
        CreateDiff(expected, actual, message, new Differ());

    public static string Diff(this string expected, string actual, string message = null)
    {
        var d = new Differ();
        var diff = CreateDiff(expected, actual, message, d);
        return !string.IsNullOrWhiteSpace(diff) ? AppendCsharpApproximation(expected, actual, diff) : diff;
    }

    private static string CreateDiff(string expected, string actual, string message, Differ d)
    {
        var inlineBuilder = new InlineDiffBuilder(d);
        var result = inlineBuilder.BuildDiffModel(expected, actual);
        var hasChanges = result.Lines.Any(l => l.Type != ChangeType.Unchanged);
        if (!hasChanges) return string.Empty;

        return result.Lines.Aggregate(new StringBuilder().AppendLine(message), (sb, line) =>
        {
            if (line.Type == ChangeType.Inserted)
                sb.Append("+ ");
            else if (line.Type == ChangeType.Deleted)
                sb.Append("- ");
            else
                sb.Append("  ");
            sb.AppendLine(line.Text);
            return sb;
        }, sb => sb.ToString());
    }

    private static string AppendCsharpApproximation(string expected, string actual, string diff)
    {
        var eol = Environment.NewLine;
        diff += $"{eol} C# approximation of actual ------ ";
        diff += $"{eol} new ";
        var approx = Regex.Replace(actual, @"^(?=.*:.*)[^:]+:", (s) => s
                        .Value.Replace("\"", "")
                        .Replace(":", " =")
                    , RegexOptions.Multiline)
                .Replace(" = {", " = new {")
                .Replace(" = [", " = new [] {")
            ;
        approx = Regex.Replace(approx, @"^\s*\],?.*$", s => s.Value.Replace("]", "}"), RegexOptions.Multiline);
        diff += approx + ";";

        diff += $"{eol} C# approximation of expected ------ ";
        diff += $"{eol} new ";
        approx = Regex.Replace(expected, @"^(?=.*:.*)[^:]+:", (s) => s
                        .Value.Replace("\"", "")
                        .Replace(":", " =")
                    , RegexOptions.Multiline)
                .Replace(" = {", " = new {")
                .Replace(" = [", " = new [] {")
            ;
        approx = Regex.Replace(approx, @"^\s*\],?.*$", s => s.Value.Replace("]", "}"), RegexOptions.Multiline);
        diff += approx + ";";
        return diff;
    }
}
