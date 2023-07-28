// SPDX-License-Identifier: Apache-2.0
//
// The OpenSearch Contributors require contributions made to
// this file be licensed under the Apache-2.0 license or a
// compatible open source license.
//
// Modifications Copyright OpenSearch Contributors. See
// GitHub history for details.
//
//  Licensed to Elasticsearch B.V. under one or more contributor
//  license agreements. See the NOTICE file distributed with
//  this work for additional information regarding copyright
//  ownership. Elasticsearch B.V. licenses this file to you under
//  the Apache License, Version 2.0 (the "License"); you may
//  not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//  under the License.
//

module Tests.YamlRunner.Commands

open System
open System.IO.Compression
open FSharp.Data
open ShellProgressBar
open Tests.YamlRunner.AsyncExtensions
open Tests.YamlRunner.TestsLocator
open Tests.YamlRunner.TestsReader
open Tests.YamlRunner

let private barOptions = 
    ProgressBarOptions(
       ForegroundColor = ConsoleColor.Cyan,
       ForegroundColorDone = Nullable ConsoleColor.DarkGreen,
       BackgroundColor = Nullable ConsoleColor.DarkGray,
       ProgressCharacter = '─',
       CollapseWhenFinished = true
    )
let private subBarOptions = 
    ProgressBarOptions(
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = Nullable ConsoleColor.DarkGreen,
        ProgressCharacter = '─',
        CollapseWhenFinished = true
    )

let LocateTests namedSuite revision directoryFilter fileFilter = async {
    let! response = Http.AsyncRequestStream(
        $"https://api.github.com/repos/opensearch-project/OpenSearch/zipball/%s{revision}",
        headers=[
            "User-Agent", "OpenSearch .NET YAML Tests"
        ]
    )
    use zip = new ZipArchive(response.ResponseStream, ZipArchiveMode.Read)
    
    let folders =
        let testDir = "rest-api-spec/src/main/resources/rest-api-spec/test/"
        let trimParent (p:string) =
            p.Substring(p.IndexOf('/')+1)
            
        zip.Entries
        |> Seq.map (fun e -> e, trimParent e.FullName)
        |> Seq.filter (fun (_, p) -> p.StartsWith(testDir) && p.EndsWith(".yml"))
        |> Seq.map (fun (e, p) -> e, p.Substring(testDir.Length))
        |> Seq.map (fun (e, p) ->
                let parts = p.Split('/')
                parts[0], parts[1], e
            )
        |> Seq.groupBy (fun (folder, _, _) -> folder)
        |> Seq.filter (fun (folder, _) -> match directoryFilter with | Some d -> folder.StartsWith(d, StringComparison.OrdinalIgnoreCase) | None -> true)
        |> Seq.map (fun (folder, entries) ->
            ExtractTestsInFolder folder (entries |> Seq.map (fun (_, f, e) -> f, e)) fileFilter namedSuite revision
            )
    
    let! completed = Async.ForEachAsync 1 folders
    
    return completed
}

let ReadTests (tests:LocateResults list) = 
    let safeRead yamlInfo = 
        try
            ReadYamlFile yamlInfo |> Some
        with
        | ex -> 
            printfn "%s" ex.Message
            None

    let readPaths paths = paths |> List.map safeRead |> List.filter Option.isSome |> List.map Option.get
    
    tests |> List.map (fun t -> { Folder= t.Folder; Files = readPaths t.Paths})
    
let RunTests (tests:YamlTestFolder list) client version namedSuite sectionFilter = async {
    do! Async.SwitchToNewThread()
    
    let f = tests.Length
    let l = tests |> List.sumBy (fun t -> t.Files.Length)
    use progress = new ProgressBar(l, sprintf "Folders [0/%i]" l, barOptions)
    let runner = TestRunner(client, version, namedSuite, progress, subBarOptions)
    let a (i, v) = async {
        let mainMessage = sprintf "[%i/%i] Folders : %s | " (i+1) f v.Folder
        let! op = runner.RunTestsInFolder mainMessage v sectionFilter
        return v, op |> Seq.toList
    }
    let x =
        tests
        |> Seq.indexed
        |> Seq.map a
        |> Seq.map Async.RunSynchronously
        |> Seq.toList
    
    progress.Message <- sprintf "[%i/%i] Folders [%i/%i] Files" f f l l
        
    return x
}

let ExportTests = TestsExporter.Export

let PrettyPrintResults = TestsExporter.PrettyPrintResults 

