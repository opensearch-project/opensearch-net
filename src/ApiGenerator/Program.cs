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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiGenerator.Configuration;
using ProcNet;
using Spectre.Console;

#pragma warning disable 162

namespace ApiGenerator
{
    public static class Program
    {
        private static bool Interactive { get; set; } = false;

        /// <summary>
        /// A main function can also take <see cref="CancellationToken"/> which is hooked up to support termination (e.g CTRL+C)
        /// </summary>
        /// <param name="branch">The stack's branch we are targeting the generation for</param>
        /// <param name="interactive">Run the generation interactively, this will ignore all flags</param>
        /// <param name="download">Whether to download the specs or use an already downloaded copy</param>
        /// <param name="includeHighLevel">Also generate the high level client (OpenSearch.Client)</param>
        /// <param name="skipGenerate">Only download the specs, skip all code generation</param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task<int> Main(
            string branch, bool interactive = false, bool download = false, bool includeHighLevel = false, bool skipGenerate = false
            , CancellationToken token = default)
        {
            Interactive = interactive;
            try
            {
                if (string.IsNullOrEmpty(branch))
                {

                    throw new ArgumentException("--branch can not be null");
                }
                await Generate(download, branch, includeHighLevel, skipGenerate, token);
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
                AnsiConsole.WriteException(ex, ExceptionFormats.ShowLinks);
                return 1;
            }
            return 0;
        }

        private static async Task<int> Generate(bool download, string branch, bool includeHighLevel, bool skipGenerate, CancellationToken token = default)
        {
            var redownloadCoreSpecification = Ask("Download online rest specifications?", download);

            var downloadBranch = branch;
            if (Interactive && redownloadCoreSpecification)
            {
                Console.Write($"Branch to download specification from (default {downloadBranch}): ");
                var readBranch = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(readBranch))
                    downloadBranch = readBranch;
            }

            if (string.IsNullOrEmpty(downloadBranch))
                throw new Exception($"Branch to download from is null or empty");

            var generateCode = Ask("Generate code from the specification files on disk?", !skipGenerate);
            var lowLevelOnly = generateCode && Ask("Generate low level client only?", !includeHighLevel);

            static string YesNo(bool value) => value ? "[bold green]Yes[/]" : "[grey]No[/]";
            var grid = new Grid()
                .AddColumn(new GridColumn().PadRight(4))
                .AddColumn()
                .AddRow("[b]Download specification[/]", $"{YesNo(download)}")
                .AddRow("[b]Download branch[/]", $"{downloadBranch}")
                .AddRow("[b]Generate code from specification[/]", $"{YesNo(generateCode)}")
                .AddRow("[b]Include high level client[/]", $"{YesNo(!lowLevelOnly)}");

            Console.WriteLine();
            AnsiConsole.Write(
                new Panel(grid)
                    .Header(new PanelHeader("[b white on chartreuse4] OpenSearch .NET client API generator [/]", Justify.Left))
            );
            Console.WriteLine();

            if (redownloadCoreSpecification)
            {
                Console.WriteLine();
                AnsiConsole.Write(new Rule("[b white on chartreuse4] Downloading specification [/]").LeftJustified());
                Console.WriteLine();
                await RestSpecDownloader.DownloadAsync(downloadBranch, token);
            }

            if (!generateCode)
                return 0;

            Console.WriteLine();
            AnsiConsole.Write(new Rule("[b white on chartreuse4] Loading specification [/]").LeftJustified());
            Console.WriteLine();

            var spec = await Generator.ApiGenerator.CreateRestApiSpecModel(token);
            if (!lowLevelOnly)
            {
                foreach (var endpoint in spec.Endpoints.Select(e => e.Value.Name))
                {
                    if (CodeConfiguration.IsNewHighLevelApi(endpoint)
                        && Ask($"Generate highlevel code for new api {endpoint}", false))
                        CodeConfiguration.EnableHighLevelCodeGen.Add(endpoint);

                }
            }

            Console.WriteLine();
            AnsiConsole.Write(new Rule("[b white on chartreuse4] Generating code [/]").LeftJustified());
            Console.WriteLine();

            await Generator.ApiGenerator.Generate(lowLevelOnly, spec, token);

            RunDotNetFormat();

            var warnings = Generator.ApiGenerator.Warnings;
            if (warnings.Count > 0)
            {
                Console.WriteLine();
                AnsiConsole.Write(new Rule("[b black on yellow] Specification warnings [/]").LeftJustified());
                Console.WriteLine();

                foreach (var warning in warnings.Distinct().OrderBy(w => w))
                    AnsiConsole.MarkupLine(" {0} [yellow] {1} [/] ", Emoji.Known.Warning, warning);
            }

            return 0;
        }

        private static void RunDotNetFormat()
        {
            Console.WriteLine();
            AnsiConsole.Write(new Rule("[b white on chartreuse4] Formatting Code using dotnet format [/]").LeftJustified());
            Console.WriteLine();

            Proc.Exec(new ExecArguments("dotnet", "format", "--verbosity", "normal", "--include", "./src/*/_Generated/")
            {
                WorkingDirectory = GeneratorLocations.SolutionFolder
            });
        }

        private static bool Ask(string question, bool defaultAnswer = true)
        {
            if (!Interactive)
                return defaultAnswer;

            var answer = "invalid";
            var defaultResponse = defaultAnswer ? "y" : "n";

            while (answer != "y" && answer != "n" && answer != "")
            {
                Console.Write($"{question}[y/N] (default {defaultResponse}): ");
                answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(answer))
                    answer = defaultResponse;
                defaultAnswer = answer == "y";
            }
            return defaultAnswer;
        }
    }
}
