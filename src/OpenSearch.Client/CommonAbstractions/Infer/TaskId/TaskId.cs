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
using System.Diagnostics;
using System.Globalization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[DebuggerDisplay("{DebugDisplay,nq}")]
[JsonFormatter(typeof(TaskIdFormatter))]
public class TaskId : IUrlParameter, IEquatable<TaskId>
{
    /// <summary>
    /// A task id exists in the form [node_id]:[task_id]
    /// </summary>
    /// <param name="taskId">the task identifier</param>
    public TaskId(string taskId)
    {
        if (string.IsNullOrWhiteSpace(taskId))
            throw new ArgumentException("TaskId can not be an empty string", nameof(taskId));

        var tokens = taskId.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length != 2)
            throw new ArgumentException($"TaskId:{taskId} not in expected format of <node_id>:<task_id>", nameof(taskId));

        NodeId = tokens[0];
        FullyQualifiedId = taskId;

        if (!long.TryParse(tokens[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var t) || t < -1 || t == 0)
            throw new ArgumentException($"TaskId task component:{tokens[1]} could not be parsed to long or is out of range", nameof(taskId));

        TaskNumber = t;
    }

    public string FullyQualifiedId { get; }
    public string NodeId { get; }
    public long TaskNumber { get; }

    private string DebugDisplay => FullyQualifiedId;

    public bool Equals(TaskId other) => EqualsString(other?.FullyQualifiedId);

    public string GetString(IConnectionConfigurationValues settings) => FullyQualifiedId;

    public override string ToString() => FullyQualifiedId;

    public static implicit operator TaskId(string taskId) => taskId.IsNullOrEmpty() ? null : new TaskId(taskId);

    public static bool operator ==(TaskId left, TaskId right) => Equals(left, right);

    public static bool operator !=(TaskId left, TaskId right) => !Equals(left, right);

    public override bool Equals(object obj) =>
        obj != null && obj is string s ? EqualsString(s) : obj is TaskId i && EqualsString(i.FullyQualifiedId);

    private bool EqualsString(string other) => !other.IsNullOrEmpty() && other == FullyQualifiedId;

    public override int GetHashCode()
    {
        unchecked
        {
            return (NodeId.GetHashCode() * 397) ^ TaskNumber.GetHashCode();
        }
    }
}

internal class TaskIdFormatter : IJsonFormatter<TaskId>, IObjectPropertyNameFormatter<TaskId>
{
    public TaskId Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.GetCurrentJsonToken() == JsonToken.String)
            return new TaskId(reader.ReadString());

        reader.ReadNextBlock();
        return null;
    }

    public void Serialize(ref JsonWriter writer, TaskId value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteString(value.ToString());
    }

    public TaskId DeserializeFromPropertyName(ref JsonReader reader, IJsonFormatterResolver formatterResolver) =>
        Deserialize(ref reader, formatterResolver);

    public void SerializeToPropertyName(ref JsonWriter writer, TaskId value, IJsonFormatterResolver formatterResolver) =>
        Serialize(ref writer, value, formatterResolver);
}
