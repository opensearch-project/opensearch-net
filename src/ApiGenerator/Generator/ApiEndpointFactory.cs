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
using ApiGenerator.Configuration;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Domain;
using ApiGenerator.Domain.Code;
using ApiGenerator.Domain.Specification;
using NJsonSchema;
using NSwag;

namespace ApiGenerator.Generator
{
    public static class ApiEndpointFactory
    {
        public static ApiEndpoint From(string name, List<(string HttpPath, OpenApiPathItem Path, string HttpMethod, OpenApiOperation Operation)> variants)
	    {
	        var tokens = name.Split(".");
	        var methodName = tokens[^1];
	        var ns = tokens.Length > 1 ? tokens[0] : null;

	        var urlInfo = new UrlInformation();
	        HashSet<string> requiredPathParams = null;
	        var allPathParams = new List<OpenApiParameter>();

	        foreach (var (httpPath, path, _, operation) in variants.DistinctBy(v => v.HttpPath))
	        {
	            urlInfo.OriginalPaths.Add(httpPath);
	            var pathParams = path.Parameters
	                .Concat(operation.Parameters)
	                .Where(p => p.Kind == OpenApiParameterKind.Path)
	                .ToList();
	            var paramNames = pathParams.Select(p => p.Name);
	            if (requiredPathParams != null)
	                requiredPathParams.IntersectWith(paramNames);
	            else
	                requiredPathParams = new HashSet<string>(paramNames);
	            allPathParams.AddRange(pathParams);
	        }

	        urlInfo.OriginalParts = allPathParams.DistinctBy(p => p.Name)
	            .Select(p => new UrlPart
	            {
	                ClrTypeNameOverride = null,
	                Deprecated = p.IsDeprecated,
	                Description = p.Description,
	                Name = p.Name,
	                Required = requiredPathParams?.Contains(p.Name) ?? false,
	                Type = GetOpenSearchType(p.Schema),
	                Options = GetEnumOptions(p.Schema),
	            })
	            .ToImmutableSortedDictionary(p => p.Name, p => p);

	        urlInfo.Params = variants.SelectMany(v => v.Path.Parameters.Concat(v.Operation.Parameters))
	            .Where(p => p.Kind == OpenApiParameterKind.Query)
	            .DistinctBy(p => p.Name)
	            .ToImmutableSortedDictionary(p => p.Name,
	                p => new QueryParameters
	                {
	                    Type = GetOpenSearchType(p.Schema),
	                    Description = p.Description,
	                    Options = GetEnumOptions(p.Schema),
	                    Deprecated = GetDeprecation(p.Schema)
	                });

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
	                Url = variants[0].Operation.ExternalDocumentation?.Url,
	            },
	            Url = urlInfo,
	            Body = variants.Select(v => v.Operation.RequestBody).FirstOrDefault(b => b != null) is {} reqBody ? new Body
	            {
	                Description = reqBody.Description,
	                Required = reqBody.IsRequired
	            } : null,
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

	    private static string GetOpenSearchType(JsonSchema schema)
	    {
	        while (schema.HasReference) schema = schema.Reference;

			if (schema.GetExtension("x-data-type") is string dataType)
				return dataType == "array" ? "list" : dataType;

	        return schema.Type switch
	        {
	            JsonObjectType.String when schema.Enumeration is { Count: > 0 } => "enum",
	            JsonObjectType.Integer => "number",
	            JsonObjectType.Array => "list",
	            var t => t.ToString().ToLowerInvariant()
	        };
	    }

	    private static IEnumerable<string> GetEnumOptions(JsonSchema schema)
	    {
	        while (schema.HasReference) schema = schema.Reference;

	        return schema.Enumeration?.Select(e => e.ToString()) ?? Enumerable.Empty<string>();
	    }

		private static QueryParameterDeprecation GetDeprecation(IJsonExtensionObject schema)
		{
			var message = schema.GetExtension("x-deprecation-message") as string;
			var version = schema.GetExtension("x-version-deprecated") as string;

			return message != null || version != null
				? new QueryParameterDeprecation { Description = message, Version = version }
				: null;
		}

	    private static object GetExtension(this IJsonExtensionObject schema, string key) =>
	        schema.ExtensionData?.TryGetValue(key, out var value) ?? false ? value : null;
	}
}
