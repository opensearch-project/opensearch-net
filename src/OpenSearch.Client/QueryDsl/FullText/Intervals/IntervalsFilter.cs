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
using System.Runtime.Serialization;

namespace OpenSearch.Client;

/// <summary>
/// Filters intervals produced by any rules by their relation to the intervals produced by another rule
/// </summary>
[ReadAs(typeof(IntervalsFilter))]
public interface IIntervalsFilter
{
    /// <summary>
    /// Produces intervals that appear after an interval from the filter role
    /// </summary>
    [DataMember(Name = "after")]
    IntervalsContainer After { get; set; }

    /// <summary>
    /// Produces intervals that appear before an interval from the filter role
    /// </summary>
    [DataMember(Name = "before")]
    IntervalsContainer Before { get; set; }

    /// <summary>
    /// Produces intervals that are contained by an interval from the filter rule
    /// </summary>
    [DataMember(Name = "contained_by")]
    IntervalsContainer ContainedBy { get; set; }

    /// <summary>
    /// Produces intervals that contain an interval from the filter rule
    /// </summary>
    [DataMember(Name = "containing")]
    IntervalsContainer Containing { get; set; }

    /// <summary>
    /// Produces intervals that are not contained by an interval from the filter rule
    /// </summary>
    [DataMember(Name = "not_contained_by")]
    IntervalsContainer NotContainedBy { get; set; }

    /// <summary>
    /// Produces intervals that do not contain an interval from the filter rule
    /// </summary>
    [DataMember(Name = "not_containing")]
    IntervalsContainer NotContaining { get; set; }

    /// <summary>
    /// Produces intervals that do not overlap with an interval from the filter rule
    /// </summary>
    [DataMember(Name = "not_overlapping")]
    IntervalsContainer NotOverlapping { get; set; }

    /// <summary>
    /// Produces intervals that overlap with an interval from the filter rule
    /// </summary>
    [DataMember(Name = "overlapping")]
    IntervalsContainer Overlapping { get; set; }

    /// <summary>
    /// filter intervals based on their start position, end position and internal gap count, using a script.
    /// The script has access to an <code>interval</code> variable, with <code>start</code>,
    /// <code>end</code> and <code>gaps</code> properties
    /// </summary>
    [DataMember(Name = "script")]
    IScript Script { get; set; }
}

/// <inheritdoc />
public class IntervalsFilter : IIntervalsFilter
{
    /// <inheritdoc />
    public IntervalsContainer After { get; set; }

    /// <inheritdoc />
    public IntervalsContainer Before { get; set; }

    /// <inheritdoc />
    public IntervalsContainer ContainedBy { get; set; }

    /// <inheritdoc />
    public IntervalsContainer Containing { get; set; }

    /// <inheritdoc />
    public IntervalsContainer NotContainedBy { get; set; }

    /// <inheritdoc />
    public IntervalsContainer NotContaining { get; set; }

    /// <inheritdoc />
    public IntervalsContainer NotOverlapping { get; set; }

    /// <inheritdoc />
    public IntervalsContainer Overlapping { get; set; }

    /// <inheritdoc />
    public IScript Script { get; set; }
}

/// <inheritdoc cref="IIntervalsFilter" />
public class IntervalsFilterDescriptor : DescriptorBase<IntervalsFilterDescriptor, IIntervalsFilter>, IIntervalsFilter
{
    IntervalsContainer IIntervalsFilter.After { get; set; }
    IntervalsContainer IIntervalsFilter.Before { get; set; }
    IntervalsContainer IIntervalsFilter.ContainedBy { get; set; }
    IntervalsContainer IIntervalsFilter.Containing { get; set; }
    IntervalsContainer IIntervalsFilter.NotContainedBy { get; set; }
    IntervalsContainer IIntervalsFilter.NotContaining { get; set; }
    IntervalsContainer IIntervalsFilter.NotOverlapping { get; set; }
    IntervalsContainer IIntervalsFilter.Overlapping { get; set; }
    IScript IIntervalsFilter.Script { get; set; }

    /// <inheritdoc cref="IIntervalsFilter.Containing" />
    public IntervalsFilterDescriptor Containing(Func<IntervalsDescriptor, IntervalsContainer> selector) =>
        Assign(selector?.Invoke(new IntervalsDescriptor()), (a, v) => a.Containing = v);

    /// <inheritdoc cref="IIntervalsFilter.ContainedBy" />
    public IntervalsFilterDescriptor ContainedBy(Func<IntervalsDescriptor, IntervalsContainer> selector) =>
        Assign(selector?.Invoke(new IntervalsDescriptor()), (a, v) => a.ContainedBy = v);

    /// <inheritdoc cref="IIntervalsFilter.NotContaining" />
    public IntervalsFilterDescriptor NotContaining(Func<IntervalsDescriptor, IntervalsContainer> selector) =>
        Assign(selector?.Invoke(new IntervalsDescriptor()), (a, v) => a.NotContaining = v);

    /// <inheritdoc cref="IIntervalsFilter.NotContainedBy" />
    public IntervalsFilterDescriptor NotContainedBy(Func<IntervalsDescriptor, IntervalsContainer> selector) =>
        Assign(selector?.Invoke(new IntervalsDescriptor()), (a, v) => a.NotContainedBy = v);

    /// <inheritdoc cref="IIntervalsFilter.Overlapping" />
    public IntervalsFilterDescriptor Overlapping(Func<IntervalsDescriptor, IntervalsContainer> selector) =>
        Assign(selector?.Invoke(new IntervalsDescriptor()), (a, v) => a.Overlapping = v);

    /// <inheritdoc cref="IIntervalsFilter.NotOverlapping" />
    public IntervalsFilterDescriptor NotOverlapping(Func<IntervalsDescriptor, IntervalsContainer> selector) =>
        Assign(selector?.Invoke(new IntervalsDescriptor()), (a, v) => a.NotOverlapping = v);

    /// <inheritdoc cref="IIntervalsFilter.Before" />
    public IntervalsFilterDescriptor Before(Func<IntervalsDescriptor, IntervalsContainer> selector) =>
        Assign(selector?.Invoke(new IntervalsDescriptor()), (a, v) => a.Before = v);

    /// <inheritdoc cref="IIntervalsFilter.After" />
    public IntervalsFilterDescriptor After(Func<IntervalsDescriptor, IntervalsContainer> selector) =>
        Assign(selector?.Invoke(new IntervalsDescriptor()), (a, v) => a.After = v);

    /// <inheritdoc cref="IIntervalsFilter.Script" />
    public IntervalsFilterDescriptor Script(Func<ScriptDescriptor, IScript> selector) =>
        Assign(selector?.Invoke(new ScriptDescriptor()), (a, v) => a.Script = v);
}
