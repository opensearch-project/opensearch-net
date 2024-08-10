/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;

namespace OpenSearch.Client;

public class StrictnessPropagatingVisitor : QueryVisitor
{
    private readonly bool _strict;

    public StrictnessPropagatingVisitor(bool strict) => _strict = strict;

    public override void Visit(IQuery query)
    {
        query.IsStrict = _strict;
        base.Visit(query);
    }
}
