/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiGenerator.Generator
{
    /// <summary>
    /// Line-wrapping helpers for templates. <c>dotnet format</c> fixes indentation but never wraps long lines, so
    /// templates wrap long declarations themselves. Continuation lines are emitted one indentation level deeper
    /// than the declaration; <c>dotnet format</c> preserves that relative indentation.
    /// </summary>
    public static class CodeLayout
    {
        private const int MaxLineLength = 120;
        private const string IndentUnit = "    ";

        /// <summary>
        /// Renders <c>head(parameters)</c>, putting each parameter on its own line when the declaration would
        /// exceed <see cref="MaxLineLength"/>.
        /// </summary>
        /// <param name="head">Everything before the opening parenthesis, e.g. <c>public TResponse Get&lt;TResponse&gt;</c></param>
        /// <param name="parameters">The comma separated parameter list</param>
        /// <param name="indent">The column at which the template emits <paramref name="head" /></param>
        /// <param name="offset">Width of anything the template emits on the same line before <paramref name="head" /></param>
        public static string ParameterList(string head, string parameters, int indent, int offset = 0)
        {
            var singleLine = $"{head}({parameters})";
            var split = SplitTopLevel(parameters);
            if (indent + offset + singleLine.Length <= MaxLineLength || split.Count == 0)
                return singleLine;

            var baseIndent = new string(' ', indent);
            var sb = new StringBuilder(head).Append('(');
            for (var i = 0; i < split.Count; i++)
            {
                sb.Append('\n').Append(baseIndent).Append(IndentUnit).Append(split[i]);
                if (i < split.Count - 1)
                    sb.Append(',');
            }
            return sb.Append('\n').Append(baseIndent).Append(')').ToString();
        }

        /// <summary>
        /// Renders <c>head : bases</c>, moving the base list to the next line when the declaration would exceed
        /// <see cref="MaxLineLength"/>.
        /// </summary>
        public static string BaseList(string head, string bases, int indent)
        {
            var singleLine = $"{head} : {bases}";
            if (indent + singleLine.Length <= MaxLineLength)
                return singleLine;

            var continuation = "\n" + new string(' ', indent) + IndentUnit;
            return $"{head}{continuation}: {string.Join("," + continuation + IndentUnit, SplitTopLevel(bases))}";
        }

        /// <summary>
        /// Moves the body of an expression-bodied member (everything after the first <c> =&gt; </c>) to the next
        /// line when the member would exceed <see cref="MaxLineLength"/>.
        /// </summary>
        public static string ExpressionBody(string member, int indent)
        {
            var arrow = member.IndexOf(" => ", System.StringComparison.Ordinal);
            if (indent + member.Length <= MaxLineLength || arrow < 0)
                return member;

            return $"{member[..arrow]} =>\n{new string(' ', indent)}{IndentUnit}{member[(arrow + 4)..]}";
        }

        /// <summary>Splits on commas that are not nested inside <c>&lt;&gt;</c>, <c>()</c> or <c>[]</c>.</summary>
        private static List<string> SplitTopLevel(string list)
        {
            var parts = new List<string>();
            var depth = 0;
            var start = 0;
            for (var i = 0; i < list.Length; i++)
            {
                switch (list[i])
                {
                    case '<' or '(' or '[': depth++; break;
                    case '>' or ')' or ']': depth--; break;
                    case ',' when depth == 0:
                        parts.Add(list[start..i].Trim());
                        start = i + 1;
                        break;
                }
            }
            parts.Add(list[start..].Trim());
            return parts.Where(p => p.Length > 0).ToList();
        }
    }
}
