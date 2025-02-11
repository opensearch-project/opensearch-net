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
    SkipFile "index/91_flat_object_null_value.yml", Section "Supported queries"
    
    // Incorrectly being run due to OpenSearch 1.x/2.x being numerically <7.2.0, but feature-wise >7.10
    SkipFile "cat.indices/10_basic.yml", Section "Test cat indices output for closed index (pre 7.2.0)"
    SkipFile "cluster.health/10_basic.yml", Section "cluster health with closed index (pre 7.2.0)"

    // .NET method arg typings make this not possible, index is a required parameter
    SkipFile "indices.put_mapping/all_path_options_with_types.yml", Section "put mapping with blank index"

    // argument is an enum in .NET client, meaning invalid value can't be passed
    SkipFile "indices.stats/10_index.yml", Section "Indices stats unrecognized parameter"

    // TODO: Better support parsing and asserting unsigned longs (hitting long vs double precision issues)
    SkipFile "search.aggregation/20_terms.yml", Section "Unsigned Long test"
    SkipFile "search.aggregation/230_composite_unsigned.yml", All
    SkipFile "search.aggregation/370_multi_terms.yml", Section "Unsigned Long test"
    SkipFile "search.aggregation/410_nested_aggs.yml", Section "Supported queries"
    SkipFile "search/90_search_after.yml", Section "unsigned long"
    SkipFile "search/260_sort_unsigned_long.yml", Section "test sorting against unsigned_long only fields"
    
    SkipFile "search_shards/20_slice.yml", Section "Search shards with slice specified in body"
]
