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

#region Utf8Json License https://github.com/neuecc/Utf8Json/blob/master/LICENSE
// MIT License
//
// Copyright (c) 2017 Yoshifumi Kawai
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System;
using System.Reflection;
using OpenSearch.Net.Utf8Json.Formatters;
using OpenSearch.Net.Utf8Json.Internal;

namespace OpenSearch.Net.Utf8Json.Resolvers;

internal static class EnumResolver
{
    /// <summary>Serialize as Name.</summary>
    public static readonly IJsonFormatterResolver Default = EnumDefaultResolver.Instance;
    /// <summary>Serialize as Value.</summary>
    public static readonly IJsonFormatterResolver UnderlyingValue = EnumUnderlyingValueResolver.Instance;
}

internal sealed class EnumDefaultResolver : IJsonFormatterResolver
{
    public static readonly IJsonFormatterResolver Instance = new EnumDefaultResolver();

    private EnumDefaultResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>() => FormatterCache<T>.formatter;

    private static class FormatterCache<T>
    {
        public static readonly IJsonFormatter<T> formatter;

        static FormatterCache()
        {
            var type = typeof(T);

            if (type.IsNullable())
            {
                // build underlying type and use wrapped formatter.
                type = type.GenericTypeArguments[0];
                if (!type.IsEnum)
                    return;

                var innerFormatter = Instance.GetFormatterDynamic(type);
                if (innerFormatter == null)
                    return;

                formatter = (IJsonFormatter<T>)Activator.CreateInstance(typeof(StaticNullableFormatter<>).MakeGenericType(type), innerFormatter);
                return;
            }

            if (typeof(T).IsEnum)
                formatter = new EnumFormatter<T>(true);
        }
    }
}

internal sealed class EnumUnderlyingValueResolver : IJsonFormatterResolver
{
    public static readonly IJsonFormatterResolver Instance = new EnumUnderlyingValueResolver();

    private EnumUnderlyingValueResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>() => FormatterCache<T>.formatter;

    private static class FormatterCache<T>
    {
        public static readonly IJsonFormatter<T> formatter;

        static FormatterCache()
        {
            var type = typeof(T);

            if (type.IsNullable())
            {
                // build underlying type and use wrapped formatter.
                type = type.GenericTypeArguments[0];
                if (!type.IsEnum) return;

                var innerFormatter = Instance.GetFormatterDynamic(type);
                if (innerFormatter == null)
                    return;

                formatter = (IJsonFormatter<T>)Activator.CreateInstance(typeof(StaticNullableFormatter<>).MakeGenericType(type), innerFormatter);
                return;
            }

            if (typeof(T).IsEnum)
                formatter = new EnumFormatter<T>(false);
        }
    }
}
