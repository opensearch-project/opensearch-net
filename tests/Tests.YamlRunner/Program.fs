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

module Tests.YamlRunner.Main

open System
open System.Linq
open System.Diagnostics
open System.Security.Cryptography.X509Certificates
open Argu
open Tests.YamlRunner
open Tests.YamlRunner.Models
open OpenSearch.Net

type Arguments =
    | [<First; MainCommand; CliPrefix(CliPrefix.None)>] Named_Suite of string
    | Folder of string
    | Test_File of string
    | Test_Section of string
    | Endpoint of string
    | Auth_Basic of string
    | Auth_Cert of string
    | Auth_Cert_Pass of string
    | Revision of string
    | JUnit_Output_File of string
    | Profile of bool
    with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Named_Suite _ -> "specify a known yaml test suite. defaults to `opensource`."
            | Revision _ -> "The git revision to reference (commit/branch/tag). defaults to `main`"
            | Folder _ -> "Only run tests in this folder"
            | Test_File _ -> "Only run tests starting with this filename"
            | Test_Section _ -> "Only run test with this name (best used in conjunction with --test-file)"
            | Endpoint _ -> "The opensearch endpoint to run tests against"
            | Auth_Basic _ -> "The username and password to use for client authentication in the form of `username:password`"
            | Auth_Cert _ -> "The certificate to use for client authentication"
            | Auth_Cert_Pass _ -> "The password to use for the auth certificate"
            | JUnit_Output_File _ -> "The path and file name to use for the junit xml output, defaults to a random tmp filename"
            | Profile _ -> "Print out process id and wait for confirmation to kick off the tests"

let private runningMitmProxy = Process.GetProcessesByName("mitmproxy").Length > 0
let private runningProxy = runningMitmProxy || Process.GetProcessesByName("fiddler").Length > 0
let private defaultEndpoint namedSuite = 
    let host = 
        match (runningProxy, namedSuite) with
        | (true, _) -> "ipv4.fiddler"
        | _ -> "localhost"
    let https = "s" // ""
    sprintf "http%s://%s:9200" https host;

let private createClient endpoint (authBasic: string option) (authCert: string option * string option) namedSuite = 
    let mutable settings = new ConnectionConfiguration(Uri(endpoint))
    settings <-
        settings.DisableDirectStreaming(true)
    // proxy 
    settings <-
        match runningMitmProxy with
        | true -> settings.Proxy(Uri("http://ipv4.fiddler:8080"), String(null), String(null))
        | _ -> settings
    // auth
    settings <-
        match (authCert, authBasic) with
        | (Some(certPath), None), _ ->
            settings.ClientCertificate(new X509Certificate2(certPath))
        | (Some(certPath), Some(certPass)), _ ->
            settings.ClientCertificate(new X509Certificate2(certPath, certPass))
        | _, Some(userPass) ->
            match userPass.Split(':') with
            | [| username; password |] -> settings.BasicAuthentication(username, password)
            | _ -> settings
        | _ -> settings
    // certs
    settings <-
        settings.ServerCertificateValidationCallback(fun _ _ _ _ -> true)
    OpenSearchLowLevelClient(settings)
    
let validateRevisionParams endpoint authBasic authCert _passedRevision namedSuite =    
    let client = createClient endpoint authBasic authCert namedSuite
    
    let node = client.Settings.ConnectionPool.Nodes.First()
    let auth =     
        match client.Settings.BasicAuthenticationCredentials with 
        | null -> ""
        | s -> sprintf "%s:%s" s.Username (s.Password.CreateString())
        
    printfn "Running opensearch %O %s" (node.Uri) auth
    
    let r =
       client.RootNodeInfo<DynamicResponse>()
        
    printfn "%s" r.DebugInformation
    if not r.Success then
        failwithf "No running opensearch found at %s" endpoint
    
    let version = r.Get<string>("version.number") 
    let runningRevision = r.Get<string>("version.build_hash")
    
    // TODO validate the endpoint running confirms to expected `passedRevision`
    // needs to handle tags (1.2.0) and branches (1.x, 1.2, main)
    // not quite sure whats the rules are
    let revision = runningRevision
        
    (client, revision, version)
    
let runMain (parsed:ParseResults<Arguments>) = async {
    let namedSuite = parsed.TryGetResult Named_Suite |> Option.defaultValue "_"
    let directory = parsed.TryGetResult Folder //|> Option.defaultValue "indices.create" |> Some
    let file = parsed.TryGetResult Test_File //|> Option.defaultValue "10_basic.yml" |> Some
    let section = parsed.TryGetResult Test_Section //|> Option.defaultValue "10_basic.yml" |> Some
    let endpoint = parsed.TryGetResult Endpoint |> Option.defaultValue (defaultEndpoint namedSuite)
    let authBasic = parsed.TryGetResult Auth_Basic
    let authCert = parsed.TryGetResult Auth_Cert
    let authCertPass = parsed.TryGetResult Auth_Cert_Pass
    let profile = parsed.TryGetResult Profile |> Option.defaultValue false
    let passedRevision = parsed.TryGetResult Revision
    let outputFile =
        parsed.TryGetResult JUnit_Output_File
        |> Option.defaultValue (System.IO.Path.GetTempFileName())
        
    let client, revision, version = validateRevisionParams endpoint authBasic (authCert, authCertPass) passedRevision namedSuite
    
    printfn "Found version %s downloading specs from: %s" version revision
    
    let! locateResults = Commands.LocateTests namedSuite revision directory file
    let readResults = Commands.ReadTests locateResults 
    if profile then
        printf "Waiting for profiler to attach to pid: %O" <| Process.GetCurrentProcess().Id
        Console.ReadKey() |> ignore
        
    let! runResults = Commands.RunTests readResults client version namedSuite section
    let summary = Commands.ExportTests runResults outputFile
    
    Commands.PrettyPrintResults outputFile
    
    printfn "JUnit output: %s" outputFile
    printfn "Total Tests: %i Failed: %i Errors: %i Skipped: %i"
        summary.Tests summary.Failed summary.Errors summary.Skipped
    printfn "Total Time %O" <| TimeSpan.FromSeconds summary.Time
        
    return summary.Failed + summary.Errors
}

[<EntryPoint>]
let main argv =
    
    let parser = ArgumentParser.Create<Arguments>(programName = "yaml-test-runner")
    let parsed = 
        try
            Some <| parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
        with e ->
            printfn "%s" e.Message
            None
    match parsed with
    | None -> 1
    | Some parsed ->
        async {
            do! Async.SwitchToThreadPool ()
            return! runMain parsed
        } |> Async.RunSynchronously
    
