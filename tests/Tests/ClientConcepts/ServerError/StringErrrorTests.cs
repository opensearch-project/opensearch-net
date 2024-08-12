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

namespace Tests.ClientConcepts.ServerError;

public class StringErrorTests : ServerErrorTestsBase
{
    protected override string Json => @"""alias [x] is missing""";

    [U] protected override void AssertServerError() => base.AssertServerError();

    protected override void AssertResponseError(string origin, Error error)
    {
        error.Type.Should().BeNullOrEmpty(origin);
        error.Reason.Should().NotBeNullOrWhiteSpace(origin).And.Contain("is missing");
        error.RootCause.Should().BeNull(origin);
    }
}

public class TempErrorTests : ServerErrorTestsBase
{
    protected override string Json =>
        @"{""root_cause"":[{""type"":""index_not_found_exception"",""reason"":""no such index"",""index_uuid"":""_na_"",""index"":""non-existent-index""}],""type"":""index_not_found_exception"",""reason"":""no such index"",""index_uuid"":""_na_"",""index"":""non-existent-index""}";

    [U] protected override void AssertServerError() => base.AssertServerError();

    protected override void AssertResponseError(string origin, Error error)
    {
        error.Type.Should().NotBeNullOrEmpty(origin);
        error.Reason.Should().NotBeNullOrWhiteSpace(origin);
        error.RootCause.Should().NotBeNull(origin).And.HaveCount(1);
    }
}
