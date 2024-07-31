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

module Tests.YamlRunner.AsyncExtensions

open System.Runtime.CompilerServices
open Microsoft.FSharp.Control
open System.Threading.Tasks
open System.Collections.Generic
open System.Runtime.ExceptionServices

[<AutoOpen>]
[<Extension>]
module AsyncExtensions =
    type Microsoft.FSharp.Control.Async with

        // Wrote this as Async.Parallel eagerly materializes and forcefully executes in order.
        // There is an extension that came in as dependency that extends Async.Parallel with maxDegreeOfParallelism
        // but for some reason this did not behave as I had expected
        [<Extension>]
        static member inline ForEachAsync (maxDegreeOfParallelism: int) asyncs = async {
            let tasks = List<Task<'a>>(maxDegreeOfParallelism)
            let results = List<'a>()
            for async in asyncs do
                let! x = Async.StartChildAsTask async
                tasks.Add <| x
                if (tasks.Count >= maxDegreeOfParallelism) then
                    let! task = Async.AwaitTask <| Task.WhenAny(tasks)
                    if (task.IsFaulted) then ExceptionDispatchInfo.Capture(task.Exception).Throw();
                    results.Add(task.Result)
                    let _ = tasks.Remove <| task
                    ignore()

            let! completed = Async.AwaitTask <| Task.WhenAll tasks
            for c in completed do results.Add c
            return results |> Seq.toList
        }
