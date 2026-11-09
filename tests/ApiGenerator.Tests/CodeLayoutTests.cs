/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using ApiGenerator.Generator;
using ApiGenerator.Generator.Razor;
using Xunit;

namespace ApiGenerator.Tests;

/// <summary>
/// Generated code is formatted with <c>dotnet format</c>, which fixes indentation but never wraps lines or removes
/// blank lines, so templates rely on these helpers for layout.
/// </summary>
public class CodeLayoutTests
{
    [Fact]
    public void ShortParameterListStaysOnOneLine() =>
        Assert.Equal("public void Foo(string a, int b)", CodeLayout.ParameterList("public void Foo", "string a, int b", 8));

    [Fact]
    public void LongParameterListIsWrappedOnTopLevelCommas()
    {
        var parameters = "Func<SomeVeryLongDescriptorName, ISomeVeryLongRequestInterface> selector = null, CancellationToken ct = default";

        var wrapped = CodeLayout.ParameterList("public Task<SomeResponse> SomeMethodAsync", parameters, 8);

        Assert.Equal(
            "public Task<SomeResponse> SomeMethodAsync(\n"
            + "            Func<SomeVeryLongDescriptorName, ISomeVeryLongRequestInterface> selector = null,\n"
            + "            CancellationToken ct = default\n"
            + "        )",
            wrapped);
    }

    [Fact]
    public void OffsetCountsTowardsLineLength()
    {
        var parameters = new string('a', 50) + " x";

        Assert.DoesNotContain("\n", CodeLayout.ParameterList("Response Method", parameters, 0));
        Assert.Contains("\n", CodeLayout.ParameterList("Response Method", parameters, 0, offset: 60));
    }

    [Fact]
    public void LongBaseListIsWrapped()
    {
        var wrapped = CodeLayout.BaseList(
            "public partial class SomeVeryLongDescriptorName",
            "RequestDescriptorBase<SomeVeryLongDescriptorName, SomeRequestParameters, ISomeRequest>, ISomeRequest",
            4);

        Assert.Equal(
            "public partial class SomeVeryLongDescriptorName\n"
            + "        : RequestDescriptorBase<SomeVeryLongDescriptorName, SomeRequestParameters, ISomeRequest>,\n"
            + "            ISomeRequest",
            wrapped);
    }

    [Fact]
    public void LongExpressionBodyMovesToNextLineAtFirstArrow()
    {
        var member = "public SomeVeryLongDescriptorName Index<TOther>() where TOther : class => "
            + "Assign(typeof(TOther), (a, v) => a.RouteValues.Optional(\"index\", (Indices)v));";

        Assert.Equal(
            "public SomeVeryLongDescriptorName Index<TOther>() where TOther : class =>\n"
            + "            Assign(typeof(TOther), (a, v) => a.RouteValues.Optional(\"index\", (Indices)v));",
            CodeLayout.ExpressionBody(member, 8));
    }

    [Fact]
    public void NormalizeBlankLinesCollapsesRunsAndTrimsAroundBraces()
    {
        const string input = "namespace X\r\n{\n\n    class A { }   \n\n\n\n    class B { }\n\n}\n\n";

        Assert.Equal("namespace X\n{\n    class A { }\n\n    class B { }\n}\n", RazorGeneratorBase.NormalizeBlankLines(input));
    }
}
