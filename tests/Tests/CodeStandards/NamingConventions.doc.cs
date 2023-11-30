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
using System.Globalization;
using System.Linq;
using System.Reflection;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Framework;

namespace Tests.CodeStandards
{
	/** == Naming Conventions
	*
	* OSC uses the following naming conventions (with _some_ exceptions).
	*/
	public class NamingConventions
	{
		/** === Class Names
		*
		* Abstract class names should end with a `Base` suffix
		*/
		[U] public void AbstractClassNamesEndWithBase()
		{
			var exceptions = new[]
			{
				typeof(DateMath)
			};

			var abstractClassesNotEndingInBase = typeof(IRequest).Assembly.GetTypes()
				.Where(t => t.IsClass && t.IsAbstract && !t.IsSealed && !exceptions.Contains(t))
				//when testing nuget package against merged internalize json.net skip its types.
				.Where(t => !t.Namespace.StartsWith("OpenSearch.Client.Json"))
				.Where(t => !t.Namespace.StartsWith("OpenSearch.Internal"))
				.Where(t => !t.Name.Split('`')[0].EndsWith("Base"))
				.Select(t => t.Name.Split('`')[0])
				.ToList();

			abstractClassesNotEndingInBase.Should().BeEmpty();
		}

		/**
		* Class names that end with `Base` suffix are abstract
		*/
		[U] public void ClassNameContainsBaseShouldBeAbstract()
		{
			var exceptions = new[] { typeof(DateMath) };

			var baseClassesNotAbstract = typeof(IRequest).Assembly.GetTypes()
				.Where(t => t.IsClass && !exceptions.Contains(t))
				.Where(t => t.Name.Split('`')[0].EndsWith("Base"))
				.Where(t => !t.IsAbstract)
				.Select(t => t.Name.Split('`')[0])
				.ToList();

			baseClassesNotAbstract.Should().BeEmpty();
		}

		/** === Requests and Responses
		*
		* Request class names should end with `Request`
		*/
		[U]
		public void RequestClassNamesEndWithRequest()
		{
			var types = typeof(IRequest).Assembly.GetTypes();
			var requestsNotEndingInRequest = types
				.Where(t => typeof(IRequest).IsAssignableFrom(t) && !t.IsAbstract)
				.Where(t => !typeof(IDescriptor).IsAssignableFrom(t))
				.Where(t => !t.Name.Split('`')[0].EndsWith("Request"))
				.Select(t => t.Name.Split('`')[0])
				.ToList();

			requestsNotEndingInRequest.Should().BeEmpty();
		}

		/**
		* Response class names should end with `Response`
		**/
		[U]
		public void ResponseClassNamesEndWithResponse()
		{
			var types = typeof(IRequest).Assembly.GetTypes();
			var responsesNotEndingInResponse = types
				.Where(t => typeof(IResponse).IsAssignableFrom(t) && !t.IsAbstract)
				.Where(t => !t.Name.Split('`')[0].EndsWith("Response"))
				.Select(t => t.Name.Split('`')[0])
				.ToList();

			responsesNotEndingInResponse.Should().BeEmpty();
		}

