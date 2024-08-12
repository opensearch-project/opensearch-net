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
using OpenSearch.Net.Utf8Json.Internal;

namespace OpenSearch.Net.Utf8Json.Resolvers;

/// <summary>
/// Get formatter from [JsonFormatter] attribute.
/// </summary>
internal sealed class AttributeFormatterResolver : IJsonFormatterResolver
{
    public static readonly IJsonFormatterResolver Instance = new AttributeFormatterResolver();

    private AttributeFormatterResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>() => FormatterCache<T>.formatter;

    private static class FormatterCache<T>
    {
        public static readonly IJsonFormatter<T> formatter;

        static FormatterCache()
        {
            var attr = typeof(T).GetCustomAttribute<JsonFormatterAttribute>(true);
            if (attr == null)
                return;

            try
            {
                if (attr.FormatterType.IsGenericType && !attr.FormatterType.IsConstructedGenericType)
                {
                    // generic types need to be deconstructed
                    var types = typeof(T).IsGenericType
                        ? typeof(T).GenericTypeArguments
                        : new[] { typeof(T) };

                    var t = attr.FormatterType.MakeGenericType(types);
                    formatter = (IJsonFormatter<T>)Activator.CreateInstance(t, attr.Arguments);
                }
                else
                    formatter = (IJsonFormatter<T>)Activator.CreateInstance(attr.FormatterType, attr.Arguments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Can not create formatter from JsonFormatterAttribute, check the target formatter is public and has constructor with right argument. FormatterType:" + attr.FormatterType.Name, ex);
            }
        }
    }
}
