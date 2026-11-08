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

using System.Text;

namespace OpenSearch.Net
{
    internal sealed class MetaDataHeader
    {
        private const char _separator = ',';

        private readonly string _headerValue;

        public MetaDataHeader(VersionInfo version, string serviceIdentifier, bool isAsync)
        {
            ClientVersion = version.ToString();
            RuntimeVersion = new RuntimeVersionInfo().ToString();
            ServiceIdentifier = serviceIdentifier;

            // This code is expected to be called infrequently so we're not concerns with over optimising this

            _headerValue = new StringBuilder(64)
                .Append(serviceIdentifier).Append("=").Append(ClientVersion).Append(_separator)
                .Append("a=").Append(isAsync ? "1" : "0").Append(_separator)
                .Append("net=").Append(RuntimeVersion).Append(_separator)
                .Append(_httpClientIdentifier).Append("=").Append(RuntimeVersion)
                .ToString();
        }

        private static readonly string _httpClientIdentifier =
            ConnectionInfo.UsingCurlHandler ? "cu" : "so";

        public string ServiceIdentifier { get; private set; }
        public string ClientVersion { get; private set; }
        public string RuntimeVersion { get; private set; }

        public override string ToString() => _headerValue;
    }
}
