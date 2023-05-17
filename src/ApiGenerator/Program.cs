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
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiGenerator.Configuration;
using Spectre.Console;

#pragma warning disable 162

namespace ApiGenerator;

public static class Program
{
    private static bool Interactive { get; set; }

    /// <summary>
    /// A main function can also take <see cref="CancellationToken"/> which is hooked up to support termination (e.g CTRL+C)
    /// </summary>
    /// <param name="interactive">Run the generation interactively, this will ignore all flags</param>
    /// <param name="download">Whether to download the specs or use an already downloaded copy</param>
    /// <param name="includeHighLevel">Also generate the high level client (OpenSearch.Client)</param>
    /// <param name="skipGenerate">Only download the specs, skip all code generation</param>
    /// <param name="namespaces">Namespaces to generate</param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async Task<int> Main(
        bool interactive = false, bool download = false, bool includeHighLevel = false, bool skipGenerate = false,
        string[] namespaces = null, CancellationToken token = default
    )
    {
        Interactive = interactive;
        try
        {
            await Generate(download, includeHighLevel, skipGenerate, namespaces ?? Array.Empty<string>(), token);
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[b white on orange4_1] Cancelled [/]").LeftJustified());
            AnsiConsole.WriteLine();
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[b white on darkred] Exception [/]").LeftJustified());
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(ex);
            return 1;
        }
        return 0;
    }

    private static async Task Generate(bool download, bool includeHighLevel, bool skipGenerate, string[] namespaces, CancellationToken token = default)
    {
        var generateCode = Ask("Generate code from the specification files on disk?", !skipGenerate);
        var lowLevelOnly = generateCode && Ask("Generate low level client only?", !includeHighLevel);

        static string YesNo(bool value)
        {
            return value ? "[bold green]Yes[/]" : "[grey]No[/]";
        }

        var grid = new Grid()
            .AddColumn(new GridColumn().PadRight(4))
            .AddColumn()
            .AddRow("[b]Download specification[/]", $"{YesNo(download)}")
            .AddRow("[b]Generate code from specification[/]", $"{YesNo(generateCode)}")
            .AddRow("[b]Include high level client[/]", $"{YesNo(!lowLevelOnly)}");

        Console.WriteLine();
        AnsiConsole.Write(
            new Panel(grid)
                .Header(new PanelHeader("[b white on chartreuse4] OpenSearch .NET client API generator [/]", Justify.Left))
        );
        Console.WriteLine();

        if (!generateCode) return;

        Console.WriteLine();
        AnsiConsole.Write(new Rule("[b white on chartreuse4] Loading specification [/]").LeftJustified());
        Console.WriteLine();

        var spec = await Generator.ApiGenerator.CreateRestApiSpecModel("/Users/tsfarr/Development/opensearch-api-specification/build/smithyprojections/opensearch-api-specification/full/openapi/OpenSearch.openapi.json", namespaces.ToImmutableHashSet());

        Console.WriteLine();
        AnsiConsole.Write(new Rule("[b white on chartreuse4] Generating code [/]").LeftJustified());
        Console.WriteLine();

        await Generator.ApiGenerator.Generate(lowLevelOnly, spec, token);

        var warnings = Generator.ApiGenerator.Warnings;
        if (warnings.Count > 0)
        {
            Console.WriteLine();
            AnsiConsole.Write(new Rule("[b black on yellow] Specification warnings [/]").LeftJustified());
            Console.WriteLine();

            foreach (var warning in warnings.Distinct().OrderBy(w => w))
                AnsiConsole.MarkupLine(" {0} [yellow] {1} [/] ", Emoji.Known.Warning, warning);
        }
    }

    private static bool Ask(string question, bool defaultAnswer = true)
    {
        if (!Interactive) return defaultAnswer;

        var answer = "invalid";
        var defaultResponse = defaultAnswer ? "y" : "n";

        while (answer != "y" && answer != "n" && answer != "")
        {
            Console.Write($"{question}[y/N] (default {defaultResponse}): ");
            answer = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(answer)) answer = defaultResponse;
            defaultAnswer = answer == "y";
        }
        return defaultAnswer;
    }
}
