/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using OpenSearch.Net;

namespace Tests.Auth.AwsSigV4.Utils;

internal class FixedDateTimeProvider : DateTimeProvider
{
    private readonly DateTime _now;

    public FixedDateTimeProvider(DateTime now) => _now = now;

    public override DateTime Now() => _now;
}
