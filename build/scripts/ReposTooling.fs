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
open System.IO
open Fake.IO
open Commandline

module ReposTooling =

    let LaunchCluster args =
        let clusterName = Option.defaultValue "" <| match args.CommandArguments with | Cluster c -> Some c.Name | _ -> None
        let clusterVersion = Option.defaultValue "" <|match args.CommandArguments with | Cluster c -> c.Version | _ -> None
        
        let testsProjectDirectory = Path.GetFullPath(Paths.InplaceBuildTestOutput "Tests.ClusterLauncher" "net6.0")
        let tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        
        printfn "%s" testsProjectDirectory
        
        Shell.copyDir tempDir testsProjectDirectory (fun _ -> true)
        
        let command = sprintf "%s %s" clusterName clusterVersion
        let timeout = TimeSpan.FromMinutes(120.)
        let dll = Path.Combine(tempDir, "Tests.ClusterLauncher.dll");
        Tooling.DotNet.ExecInWithTimeout tempDir [dll; command] timeout  |> ignore
        
        Shell.deleteDir tempDir
        
    let RestSpecTests args =
        let folder = Path.getDirectory (Paths.TestProjFile "Tests.YamlRunner")
        let timeout = TimeSpan.FromMinutes(120.)
        Tooling.DotNet.ExecInWithTimeout folder (["run"; "--" ] @ args) timeout  |> ignore
    
    
    let restoreOnce = lazy(Tooling.DotNet.Exec ["tool"; "restore"])
    
    let private differ = "assembly-differ"
    let Differ args =
        restoreOnce.Force()
              
        let args = args |> String.concat " "
        let command = sprintf @"%s %s -o ../../%s" differ args Paths.BuildOutput
        Tooling.DotNet.ExecIn Paths.TargetsFolder [command] |> ignore

    let private assemblyRewriter = "assembly-rewriter"
    let Rewriter args =
        restoreOnce.Force()
        Tooling.DotNet.ExecIn "." (List.append [assemblyRewriter] (List.ofSeq args)) |> ignore
        
    let private packageValidator = "nupkg-validator"
    let PackageValidator args =
        restoreOnce.Force()
        Tooling.DotNet.ExecIn "." (List.append [packageValidator] (List.ofSeq args)) |> ignore
         
