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

module Tests.YamlRunner.Models

open OpenSearch.Net
open System
open System.Collections.Generic
open System.Collections.Specialized
open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection

let private getName a = match FSharpValue.GetUnionFields(a, a.GetType()) with | (case, _) -> case.Name   

type YamlMap = Dictionary<Object,Object>
type YamlValue = YamlDictionary of YamlMap | YamlString of string

type DoCatch =
    | BadRequest // bad_request, 400 response from OpenSearch
    | Unauthorized //unauthorized a 401 response from OpenSearch
    | Forbidden //forbidden a 403 response from OpenSearch
    | Missing //missing a 404 response from OpenSearch
    | RequestTimeout //request_timeout a 408 response from OpenSearch
    | Conflict //conflict a 409 response from OpenSearch
    | Unavailable//unavailable a 503 response from OpenSearch
    | UnknownParameter //param a client-side error indicating an unknown parameter has been passed to the method
    | OtherBadResponse //request 4xx-5xx error response from OpenSearch, not equal to any named response above
    | CatchRegex of string // /foo bar/ the text of the error message matches this regular expression
    
let (|IsDoCatch|_|) (s:string) =
    match s with
    | "bad_request" -> Some BadRequest 
    | "unauthorized" -> Some Unauthorized 
    | "forbidden" -> Some Forbidden 
    | "missing" -> Some Missing 
    | "request_timeout" -> Some RequestTimeout 
    | "conflict" -> Some Conflict 
    | "unavailable" -> Some Unavailable
    | "param" -> Some UnknownParameter
    | "request" -> Some OtherBadResponse 
    | s -> Some <| CatchRegex (s.Trim('/'))
    
type NodeSelector =
    | NodeVersionSelector of string
    | NodeAttributeSelector of string * string
    | VersionAndAttributeSelector of string * string * string

type ResponseProperty = ResponseProperty of string

type StashedId = private StashedId of string with
    static member Create (s:String) =
        match s with
        | s when s.StartsWith "${" -> StashedId.Create <| s.Trim('$', '{', '}')
        | s when s.StartsWith "$" -> StashedId s
        | s -> StashedId <| sprintf "$%s" s
    static member Body = StashedId.Create "body"
    member this.Log = match this with | StashedId s -> s
    
type AssertOn = private ResponsePath of string | WholeResponse with
    static member Create s =
        match s with
        | null | "" -> WholeResponse
        | s -> ResponsePath (s.Trim())
    member this.Name = getName this
    member this.Log = match this with | ResponsePath p -> p | WholeResponse -> "WholeResponse"
    
type Transformation = {
    Function: string;
    Values: AssertOn list;
}
type SetTransformation = private SetTransformation of Transformation with
    static member Create (s:string) =
        let tokens = s.Split([| '#'; '('; ')'; ','|], StringSplitOptions.RemoveEmptyEntries) |> List.ofSeq
        match tokens with
        | f::tail ->
            let values = tail |> List.map AssertOn.Create
            SetTransformation <| { Function = f; Values = values }
        | [] ->
            raise <| Exception(sprintf "Can not create SetTransformation for: %s" s)
            
    member this.Log = match this with | SetTransformation s -> sprintf "#%s(%O)" s.Function (s.Values |> List.map (fun i -> i.Log))
    member this.Transform = match this with | SetTransformation s -> s
    
        
let (|ResponsePath|WholeResponse|) input = 
    match input with
    | ResponsePath str -> ResponsePath str
    | WholeResponse -> WholeResponse

type Set = Map<ResponseProperty, StashedId>
type TransformAndSet = Map<StashedId, SetTransformation>
type Headers = NameValueCollection
type RegexAssertion = { Regex:Regex }
type AssertValue =
    Id of StashedId | Value of Object | RegexAssertion of RegexAssertion
    with
    static member FromObject (s:Object) =
        match s with
        | :? String as id when id.StartsWith "$" -> Id <| StashedId.Create id
        | :? String as regex when regex.StartsWith "/" ->
            let expression = Regex.Replace(regex, @"(^[\s\r\n]*?\/|\/[\s\r\n]*?$)", ""); 
            let opts = RegexOptions.IgnorePatternWhitespace 
            RegexAssertion { Regex = Regex(expression, opts) }
        | s -> Value s
        
type NumericValue = NumericId of StashedId | Long of int64  | Double of double with
    member this.Name = getName this
    override this.ToString() =
        match this with
        | Long i -> sprintf "%i" i
        | Double d -> sprintf "%f" d
        | NumericId id -> sprintf "%s" id.Log
        
type Match = Map<AssertOn, AssertValue>
type NumericMatch = Map<AssertOn, NumericValue>
    
type Do = {
    ApiCall: string * YamlMap
    Catch:DoCatch option
    Warnings:option<string list>
    AllowedWarnings:option<string list>
    NodeSelector:NodeSelector option
    Headers: Headers option
    AutoFail: bool 
}
    with member this.Log () = sprintf "Api %s" <| fst this.ApiCall

