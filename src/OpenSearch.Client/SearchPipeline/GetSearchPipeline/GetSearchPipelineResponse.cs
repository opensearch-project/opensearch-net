/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	[JsonFormatter(typeof(DictionaryResponseFormatter<GetSearchPipelineResponse, string, ISearchPipeline>))]
	public class GetSearchPipelineResponse : DictionaryResponseBase<string, ISearchPipeline>
	{
		[IgnoreDataMember]
		public IReadOnlyDictionary<string, ISearchPipeline> Pipelines => Self.BackingDictionary;
	}
}
