/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenSearch.OpenSearch.Xunit.XunitPlumbing
{
	/// <summary>
	///     An Xunit theory unit test
	/// </summary>
	[XunitTestCaseDiscoverer("OpenSearch.OpenSearch.Xunit.XunitPlumbing.TheoryUnitTestDiscoverer",
		"OpenSearch.OpenSearch.Xunit")]
	public class TU : TheoryAttribute
	{
	}

	public class TheoryUnitTestDiscoverer : UnitTestDiscoverer
	{
		private readonly TheoryDiscoverer _discoverer;

		public TheoryUnitTestDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink) =>
			_discoverer = new TheoryDiscoverer(diagnosticMessageSink);

		protected override IEnumerable<IXunitTestCase> DiscoverImpl(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod,
			IAttributeInfo factAttribute
		) => _discoverer.Discover(discoveryOptions, testMethod, factAttribute);
	}
}
