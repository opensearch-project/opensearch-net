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
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenSearch.OpenSearch.Xunit.XunitPlumbing;

/// <summary>
///     Base test discoverer used to discover tests cases attached
///     to test methods that are attributed with <see cref="T:Xunit.FactAttribute" /> (or a subclass).
/// </summary>
public abstract class OpenSearchTestCaseDiscoverer : IXunitTestCaseDiscoverer
{
    protected readonly IMessageSink DiagnosticMessageSink;
    private readonly FactDiscoverer _factDiscoverer;

    protected OpenSearchTestCaseDiscoverer(IMessageSink diagnosticMessageSink)
    {
        DiagnosticMessageSink = diagnosticMessageSink;
        _factDiscoverer = new FactDiscoverer(diagnosticMessageSink);
    }

    /// <inheritdoc />
    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod, IAttributeInfo factAttribute) =>
        SkipMethod(discoveryOptions, testMethod, out var skipReason)
            ? string.IsNullOrEmpty(skipReason)
                ? Enumerable.Empty<IXunitTestCase>()
                : new IXunitTestCase[] { new SkippingTestCase(skipReason, testMethod, null) }
            : DiscoverImpl(discoveryOptions, testMethod, factAttribute);

    protected virtual IEnumerable<IXunitTestCase> DiscoverImpl(ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod, IAttributeInfo factAttribute
    ) => _factDiscoverer.Discover(discoveryOptions, testMethod, factAttribute);

    /// <summary>
    ///     Detemines whether a test method should be skipped, and the reason why
    /// </summary>
    /// <param name="discoveryOptions">The discovery options</param>
    /// <param name="testMethod">The test method</param>
    /// <param name="skipReason">The reason to skip</param>
    /// <returns></returns>
    protected virtual bool SkipMethod(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod,
        out string skipReason)
    {
        skipReason = null;
        return false;
    }

    protected static TValue GetAttribute<TAttribute, TValue>(ITestMethod testMethod, string propertyName)
        where TAttribute : Attribute
    {
        var classAttributes = testMethod.TestClass.Class.GetCustomAttributes(typeof(TAttribute)) ??
                              Enumerable.Empty<IAttributeInfo>();
        var methodAttributes = testMethod.Method.GetCustomAttributes(typeof(TAttribute)) ??
                               Enumerable.Empty<IAttributeInfo>();
        var attribute = classAttributes.Concat(methodAttributes).FirstOrDefault();
        return attribute == null ? default(TValue) : attribute.GetNamedArgument<TValue>(propertyName);
    }

    protected static IList<IAttributeInfo> GetAttributes<TAttribute>(ITestMethod testMethod)
        where TAttribute : Attribute
    {
        var classAttributes = testMethod.TestClass.Class.GetCustomAttributes(typeof(TAttribute)) ??
                              Enumerable.Empty<IAttributeInfo>();
        var methodAttributes = testMethod.Method.GetCustomAttributes(typeof(TAttribute)) ??
                               Enumerable.Empty<IAttributeInfo>();
        return classAttributes.Concat(methodAttributes).ToList();
    }

    protected static IEnumerable<TValue> GetAttributes<TAttribute, TValue>(ITestMethod testMethod,
        string propertyName)
        where TAttribute : Attribute
    {
        var classAttributes = testMethod.TestClass.Class.GetCustomAttributes(typeof(TAttribute));
        var methodAttributes = testMethod.Method.GetCustomAttributes(typeof(TAttribute));
        return classAttributes
            .Concat(methodAttributes)
            .Select(a => a.GetNamedArgument<TValue>(propertyName));
    }
}
