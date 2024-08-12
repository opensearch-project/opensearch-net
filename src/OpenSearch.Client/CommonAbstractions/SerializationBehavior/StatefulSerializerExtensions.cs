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
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

internal static class StatefulSerializerExtensions
{
    public static DefaultHighLevelSerializer CreateStateful<T>(this IOpenSearchSerializer serializer, IJsonFormatter<T> formatter)
    {
        if (!(serializer is IInternalSerializer s) || !s.TryGetJsonFormatter(out var currentFormatterResolver))
            throw new Exception($"Can not create a stateful serializer because {serializer.GetType()} does not yield a json formatter");

        var formatterResolver = new StatefulFormatterResolver<T>(formatter, currentFormatterResolver);
        return new DefaultHighLevelSerializer(formatterResolver);
    }

    private class StatefulFormatterResolver<TStateful> : IJsonFormatterResolver, IJsonFormatterResolverWithSettings
    {
        private readonly IJsonFormatter<TStateful> _jsonFormatter;
        private readonly IJsonFormatterResolver _formatterResolver;

        public StatefulFormatterResolver(IJsonFormatter<TStateful> jsonFormatter, IJsonFormatterResolver formatterResolver)
        {
            _jsonFormatter = jsonFormatter;
            _formatterResolver = formatterResolver;
        }

        public IJsonFormatter<T> GetFormatter<T>()
        {
            if (typeof(T) == typeof(TStateful))
                return (IJsonFormatter<T>)_jsonFormatter;

            return _formatterResolver.GetFormatter<T>();
        }

        public IConnectionSettingsValues Settings => ((IJsonFormatterResolverWithSettings)_formatterResolver).Settings;
    }
}
