/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/
// ███╗   ██╗ ██████╗ ████████╗██╗ ██████╗███████╗
// ████╗  ██║██╔═══██╗╚══██╔══╝██║██╔════╝██╔════╝
// ██╔██╗ ██║██║   ██║   ██║   ██║██║     █████╗
// ██║╚██╗██║██║   ██║   ██║   ██║██║     ██╔══╝
// ██║ ╚████║╚██████╔╝   ██║   ██║╚██████╗███████╗
// ╚═╝  ╚═══╝ ╚═════╝    ╚═╝   ╚═╝ ╚═════╝╚══════╝
// -----------------------------------------------
//
// This file is automatically generated
// Please do not edit these files manually
// Run the following in the root of the repos:
//
//      *NIX        :   ./build.sh codegen
//      Windows     :   build.bat codegen
//
// -----------------------------------------------
namespace OpenSearch.Client
{
    internal static partial class ApiUrlsLookups
    {
        internal static readonly ApiUrls CatAllPitSegments =
            new(new[] { "_cat/pit_segments/_all" });

        internal static readonly ApiUrls CatPitSegments = new(new[] { "_cat/pit_segments" });

        internal static readonly ApiUrls CatSegmentReplication =
            new(new[] { "_cat/segment_replication", "_cat/segment_replication/{index}" });

        internal static readonly ApiUrls ClusterDeleteComponentTemplate =
            new(new[] { "_component_template/{name}" });

        internal static readonly ApiUrls ClusterComponentTemplateExists =
            new(new[] { "_component_template/{name}" });

        internal static readonly ApiUrls ClusterGetComponentTemplate =
            new(new[] { "_component_template", "_component_template/{name}" });

        internal static readonly ApiUrls ClusterPutComponentTemplate =
            new(new[] { "_component_template/{name}" });

        internal static readonly ApiUrls NoNamespaceCreatePit =
            new(new[] { "{index}/_search/point_in_time" });

        internal static readonly ApiUrls NoNamespaceDeleteAllPits =
            new(new[] { "_search/point_in_time/_all" });

        internal static readonly ApiUrls NoNamespaceDeletePit =
            new(new[] { "_search/point_in_time" });

        internal static readonly ApiUrls NoNamespaceGetAllPits =
            new(new[] { "_search/point_in_time/_all" });

        internal static readonly ApiUrls IndicesDeleteComposableTemplate =
            new(new[] { "_index_template/{name}" });

        internal static readonly ApiUrls IndicesComposableTemplateExists =
            new(new[] { "_index_template/{name}" });

        internal static readonly ApiUrls IndicesGetComposableTemplate =
            new(new[] { "_index_template", "_index_template/{name}" });

        internal static readonly ApiUrls IndicesPutComposableTemplate =
            new(new[] { "_index_template/{name}" });

        internal static readonly ApiUrls IndicesStats =
            new(new[] { "_stats", "{index}/_stats", "{index}/_stats/{metric}", "_stats/{metric}" });
    }
}
