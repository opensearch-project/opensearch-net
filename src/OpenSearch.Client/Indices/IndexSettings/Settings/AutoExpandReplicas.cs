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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(AutoExpandReplicasFormatter))]
public class AutoExpandReplicas
{
    private const string AllMaxReplicas = "all";
    private Union<int?, string> _maxReplicas;
    private int? _minReplicas;

    public static AutoExpandReplicas Disabled { get; } = new AutoExpandReplicas();

    /// <summary>
    /// Whether auto expand replicas is enabled
    /// </summary>
    public bool Enabled { get; private set; }

    /// <summary>
    /// The upper bound of replicas. Can be an integer value or a string value of "all"
    /// </summary>
    public Union<int?, string> MaxReplicas
    {
        get => _maxReplicas;
        private set
        {
            if (value == null && _minReplicas == null) Enabled = false;
            else Enabled = true;
            _maxReplicas = value;
        }
    }

    /// <summary>
    /// The lower bound of replicas
    /// </summary>
    public int? MinReplicas
    {
        get => _minReplicas;
        private set
        {
            if (value == null && _maxReplicas == null) Enabled = false;
            else Enabled = true;
            _minReplicas = value;
        }
    }

    /// <summary>
    /// Creates an <see cref="AutoExpandReplicas" /> with the specified lower and upper bounds of replicas
    /// </summary>
    public static AutoExpandReplicas Create(int minReplicas, int maxReplicas)
    {
        if (minReplicas < 0)
            throw new ArgumentException("minReplicas must be greater than or equal to 0", nameof(minReplicas));

        if (maxReplicas < 0)
            throw new ArgumentException("maxReplicas must be greater than or equal to 0", nameof(minReplicas));

        if (minReplicas > maxReplicas)
            throw new ArgumentException("minReplicas must be less than or equal to maxReplicas", nameof(minReplicas));

        return new AutoExpandReplicas
        {
            Enabled = true,
            MinReplicas = minReplicas,
            MaxReplicas = maxReplicas
        };
    }

    /// <summary>
    /// Creates an <see cref="AutoExpandReplicas" /> with the specified lower bound of replicas and an
    /// "all" upper bound of replicas
    /// </summary>
    public static AutoExpandReplicas Create(int minReplicas)
    {
        if (minReplicas < 0)
            throw new ArgumentException("minReplicas must be greater than or equal to 0", nameof(minReplicas));

        return new AutoExpandReplicas
        {
            Enabled = true,
            MinReplicas = minReplicas,
            MaxReplicas = AllMaxReplicas
        };
    }

    /// <summary>
    /// Creates an <see cref="AutoExpandReplicas" /> with the specified lower and upper bounds of replicas.
    /// </summary>
    /// <example>0-5</example>
    /// <example>0-all</example>
    public static AutoExpandReplicas Create(string value)
    {
        if (value.IsNullOrEmpty())
            throw new ArgumentException("cannot be null or empty", nameof(value));

        if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
            return Disabled;

        var expandReplicaParts = value.Split('-');
        if (expandReplicaParts.Length != 2)
            throw new ArgumentException("must contain a 'from' and 'to' value", nameof(value));

        if (!int.TryParse(expandReplicaParts[0], out var minReplicas))
            throw new FormatException("minReplicas must be an integer");

        var maxReplicas = 0;
        var parsedMaxReplicas = false;
        var allMaxReplicas = expandReplicaParts[1] == AllMaxReplicas;

        if (!allMaxReplicas)
            parsedMaxReplicas = int.TryParse(expandReplicaParts[1], out maxReplicas);

        if (!parsedMaxReplicas && !allMaxReplicas)
            throw new FormatException("minReplicas must be an integer or 'all'");

        return parsedMaxReplicas
            ? Create(minReplicas, maxReplicas)
            : Create(minReplicas);
    }

    public static implicit operator AutoExpandReplicas(string value) =>
        value.IsNullOrEmpty() ? null : Create(value);

    public override string ToString()
    {
        if (!Enabled) return "false";

        var maxReplicas = MaxReplicas.Match(i => i.ToString(), s => s);
        return string.Join("-", MinReplicas, maxReplicas);
    }
}

internal class AutoExpandReplicasFormatter : IJsonFormatter<AutoExpandReplicas>
{
    public AutoExpandReplicas Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();

        if (token == JsonToken.False)
            return AutoExpandReplicas.Disabled;
        if (token == JsonToken.String)
            return AutoExpandReplicas.Create(reader.ReadString());

        throw new Exception($"Cannot deserialize {typeof(AutoExpandReplicas)} from {token}");
    }

    public void Serialize(ref JsonWriter writer, AutoExpandReplicas value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null || !value.Enabled)
        {
            writer.WriteBoolean(false);
            return;
        }

        writer.WriteString(value.ToString());
    }
}
