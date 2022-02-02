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

module Tests.YamlRunner.Stashes

open System.Text.RegularExpressions
open OpenSearch.Net
open System
open System.Collections.Generic
open Tests.YamlRunner.Models
open ShellProgressBar

type Stashes() =
    inherit Dictionary<StashedId, Object>()
    
    member val ResponseOption:DynamicResponse option = None with get, set
    member private this.LazyResponse = lazy(this.ResponseOption.Value)
    member this.Response () = this.LazyResponse.Force()
    
    member this.ResolvePath (progress:IProgressBar) (path:String) =
        let r = this.ResolveToken progress
        let tokens =
            path.Split('.')
            |> Seq.map r
        String.Join('.', tokens)
        
    member this.GetResponseValue (progress:IProgressBar) (path:String) =
        let g = this.Response().Get 
        match path with
        | "$body" -> g "body"
        | _ -> 
            let path = this.ResolvePath progress path
            g path
    
    member this.Resolve (progress:IProgressBar) (object:YamlMap) =
        let rec resolve (o:Object) : Object =
            match o with
            | :? List<Object> as list ->
                let resolved = List<Object>();
                list |> Seq.iter(fun i -> resolved.Add <| resolve i)
                resolved :> Object
            | :? YamlMap as map ->
                let newDict = YamlMap()
                map
                |> Seq.iter (fun kv -> newDict.[kv.Key] <- resolve kv.Value)
                newDict :> Object
            | :? String as value -> this.ResolveToken progress value 
            | value -> value
        
        let resolved = resolve object :?> YamlMap
        resolved
    
    /// Optionally replaces all ${id} references in s, returns whether it actually replaced anything and the string
    member this.ReplaceStaches (progress:IProgressBar) (s:String) =
        match s.Contains("$") with
        | false -> (false, s)
        | true ->
            let re = Regex.Replace(s, "\$\{?\w+\}?", fun r -> (this.ResolveToken progress r.Value).ToString())
            (true, re)
        
    member this.ResolveToken (progress:IProgressBar) (s:String) : Object =
        match s with
        | s when s.StartsWith "$" ->
            let found, value = this.TryGetValue <| StashedId.Create s
            if not found then
                let s = sprintf "Expected to resolve %s but no such value was stashed at this point" s 
                progress.WriteLine s 
                failwith s
            value
        | s when s.Contains "$" ->
            let (_, r) = this.ReplaceStaches progress s
            r :> Object
        | s -> s :> Object
        
        
        
        
