/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="SortOrderConverter{TSortOrder}"/>. An <see cref="ISortOrder"/> is
	/// serialized as a single-property JSON object keyed by <see cref="ISortOrder.Key"/> with the
	/// <see cref="SortOrder"/> as its value; non-object tokens are skipped and yield <c>null</c>.
	/// </summary>
	public class SortOrderConverterTests
	{
		// SortOrderConverter<TSortOrder> is internal-generic; exercise it through the concrete HistogramOrder.
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new SortOrderConverter<HistogramOrder>());
			// SortOrder enum serializes via the StringEnum factory (asc/desc).
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		[U] public void Read_Object_Ascending()
		{
			var order = JsonSerializer.Deserialize<HistogramOrder>(@"{""_count"":""asc""}", Options());
			order.Should().NotBeNull();
			order.Key.Should().Be("_count");
			order.Order.Should().Be(SortOrder.Ascending);
		}

		[U] public void Read_Object_Descending()
		{
			var order = JsonSerializer.Deserialize<HistogramOrder>(@"{""_key"":""desc""}", Options());
			order.Should().NotBeNull();
			order.Key.Should().Be("_key");
			order.Order.Should().Be(SortOrder.Descending);
		}

		[U] public void Read_Null_Object_IsSkipped_ReturnsNull()
		{
			// A null token is not StartObject; the converter skips it and returns null.
			var order = JsonSerializer.Deserialize<HistogramOrder>("null", Options());
			order.Should().BeNull();
		}

		[U] public void Read_NonObjectToken_IsSkipped_ReturnsNull()
		{
			var order = JsonSerializer.Deserialize<HistogramOrder>(@"""asc""", Options());
			order.Should().BeNull();
		}

		[U] public void Write_Value()
		{
			var json = JsonSerializer.Serialize(new HistogramOrder { Key = "_count", Order = SortOrder.Descending }, Options());
			json.Should().Be(@"{""_count"":""desc""}");
		}

		[U] public void Write_NullValue()
		{
			var json = JsonSerializer.Serialize<HistogramOrder>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Write_NullKey_WritesNull()
		{
			// A non-null value with a null Key must still serialize to JSON null.
			var json = JsonSerializer.Serialize(new HistogramOrder { Key = null, Order = SortOrder.Ascending }, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new HistogramOrder { Key = "_key", Order = SortOrder.Ascending }, options);
			var order = JsonSerializer.Deserialize<HistogramOrder>(json, options);
			order.Should().NotBeNull();
			order.Key.Should().Be("_key");
			order.Order.Should().Be(SortOrder.Ascending);
		}
	}
}
