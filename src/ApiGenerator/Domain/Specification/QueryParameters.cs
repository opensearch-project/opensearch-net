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
using ApiGenerator.Generator;
using Version = SemanticVersioning.Version;

namespace ApiGenerator.Domain.Specification
{
    public class QueryParameters
    {
        private static readonly string[] FieldsParams = { "fields", "_source_includes", "_source_excludes", };

        public bool Skip { get; set; }

        public string ClsArgumentName => ClsName.ToCamelCase();

        public string ClsName { get; set; }

        public string Description { get; set; }

		public Version VersionAdded { get; set; }

        public IEnumerable<string> DescriptionHighLevel
        {
            get
            {
                switch (QueryStringKey)
                {
                    case "routing":
                        yield return "A document is routed to a particular shard in an index using the following formula";
                        yield return "<para> shard_num = hash(_routing) % num_primary_shards</para>";
                        yield return "<para>OpenSearch will use the document id if not provided. </para>";
                        yield return "<para>For requests that are constructed from/for a document OpenSearch.Client will automatically infer the routing key";
                        yield return
                            "if that document has a <see cref=\"OpenSearch.Client.JoinField\" /> or a routing mapping on for its type exists on <see cref=\"OpenSearch.Client.ConnectionSettings\" /></para> ";

                        yield break;
                    case "_source":
                        yield return "Whether the _source should be included in the response.";

                        yield break;
                    case "filter_path":
                        yield return Description;
                        yield return "<para>Use of response filtering can result in a response from OpenSearch ";
                        yield return "that cannot be correctly deserialized to the respective response type for the request. ";
                        yield return "In such situations, use the low level client to issue the request and handle response deserialization</para>";

                        yield break;
                    default:
                        yield return Description ?? "TODO";

                        yield break;
                }
            }
        }

        public bool IsArray => Type == "list" && TypeHighLevel.EndsWith("[]");

        public string DescriptorArgumentType => IsArray ? "params " + TypeHighLevel : TypeHighLevel;

        public string DescriptorEnumerableArgumentType =>
            IsArray
                ? $"IEnumerable<{TypeHighLevel.TrimEnd('[', ']')}>"
                : throw new InvalidOperationException("Only array arguments have IEnumerable overload");

        public Func<string, string, string, string, string> FluentGenerator { get; set; }
        public bool IsFieldParam => TypeHighLevel == "Field";

        public bool IsFieldsParam => TypeHighLevel == "Fields";

        public string Obsolete
        {
            get => !string.IsNullOrEmpty(_obsolete)
				? _obsolete
				: Deprecated?.ToString();
			set => _obsolete = value;
        }

        public Deprecation Deprecated { get; set; }

        public string QueryStringKey { get; set; }

        public bool RenderPartial { get; set; }
        public string SetterHighLevel => "value";

        public string SetterLowLevel => "value";

        private string _type;
        private string _obsolete;

        public string Type
        {
            // TODO support unions
            get => !_type.Contains("|")
                ? _type
                : _type.Split('|', StringSplitOptions.RemoveEmptyEntries).First().Trim();
            set => _type = value;
        }

        public string TypeHighLevel
        {
            get
            {
                if (QueryStringKey == "routing") return "Routing";

                var isFields = FieldsParams.Contains(QueryStringKey) || QueryStringKey.EndsWith("_fields");

                var csharpType = TypeLowLevel;
                switch (csharpType)
                {
                    case "TimeSpan": return "Time";
                }

                switch (Type)
                {
                    case "list" when isFields:
                    case "string" when isFields: return "Fields";
                    case "string" when QueryStringKey.Contains("field"): return "Field";
                    default:
                        return csharpType;
                }
            }
        }

        public string TypeLowLevel
        {
            get
            {
                switch (Type)
                {
                    case "boolean": return "bool?";
                    case "list": return "string[]";
                    case "int": return "int?";
                    case "date": return "DateTimeOffset?";
                    case "enum": return $"{ClsName}?";
                    case "number":
                        return new[] { "boost", "percen", "score" }.Any(s => QueryStringKey.ToLowerInvariant().Contains(s))
                            ? "double?"
                            : "long?";
                    case "duration":
                    case "time":
                        return "TimeSpan";
                    case "text":
                    case "":
                    case null:
                        return "string";
                    default:
                        return Type;
                }
            }
        }


        public string InitializerGenerator(string @namespace, string type, string name, string key, string setter, Version versionAdded, params string[] doc) =>
            CodeGenerator.Property(@namespace, type, name, key, setter, Obsolete, versionAdded, doc);
    }
}
