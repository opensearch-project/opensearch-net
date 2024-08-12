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
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A snapshot repository that can be used as an alternative read-only way to access data created by the <see cref="IFileSystemRepository"/>.
/// The URL specified in the url parameter should point to the root of the shared filesystem repository.
/// </summary>
public interface IReadOnlyUrlRepository : IRepository<IReadOnlyUrlRepositorySettings> { }

/// <inheritdoc />
public class ReadOnlyUrlRepository : IReadOnlyUrlRepository
{
    public ReadOnlyUrlRepository(ReadOnlyUrlRepositorySettings settings) => Settings = settings;

    public IReadOnlyUrlRepositorySettings Settings { get; set; }
    object IRepositoryWithSettings.DelegateSettings => Settings;
    public string Type { get; } = "url";
}

/// <summary>
/// Snapshot repository settings for <see cref="IReadOnlyUrlRepository"/>
/// </summary>
public interface IReadOnlyUrlRepositorySettings : IRepositorySettings
{
    /// <summary>
    /// Throttles the number of streams (per node) preforming snapshot operation. Defaults to 5
    /// </summary>
    [DataMember(Name = "concurrent_streams")]
    [JsonFormatter(typeof(NullableStringIntFormatter))]
    int? ConcurrentStreams { get; set; }

    /// <summary>
    /// Location of the snapshots. Required
    /// </summary>
    [DataMember(Name = "location")]
    string Location { get; set; }
}

/// <inheritdoc />
public class ReadOnlyUrlRepositorySettings : IReadOnlyUrlRepositorySettings
{
    internal ReadOnlyUrlRepositorySettings() { }

    public ReadOnlyUrlRepositorySettings(string location) => Location = location;

    /// <inheritdoc />
    public int? ConcurrentStreams { get; set; }

    /// <inheritdoc />
    public string Location { get; set; }
}

/// <inheritdoc cref="IReadOnlyUrlRepositorySettings"/>
public class ReadOnlyUrlRepositorySettingsDescriptor
    : DescriptorBase<ReadOnlyUrlRepositorySettingsDescriptor, IReadOnlyUrlRepositorySettings>, IReadOnlyUrlRepositorySettings
{
    int? IReadOnlyUrlRepositorySettings.ConcurrentStreams { get; set; }
    string IReadOnlyUrlRepositorySettings.Location { get; set; }

    /// <inheritdoc cref="IReadOnlyUrlRepositorySettings.Location"/>
    public ReadOnlyUrlRepositorySettingsDescriptor Location(string location) => Assign(location, (a, v) => a.Location = v);

    /// <inheritdoc cref="IReadOnlyUrlRepositorySettings.ConcurrentStreams"/>
    public ReadOnlyUrlRepositorySettingsDescriptor ConcurrentStreams(int? concurrentStreams) =>
        Assign(concurrentStreams, (a, v) => a.ConcurrentStreams = v);
}

/// <inheritdoc cref="IReadOnlyUrlRepository"/>
public class ReadOnlyUrlRepositoryDescriptor
    : DescriptorBase<ReadOnlyUrlRepositoryDescriptor, IReadOnlyUrlRepository>, IReadOnlyUrlRepository
{
    IReadOnlyUrlRepositorySettings IRepository<IReadOnlyUrlRepositorySettings>.Settings { get; set; }
    object IRepositoryWithSettings.DelegateSettings => Self.Settings;
    string ISnapshotRepository.Type => "url";

    /// <inheritdoc cref="IReadOnlyUrlRepositorySettings"/>
    public ReadOnlyUrlRepositoryDescriptor Settings(string location,
        Func<ReadOnlyUrlRepositorySettingsDescriptor, IReadOnlyUrlRepositorySettings> settingsSelector = null
    ) =>
        Assign(settingsSelector.InvokeOrDefault(new ReadOnlyUrlRepositorySettingsDescriptor().Location(location)), (a, v) => a.Settings = v);
}
