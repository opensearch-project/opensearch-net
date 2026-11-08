/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;

namespace Tests.Mapping.Types.Specialized.Knn;

/// <summary>
/// Unit tests for the top-level <see cref="IKnnVectorProperty"/> fields modelled by the OpenSearch
/// API specification — <c>data_type</c>, <c>space_type</c>, <c>mode</c>, and <c>compression_level</c> —
/// exercising both the object-initializer and fluent descriptor surfaces plus round-trip deserialization.
/// Kept as unit-only (no <c>[I]</c>) because byte vectors are engine-dependent server-side.
/// </summary>
public class KnnVectorDataTypeTests
{
	[U]
	public void InitializerSerializesTopLevelFields()
	{
		var property = new KnnVectorProperty
		{
			Dimension = 3,
			DataType = "byte",
			SpaceType = "l2",
			Mode = "on_disk",
			CompressionLevel = "4x",
		};

		var json = TestClient.DisabledStreaming.RequestResponseSerializer.SerializeToString(property);

		json.Should().Contain("\"type\":\"knn_vector\"");
		json.Should().Contain("\"dimension\":3");
		json.Should().Contain("\"data_type\":\"byte\"");
		json.Should().Contain("\"space_type\":\"l2\"");
		json.Should().Contain("\"mode\":\"on_disk\"");
		json.Should().Contain("\"compression_level\":\"4x\"");
	}

	[U]
	public void FluentSerializesTopLevelFields()
	{
		var descriptor = new PropertiesDescriptor<object>()
			.KnnVector(k => k
				.Name("embedding")
				.Dimension(3)
				.DataType("byte")
				.SpaceType("l2")
				.Mode("on_disk")
				.CompressionLevel("4x"));

		var json = TestClient.DisabledStreaming.RequestResponseSerializer.SerializeToString(descriptor);

		json.Should().Contain("\"data_type\":\"byte\"");
		json.Should().Contain("\"space_type\":\"l2\"");
		json.Should().Contain("\"mode\":\"on_disk\"");
		json.Should().Contain("\"compression_level\":\"4x\"");
	}

	[U]
	public void RoundTripsTopLevelFields()
	{
		const string json =
			"{\"type\":\"knn_vector\",\"dimension\":3,\"data_type\":\"byte\",\"space_type\":\"l2\",\"mode\":\"on_disk\",\"compression_level\":\"4x\"}";

		var property = TestClient.DisabledStreaming.RequestResponseSerializer.Deserialize<IKnnVectorProperty>(
			new MemoryStream(Encoding.UTF8.GetBytes(json)));

		property.Dimension.Should().Be(3);
		property.DataType.Should().Be("byte");
		property.SpaceType.Should().Be("l2");
		property.Mode.Should().Be("on_disk");
		property.CompressionLevel.Should().Be("4x");
	}
}
