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

using System.IO;
using OpenSearch.OpenSearch.Managed.FileSystem;
using OpenSearch.Stack.ArtifactsApi;
using OpenSearch.Stack.ArtifactsApi.Products;

namespace OpenSearch.OpenSearch.Ephemeral
{
	public class EphemeralFileSystem : NodeFileSystem
	{
		public EphemeralFileSystem(OpenSearchVersion version, string clusterName) : base(version,
			EphemeralHome(version, clusterName)) => ClusterName = clusterName;

		private string ClusterName { get; }

		public string TempFolder => Path.Combine(Path.GetTempPath(), SubFolder, Artifact.LocalFolderName, ClusterName);

		public override string ConfigPath => Path.Combine(TempFolder, "config");
		public override string LogsPath => Path.Combine(TempFolder, "logs");
		public override string RepositoryPath => Path.Combine(TempFolder, "repositories");
		public override string DataPath => Path.Combine(TempFolder, "data");

		//certificates
		public string CertificateFolderName => "node-certificates";
		public string CertificateNodeName => "node01";
		public string ClientCertificateName => "cn=John Doe,ou=example,o=com";
		public string ClientCertificateFilename => "john_doe";

		public string CertificatesPath => Path.Combine(ConfigPath, CertificateFolderName);

		public string CaCertificate => Path.Combine(CertificatesPath, "ca", "ca") + ".crt";

		public string NodePrivateKey =>
			Path.Combine(CertificatesPath, CertificateNodeName, CertificateNodeName) + ".key";

		public string NodeCertificate =>
			Path.Combine(CertificatesPath, CertificateNodeName, CertificateNodeName) + ".crt";

		public string ClientCertificate =>
			Path.Combine(CertificatesPath, ClientCertificateFilename, ClientCertificateFilename) + ".crt";

		public string ClientPrivateKey =>
			Path.Combine(CertificatesPath, ClientCertificateFilename, ClientCertificateFilename) + ".key";

		public string UnusedCertificateFolderName => $"unused-{CertificateFolderName}";
		public string UnusedCertificatesPath => Path.Combine(ConfigPath, UnusedCertificateFolderName);
		public string UnusedCaCertificate => Path.Combine(UnusedCertificatesPath, "ca", "ca") + ".crt";

		public string UnusedClientCertificate =>
			Path.Combine(UnusedCertificatesPath, ClientCertificateFilename, ClientCertificateFilename) + ".crt";


		protected static string EphemeralHome(OpenSearchVersion version, string clusterName)
		{
			var temp = Path.Combine(Path.GetTempPath(), SubFolder,
				version.Artifact(Product.OpenSearch).LocalFolderName, clusterName);
			return Path.Combine(temp, "home");
		}
	}
}
