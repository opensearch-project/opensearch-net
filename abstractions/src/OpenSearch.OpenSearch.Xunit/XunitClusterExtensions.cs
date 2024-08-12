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
using System.Collections.Concurrent;
using OpenSearch.OpenSearch.Ephemeral;

namespace OpenSearch.OpenSearch.Xunit;

/// <summary>
///     Extension methods for <see cref="IEphemeralCluster" />
/// </summary>
public static class XunitClusterExtensions
{
    private static readonly ConcurrentDictionary<IEphemeralCluster, object> Clients =
        new ConcurrentDictionary<IEphemeralCluster, object>();

    /// <summary>
    ///     Gets a client for the cluster if one exists, or creates a new client if one doesn't.
    /// </summary>
    /// <param name="cluster">the cluster to create a client for</param>
    /// <param name="getOrAdd">A delegate to create a client, given the cluster to create it for</param>
    /// <typeparam name="T">the type of the client</typeparam>
    /// <returns>An instance of a client</returns>
    public static T GetOrAddClient<T>(this IEphemeralCluster cluster, Func<IEphemeralCluster, T> getOrAdd) =>
        (T)Clients.GetOrAdd(cluster, c => getOrAdd(c));
}
