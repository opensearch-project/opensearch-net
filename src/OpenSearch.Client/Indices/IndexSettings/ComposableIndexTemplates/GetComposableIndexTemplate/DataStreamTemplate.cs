/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Runtime.Serialization;

namespace OpenSearch.Client;

[ReadAs(typeof(DataStreamTemplate))]
public interface IDataStreamTemplate
{
	[DataMember(Name = "timestamp_field")]
	ITimestampField TimestampField { get; set; }
}

public class DataStreamTemplate : IDataStreamTemplate
{
	public ITimestampField TimestampField { get; set; }
}

public class DataStreamTemplateDescriptor : DescriptorBase<DataStreamTemplateDescriptor, IDataStreamTemplate>, IDataStreamTemplate
{
	ITimestampField IDataStreamTemplate.TimestampField { get; set; }

	public DataStreamTemplateDescriptor TimestampField(Func<TimestampFieldDescriptor, ITimestampField> selector) =>
		Assign(selector, (a, v) => a.TimestampField = v?.Invoke(new TimestampFieldDescriptor()));
}

[ReadAs(typeof(TimestampField))]
public interface ITimestampField
{
	[DataMember(Name = "name")]
	string Name { get; set; }
}

public class TimestampField : ITimestampField
{
	public string Name { get; set; }
}

public class TimestampFieldDescriptor : DescriptorBase<TimestampFieldDescriptor, ITimestampField>, ITimestampField
{
	string ITimestampField.Name { get; set; }

	public TimestampFieldDescriptor Name(string name) => Assign(name, (a, v) => a.Name = v);
}
