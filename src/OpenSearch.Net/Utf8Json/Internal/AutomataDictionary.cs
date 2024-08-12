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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using OpenSearch.Net.Utf8Json.Internal.Emit;

namespace OpenSearch.Net.Utf8Json.Internal;

// Key = long, Value = int for UTF8String Dictionary

internal class AutomataDictionary : IEnumerable<KeyValuePair<string, int>>
{
    private readonly AutomataNode _root;

    public AutomataDictionary() => _root = new AutomataNode(0);

    public void Add(string str, int value) => Add(JsonWriter.GetEncodedPropertyNameWithoutQuotation(str), value);

    public unsafe void Add(byte[] bytes, int value)
    {
        fixed (byte* buffer = &bytes[0])
        {
            var node = _root;

            var p = buffer;
            var rest = bytes.Length;
            while (rest != 0)
            {
                var key = AutomataKeyGen.GetKey(ref p, ref rest);

                node = rest == 0
                    ? node.Add(key, value, Encoding.UTF8.GetString(bytes))
                    : node.Add(key);
            }
        }
    }

    public bool TryGetValue(ArraySegment<byte> bytes, out int value) =>
        TryGetValue(bytes.Array, bytes.Offset, bytes.Count, out value);

    private unsafe bool TryGetValue(byte[] bytes, int offset, int count, out int value)
    {
        fixed (byte* p = &bytes[offset])
        {
            var p1 = p;
            var node = _root;
            var rest = count;

            while (rest != 0 && node != null)
                node = node.SearchNext(ref p1, ref rest);

            if (node == null)
            {
                value = -1;
                return false;
            }

            value = node.Value;
            return true;
        }
    }

    public bool TryGetValueSafe(ArraySegment<byte> key, out int value)
    {
        var node = _root;
        var bytes = key.Array;
        var offset = key.Offset;
        var rest = key.Count;

        while (rest != 0 && node != null)
            node = node.SearchNextSafe(bytes, ref offset, ref rest);

        if (node == null)
        {
            value = -1;
            return false;
        }

        value = node.Value;
        return true;
    }

    // for debugging
    public override string ToString()
    {
        var sb = new StringBuilder();
        ToStringCore(_root.YieldChildren(), sb, 0);
        return sb.ToString();
    }

