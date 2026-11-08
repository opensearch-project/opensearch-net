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
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenSearch.OpenSearch.Xunit.Sdk
{
	internal class TestCollectionRunner : XunitTestCollectionRunner
	{
		private readonly Dictionary<Type, object> _assemblyFixtureMappings;
		private readonly IMessageSink _diagnosticMessageSink;

		public TestCollectionRunner(Dictionary<Type, object> assemblyFixtureMappings,
			ITestCollection testCollection,
			IEnumerable<IXunitTestCase> testCases,
			IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
			: base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator,
				cancellationTokenSource)
		{
			_assemblyFixtureMappings = assemblyFixtureMappings;
			_diagnosticMessageSink = diagnosticMessageSink;
		}

		protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class,
			IEnumerable<IXunitTestCase> testCases)
		{
			// whats this doing exactly??
			var combinedFixtures = new Dictionary<Type, object>(_assemblyFixtureMappings);
			foreach (var kvp in CollectionFixtureMappings)
				combinedFixtures[kvp.Key] = kvp.Value;

			// We've done everything we need, so hand back off to default Xunit implementation for class runner
			return new XunitTestClassRunner(testClass, @class, testCases, _diagnosticMessageSink, MessageBus,
					TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, combinedFixtures)
				.RunAsync();
		}
	}
}
