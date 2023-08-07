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

module Tests.YamlRunner.TestsLocator

open System
open System.IO
open System.IO.Compression
open Tests.YamlRunner.AsyncExtensions

type YamlFileInfo = { File: string; Yaml: string }

let TestLocalFile file =
    let yaml = System.IO.File.ReadAllText file
    { File = file; Yaml = yaml }

type LocateResults = { Folder: string; Paths: YamlFileInfo list } 

let TemporaryPath revision suite = lazy(Path.Combine(Path.GetTempPath(), "opensearch", $"tests-%s{suite}-%s{revision}"))

let ExtractTestsInFolder folder (entries:seq<string * ZipArchiveEntry>) fileFilter namedSuite revision = async {
    let parent = (TemporaryPath revision "_").Force()
    let directory = Path.Combine(parent, folder)
    
    let cachedOrExtract file (entry: ZipArchiveEntry) = async {
        let file = Path.Combine(directory, file)
        let fileExists = File.Exists file
        let directoryExists = Directory.Exists directory
        let! result = async {
            match (fileExists, directoryExists) with
            | true, _ ->
                let! text = Async.AwaitTask <| File.ReadAllTextAsync file
                return text
            | _, d ->
                if (not d) then Directory.CreateDirectory(directory) |> ignore
                use zipStream = new StreamReader(entry.Open())
                let! contents = zipStream.ReadToEndAsync() |> Async.AwaitTask
                do! File.WriteAllTextAsync(file, contents) |> Async.AwaitTask
                return contents
        }
        return (file, result)
    }
    
    let! localFiles = async {
        let actions =
            entries
            |> Seq.filter (fun (file, _) -> match fileFilter with | Some f -> file.StartsWith(f, StringComparison.OrdinalIgnoreCase) | None -> true)
            |> Seq.map (fun (file, entry) -> async {
                let! localFile, yaml = cachedOrExtract file entry
                match String.IsNullOrWhiteSpace yaml with
                | true ->
                    return None
                | _ ->
                    return Some {File = localFile; Yaml = yaml}
            })
            |> Seq.toList
            
        let! completed = Async.ForEachAsync 1 actions
        let files = completed |> List.choose id;
        return files 
    }
    
    return { Folder = folder; Paths = localFiles }
}
