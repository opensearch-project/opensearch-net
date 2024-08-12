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

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

internal class NullableStringLongFormatter : IJsonFormatter<long?>
{
    public long? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();
        switch (token)
        {
            case JsonToken.Null:
                reader.ReadNext();
                return null;
            case JsonToken.String:
                var s = reader.ReadString();
                if (!long.TryParse(s, out var l))
                    throw new JsonParsingException($"Cannot parse {typeof(long).FullName} from: {s}");

                return l;
            case JsonToken.Number:
                return reader.ReadInt64();
            default:
                throw new JsonParsingException($"Cannot parse {typeof(long).FullName} from: {token}");
        }
    }

    public void Serialize(ref JsonWriter writer, long? value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteInt64(value.Value);
    }
}

internal class NullableStringBooleanFormatter : IJsonFormatter<bool?>
{
    public bool? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();
        switch (token)
        {
            case JsonToken.Null:
                reader.ReadNext();
                return null;
            case JsonToken.String:
                var s = reader.ReadString();
                if (!bool.TryParse(s, out var b))
                    throw new JsonParsingException($"Cannot parse {typeof(bool).FullName} from: {s}");

                return b;
            case JsonToken.True:
            case JsonToken.False:
                return reader.ReadBoolean();
            default:
                throw new JsonParsingException($"Cannot parse {typeof(bool).FullName} from: {token}");
        }
    }

    public void Serialize(ref JsonWriter writer, bool? value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteBoolean(value.Value);
    }
}

internal class StringLongFormatter : IJsonFormatter<long>
{
    public long Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();
        switch (token)
        {
            case JsonToken.String:
                var s = reader.ReadString();
                if (!long.TryParse(s, out var i))
                    throw new JsonParsingException($"Cannot parse {typeof(long).FullName} from: {s}");

                return i;
            case JsonToken.Number:
                return reader.ReadInt64();
            default:
                throw new JsonParsingException($"Cannot parse {typeof(long).FullName} from: {token}");
        }
    }

    public void Serialize(ref JsonWriter writer, long value, IJsonFormatterResolver formatterResolver) =>
        writer.WriteInt64(value);
}

internal class StringIntFormatter : IJsonFormatter<int>
{
    public int Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();
        switch (token)
        {
            case JsonToken.String:
                var s = reader.ReadString();
                if (!int.TryParse(s, out var i))
                    throw new JsonParsingException($"Cannot parse {typeof(int).FullName} from: {s}");

                return i;
            case JsonToken.Number:
                return reader.ReadInt32();
            default:
                throw new JsonParsingException($"Cannot parse {typeof(int).FullName} from: {token}");
        }
    }

    public void Serialize(ref JsonWriter writer, int value, IJsonFormatterResolver formatterResolver) =>
        writer.WriteInt32(value);
}

internal class NullableStringIntFormatter : IJsonFormatter<int?>
{
    public int? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();
        switch (token)
        {
            case JsonToken.Null:
                reader.ReadNext();
                return null;
            case JsonToken.String:
                var s = reader.ReadString();
                if (!int.TryParse(s, out var i))
                    throw new JsonParsingException($"Cannot parse {typeof(int).FullName} from: {s}");

                return i;
            case JsonToken.Number:
                return reader.ReadInt32();
            default:
                throw new JsonParsingException($"Cannot parse {typeof(int).FullName} from: {token}");
        }
    }

    public void Serialize(ref JsonWriter writer, int? value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteInt32(value.Value);
    }
}

internal class NullableStringDoubleFormatter : IJsonFormatter<double?>
{
    public double? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();
        switch (token)
        {
            case JsonToken.Null:
                reader.ReadNext();
                return null;
            case JsonToken.String:
                var s = reader.ReadString();
                if (!double.TryParse(s, out var d))
                    throw new JsonParsingException($"Cannot parse {typeof(double).FullName} from: {s}");

                return d;
            case JsonToken.Number:
                return reader.ReadDouble();
            default:
                throw new JsonParsingException($"Cannot parse {typeof(double).FullName} from: {token}");
        }
    }

    public void Serialize(ref JsonWriter writer, double? value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteDouble(value.Value);
    }
}
