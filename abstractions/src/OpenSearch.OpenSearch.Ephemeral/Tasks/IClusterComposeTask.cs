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
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;
using ProcNet;
using ProcNet.Std;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks
{
    public interface IClusterComposeTask
    {
        void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster);
    }

    public interface IClusterTeardownTask
    {
        /// <summary>
        ///     Called when the cluster disposes, used to clean up after itself.
        /// </summary>
        /// <param name="cluster">The cluster configuration of the node that was started</param>
        /// <param name="nodeStarted">Whether the cluster composer was successful in starting the node</param>
        void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster, bool nodeStarted);
    }

    public abstract class ClusterComposeTask : IClusterComposeTask
    {
        protected static bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        protected static string BinarySuffix => IsWindows ? ".bat" : string.Empty;
        public abstract void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster);

        protected static void DownloadFile(string from, string to)
        {
            if (File.Exists(to)) return;
            var http = new HttpClient();
            using (var stream = http.GetStreamAsync(new Uri(from)).GetAwaiter().GetResult())
            using (var fileStream = File.Create(to))
            {
                stream.CopyTo(fileStream);
                fileStream.Flush();
            }
        }

        protected string GetResponseException(HttpResponseMessage m) =>
            $"Code: {m?.StatusCode} Reason: {m?.ReasonPhrase} Content: {GetResponseString(m)}";

        protected string GetResponseString(HttpResponseMessage m) =>
            m?.Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult() ?? string.Empty;

        protected HttpResponseMessage Get(IEphemeralCluster<EphemeralClusterConfiguration> cluster, string path,
            string query) =>
            Call(cluster, path, query, (c, u, t) => c.GetAsync(u, t));

        protected HttpResponseMessage Post(IEphemeralCluster<EphemeralClusterConfiguration> cluster, string path,
            string query, string json) =>
            Call(cluster, path, query,
                (c, u, t) => c.PostAsync(u, new StringContent(json, Encoding.UTF8, "application/json"), t));

        private HttpResponseMessage Call(
            IEphemeralCluster<EphemeralClusterConfiguration> cluster,
            string path,
            string query,
            Func<HttpClient, Uri, CancellationToken, Task<HttpResponseMessage>> verb)
        {
            var q = string.IsNullOrEmpty(query) ? "pretty=true" : query + "&pretty=true";
            var statusUrl = new UriBuilder(cluster.NodesUris().First()) { Path = path, Query = q }.Uri;

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var handler = new HttpClientHandler
            {
                AutomaticDecompression =
                    DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None,
            };
            cluster.Writer.WriteDiagnostic(
                $"{{{nameof(Call)}}} [{statusUrl}] SSL: {cluster.ClusterConfiguration.EnableSsl}");
            if (cluster.ClusterConfiguration.EnableSsl)
            {
#if !NETSTANDARD
				ServicePointManager.ServerCertificateValidationCallback += ServerCertificateValidationCallback;
#else
                handler.ServerCertificateCustomValidationCallback += (m, c, cn, p) => true;
#endif
            }

            using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) })
            {
                if (cluster.ClusterConfiguration.EnableSsl)
                {
                    var byteArray =
                        Encoding.ASCII.GetBytes(
                            $"{ClusterAuthentication.Admin.Username}:{ClusterAuthentication.Admin.Password}");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }

                try
                {
                    var response = verb(client, statusUrl, tokenSource.Token).ConfigureAwait(false).GetAwaiter()
                        .GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        cluster.Writer.WriteDiagnostic(
                            $"{{{nameof(Call)}}} [{statusUrl}] Unsuccessful status code: [{(int)response.StatusCode}]");
                        var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        foreach (var l in (body ?? string.Empty).Split('\n', '\r'))
                            cluster.Writer.WriteDiagnostic($"{{{nameof(Call)}}} [{statusUrl}] returned [{l}]");
                    }

                    return response;
                }
                catch (Exception e)
                {
                    cluster.Writer.WriteError($"{{{nameof(Call)}}} [{statusUrl}] exception: {e}");
                    throw;
                }
                finally
                {
#if !NETSTANDARD
				ServicePointManager.ServerCertificateValidationCallback -= ServerCertificateValidationCallback;
#endif
                }
            }
        }

        private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslpolicyerrors) => true;

        protected static void WriteFileIfNotExist(string fileLocation, string contents)
        {
            if (!File.Exists(fileLocation)) File.WriteAllText(fileLocation, contents);
        }

        protected static void ExecuteBinary(EphemeralClusterConfiguration config, IConsoleLineHandler writer,
            string binary, string description, params string[] arguments) =>
            ExecuteBinaryInternal(config, writer, binary, description, null, arguments);

        protected static void ExecuteBinary(EphemeralClusterConfiguration config, IConsoleLineHandler writer,
            string binary, string description, IDictionary<string, string> environmentVariables,
            params string[] arguments) =>
            ExecuteBinaryInternal(config, writer, binary, description, environmentVariables, arguments);

        private static void ExecuteBinaryInternal(EphemeralClusterConfiguration config, IConsoleLineHandler writer,
            string binary, string description, IDictionary<string, string> environmentVariables, params string[] arguments)
        {
            var command = $"{{{binary}}} {{{string.Join(" ", arguments)}}}";
            writer?.WriteDiagnostic($"{{{nameof(ExecuteBinary)}}} starting process [{description}] {command}");

            var environment = new Dictionary<string, string>
            {
                {config.FileSystem.ConfigEnvironmentVariableName, config.FileSystem.ConfigPath},
                {"OPENSEARCH_HOME", config.FileSystem.OpenSearchHome}
            };

            if (environmentVariables != null)
            {
                foreach (var kvp in environmentVariables)
                    environment[kvp.Key] = kvp.Value;
            }

            var timeout = TimeSpan.FromSeconds(420);
            var processStartArguments = new StartArguments(binary, arguments)
            {
                Environment = environment
            };

            var result = Proc.Start(processStartArguments, timeout, new ConsoleOutColorWriter());

            if (!result.Completed)
                throw new Exception($"Timeout while executing {description} exceeded {timeout}");

            if (result.ExitCode != 0)
                throw new Exception(
                    $"Expected exit code 0 but received ({result.ExitCode}) while executing {description}: {command}");

            var errorOut = result.ConsoleOut.Where(c => c.Error).ToList();

            if (errorOut.Any(e =>
                    !string.IsNullOrWhiteSpace(e.Line) && !e.Line.Contains("usage of JAVA_HOME is deprecated")) &&
                !binary.Contains("plugin") && !binary.Contains("cert"))
                throw new Exception(
                    $"Received error out with exitCode ({result.ExitCode}) while executing {description}: {command}");

            writer?.WriteDiagnostic(
                $"{{{nameof(ExecuteBinary)}}} finished process [{description}] {{{result.ExitCode}}}");
        }

        protected static void CopyFolder(string source, string target, bool overwrite = true)
        {
            foreach (var sourceDir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                var targetDir = sourceDir.Replace(source, target);
                Directory.CreateDirectory(targetDir);
            }

            foreach (var sourcePath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            {
                var targetPath = sourcePath.Replace(source, target);
                if (!overwrite && File.Exists(targetPath)) continue;
                File.Copy(sourcePath, targetPath, overwrite);
            }
        }

        protected static void Extract(string file, string toFolder)
        {
            if (file.EndsWith(".zip")) ExtractZip(file, toFolder);
            else if (file.EndsWith(".tar.gz")) ExtractTarGz(file, toFolder);
            else if (file.EndsWith(".tar")) ExtractTar(file, toFolder);
            else throw new Exception("Can not extract:" + file);
        }

        private static void ExtractTar(string file, string toFolder)
        {
            using (var inStream = File.OpenRead(file))
            using (var tarArchive = TarArchive.CreateInputTarArchive(inStream, Encoding.UTF8))
                tarArchive.ExtractContents(toFolder);
        }

        private static void ExtractTarGz(string file, string toFolder)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                using (var inStream = File.OpenRead(file))
                using (var gzipStream = new GZipInputStream(inStream))
                using (var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8))
                    tarArchive.ExtractContents(toFolder);
            else
                //SharpZipLib loses permissions when untarring
                Proc.Exec("tar", "-xvf", file, "-C", toFolder);
        }

        private static void ExtractZip(string file, string toFolder) =>
            ZipFile.ExtractToDirectory(file, toFolder);
    }
}
