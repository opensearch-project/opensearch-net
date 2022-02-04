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
open System.Threading
open FSharp.Data
open Tests.YamlRunner.AsyncExtensions
open ShellProgressBar
open Tests.YamlRunner


let ListFolders namedSuite revision directory = async {
    let url = TestsDownloader.TestGithubRootUrl namedSuite revision
    let! (_, html) = TestsDownloader.CachedOrDownload namedSuite revision "_root_" "index.html" url 
    let doc = HtmlDocument.Parse(html)
    
    return
        doc.CssSelect("div.js-details-container a.js-navigation-open")
        |> List.map (fun a -> a.InnerText())
        |> List.filter (fun f -> match directory with | Some s -> f.StartsWith(s, StringComparison.OrdinalIgnoreCase) | None -> true)
        |> List.filter(fun f -> f.Replace(" ", "") <> "..")
        |> List.filter (fun f -> not <| f.EndsWith(".asciidoc"))
}
    
let ListFolderFiles namedSuite revision folder fileFilter = async { 
    let url = TestsDownloader.FolderListUrl namedSuite revision folder
    let! (_, html) =  TestsDownloader.CachedOrDownload namedSuite revision folder "index.html" url 
    let doc = HtmlDocument.Parse(html)
    let yamlFiles =
        let fileUrl file = (file, TestsDownloader.TestRawUrl namedSuite revision folder file)
        doc.CssSelect("div.js-details-container a.js-navigation-open")
        |> List.map(fun a -> a.InnerText())
        |> List.filter(fun f -> f.EndsWith(".yml"))
        |> List.filter (fun f -> match fileFilter with | Some s -> f.StartsWith(s, StringComparison.OrdinalIgnoreCase) | None -> true)
        |> List.map fileUrl
    return yamlFiles
}

type YamlFileInfo = { File: string; Yaml: string }

let TestLocalFile file =
    let yaml = System.IO.File.ReadAllText file
    { File = file; Yaml = yaml }

let private downloadTestsInFolder (yamlFiles:list<string * string>) folder namedSuite revision (progress: IProgressBar) subBarOptions = async {
    let mutable seenFiles = 0;
    use filesProgress = progress.Spawn(yamlFiles.Length, sprintf "Downloading [0/%i] files in %s" yamlFiles.Length folder, subBarOptions)
    let actions =
        yamlFiles
        |> Seq.map (fun (file, url) -> async {
            let! (localFile, yaml) =  TestsDownloader.CachedOrDownload namedSuite revision folder file url
            let i = Interlocked.Increment (&seenFiles)
            let message = sprintf "Downloaded [%i/%i] files in %s" i yamlFiles.Length folder
            filesProgress.Tick(message)
            match String.IsNullOrWhiteSpace yaml with
            | true ->
                progress.WriteLine(sprintf "Skipped %s since it returned no data" url)
                return None
            | _ ->
                return Some {File = localFile; Yaml = yaml}
        })
        |> Seq.toList
        
    let! completed = Async.ForEachAsync 4 actions
    let files = completed |> List.choose id;
    return files 
}

type LocateResults = { Folder: string; Paths: YamlFileInfo list } 

let DownloadTestsInFolder folder fileFilter namedSuite revision (progress: IProgressBar) subBarOptions = async {
    let! token = Async.StartChild <| ListFolderFiles namedSuite revision folder fileFilter
    let! yamlFiles = token
    let! localFiles = async {
       match yamlFiles.Length with
       | 0 ->
           progress.WriteLine(sprintf "%s folder yielded no tests (fileFilter: %O)" folder fileFilter)
           return List.empty
       | _ ->
           let! result = downloadTestsInFolder yamlFiles folder namedSuite revision progress subBarOptions
           return result
    }
    progress.Tick()
    return { Folder = folder; Paths = localFiles }
}
