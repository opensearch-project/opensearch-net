/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.CommandLine;
using Amazon;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Auth.AwsSigV4;

namespace Samples.Utils;

public static class OpenSearchClientOptions
{
    public static Func<ParseResult, IOpenSearchClient> AddOpenSearchClientOptions(this Command command, bool global = true)
    {
        Option<string> host = new("--host")
        {
            Description = "The OpenSearch host to connect to",
            DefaultValueFactory = _ => "https://localhost:9200",
            Recursive = global
        };
        Option<string> username = new("--username")
        {
            Description = "The username to use for basic authentication",
            DefaultValueFactory = _ => "admin",
            Recursive = global
        };
        Option<string> password = new("--password")
        {
            Description = "The password to use for basic authentication",
            DefaultValueFactory = _ => "admin",
            Recursive = global
        };
        Option<bool> aws = new("--aws")
        {
            Description = "Use AWS SigV4 signing (for Amazon OpenSearch Service). Credentials and region are read from the environment (e.g. ~/.aws/credentials set by ada).",
            DefaultValueFactory = _ => false,
            Recursive = global
        };
        Option<string> awsRegion = new("--aws-region")
        {
            Description = "AWS region for SigV4 signing (overrides the environment). Required when --aws is set and the region cannot be inferred.",
            DefaultValueFactory = _ => "",
            Recursive = global
        };

        command.Add(host);
        command.Add(username);
        command.Add(password);
        command.Add(aws);
        command.Add(awsRegion);

        return parseResult =>
        {
            var hostValue = new Uri(parseResult.GetRequiredValue(host));
            var useAws = parseResult.GetRequiredValue(aws);
            var regionValue = parseResult.GetRequiredValue(awsRegion);

            ConnectionSettings config;
            if (useAws)
            {
                var region = string.IsNullOrEmpty(regionValue)
                    ? null
                    : RegionEndpoint.GetBySystemName(regionValue);

                config = new ConnectionSettings(hostValue, new AwsSigV4HttpConnection(region))
                    .DisableDirectStreaming();
            }
            else
            {
                var usernameValue = parseResult.GetRequiredValue(username);
                var passwordValue = parseResult.GetRequiredValue(password);

                config = new ConnectionSettings(hostValue)
                    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                    .BasicAuthentication(usernameValue, passwordValue)
                    .DisableDirectStreaming();
            }

            return new OpenSearchClient(config);
        };
    }
}
