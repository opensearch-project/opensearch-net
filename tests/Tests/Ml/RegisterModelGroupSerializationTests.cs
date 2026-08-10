/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Serialization;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.Ml
{
	/// <summary>
	/// Round-trip serialization tests for the generated <c>ml.register_model_group</c> types:
	/// <see cref="RegisterModelGroupRequest"/>, <see cref="RegisterModelGroupDescriptor"/>,
	/// and <see cref="RegisterModelGroupResponse"/>.
	/// </summary>
	public class RegisterModelGroupSerializationTests
	{
		private static readonly object ExpectedJson = new
		{
			name = "g1",
			description = "d",
			access_mode = "restricted",
			backend_roles = new[] { "r1", "r2" },
			add_all_backend_roles = false,
		};

		/// <summary>
		/// Object-initializer form: all properties must appear with their wire names in the request body.
		/// Uses <see cref="JsonRoundTripper.FromRequest{T}"/> which captures <c>ApiCall.RequestBodyInBytes</c>
		/// from an in-memory client call — the same pattern used throughout the OSC test suite for request
		/// body assertions.
		/// </summary>
		[U]
		public void InitializerSerializesRequest()
		{
			var request = new RegisterModelGroupRequest
			{
				Name = "g1",
				Description = "d",
				AccessMode = ModelGroupAccessMode.Restricted,
				BackendRoles = new List<string> { "r1", "r2" },
				AddAllBackendRoles = false,
			};

			Expect(ExpectedJson).FromRequest(c => c.Ml.RegisterModelGroup(request));
		}

		/// <summary>
		/// Fluent descriptor form: all properties must appear with their wire names in the request body.
		/// </summary>
		[U]
		public void FluentDescriptorSerializesRequest()
		{
			Expect(ExpectedJson).FromRequest(c => c.Ml.RegisterModelGroup(d => d
				.Name("g1")
				.Description("d")
				.AccessMode(ModelGroupAccessMode.Restricted)
				.BackendRoles(new List<string> { "r1", "r2" })
				.AddAllBackendRoles(false)));
		}

		/// <summary>
		/// Response deserialization: JSON wire names must map to the correct C# properties.
		/// Uses <see cref="JsonRoundTripper.NoRoundTrip"/> because response objects are deserialized
		/// differently from how they would be re-serialized (they carry extra OSC metadata).
		/// </summary>
		[U]
		public void ResponseDeserializesCorrectly()
		{
			const string json = @"{""model_group_id"":""abc"",""status"":""created""}";

			var response = Expect(json).NoRoundTrip().DeserializesTo<RegisterModelGroupResponse>();

			response.Should().NotBeNull();
			response.ModelGroupId.Should().Be("abc");
			response.OperationStatus.Should().Be("created");
		}
	}
}
