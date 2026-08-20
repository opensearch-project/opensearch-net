/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Serialization;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.ML
{
	/// <summary>
	/// Regression tests for the response-field vs ResponseBase collision fix.
	///
	/// The bug: generated response types with a string <c>status</c> field would throw a
	/// Utf8Json parse exception on any 4xx/5xx server response because the error envelope
	/// <c>{"error":{...},"status":400}</c> routes the numeric token into the string property.
	///
	/// The fix renames the C# property to <c>OperationStatus</c> (keeping wire name "status")
	/// and attaches <c>[JsonFormatter(typeof(IntStringFormatter))]</c> so both a string token
	/// (success path) and an integer token (error envelope path) are absorbed without throwing.
	/// </summary>
	public class ResponseStatusEnvelopeTests
	{
		/// <summary>
		/// Success path: a normal 200 response with a string "status" value must deserialize
		/// into <c>OperationStatus</c> correctly and must not set <c>ServerError</c>.
		/// </summary>
		[U]
		public void SuccessResponse_StringStatus_DeserializesToOperationStatus()
		{
			const string json = @"{""model_group_id"":""g1"",""status"":""CREATED""}";

			var response = Expect(json).NoRoundTrip().DeserializesTo<RegisterModelGroupResponse>();

			response.Should().NotBeNull();
			response.ModelGroupId.Should().Be("g1");
			response.OperationStatus.Should().Be("CREATED");
			// No error present in this payload → ServerError should be null
			response.ServerError.Should().BeNull();
		}

		/// <summary>
		/// Error path: an OpenSearch error envelope <c>{"error":{...},"status":400}</c> must
		/// deserialize without throwing even though the "status" value is a number, not a string.
		/// The <c>IntStringFormatter</c> absorbs the numeric token; <c>ServerError</c> is
		/// populated from the "error" object.
		/// </summary>
		[U]
		public void ErrorEnvelope_NumericStatus_DoesNotThrow_AndSetsServerError()
		{
			const string json = @"{""error"":{""type"":""model_group_access_denied"",""reason"":""no access""},""status"":400}";

			// Before the fix this would throw: Utf8Json.JsonParsingException "expected String Begin Token, actual 400"
			var response = Expect(json).NoRoundTrip().DeserializesTo<RegisterModelGroupResponse>();

			response.Should().NotBeNull();
			// The numeric 400 token is absorbed by IntStringFormatter and returned as the string "400"
			response.OperationStatus.Should().Be("400");
			// The "error" key is absorbed by ResponseBase.Error and surfaced as ServerError
			response.ServerError.Should().NotBeNull();
			response.ServerError.Error.Type.Should().Be("model_group_access_denied");
		}
	}
}