type Feature =
    | CatchUnauthorized // "catch_unauthorized", //NOT seen in main branch
    | DefaultShards // "default_shards", //NOT seen in main branch
    | EmbeddedStashKey // "embedded_stash_key", //NOT seen in main branch
    | Headers // "headers", 
    | NodeSelector // "node_selector", // not needed in the client yaml runner always runs single node mode.
    | StashInKey // "stash_in_key", //NOT seen in main branch
    | StashInPath // "stash_in_path",
    | StashPathReplace // "stash_path_replace", //NOT seen in main branch
    | Warnings // "warnings", 
    | Yaml // "yaml", 
    | Contains // "contains", //NOT seen in main branch
    | TransformAndSet // "transform_and_set", 
    | ArbitraryKey // "arbitrary_key"
    | Unsupported of string

let SupportedFeatures = [EmbeddedStashKey; StashInPath; ArbitraryKey; Warnings; Headers
                         Contains; DefaultShards; CatchUnauthorized; TransformAndSet ]
    
let (|ToFeature|) (s:string) =
    match s with
    | "catch_unauthorized" -> CatchUnauthorized
    | "default_shards" -> DefaultShards
    | "embedded_stash_key" -> EmbeddedStashKey
    | "headers" -> Headers
    | "node_selector" -> NodeSelector
    | "stash_in_key" -> StashInKey
    | "stash_in_path" -> StashInPath
    | "stash_path_replace" -> StashPathReplace
    | "warnings" -> Warnings
    | "yaml" -> Yaml
    | "contains" -> Contains
    | "transform_and_set" -> TransformAndSet
    | "arbitrary_key" -> ArbitraryKey
    | s -> Unsupported s

type Skip = { Version:SemVer.Range list option; Reason:string option; Features: Feature list option }
    with member this.Log = sprintf "Version %A Features:%A Reason: %A" this.Version this.Features this.Reason

type NumericAssert = 
    | LowerThan 
    | GreaterThan 
    | GreaterThanOrEqual 
    | LowerThanOrEqual 
    | Equal 
    | Length
    with member this.Name = getName this

let (|IsNumericAssert|_|) (s:string) =
    match s with
    | "length" -> Some Length
    | "eq" -> Some Equal
    | "gte" -> Some GreaterThanOrEqual
    | "lte" -> Some LowerThanOrEqual
    | "gt" -> Some GreaterThan
    | "lt" -> Some LowerThan
    | _ -> None
    
type Assert = 
    | IsTrue of AssertOn
    | IsFalse of AssertOn
    | Match of Match
    | Contains of Match 
    | NumericAssert of NumericAssert * NumericMatch
    with
        member this.Name = getName this
        member this.Log () =
            match this with
            | IsTrue s -> sprintf "%s %s" this.Name s.Log
            | IsFalse s -> sprintf "%s %s" this.Name s.Log
            | Contains s
            | Match s -> 
                sprintf "%s %s" this.Name (s |> Seq.map (fun k -> sprintf "%s %20A" k.Key.Log k.Value) |> String.concat " ")
            | NumericAssert (a, m) ->
                sprintf "%s %s %s"
                    this.Name
                    a.Name
                    (m |> Seq.map (fun k -> sprintf "%s %A" k.Key.Log k.Value.Name) |> String.concat " ")

type Operation =
    | Unknown of string
    | Actions of string * (IOpenSearchLowLevelClient * string -> DynamicResponse option)
    | Skip of Skip
    | Do of Do
    | Set of Set
    | TransformAndSet of TransformAndSet
    | Assert of Assert
    with
        member this.Name = getName this
        member this.Log () =
            match this with
            | Assert s -> sprintf "%s operation %s" this.Name (s.Log())
            | Do s -> sprintf "%s operation %s" "Do" (s.Log())
            | Unknown s -> sprintf "%s operation %s" this.Name s
            | Actions (s, _) -> sprintf "%s custom %s actions" this.Name s
            | Skip s -> sprintf "%s operation %s" this.Name s.Log
            | Set s ->
                sprintf "%s operation %s" this.Name (s |> Seq.map (fun k -> sprintf "%A %A" k.Key k.Value) |> String.concat " ")
            | TransformAndSet s ->
                sprintf "%s operation %s" this.Name (s |> Seq.map (fun k -> sprintf "%A %A" k.Key k.Value) |> String.concat " ")
    
let (|IsOperation|_|) (s:string) =
    match s with
    | "skip" 
    | "warnings" 
    | "set" 
    | "transform_and_set" 
    | "headers" 
    | "do" 
    | "contains" 
    | "match" 
    | "is_false" 
    | "is_true" -> Some s
    | IsNumericAssert _ -> Some s
    | _ -> None
    
type Operations = Operation list

type YamlTest = {
    Name:string
    Operations: Operations
}
    
type YamlTestSection = | Setup of Operations | Teardown of Operations | YamlTest of YamlTest

