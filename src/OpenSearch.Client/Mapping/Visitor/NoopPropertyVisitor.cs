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

using System.Reflection;

namespace OpenSearch.Client;

// TODO: Make all methods virtual
public class NoopPropertyVisitor : IPropertyVisitor
{
    public virtual bool SkipProperty(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) => false;

    public virtual void Visit(IBooleanProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IBinaryProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IObjectProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IGeoShapeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(ICompletionProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IMurmur3HashProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(ITokenCountProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public void Visit(IPercolatorProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public void Visit(IIntegerRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public void Visit(IFloatRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public void Visit(ILongRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public void Visit(IDoubleRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public void Visit(IDateRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public void Visit(IIpRangeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public void Visit(IJoinProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IRankFeatureProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IRankFeaturesProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IIpProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IGeoPointProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(INestedProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IDateProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IDateNanosProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(INumberProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(ITextProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IKeywordProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(ISearchAsYouTypeProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IFieldAliasProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual void Visit(IKnnVectorProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) { }

    public virtual IProperty Visit(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) => null;

    public void Visit(IProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute)
    {
        switch (type)
        {
            case INestedProperty nestedType:
                Visit(nestedType, propertyInfo, attribute);
                break;
            case IObjectProperty objectType:
                Visit(objectType, propertyInfo, attribute);
                break;
            case IBinaryProperty binaryType:
                Visit(binaryType, propertyInfo, attribute);
                break;
            case IBooleanProperty booleanType:
                Visit(booleanType, propertyInfo, attribute);
                break;
            case IDateProperty dateType:
                Visit(dateType, propertyInfo, attribute);
                break;
            case IDateNanosProperty dateNanosType:
                Visit(dateNanosType, propertyInfo, attribute);
                break;
            case INumberProperty numberType:
                Visit(numberType, propertyInfo, attribute);
                break;
            case ITextProperty textType:
                Visit(textType, propertyInfo, attribute);
                break;
            case IKeywordProperty keywordType:
                Visit(keywordType, propertyInfo, attribute);
                break;
            case IGeoShapeProperty geoShapeType:
                Visit(geoShapeType, propertyInfo, attribute);
                break;
            case IGeoPointProperty geoPointType:
                Visit(geoPointType, propertyInfo, attribute);
                break;
            case ICompletionProperty completionType:
                Visit(completionType, propertyInfo, attribute);
                break;
            case IIpProperty ipType:
                Visit(ipType, propertyInfo, attribute);
                break;
            case IMurmur3HashProperty murmurType:
                Visit(murmurType, propertyInfo, attribute);
                break;
            case ITokenCountProperty tokenCountType:
                Visit(tokenCountType, propertyInfo, attribute);
                break;
            case IPercolatorProperty percolatorType:
                Visit(percolatorType, propertyInfo, attribute);
                break;
            case IJoinProperty joinType:
                Visit(joinType, propertyInfo, attribute);
                break;
            case IIntegerRangeProperty integerRangeType:
                Visit(integerRangeType, propertyInfo, attribute);
                break;
            case ILongRangeProperty longRangeType:
                Visit(longRangeType, propertyInfo, attribute);
                break;
            case IDoubleRangeProperty doubleRangeType:
                Visit(doubleRangeType, propertyInfo, attribute);
                break;
            case IFloatRangeProperty floatRangeType:
                Visit(floatRangeType, propertyInfo, attribute);
                break;
            case IDateRangeProperty dateRangeType:
                Visit(dateRangeType, propertyInfo, attribute);
                break;
            case IIpRangeProperty ipRangeType:
                Visit(ipRangeType, propertyInfo, attribute);
                break;
            case IRankFeatureProperty rankFeature:
                Visit(rankFeature, propertyInfo, attribute);
                break;
            case IRankFeaturesProperty rankFeatures:
                Visit(rankFeatures, propertyInfo, attribute);
                break;
            case ISearchAsYouTypeProperty searchAsYouType:
                Visit(searchAsYouType, propertyInfo, attribute);
                break;
            case IFieldAliasProperty fieldAlias:
                Visit(fieldAlias, propertyInfo, attribute);
                break;
            case IKnnVectorProperty knnVector:
                Visit(knnVector, propertyInfo, attribute);
                break;
        }
    }
}
