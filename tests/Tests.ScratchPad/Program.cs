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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Diagnostics;
using Tests.Core.Client;
using Tests.Domain;

namespace Tests.ScratchPad;

public class Program
{
    private class ListenerObserver : IObserver<DiagnosticListener>
    {
        public void OnCompleted() => Console.WriteLine("Completed");

        public void OnError(Exception error) => Console.Error.WriteLine(error.Message);

        public void OnNext(DiagnosticListener value)
        {

            var client = new OpenSearchClient();

            client.Search<Project>();

            client.LowLevel.Search<SearchResponse<Project>>(PostData.Serializable(new SearchRequest()));


            void WriteToConsole<T>(string eventName, T data)
            {
                var a = Activity.Current;
                Console.WriteLine($"{eventName?.PadRight(30)} {a.Id?.PadRight(32)} {a.ParentId?.PadRight(32)} {data?.ToString().PadRight(10)}");
            }
            if (value.Name == DiagnosticSources.AuditTrailEvents.SourceName)
                value.Subscribe(new AuditDiagnosticObserver(v => WriteToConsole(v.Key, v.Value)));

            if (value.Name == DiagnosticSources.RequestPipeline.SourceName)
                value.Subscribe(new RequestPipelineDiagnosticObserver(
                    v => WriteToConsole(v.Key, v.Value),
                    v => WriteToConsole(v.Key, v.Value))
                );

            if (value.Name == DiagnosticSources.HttpConnection.SourceName)
                value.Subscribe(new HttpConnectionDiagnosticObserver(
                    v => WriteToConsole(v.Key, v.Value),
                    v => WriteToConsole(v.Key, v.Value)
                ));

            if (value.Name == DiagnosticSources.Serializer.SourceName)
                value.Subscribe(new SerializerDiagnosticObserver(v => WriteToConsole(v.Key, v.Value)));
        }
    }

    private static readonly IList<Project> Projects = Project.Generator.Clone().Generate(10000);
    private static readonly byte[] Response = TestClient.DefaultInMemoryClient.ConnectionSettings.RequestResponseSerializer.SerializeToBytes(ReturnBulkResponse(Projects));

    private static readonly IOpenSearchClient Client =
        new OpenSearchClient(new ConnectionSettings(new InMemoryConnection(Response, 200, null, null))
            .DefaultIndex("index")
            .EnableHttpCompression(false)
        );


    private static async Task Main(string[] args)
    {
        Console.Write($"Warmup...");
        var response = Client.Bulk(b => b.IndexMany(Projects));
        Console.WriteLine("\rWarmed up kicking off in 2 seconds!");

        await Task.Delay(TimeSpan.FromSeconds(2));
        Console.WriteLine($"Kicking off");

        for (var i = 0; i < 10_000; i++)
        {
            var r = Client.Bulk(b => b.IndexMany(Projects));
            Console.Write($"\r{i}: {r.IsValid} {r.Items.Count}");
        }
    }


    private static object BulkItemResponse(Project project) => new
    {
        index = new
        {
            _index = "osc-52cfd7aa",
            _type = "_doc",
            _id = project.Name,
            _version = 1,
            _shards = new
            {
                total = 2,
                successful = 1,
                failed = 0
            },
            created = true,
            status = 201
        }
    };

    private static object ReturnBulkResponse(IList<Project> projects) => new
    {
        took = 276,
        errors = false,
        items = projects
            .Select(p => BulkItemResponse(p))
            .ToArray()
    };

    private static void Bench<TBenchmark>() where TBenchmark : RunBase => BenchmarkRunner.Run<TBenchmark>();

    private static void Run<TRun>() where TRun : RunBase, new()
    {
        var runner = new TRun { IsNotBenchmark = true };
        runner.GlobalSetup();
        runner.Run();
    }

    private static void RunCreateOnce<TRun>() where TRun : RunBase, new()
    {
        var runner = new TRun { IsNotBenchmark = true };
        runner.GlobalSetup();
        runner.RunCreateOnce();
    }
}
