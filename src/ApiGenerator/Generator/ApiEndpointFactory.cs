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
*   http://www.apache.org/licenses/LICENSE-2.0
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
using System.Collections.Immutable;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using ApiGenerator.Configuration;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Domain;
using ApiGenerator.Domain.Code;
using ApiGenerator.Domain.Specification;
using NJsonSchema;
using NJsonSchema.References;
using NSwag;
using SemanticVersioning;
using Version = SemanticVersioning.Version;

namespace ApiGenerator.Generator
{
    public static class ApiEndpointFactory
    {
        public static ApiEndpoint From(string name, List<(string HttpPath, OpenApiPathItem Path, string HttpMethod, OpenApiOperation Operation)> variants)
	    {
	        var tokens = name.Split(".");
	        var methodName = tokens[^1];
	        var ns = tokens.Length > 1 ? tokens[0] : null;

	        HashSet<string> requiredPathParts = null;
	        var allParts = new Dictionary<string, UrlPart>();
			var canonicalPaths = new Dictionary<HashSet<string>, UrlPath>(HashSet<string>.CreateSetComparer());
			var deprecatedPaths = new Dictionary<HashSet<string>, UrlPath>(HashSet<string>.CreateSetComparer());
			var overloads = new List<(UrlPath Path, List<(string From, string To)> Renames)>();

	        foreach (var (httpPath, path, _, operation) in variants.DistinctBy(v => v.HttpPath))
			{
				var parts = new List<UrlPart>();
				var partNames = new HashSet<string>();
				var overloadedParts = new List<(string From, string To)>();

				foreach (var param in path.Parameters
							 .Concat(operation.Parameters)
							 .Where(p => p.Kind == OpenApiParameterKind.Path))
				{
					var partName = param.Name;
					if (!allParts.TryGetValue(partName, out var part))
					{
						part = allParts[partName] = new UrlPart
						{
							ClrTypeNameOverride = null,
							Deprecated = param.IsDeprecated,
							Description = param.Description,
							Name = partName,
							Type = GetOpenSearchType(param.Schema),
							Options = GetEnumOptions(param.Schema)
						};
					}
					partNames.Add(partName);
					parts.Add(part);

					if (param.Schema.XOverloadedParam() is {} overloadedParam) overloadedParts.Add((partName, overloadedParam));
				}

				parts.SortBy(p => httpPath.IndexOf($"{{{p.Name}}}", StringComparison.Ordinal));

				var urlPath = new UrlPath(httpPath, parts, GetDeprecation(operation), operation.XVersionAdded());
				(urlPath.Deprecation == null ? canonicalPaths : deprecatedPaths).TryAdd(partNames, urlPath);

				if (overloadedParts.Count > 0)
					overloads.Add((urlPath, overloadedParts));

				if (requiredPathParts != null)
	                requiredPathParts.IntersectWith(partNames);
	            else
	                requiredPathParts = partNames;
	        }

			foreach (var (path, renames) in overloads)
			{
				foreach (var (from, to) in renames)
				{
					var newPath = path.Path.Replace($"{{{from}}}", $"{{{to}}}");
					var newParts = path.Parts.Select(p => p.Name == from ? allParts[to] : p).ToList();
					var newPartNames = newParts.Select(p => p.Name).ToHashSet();
					var newUrlPath = new UrlPath(newPath, newParts, path.Deprecation, path.VersionAdded);
					(newUrlPath.Deprecation == null ? canonicalPaths : deprecatedPaths).TryAdd(newPartNames, newUrlPath);
				}
			}

			//some deprecated paths describe aliases to the canonical using the same path e.g
            // PUT /{index}/_mapping/{type}
            // PUT /{index}/{type}/_mappings
            //
            //The following routine dedups these occasions and prefers either the canonical path
            //or the first duplicate deprecated path

			var paths = canonicalPaths.Values
				.Concat(deprecatedPaths
					.Where(p => !canonicalPaths.ContainsKey(p.Key))
					.Select(p => p.Value))
				.ToList();
			paths.Sort((p1, p2) => p1.Parts
					.Zip(p2.Parts)
					.Select(t => string.Compare(t.First.Name, t.Second.Name, StringComparison.Ordinal))
					.SkipWhile(c => c == 0)
					.FirstOrDefault());

                // // now, check for and prefer deprecated URLs
                //
                // var finalPathsWithDeprecations = new List<UrlPath>(_pathsWithDeprecation.Count);
                //
                // foreach (var path in _pathsWithDeprecation)
                // {
                //     if (path.Deprecation is null &&
                //         DeprecatedPaths.SingleOrDefault(p => p.Path.Equals(path.Path, StringComparison.OrdinalIgnoreCase)) is { } match)
                //     {
                //         finalPathsWithDeprecations.Add(new UrlPath(match, OriginalParts, Paths));
                //     }
                //     else
                //     {
                //         finalPathsWithDeprecations.Add(path);
                //     }
                // }

			foreach (var partName in requiredPathParts ?? Enumerable.Empty<string>()) allParts[partName].Required = true;

	        var endpoint = new ApiEndpoint
	        {
	            Name = name,
	            Namespace = ns,
	            MethodName = methodName,
	            CsharpNames = new CsharpNames(name, methodName, ns),
	            Stability = Stability.Stable, // TODO: for realsies
	            OfficialDocumentationLink = new Documentation
	            {
	                Description = variants[0].Operation.Description,
	                Url = variants[0].Operation.ExternalDocumentation?.Url
	            },
	            Url = new UrlInformation
				{
					AllPaths = paths,
					Params = variants.SelectMany(v => v.Path.Parameters.Concat(v.Operation.Parameters))
						.Where(p => p.Kind == OpenApiParameterKind.Query)
						.DistinctBy(p => p.Name)
						.ToImmutableSortedDictionary(p => p.Name, BuildQueryParam)
				},
	            Body = variants
					.Select(v => v.Operation.RequestBody)
					.FirstOrDefault(b => b != null) is { } reqBody
					? new Body { Description = GetDescription(reqBody), Required = reqBody.IsRequired }
					: null,
	            HttpMethods = variants.Select(v => v.HttpMethod.ToString().ToUpper()).Distinct().ToList(),
	        };

	        LoadOverridesOnEndpoint(endpoint);
	        PatchRequestParameters(endpoint);

	        return endpoint;
	    }

