/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Client;

namespace Tests.Mapping.Types.Specialized.Wildcard;

public class WildcardTest
{
    [Wildcard(NullValue = "NULL", Normalizer = "my_normalizer")]
    public string Full { get; set; }

    [Wildcard]
    public string Minimal { get; set; }
}

public class WildcardAttributeTests : AttributeTestsBase<WildcardTest>
{
    protected override object ExpectJson => new
    {
        properties = new
        {
            full = new
            {
                type = "wildcard",
                null_value = "NULL",
                normalizer = "my_normalizer",
            },
            minimal = new
            {
                type = "wildcard"
            }
        }
    };
}
