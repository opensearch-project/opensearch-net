/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.CommandLine;
using Samples.Utils;

namespace Samples;

public static class Program
{
	public static async Task Main(string[] args)
	{
		var rootCommand = new RootCommand("A collection of samples demonstrating how to use the OpenSearch .NET client");
		var clientDescriptor = rootCommand.AddOpenSearchClientOptions();

		foreach (var sample in Sample.GetAllSamples()) rootCommand.Add(sample.AsCommand(clientDescriptor));

		await rootCommand.Parse(args).InvokeAsync();
	}
}
