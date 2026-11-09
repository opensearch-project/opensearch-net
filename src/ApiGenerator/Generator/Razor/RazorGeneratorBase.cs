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
using System.Threading;
using System.Threading.Tasks;
using ApiGenerator.Domain;
using RazorLight;
using RazorLight.Generation;
using RazorLight.Razor;
using ShellProgressBar;

namespace ApiGenerator.Generator.Razor
{
    public abstract class RazorGeneratorBase
    {
        private static readonly RazorLightEngine Engine = new RazorLightEngineBuilder()
            .UseProject(new EmbeddedRazorProject(typeof(CodeTemplatePage<>).Assembly, "ApiGenerator.Views"))
            .SetOperatingAssembly(typeof(CodeTemplatePage<>).Assembly)
            .UseMemoryCachingProvider()
            .EnableDebugMode()
            .Build();

        protected static Task<string> RenderAsync<TModel>(string viewLocation, TModel model)
            => Engine.CompileRenderAsync(viewLocation, model);

        protected static async Task DoRazor<TModel>(TModel model, string viewLocation, string targetLocation, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var generated = await Engine.CompileRenderAsync(viewLocation, model);
                await WriteCsharpFile(targetLocation, generated);
            }
            catch (TemplateGenerationException e)
            {
                foreach (var d in e.Diagnostics)
                    Console.WriteLine(d.GetMessage());
                throw;
            }
        }

        protected async Task DoRazorDependantFiles<TModel>(
            ProgressBar pbar, IReadOnlyCollection<TModel> items, string viewLocation,
            Func<TModel, string> identifier, Func<string, string> target,
            CancellationToken token
            )
        {
            using var c = pbar.Spawn(items.Count, "Generating namespaces", new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ForegroundColor = ConsoleColor.Yellow
            });
            foreach (var item in items)
            {
                var id = identifier(item);
                var targetLocation = target(id);
                await DoRazor(item, viewLocation, targetLocation, token);
                c.Tick($"{Title}: {id}");
            }
        }

        // Formatting is applied to the whole output folder with `dotnet format` once generation completes,
        // see DotNetFormat.
        private static async Task WriteCsharpFile(string path, string contents)
        {
            contents = NormalizeBlankLines(contents);

            if (Directory.GetParent(path) is { Exists: false } dir)
                dir.Create();

            await File.WriteAllTextAsync(path, contents);
        }

        /// <summary>
        /// `dotnet format whitespace` fixes indentation and spacing but leaves blank lines alone, so collapse the
        /// blank lines that templates leave behind: no runs of blank lines, none right after an opening brace and
        /// none right before a closing brace.
        /// </summary>
        internal static string NormalizeBlankLines(string contents)
        {
            var lines = contents.Replace("\r\n", "\n").Split('\n');
            var result = new List<string>(lines.Length);
            var pendingBlank = false;
            foreach (var raw in lines)
            {
                var line = raw.TrimEnd();
                if (line.Length == 0)
                {
                    pendingBlank = result.Count > 0;
                    continue;
                }

                if (pendingBlank && !result[^1].EndsWith("{") && !line.TrimStart().StartsWith("}"))
                    result.Add(string.Empty);

                pendingBlank = false;
                result.Add(line);
            }

            return string.Join("\n", result) + "\n";
        }

        public abstract string Title { get; }
        public abstract Task Generate(RestApiSpec spec, ProgressBar progressBar, CancellationToken token);
    }
}
