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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.CodeStandards.Serialization;

public class Enums
{
    [U]
    public void EnumsWithEnumMembersShouldBeMarkedWithStringEnumAttribute()
    {
        var exceptions = new HashSet<Type>
        {
            typeof(SimpleQueryStringFlags)
        };

        var enums = typeof(IOpenSearchClient).Assembly
            .GetTypes()
            .Where(t => t.IsEnum)
            .Except(exceptions)
            .ToList();
        var notMarkedStringEnum = new List<string>();
        foreach (var e in enums)
        {
            if (e.GetFields().Any(fi => fi.FieldType == e && fi.GetCustomAttribute<EnumMemberAttribute>() != null)
                && e.GetCustomAttribute<StringEnumAttribute>() == null)
                notMarkedStringEnum.Add(e.Name);
        }
        notMarkedStringEnum.Should().BeEmpty();
    }

    [U]
    public void CanSerializeEnumsWithMultipleMembersMappedToSameValue()
    {
        var document = new EnumSameValuesDocument
        {
            Int = HttpStatusCode.Moved,
            String = AnotherEnum.Value1
        };

        var client = new OpenSearchClient();
        var json = client.RequestResponseSerializer.SerializeToString(document);

        // "Value2" will be written for both "Value1" and "Value2" because the underlying integer value
        // for both is the same, and "Value2" field member is listed after "Value1", overwriting
        // the value mapping.
        //
        // Json.Net behaves similarly, except the first string mapped for a value
        // is not overwritten i.e. "Value1" will be written for both "Value1" and "Value2"
        json.Should().Be("{\"int\":301,\"string\":\"Value2\"}");

        EnumSameValuesDocument deserializedDocument;
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            deserializedDocument = client.RequestResponseSerializer.Deserialize<EnumSameValuesDocument>(stream);

        deserializedDocument.Int.Should().Be(document.Int);
        deserializedDocument.String.Should().Be(document.String);
    }

    [U]
    public void CanSerializeEnumPropertiesWithStringEnumAttribute()
    {
        var httpStatusCode = HttpStatusCode.OK;
        var document = new StringEnumDocument
        {
            Int = httpStatusCode,
            String = httpStatusCode,
            NullableString = httpStatusCode
        };

        var client = new OpenSearchClient();
        var json = client.RequestResponseSerializer.SerializeToString(document);

        json.Should().Be("{\"int\":200,\"string\":\"OK\",\"nullableString\":\"OK\"}");

        StringEnumDocument deserializedDocument;
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            deserializedDocument = client.RequestResponseSerializer.Deserialize<StringEnumDocument>(stream);

        deserializedDocument.Int.Should().Be(document.Int);
        deserializedDocument.String.Should().Be(document.String);
        deserializedDocument.NullableString.Should().Be(document.NullableString);
    }

    private class StringEnumDocument
    {
        public HttpStatusCode Int { get; set; }

        [StringEnum]
        public HttpStatusCode String { get; set; }

        [StringEnum]
        public HttpStatusCode? NullableString { get; set; }
    }

    private class EnumSameValuesDocument
    {
        public HttpStatusCode Int { get; set; }

        public AnotherEnum String { get; set; }
    }

    [StringEnum]
    public enum AnotherEnum : int
    {
        Value1 = 1,
        Value2 = 1
    }
}
