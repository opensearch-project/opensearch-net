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
open ProcNet
open ProcNet.Std

module Tooling = 

    type ExecResult = { ExitCode: int; Output: LineOut seq;}
    
    let private defaultTimeout = TimeSpan.FromMinutes(5.)
    
    type NoopWriter () =
        interface IConsoleOutWriter with
            member self.Write (_: Exception) = ignore()
            member self.Write (_: ConsoleOut) = ignore()
    
    let private defaultConsoleWriter = Some <| (ConsoleOutColorWriter() :> IConsoleOutWriter)
    
    let readInWithTimeout (timeout: TimeSpan) workinDir bin (writer: IConsoleOutWriter option) args = 
        let startArgs = StartArguments(bin, args |> List.toArray)
        if (Option.isSome workinDir) then
            startArgs.WorkingDirectory <- Option.defaultValue "" workinDir
        startArgs.Timeout <- timeout
        startArgs.ConsoleOutWriter <- Option.defaultValue<IConsoleOutWriter> (NoopWriter()) writer
        let result = Proc.Start(startArgs)
        
        if not result.Completed then failwithf "process failed to complete within %O: %s" timeout bin
        if not result.ExitCode.HasValue then failwithf "process yielded no exit code: %s" bin
        { ExitCode = result.ExitCode.Value; Output = seq result.ConsoleOut}
        
    let read bin args = readInWithTimeout defaultTimeout None bin defaultConsoleWriter args 
    let readQuiet bin args = readInWithTimeout defaultTimeout None bin None args
    
    let execInWithTimeout (timeout: TimeSpan) workinDir bin args = 
        let startArgs = ExecArguments(bin, args |> List.toArray)
        if (Option.isSome workinDir) then
            startArgs.WorkingDirectory <- Option.defaultValue "" workinDir
        startArgs.Timeout <- timeout
        let options = args |> String.concat " "
        printfn ":: Running command: %s %s" bin options
        try
            let result = Proc.Exec(startArgs)
            if result > 0 then
                failwithf "process returned %i: %s" result bin
        with
        | :? ProcExecException as ex -> failwithf "%s" ex.Message

    let execIn workingDir bin args = execInWithTimeout defaultTimeout workingDir bin args
    
    let exec bin args = execIn None bin args
    

    type BuildTooling(timeout, path) =
        let timeout = match timeout with | Some t -> t | None -> defaultTimeout
        member this.Path = path
        member this.ReadQuietIn workingDirectory arguments =
            readInWithTimeout defaultTimeout (Some workingDirectory) this.Path None arguments
        member this.ReadInWithTimeout workingDirectory arguments timeout =
            readInWithTimeout timeout (Some workingDirectory) this.Path defaultConsoleWriter arguments
        member this.ExecInWithTimeout workingDirectory arguments timeout = execInWithTimeout timeout (Some workingDirectory) this.Path arguments
        member this.ExecWithTimeout arguments timeout = execInWithTimeout timeout None this.Path arguments
        member this.ExecIn workingDirectory arguments = this.ExecInWithTimeout workingDirectory arguments timeout
        member this.Exec arguments = this.ExecWithTimeout arguments timeout

    let DotNet = BuildTooling(None, "dotnet")

    
