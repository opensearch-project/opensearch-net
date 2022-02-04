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
open System.Globalization
open Fake.Core
open System.IO
open Commandline
open Versioning

module Tests =


    let SetTestEnvironmentVariables args = 
        let clusterFilter = match args.CommandArguments with | Integration a -> a.ClusterFilter | _ -> None
        let testFilter = match args.CommandArguments with | Integration a -> a.TestFilter | Test t -> t.TestFilter | _ -> None
        
        let env key v =
            match v with
            | Some v -> Environment.setEnvironVar key <| sprintf "%O" v
            | None -> ignore()
        
        env "OSC_INTEGRATION_CLUSTER" clusterFilter
        env "OSC_TEST_FILTER" testFilter
        let seed = Some <| args.Seed.ToString(CultureInfo.InvariantCulture)
        env "OSC_TEST_SEED" seed

        for random in args.RandomArguments do 
            let tokens = random.Split [|':'|]
            let key = tokens.[0].ToUpper()
            let b = if tokens.Length = 1 then true else (bool.Parse (tokens.[1]))
            let key = sprintf "OSC_RANDOM_%s" key
            let value = (if b then "true" else "false")
            env key (Some <| value)
        ignore()

    let private dotnetTest proj args =
        let runSettings =
            // force the logger section to be cleared so that azure devops can work its magic.
            // relies heavily on the original console logger
            let wants = match args.CommandArguments with | Integration a -> a.TrxExport | Test t -> t.TrxExport | _ -> false
            let prefix = if wants then ".ci" else ""
            sprintf "tests/%s.runsettings" prefix
        
        Directory.CreateDirectory Paths.BuildOutput |> ignore
        let command = ["test"; proj; "--nologo"; "-c"; "Release"; "-s"; runSettings; "--no-build"]
        
        let wantsTrx =
            let wants = match args.CommandArguments with | Integration a -> a.TrxExport | Test t -> t.TrxExport | _ -> false
            let junitOutput = Path.GetFullPath <| Path.Combine(Paths.BuildOutput, "junit-{assembly}-{framework}-test-results.xml")
            let loggerPathArgs = sprintf "LogFilePath=%s" junitOutput
            let loggerArg = sprintf "--logger:\"junit;%s\"" loggerPathArgs
            match wants with | true -> [loggerArg] | false -> []
           
        let commandWithAdditionalOptions = wantsTrx |> List.append command
            
        Tooling.DotNet.ExecInWithTimeout "." commandWithAdditionalOptions (TimeSpan.FromMinutes 30.)

    let RunReleaseUnitTests version args =
        //xUnit always does its own build, this env var is picked up by Tests.csproj
        //if its set it will include the local package source (build/output/)
        //and references OSC and OSC.JsonNetSerializer by the current version
        //this works by not including the local package cache (nay source) 
        //in the project file via:
        //<RestoreSources></RestoreSources>
        //This will download all packages but its the only way to make sure we reference the built
        //package and not one from cache...y
        Environment.setEnvironVar "TestPackageVersion" (version.Full.ToString())
        Tooling.DotNet.ExecIn "tests/Tests" ["clean";] |> ignore
        // needs forced eval because it picks up local nuget packages not part of package.lock.json
        Tooling.DotNet.ExecIn "tests/Tests" ["restore"; "--force-evaluate"] |> ignore
        dotnetTest "tests/Tests/Tests.csproj" args

    let RunUnitTests args =
        dotnetTest "tests/tests.proj" args

    let RunIntegrationTests args =
        let passedVersions = match args.CommandArguments with | Integration a -> Some a.OpenSearchVersions | _ -> None
        match passedVersions with
        | None -> failwith "No versions specified to run integration tests against"
        | Some opensearchVersions ->
            for opensearchVersion in opensearchVersions do
                Environment.setEnvironVar "OSC_INTEGRATION_TEST" "1"
                Environment.setEnvironVar "OSC_INTEGRATION_VERSION" opensearchVersion
                dotnetTest "tests/Tests/Tests.csproj" args
