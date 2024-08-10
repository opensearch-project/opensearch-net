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
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiGenerator.Configuration;
using ApiGenerator.Domain;
using ApiGenerator.Domain.Code;
using ApiGenerator.Domain.Specification;
using ApiGenerator.Generator.Razor;
using NJsonSchema;
using NSwag;
using ShellProgressBar;
using YamlDotNet.Serialization;

namespace ApiGenerator.Generator;

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
        var json = PreprocessRawOpenApiSpec(await File.ReadAllTextAsync(GeneratorLocations.OpenApiSpecFile, token));
        var document = await OpenApiDocument.FromJsonAsync(json, token);
        JsonSchemaReferenceUtilities.UpdateSchemaReferencePaths(document);

        var enumsToGenerate = new Dictionary<string, bool>();

        var endpoints = document.Paths
            .Select(kv => new { HttpPath = kv.Key, PathItem = kv.Value })
            .SelectMany(p => p.PathItem.Select(kv => new
            {
                p.HttpPath,
                p.PathItem,
                HttpMethod = kv.Key,
                Operation = kv.Value
            }))
            .GroupBy(o => o.Operation.ExtensionData!["x-operation-group"]!.ToString())
            .Where(o => CodeConfiguration.IncludeOperation(o.Key))
            .Select(o => ApiEndpointFactory.From(
                o.Key,
                o.Select(i => (i.HttpPath, i.PathItem, i.HttpMethod, i.Operation)).ToList(),
                (e, isFlag) =>
                {
                    if (enumsToGenerate.TryGetValue(e, out var f)) isFlag |= f;
                    enumsToGenerate[e] = isFlag;
                }))
            .ToImmutableSortedDictionary(e => e.Name, e => e);

        var enumsInSpec = enumsToGenerate.Select(kvp => new EnumDescription
        {
            Name = CsharpNames.GetEnumName(kvp.Key),
            IsFlag = kvp.Value,
            Options = document.Components.Schemas[kvp.Key].Enumeration.Where(e => e != null).Select(e => e.ToString()).ToImmutableList()
        })
            .OrderBy(e => e.Name)
            .ToImmutableList();

        return new RestApiSpec { Endpoints = endpoints, EnumsInTheSpec = enumsInSpec };
    }

    private static string PreprocessRawOpenApiSpec(string yaml)
    {
        // FIXME: work-around until NSwag adds support for requestBody references: https://github.com/RicoSuter/NSwag/pull/4747
        dynamic doc = new DeserializerBuilder().Build().Deserialize(yaml)!;
        var requestBodies = doc["components"]["requestBodies"];
        foreach (KeyValuePair<object, dynamic> pathPair in doc["paths"])
        {
            foreach (KeyValuePair<object, dynamic> operationPair in pathPair.Value)
            {
                var operation = (Dictionary<object, dynamic>)operationPair.Value;
                if (!operation.TryGetValue("requestBody", out var rb)) continue;

                var requestBody = (Dictionary<object, dynamic>)rb;
                if (!requestBody.TryGetValue("$ref", out var reference)) continue;

                operation["requestBody"] = requestBodies[((string)reference).Split('/').Last()];
            }
        }
        return new SerializerBuilder()
            .JsonCompatible()
            .Build()
            .Serialize(doc);
    }

    private static void RecursiveDelete(string path)
    {
        if (!Directory.Exists(path)) return;

        Directory.Delete(path, true);
    }
}
