/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Reflection;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;

namespace Tests.Mapping.Types.Specialized.Knn;

/// <summary>
/// Regression tests for <see cref="IKnnVectorProperty"/> serialization through the
/// <see cref="IPropertyVisitor"/> extension point.
///
/// Before the corresponding <c>PropertyFormatter</c> fix, the switch in
/// <c>PropertyFormatter.Serialize</c> had no <c>case IKnnVectorProperty</c> arm, so
/// any <see cref="KnnVectorProperty"/> produced from <c>AutoMap(visitor)</c> fell
/// into the reflection-based catch-all and walked
/// <c>PropertyName.Property</c> → <c>PropertyInfo</c> → <c>Module</c> →
/// <c>Assembly</c> → <c>Type[]</c> → <c>ConstructorInfo[]</c> forever, terminating
/// the process with a <see cref="System.StackOverflowException"/>.
/// </summary>
public class KnnVectorVisitorTests
{
    private class DocWithVector
    {
        public float[] Vec { get; set; }
    }

    private class KnnReturningVisitor : NoopPropertyVisitor
    {
        public override IProperty Visit(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute)
        {
            if (propertyInfo.PropertyType == typeof(float[]))
            {
                return new KnnVectorProperty
                {
                    Dimension = 3,
                    Method = new KnnMethod
                    {
                        Name = "hnsw",
                        Engine = "lucene",
                        SpaceType = "l2",
                        Parameters = new KnnMethodParameters { { "m", 24 } },
                    },
                };
            }
            return null;
        }
    }

    [U]
    public void KnnVectorReturnedFromVisitorSerializesViaTypedFormatter()
    {
        var mapping = new TypeMappingDescriptor<DocWithVector>().AutoMap(new KnnReturningVisitor());

        var json = TestClient.DisabledStreaming.RequestResponseSerializer.SerializeToString(mapping);

        json.Should().Contain("\"type\":\"knn_vector\"");
        json.Should().Contain("\"dimension\":3");
        json.Should().NotContain("RuntimeConstructorInfo");
        json.Should().NotContain("DeclaringType");
    }
}
