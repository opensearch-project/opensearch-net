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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using ApiGenerator.Configuration;
using ApiGenerator.Domain.Specification;

namespace ApiGenerator.Domain
{
	public class EnumDescription
	{
		public string Name { get; set; }
		public IEnumerable<string> Options { get; set; }
	}

	public class RestApiSpec
	{
		public string Commit { get; set; }

		public static SortedDictionary<string, QueryParameters> CommonApiQueryParameters { get; set; }

		public IDictionary<string, ApiEndpoint> Endpoints { get; set; }

		public ImmutableSortedDictionary<string, ReadOnlyCollection<ApiEndpoint>> EndpointsPerNamespaceLowLevel =>
			Endpoints.Values.GroupBy(e=>e.CsharpNames.Namespace)
				.ToImmutableSortedDictionary(kv => kv.Key, kv => kv.ToList().AsReadOnly());

		public ImmutableSortedDictionary<string, ReadOnlyCollection<ApiEndpoint>> EndpointsPerNamespaceHighLevel =>
			Endpoints.Values
				.Where(v => !CodeConfiguration.IgnoreHighLevelApi(v.FileName))
				.GroupBy(e => e.CsharpNames.Namespace)
				.ToImmutableSortedDictionary(kv => kv.Key, kv => kv.ToList().AsReadOnly());


		private IEnumerable<EnumDescription> _enumDescriptions;
		public IEnumerable<EnumDescription> EnumsInTheSpec
		{
			get
			{
				if (_enumDescriptions != null) return _enumDescriptions;

				string CreateName(string name, string methodName, string @namespace)
				{
					if (
						name.ToLowerInvariant().Contains("metric")
						 ||(name.ToLowerInvariant() == "status")
						 ||(name.ToLowerInvariant() == "format")
					)
					{
						if (methodName.StartsWith(@namespace))
							return methodName + name;
						else
							return @namespace + methodName + name;
					}

					return name;
				}

				var urlParameterEnums = (
					from e in Endpoints.Values
					from para in e.Url.Params.Values
					where para.Options != null && para.Options.Any()
					let name = CreateName(para.ClsName, e.CsharpNames.MethodName, e.CsharpNames.Namespace)
					where name != "Time"
					select new EnumDescription
					{
						Name = name,
						Options = para.Options
					}).ToList();

				var urlPartEnums = (
					from e in Endpoints.Values
					from part in e.Url.Parts
					where part.Options != null && part.Options.Any()
					select new EnumDescription
					{
						Name = CreateName(part.Name.ToPascalCase(), e.CsharpNames.MethodName, e.CsharpNames.Namespace),
						Options = part.Options
					}).ToList();

				_enumDescriptions = urlPartEnums
					.Concat(urlParameterEnums)
					.DistinctBy(e => e.Name)
					.ToList();

				//TODO can be removed in 8.x
				var versionType = _enumDescriptions.FirstOrDefault(f => f.Name == "VersionType");
				if (versionType != null)
				{
					var options = new List<string>(versionType.Options);
					options.Add("force");
					versionType.Options = options;
				}

				return _enumDescriptions;
			}
		}
	}
}
