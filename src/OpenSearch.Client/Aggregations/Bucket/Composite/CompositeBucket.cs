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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A bucket with a composite key
/// </summary>
public class CompositeBucket : BucketBase
{
    public CompositeBucket(IReadOnlyDictionary<string, IAggregate> dict, CompositeKey key) : base(dict) =>
        Key = key;

    /// <summary>
    /// The count of documents
    /// </summary>
    public long? DocCount { get; set; }

    /// <summary>
    /// The bucket key
    /// </summary>
    public CompositeKey Key { get; }
}

/// <summary>
/// A key for a <see cref="CompositeBucket" />
/// </summary>
[JsonFormatter(typeof(CompositeKeyFormatter))]
public class CompositeKey : IsAReadOnlyDictionaryBase<string, object>
{
    public CompositeKey(IReadOnlyDictionary<string, object> backingDictionary) : base(backingDictionary) { }

    /// <summary>
    /// Tries to get a value with the given key as a string. Returns <c>false</c> if the key does
    /// not exist or is not a string.
    /// </summary>
    public bool TryGetValue(string key, out string value) => TryGetValue<string>(key, out value);

    /// <summary>
    /// Tries to get a value with the given key as a double. Returns <c>false</c> if the key does
    /// not exist or is not a double.
    /// </summary>
    public bool TryGetValue(string key, out double value) => TryGetValue<double>(key, out value);

    /// <summary>
    /// Tries to get a value with the given key as a int. Returns <c>false</c> if the key does
    /// not exist or is not a int.
    /// </summary>
    public bool TryGetValue(string key, out int value) => TryGetValue<int>(key, out value);

    /// <summary>
    /// Tries to get a value with the given key as a long. Returns <c>false</c> if the key does
    /// not exist or is not a long.
    /// </summary>
    public bool TryGetValue(string key, out long value) => TryGetValue<long>(key, out value);

    /// <summary>
    /// Tries to get a value with the given key as a DateTime. Returns <c>false</c> if the key does
    /// not exist or cannot be converted to a DateTime.
    /// </summary>
    public bool TryGetValue(string key, out DateTime value)
    {
        if (TryGetValue(key, out DateTimeOffset dateTimeOffset))
        {
            value = dateTimeOffset.DateTime;
            return true;
        }

        value = default(DateTime);
        return false;
    }

    /// <summary>
    /// Tries to get a value with the given key as a DateTimeOffset. Returns <c>false</c> if the key does
    /// not exist or cannot be converted to a DateTimeOffset.
    /// </summary>
    public bool TryGetValue(string key, out DateTimeOffset value)
    {
        var exists = TryGetValue(key, out object o);
        if (!exists || !long.TryParse(o.ToString(), out var l))
        {
            value = default(DateTimeOffset);
            return false;
        }

        value = DateTimeUtil.UnixEpoch.AddMilliseconds(l);
        return true;
    }

    private bool TryGetValue<TValue>(string key, out TValue value)
    {
        if (!BackingDictionary.TryGetValue(key, out var obj))
        {
            value = default(TValue);
            return false;
        }

        try
        {
            value = (TValue)Convert.ChangeType(obj, typeof(TValue));
            return true;
        }
        catch
        {
            value = default(TValue);
            return false;
        }
    }
}

internal class CompositeKeyFormatter : IJsonFormatter<CompositeKey>
{
    private static readonly VerbatimInterfaceReadOnlyDictionaryKeysPreservingNullFormatter<string, object> DictionaryFormatter =
        new VerbatimInterfaceReadOnlyDictionaryKeysPreservingNullFormatter<string, object>();

    public void Serialize(ref JsonWriter writer, CompositeKey value, IJsonFormatterResolver formatterResolver) =>
        DictionaryFormatter.Serialize(ref writer, value, formatterResolver);

    public CompositeKey Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.ReadIsNull())
            return null;

        var dictionary = DictionaryFormatter.Deserialize(ref reader, formatterResolver);
        return new CompositeKey(dictionary);
    }
}