		/**
		* Request and Response class names should be one to one in *most* cases.
		* e.g. `ValidateRequest` => `ValidateResponse`, and not `ValidateQueryRequest` => `ValidateResponse`
		* There are a few exceptions to this rule, most notably the `Cat` prefixed requests and
		* the `Exists` requests.
		*/
		[U]
		public void ParityBetweenRequestsAndResponses()
		{
			var exceptions = new[] // <1> _Exceptions to the rule_
			{
				//TODO MAP THIS
				//typeof(RankEvalRequest),
				//TODO add unit tests that we have no requests starting with Exists
				typeof(HttpDeleteRequest),
				typeof(HttpGetRequest),
				typeof(HttpHeadRequest),
				typeof(HttpPatchRequest),
				typeof(HttpPostRequest),
				typeof(HttpPutRequest),

				typeof(SourceExistsRequest),
				typeof(SourceExistsRequest<>),
				typeof(DocumentExistsRequest),
				typeof(DocumentExistsRequest<>),
				typeof(AliasExistsRequest),
				typeof(IndexExistsRequest),
				typeof(TypeExistsRequest),
				typeof(IndexTemplateExistsRequest),
				typeof(ComponentTemplateExistsRequest),
				typeof(ComposableIndexTemplateExistsRequest),
				typeof(SearchTemplateRequest),
				typeof(SearchTemplateRequest<>),
				typeof(ScrollRequest),
				typeof(SourceRequest),
				typeof(SourceRequest<>),
				typeof(ValidateQueryRequest<>),
				typeof(GetAliasRequest),
				typeof(IndicesShardStoresRequest),
				typeof(RenderSearchTemplateRequest),
				typeof(MultiSearchTemplateRequest),
				typeof(CreateRequest<>),
				typeof(DeleteByQueryRethrottleRequest), // uses ListTasksResponse
				typeof(UpdateByQueryRethrottleRequest) // uses ListTasksResponse
			};

			var types = typeof(IRequest).Assembly.GetTypes();

			var requests = new HashSet<string>(types
				.Where(t =>
					t.IsClass &&
					!t.IsAbstract &&
					typeof(IRequest).IsAssignableFrom(t) &&
					!typeof(IDescriptor).IsAssignableFrom(t)
					&& !t.Name.StartsWith("Cat")
					&& !exceptions.Contains(t))
				.Select(t => t.Name.Split('`')[0].Replace("Request", ""))
			);

			var responses = types
				.Where(t => t.IsClass && !t.IsAbstract && typeof(IResponse).IsAssignableFrom(t))
				.Select(t => t.Name.Split('`')[0].Replace("Response", ""));

			requests.Except(responses).Should().BeEmpty();
		}

		[U]
		public void AllOscTypesAreInOscNamespace()
		{
			var oscAssembly = typeof(IOpenSearchClient).Assembly;

			var exceptions = new List<Type>
			{
				oscAssembly.GetType("System.AssemblyVersionInformation", throwOnError: false),
				oscAssembly.GetType("System.Runtime.Serialization.Formatters.FormatterAssemblyStyle", throwOnError: false),
				oscAssembly.GetType("System.ComponentModel.Browsable", throwOnError: false),
				oscAssembly.GetType("Microsoft.CodeAnalysis.EmbeddedAttribute", throwOnError: false),
				oscAssembly.GetType("System.Runtime.CompilerServices.IsReadOnlyAttribute", throwOnError: false),
			};

			var types = oscAssembly.GetTypes();
			var typesNotInOscNamespace = types
				.Where(t => t != null)
				.Where(t => !exceptions.Contains(t))
				.Where(t => t.Namespace != "OpenSearch.Client")
				//when testing nuget package against merged internalize json.net skip its types.
				.Where(t => !string.IsNullOrWhiteSpace(t.Namespace) && !t.Namespace.StartsWith("OpenSearch.Client.Json"))
				.Where(t => !string.IsNullOrWhiteSpace(t.Namespace) && !t.Namespace.StartsWith("OpenSearch.Internal"))
				.Where(t => !string.IsNullOrWhiteSpace(t.Namespace) && !t.Namespace.StartsWith("OpenSearch.Client.Specification"))
				.Where(t => !t.Name.StartsWith("<"))
				.Where(t => IsValidTypeNameOrIdentifier(t.Name, true))
				.ToList();

			typesNotInOscNamespace.Should().BeEmpty();
		}

