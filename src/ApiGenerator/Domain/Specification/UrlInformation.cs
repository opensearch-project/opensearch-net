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
using Newtonsoft.Json;

namespace ApiGenerator.Domain.Specification
{

	// ReSharper disable once ClassNeverInstantiated.Global
	public class UrlInformation
	{
		public IDictionary<string, QueryParameters> Params { get; set; } = new SortedDictionary<string, QueryParameters>();

		[JsonProperty("paths")]
		private IReadOnlyCollection<string> OriginalPaths { get; set; }

		[JsonProperty("parts")]
		public IDictionary<string, UrlPart> OriginalParts { get; set; }

		[JsonProperty("deprecated_paths")]
		private IReadOnlyCollection<DeprecatedPath> DeprecatedPaths { get; set; }

		private List<UrlPath> _paths;
		public IReadOnlyCollection<UrlPath> Paths
		{
			get
			{
				if (_paths != null && _paths.Count > 0) return _paths;

				_paths = OriginalPaths.Select(p => new UrlPath(p, OriginalParts)).ToList();
				return _paths;
			}
		}

		private List<UrlPath> _pathsWithDeprecation;
		public IReadOnlyCollection<UrlPath> PathsWithDeprecations
		{
			get
			{
				if (_pathsWithDeprecation != null && _pathsWithDeprecation.Count > 0) return _pathsWithDeprecation;

				var paths = Paths ?? new UrlPath[] {};
				if (DeprecatedPaths == null || DeprecatedPaths.Count == 0) return Paths;
				if (OriginalParts == null) return Paths;

				//some deprecated paths describe aliases to the canonical using the same path e.g
				// PUT /{index}/_mapping/{type}
				// PUT /{index}/{type}/_mappings
				//
				//The following routine dedups these occasions and prefers either the canonical path
				//or the first duplicate deprecated path

				var canonicalPartNameLookup = paths.Select(path => new HashSet<string>(path.Parts.Select(p => p.Name))).ToList();
				var withoutDeprecatedAliases = DeprecatedPaths
					.Select(deprecatedPath => new
					{
						deprecatedPath,
						parts = new HashSet<string>(OriginalParts.Keys.Where(k => deprecatedPath.Path.Contains($"{{{k}}}")))
					})
					.GroupBy(t => t.parts, HashSet<string>.CreateSetComparer())
					.Where(grouped => !canonicalPartNameLookup.Any(set => set.SetEquals(grouped.Key)))
					.Select(grouped => grouped.First().deprecatedPath);

				_pathsWithDeprecation = paths
					.Concat(withoutDeprecatedAliases.Select(p => new UrlPath(p, OriginalParts, Paths)))
					.ToList();

				// now, check for and prefer deprecated URLs

				var finalPathsWithDeprecations = new List<UrlPath>(_pathsWithDeprecation.Count);

				foreach (var path in _pathsWithDeprecation)
				{
					if (path.Deprecation is null &&
						DeprecatedPaths.SingleOrDefault(p => p.Path.Equals(path.Path, StringComparison.OrdinalIgnoreCase)) is { } match)
					{
						finalPathsWithDeprecations.Add(new UrlPath(match, OriginalParts, Paths));
					}
					else
					{
						finalPathsWithDeprecations.Add(path);
					}
				}

				_pathsWithDeprecation = finalPathsWithDeprecations;

				return _pathsWithDeprecation;
			}
		}


		public IReadOnlyCollection<UrlPart> Parts => Paths.SelectMany(p => p.Parts).DistinctBy(p => p.Name).ToList();

		public bool IsPartless => !Parts.Any();

		private static readonly string[] DocumentApiParts = { "index", "id" };

		public bool IsDocumentApi => IsADocumentRoute(Parts);

		public static bool IsADocumentRoute(IReadOnlyCollection<UrlPart> parts) =>
			parts.Count() == DocumentApiParts.Length
			&& parts.All(p => DocumentApiParts.Contains(p.Name));


		public bool TryGetDocumentApiPath(out UrlPath path)
		{
			path = null;
			if (!IsDocumentApi) return false;

			var mostVerbosePath = _paths.OrderByDescending(p => p.Parts.Count()).First();
			path = new UrlPath(mostVerbosePath.Path, OriginalParts, mostVerbosePath.Parts);
			return true;
		}

	}
}
