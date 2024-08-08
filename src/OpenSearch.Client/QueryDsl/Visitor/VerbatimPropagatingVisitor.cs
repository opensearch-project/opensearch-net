/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

namespace OpenSearch.Client
{
    public class VerbatimPropagatingVisitor : QueryVisitor
    {
        private readonly bool _verbatim;

        public VerbatimPropagatingVisitor(bool verbatim) => _verbatim = verbatim;

        public override void Visit(IQuery query)
        {
            query.IsVerbatim = _verbatim;
            base.Visit(query);
        }
    }
}
