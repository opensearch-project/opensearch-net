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

namespace Scripts

open System
open System.Runtime.InteropServices
open Fake.Core
open Fake.IO
open Octokit

//this is ugly but a direct port of what used to be duplicated in our DOS and bash scripts
module Commandline =

    let private usage = """
USAGE:

build <target> [params] [skiptests]

Targets:

* build-all
  - default target if non provided. Performs rebuild and tests all TFM's
* clean
  - cleans build output folders
* test [testfilter]
  - incremental build and unit test for .NET 4.5, [testfilter] allows you to do a contains match on the tests to be run.
* release <version>
  - 0 create a release worthy nuget packages for [version] under build\output
* integrate <opensearch_versions> [clustername] [testfilter] 
  - run integration tests for <opensearch_versions> which is a semicolon separated list of
    opensearch versions to test or `latest`. Can filter tests by <clustername> and <testfilter>
* canary 
  - create a canary nuget package based on the current version.
* cluster <cluster-name> [version]
  - Start a cluster defined in Tests.Core or Tests from the command line and leaves it running
    untill a key is pressed. Handy if you want to run the integration tests numerous times while developing  
* benchmark [non-interactive] [url] [username] [password] 
  - Runs a benchmark from Tests.Benchmarking and indexes the results to [url] when provided.
    If non-interactive runs all benchmarks without prompting
  
NOTE: both the `test` and `integrate` targets can be suffixed with `-all` to force the tests against all suported TFM's

Execution hints can be provided anywhere on the command line
- skiptests : skip running tests as part of the target chain
- non-interactive : make targets that run in interactive mode by default to run unassisted.
- seed:<N> : provide a seed to run the tests with.
- random:<K><:B> : sets random K to bool B if if B is omitted will default to true
  K can be: sourceserializer, typedkeys or oldconnection (only valid on windows)
"""

    let private (|IsUrl|_|) (candidate:string) =
        match Uri.TryCreate(candidate, UriKind.RelativeOrAbsolute) with
        | true, _ -> Some candidate
        | _ -> None
        
    let private (|IsDiff|_|) (candidate:string) =
        let c = candidate.ToLowerInvariant() 
        match c with
        | "github" | "nuget" | "directories" | "assemblies" -> Some c
        | _ -> failwith (sprintf "Unknown diff type: %s" candidate)
        
    let private (|IsProject|_|) (candidate:string) =
        let c = candidate.ToLowerInvariant()
        match c with
        | "opensearch.client" | "opensearch.net" | "opensearch.client.jsonnetserializer" -> Some c
        | _ -> None     
        
    let private (|IsFormat|_|) (candidate:string) =
        let c = candidate.ToLowerInvariant()
        match c with
        | "xml" | "markdown" | "asciidoc" -> Some c
        | _ -> None 

    type MultiTarget = All | One

    type VersionArguments = { Version: string; OutputLocation: string option }
    type TestArguments = { TrxExport: bool; TestFilter: string option; }
    type IntegrationArguments = { TrxExport: bool; TestFilter: string option; ClusterFilter: string option; OpenSearchVersions: string list; }

    type BenchmarkArguments = { Endpoint: string; Username: string option; Password: string option; }
    type ClusterArguments = { Name: string; Version: string option; }
    type CommandArguments =
        | Unknown
        | SetVersion of VersionArguments
        | Test of TestArguments
        | Integration of IntegrationArguments
        | Benchmark of BenchmarkArguments
        | Cluster of ClusterArguments

    type PassedArguments = {
        NonInteractive: bool;
        SkipTests: bool;
        Seed: int;
        RandomArguments: string list;
        RemainingArguments: string list;
        MultiTarget: MultiTarget
        ReleaseBuild: bool;
        Target: string;
        CommandArguments: CommandArguments;
    }

    let notWindows =
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
        
    let parse (args: string list) =
        
        let filteredArgs = 
            args
            |> List.filter(fun x -> 
               x <> "--report" && 
               x <> "skiptests" && 
               x <> "non-interactive" && 
               not (x.StartsWith("seed:")) && 
               not (x.StartsWith("random:")))
            |> List.map(fun (s:string) ->
                let containsSpace = s.Contains(" ")
                match s with | x when containsSpace -> sprintf "\"%s\"" x | s -> s
            )
            
        let target = 
            match (filteredArgs |> List.tryHead) with
            | Some t -> t.Replace("-one", "")
            | _ -> "build"
        let skipTests = args |> List.exists (fun x -> x = "skiptests")
        let report = args |> List.exists (fun x -> x = "--report")
        
        printf "%b exist" report

        let parsed = {
            NonInteractive = args |> List.exists (fun x -> x = "non-interactive")
            SkipTests = skipTests
            Seed = 
                match args |> List.tryFind (fun x -> x.StartsWith("seed:")) with
                | Some t -> Int32.Parse (t.Replace("seed:", ""))
                | _ -> Random().Next(1, 100_000)
            RandomArguments = 
                args 
                |> List.filter (fun x -> (x.StartsWith("random:")))
                |> List.map (fun x -> (x.Replace("random:", "")))
            RemainingArguments = filteredArgs
            MultiTarget = 
                match (filteredArgs |> List.tryHead) with
                | Some t when t.EndsWith("-one") -> MultiTarget.One
                | _ -> MultiTarget.All
            Target = 
                match (filteredArgs |> List.tryHead) with
                | Some t -> t.Replace("-one", "")
                | _ -> "build"
            ReleaseBuild = 
                match target with
                | "canary"
                | "release" -> true
                | _ -> false
            CommandArguments = Unknown
        }
            
        let arguments =
            match filteredArgs with
            | _ :: tail -> target :: tail
            | [] -> [target]
        
        let split (s:string) = s.Split ',' |> Array.toList 

        match arguments with
        | []
        | ["build"]
        | ["clean"]
        | ["benchmark"]
        | ["profile"] -> parsed
        | "rest-spec-tests" :: tail -> { parsed with RemainingArguments = tail }
        
        | ["release"; version] -> { parsed with CommandArguments = SetVersion { Version = version; OutputLocation = None }; }
        | ["release"; version; path] ->
            if (not <| System.IO.Directory.Exists path) then failwithf "'%s' is not an existing directory" (Path.getFullName path)
            { parsed with CommandArguments = SetVersion { Version = version; OutputLocation = Some path }; }
        | ["canary"] ->
            {
                parsed with CommandArguments = Test {
                        TestFilter = None
                        TrxExport = report 
                }
            }
        
        | ["test"] ->
            {
                parsed with CommandArguments = Test {
                        TestFilter = None
                        TrxExport = report 
                }
            }
        | ["test"; testFilter] ->
            {
                parsed with CommandArguments = Test {
                        TestFilter = Some testFilter
                        TrxExport = report 
                }
            }

        | ["benchmark"; IsUrl opensearch; username; password] ->
            {
                parsed with CommandArguments = Benchmark {
                        Endpoint = opensearch;
                        Username = Some username;
                        Password = Some password;
                }
            }
        | ["benchmark"; IsUrl opensearch] ->
            {
                parsed with CommandArguments = Benchmark {
                        Endpoint = opensearch;
                        Username = None
                        Password = None
                }
            }
          
        | ["integrate"; opensearchVersions] ->
            {
                parsed with CommandArguments = Integration {
                        TrxExport = report
                        OpenSearchVersions = split opensearchVersions; ClusterFilter = None; TestFilter = None
                }
            }
        | ["integrate"; opensearchVersions; clusterFilter] ->
            {
                parsed with CommandArguments = Integration {
                        TrxExport = report
                        OpenSearchVersions = split opensearchVersions;
                        ClusterFilter = Some clusterFilter;
                        TestFilter = None
                }
            }
        | ["integrate"; opensearchVersions; clusterFilter; testFilter] ->
            {
                parsed with CommandArguments = Integration {
                        TrxExport = report
                        OpenSearchVersions = split opensearchVersions;
                        ClusterFilter = Some clusterFilter
                        TestFilter = Some testFilter
                }
            }
            
        | ["cluster"; clusterName] ->
            {
                parsed with CommandArguments = Cluster { Name = clusterName; Version = None }
            }
        | ["cluster"; clusterName; clusterVersion] ->
            {
                parsed with CommandArguments = Cluster { Name = clusterName; Version = Some clusterVersion }
            }
        | _ ->
            eprintf "%s" usage
            failwith "Please consult printed help text on how to call our build"
