/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;

namespace OpenSearch.OpenSearch.Xunit.XunitPlumbing;

/// <summary>
/// A Xunit test that should be skipped for prerelease OpenSearch versions, and a reason why.
/// </summary>
public class SkipPrereleaseVersionsAttribute : Attribute
{
    public SkipPrereleaseVersionsAttribute(string reason) => Reason = reason;

    /// <summary>
    /// The reason why the test should be skipped
    /// </summary>
    public string Reason { get; }
}
