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
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using ApiGenerator.Configuration;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Domain;
using ApiGenerator.Domain.Code;
using ApiGenerator.Domain.Specification;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.References;
using NSwag;
using Version = SemanticVersioning.Version;

namespace ApiGenerator.Generator
{
	public static class ApiEndpointFactory
	{
		public static ApiEndpoint From(
			string name,
			List<(string HttpPath, OpenApiPathItem Path, string HttpMethod, OpenApiOperation Operation)> variants
		)
		{
			var tokens = name.Split(".");
			var methodName = tokens[^1];
			var ns = tokens.Length > 1 ? tokens[0] : null;
			var names = new CsharpNames(name, methodName, ns);
			var overrides = LoadOverrides(name, names.MethodName);

			HashSet<string> requiredPathParts = null;
			var allParts = new Dictionary<string, UrlPart>();
			var canonicalPaths = new Dictionary<HashSet<string>, UrlPath>(HashSet<string>.CreateSetComparer());
			var deprecatedPaths = new Dictionary<HashSet<string>, UrlPath>(HashSet<string>.CreateSetComparer());

			foreach (var (httpPath, path, _, operation) in variants.DistinctBy(v => v.HttpPath))
			{
				var parts = new List<UrlPart>();
				var partNames = new HashSet<string>();

				foreach (var param in path.Parameters
							 .Concat(operation.Parameters)
							 .Select(p => p.ActualParameter)
							 .Where(p => p.Kind == OpenApiParameterKind.Path))
				{
					var partName = param.Name;
					if (!allParts.TryGetValue(partName, out var part))
					{
						var (type, options) = GetOpenSearchType(param.Schema);
						part = allParts[partName] = new UrlPart
						{
							ClrTypeNameOverride = null,
							Deprecated = param.IsDeprecated,
							Description = param.Description?.SanitizeDescription(),
							Name = partName,
							Type = type,
							Options = options
						};
					}
					partNames.Add(partName);
					parts.Add(part);
				}

				parts.SortBy(p => httpPath.IndexOf($"{{{p.Name}}}", StringComparison.Ordinal));

				var urlPath = new UrlPath(httpPath, parts, GetDeprecation(operation), operation.XVersionAdded());
				(urlPath.Deprecation == null ? canonicalPaths : deprecatedPaths).TryAdd(partNames, urlPath);

				if (requiredPathParts != null)
					requiredPathParts.IntersectWith(partNames);
				else
					requiredPathParts = partNames;
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

			ApiRequestParametersPatcher.PatchUrlPaths(name, paths, overrides);

			paths.Sort((p1, p2) => p1.Parts
				.Zip(p2.Parts)
				.Select(t => string.Compare(t.First.Name, t.Second.Name, StringComparison.Ordinal))
				.SkipWhile(c => c == 0)
				.FirstOrDefault());

			foreach (var partName in requiredPathParts ?? Enumerable.Empty<string>()) allParts[partName].Required = true;

			IDictionary<string, QueryParameters> queryParams = variants.SelectMany(v => v.Path.Parameters.Concat(v.Operation.Parameters))
				.Select(p => p.ActualParameter)
				.Where(p => p.Kind == OpenApiParameterKind.Query)
				.DistinctBy(p => p.Name)
				.ToDictionary(p => p.Name, BuildQueryParam);
			queryParams = ApiRequestParametersPatcher.PatchQueryParameters(name, queryParams, overrides);

			Body body = null;
			if (variants.Select(v => v.Operation.RequestBody).FirstOrDefault() is {} requestBody)
			{
				body = new Body
				{
					Description = GetDescription(requestBody)?.SanitizeDescription(),
					Required = requestBody.IsRequired
				};
			}

			return new ApiEndpoint
			{
				Name = name,
				Namespace = ns,
				MethodName = methodName,
				CsharpNames = names,
				Overrides = overrides,
				Stability = Stability.Stable, // TODO: for realsies
				OfficialDocumentationLink = new Documentation
				{
					Description = variants[0].Operation.Description?.SanitizeDescription(),
					Url = variants[0].Operation.ExternalDocumentation?.Url
				},
				Url = new UrlInformation { AllPaths = paths, Params = queryParams },
				Body = body,
				HttpMethods = variants.Select(v => v.HttpMethod.ToString().ToUpper()).Distinct().ToList(),
			};
		}

		private static IEndpointOverrides LoadOverrides(string endpointName, string methodName)
		{
			if (CodeConfiguration.ApiNameMapping.TryGetValue(endpointName, out var mapsApiMethodName))
				methodName = mapsApiMethodName;

			var namespacePrefix = $"{typeof(GlobalOverrides).Namespace}.Endpoints.";
			var typeName = $"{namespacePrefix}{methodName}Overrides";
			var type = GeneratorLocations.Assembly.GetType(typeName);

			return type != null && Activator.CreateInstance(type) is IEndpointOverrides overrides ? overrides : null;
		}

		private static QueryParameters BuildQueryParam(OpenApiParameter p)
		{
			var schema = p.Schema.ActualSchema;

			var (type, options) = GetOpenSearchType(schema);

			if (type == "enum" && p.Schema.HasReference)
			{
				type = ((IJsonReference)p.Schema).ReferencePath
					!.Split('/')
					.Last()
					.Replace("_common", "")
					.SplitPascalCase()
					.ToPascalCase() + "?";
			}

			var param = new QueryParameters
			{
				Type = type,
				Description = p.Description?.SanitizeDescription(),
				Deprecated = GetDeprecation(p) ?? GetDeprecation(schema),
				VersionAdded = p.XVersionAdded(),
			};

			return param;
		}

		private static (string Type, IEnumerable<string> EnumOptions) GetOpenSearchType(JsonSchema schema)
		{
			schema = schema.ActualSchema;

			if (schema.OneOf is { Count: > 0 })
			{
				var oneOf = schema.OneOf.ToArray();

				if (oneOf.Length == 2)
				{
					var first = GetOpenSearchType(oneOf[0]);
					var second = GetOpenSearchType(oneOf[1]);
					switch (first.Type, second.Type)
					{
						case ("enum", "list"): return first;
						case ("string", "list"): return second;
						case ("boolean", "string"): return first;
						case ("string", "number"): return first;
						case ("number", "enum"): return ("string", null);
					}
				}
			}

			var enumOptions = (schema.XEnumOptions()
				?? schema.Enumeration.Select(e => e.ToString())).ToList();

			if (schema.XDataType() is { } dataType)
				return (dataType == "array" ? "list" : dataType, enumOptions);

			return schema.Type switch
			{
				JsonObjectType.String when enumOptions is { Count: > 0 } => ("enum", enumOptions),
				JsonObjectType.Integer => ("number", null),
				JsonObjectType.Array => ("list", enumOptions),
				var t => (t.ToString().ToLowerInvariant(), null)
			};
		}

		private static Deprecation GetDeprecation(IJsonExtensionObject schema) =>
			(schema.XDeprecationMessage(), schema.XVersionDeprecated()) switch
			{
				(null, null) => null,
				var (m, v) => new Deprecation { Description = m?.SanitizeDescription(), Version = v }
			};

		private static string GetDescription(OpenApiRequestBody requestBody)
		{
			if (!string.IsNullOrWhiteSpace(requestBody.Description))
				return requestBody.Description;

			return requestBody.Content.TryGetValue(MediaTypeNames.Application.Json, out var content)
				? content.Schema?.ActualSchema.Description
				: null;
		}

		private static string SanitizeDescription(this string description)
		{
			if (string.IsNullOrWhiteSpace(description)) return null;

			description = Regex.Replace(description, @"\s+", " ");

			if (!description.EndsWith('.')) description += '.';

			return description;
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

		private static IEnumerable<string> XEnumOptions(this IJsonExtensionObject schema) =>
			schema.GetExtension("x-enum-options") is object[] opts ? opts.Cast<string>() : null;

		private static object GetExtension(this IJsonExtensionObject schema, string key) =>
			schema.ExtensionData?.TryGetValue(key, out var value) ?? false ? value : null;
	}
}
