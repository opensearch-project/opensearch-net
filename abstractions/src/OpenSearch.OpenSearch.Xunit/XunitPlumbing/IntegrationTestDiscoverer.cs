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
using OpenSearch.OpenSearch.Xunit.Sdk;
using OpenSearch.Stack.ArtifactsApi;
using SemanticVersioning;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Enumerable = System.Linq.Enumerable;

namespace OpenSearch.OpenSearch.Xunit.XunitPlumbing
{
    /// <summary>
    ///     An Xunit test that should be skipped, and a reason why.
    /// </summary>
    public abstract class SkipTestAttributeBase : Attribute
    {
        /// <summary>
        ///     Whether the test should be skipped
        /// </summary>
        public abstract bool Skip { get; }

        /// <summary>
        ///     The reason why the test should be skipped
        /// </summary>
        public abstract string Reason { get; }
    }

    /// <summary>
    ///     An Xunit integration test
    /// </summary>
    [XunitTestCaseDiscoverer("OpenSearch.OpenSearch.Xunit.XunitPlumbing.IntegrationTestDiscoverer",
        "OpenSearch.OpenSearch.Xunit")]
    public class I : FactAttribute
    {
    }

    /// <summary>
    ///     A test discoverer used to discover integration tests cases attached
    ///     to test methods that are attributed with <see cref="I" /> attribute
    /// </summary>
    public class IntegrationTestDiscoverer : OpenSearchTestCaseDiscoverer
    {
        public IntegrationTestDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
        {
        }

        /// <inheritdoc />
        protected override bool SkipMethod(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod,
            out string skipReason)
        {
            skipReason = null;
            var runIntegrationTests =
                discoveryOptions.GetValue<bool>(nameof(OpenSearchXunitRunOptions.RunIntegrationTests));
            if (!runIntegrationTests) return true;

            var cluster = TestAssemblyRunner.GetClusterForClass(testMethod.TestClass.Class);
            if (cluster == null)
            {
                skipReason +=
                    $"{testMethod.TestClass.Class.Name} does not define a cluster through IClusterFixture or {nameof(IntegrationTestClusterAttribute)}";
                return true;
            }

            var openSearchVersion =
                discoveryOptions.GetValue<OpenSearchVersion>(nameof(OpenSearchXunitRunOptions.Version));

            // Skip if the version we are testing against is attributed to be skipped do not run the test nameof(SkipVersionAttribute.Ranges)
            var skipVersionAttribute = Enumerable.FirstOrDefault(GetAttributes<SkipVersionAttribute>(testMethod));
            if (skipVersionAttribute != null)
            {
                var skipVersionRanges =
                    skipVersionAttribute.GetNamedArgument<IList<Range>>(nameof(SkipVersionAttribute.Ranges)) ??
                    new List<Range>();
                if (openSearchVersion == null && skipVersionRanges.Count > 0)
                {
                    skipReason = $"{nameof(SkipVersionAttribute)} has ranges defined for this test but " +
                                 $"no {nameof(OpenSearchXunitRunOptions.Version)} has been provided to {nameof(OpenSearchXunitRunOptions)}";
                    return true;
                }

                if (openSearchVersion != null)
                {
                    var reason = skipVersionAttribute.GetNamedArgument<string>(nameof(SkipVersionAttribute.Reason));
                    for (var index = 0; index < skipVersionRanges.Count; index++)
                    {
                        var range = skipVersionRanges[index];
                        // inrange takes prereleases into account
                        if (!openSearchVersion.InRange(range)) continue;
                        skipReason =
                            $"{nameof(SkipVersionAttribute)} has range {range} that {openSearchVersion} satisfies";
                        if (!string.IsNullOrWhiteSpace(reason)) skipReason += $": {reason}";
                        return true;
                    }
                }
            }

            var skipTests = GetAttributes<SkipTestAttributeBase>(testMethod)
                .FirstOrDefault(a => a.GetNamedArgument<bool>(nameof(SkipTestAttributeBase.Skip)));

            if (skipTests == null) return false;

            skipReason = skipTests.GetNamedArgument<string>(nameof(SkipTestAttributeBase.Reason));
            return true;
        }
    }
}
