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
using OpenSearch.Net.Utf8Json.Internal;

namespace OpenSearch.Net.Utf8Json.Formatters
{
    // reduce static constructor generate size on generics(especially IL2CPP on Unity)
    internal static class TupleFormatterHelper
    {
        internal static readonly byte[][] nameCache1;
        internal static readonly AutomataDictionary dictionary1;
        internal static readonly byte[][] nameCache2;
        internal static readonly AutomataDictionary dictionary2;
        internal static readonly byte[][] nameCache3;
        internal static readonly AutomataDictionary dictionary3;
        internal static readonly byte[][] nameCache4;
        internal static readonly AutomataDictionary dictionary4;
        internal static readonly byte[][] nameCache5;
        internal static readonly AutomataDictionary dictionary5;
        internal static readonly byte[][] nameCache6;
        internal static readonly AutomataDictionary dictionary6;
        internal static readonly byte[][] nameCache7;
        internal static readonly AutomataDictionary dictionary7;
        internal static readonly byte[][] nameCache8;
        internal static readonly AutomataDictionary dictionary8;

        static TupleFormatterHelper()
        {
            nameCache1 = new[]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Item1"),
            };
            dictionary1 = new AutomataDictionary
            {
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item1"), 0 },
            };
            nameCache2 = new[]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Item1"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item2"),
            };
            dictionary2 = new AutomataDictionary
            {
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item1"), 0 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item2"), 1 },
            };
            nameCache3 = new[]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Item1"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item2"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item3"),
            };
            dictionary3 = new AutomataDictionary
            {
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item1"), 0 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item2"), 1 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item3"), 2 },
            };
            nameCache4 = new[]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Item1"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item2"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item3"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item4"),
            };
            dictionary4 = new AutomataDictionary
            {
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item1"), 0 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item2"), 1 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item3"), 2 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item4"), 3 },
            };
            nameCache5 = new[]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Item1"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item2"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item3"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item4"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item5"),
            };
            dictionary5 = new AutomataDictionary
            {
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item1"), 0 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item2"), 1 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item3"), 2 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item4"), 3 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item5"), 4 },
            };
            nameCache6 = new[]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Item1"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item2"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item3"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item4"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item5"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item6"),
            };
            dictionary6 = new AutomataDictionary
            {
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item1"), 0 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item2"), 1 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item3"), 2 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item4"), 3 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item5"), 4 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item6"), 5 },
            };
            nameCache7 = new[]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Item1"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item2"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item3"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item4"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item5"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item6"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item7"),
            };
            dictionary7 = new AutomataDictionary
            {
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item1"), 0 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item2"), 1 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item3"), 2 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item4"), 3 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item5"), 4 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item6"), 5 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item7"), 6 },
            };
            nameCache8 = new[]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Item1"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item2"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item3"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item4"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item5"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item6"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Item7"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Rest"),
            };
            dictionary8 = new AutomataDictionary
            {
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item1"), 0 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item2"), 1 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item3"), 2 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item4"), 3 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item5"), 4 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item6"), 5 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Item7"), 6 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Rest"), 7 },
            };
        }
    }
}

namespace OpenSearch.Net.Utf8Json.Formatters
{

    internal sealed class TupleFormatter<T1> : IJsonFormatter<Tuple<T1>>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly byte[][] Cache = TupleFormatterHelper.nameCache1;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly AutomataDictionary Dictionary = TupleFormatterHelper.dictionary1;

