/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

internal static class NamingConventions
{
    /// <summary>
    /// Converts a snake_case or kebab-case wire name to PascalCase C# identifier.
    /// Splits on underscores and hyphens, capitalizes each segment.
    /// </summary>
    public static string ToPascal(string name)
    {
        var clean = name.TrimStart('_');
        return string.Concat(clean.Split('_', '-')
            .Where(p => p.Length > 0)
            .Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
    }

    /// <summary>
    /// Converts an operation snake_name to PascalCase (splits only on underscores).
    /// </summary>
    public static string OperationToPascal(string snake) =>
        string.Concat(snake.TrimStart('_').Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
}
