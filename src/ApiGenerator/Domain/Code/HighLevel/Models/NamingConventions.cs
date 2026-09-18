/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

internal static class NamingConventions
{
    /// <summary>
    /// Matches individual "words" within an identifier that may mix snake_case, kebab-case, and
    /// camelCase/PascalCase segments (e.g. wire names like "querySetId" or "restoreUUID"). Four
    /// alternatives are tried in order at each position:
    ///   1. An acronym run immediately followed by a new capitalized word (e.g. the "HTTP" in
    ///      "HTTPServer") - a lookahead only; the following word is matched separately.
    ///   2. An optional leading capital plus one or more lowercase letters (a normal lowercase
    ///      word, or a single capitalized word like "Set" in "querySetId").
    ///   3. A bare run of uppercase letters (a trailing acronym, e.g. "UUID" in "restoreUUID",
    ///      or a fully-uppercase enum value like "ONNX").
    ///   4. A run of digits.
    /// Underscores and hyphens aren't matched by any alternative, so they act as separators
    /// without needing to be split on explicitly.
    /// </summary>
    private static readonly Regex WordBoundary =
        new(@"[A-Z]+(?=[A-Z][a-z])|[A-Z]?[a-z]+|[A-Z]+|[0-9]+", RegexOptions.Compiled);

    /// <summary>
    /// Converts a wire name - snake_case, kebab-case, camelCase, PascalCase, or any mix (e.g.
    /// "querySetId", "data_type", "TORCH_SCRIPT") - to a PascalCase C# identifier. Each
    /// recognized word segment has its first letter capitalized and the rest lower-cased. The
    /// lower-casing is intentional: it normalizes UPPER_SNAKE_CASE wire enum values (such as
    /// "TORCH_SCRIPT" or "ONNX") into idiomatic PascalCase ("TorchScript", "Onnx") rather than
    /// leaving them as "TORCHSCRIPT"/"ONNX", while still splitting camelCase wire names at word
    /// boundaries ("querySetId" -> "QuerySetId") instead of collapsing them ("Querysetid").
    /// </summary>
    public static string ToPascal(string name)
    {
        var clean = name.TrimStart('_');
        return string.Concat(WordBoundary.Matches(clean)
            .Select(m => char.ToUpperInvariant(m.Value[0]) + m.Value.Substring(1).ToLowerInvariant()));
    }

    /// <summary>
    /// Converts an operation snake_name to PascalCase (splits only on underscores).
    /// </summary>
    public static string OperationToPascal(string snake) =>
        string.Concat(snake.TrimStart('_').Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
}
