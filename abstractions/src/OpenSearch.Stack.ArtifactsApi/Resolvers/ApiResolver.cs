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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenSearch.Stack.ArtifactsApi.Resolvers;

public static class ApiResolver
{
    // TODO: update string when working on artifacts API
    private const string ArtifactsApiUrl = "https://artifacts-api.opensearch.org/v1/";

    private static readonly ConcurrentDictionary<string, bool> Releases = new ConcurrentDictionary<string, bool>();

    private static HttpClient HttpClient { get; } =
        new HttpClient(new HttpClientHandler { SslProtocols = SslProtocols.Tls12 })
        {
            BaseAddress = new Uri(ArtifactsApiUrl)
        };

    private static Regex BuildHashRegex { get; } =
        // TODO: update string when working on artifacts API
        new Regex(@"https://(?:snapshots|staging).opensearch.org/(\d+\.\d+\.\d+-([^/]+)?)");

    public static string FetchJson(string path)
    {
        using (var stream = HttpClient.GetStreamAsync(path).GetAwaiter().GetResult())
        using (var fileStream = new StreamReader(stream))
            return fileStream.ReadToEnd();
    }

    public static bool IsReleasedVersion(string version)
    {
        if (Releases.TryGetValue(version, out var released)) return released;
        var versionPath = "https://github.com/opensearch-project/opensearch/releases/tag/" + version;
        var message = new HttpRequestMessage { Method = HttpMethod.Head, RequestUri = new Uri(versionPath) };

        using (var response = HttpClient.SendAsync(message).GetAwaiter().GetResult())
        {
            released = response.IsSuccessStatusCode;
            Releases.TryAdd(version, released);
            return released;
        }
    }

    public static string LatestBuildHash(string version)
    {
        var json = FetchJson($"search/{version}/msi");
        try
        {
            // if packages is empty it turns into an array[] otherwise its a dictionary :/
            var packages = JsonSerializer.Deserialize<ArtifactsSearchResponse>(json).Packages;
            if (packages.Count == 0)
                throw new Exception("Can not get build hash for: " + version);
            return GetBuildHash(packages.First().Value.DownloadUrl);
        }
        catch
        {
            throw new Exception("Can not get build hash for: " + version);
        }
    }

    public static string GetBuildHash(string url)
    {
        var tokens = BuildHashRegex.Split(url).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (tokens.Length < 2)
            throw new Exception("Can not parse build hash from: " + url);

        return tokens[1];
    }
}

internal class ArtifactsVersionsResponse
{
    [JsonPropertyName("versions")] public List<string> Versions { get; set; }
}

internal class ArtifactsSearchResponse
{
    [JsonPropertyName("packages")] public Dictionary<string, SearchPackage> Packages { get; set; }
}

internal class SearchPackage
{
    [JsonPropertyName("url")] public string DownloadUrl { get; set; }
    [JsonPropertyName("sha_url")] public string ShaUrl { get; set; }
    [JsonPropertyName("asc_url")] public string AscUrl { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; }
    [JsonPropertyName("architecture")] public string Architecture { get; set; }
    [JsonPropertyName("os")] public string[] OperatingSystem { get; set; }
    [JsonPropertyName("classifier")] public string Classifier { get; set; }
}