    private static void ToStringCore(IEnumerable<AutomataNode> nexts, StringBuilder sb, int depth)
    {
        foreach (var item in nexts)
        {
            if (depth != 0)
                sb.Append(' ', depth * 2);
            sb.Append("[" + item.Key + "]");
            if (item.Value != -1)
            {
                sb.Append("(" + item.OriginalKey + ")");
                sb.Append(" = ");
                sb.Append(item.Value);
            }
            sb.AppendLine();
            ToStringCore(item.YieldChildren(), sb, depth + 1);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, int>> GetEnumerator() => YieldCore(_root.YieldChildren()).GetEnumerator();

    private static IEnumerable<KeyValuePair<string, int>> YieldCore(IEnumerable<AutomataNode> nexts)
    {
        foreach (var item in nexts)
        {
            if (item.Value != -1) yield return new KeyValuePair<string, int>(item.OriginalKey, item.Value);
            foreach (var x in YieldCore(item.YieldChildren())) yield return x;
        }
    }

    // IL Emit

    public void EmitMatch(ILGenerator il, LocalBuilder p, LocalBuilder rest, LocalBuilder key, Action<KeyValuePair<string, int>> onFound, Action onNotFound) =>
        _root.EmitSearchNext(il, p, rest, key, onFound, onNotFound);

    private class AutomataNode : IComparable<AutomataNode>
    {
        private static readonly AutomataNode[] EmptyNodes = new AutomataNode[0];
        private static readonly ulong[] EmptyKeys = new ulong[0];

        public readonly ulong Key;
        public int Value;
        public string OriginalKey;

        private AutomataNode[] _nexts;
        private ulong[] _nextKeys;
        private int _count;

        public bool HasChildren => _count != 0;

        public AutomataNode(ulong key)
        {
            Key = key;
            Value = -1;
            _nexts = EmptyNodes;
            _nextKeys = EmptyKeys;
            _count = 0;
            OriginalKey = null;
        }

        public AutomataNode Add(ulong key)
        {
            var index = Array.BinarySearch(_nextKeys, 0, _count, key);
            if (index < 0)
            {
                if (_nexts.Length == _count)
                {
                    Array.Resize(ref _nexts, (_count == 0) ? 4 : (_count * 2));
                    Array.Resize(ref _nextKeys, (_count == 0) ? 4 : (_count * 2));
                }
                _count++;

                var nextNode = new AutomataNode(key);
                _nexts[_count - 1] = nextNode;
                _nextKeys[_count - 1] = key;
                Array.Sort(_nexts, 0, _count);
                Array.Sort(_nextKeys, 0, _count);
                return nextNode;
            }

            return _nexts[index];
        }

        public AutomataNode Add(ulong key, int value, string originalKey)
        {
            var v = Add(key);
            v.Value = value;
            v.OriginalKey = originalKey;
            return v;
        }

        public unsafe AutomataNode SearchNext(ref byte* p, ref int rest)
        {
            var key = AutomataKeyGen.GetKey(ref p, ref rest);
            if (_count < 4)
            {
                // linear search
                for (var i = 0; i < _count; i++)
                {
                    if (_nextKeys[i] == key)
                    {
                        return _nexts[i];
                    }
                }
            }
            else
            {
                // binary search
                var index = BinarySearch(_nextKeys, 0, _count, key);
                if (index >= 0)
                {
                    return _nexts[index];
                }
            }

            return null;
        }

        public unsafe AutomataNode SearchNextSafe(byte[] p, ref int offset, ref int rest)
        {
            var key = AutomataKeyGen.GetKeySafe(p, ref offset, ref rest);
            if (_count < 4)
            {
                // linear search
                for (var i = 0; i < _count; i++)
                {
                    if (_nextKeys[i] == key)
                    {
                        return _nexts[i];
                    }
                }
            }
            else
            {
                // binary search
                var index = BinarySearch(_nextKeys, 0, _count, key);
                if (index >= 0)
                {
                    return _nexts[index];
                }
            }

            return null;
        }

        private static int BinarySearch(ulong[] array, int index, int length, ulong value)
        {
            var lo = index;
            var hi = index + length - 1;
            while (lo <= hi)
            {
                var i = lo + ((hi - lo) >> 1);

                var arrayValue = array[i];
                int order;
                if (arrayValue < value) order = -1;
                else if (arrayValue > value) order = 1;
                else order = 0;

                if (order == 0) return i;
                if (order < 0)
                    lo = i + 1;
                else
                    hi = i - 1;
            }

            return ~lo;
        }

        public int CompareTo(AutomataNode other) => Key.CompareTo(other.Key);

        public IEnumerable<AutomataNode> YieldChildren()
        {
            for (var i = 0; i < _count; i++)
                yield return _nexts[i];
        }

        // SearchNext(ref byte* p, ref int rest, ref ulong key)
        public void EmitSearchNext(ILGenerator il, LocalBuilder p, LocalBuilder rest, LocalBuilder key, Action<KeyValuePair<string, int>> onFound, Action onNotFound)
        {
            // key = AutomataKeyGen.GetKey(ref p, ref rest);
            il.EmitLdloca(p);
            il.EmitLdloca(rest);
            il.EmitCall(AutomataKeyGen.GetKeyMethod);
            il.EmitStloc(key);

            // match children.
            EmitSearchNextCore(il, p, rest, key, onFound, onNotFound, _nexts, _count);
        }

        private static void EmitSearchNextCore(ILGenerator il, LocalBuilder p, LocalBuilder rest, LocalBuilder key, Action<KeyValuePair<string, int>> onFound, Action onNotFound, AutomataNode[] nexts, int count)
        {
            if (count < 4)
            {
                // linear-search
                var valueExists = nexts.Take(count).Where(x => x.Value != -1).ToArray();
                var childrenExists = nexts.Take(count).Where(x => x.HasChildren).ToArray();
                var gotoSearchNext = il.DefineLabel();
                var gotoNotFound = il.DefineLabel();

                {
                    il.EmitLdloc(rest);
                    if (childrenExists.Length != 0 && valueExists.Length == 0)
                        il.Emit(OpCodes.Brfalse, gotoNotFound); // if(rest == 0)
                    else
                        il.Emit(OpCodes.Brtrue, gotoSearchNext); // if(rest != 0)
                }
                {
                    var ifValueNexts = Enumerable.Range(0, Math.Max(valueExists.Length - 1, 0)).Select(_ => il.DefineLabel()).ToArray();
                    for (var i = 0; i < valueExists.Length; i++)
                    {
                        var notFoundLabel = il.DefineLabel();
                        if (i != 0)
                        {
                            il.MarkLabel(ifValueNexts[i - 1]);
                        }

                        il.EmitLdloc(key);
                        il.EmitULong(valueExists[i].Key);
                        il.Emit(OpCodes.Bne_Un, notFoundLabel);
                        // found
                        onFound(new KeyValuePair<string, int>(valueExists[i].OriginalKey, valueExists[i].Value));

                        // notfound
                        il.MarkLabel(notFoundLabel);
                        if (i != valueExists.Length - 1)
                            il.Emit(OpCodes.Br, ifValueNexts[i]);
                        else
                            onNotFound();
                    }
                }

                il.MarkLabel(gotoSearchNext);
                var ifRecNext = Enumerable.Range(0, Math.Max(childrenExists.Length - 1, 0)).Select(_ => il.DefineLabel()).ToArray();
                for (var i = 0; i < childrenExists.Length; i++)
                {
                    var notFoundLabel = il.DefineLabel();
                    if (i != 0)
                        il.MarkLabel(ifRecNext[i - 1]);

                    il.EmitLdloc(key);
                    il.EmitULong(childrenExists[i].Key);
                    il.Emit(OpCodes.Bne_Un, notFoundLabel);
                    // found
                    childrenExists[i].EmitSearchNext(il, p, rest, key, onFound, onNotFound);
                    // notfound
                    il.MarkLabel(notFoundLabel);
                    if (i != childrenExists.Length - 1)
                        il.Emit(OpCodes.Br, ifRecNext[i]);
                    else
                        onNotFound();
                }

                il.MarkLabel(gotoNotFound);
                onNotFound();
            }
            else
            {
                // binary-search
                var midline = count / 2;
                var mid = nexts[midline].Key;
                var l = nexts.Take(count).Take(midline).ToArray();
                var r = nexts.Take(count).Skip(midline).ToArray();

                var gotoRight = il.DefineLabel();

                // if(key < mid)
                il.EmitLdloc(key);
                il.EmitULong(mid);
                il.Emit(OpCodes.Bge, gotoRight);
                EmitSearchNextCore(il, p, rest, key, onFound, onNotFound, l, l.Length);

                // else
                il.MarkLabel(gotoRight);
                EmitSearchNextCore(il, p, rest, key, onFound, onNotFound, r, r.Length);
            }
        }
    }
}

internal static class AutomataKeyGen
{
    public static readonly MethodInfo GetKeyMethod = typeof(AutomataKeyGen).GetRuntimeMethod(nameof(GetKey), new[] { typeof(byte*).MakeByRefType(), typeof(int).MakeByRefType() });
    public static unsafe ulong GetKey(ref byte* p, ref int rest)
    {
        int readSize;
        ulong key;

        unchecked
        {
            if (rest >= 8)
            {
                key = *(ulong*)p;
                readSize = 8;
            }
            else
            {
                switch (rest)
                {
                    case 1:
                        {
                            key = *(byte*)p;
                            readSize = 1;
                            break;
                        }
                    case 2:
                        {
                            key = *(ushort*)p;
                            readSize = 2;
                            break;
                        }
                    case 3:
                        {
                            var a = *p;
                            var b = *(ushort*)(p + 1);
                            key = ((ulong)a | (ulong)b << 8);
                            readSize = 3;
                            break;
                        }
                    case 4:
                        {
                            key = *(uint*)p;
                            readSize = 4;
                            break;
                        }
                    case 5:
                        {
                            var a = *p;
                            var b = *(uint*)(p + 1);
                            key = ((ulong)a | (ulong)b << 8);
                            readSize = 5;
                            break;
                        }
                    case 6:
                        {
                            ulong a = *(ushort*)p;
                            ulong b = *(uint*)(p + 2);
                            key = (a | (b << 16));
                            readSize = 6;
                            break;
                        }
                    case 7:
                        {
                            var a = *(byte*)p;
                            var b = *(ushort*)(p + 1);
                            var c = *(uint*)(p + 3);
                            key = ((ulong)a | (ulong)b << 8 | (ulong)c << 24);
                            readSize = 7;
                            break;
                        }
                    default:
                        throw new InvalidOperationException("Not Supported Length");
                }
            }

            p += readSize;
            rest -= readSize;
            return key;
        }
    }

    public static ulong GetKeySafe(byte[] bytes, ref int offset, ref int rest)
    {
        int readSize;
        ulong key;

        if (BitConverter.IsLittleEndian)
        {
            unchecked
            {
                if (rest >= 8)
                {
                    key = (ulong)bytes[offset] << 0 | (ulong)bytes[offset + 1] << 8 | (ulong)bytes[offset + 2] << 16 | (ulong)bytes[offset + 3] << 24
                        | (ulong)bytes[offset + 4] << 32 | (ulong)bytes[offset + 5] << 40 | (ulong)bytes[offset + 6] << 48 | (ulong)bytes[offset + 7] << 56;
                    readSize = 8;
                }
                else
                {
                    switch (rest)
                    {
                        case 1:
                            {
                                key = bytes[offset];
                                readSize = 1;
                                break;
                            }
                        case 2:
                            {
                                key = (ulong)bytes[offset] << 0 | (ulong)bytes[offset + 1] << 8;
                                readSize = 2;
                                break;
                            }
                        case 3:
                            {
                                key = (ulong)bytes[offset] << 0 | (ulong)bytes[offset + 1] << 8 | (ulong)bytes[offset + 2] << 16;
                                readSize = 3;
                                break;
                            }
                        case 4:
                            {
                                key = (ulong)bytes[offset] << 0 | (ulong)bytes[offset + 1] << 8 | (ulong)bytes[offset + 2] << 16 | (ulong)bytes[offset + 3] << 24;
                                readSize = 4;
                                break;
                            }
                        case 5:
                            {
                                key = (ulong)bytes[offset] << 0 | (ulong)bytes[offset + 1] << 8 | (ulong)bytes[offset + 2] << 16 | (ulong)bytes[offset + 3] << 24
                                    | (ulong)bytes[offset + 4] << 32;
                                readSize = 5;
                                break;
                            }
                        case 6:
                            {
                                key = (ulong)bytes[offset] << 0 | (ulong)bytes[offset + 1] << 8 | (ulong)bytes[offset + 2] << 16 | (ulong)bytes[offset + 3] << 24
                                    | (ulong)bytes[offset + 4] << 32 | (ulong)bytes[offset + 5] << 40;
                                readSize = 6;
                                break;
                            }
                        case 7:
                            {
                                key = (ulong)bytes[offset] << 0 | (ulong)bytes[offset + 1] << 8 | (ulong)bytes[offset + 2] << 16 | (ulong)bytes[offset + 3] << 24
                                    | (ulong)bytes[offset + 4] << 32 | (ulong)bytes[offset + 5] << 40 | (ulong)bytes[offset + 6] << 48;
                                readSize = 7;
                                break;
                            }
                        default:
                            throw new InvalidOperationException("Not Supported Length");
                    }
                }

                offset += readSize;
                rest -= readSize;
                return key;
            }
        }

        unchecked
        {
            if (rest >= 8)
            {
                key = (ulong)bytes[offset] << 56 | (ulong)bytes[offset + 1] << 48 | (ulong)bytes[offset + 2] << 40 | (ulong)bytes[offset + 3] << 32
                    | (ulong)bytes[offset + 4] << 24 | (ulong)bytes[offset + 5] << 16 | (ulong)bytes[offset + 6] << 8 | (ulong)bytes[offset + 7];
                readSize = 8;
            }
            else
            {
                switch (rest)
                {
                    case 1:
                        {
                            key = bytes[offset];
                            readSize = 1;
                            break;
                        }
                    case 2:
                        {
                            key = (ulong)bytes[offset] << 8 | (ulong)bytes[offset + 1] << 0;
                            readSize = 2;
                            break;
                        }
                    case 3:
                        {
                            key = (ulong)bytes[offset] << 16 | (ulong)bytes[offset + 1] << 8 | (ulong)bytes[offset + 2] << 0;
                            readSize = 3;
                            break;
                        }
                    case 4:
                        {
                            key = (ulong)bytes[offset] << 24 | (ulong)bytes[offset + 1] << 16 | (ulong)bytes[offset + 2] << 8 | (ulong)bytes[offset + 3] << 0;
                            readSize = 4;
                            break;
                        }
                    case 5:
                        {
                            key = (ulong)bytes[offset] << 32 | (ulong)bytes[offset + 1] << 24 | (ulong)bytes[offset + 2] << 16 | (ulong)bytes[offset + 3] << 8
                                | (ulong)bytes[offset + 4] << 0;
                            readSize = 5;
                            break;
                        }
                    case 6:
                        {
                            key = (ulong)bytes[offset] << 40 | (ulong)bytes[offset + 1] << 32 | (ulong)bytes[offset + 2] << 24 | (ulong)bytes[offset + 3] << 16
                                | (ulong)bytes[offset + 4] << 8 | (ulong)bytes[offset + 5] << 0;
                            readSize = 6;
                            break;
                        }
                    case 7:
                        {
                            key = (ulong)bytes[offset] << 48 | (ulong)bytes[offset + 1] << 40 | (ulong)bytes[offset + 2] << 32 | (ulong)bytes[offset + 3] << 24
                                | (ulong)bytes[offset + 4] << 16 | (ulong)bytes[offset + 5] << 8 | (ulong)bytes[offset + 6] << 0;
                            readSize = 7;
                            break;
                        }
                    default:
                        throw new InvalidOperationException("Not Supported Length");
                }
            }

            offset += readSize;
            rest -= readSize;
            return key;
        }
    }
}
