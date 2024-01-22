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
open OpenSearch.Net
open OpenSearch.Net.Specification.CatApi
open OpenSearch.Net.Specification.IndicesApi
open Tests.YamlRunner.Models

let private deleteAllIndices (client: IOpenSearchLowLevelClient) =
    [
        client.Indices.Delete<DynamicResponse>("*", DeleteIndexRequestParameters(ExpandWildcards=ExpandWildcards.All))
    ]

let private deleteAllTemplates (client: IOpenSearchLowLevelClient) =
    [
        client.Indices.DeleteTemplateForAll<DynamicResponse>("*")
        client.Indices.DeleteComposableTemplateForAll<DynamicResponse>("*")
        client.Cluster.DeleteComponentTemplate<DynamicResponse>("*")
    ]

let private deleteAllSnapshotsAndRepositories (client: IOpenSearchLowLevelClient) =
    let snapshotResps =
        client.Cat.Repositories<StringResponse>(CatRepositoriesRequestParameters(Headers=[| "id" |]))
            .Body.Split("\n")
        |> Seq.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
        |> Seq.map (fun repo -> client.Snapshot.Delete<DynamicResponse>(repo, "*"))
        |> Seq.toList
    snapshotResps @ [
        client.Snapshot.DeleteRepository<DynamicResponse>("*")
    ]

let private resetSettings (client: IOpenSearchLowLevelClient) =
    [
        client.Cluster.PutSettings<DynamicResponse>(PostData.String("{ \"persistent\": { \"*\": null }, \"transient\": { \"*\": null } }"))
    ]

let DefaultSetup : Operation list = [Actions("Setup", fun (client, _) ->
    seq {
        deleteAllIndices;
        deleteAllTemplates;
        deleteAllSnapshotsAndRepositories;
        resetSettings
    }
    |> Seq.collect (fun f -> f client)
    |> Seq.filter (fun r -> not r.Success)
    |> Seq.tryHead
)]

