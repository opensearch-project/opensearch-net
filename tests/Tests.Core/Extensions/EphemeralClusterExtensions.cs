/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
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
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Xunit;
using OpenSearch.Net;
using OpenSearch.Client;
using Tests.Core.Client.Settings;

namespace Tests.Core.Extensions
{
	public static class EphemeralClusterExtensions
	{
		public static ConnectionSettings CreateConnectionSettings<TConfig>(this IEphemeralCluster<TConfig> cluster)
			where TConfig : EphemeralClusterConfiguration
		{
			var clusterNodes = cluster.NodesUris(TestConnectionSettings.LocalOrProxyHost);
			//we ignore the uri's that TestConnection provides and seed with the nodes the cluster dictates.
			return new TestConnectionSettings(uris => new StaticConnectionPool(clusterNodes));
		}

		public static IOpenSearchClient GetOrAddClient<TConfig>(
			this IEphemeralCluster<TConfig> cluster,
			Func<ConnectionSettings, ConnectionSettings> modifySettings = null
		)
			where TConfig : EphemeralClusterConfiguration
		{
			modifySettings = modifySettings ?? (s => s);
			return cluster.GetOrAddClient(c =>
			{
				var settings = modifySettings(cluster.CreateConnectionSettings());
				settings = (ConnectionSettings)UpdateSettings(cluster, settings);

				var client = new OpenSearchClient(settings);
				return client;
			});
		}

		public static ConnectionConfiguration<TConnectionSettings> UpdateSettings<TConfig, TConnectionSettings>
			(this IEphemeralCluster<TConfig> cluster, ConnectionConfiguration<TConnectionSettings> settings)
			where TConfig : EphemeralClusterConfiguration
			where TConnectionSettings : ConnectionConfiguration<TConnectionSettings>
		{
			var current = (IConnectionConfigurationValues)settings;
			var notAlreadyAuthenticated = current.BasicAuthenticationCredentials == null
				&& current.ApiKeyAuthenticationCredentials == null
				&& current.ClientCertificates == null;

			if (notAlreadyAuthenticated)
				settings = settings.BasicAuthentication(ClusterAuthentication.Admin.Username,
														ClusterAuthentication.Admin.Password);

			var noCertValidation = current.ServerCertificateValidationCallback == null;

			if (cluster.ClusterConfiguration.EnableSsl && noCertValidation)
			{
				//todo use CA callback instead of allowall
				settings = settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
			}
			return settings;
		}
	}
}
