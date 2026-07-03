/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="IProcessor"/>, replacing
	/// the vendored Utf8Json <c>ProcessorFormatter</c> as part of #388. Each processor is a single-property
	/// object <c>{ "&lt;name&gt;": { …body… } }</c>; on write the concrete runtime type is serialized under
	/// its <see cref="IProcessor.Name"/> discriminator, on read the property name selects the concrete
	/// processor type.
	/// </summary>
	internal sealed class ProcessorConverter : JsonConverter<IProcessor>
	{
		private static readonly Dictionary<string, Type> NameToType = new(StringComparer.Ordinal)
		{
			{ "attachment", typeof(AttachmentProcessor) },
			{ "append", typeof(AppendProcessor) },
			{ "convert", typeof(ConvertProcessor) },
			{ "date", typeof(DateProcessor) },
			{ "date_index_name", typeof(DateIndexNameProcessor) },
			{ "dot_expander", typeof(DotExpanderProcessor) },
			{ "fail", typeof(FailProcessor) },
			{ "foreach", typeof(ForeachProcessor) },
			{ "json", typeof(JsonProcessor) },
			{ "user_agent", typeof(UserAgentProcessor) },
			{ "kv", typeof(KeyValueProcessor) },
			{ "geoip", typeof(GeoIpProcessor) },
			{ "grok", typeof(GrokProcessor) },
			{ "gsub", typeof(GsubProcessor) },
			{ "join", typeof(JoinProcessor) },
			{ "lowercase", typeof(LowercaseProcessor) },
			{ "remove", typeof(RemoveProcessor) },
			{ "rename", typeof(RenameProcessor) },
			{ "script", typeof(ScriptProcessor) },
			{ "set", typeof(SetProcessor) },
			{ "sort", typeof(SortProcessor) },
			{ "split", typeof(SplitProcessor) },
			{ "trim", typeof(TrimProcessor) },
			{ "uppercase", typeof(UppercaseProcessor) },
			{ "urldecode", typeof(UrlDecodeProcessor) },
			{ "bytes", typeof(BytesProcessor) },
			{ "dissect", typeof(DissectProcessor) },
			{ "pipeline", typeof(PipelineProcessor) },
			{ "drop", typeof(DropProcessor) },
			{ "csv", typeof(CsvProcessor) },
			{ "uri_parts", typeof(UriPartsProcessor) },
			{ "fingerprint", typeof(FingerprintProcessor) },
			{ "community_id", typeof(NetworkCommunityIdProcessor) },
			{ "network_direction", typeof(NetworkDirectionProcessor) },
			{ "text_embedding", typeof(TextEmbeddingProcessor) },
		};

		public override void Write(Utf8JsonWriter writer, IProcessor value, JsonSerializerOptions options)
		{
			if (value?.Name == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(value.Name);
			// Serialize the concrete runtime type; its [DataMember] body matches what the original
			// formatter produced by serializing the specific processor interface. Does not re-enter this
			// converter (it targets IProcessor, not the concrete type).
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
			writer.WriteEndObject();
		}

		public override IProcessor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) { reader.Skip(); return null; }

			using var document = JsonDocument.ParseValue(ref reader);
			foreach (var member in document.RootElement.EnumerateObject())
			{
				if (NameToType.TryGetValue(member.Name, out var type))
					return (IProcessor)member.Value.Deserialize(type, options);
				break;
			}

			return null;
		}
	}
}
