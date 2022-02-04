/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
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

using System.Text;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using Osc;
using Tests.Core.Client;

namespace Tests.CodeStandards.Serialization
{
	public class GeoLocationTests
	{
		[U]
		public void CanDeserializeAndSerializeToWellKnownText()
		{
			var wkt = "{\"location\":\"POINT (-90 90)\"}";
			var client = TestClient.DisabledStreaming;

			Doc deserialized;
			using (var stream = RecyclableMemoryStreamFactory.Default.Create(Encoding.UTF8.GetBytes(wkt)))
				deserialized = client.RequestResponseSerializer.Deserialize<Doc>(stream);

			deserialized.Location.Should().Be(new GeoLocation(90, -90));
			client.RequestResponseSerializer.SerializeToString(deserialized).Should().Be(wkt);
		}

		private class Doc
		{
			public GeoLocation Location { get; set; }
		}
	}
}
