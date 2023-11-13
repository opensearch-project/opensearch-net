/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Runtime.Serialization;

namespace OpenSearch.Client;

[ReadAs(typeof(PointInTime))]
public interface IPointInTime
{
	[DataMember(Name = "id")]
	string Id { get; set; }

	[DataMember(Name = "keep_alive")]
	Time KeepAlive { get; set; }
}

public class PointInTime : IPointInTime
{
	public string Id { get; set; }
	public Time KeepAlive { get; set; }
}

public class PointInTimeDescriptor : DescriptorBase<PointInTimeDescriptor, IPointInTime>, IPointInTime
{
	string IPointInTime.Id { get; set; }
	Time IPointInTime.KeepAlive { get; set; }

	public PointInTimeDescriptor Id(string id) => Assign(id, (a, v) => a.Id = v);

	public PointInTimeDescriptor KeepAlive(Time keepAlive) => Assign(keepAlive, (a, v) => a.KeepAlive = v);
}
