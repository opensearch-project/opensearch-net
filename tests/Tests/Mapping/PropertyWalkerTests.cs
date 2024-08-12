/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Mapping;

public class PropertyWalkerTests
{
    [U]
    public void BoolGetsMappedCorrectly()
    {
        var result = TestPropertyType<bool>();
        result.Should().Be("boolean");
    }

    [U]
    public void StringGetsMappedCorrectly()
    {
        var result = TestPropertyType<string>();
        result.Should().Be("text");
    }


    [U]
    public void DateTimeGetsMappedCorrectly()
    {
        var result = TestPropertyType<DateTime>();
        result.Should().Be("date");
    }

    [U]
    public void IEnumerableOfNullableGetsMappedCorrectly()
    {
        var result = TestPropertyType<IEnumerable<int?>>();
        result.Should().Be("integer");
    }

    [U]
    public void IListOfNullableGetsMappedCorrectly()
    {
        var result = TestPropertyType<IList<int?>>();
        result.Should().Be("integer");
    }

    [U]
    public void IEnumerableOfValueTypesGetsMappedCorrectly()
    {
        var result = TestPropertyType<IEnumerable<int>>();
        result.Should().Be("integer");
    }

    [U]
    public void IListOfValueTypesGetsMappedCorrectly()
    {
        var result = TestPropertyType<IList<int>>();
        result.Should().Be("integer");
    }

    private static string TestPropertyType<T>()
    {
        var walker = new PropertyWalker(typeof(Test<T>), null, 0);
        var properties = walker.GetProperties();
        var result = properties.SingleOrDefault();
        return result.Value?.Type;
    }


    private class Test<T>
    {
        public T Values { get; set; }
    }

}