        public void Serialize(ref JsonWriter writer, Tuple<T1> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteRaw(Cache[0]);
            formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, formatterResolver);
            writer.WriteEndObject();
        }

        public Tuple<T1> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            T1 item1 = default;

            var count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var keyString = reader.ReadPropertyNameSegmentRaw();
                Dictionary.TryGetValue(keyString, out var key);

                switch (key)
                {
                    case 0:
                        item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, formatterResolver);
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
            }

            return new Tuple<T1>(item1);
        }
    }


    internal sealed class TupleFormatter<T1, T2> : IJsonFormatter<Tuple<T1, T2>>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly byte[][] Cache = TupleFormatterHelper.nameCache2;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly AutomataDictionary Dictionary = TupleFormatterHelper.dictionary2;

        public void Serialize(ref JsonWriter writer, Tuple<T1, T2> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteRaw(Cache[0]);
            formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, formatterResolver);
            writer.WriteRaw(Cache[1]);
            formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, formatterResolver);
            writer.WriteEndObject();
        }

        public Tuple<T1, T2> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            T1 item1 = default;
            T2 item2 = default;

            var count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var keyString = reader.ReadPropertyNameSegmentRaw();
                Dictionary.TryGetValue(keyString, out var key);

                switch (key)
                {
                    case 0:
                        item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 1:
                        item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, formatterResolver);
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
            }

            return new Tuple<T1, T2>(item1, item2);
        }
    }


    internal sealed class TupleFormatter<T1, T2, T3> : IJsonFormatter<Tuple<T1, T2, T3>>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly byte[][] Cache = TupleFormatterHelper.nameCache3;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly AutomataDictionary Dictionary = TupleFormatterHelper.dictionary3;

        public void Serialize(ref JsonWriter writer, Tuple<T1, T2, T3> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteRaw(Cache[0]);
            formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, formatterResolver);
            writer.WriteRaw(Cache[1]);
            formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, formatterResolver);
            writer.WriteRaw(Cache[2]);
            formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, formatterResolver);
            writer.WriteEndObject();
        }

        public Tuple<T1, T2, T3> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            T1 item1 = default;
            T2 item2 = default;
            T3 item3 = default;

            var count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var keyString = reader.ReadPropertyNameSegmentRaw();
                Dictionary.TryGetValue(keyString, out var key);

                switch (key)
                {
                    case 0:
                        item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 1:
                        item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 2:
                        item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, formatterResolver);
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
            }

            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }
    }


    internal sealed class TupleFormatter<T1, T2, T3, T4> : IJsonFormatter<Tuple<T1, T2, T3, T4>>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly byte[][] Cache = TupleFormatterHelper.nameCache4;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly AutomataDictionary Dictionary = TupleFormatterHelper.dictionary4;

        public void Serialize(ref JsonWriter writer, Tuple<T1, T2, T3, T4> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteRaw(Cache[0]);
            formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, formatterResolver);
            writer.WriteRaw(Cache[1]);
            formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, formatterResolver);
            writer.WriteRaw(Cache[2]);
            formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, formatterResolver);
            writer.WriteRaw(Cache[3]);
            formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, formatterResolver);
            writer.WriteEndObject();
        }

        public Tuple<T1, T2, T3, T4> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            T1 item1 = default;
            T2 item2 = default;
            T3 item3 = default;
            T4 item4 = default;

            var count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var keyString = reader.ReadPropertyNameSegmentRaw();
                Dictionary.TryGetValue(keyString, out var key);

                switch (key)
                {
                    case 0:
                        item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 1:
                        item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 2:
                        item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 3:
                        item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, formatterResolver);
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
            }

            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }


    internal sealed class TupleFormatter<T1, T2, T3, T4, T5> : IJsonFormatter<Tuple<T1, T2, T3, T4, T5>>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly byte[][] Cache = TupleFormatterHelper.nameCache5;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly AutomataDictionary Dictionary = TupleFormatterHelper.dictionary5;

        public void Serialize(ref JsonWriter writer, Tuple<T1, T2, T3, T4, T5> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteRaw(Cache[0]);
            formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, formatterResolver);
            writer.WriteRaw(Cache[1]);
            formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, formatterResolver);
            writer.WriteRaw(Cache[2]);
            formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, formatterResolver);
            writer.WriteRaw(Cache[3]);
            formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, formatterResolver);
            writer.WriteRaw(Cache[4]);
            formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, formatterResolver);
            writer.WriteEndObject();
        }

        public Tuple<T1, T2, T3, T4, T5> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            T1 item1 = default;
            T2 item2 = default;
            T3 item3 = default;
            T4 item4 = default;
            T5 item5 = default;

            var count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var keyString = reader.ReadPropertyNameSegmentRaw();
                Dictionary.TryGetValue(keyString, out var key);

                switch (key)
                {
                    case 0:
                        item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 1:
                        item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 2:
                        item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 3:
                        item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 4:
                        item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, formatterResolver);
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
            }

            return new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }
    }


    internal sealed class TupleFormatter<T1, T2, T3, T4, T5, T6> : IJsonFormatter<Tuple<T1, T2, T3, T4, T5, T6>>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly byte[][] Cache = TupleFormatterHelper.nameCache6;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly AutomataDictionary Dictionary = TupleFormatterHelper.dictionary6;

        public void Serialize(ref JsonWriter writer, Tuple<T1, T2, T3, T4, T5, T6> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteRaw(Cache[0]);
            formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, formatterResolver);
            writer.WriteRaw(Cache[1]);
            formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, formatterResolver);
            writer.WriteRaw(Cache[2]);
            formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, formatterResolver);
            writer.WriteRaw(Cache[3]);
            formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, formatterResolver);
            writer.WriteRaw(Cache[4]);
            formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, formatterResolver);
            writer.WriteRaw(Cache[5]);
            formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, formatterResolver);
            writer.WriteEndObject();
        }

        public Tuple<T1, T2, T3, T4, T5, T6> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            T1 item1 = default;
            T2 item2 = default;
            T3 item3 = default;
            T4 item4 = default;
            T5 item5 = default;
            T6 item6 = default;

            var count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var keyString = reader.ReadPropertyNameSegmentRaw();
                Dictionary.TryGetValue(keyString, out var key);

                switch (key)
                {
                    case 0:
                        item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 1:
                        item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 2:
                        item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 3:
                        item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 4:
                        item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 5:
                        item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, formatterResolver);
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
            }

            return new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }
    }


    internal sealed class TupleFormatter<T1, T2, T3, T4, T5, T6, T7> : IJsonFormatter<Tuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly byte[][] Cache = TupleFormatterHelper.nameCache7;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly AutomataDictionary Dictionary = TupleFormatterHelper.dictionary7;

        public void Serialize(ref JsonWriter writer, Tuple<T1, T2, T3, T4, T5, T6, T7> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteRaw(Cache[0]);
            formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, formatterResolver);
            writer.WriteRaw(Cache[1]);
            formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, formatterResolver);
            writer.WriteRaw(Cache[2]);
            formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, formatterResolver);
            writer.WriteRaw(Cache[3]);
            formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, formatterResolver);
            writer.WriteRaw(Cache[4]);
            formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, formatterResolver);
            writer.WriteRaw(Cache[5]);
            formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, formatterResolver);
            writer.WriteRaw(Cache[6]);
            formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, formatterResolver);
            writer.WriteEndObject();
        }

        public Tuple<T1, T2, T3, T4, T5, T6, T7> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            T1 item1 = default;
            T2 item2 = default;
            T3 item3 = default;
            T4 item4 = default;
            T5 item5 = default;
            T6 item6 = default;
            T7 item7 = default;

            var count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var keyString = reader.ReadPropertyNameSegmentRaw();
                Dictionary.TryGetValue(keyString, out var key);

                switch (key)
                {
                    case 0:
                        item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 1:
                        item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 2:
                        item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 3:
                        item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 4:
                        item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 5:
                        item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 6:
                        item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, formatterResolver);
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
            }

            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }
    }


    internal sealed class TupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : IJsonFormatter<Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly byte[][] Cache = TupleFormatterHelper.nameCache8;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly AutomataDictionary Dictionary = TupleFormatterHelper.dictionary8;

        public void Serialize(ref JsonWriter writer, Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteRaw(Cache[0]);
            formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, formatterResolver);
            writer.WriteRaw(Cache[1]);
            formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, formatterResolver);
            writer.WriteRaw(Cache[2]);
            formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, formatterResolver);
            writer.WriteRaw(Cache[3]);
            formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, formatterResolver);
            writer.WriteRaw(Cache[4]);
            formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, formatterResolver);
            writer.WriteRaw(Cache[5]);
            formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, formatterResolver);
            writer.WriteRaw(Cache[6]);
            formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, formatterResolver);
            writer.WriteRaw(Cache[7]);
            formatterResolver.GetFormatterWithVerify<TRest>().Serialize(ref writer, value.Rest, formatterResolver);
            writer.WriteEndObject();
        }

        public Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            T1 item1 = default;
            T2 item2 = default;
            T3 item3 = default;
            T4 item4 = default;
            T5 item5 = default;
            T6 item6 = default;
            T7 item7 = default;
            TRest item8 = default;

            var count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var keyString = reader.ReadPropertyNameSegmentRaw();
                Dictionary.TryGetValue(keyString, out var key);

                switch (key)
                {
                    case 0:
                        item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 1:
                        item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 2:
                        item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 3:
                        item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 4:
                        item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 5:
                        item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 6:
                        item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 7:
                        item8 = formatterResolver.GetFormatterWithVerify<TRest>().Deserialize(ref reader, formatterResolver);
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
            }

            return new Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>(item1, item2, item3, item4, item5, item6, item7, item8);
        }
    }

}

