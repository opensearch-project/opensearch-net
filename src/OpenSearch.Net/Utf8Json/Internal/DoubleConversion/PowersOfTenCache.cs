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

namespace OpenSearch.Net.Utf8Json.Internal.DoubleConversion;

using int16_t = Int16;
using uint64_t = UInt64;

// https://github.com/google/double-conversion/blob/master/double-conversion/cached-powers.h
// https://github.com/google/double-conversion/blob/master/double-conversion/cached-powers.cc

internal readonly struct CachedPower
{
    public readonly ulong Significand;
    public readonly short BinaryExponent;
    public readonly short DecimalExponent;

    public CachedPower(ulong significand, short binaryExponent, short decimalExponent)
    {
        Significand = significand;
        BinaryExponent = binaryExponent;
        DecimalExponent = decimalExponent;
    }
};

internal static class PowersOfTenCache
{
    private static readonly CachedPower[] KCachedPowers = {
        new CachedPower (0xfa8fd5a0081c0288, -1220, -348),
        new CachedPower (0xbaaee17fa23ebf76, -1193, -340),
        new CachedPower (0x8b16fb203055ac76, -1166, -332),
        new CachedPower (0xcf42894a5dce35ea, -1140, -324),
        new CachedPower (0x9a6bb0aa55653b2d, -1113, -316),
        new CachedPower (0xe61acf033d1a45df, -1087, -308),
        new CachedPower (0xab70fe17c79ac6ca, -1060, -300),
        new CachedPower (0xff77b1fcbebcdc4f, -1034, -292),
        new CachedPower (0xbe5691ef416bd60c, -1007, -284),
        new CachedPower (0x8dd01fad907ffc3c, -980, -276),
        new CachedPower (0xd3515c2831559a83, -954, -268),
        new CachedPower (0x9d71ac8fada6c9b5, -927, -260),
        new CachedPower (0xea9c227723ee8bcb, -901, -252),
        new CachedPower (0xaecc49914078536d, -874, -244),
        new CachedPower (0x823c12795db6ce57, -847, -236),
        new CachedPower (0xc21094364dfb5637, -821, -228),
        new CachedPower (0x9096ea6f3848984f, -794, -220),
        new CachedPower (0xd77485cb25823ac7, -768, -212),
        new CachedPower (0xa086cfcd97bf97f4, -741, -204),
        new CachedPower (0xef340a98172aace5, -715, -196),
        new CachedPower (0xb23867fb2a35b28e, -688, -188),
        new CachedPower (0x84c8d4dfd2c63f3b, -661, -180),
        new CachedPower (0xc5dd44271ad3cdba, -635, -172),
        new CachedPower (0x936b9fcebb25c996, -608, -164),
        new CachedPower (0xdbac6c247d62a584, -582, -156),
        new CachedPower (0xa3ab66580d5fdaf6, -555, -148),
        new CachedPower (0xf3e2f893dec3f126, -529, -140),
        new CachedPower (0xb5b5ada8aaff80b8, -502, -132),
        new CachedPower (0x87625f056c7c4a8b, -475, -124),
        new CachedPower (0xc9bcff6034c13053, -449, -116),
        new CachedPower (0x964e858c91ba2655, -422, -108),
        new CachedPower (0xdff9772470297ebd, -396, -100),
        new CachedPower (0xa6dfbd9fb8e5b88f, -369, -92),
        new CachedPower (0xf8a95fcf88747d94, -343, -84),
        new CachedPower (0xb94470938fa89bcf, -316, -76),
        new CachedPower (0x8a08f0f8bf0f156b, -289, -68),
        new CachedPower (0xcdb02555653131b6, -263, -60),
        new CachedPower (0x993fe2c6d07b7fac, -236, -52),
        new CachedPower (0xe45c10c42a2b3b06, -210, -44),
        new CachedPower (0xaa242499697392d3, -183, -36),
        new CachedPower (0xfd87b5f28300ca0e, -157, -28),
        new CachedPower (0xbce5086492111aeb, -130, -20),
        new CachedPower (0x8cbccc096f5088cc, -103, -12),
        new CachedPower (0xd1b71758e219652c, -77, -4),
        new CachedPower (0x9c40000000000000, -50, 4),
        new CachedPower (0xe8d4a51000000000, -24, 12),
        new CachedPower (0xad78ebc5ac620000, 3, 20),
        new CachedPower (0x813f3978f8940984, 30, 28),
        new CachedPower (0xc097ce7bc90715b3, 56, 36),
        new CachedPower (0x8f7e32ce7bea5c70, 83, 44),
        new CachedPower (0xd5d238a4abe98068, 109, 52),
        new CachedPower (0x9f4f2726179a2245, 136, 60),
        new CachedPower (0xed63a231d4c4fb27, 162, 68),
        new CachedPower (0xb0de65388cc8ada8, 189, 76),
        new CachedPower (0x83c7088e1aab65db, 216, 84),
        new CachedPower (0xc45d1df942711d9a, 242, 92),
        new CachedPower (0x924d692ca61be758, 269, 100),
        new CachedPower (0xda01ee641a708dea, 295, 108),
        new CachedPower (0xa26da3999aef774a, 322, 116),
        new CachedPower (0xf209787bb47d6b85, 348, 124),
        new CachedPower (0xb454e4a179dd1877, 375, 132),
        new CachedPower (0x865b86925b9bc5c2, 402, 140),
        new CachedPower (0xc83553c5c8965d3d, 428, 148),
        new CachedPower (0x952ab45cfa97a0b3, 455, 156),
        new CachedPower (0xde469fbd99a05fe3, 481, 164),
        new CachedPower (0xa59bc234db398c25, 508, 172),
        new CachedPower (0xf6c69a72a3989f5c, 534, 180),
        new CachedPower (0xb7dcbf5354e9bece, 561, 188),
        new CachedPower (0x88fcf317f22241e2, 588, 196),
        new CachedPower (0xcc20ce9bd35c78a5, 614, 204),
        new CachedPower (0x98165af37b2153df, 641, 212),
        new CachedPower (0xe2a0b5dc971f303a, 667, 220),
        new CachedPower (0xa8d9d1535ce3b396, 694, 228),
        new CachedPower (0xfb9b7cd9a4a7443c, 720, 236),
        new CachedPower (0xbb764c4ca7a44410, 747, 244),
        new CachedPower (0x8bab8eefb6409c1a, 774, 252),
        new CachedPower (0xd01fef10a657842c, 800, 260),
        new CachedPower (0x9b10a4e5e9913129, 827, 268),
        new CachedPower (0xe7109bfba19c0c9d, 853, 276),
        new CachedPower (0xac2820d9623bf429, 880, 284),
        new CachedPower (0x80444b5e7aa7cf85, 907, 292),
        new CachedPower (0xbf21e44003acdd2d, 933, 300),
        new CachedPower (0x8e679c2f5e44ff8f, 960, 308),
        new CachedPower (0xd433179d9c8cb841, 986, 316),
        new CachedPower (0x9e19db92b4e31ba9, 1013, 324),
        new CachedPower (0xeb96bf6ebadf77d9, 1039, 332),
        new CachedPower (0xaf87023b9bf0ee6b, 1066, 340),
    };

