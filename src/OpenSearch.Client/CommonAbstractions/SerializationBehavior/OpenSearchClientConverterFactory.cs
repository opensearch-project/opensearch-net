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

#nullable enable

using System;
#nullable enable

using System.Reflection;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="JsonConverterFactory"/> that handles OpenSearch.Client domain types requiring
	/// custom serialization, including types with <see cref="ReadAsAttribute"/> for polymorphic
	/// deserialization and other connection-settings-aware converters.
	/// <para>
	/// This is the primary extension point for registering domain-specific converters.
	/// As converters are implemented, they are registered here.
	/// </para>
	/// </summary>
	internal class OpenSearchClientConverterFactory : JsonConverterFactory
	{
		/// <summary>
		/// The connection settings used by converters that need access to property mappings,
		/// field name inferrer, and other client configuration.
		/// </summary>
		public IConnectionSettingsValues Settings { get; }

		public OpenSearchClientConverterFactory(IConnectionSettingsValues settings)
		{
			Settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		/// <inheritdoc />
		/// <remarks>
		/// Returns <c>true</c> for types that require custom serialization within OpenSearch.Client.
		/// Currently a placeholder that will be populated as domain-specific converters are added.
		/// Candidates include:
		/// <list type="bullet">
		///   <item>Types decorated with <see cref="ReadAsAttribute"/> (polymorphic deserialization)</item>
		///   <item>Union types and other OpenSearch.Client abstractions</item>
		///   <item>Request/response types requiring special property handling</item>
		/// </list>
		/// </remarks>
		public override bool CanConvert(Type typeToConvert)
		{
			// TODO: Populate as converters are implemented. Planned checks:
			// 1. typeToConvert.GetCustomAttribute<ReadAsAttribute>() != null
			// 2. Known domain type registrations
			return false;
		}

		/// <inheritdoc />
		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			// Delegate to specific converters based on type.
			// This will be populated as individual converters are implemented.

			// Example future implementation:
			// var readAs = typeToConvert.GetCustomAttribute<ReadAsAttribute>();
			// if (readAs != null)
			//     return CreateReadAsConverter(typeToConvert, readAs.Type, options);

			throw new NotSupportedException(
				$"No converter registered for type '{typeToConvert.FullName}'. " +
				$"This indicates CanConvert returned true but CreateConverter has no handler for the type.");
		}
	}
}
