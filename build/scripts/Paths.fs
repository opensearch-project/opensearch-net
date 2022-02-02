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

module Paths =

    let OwnerName = "elastic"
    let RepositoryName = "elasticsearch-net"
    let Repository = sprintf "https://github.com/%s/%s/" OwnerName RepositoryName

    let BuildFolder = "build"
    let TargetsFolder = "build/scripts"
    
    let BuildOutput = sprintf "%s/output" BuildFolder
    let Output(folder) = sprintf "%s/%s" BuildOutput folder
    
    let InplaceBuildOutput project tfm = 
        sprintf "src/%s/bin/Release/%s" project tfm
    let InplaceBuildTestOutput project tfm = 
        sprintf "tests/%s/bin/Release/%s" project tfm
    let MagicDocumentationFile  = 
        "src/OpenSearch.Net/obj/Release/netstandard2.1/OpenSearch.Net.csprojAssemblyReference.cache" 
  
    let Tool tool = sprintf "packages/build/%s" tool
    let CheckedInToolsFolder = "build/tools"
    let KeysFolder = sprintf "%s/keys" BuildFolder
    let NugetOutput = sprintf "%s" BuildOutput
    let SourceFolder = "src"
    
    let Solution = "Elasticsearch.sln"
    
    let Keys(keyFile) = sprintf "%s/%s" KeysFolder keyFile
    let Source(folder) = sprintf "%s/%s" SourceFolder folder
    let TestsSource(folder) = sprintf "tests/%s"  folder
    
    let ProjFile project = sprintf "%s/%s/%s.csproj" SourceFolder project project
    let TestProjFile project = sprintf "tests/%s/%s.csproj" project project

    let BinFolder (folder:string) = 
        let f = folder.Replace(@"\", "/")
        sprintf "%s/%s/bin/Release" SourceFolder f
        
        
