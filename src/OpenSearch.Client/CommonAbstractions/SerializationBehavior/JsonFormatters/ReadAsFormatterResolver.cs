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
using System.Reflection;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Resolvers;


namespace OpenSearch.Client;

internal sealed class ReadAsFormatterResolver : IJsonFormatterResolver
{
    public static readonly IJsonFormatterResolver Instance = new ReadAsFormatterResolver();

    private ReadAsFormatterResolver() { }

    public IJsonFormatter<T> GetFormatter<T>() => FormatterCache<T>.Formatter;

    private static class FormatterCache<T>
    {
        public static readonly IJsonFormatter<T> Formatter;

        static FormatterCache()
        {
            var readAsAttribute = typeof(T).GetCustomAttribute<ReadAsAttribute>();
            if (readAsAttribute == null)
                return;

            try
            {
                Type formatterType;
                if (readAsAttribute.Type.IsGenericType && !readAsAttribute.Type.IsConstructedGenericType)
                {
                    var genericType = readAsAttribute.Type.MakeGenericType(typeof(T).GenericTypeArguments);
                    formatterType = typeof(ReadAsFormatter<,>).MakeGenericType(genericType, typeof(T));
                }
                else
                    formatterType = typeof(ReadAsFormatter<,>).MakeGenericType(readAsAttribute.Type, typeof(T));

                Formatter = (IJsonFormatter<T>)Activator.CreateInstance(formatterType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Can not create formatter from {nameof(ReadAsAttribute)} for {readAsAttribute.Type.Name}", ex);
            }
        }
    }
}

internal class ReadAsFormatter<TRead, T> : IJsonFormatter<T>
    where TRead : T
{
    public virtual T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var formatter = formatterResolver.GetFormatter<TRead>();
        return formatter.Deserialize(ref reader, formatterResolver);
    }

    public virtual void Serialize(ref JsonWriter writer, T value, IJsonFormatterResolver formatterResolver) =>
        SerializeInternal(ref writer, value, formatterResolver);

    public virtual void SerializeInternal(ref JsonWriter writer, T value, IJsonFormatterResolver formatterResolver)
    {
        var formatter = DynamicObjectResolver.ExcludeNullCamelCase.GetFormatter<T>();
        formatter.Serialize(ref writer, value, formatterResolver);
    }
}
