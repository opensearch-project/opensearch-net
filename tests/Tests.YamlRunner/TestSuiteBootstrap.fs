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

module Tests.YamlRunner.TestSuiteBootstrap

open System
open System.Linq
open OpenSearch.Net
open OpenSearch.Net.Specification.CatApi
open OpenSearch.Net.Specification.ClusterApi
open OpenSearch.Net.Specification.IndicesApi
open Tests.YamlRunner.Models

let DefaultSetup : Operation list = [Actions("Setup", fun (client, suite) ->
    let firstFailure (responses:DynamicResponse seq) =
            responses
            |> Seq.filter (fun r -> not r.Success && r.HttpStatusCode <> Nullable.op_Implicit 404)
            |> Seq.tryHead

    let deleteAll () =
        let dp = DeleteIndexRequestParameters()
        dp.SetQueryString("expand_wildcards", "open,closed,hidden")
        client.Indices.Delete<DynamicResponse>("*", dp)
    let templates () =
        client.Cat.Templates<StringResponse>("*", CatTemplatesRequestParameters(Headers=["name";"order"].ToArray()))
            .Body.Split("\n")
            |> Seq.map(fun line -> line.Split(" ", StringSplitOptions.RemoveEmptyEntries))
            |> Seq.filter(fun line -> line.Length = 2)
            |> Seq.map(fun tokens -> tokens.[0], Int32.Parse(tokens.[1]))
            //assume templates with order 100 or higher are defaults
            |> Seq.filter(fun (_, order) -> order < 100)
            |> Seq.filter(fun (name, _) -> not(String.IsNullOrWhiteSpace(name)) && not(name.StartsWith(".")) && name <> "security-audit-log")
            //TODO template does not accept comma separated list but is documented as such
            |> Seq.map(fun (template, _) ->
                let result = client.Indices.DeleteTemplateForAll<DynamicResponse>(template)
                match result.Success with
                | true -> result
                | false -> client.Indices.DeleteComposableTemplateForAll<DynamicResponse>(template)
            )
            |> Seq.toList

    let snapshots =
        client.Cat.Snapshots<StringResponse>(CatSnapshotsRequestParameters(Headers=["id,repository"].ToArray()))
            .Body.Split("\n")
            |> Seq.map(fun line -> line.Split " ")
            |> Seq.filter(fun tokens -> tokens.Length = 2)
            |> Seq.map(fun tokens -> (tokens.[0].Trim(), tokens.[1].Trim()))
            |> Seq.filter(fun (id, repos) -> not(String.IsNullOrWhiteSpace(id)) && not(String.IsNullOrWhiteSpace(repos)))
            //TODO template does not accept comma separated list but is documented as such
            |> Seq.map(fun (id, repos) -> client.Snapshot.Delete<DynamicResponse>(repos, id))
            |> Seq.toList

    let deleteRepositories = client.Snapshot.DeleteRepository<DynamicResponse>("*")
    firstFailure <| [deleteAll()] @ templates() @ snapshots @ [deleteRepositories]
)]

