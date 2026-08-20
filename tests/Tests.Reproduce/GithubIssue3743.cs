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

using System.Linq;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using Newtonsoft.Json;
using Tests.Core.Serialization;

namespace Tests.Reproduce
{
	public class GithubIssue3743
	{
		[U]
		public void SerializesUnicodeEscapeSequences()
		{
			var value = new string(Enumerable.Range(0, 9727).Select(i => (char)i).ToArray());
			var doc = new { value };

			var internalJson = SerializationTester.Default.Client.SourceSerializer.SerializeToString(doc, formatting: SerializationFormatting.None);
			var jsonNet = JsonConvert.SerializeObject(doc, Formatting.None);

			// The built-in serializers emit valid JSON but escape some characters differently: the case of the hex
			// digits differs (json.net lowercases, utf8json uppercases), and the System.Text.Json engine
			// (UnsafeRelaxedJsonEscaping) escapes U+007F and the C1 control block that Json.NET writes raw. All variants
			// are valid and accepted, so rather than compare the raw escaping we verify both serializers preserve the
			// exact string value by decoding them back to it.
			var internalValue = JsonConvert.DeserializeAnonymousType(internalJson, new { value = "" }).value;
			var jsonNetValue = JsonConvert.DeserializeAnonymousType(jsonNet, new { value = "" }).value;

			internalValue.Should().Be(value);
			jsonNetValue.Should().Be(value);
		}
	}
}
