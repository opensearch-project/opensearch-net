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

namespace ApiGenerator.Domain.Specification
{
    public class UrlPart
    {
        private string _description;

        public string Argument => $"{LowLevelTypeName} {NameAsArgument}";

        public string LowLevelTypeName =>
            //TODO treat list with fixed options as Flags Enum
            Type switch
            {
                "string" => Type,
                "int" => "int?",
                "long" => "long?",
                "float" => "float?",
                "double" => "double?",
                "list" => "string",
                "enum" => Name.ToPascalCase(),
                _ => Type
            };

        public string HighLevelTypeName
        {
            get
            {
                if (ClrTypeNameOverride != null) return ClrTypeNameOverride;

                switch (Name)
                {
                    case "category_id": return "LongId";
                    case "timestamp": return "Timestamp";
                    case "index_metric": return "IndexMetrics";
                    case "metric": return "Metrics";

                    case "node_id" when Type == "list":
                        return "NodeIds";

                    case "fields" when Type == "list":
                        return "Fields";

                    case "parent_task_id":
                    case "task_id":
                        return "TaskId";

                    case "forecast_id":
                    case "action_id":
                    case "ids" when Type == "list":
                        return "Ids";

                    case "index":
                    case "new_index":
                    case "target":
                        return Type == "string" ? "IndexName" : "Indices";

                    case "job_id":
                    case "calendar_id":
                    case "event_id":
                    case "snapshot_id":
                    case "filter_id":
                    case "model_id":
                    case "id":
                        return "Id";

                    case "policy_id":
                        return Type == "string" ? "Id" : "Ids";

                    case "application":
                    case "repository":
                    case "snapshot":
                    case "target_snapshot":
                    case "user":
                    case "username":
                    case "realms":
                    case "alias":
                    case "context":
                    case "name":
                    case "thread_pool_patterns":
                    case "type":
                        return Type == "string" ? "Name" : "Names";

                    case "block":
                        return "IndexBlock";

                    case "index_uuid":
                        return "IndexUuid";

                    // ML-specific opaque identifiers
                    case "agent_id":
                    case "connector_id":
                    case "memory_container_id":
                    case "memory_id":
                    case "message_id":
                    case "model_group_id":
                        return "Id";

                    // ML-specific name parameters (not identifiers)
                    case "algorithm_name":
                    case "stat":
                    case "tool_name":
                    case "node_id":
                        return "Name";

                    // Integer URL segment used by ml.upload_chunk.
                    case "chunk_number":
                        return "long?";

                    // For URL parts whose type was already resolved to a C# nullable enum type
                    // by GetOpenSearchType (e.g. "MlFunctionName?", "MlToolName?"), the Type
                    // field already holds the correct CLR type.  Return it directly so the
                    // generated code compiles without a manual case for every future enum.
                    default:
                        return IsResolvedNullableType(Type) ? Type : Type + "_";
                }
            }
        }

        private static readonly HashSet<string> PrimitiveNullables =
            new HashSet<string> { "int?", "long?", "float?", "double?", "bool?" };

        /// <summary>
        /// Returns true when <paramref name="type"/> looks like a resolved C# nullable type
        /// (ends with "?") but is NOT one of the well-known primitives.  These come from
        /// <c>GetOpenSearchType</c> for enum-typed URL path parameters and are safe to use
        /// directly as the high-level CLR type.
        /// </summary>
        private static bool IsResolvedNullableType(string type) =>
            type.EndsWith("?") && !PrimitiveNullables.Contains(type);

        public string ClrTypeNameOverride { get; set; }

        public string Description
        {
            get => _description;
            set => _description = CleanUpDescription(value);
        }

        public string InterfaceName =>
			Name switch
			{
				"repository" => "RepositoryName",
				_ => Name.ToPascalCase()
			};

		public string Name { get; set; }
        public string NameAsArgument => Name.ToCamelCase();
        public bool Required { get; set; }
        public bool Deprecated { get; set; }
        public string Type { get; set; }

        private static string CleanUpDescription(string value) =>
			string.IsNullOrWhiteSpace(value)
				? value
				: value.Replace("use `_all` or empty string", "use the special string `_all` or Indices.All");
	}
}
