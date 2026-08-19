/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.CommandLine;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Samples.Utils;

public static class OpenSearchClientOptions
{
	public static Func<ParseResult, IOpenSearchClient> AddOpenSearchClientOptions(this Command command, bool global = true)
	{
		Option<Uri> host = new("--host")
        {
            Description = "The OpenSearch host to connect to",
            DefaultValueFactory = _ => new Uri("https://localhost:9200"),
            Recursive = global
        };
        Option<string> username = new("--username")
        {
            Description = "The username to use for authentication",
            DefaultValueFactory = _ => "admin",
            Recursive = global
        };
        Option<string> password = new("--password")
        {
            Description = "The password to use for authentication",
            DefaultValueFactory = _ => "admin",
            Recursive = global
        };

		command.Add(host);
        command.Add(username);
        command.Add(password);

        return parseResult =>
        {
            var hostValue = parseResult.GetRequiredValue(host);
            var usernameValue = parseResult.GetRequiredValue(username);
            var passwordValue = parseResult.GetRequiredValue(password);

            var config = new ConnectionSettings(hostValue)
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                .BasicAuthentication(usernameValue, passwordValue)
                .DisableDirectStreaming();

            return new OpenSearchClient(config);
        };
    }
}