    public const int KCachedPowersOffset = 348;  // -1 * the first decimal_exponent.
    public const double KD1Log210 = 0.30102999566398114;  //  1 / lg(10)
                                                          // Difference between the decimal exponents in the table above.
    public const int KDecimalExponentDistance = 8;
    public const int KMinDecimalExponent = -348;
    public const int KMaxDecimalExponent = 340;

    public static void GetCachedPowerForBinaryExponentRange(
        int minExponent,
        int maxExponent,
        out DiyFp power,
        out int decimalExponent)
    {
        var kQ = DiyFp.KSignificandSize;
        var k = Math.Ceiling((minExponent + kQ - 1) * KD1Log210);
        var foo = KCachedPowersOffset;
        var index = (foo + (int)(k) - 1) / KDecimalExponentDistance + 1;

        var cachedPower = KCachedPowers[index];
        // (void)max_exponent;  // Mark variable as used.
        decimalExponent = cachedPower.DecimalExponent;
        power = new DiyFp(cachedPower.Significand, cachedPower.BinaryExponent);
    }

    public static void GetCachedPowerForDecimalExponent(int requestedExponent, out DiyFp power, out int foundExponent)
    {
        var index = (requestedExponent + KCachedPowersOffset) / KDecimalExponentDistance;
        var cachedPower = KCachedPowers[index];
        power = new DiyFp(cachedPower.Significand, cachedPower.BinaryExponent);
        foundExponent = cachedPower.DecimalExponent;
    }
}
