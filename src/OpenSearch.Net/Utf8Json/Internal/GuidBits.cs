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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenSearch.Net.Utf8Json.Internal;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
internal struct GuidBits
{
    [FieldOffset(0)]
    public readonly Guid Value;

    [FieldOffset(0)]
    public readonly byte Byte0;
    [FieldOffset(1)]
    public readonly byte Byte1;
    [FieldOffset(2)]
    public readonly byte Byte2;
    [FieldOffset(3)]
    public readonly byte Byte3;
    [FieldOffset(4)]
    public readonly byte Byte4;
    [FieldOffset(5)]
    public readonly byte Byte5;
    [FieldOffset(6)]
    public readonly byte Byte6;
    [FieldOffset(7)]
    public readonly byte Byte7;
    [FieldOffset(8)]
    public readonly byte Byte8;
    [FieldOffset(9)]
    public readonly byte Byte9;
    [FieldOffset(10)]
    public readonly byte Byte10;
    [FieldOffset(11)]
    public readonly byte Byte11;
    [FieldOffset(12)]
    public readonly byte Byte12;
    [FieldOffset(13)]
    public readonly byte Byte13;
    [FieldOffset(14)]
    public readonly byte Byte14;
    [FieldOffset(15)]
    public readonly byte Byte15;

    // string.Join(", ", Enumerable.Range(0, 256).Select(x => (int)BitConverter.ToString(new byte[] { (byte)x }).ToLower()[0]))
    private static byte[] byteToHexStringHigh = [48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102];

    // string.Join(", ", Enumerable.Range(0, 256).Select(x => (int)BitConverter.ToString(new byte[] { (byte)x }).ToLower()[1]))
    private static byte[] byteToHexStringLow = [48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102];

    public GuidBits(ref Guid value)
    {
        this = default;
        Value = value;
    }

