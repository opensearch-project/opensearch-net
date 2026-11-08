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

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenSearch.OpenSearch.Xunit.Sdk
{
    public class OpenSearchTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
    {
        public OpenSearchTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider,
            IMessageSink diagnosticMessageSink, IXunitTestCollectionFactory collectionFactory = null) : base(
            assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory)
        {
            var a = Assembly.Load(new AssemblyName(assemblyInfo.Name));
            var options = a.GetCustomAttribute<OpenSearchXunitConfigurationAttribute>()?.Options ??
                          new OpenSearchXunitRunOptions();
            Options = options;
        }

        /// <summary>
        ///     The options for
        /// </summary>
        public OpenSearchXunitRunOptions Options { get; }

        protected override bool FindTestsForType(ITestClass testClass, bool includeSourceInformation,
            IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.Version), Options.Version);
            discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.RunIntegrationTests), Options.RunIntegrationTests);
            discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.IntegrationTestsMayUseAlreadyRunningNode),
                Options.IntegrationTestsMayUseAlreadyRunningNode);
            discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.RunUnitTests), Options.RunUnitTests);
            discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.TestFilter), Options.TestFilter);
            discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.ClusterFilter), Options.ClusterFilter);
            return base.FindTestsForType(testClass, includeSourceInformation, messageBus, discoveryOptions);
        }
    }
}
