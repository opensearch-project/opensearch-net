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

module Tests.YamlRunner.Skips

type SkipSection = All | Section of string | Sections of string list

type SkipFile = SkipFile of string 

let SkipList = dict<SkipFile,SkipSection> [    
    // funny looking dispatch /_security/privilege/app?name
    SkipFile "privileges/10_basic.yml", All
    
    // We skip the generation of this API till one of the later minors
    SkipFile "indices.upgrade/10_basic.yml", All
    
    // - Failed: Assert operation NumericAssert Length invalidated_api_keys "Long" Reason: Expected 2.000000 = 3.000000        
    SkipFile "api_key/11_invalidation.yml", Section "Test invalidate api key by realm name"
    
    // Uses variables in strings e.g Bearer ${token} we can not due variable substitution in string yet
    SkipFile "token/10_basic.yml", All
    
    SkipFile "change_password/11_token.yml", Section "Test user changing their password authenticating with token not allowed"

    SkipFile "change_password/10_basic.yml", Sections [
        // Changing password locks out tests
        "Test user changing their own password"
        // Uses variables in strings e.g Bearer ${token} we can not due variable substitution in string yet
        "Test user changing their password authenticating with token not allowed"
    ] 

    // TEMPORARY: Missing 'body: { indices: "test_index" }' payload, TODO: PR
    SkipFile "snapshot/10_basic.yml", Section "Create a source only snapshot and then restore it"
    // illegal_argument_exception: Provided password hash uses [NOOP] but the configured hashing algorithm is [BCRYPT]
    SkipFile "users/10_basic.yml", Section "Test put user with password hash"
    // Slash in index name is not escaped (BUG)
    SkipFile "security/authz/13_index_datemath.yml", Section "Test indexing documents with datemath, when permitted"
    // Possibly a cluster health color mismatch...
    SkipFile "security/authz/14_cat_indices.yml", All
   
    // Snapshot testing requires local filesystem access
    SkipFile "snapshot.create/10_basic.yml", All
    SkipFile "snapshot.get/10_basic.yml", All
    SkipFile "snapshot.get_repository/10_basic.yml", All
    SkipFile "snapshot.restore/10_basic.yml", All
    SkipFile "snapshot.status/10_basic.yml", All
      
    // uses $stashed id in match with object
    SkipFile "cluster.reroute/11_explain.yml", Sections [
        "Explain API for non-existent node & shard"
    ]
    
    //These are ignored because they were flagged on a big PR.
    
    //additional enters in regex
    SkipFile "cat.templates/10_basic.yml", Sections [ "Multiple template"; "Sort templates"; "No templates" ]
    
    //Replace stashed value in body that is passed as string json
    SkipFile "api_key/10_basic.yml", Section "Test get api key"
        
    //new API TODO remove when we regenerate
    SkipFile "cluster.voting_config_exclusions/10_basic.yml", All
    
    //TODO has dates without strings which trips up our yaml parser
    SkipFile "runtime_fields/40_date.yml", All

    SkipFile "nodes.info/10_basic.yml", Section "node_info role test"
]


