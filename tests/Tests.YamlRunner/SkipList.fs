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
    // Incorrectly being run due to OpenSearch 1.x/2.x being numerically <7.2.0, but feature-wise >7.10
    SkipFile "cat.indices/10_basic.yml", Section "Test cat indices output for closed index (pre 7.2.0)"
    SkipFile "cluster.health/10_basic.yml", Section "cluster health with closed index (pre 7.2.0)"

    // Variations of `indices.put_alias` that accept index/alias in request body rather than path which are not supported by .NET client
    // https://github.com/opensearch-project/opensearch-net/issues/718
    SkipFile "indices.put_alias/10_basic.yml", All

    // .NET method arg typings make this not possible, index is a required parameter
    SkipFile "indices.put_mapping/all_path_options_with_types.yml", Section "put mapping with blank index"

    // The client doesn't support the indices.upgrade API
    SkipFile "indices.upgrade/10_basic.yml", All

    // TODO: Add support for search pipeline APIs
    SkipFile "search_pipeline/10_basic.yml", All

    // TODO: Better support parsing and asserting unsigned longs (hitting long vs double precision issues)
    SkipFile "search.aggregation/20_terms.yml", Section "Unsigned Long test"
    SkipFile "search.aggregation/230_composite_unsigned.yml", All
    SkipFile "search.aggregation/370_multi_terms.yml", Section "Unsigned Long test"
    SkipFile "search/90_search_after.yml", Section "unsigned long"
]
