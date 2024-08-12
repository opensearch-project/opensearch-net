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
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Framework.SerializationTests;

public class ExceptionSerializationTests
{
    private readonly IOpenSearchSerializer _opensearchNetSerializer;

    private readonly Exception _exception = new Exception("outer_exception",
        new InnerException("inner_exception",
            new InnerInnerException("inner_inner_exception")));

    public ExceptionSerializationTests()
    {
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var connection = new InMemoryConnection();
        var values = new ConnectionConfiguration(pool, connection);
        var lowlevelClient = new OpenSearchLowLevelClient(values);
        _opensearchNetSerializer = lowlevelClient.Serializer;
    }

    [U]
    public void LowLevelExceptionSerializationMatchesJsonNet()
    {
        var serialized = _opensearchNetSerializer.SerializeToString(_exception);

        object CreateException(Type exceptionType, string message, int depth)
        {
            return new
            {
                Depth = depth,
                ClassName = exceptionType.FullName,
                Message = message,
                Source = (object)null,
                StackTraceString = (object)null,
                RemoteStackTraceString = (object)null,
                RemoteStackIndex = 0,
                HResult = -2146233088,
                HelpURL = (object)null
            };
        }

        var simpleJsonException = new[]
        {
            CreateException(typeof(Exception), "outer_exception", 0),
            CreateException(typeof(InnerException), "inner_exception", 1),
            CreateException(typeof(InnerInnerException), "inner_inner_exception", 2),
        };

        var jArray = JArray.Parse(serialized);
        var jArray2 = JArray.Parse(JsonConvert.SerializeObject(simpleJsonException));

        JToken.DeepEquals(jArray, jArray2).Should().BeTrue();
    }

    public class InnerException : Exception
    {
        public InnerException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InnerInnerException : Exception
    {
        public InnerInnerException(string message) : base(message) { }
    }
}
