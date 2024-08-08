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
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;

namespace OpenSearch.Stack.ArtifactsApi.Products
{
    /// <summary> An OpenSearch plugin </summary>
    public class OpenSearchPlugin : SubProduct
    {
        public OpenSearchPlugin(string plugin, Func<OpenSearchVersion, bool> isValid = null,
            Func<OpenSearchVersion, string> listName = null)
            : base(plugin, isValid, listName) =>
            PlatformDependent = false;  // ReSharper disable InconsistentNaming
        public static OpenSearchPlugin AnalysisIcu { get; } = new("analysis-icu");
        public static OpenSearchPlugin AnalysisKuromoji { get; } = new("analysis-kuromoji");
        public static OpenSearchPlugin AnalysisNori { get; } = new("analysis-nori");
        public static OpenSearchPlugin AnalysisPhonetic { get; } = new("analysis-phonetic");
        public static OpenSearchPlugin AnalysisSmartCn { get; } = new("analysis-smartcn");
        public static OpenSearchPlugin AnalysisStempel { get; } = new("analysis-stempel");
        public static OpenSearchPlugin AnalysisUkrainian { get; } = new("analysis-ukrainian");

        public static OpenSearchPlugin DiscoveryAzureClassic { get; } = new("discovery-azure-classic");

        public static OpenSearchPlugin DiscoveryEC2 { get; } = new("discovery-ec2");
        public static OpenSearchPlugin DiscoveryFile { get; } = new("discovery-file");
        public static OpenSearchPlugin DiscoveryGCE { get; } = new("discovery-gce");

        public static OpenSearchPlugin IngestAttachment { get; } = new("ingest-attachment", version => version >= "1.0.0");

        public static OpenSearchPlugin IngestGeoIp { get; } =
            new("ingest-geoip", version => version >= "1.0.0")
            {
                ShippedByDefaultAsOf = "1.0.0"
            };

        public static OpenSearchPlugin IngestUserAgent { get; } =
            new("ingest-user-agent", version => version >= "1.0.0")
            {
                ShippedByDefaultAsOf = "1.0.0"
            };

        public static OpenSearchPlugin MapperAttachment { get; } = new("mapper-attachments");
        public static OpenSearchPlugin MapperMurmur3 { get; } = new("mapper-murmur3");
        public static OpenSearchPlugin MapperSize { get; } = new("mapper-size");

        public static OpenSearchPlugin RepositoryAzure { get; } = new("repository-azure");
        public static OpenSearchPlugin RepositoryGCS { get; } = new("repository-gcs");
        public static OpenSearchPlugin RepositoryHDFS { get; } = new("repository-hdfs");
        public static OpenSearchPlugin RepositoryS3 { get; } = new("repository-s3");

        public static OpenSearchPlugin Security { get; } = new("opensearch-security");

        public static OpenSearchPlugin StoreSMB { get; } = new("store-smb");

        public static OpenSearchPlugin DeleteByQuery { get; } = new("delete-by-query", version => version < "1.0.0");

        public static OpenSearchPlugin Knn { get; } = new("opensearch-knn");
    }
}