    // 4-pattern, lower/upper and '-' or no
    public GuidBits(ref ArraySegment<byte> utf8string)
    {
        this = default(GuidBits);

        var array = utf8string.Array;
        var offset = utf8string.Offset;

        // 32
        if (utf8string.Count == 32)
        {
            if (BitConverter.IsLittleEndian)
            {
                Byte0 = Parse(array, offset + 6);
                Byte1 = Parse(array, offset + 4);
                Byte2 = Parse(array, offset + 2);
                Byte3 = Parse(array, offset + 0);

                Byte4 = Parse(array, offset + 10);
                Byte5 = Parse(array, offset + 8);

                Byte6 = Parse(array, offset + 14);
                Byte7 = Parse(array, offset + 12);
            }
            else
            {
                Byte0 = Parse(array, offset + 0);
                Byte1 = Parse(array, offset + 2);
                Byte2 = Parse(array, offset + 4);
                Byte3 = Parse(array, offset + 6);

                Byte4 = Parse(array, offset + 8);
                Byte5 = Parse(array, offset + 10);

                Byte6 = Parse(array, offset + 12);
                Byte7 = Parse(array, offset + 14);
            }
            Byte8 = Parse(array, offset + 16);
            Byte9 = Parse(array, offset + 18);

            Byte10 = Parse(array, offset + 20);
            Byte11 = Parse(array, offset + 22);
            Byte12 = Parse(array, offset + 24);
            Byte13 = Parse(array, offset + 26);
            Byte14 = Parse(array, offset + 28);
            Byte15 = Parse(array, offset + 30);
            return;
        }
        else if (utf8string.Count == 36)
        {
            // '-' => 45
            if (BitConverter.IsLittleEndian)
            {
                Byte0 = Parse(array, offset + 6);
                Byte1 = Parse(array, offset + 4);
                Byte2 = Parse(array, offset + 2);
                Byte3 = Parse(array, offset + 0);

                if (array[offset + 8] != '-') goto ERROR;

                Byte4 = Parse(array, offset + 11);
                Byte5 = Parse(array, offset + 9);

                if (array[offset + 13] != '-') goto ERROR;

                Byte6 = Parse(array, offset + 16);
                Byte7 = Parse(array, offset + 14);
            }
            else
            {
                Byte0 = Parse(array, offset + 0);
                Byte1 = Parse(array, offset + 2);
                Byte2 = Parse(array, offset + 4);
                Byte3 = Parse(array, offset + 6);

                if (array[offset + 8] != '-') goto ERROR;

                Byte4 = Parse(array, offset + 9);
                Byte5 = Parse(array, offset + 11);

                if (array[offset + 13] != '-') goto ERROR;

                Byte6 = Parse(array, offset + 14);
                Byte7 = Parse(array, offset + 16);
            }

            if (array[offset + 18] != '-') goto ERROR;

            Byte8 = Parse(array, offset + 19);
            Byte9 = Parse(array, offset + 21);

            if (array[offset + 23] != '-') goto ERROR;

            Byte10 = Parse(array, offset + 24);
            Byte11 = Parse(array, offset + 26);
            Byte12 = Parse(array, offset + 28);
            Byte13 = Parse(array, offset + 30);
            Byte14 = Parse(array, offset + 32);
            Byte15 = Parse(array, offset + 34);
            return;
        }

    ERROR:
        throw new ArgumentException("Invalid Guid Pattern.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte Parse(byte[] bytes, int highOffset) => unchecked((byte)(SwitchParse(bytes[highOffset]) * 16 + SwitchParse(bytes[highOffset + 1])));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte SwitchParse(byte b)
    {
        // '0'(48) ~ '9'(57) => -48
        // 'A'(65) ~ 'F'(70) => -55
        // 'a'(97) ~ 'f'(102) => -87
        switch (b)
        {
            case 48:
            case 49:
            case 50:
            case 51:
            case 52:
            case 53:
            case 54:
            case 55:
            case 56:
            case 57:
                return unchecked((byte)((b - 48)));
            case 65:
            case 66:
            case 67:
            case 68:
            case 69:
            case 70:
                return unchecked((byte)((b - 55)));
            case 97:
            case 98:
            case 99:
            case 100:
            case 101:
            case 102:
                return unchecked((byte)((b - 87)));
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
            case 32:
            case 33:
            case 34:
            case 35:
            case 36:
            case 37:
            case 38:
            case 39:
            case 40:
            case 41:
            case 42:
            case 43:
            case 44:
            case 45:
            case 46:
            case 47:
            case 58:
            case 59:
            case 60:
            case 61:
            case 62:
            case 63:
            case 64:
            case 71:
            case 72:
            case 73:
            case 74:
            case 75:
            case 76:
            case 77:
            case 78:
            case 79:
            case 80:
            case 81:
            case 82:
            case 83:
            case 84:
            case 85:
            case 86:
            case 87:
            case 88:
            case 89:
            case 90:
            case 91:
            case 92:
            case 93:
            case 94:
            case 95:
            case 96:
            default:
                throw new ArgumentException("Invalid Guid Pattern.");
        }
    }

    // 4(x2) - 2(x2) - 2(x2) - 2(x2) - 6(x2)
    public void Write(byte[] buffer, int offset)
    {
        if (BitConverter.IsLittleEndian)
        {
            // int(_a)
            buffer[offset + 6] = byteToHexStringHigh[Byte0];
            buffer[offset + 7] = byteToHexStringLow[Byte0];
            buffer[offset + 4] = byteToHexStringHigh[Byte1];
            buffer[offset + 5] = byteToHexStringLow[Byte1];
            buffer[offset + 2] = byteToHexStringHigh[Byte2];
            buffer[offset + 3] = byteToHexStringLow[Byte2];
            buffer[offset + 0] = byteToHexStringHigh[Byte3];
            buffer[offset + 1] = byteToHexStringLow[Byte3];

            buffer[offset + 8] = (byte)'-';

            // short(_b)
            buffer[offset + 11] = byteToHexStringHigh[Byte4];
            buffer[offset + 12] = byteToHexStringLow[Byte4];
            buffer[offset + 9] = byteToHexStringHigh[Byte5];
            buffer[offset + 10] = byteToHexStringLow[Byte5];

            buffer[offset + 13] = (byte)'-';

            // short(_c)
            buffer[offset + 16] = byteToHexStringHigh[Byte6];
            buffer[offset + 17] = byteToHexStringLow[Byte6];
            buffer[offset + 14] = byteToHexStringHigh[Byte7];
            buffer[offset + 15] = byteToHexStringLow[Byte7];
        }
        else
        {
            buffer[offset + 0] = byteToHexStringHigh[Byte0];
            buffer[offset + 1] = byteToHexStringLow[Byte0];
            buffer[offset + 2] = byteToHexStringHigh[Byte1];
            buffer[offset + 3] = byteToHexStringLow[Byte1];
            buffer[offset + 4] = byteToHexStringHigh[Byte2];
            buffer[offset + 5] = byteToHexStringLow[Byte2];
            buffer[offset + 6] = byteToHexStringHigh[Byte3];
            buffer[offset + 7] = byteToHexStringLow[Byte3];

            buffer[offset + 8] = (byte)'-';

            buffer[offset + 9] = byteToHexStringHigh[Byte4];
            buffer[offset + 10] = byteToHexStringLow[Byte4];
            buffer[offset + 11] = byteToHexStringHigh[Byte5];
            buffer[offset + 12] = byteToHexStringLow[Byte5];

            buffer[offset + 13] = (byte)'-';

            buffer[offset + 14] = byteToHexStringHigh[Byte6];
            buffer[offset + 15] = byteToHexStringLow[Byte6];
            buffer[offset + 16] = byteToHexStringHigh[Byte7];
            buffer[offset + 17] = byteToHexStringLow[Byte7];
        }

        buffer[offset + 18] = (byte)'-';

        buffer[offset + 19] = byteToHexStringHigh[Byte8];
        buffer[offset + 20] = byteToHexStringLow[Byte8];
        buffer[offset + 21] = byteToHexStringHigh[Byte9];
        buffer[offset + 22] = byteToHexStringLow[Byte9];

        buffer[offset + 23] = (byte)'-';

        buffer[offset + 24] = byteToHexStringHigh[Byte10];
        buffer[offset + 25] = byteToHexStringLow[Byte10];
        buffer[offset + 26] = byteToHexStringHigh[Byte11];
        buffer[offset + 27] = byteToHexStringLow[Byte11];
        buffer[offset + 28] = byteToHexStringHigh[Byte12];
        buffer[offset + 29] = byteToHexStringLow[Byte12];
        buffer[offset + 30] = byteToHexStringHigh[Byte13];
        buffer[offset + 31] = byteToHexStringLow[Byte13];
        buffer[offset + 32] = byteToHexStringHigh[Byte14];
        buffer[offset + 33] = byteToHexStringLow[Byte14];
        buffer[offset + 34] = byteToHexStringHigh[Byte15];
        buffer[offset + 35] = byteToHexStringLow[Byte15];
    }
}
