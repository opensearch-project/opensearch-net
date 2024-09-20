/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Runtime.Serialization;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.ClientConcepts.Enums;

public class EnumParsingTests
{
    [U]
    public void CanParseRegularEnum() => KnownEnums.Parse<RegularEnum>("first").Should().Be(RegularEnum.First);

    [U]
    public void CanParseEnumWithAttribute() => KnownEnums.Parse<EnumWithAttribute>("second_value").Should().Be(EnumWithAttribute.Second);

    [U]
    public void CanParseFlagsEnum() => KnownEnums.Parse<FlagsEnum>("first,third").Should().Be(FlagsEnum.First | FlagsEnum.Third);

    [U]
    public void CanParseFlagsEnumWithAttribute() => KnownEnums.Parse<FlagsEnumWithAttribute>("first_value,third_value").Should().Be(FlagsEnumWithAttribute.First | FlagsEnumWithAttribute.Third);

private enum RegularEnum
    {
        First,
        Second
    }

    private enum EnumWithAttribute
    {
        [EnumMember(Value = "first_value")]
        First,
        [EnumMember(Value = "second_value")]
        Second
    }

    [Flags]
    private enum FlagsEnum
    {
        First = 1 << 0,
        Second = 1 << 1,
        Third = 1 << 2
    }

    [Flags]
    private enum FlagsEnumWithAttribute
    {
        [EnumMember(Value = "first_value")]
        First = 1 << 0,
        [EnumMember(Value = "second_value")]
        Second = 1 << 1,
        [EnumMember(Value = "third_value")]
        Third = 1 << 2
    }
}
