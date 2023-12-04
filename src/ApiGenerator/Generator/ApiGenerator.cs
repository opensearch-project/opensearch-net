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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiGenerator.Configuration;
using ApiGenerator.Domain;
using ApiGenerator.Generator.Razor;
using NJsonSchema;
using NSwag;
using ShellProgressBar;

namespace ApiGenerator.Generator
{
    public class ApiGenerator
    {
        public static List<string> Warnings { get; private set; } = new();

        public static async Task Generate(bool lowLevelOnly, RestApiSpec spec, CancellationToken token)
        {
			async Task DoGenerate(IReadOnlyCollection<RazorGeneratorBase> generators, bool highLevel)
            {
                var pbarOpts = new ProgressBarOptions { ProgressCharacter = '─', BackgroundColor = ConsoleColor.Yellow };
                var message = $"Generating {(highLevel ? "high" : "low")} level code";
                using var pbar = new ProgressBar(generators.Count, message, pbarOpts);
                foreach (var generator in generators)
                {
                    pbar.Message = "Generating " + generator.Title;
                    await generator.Generate(spec, pbar, token);
                    pbar.Tick("Generated " + generator.Title);
                }
            }

			RecursiveDelete(GeneratorLocations.LowLevelGeneratedFolder);
			await DoGenerate(
				new RazorGeneratorBase[] {
					//low level client
					new LowLevelClientInterfaceGenerator(),
					new LowLevelClientImplementationGenerator(),
					new RequestParametersGenerator(),
					new EnumsGenerator()
				},
				highLevel: false
			);

			if (lowLevelOnly) return;

			RecursiveDelete(GeneratorLocations.HighLevelGeneratedFolder);
			await DoGenerate(
				new RazorGeneratorBase[]
				{
					//high level client
					new ApiUrlsLookupsGenerator(),
					new HighLevelClientInterfaceGenerator(),
					new HighLevelClientImplementationGenerator(),
					new DescriptorsGenerator(),
					new RequestsGenerator(),
				},
				highLevel: true
			);
		}

		public static async Task<RestApiSpec> CreateRestApiSpecModel(CancellationToken token = default)
		{
			var document = await OpenApiYamlDocument.FromFileAsync(GeneratorLocations.OpenApiSpecFile, token);
            JsonSchemaReferenceUtilities.UpdateSchemaReferencePaths(document);

			var endpoints = document.Paths
				.Select(kv => new { HttpPath = kv.Key, PathItem = kv.Value })
				.SelectMany(p => p.PathItem.Select(kv => new { p.HttpPath, p.PathItem, HttpMethod = kv.Key, Operation = kv.Value }))
				.GroupBy(o => o.Operation.ExtensionData["x-operation-group"].ToString())
				.Where(o => CodeConfiguration.IncludeOperation(o.Key))
				.Select(o => ApiEndpointFactory.From(o.Key, o.Select(i => (i.HttpPath, i.PathItem, i.HttpMethod, i.Operation)).ToList()))
				.ToImmutableSortedDictionary(e => e.Name, e => e);

			return new RestApiSpec { Endpoints = endpoints };
		}

		private static void RecursiveDelete(string path)
		{
			if (!Directory.Exists(path)) return;

			Directory.Delete(path, true);
		}
    }
}
