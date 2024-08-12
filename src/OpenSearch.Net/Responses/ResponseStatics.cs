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
using System.Linq;
using System.Text;
using OpenSearch.Net.Extensions;

namespace OpenSearch.Net;

public static class ResponseStatics
{
    private static readonly string RequestAlreadyCaptured =
        "<Request stream not captured or already read to completion by serializer. Set DisableDirectStreaming() on ConnectionSettings to force it to be set on the response.>";

    private static readonly string ResponseAlreadyCaptured =
        "<Response stream not captured or already read to completion by serializer. Set DisableDirectStreaming() on ConnectionSettings to force it to be set on the response.>";

    public static string DebugInformationBuilder(IApiCallDetails r, StringBuilder sb)
    {
        if (r.DeprecationWarnings.HasAny())
        {
            sb.AppendLine($"# Server indicated deprecations:");
            foreach (var deprecation in r.DeprecationWarnings)
                sb.AppendLine($"- {deprecation}");
        }
        sb.AppendLine($"# Audit trail of this API call:");
        var auditTrail = (r.AuditTrail ?? Enumerable.Empty<Audit>()).ToList();
        DebugAuditTrail(auditTrail, sb);
        if (r.OriginalException != null) sb.AppendLine($"# OriginalException: {r.OriginalException}");
        DebugAuditTrailExceptions(auditTrail, sb);

        var response = r.ResponseBodyInBytes?.Utf8String() ?? ResponseAlreadyCaptured;
        var request = r.RequestBodyInBytes?.Utf8String() ?? RequestAlreadyCaptured;
        sb.AppendLine($"# Request:{Environment.NewLine}{request}");
        sb.AppendLine($"# Response:{Environment.NewLine}{response}");

        if (r.TcpStats != null)
        {
            sb.AppendLine("# TCP states:");
            foreach (var stat in r.TcpStats)
            {
                sb.Append("  ");
                sb.Append(stat.Key);
                sb.Append(": ");
                sb.AppendLine($"{stat.Value}");
            }
            sb.AppendLine();
        }

        if (r.ThreadPoolStats != null)
        {
            sb.AppendLine("# ThreadPool statistics:");
            foreach (var stat in r.ThreadPoolStats)
            {
                sb.Append("  ");
                sb.Append(stat.Key);
                sb.AppendLine(": ");
                sb.Append("    Busy: ");
                sb.AppendLine($"{stat.Value.Busy}");
                sb.Append("    Free: ");
                sb.AppendLine($"{stat.Value.Free}");
                sb.Append("    Min: ");
                sb.AppendLine($"{stat.Value.Min}");
                sb.Append("    Max: ");
                sb.AppendLine($"{stat.Value.Max}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static void DebugAuditTrailExceptions(List<Audit> auditTrail, StringBuilder sb)
    {
        if (auditTrail == null) return;

        var auditExceptions = auditTrail.Select((audit, i) => new { audit, i }).Where(a => a.audit.Exception != null);
        foreach (var a in auditExceptions)
            sb.AppendLine($"# Audit exception in step {a.i + 1} {a.audit.Event.GetStringValue()}:{Environment.NewLine}{a.audit.Exception}");
    }

    public static void DebugAuditTrail(List<Audit> auditTrail, StringBuilder sb)
    {
        if (auditTrail == null) return;

        foreach (var a in auditTrail.Select((a, i) => new { a, i }))
        {
            var audit = a.a;
            sb.Append($" - [{a.i + 1}] {audit.Event.GetStringValue()}:");

            AuditNodeUrl(sb, audit);

            if (audit.Exception != null) sb.Append($" Exception: {audit.Exception.GetType().Name}");
            if (audit.Ended == default)
                sb.AppendLine();
            else sb.AppendLine($" Took: {(audit.Ended - audit.Started).ToString()}");
        }
    }

    private static void AuditNodeUrl(StringBuilder sb, Audit audit)
    {
        var uri = audit.Node?.Uri;
        if (uri == null) return;

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var builder = new UriBuilder(uri)
            {
                Password = "redacted"
            };
            uri = builder.Uri;
        }
        sb.Append($" Node: {uri}");
    }
}