		[U]
		public void AllOpenSearchNetTypesAreInOpenSearchNetNamespace()
		{
			var opensearchNetAssembly = typeof(IOpenSearchLowLevelClient).Assembly;

			var exceptions = new List<Type>
			{
				opensearchNetAssembly.GetType("Microsoft.CodeAnalysis.EmbeddedAttribute"),
				opensearchNetAssembly.GetType("System.Runtime.CompilerServices.IsReadOnlyAttribute"),
				opensearchNetAssembly.GetType("System.AssemblyVersionInformation"),
				opensearchNetAssembly.GetType("System.FormattableString"),
				opensearchNetAssembly.GetType("System.Runtime.CompilerServices.FormattableStringFactory"),
				opensearchNetAssembly.GetType("System.Runtime.CompilerServices.FormattableStringFactory"),
				opensearchNetAssembly.GetType("Purify.Purifier"),
				opensearchNetAssembly.GetType("Purify.Purifier+IPurifier"),
				opensearchNetAssembly.GetType("Purify.Purifier+PurifierDotNet"),
				opensearchNetAssembly.GetType("Purify.Purifier+PurifierMono"),
				opensearchNetAssembly.GetType("Purify.Purifier+UriInfo"),
				opensearchNetAssembly.GetType("System.ComponentModel.Browsable")
			};

			var types = opensearchNetAssembly.GetTypes();
			var typesNotIOpenSearchNetNamespace = types
				.Where(t => !exceptions.Contains(t))
				.Where(t => t.Namespace != null)
				.Where(t => t.Namespace != "OpenSearch.Net" && !t.Namespace.StartsWith("OpenSearch.Net.Specification"))
				.Where(t => !t.Namespace.StartsWith("OpenSearch.Net.Utf8Json"))
				.Where(t => !t.Namespace.StartsWith("OpenSearch.Net.Extensions"))
				.Where(t => !t.Namespace.StartsWith("OpenSearch.Net.Diagnostics"))
				.Where(t => !t.Name.StartsWith("<"))
				.Where(t => IsValidTypeNameOrIdentifier(t.Name, true))
				.ToList();

			typesNotIOpenSearchNetNamespace.Should().BeEmpty();
		}

		/// implementation from System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(string value)
		private static bool IsValidTypeNameOrIdentifier(string value, bool isTypeName)
		{
			var nextMustBeStartChar = true;
			if (value.Length == 0)
				return false;
			for (var index = 0; index < value.Length; ++index)
			{
				var character = value[index];
				var unicodeCategory = char.GetUnicodeCategory(character);

				switch (unicodeCategory)
				{
					case UnicodeCategory.UppercaseLetter:
					case UnicodeCategory.LowercaseLetter:
					case UnicodeCategory.TitlecaseLetter:
					case UnicodeCategory.ModifierLetter:
					case UnicodeCategory.OtherLetter:
					case UnicodeCategory.LetterNumber:
						nextMustBeStartChar = false;
						break;
					case UnicodeCategory.NonSpacingMark:
					case UnicodeCategory.SpacingCombiningMark:
					case UnicodeCategory.DecimalDigitNumber:
					case UnicodeCategory.ConnectorPunctuation:
						if (nextMustBeStartChar && (int)character != 95)
							return false;
						nextMustBeStartChar = false;
						break;
					default:
						if (!isTypeName || !IsSpecialTypeChar(character, ref nextMustBeStartChar))
							return false;
						break;
				}
			}
			return true;
		}

		private static bool IsSpecialTypeChar(char ch, ref bool nextMustBeStartChar)
		{
			if ((uint)ch <= 62U)
			{
				switch (ch)
				{
					case '$':
					case '&':
					case '*':
					case '+':
					case ',':
					case '-':
					case '.':
					case ':':
					case '<':
					case '>':
						break;
					default:
						goto label_6;
				}
			}
			else if ((int)ch != 91 && (int)ch != 93)
			{
				if ((int)ch == 96)
					return true;
				goto label_6;
			}
			nextMustBeStartChar = true;
			return true;
			label_6:
			return false;
		}
	}
}


