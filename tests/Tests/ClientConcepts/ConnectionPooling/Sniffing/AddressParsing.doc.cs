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

using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;

namespace Tests.ClientConcepts.ConnectionPooling.Sniffing;

public class AddressParsing
{
    [U]
    public void IsMatched()
    {
        //based on examples from http://www.ietf.org/rfc/rfc2732.txt
        var testcases = new[,]
        {
            {"[::1]:9200", "[::1]", "9200"},
            {"192.168.2.1:231", "192.168.2.1", "231"},
            {"[FEDC:BA98:7654:3210:FEDC:BA98:7654:3210]:80", "[FEDC:BA98:7654:3210:FEDC:BA98:7654:3210]", "80"},
            {"[1080:0:0:0:8:800:200C:417A]:1234", "[1080:0:0:0:8:800:200C:417A]", "1234"},
            {"[3ffe:2a00:100:7031::1]:1", "[3ffe:2a00:100:7031::1]", "1"},
            {"[1080::8:800:200C:417A]:123", "[1080::8:800:200C:417A]", "123"},
            {"[::192.9.5.5]:12", "[::192.9.5.5]", "12"},
            {"[::FFFF:129.144.52.38]:80", "[::FFFF:129.144.52.38]", "80"},
            {"[2010:836B:4179::836B:4179]:34533", "[2010:836B:4179::836B:4179]", "34533"}
        };

        for (var i = 0; i < testcases.GetLength(0); i++)
        {
            var address = testcases[i, 0];
            var ip = testcases[i, 1];
            var port = testcases[i, 2];

            var match = SniffParser.AddressRegex.Match(address);

            match.Success.Should().BeTrue();

            match.Groups["ip"].Value.Should().BeEquivalentTo(ip);
            match.Groups["port"].Value.Should().BeEquivalentTo(port);
        }
    }

    [U]
    public void FqdnIsReadCorrectly()
    {
        //based on examples from http://www.ietf.org/rfc/rfc2732.txt
        var testcases = new[,]
        {
            {"helloworld/[::1]:9200", "helloworld", "[::1]", "9200"},
            {"opensearch.org/192.168.2.1:231", "opensearch.org", "192.168.2.1", "231"}
        };

        for (var i = 0; i < testcases.GetLength(0); i++)
        {
            var address = testcases[i, 0];
            var fqdn = testcases[i, 1];
            var ip = testcases[i, 2];
            var port = testcases[i, 3];

            var match = SniffParser.AddressRegex.Match(address);

            match.Success.Should().BeTrue();

            match.Groups["fqdn"].Value.Should().BeEquivalentTo(fqdn);
            match.Groups["ip"].Value.Should().BeEquivalentTo(ip);
            match.Groups["port"].Value.Should().BeEquivalentTo(port);
        }
    }
}
