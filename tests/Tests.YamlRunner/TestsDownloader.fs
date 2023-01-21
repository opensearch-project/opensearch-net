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

module Tests.YamlRunner.TestsDownloader

open System
open System.IO
open FSharp.Data
open Tests.YamlRunner.Models


let private rootListingUrl = "https://github.com/opensearch-project/opensearch-net"
let private rootRawUrl = "https://raw.githubusercontent.com/opensearch-project/opensearch-net"

let private openSourceResourcePath = "rest-api-spec/src/main/resources"

let private path namedSuite revision =
    let path = openSourceResourcePath
    sprintf "%s/%s/rest-api-spec/test" revision  path
    
let TestGithubRootUrl namedSuite revision = sprintf "%s/tree/%s" rootListingUrl <| path namedSuite revision
    
let FolderListUrl namedSuite revision folder =
    let root = TestGithubRootUrl namedSuite revision
    sprintf "%s/%s" root folder
    
let TestRawUrl namedSuite revision folder file =
    let path = path namedSuite revision
    sprintf "%s/%s/%s/%s" rootRawUrl path folder file
        
let private randomTime = Random()

let TemporaryPath revision suite = lazy(Path.Combine(Path.GetTempPath(), "opensearch", sprintf "tests-%s-%s" suite revision))

let private download url = async {
    let! _wait = Async.Sleep (randomTime.Next(500, 900))
    let! yaml = Http.AsyncRequestString url
    return yaml
}
let CachedOrDownload namedSuite revision folder file url = async {
    let parent = (TemporaryPath revision "_").Force()
    let directory = Path.Combine(parent, folder)
    let file = Path.Combine(directory, file)
    let fileExists = File.Exists file
    let directoryExists = Directory.Exists directory
    let! result = async {
        match (fileExists, directoryExists) with
        | (true, _) ->
            let! text = Async.AwaitTask <| File.ReadAllTextAsync file
            return text
        | (_, d) ->
            if (not d) then Directory.CreateDirectory(directory) |> ignore
            let! contents = download url
            let write = File.WriteAllTextAsync(file, contents)
            do! Async.AwaitTask write
            return contents
    }
    return (file, result)
}
    
