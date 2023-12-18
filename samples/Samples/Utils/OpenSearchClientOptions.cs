/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.CommandLine;
using System.CommandLine.Binding;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Samples.Utils;

public static class OpenSearchClientOptions
{
	public static IValueDescriptor<IOpenSearchClient> AddOpenSearchClientOptions(this Command command, bool global = true)
	{
		Option<Uri> host = new("--host", () => new Uri("https://localhost:9200"), "The OpenSearch host to connect to");
		Option<string> username = new("--username", () => "admin", "The username to use for authentication");
		Option<string> password = new("--password", () => "admin", "The password to use for authentication");

		Action<Option> add = global ? command.AddGlobalOption : command.AddOption;

		add(host);
		add(username);
		add(password);

		return new Binder(host, username, password);
	}

	private class Binder : BinderBase<IOpenSearchClient>
	{
		private readonly Option<Uri> _host;
		private readonly Option<string> _username;
		private readonly Option<string> _password;

		public Binder(Option<Uri> host, Option<string> username, Option<string> password)
		{
			_host = host;
			_username = username;
			_password = password;
		}

		protected override IOpenSearchClient GetBoundValue(BindingContext bindingContext)
		{
			var host = bindingContext.ParseResult.GetValueForOption(_host);
			var user = bindingContext.ParseResult.GetValueForOption(_username);
			var password = bindingContext.ParseResult.GetValueForOption(_password);

			var config = new ConnectionSettings(host)
				.ServerCertificateValidationCallback(CertificateValidations.AllowAll)
				.BasicAuthentication(user, password)
				.DisableDirectStreaming();

			return new OpenSearchClient(config);
		}
	}
}
