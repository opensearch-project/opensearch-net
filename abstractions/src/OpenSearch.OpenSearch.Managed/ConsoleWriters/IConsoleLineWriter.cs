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
using ProcNet.Std;

namespace OpenSearch.OpenSearch.Managed.ConsoleWriters;

public static class LineWriterExtensions
{
    public static void WriteDiagnostic(this IConsoleLineHandler writer, string message) =>
        writer.Handle(Info(message));

    public static void WriteDiagnostic(this IConsoleLineHandler writer, string message, string node) =>
        writer?.Handle(Info(node != null ? $"[{node}] {message}" : message));

    public static void WriteError(this IConsoleLineHandler writer, string message) => writer.Handle(Error(message));

    public static void WriteError(this IConsoleLineHandler writer, string message, string node) =>
        writer?.Handle(Error(node != null ? $"[{node}] {message}" : message));

    private static string Format(bool error, string message) =>
        $"[{DateTime.UtcNow:yyyy-MM-ddThh:mm:ss,fff}][{(error ? "ERROR" : "INFO ")}][Managed OpenSearch\t] {message}";

    private static LineOut Info(string message) => ConsoleOut.Out(Format(false, message));
    private static LineOut Error(string message) => ConsoleOut.ErrorOut(Format(true, message));
}
