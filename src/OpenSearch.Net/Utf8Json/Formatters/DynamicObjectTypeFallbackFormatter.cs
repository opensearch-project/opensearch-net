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

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using OpenSearch.Net.Utf8Json.Internal;
using OpenSearch.Net.Utf8Json.Internal.Emit;
using OpenSearch.Net.Utf8Json.Resolvers;

namespace OpenSearch.Net.Utf8Json.Formatters;

internal sealed class DynamicObjectTypeFallbackFormatter : IJsonFormatter<object>
{
    private delegate void SerializeMethod(object dynamicFormatter, ref JsonWriter writer, object value, IJsonFormatterResolver formatterResolver);

    private readonly ThreadsafeTypeKeyHashTable<KeyValuePair<object, SerializeMethod>> _serializers =
        new ThreadsafeTypeKeyHashTable<KeyValuePair<object, SerializeMethod>>();

    private readonly IJsonFormatterResolver[] _innerResolvers;

    public DynamicObjectTypeFallbackFormatter(params IJsonFormatterResolver[] innerResolvers) => _innerResolvers = innerResolvers;

    public void Serialize(ref JsonWriter writer, object value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null) { writer.WriteNull(); return; }

        var type = value.GetType();

        if (type == typeof(object))
        {
            // serialize to empty object
            writer.WriteBeginObject();
            writer.WriteEndObject();
            return;
        }

        if (!_serializers.TryGetValue(type, out var formatterAndDelegate))
        {
            lock (_serializers)
            {
                if (!_serializers.TryGetValue(type, out formatterAndDelegate))
                {
                    object formatter = null;
                    foreach (var innerResolver in _innerResolvers)
                    {
                        formatter = innerResolver.GetFormatterDynamic(type);
                        if (formatter != null) break;
                    }
                    if (formatter == null)
                        throw new FormatterNotRegisteredException(type.FullName + " is not registered in this resolver. resolvers:" + string.Join(", ", _innerResolvers.Select(x => x.GetType().Name).ToArray()));

                    var t = type;
                    {
                        var dm = new DynamicMethod("Serialize", null, new[] { typeof(object), typeof(JsonWriter).MakeByRefType(), typeof(object), typeof(IJsonFormatterResolver) }, type.Module, true);
                        var il = dm.GetILGenerator();

                        // delegate void SerializeMethod(object dynamicFormatter, ref JsonWriter writer, object value, IJsonFormatterResolver formatterResolver);

                        il.EmitLdarg(0);
                        il.Emit(OpCodes.Castclass, typeof(IJsonFormatter<>).MakeGenericType(t));
                        il.EmitLdarg(1);
                        il.EmitLdarg(2);
                        il.EmitUnboxOrCast(t);
                        il.EmitLdarg(3);

                        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.Serialize(t));

                        il.Emit(OpCodes.Ret);

                        formatterAndDelegate = new KeyValuePair<object, SerializeMethod>(formatter, (SerializeMethod)dm.CreateDelegate(typeof(SerializeMethod)));
                    }

                    _serializers.TryAdd(t, formatterAndDelegate);
                }
            }
        }

        formatterAndDelegate.Value(formatterAndDelegate.Key, ref writer, value, formatterResolver);
    }

    public object Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver) =>
        PrimitiveObjectFormatter.Default.Deserialize(ref reader, formatterResolver);
}
