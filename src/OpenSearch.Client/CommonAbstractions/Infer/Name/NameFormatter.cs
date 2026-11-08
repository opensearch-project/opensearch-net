/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// Serializes a <see cref="Name"/> as a JSON string, so it can be used as a body or response property
	/// and not only as a URL path parameter.
	/// </summary>
	internal class NameFormatter : IJsonFormatter<Name>, IObjectPropertyNameFormatter<Name>
	{
		public Name Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
		{
			if (reader.GetCurrentJsonToken() == JsonToken.String)
				return new Name(reader.ReadString());

			reader.ReadNextBlock();
			return null;
		}

		public void Serialize(ref JsonWriter writer, Name value, IJsonFormatterResolver formatterResolver)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteString(value.ToString());
		}

		public Name DeserializeFromPropertyName(ref JsonReader reader, IJsonFormatterResolver formatterResolver) =>
			Deserialize(ref reader, formatterResolver);

		public void SerializeToPropertyName(ref JsonWriter writer, Name value, IJsonFormatterResolver formatterResolver) =>
			Serialize(ref writer, value, formatterResolver);
	}
}
