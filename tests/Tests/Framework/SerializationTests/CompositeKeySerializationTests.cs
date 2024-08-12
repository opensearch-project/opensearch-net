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

using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Core.Extensions;

namespace Tests.Framework.SerializationTests;

public class CompositeKeySerializationTests
{
    [U]
    public void NullValuesAreSerialized()
    {
        var compositeKey = new CompositeKey(new Dictionary<string, object>
        {
            { "key_1", "value_1" },
            { "key_2", null },
        });

        var serializer = TestClient.Default.RequestResponseSerializer;
        var json = serializer.SerializeToString(compositeKey, TestClient.Default.ConnectionSettings.MemoryStreamFactory, SerializationFormatting.None);
        json.Should().Be("{\"key_1\":\"value_1\",\"key_2\":null}");

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            stream.Position = 0;
            var dictionary = serializer.Deserialize<IReadOnlyDictionary<string, object>>(stream);
            var deserializedCompositeKey = new CompositeKey(dictionary);
            compositeKey.Should().Equal(deserializedCompositeKey);
        }
    }
}
