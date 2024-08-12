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
using OpenSearch.Client;

namespace OpenSearch.Client;

public class MappingWalker
{
    private readonly IMappingVisitor _visitor;

    public MappingWalker(IMappingVisitor visitor)
    {
        visitor.ThrowIfNull(nameof(visitor));
        _visitor = visitor;
    }

    public void Accept(GetMappingResponse response)
    {
        if (response?.Indices == null) return;

        foreach (var indexMapping in response.Indices)
        {
            if (indexMapping.Value?.Mappings == null) continue;

            Accept(indexMapping.Value.Mappings);
        }
    }

    public void Accept(ITypeMapping mapping)
    {
        if (mapping == null) return;

        _visitor.Visit(mapping);
        Accept(mapping.Properties);
    }


    private static void Visit<TProperty>(IProperty prop, Action<TProperty> act)
        where TProperty : class, IProperty
    {
        if (!(prop is TProperty t)) return;

        act(t);
    }

    public void Accept(IProperties properties)
    {
        if (properties == null) return;

        foreach (var kv in properties)
        {
            var field = kv.Value;
            var type = field.Type;
            var ft = type.ToEnum<FieldType>();
            switch (ft)
            {
                case FieldType.Text:
                    Visit<ITextProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.Keyword:
                    Visit<IKeywordProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.SearchAsYouType:
                    Visit<ISearchAsYouTypeProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.HalfFloat:
                case FieldType.ScaledFloat:
                case FieldType.Float:
                case FieldType.Double:
                case FieldType.Byte:
                case FieldType.Short:
                case FieldType.Integer:
                case FieldType.Long:
                    Visit<INumberProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.Date:
                    Visit<IDateProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.DateNanos:
                    Visit<IDateNanosProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.Boolean:
                    Visit<IBooleanProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.Binary:
                    Visit<IBinaryProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.Object:
                    Visit<IObjectProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        _visitor.Depth += 1;
                        Accept(t.Properties);
                        _visitor.Depth -= 1;
                    });
                    break;
                case FieldType.Nested:
                    Visit<INestedProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        _visitor.Depth += 1;
                        Accept(t.Properties);
                        _visitor.Depth -= 1;
                    });
                    break;
                case FieldType.Ip:
                    Visit<IIpProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.GeoPoint:
                    Visit<IGeoPointProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.GeoShape:
                    Visit<IGeoShapeProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.Completion:
                    Visit<ICompletionProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.Murmur3Hash:
                    Visit<IMurmur3HashProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.TokenCount:
                    Visit<ITokenCountProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.Percolator:
                    Visit<IPercolatorProperty>(field, t => { _visitor.Visit(t); });
                    break;
                case FieldType.IntegerRange:
                    Visit<IIntegerRangeProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.FloatRange:
                    Visit<IFloatRangeProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.LongRange:
                    Visit<ILongRangeProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.DoubleRange:
                    Visit<IDoubleRangeProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.DateRange:
                    Visit<IDateRangeProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.IpRange:
                    Visit<IIpRangeProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                        Accept(t.Fields);
                    });
                    break;
                case FieldType.Join:
                    Visit<IJoinProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                    });
                    break;
                case FieldType.RankFeature:
                    Visit<IRankFeatureProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                    });
                    break;
                case FieldType.RankFeatures:
                    Visit<IRankFeaturesProperty>(field, t =>
                    {
                        _visitor.Visit(t);
                    });
                    break;
                case FieldType.None:
                    continue;
            }
        }
    }
}
