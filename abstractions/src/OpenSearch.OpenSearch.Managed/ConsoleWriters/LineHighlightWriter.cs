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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProcNet.Std;

namespace OpenSearch.OpenSearch.Managed.ConsoleWriters
{
    public class LineHighlightWriter : IConsoleLineHandler
    {
        private readonly LineOutParser _lineOutParser;

        private static readonly ConsoleColor[] AvailableNodeColors =
        {
            ConsoleColor.DarkGreen, ConsoleColor.DarkBlue, ConsoleColor.DarkRed, ConsoleColor.DarkCyan,
            ConsoleColor.DarkYellow, ConsoleColor.Blue,
        };

        private static readonly object Lock = new object();

        private static readonly char[] Anchors = { '[', ']', '{', '}' };

        private readonly Dictionary<string, ConsoleColor> _nodeColors;

        public LineHighlightWriter()
        {
        }

        public LineHighlightWriter(IList<string> nodes, LineOutParser lineOutParser)
        {
            _lineOutParser = lineOutParser ?? throw new NullReferenceException(nameof(lineOutParser));
            if (nodes == null) throw new NullReferenceException(nameof(nodes));

            var colors = new Dictionary<string, ConsoleColor>();
            for (var i = 0; i < nodes.Count; i++)
            {
                var color = i % AvailableNodeColors.Length;
                colors.Add(nodes[i], AvailableNodeColors[color]);
            }

            _nodeColors = colors;
        }

        public void Handle(Exception e)
        {
            lock (Lock)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Error.WriteLine(e);
                Console.ResetColor();
            }
        }

        public void Handle(LineOut lineOut)
        {
            var parsed = _lineOutParser.TryParse(lineOut.Line, out var date, out var level, out var section,
                out var node, out var message, out _);
            if (parsed) Write(lineOut.Error, date, level, section, node, message, NodeColor);
            else if (ExceptionLineParser.TryParseCause(lineOut.Line, out var cause, out var causeMessage))
                WriteCausedBy(lineOut.Error, cause, causeMessage);

            else if (ExceptionLineParser.TryParseStackTrace(lineOut.Line, out var at, out var method, out var file,
                out var jar))
                WriteStackTraceLine(lineOut.Error, at, method, file, jar);

            else Write(lineOut.Error, lineOut.Line);
        }

        private ConsoleColor NodeColor(string node) =>
            _nodeColors != null && _nodeColors.TryGetValue(node, out var color) ? color : AvailableNodeColors[0];

        private static void Write(bool error, string date, string level, string section, string node, string message,
            Func<string, ConsoleColor> getNodeColor = null)
        {
            lock (Lock)
            {
                var w = error ? Console.Error : Console.Out;
                WriteBlock(w, ConsoleColor.DarkGray, date);
                WriteBlock(w, LevelColor(level), level, 5);

                if (!string.IsNullOrWhiteSpace(section))
                {
                    WriteBlock(w, ConsoleColor.DarkCyan, section, 25);
                    WriteSpace(w);
                }

                WriteBlock(w, getNodeColor?.Invoke(node) ?? ConsoleColor.DarkGreen, node);
                WriteSpace(w);

                var messageColor = error || level == "ERROR" ? ConsoleColor.Red : ConsoleColor.White;
                WriteMessage(w, messageColor, message);

                Console.ResetColor();
                w.Flush();
            }
        }

        private static void WriteCausedBy(bool error, string cause, string causeMessage)
        {
            lock (Lock)
            {
                var w = error ? Console.Error : Console.Out;
                Write(w, ConsoleColor.DarkRed, cause);
                WriteSpace(w);
                Write(w, ConsoleColor.Red, causeMessage);
                w.WriteLine();
                Console.ResetColor();
                w.Flush();
            }
        }

        private static void WriteStackTraceLine(bool error, string at, string method, string file, string jar)
        {
            lock (Lock)
            {
                var w = error ? Console.Error : Console.Out;
                Write(w, ConsoleColor.DarkGray, at);
                Write(w, ConsoleColor.DarkBlue, method);
                Write(w, ConsoleColor.DarkGray, "(");
                Write(w, ConsoleColor.Blue, file);
                Write(w, ConsoleColor.DarkGray, ")");
                WriteSpace(w);
                Write(w, ConsoleColor.Gray, jar);
                w.WriteLine();

                Console.ResetColor();
                w.Flush();
            }
        }

        private static void Write(bool error, string message)
        {
            lock (Lock)
            {
                var w = error ? Console.Error : Console.Out;
                var messageColor = error ? ConsoleColor.Red : ConsoleColor.White;
                WriteMessage(w, messageColor, message);
                Console.ResetColor();
                w.Flush();
            }
        }

        private static ConsoleColor LevelColor(string level)
        {
            switch (level ?? "")
            {
                case "WARN": return ConsoleColor.Yellow;
                case "FATAL":
                case "ERROR":
                    return ConsoleColor.Red;
                case "DEBUG":
                case "TRACE":
                    return ConsoleColor.DarkGray;
                default:
                    return ConsoleColor.Cyan;
            }
        }

        private static IEnumerable<string> Parts(string s)
        {
            int start = 0, index;
            while ((index = s.IndexOfAny(Anchors, start)) != -1)
            {
                if (index - start > 0)
                    yield return s.Substring(start, index - start);

                yield return s.Substring(index, 1);
                start = index + 1;
            }

            if (start < s.Length)
                yield return s.Substring(start);
        }

        private static void WriteMessage(TextWriter w, ConsoleColor color, string message)
        {
            var insideSquareBracket = 0;
            var insideCurlyBracket = 0;
            foreach (var p in Parts(message))
            {
                if (p.Length == 0) continue;
                if (p[0] == '[') insideSquareBracket++;
                else if (p[0] == ']') insideSquareBracket--;
                else if (p[0] == '{') insideCurlyBracket++;
                else if (p[0] == '}') insideCurlyBracket--;

                if (Anchors.Contains(p[0])) Console.ForegroundColor = ConsoleColor.DarkGray;
                else if (insideSquareBracket > 0) Console.ForegroundColor = ConsoleColor.Yellow;
                else if (insideCurlyBracket > 0) Console.ForegroundColor = ConsoleColor.Blue;
                else Console.ForegroundColor = color;

                w.Write(p);
            }

            Console.ResetColor();
            w.WriteLine();
        }

        private static void WriteSpace(TextWriter w) => w.Write(" ");

        private static void WriteBlock(TextWriter w, ConsoleColor color, string block, int? pad = null)
        {
            if (string.IsNullOrEmpty(block)) return;
            var b = pad != null ? block.PadRight(pad.Value) : block;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            w.Write("[");
            Console.ForegroundColor = color;
            w.Write(b);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            w.Write("]");
        }

        private static void Write(TextWriter w, ConsoleColor color, string block, int? pad = null)
        {
            var b = pad != null ? block.PadRight(pad.Value) : block;
            var original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            w.Write(b);
            Console.ForegroundColor = original;
        }
    }
}
