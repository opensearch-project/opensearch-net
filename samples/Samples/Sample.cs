/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.CommandLine;
using System.CommandLine.Binding;
using OpenSearch.Client;

namespace Samples;

public abstract class Sample
{
	public static IEnumerable<Sample> GetAllSamples() =>
		typeof(Sample)
			.Assembly
			.GetTypes()
			.Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Sample)))
			.Select(t => (Sample) Activator.CreateInstance(t)!);

	private readonly string _name;
	private readonly string _description;

	protected Sample(string name, string description)
	{
		_name = name;
		_description = description;
	}

	public Command AsCommand(IValueDescriptor<IOpenSearchClient> clientDescriptor)
	{
		var command = new Command(_name, _description);

		command.SetHandler(Run, clientDescriptor);

		return command;
	}

	protected abstract Task Run(IOpenSearchClient client);
}
