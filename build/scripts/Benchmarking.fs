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

open System.IO
open Commandline

module Benchmarker =

    let private testsProjectDirectory = Path.GetFullPath(Paths.TestsSource("Tests.Benchmarking"))

    let Run args =
        
        let url = match args.CommandArguments with | Benchmark b -> Some b.Endpoint | _ -> None
        let username = match args.CommandArguments with | Benchmark b -> b.Username | _ -> None
        let password = match args.CommandArguments with | Benchmark b -> b.Password | _ -> None
        let runInteractive = not args.NonInteractive
        let credentials  = (username, password)
        let runCommandPrefix = "run -f net6.0 -c Release"
        let runCommand =
            match (runInteractive, url, credentials) with
            | (false, Some url, (Some username, Some password)) -> sprintf "%s -- --all \"%s\" \"%s\" \"%s\"" runCommandPrefix url username password
            | (false, Some url, _) -> sprintf "%s -- --all \"%s\"" runCommandPrefix url
            | (false, _, _) -> sprintf "%s -- --all" runCommandPrefix 
            | (true, _, _) -> runCommandPrefix
            
        Tooling.DotNet.ExecIn testsProjectDirectory [runCommand] |> ignore
