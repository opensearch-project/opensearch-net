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

using System.Collections.Generic;
using System.Threading.Tasks;
using RazorLight;

namespace ApiGenerator;

public abstract class CodeTemplatePage<TModel> : TemplatePage<TModel>
{
	protected new Task IncludeAsync(string key, object model = null)
		=> base.IncludeAsync(key.Replace('/', '.'), model);

	protected async Task IncludeLegacyGeneratorNotice() => await IncludeAsync("GeneratorNotice", true);

	protected async Task IncludeGeneratorNotice() => await IncludeAsync("GeneratorNotice", false);

	private static readonly HashSet<string> CsharpKeywords = new(System.StringComparer.Ordinal)
	{
		"abstract","as","base","bool","break","byte","case","catch","char","checked",
		"class","const","continue","decimal","default","delegate","do","double","else",
		"enum","event","explicit","extern","false","finally","fixed","float","for",
		"foreach","goto","if","implicit","in","int","interface","internal","is","lock",
		"long","namespace","new","null","object","operator","out","override","params",
		"private","protected","public","readonly","ref","return","sbyte","sealed","short",
		"sizeof","stackalloc","static","string","struct","switch","this","throw","true",
		"try","typeof","uint","ulong","unchecked","unsafe","ushort","using","virtual",
		"void","volatile","while"
	};

	/// <summary>Prefixes reserved C# keywords with @ so they are valid parameter names.</summary>
	protected static string SafeParam(string n) => CsharpKeywords.Contains(n) ? "@" + n : n;
}
