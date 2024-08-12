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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OpenSearch.Net;

namespace OpenSearch.Client;

[DataContract]
public class BulkResponse : ResponseBase
{
    [DataMember(Name = "errors")]
    public bool Errors { get; internal set; }

    public override bool IsValid => base.IsValid && !Errors && !ItemsWithErrors.HasAny();

    [DataMember(Name = "items")]
    public IReadOnlyList<BulkResponseItemBase> Items { get; internal set; } = EmptyReadOnly<BulkResponseItemBase>.List;

    [IgnoreDataMember]
    public IEnumerable<BulkResponseItemBase> ItemsWithErrors => !Items.HasAny()
        ? Enumerable.Empty<BulkResponseItemBase>()
        : Items.Where(i => !i.IsValid);

    [DataMember(Name = "took")]
    public long Took { get; internal set; }

    protected override void DebugIsValid(StringBuilder sb)
    {
        if (Items == null) return;

        sb.AppendLine($"# Invalid Bulk items:");
        foreach (var i in Items.Select((item, i) => new { item, i }).Where(i => !i.item.IsValid))
            sb.AppendLine($"  operation[{i.i}]: {i.item}");
    }
}
