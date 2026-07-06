/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ErrorFormatter</c>. Extends
	/// <see cref="ErrorCauseConverter{Error}"/> with the two <see cref="Error"/>-specific fields:
	/// <c>headers</c> and <c>root_cause</c>.
	/// </summary>
	public class ErrorConverter : ErrorCauseConverter<Error>
	{
		// Explicit converter for nested ErrorCause elements, so serialization does not depend on the caller
		// having registered ErrorCauseConverter in the options (which would otherwise fall back to reflection).
		private static readonly ErrorCauseConverter ErrorCause = new ErrorCauseConverter();

		protected override bool ReadExtraField(ref Utf8JsonReader reader, string field, Error value, JsonSerializerOptions options)
		{
			switch (field)
			{
				case "headers":
					value.Headers = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
					return true;
				case "root_cause":
					value.RootCause = ReadErrorCauseList(ref reader, options);
					return true;
				default:
					return false;
			}
		}

		protected override void WriteExtraFields(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
		{
			if (value.Headers != null && HasAny(value.Headers))
			{
				writer.WritePropertyName("headers");
				JsonSerializer.Serialize(writer, value.Headers, options);
			}

			if (value.RootCause != null && HasAny(value.RootCause))
			{
				writer.WritePropertyName("root_cause");
				writer.WriteStartArray();
				foreach (var cause in value.RootCause)
					ErrorCause.Write(writer, cause, options);
				writer.WriteEndArray();
			}
		}

		private static IReadOnlyCollection<ErrorCause> ReadErrorCauseList(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var list = new List<ErrorCause>();
			if (reader.TokenType != JsonTokenType.StartArray)
				throw new JsonException("Expected array while reading root_cause.");

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray)
					break;
				list.Add(ErrorCause.Read(ref reader, typeof(ErrorCause), options));
			}

			return new ReadOnlyCollection<ErrorCause>(list);
		}

		private static bool HasAny<T>(IEnumerable<T> items)
		{
			foreach (var _ in items) return true;
			return false;
		}
	}
}
