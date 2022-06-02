/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
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
using System.Reflection;
using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	[AttributeUsage(AttributeTargets.Property)]
	[DataContract]
	public abstract class OpenSearchPropertyAttributeBase : Attribute, IProperty, IPropertyMapping, IJsonProperty
	{
		protected OpenSearchPropertyAttributeBase(FieldType type) => Self.Type = type.GetStringValue();

		public bool? AllowPrivate { get; set; } = true;

		public bool Ignore { get; set; }

		public string Name { get; set; }

		public int Order { get; } = -2;

		IDictionary<string, object> IProperty.LocalMetadata { get; set; }

		IDictionary<string, string> IProperty.Meta { get; set; }

		PropertyName IProperty.Name { get; set; }
		private IProperty Self => this;
		string IProperty.Type { get; set; }

		public static OpenSearchPropertyAttributeBase From(MemberInfo memberInfo) =>
			memberInfo.GetCustomAttribute<OpenSearchPropertyAttributeBase>(true);
	}
}