	    private static void LoadOverridesOnEndpoint(ApiEndpoint endpoint)
	    {
	        var method = endpoint.CsharpNames.MethodName;
	        if (CodeConfiguration.ApiNameMapping.TryGetValue(endpoint.Name, out var mapsApiMethodName))
	            method = mapsApiMethodName;

	        var namespacePrefix = $"{typeof(GlobalOverrides).Namespace}.Endpoints.";
	        var typeName = $"{namespacePrefix}{method}Overrides";
	        var type = GeneratorLocations.Assembly.GetType(typeName);
	        if (type != null && Activator.CreateInstance(type) is IEndpointOverrides overrides)
	            endpoint.Overrides = overrides;
	    }

	    private static void PatchRequestParameters(ApiEndpoint endpoint) =>
	        endpoint.Url.Params = ApiQueryParametersPatcher.Patch(endpoint.Name, endpoint.Url.Params, endpoint.Overrides)
	            ?? throw new ArgumentNullException("ApiQueryParametersPatcher.Patch(endpoint.Name, endpoint.Url.Params, endpoint.Overrides)");

        private static QueryParameters BuildQueryParam(OpenApiParameter p)
        {
            var param = new QueryParameters
            {
                Type = GetOpenSearchType(p.Schema),
                Description = p.Description,
                Options = GetEnumOptions(p.Schema),
                Deprecated = GetDeprecation(p.Schema),
                VersionAdded = p.Schema.XVersionAdded(),
            };

            if (param.Type == "enum" && p.Schema.HasReference)
            {
                param.ClsName = ((IJsonReference)p.Schema).ReferencePath.Split('/').Last();
            }

            return param;
        }

	    private static string GetOpenSearchType(JsonSchema schema)
		{
			schema = schema.ActualSchema;

			if (schema.XDataType() is {} dataType)
				return dataType == "array" ? "list" : dataType;

	        return schema.Type switch
	        {
	            JsonObjectType.String when schema.Enumeration is { Count: > 0 } => "enum",
	            JsonObjectType.Integer => "number",
	            JsonObjectType.Array => "list",
	            var t => t.ToString().ToLowerInvariant()
	        };
	    }

	    private static IEnumerable<string> GetEnumOptions(JsonSchema schema) =>
			schema.ActualSchema.XEnumOptions()
			?? schema.ActualSchema.Enumeration?.Select(e => e.ToString())
			?? Enumerable.Empty<string>();

		private static Deprecation GetDeprecation(IJsonExtensionObject schema) =>
			(schema.XDeprecationMessage(), schema.XVersionDeprecated()) switch
			{
				(null, null) => null,
				var (m, v) => new Deprecation { Description = m, Version = v }
			};

		private static string GetDescription(OpenApiRequestBody requestBody)
		{
			if (!string.IsNullOrWhiteSpace(requestBody.Description))
				return requestBody.Description;

			return requestBody.Content.TryGetValue(MediaTypeNames.Application.Json, out var content)
				? content.Schema?.ActualSchema.Description
				: null;
		}

		private static string XDeprecationMessage(this IJsonExtensionObject schema) =>
			schema.GetExtension("x-deprecation-message") as string;

		private static string XVersionDeprecated(this IJsonExtensionObject schema) =>
			schema.GetExtension("x-version-deprecated") as string;

		private static Version XVersionAdded(this IJsonExtensionObject schema) =>
			schema.GetExtension("x-version-added") is string s
			? s.Split('.').Length switch
			{
				1 => new Version($"{s}.0.0"),
				2 => new Version($"{s}.0"),
				_ => new Version(s),
			}
			: null;

		private static string XDataType(this IJsonExtensionObject schema) =>
			schema.GetExtension("x-data-type") as string;

		private static string XOverloadedParam(this IJsonExtensionObject schema) =>
			schema.GetExtension("x-overloaded-param") as string;

		private static IEnumerable<string> XEnumOptions(this IJsonExtensionObject schema) =>
			schema.GetExtension("x-enum-options") is object[] opts ? opts.Cast<string>() : null;

	    private static object GetExtension(this IJsonExtensionObject schema, string key) =>
	        schema.ExtensionData?.TryGetValue(key, out var value) ?? false ? value : null;
	}
}
