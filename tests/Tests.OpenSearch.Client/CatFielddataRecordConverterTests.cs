/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="CatFielddataRecordConverter"/>, the System.Text.Json replacement for the
	/// legacy Utf8Json <c>CatFielddataRecordFormatter</c>. Reads a <c>_cat/fielddata</c> record object (with column
	/// aliases <c>n</c>→node); serialization is not supported.
	/// </summary>
	public class CatFielddataRecordConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new CatFielddataRecordConverter());
			return options;
		}

		private static CatFielddataRecord Deserialize(string json) =>
			JsonSerializer.Deserialize<CatFielddataRecord>(json, Options());

		[U] public void Read_AllColumns()
		{
			var record = Deserialize(
				@"{""id"":""i"",""node"":""nd"",""host"":""h"",""ip"":""1.2.3.4"",""field"":""f"",""size"":""1kb""}");
			record.Id.Should().Be("i");
			record.Node.Should().Be("nd");
			record.Host.Should().Be("h");
			record.Ip.Should().Be("1.2.3.4");
			record.Field.Should().Be("f");
			record.Size.Should().Be("1kb");
		}

		[U] public void Read_NodeAlias_N()
		{
			var record = Deserialize(@"{""n"":""nodeName""}");
			record.Node.Should().Be("nodeName");
		}

		[U] public void Read_SkipsUnknownColumns()
		{
			var record = Deserialize(@"{""id"":""i"",""unknown"":""zzz""}");
			record.Id.Should().Be("i");
		}

		[U] public void Read_EmptyObject_ReturnsEmptyRecord()
		{
			var record = Deserialize(@"{}");
			record.Should().NotBeNull();
			record.Id.Should().BeNull();
		}

		[U] public void Read_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Write_Throws()
		{
			Action act = () => JsonSerializer.Serialize(new CatFielddataRecord(), Options());
			act.Should().Throw<NotSupportedException>();
		}
	}
}
