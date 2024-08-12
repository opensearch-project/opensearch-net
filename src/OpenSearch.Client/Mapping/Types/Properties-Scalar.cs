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
using System.Linq.Expressions;

namespace OpenSearch.Client;

public partial interface IPropertiesDescriptor<T, out TReturnType>
    where T : class
    where TReturnType : class
{
#pragma warning disable CS3001 // Argument type is not CLS-compliant
    TReturnType Scalar(Expression<Func<T, int>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, int?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<int>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<int?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, float>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, float?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<float>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<float?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, sbyte>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, sbyte?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<sbyte>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<sbyte?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, short>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, short?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<short>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<short?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, byte>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, byte?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<byte>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<byte?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, long>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, long?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<long>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<long?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, uint>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, uint?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<uint>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<uint?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, TimeSpan>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, TimeSpan?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<TimeSpan>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<TimeSpan?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, decimal>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, decimal?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<decimal>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<decimal?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, ulong>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, ulong?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<ulong>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<ulong?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, double>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, double?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<double>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<double?>>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, Enum>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, DateTime>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, DateTime?>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<DateTime>>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<DateTime?>>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, DateTimeOffset>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, DateTimeOffset?>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<DateTimeOffset>>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<DateTimeOffset?>>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, bool>> field, Func<BooleanPropertyDescriptor<T>, IBooleanProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, bool?>> field, Func<BooleanPropertyDescriptor<T>, IBooleanProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<bool>>> field, Func<BooleanPropertyDescriptor<T>, IBooleanProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<bool?>>> field, Func<BooleanPropertyDescriptor<T>, IBooleanProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, char>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, char?>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<char>>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<char?>>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, Guid>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, Guid?>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<Guid>>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<Guid?>>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, string>> field, Func<TextPropertyDescriptor<T>, ITextProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IEnumerable<string>>> field, Func<TextPropertyDescriptor<T>, ITextProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, DateRange>> field, Func<DateRangePropertyDescriptor<T>, IDateRangeProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, DoubleRange>> field, Func<DoubleRangePropertyDescriptor<T>, IDoubleRangeProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, LongRange>> field, Func<LongRangePropertyDescriptor<T>, ILongRangeProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IntegerRange>> field, Func<IntegerRangePropertyDescriptor<T>, IIntegerRangeProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, FloatRange>> field, Func<FloatRangePropertyDescriptor<T>, IFloatRangeProperty> selector = null);

    TReturnType Scalar(Expression<Func<T, IpAddressRange>> field, Func<IpRangePropertyDescriptor<T>, IIpRangeProperty> selector = null);
#pragma warning restore CS3001 // Argument type is not CLS-compliant
}

public partial class PropertiesDescriptor<T>
    : IsADictionaryDescriptorBase<PropertiesDescriptor<T>, IProperties, PropertyName, IProperty>,
        IPropertiesDescriptor<T, PropertiesDescriptor<T>>
    where T : class
{
#pragma warning disable CS3001 // Argument type is not CLS-compliant
    public PropertiesDescriptor<T> Scalar(Expression<Func<T, int>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Integer)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<int>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Integer)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, int?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Integer)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<int?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Integer)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, float>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Float)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<float>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Float)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, float?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Float)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<float?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Float)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, sbyte>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Byte)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, sbyte?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Byte)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<sbyte>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Byte)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<sbyte?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Byte)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, short>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Short)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, short?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Short)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<short>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Short)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<short?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Short)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, byte>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Short)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, byte?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Short)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<byte>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Short)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<byte?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Short)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, long>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, long?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<long>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<long?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, uint>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, uint?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<uint>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<uint?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, TimeSpan>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, TimeSpan?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<TimeSpan>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<TimeSpan?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Long)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, decimal>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, decimal?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<decimal>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<decimal?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, ulong>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, ulong?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<ulong>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<ulong?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, double>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, double?>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<double>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<double?>>> field,
        Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Double)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, Enum>> field, Func<NumberPropertyDescriptor<T>, INumberProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new NumberPropertyDescriptor<T>().Name(field).Type(NumberType.Integer)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, DateTime>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new DatePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, DateTime?>> field, Func<DatePropertyDescriptor<T>, IDateProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new DatePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<DateTime>>> field,
        Func<DatePropertyDescriptor<T>, IDateProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new DatePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<DateTime?>>> field,
        Func<DatePropertyDescriptor<T>, IDateProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new DatePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, DateTimeOffset>> field,
        Func<DatePropertyDescriptor<T>, IDateProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new DatePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, DateTimeOffset?>> field,
        Func<DatePropertyDescriptor<T>, IDateProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new DatePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<DateTimeOffset>>> field,
        Func<DatePropertyDescriptor<T>, IDateProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new DatePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<DateTimeOffset?>>> field,
        Func<DatePropertyDescriptor<T>, IDateProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new DatePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, bool>> field, Func<BooleanPropertyDescriptor<T>, IBooleanProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new BooleanPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, bool?>> field, Func<BooleanPropertyDescriptor<T>, IBooleanProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new BooleanPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<bool>>> field,
        Func<BooleanPropertyDescriptor<T>, IBooleanProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new BooleanPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<bool?>>> field,
        Func<BooleanPropertyDescriptor<T>, IBooleanProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new BooleanPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, char>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new KeywordPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, char?>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new KeywordPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<char>>> field,
        Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new KeywordPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<char?>>> field,
        Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new KeywordPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, Guid>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new KeywordPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, Guid?>> field, Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new KeywordPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<Guid>>> field,
        Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new KeywordPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<Guid?>>> field,
        Func<KeywordPropertyDescriptor<T>, IKeywordProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new KeywordPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, string>> field, Func<TextPropertyDescriptor<T>, ITextProperty> selector = null) =>
        SetProperty(selector.InvokeOrDefault(new TextPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IEnumerable<string>>> field,
        Func<TextPropertyDescriptor<T>, ITextProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new TextPropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, DateRange>> field,
        Func<DateRangePropertyDescriptor<T>, IDateRangeProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new DateRangePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, DoubleRange>> field,
        Func<DoubleRangePropertyDescriptor<T>, IDoubleRangeProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new DoubleRangePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, LongRange>> field,
        Func<LongRangePropertyDescriptor<T>, ILongRangeProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new LongRangePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IntegerRange>> field,
        Func<IntegerRangePropertyDescriptor<T>, IIntegerRangeProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new IntegerRangePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, FloatRange>> field,
        Func<FloatRangePropertyDescriptor<T>, IFloatRangeProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new FloatRangePropertyDescriptor<T>().Name(field)));

    public PropertiesDescriptor<T> Scalar(Expression<Func<T, IpAddressRange>> field,
        Func<IpRangePropertyDescriptor<T>, IIpRangeProperty> selector = null
    ) =>
        SetProperty(selector.InvokeOrDefault(new IpRangePropertyDescriptor<T>().Name(field)));

#pragma warning restore CS3001 // Argument type is not CLS-compliant
}
