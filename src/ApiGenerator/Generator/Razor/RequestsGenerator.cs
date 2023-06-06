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

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiGenerator.Configuration;
using ApiGenerator.Domain;
using ShellProgressBar;

namespace ApiGenerator.Generator.Razor
{
	public class RequestsGenerator : RazorGeneratorBase
	{
		public override string Title => "OpenSearch.Client requests";

		public override async Task Generate(RestApiSpec spec, ProgressBar progressBar, CancellationToken token)
		{
			// Delete existing files
			foreach (var file in Directory.GetFiles(GeneratorLocations.OpenSearchClientFolder, "Requests.*.cs"))
				File.Delete(file);

			var view = ViewLocations.HighLevel("Requests", "PlainRequestBase.cshtml");
			var target = GeneratorLocations.HighLevel("Requests.cs");
			await DoRazor(spec, view, target, null, token);

			var dependantView = ViewLocations.HighLevel("Requests", "Requests.cshtml");
			string Target(string id) => GeneratorLocations.HighLevel($"Requests.{id}.cs");
			var namespaced = spec.EndpointsPerNamespaceHighLevel.ToList();
			await DoRazorDependantFiles(progressBar, namespaced, dependantView, kv => kv.Key, id => Target(id), token);
		}
	}
}
