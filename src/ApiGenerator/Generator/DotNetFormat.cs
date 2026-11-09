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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ApiGenerator.Generator
{
    /// <summary>
    /// Formats generated code with <c>dotnet format</c> so it follows the repository's <c>.editorconfig</c>,
    /// the same way the rest of the codebase is formatted.
    /// </summary>
    public static class DotNetFormat
    {
        public static async Task FormatFolderAsync(string folder, CancellationToken token)
        {
            var fullPath = Path.GetFullPath(folder);
            var startInfo = new ProcessStartInfo("dotnet")
            {
                // --include-generated: plugin models are emitted as *.g.cs, which dotnet format skips by default
                ArgumentList = { "format", "whitespace", fullPath, "--folder", "--include-generated" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Failed to start `dotnet format`");
            var stdout = process.StandardOutput.ReadToEndAsync(token);
            var stderr = process.StandardError.ReadToEndAsync(token);
            await process.WaitForExitAsync(token);

            if (process.ExitCode != 0)
                throw new InvalidOperationException(
                    $"`dotnet format` failed for {fullPath} with exit code {process.ExitCode}:{Environment.NewLine}{await stdout}{await stderr}");
        }
    }
}
