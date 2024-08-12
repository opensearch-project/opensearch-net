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
using System.IO;
using System.Runtime.CompilerServices;

namespace OpenSearch.Net.Utf8Json.Internal;

// for string key property name write optimization.

internal static class UnsafeMemory
{
    public static readonly bool Is32Bit = (IntPtr.Size == 4);

    public static void WriteRaw(ref JsonWriter writer, MemoryStream ms)
    {
        if (ms.TryGetBuffer(out var b) && !(b.Array is null) && b.Offset == 0)
            MemoryCopy(ref writer, b.Array, b.Count);
        else
        {
            var bytes = ms.ToArray();
            WriteRaw(ref writer, bytes);
        }
    }

    public static void WriteRaw(ref JsonWriter writer, byte[] src) => WriteRaw(ref writer, src, src.Length);
    public static void WriteRaw(ref JsonWriter writer, byte[] src, int length)
    {
        switch (length)
        {
            case 0: break;
            case 1: if (Is32Bit) { UnsafeMemory32.WriteRaw1(ref writer, src); } else { UnsafeMemory64.WriteRaw1(ref writer, src); } break;
            case 2: if (Is32Bit) { UnsafeMemory32.WriteRaw2(ref writer, src); } else { UnsafeMemory64.WriteRaw2(ref writer, src); } break;
            case 3: if (Is32Bit) { UnsafeMemory32.WriteRaw3(ref writer, src); } else { UnsafeMemory64.WriteRaw3(ref writer, src); } break;
            case 4: if (Is32Bit) { UnsafeMemory32.WriteRaw4(ref writer, src); } else { UnsafeMemory64.WriteRaw4(ref writer, src); } break;
            case 5: if (Is32Bit) { UnsafeMemory32.WriteRaw5(ref writer, src); } else { UnsafeMemory64.WriteRaw5(ref writer, src); } break;
            case 6: if (Is32Bit) { UnsafeMemory32.WriteRaw6(ref writer, src); } else { UnsafeMemory64.WriteRaw6(ref writer, src); } break;
            case 7: if (Is32Bit) { UnsafeMemory32.WriteRaw7(ref writer, src); } else { UnsafeMemory64.WriteRaw7(ref writer, src); } break;
            case 8: if (Is32Bit) { UnsafeMemory32.WriteRaw8(ref writer, src); } else { UnsafeMemory64.WriteRaw8(ref writer, src); } break;
            case 9: if (Is32Bit) { UnsafeMemory32.WriteRaw9(ref writer, src); } else { UnsafeMemory64.WriteRaw9(ref writer, src); } break;
            case 10: if (Is32Bit) { UnsafeMemory32.WriteRaw10(ref writer, src); } else { UnsafeMemory64.WriteRaw10(ref writer, src); } break;
            case 11: if (Is32Bit) { UnsafeMemory32.WriteRaw11(ref writer, src); } else { UnsafeMemory64.WriteRaw11(ref writer, src); } break;
            case 12: if (Is32Bit) { UnsafeMemory32.WriteRaw12(ref writer, src); } else { UnsafeMemory64.WriteRaw12(ref writer, src); } break;
            case 13: if (Is32Bit) { UnsafeMemory32.WriteRaw13(ref writer, src); } else { UnsafeMemory64.WriteRaw13(ref writer, src); } break;
            case 14: if (Is32Bit) { UnsafeMemory32.WriteRaw14(ref writer, src); } else { UnsafeMemory64.WriteRaw14(ref writer, src); } break;
            case 15: if (Is32Bit) { UnsafeMemory32.WriteRaw15(ref writer, src); } else { UnsafeMemory64.WriteRaw15(ref writer, src); } break;
            case 16: if (Is32Bit) { UnsafeMemory32.WriteRaw16(ref writer, src); } else { UnsafeMemory64.WriteRaw16(ref writer, src); } break;
            case 17: if (Is32Bit) { UnsafeMemory32.WriteRaw17(ref writer, src); } else { UnsafeMemory64.WriteRaw17(ref writer, src); } break;
            case 18: if (Is32Bit) { UnsafeMemory32.WriteRaw18(ref writer, src); } else { UnsafeMemory64.WriteRaw18(ref writer, src); } break;
            case 19: if (Is32Bit) { UnsafeMemory32.WriteRaw19(ref writer, src); } else { UnsafeMemory64.WriteRaw19(ref writer, src); } break;
            case 20: if (Is32Bit) { UnsafeMemory32.WriteRaw20(ref writer, src); } else { UnsafeMemory64.WriteRaw20(ref writer, src); } break;
            case 21: if (Is32Bit) { UnsafeMemory32.WriteRaw21(ref writer, src); } else { UnsafeMemory64.WriteRaw21(ref writer, src); } break;
            case 22: if (Is32Bit) { UnsafeMemory32.WriteRaw22(ref writer, src); } else { UnsafeMemory64.WriteRaw22(ref writer, src); } break;
            case 23: if (Is32Bit) { UnsafeMemory32.WriteRaw23(ref writer, src); } else { UnsafeMemory64.WriteRaw23(ref writer, src); } break;
            case 24: if (Is32Bit) { UnsafeMemory32.WriteRaw24(ref writer, src); } else { UnsafeMemory64.WriteRaw24(ref writer, src); } break;
            case 25: if (Is32Bit) { UnsafeMemory32.WriteRaw25(ref writer, src); } else { UnsafeMemory64.WriteRaw25(ref writer, src); } break;
            case 26: if (Is32Bit) { UnsafeMemory32.WriteRaw26(ref writer, src); } else { UnsafeMemory64.WriteRaw26(ref writer, src); } break;
            case 27: if (Is32Bit) { UnsafeMemory32.WriteRaw27(ref writer, src); } else { UnsafeMemory64.WriteRaw27(ref writer, src); } break;
            case 28: if (Is32Bit) { UnsafeMemory32.WriteRaw28(ref writer, src); } else { UnsafeMemory64.WriteRaw28(ref writer, src); } break;
            case 29: if (Is32Bit) { UnsafeMemory32.WriteRaw29(ref writer, src); } else { UnsafeMemory64.WriteRaw29(ref writer, src); } break;
            case 30: if (Is32Bit) { UnsafeMemory32.WriteRaw30(ref writer, src); } else { UnsafeMemory64.WriteRaw30(ref writer, src); } break;
            case 31: if (Is32Bit) { UnsafeMemory32.WriteRaw31(ref writer, src); } else { UnsafeMemory64.WriteRaw31(ref writer, src); } break;
            default:
                MemoryCopy(ref writer, src, length);
                break;
        }
    }

