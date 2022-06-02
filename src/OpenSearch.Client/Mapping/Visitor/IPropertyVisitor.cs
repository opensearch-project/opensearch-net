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

using System.Reflection;

namespace OpenSearch.Client
{
	public interface IPropertyVisitor
	{
		void Visit(ITextProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IKeywordProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(INumberProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IBooleanProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IDateProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IDateNanosProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IBinaryProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(INestedProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IObjectProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IGeoPointProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IGeoShapeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(ICompletionProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IIpProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IMurmur3HashProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(ITokenCountProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IPercolatorProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IIntegerRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IFloatRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(ILongRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IDoubleRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IDateRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IIpRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IJoinProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IRankFeatureProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IRankFeaturesProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(ISearchAsYouTypeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		void Visit(IFieldAliasProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		IProperty Visit(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);

		bool SkipProperty(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute);
	}
}
