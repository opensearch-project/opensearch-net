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
using System.Globalization;
using System.Linq;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

internal class VerbatimDictionaryKeysBaseFormatter<TDictionary, TKey, TValue> : IJsonFormatter<TDictionary>
    where TDictionary : IEnumerable<KeyValuePair<TKey, TValue>>
{
    private readonly bool _keyIsField = typeof(TKey) == typeof(Field);
    private readonly bool _keyIsIndexName = typeof(TKey) == typeof(IndexName);
    private readonly bool _keyIsPropertyName = typeof(TKey) == typeof(PropertyName);
    private readonly bool _keyIsRelationName = typeof(TKey) == typeof(RelationName);
    private readonly bool _keyIsString = typeof(TKey) == typeof(string);

    public virtual TDictionary Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var formatter = formatterResolver.GetFormatter<TDictionary>();
        return formatter.Deserialize(ref reader, formatterResolver);
    }

    public void Serialize(ref JsonWriter writer, TDictionary value, IJsonFormatterResolver formatterResolver)
    {
        var enumerable = (IEnumerable<KeyValuePair<TKey, TValue>>)value;
        if (enumerable == null)
        {
            writer.WriteNull();
            return;
        }

        var settings = formatterResolver.GetConnectionSettings();
        var seenEntries = new Dictionary<string, TValue>(value.Count());

        foreach (var entry in enumerable)
        {
            if (SkipValue(entry)) // TODO: pass _connectionSettingsValues and configure Null Handling on this
                continue;

            string key;
            if (_keyIsString)
                key = entry.Key?.ToString();
            else if (settings == null)
                key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
            else if (_keyIsField)
            {
                var fieldName = entry.Key as Field;
                key = settings.Inferrer.Field(fieldName);
            }
            else if (_keyIsPropertyName)
            {
                var propertyName = entry.Key as PropertyName;
                if (propertyName?.Property != null && settings.PropertyMappings.TryGetValue(propertyName.Property, out var mapping)
                    && mapping.Ignore)
                    continue;

                key = settings.Inferrer.PropertyName(propertyName);
            }
            else if (_keyIsIndexName)
            {
                var indexName = entry.Key as IndexName;
                key = settings.Inferrer.IndexName(indexName);
            }
            else if (_keyIsRelationName)
            {
                var typeName = entry.Key as RelationName;
                key = settings.Inferrer.RelationName(typeName);
            }
            else
                key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);

            if (key != null)
                seenEntries[key] = entry.Value;
        }

        writer.WriteBeginObject();
        if (seenEntries.Count > 0)
        {
            var valueFormatter = formatterResolver.GetFormatter<TValue>();
            var count = 0;
            foreach (var entry in seenEntries)
            {
                if (count > 0)
                    writer.WriteValueSeparator();

                writer.WritePropertyName(entry.Key);
                valueFormatter.Serialize(ref writer, entry.Value, formatterResolver);
                count++;
            }
        }
        writer.WriteEndObject();
    }

    protected virtual bool SkipValue(KeyValuePair<TKey, TValue> entry) => entry.Value == null;
}

internal class VerbatimInterfaceReadOnlyDictionaryKeysPreservingNullFormatter<TKey, TValue>
    : VerbatimInterfaceReadOnlyDictionaryKeysFormatter<TKey, TValue>
{
    protected override bool SkipValue(KeyValuePair<TKey, TValue> entry) => false;
}

internal class VerbatimInterfaceReadOnlyDictionaryKeysFormatter<TKey, TValue>
    : VerbatimDictionaryKeysBaseFormatter<IReadOnlyDictionary<TKey, TValue>, TKey, TValue>
{
    public override IReadOnlyDictionary<TKey, TValue> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        // needed as Utf8Json does not handle the ctor for a IReadOnlyDictionary<TKey, TValue>
        var formatter = formatterResolver.GetFormatter<Dictionary<TKey, TValue>>();
        return formatter.Deserialize(ref reader, formatterResolver);
    }
}

internal class VerbatimDictionaryKeysFormatter<TKey, TValue>
    : VerbatimDictionaryKeysBaseFormatter<Dictionary<TKey, TValue>, TKey, TValue>
{
}

internal class VerbatimDictionaryInterfaceKeysFormatter<TKey, TValue>
    : VerbatimDictionaryKeysBaseFormatter<IDictionary<TKey, TValue>, TKey, TValue>
{
}