    public static void MemoryCopy(ref JsonWriter writer, byte[] src) => MemoryCopy(ref writer, src, src.Length);

    public static unsafe void MemoryCopy(ref JsonWriter writer, byte[] src, int length)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, length);
        fixed (void* dstP = &writer.Buffer[writer.Offset])
        fixed (void* srcP = &src[0])
        {
            Buffer.MemoryCopy(srcP, dstP, writer.Buffer.Length - writer.Offset, length);
        }
        writer.Offset += length;
    }
}

internal static partial class UnsafeMemory32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw1(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(byte*)pDst = *(byte*)pSrc;
        }

        writer.Offset += src.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw2(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(short*)pDst = *(short*)pSrc;
        }

        writer.Offset += src.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw3(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(byte*)pDst = *(byte*)pSrc;
            *(short*)(pDst + 1) = *(short*)(pSrc + 1);
        }

        writer.Offset += src.Length;
    }
}

internal static partial class UnsafeMemory64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw1(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(byte*)pDst = *(byte*)pSrc;
        }

        writer.Offset += src.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw2(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(short*)pDst = *(short*)pSrc;
        }

        writer.Offset += src.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw3(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(byte*)pDst = *(byte*)pSrc;
            *(short*)(pDst + 1) = *(short*)(pSrc + 1);
        }

        writer.Offset += src.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw4(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(int*)(pDst + 0) = *(int*)(pSrc + 0);
        }

        writer.Offset += src.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw5(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(int*)(pDst + 0) = *(int*)(pSrc + 0);
            *(int*)(pDst + 1) = *(int*)(pSrc + 1);
        }

        writer.Offset += src.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw6(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(int*)(pDst + 0) = *(int*)(pSrc + 0);
            *(int*)(pDst + 2) = *(int*)(pSrc + 2);
        }

        writer.Offset += src.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void WriteRaw7(ref JsonWriter writer, byte[] src)
    {
        BinaryUtil.EnsureCapacity(ref writer.Buffer, writer.Offset, src.Length);

        fixed (byte* pSrc = &src[0])
        fixed (byte* pDst = &writer.Buffer[writer.Offset])
        {
            *(int*)(pDst + 0) = *(int*)(pSrc + 0);
            *(int*)(pDst + 3) = *(int*)(pSrc + 3);
        }

        writer.Offset += src.Length;
    }
}
