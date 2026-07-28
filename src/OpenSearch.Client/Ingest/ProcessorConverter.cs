/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ProcessorFormatter</c>.
	///
	/// An <see cref="IProcessor"/> is polymorphic: the concrete processor object is wrapped by its processor-name key,
	/// e.g. <c>{ "set": { ... } }</c> → <see cref="SetProcessor"/>, <c>{ "grok": { ... } }</c> →
	/// <see cref="GrokProcessor"/>. On read we dispatch on the single wrapping key to the concrete processor type and
	/// deserialize the nested body so no members drop. On write we open the wrapping object, write the processor's
	/// <see cref="IProcessor.Name"/> as the key and serialize the value as its declared processor interface.
	///
	/// Because <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound — unlike the Utf8Json version which
	/// re-read a byte segment — we buffer the value into a <see cref="JsonDocument"/>, read the discriminating key from
	/// the DOM and then deserialize the nested element.
	/// </summary>
	internal class ProcessorConverter : JsonConverter<IProcessor>
	{
		public override IProcessor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			foreach (var property in root.EnumerateObject())
			{
				var body = property.Value;
				switch (property.Name)
				{
					case "attachment": return body.Deserialize<AttachmentProcessor>(options);
					case "append": return body.Deserialize<AppendProcessor>(options);
					case "convert": return body.Deserialize<ConvertProcessor>(options);
					case "date": return body.Deserialize<DateProcessor>(options);
					case "date_index_name": return body.Deserialize<DateIndexNameProcessor>(options);
					case "dot_expander": return body.Deserialize<DotExpanderProcessor>(options);
					case "fail": return body.Deserialize<FailProcessor>(options);
					case "foreach": return body.Deserialize<ForeachProcessor>(options);
					case "json": return body.Deserialize<JsonProcessor>(options);
					case "user_agent": return body.Deserialize<UserAgentProcessor>(options);
					case "kv": return body.Deserialize<KeyValueProcessor>(options);
					case "geoip": return body.Deserialize<GeoIpProcessor>(options);
					case "grok": return body.Deserialize<GrokProcessor>(options);
					case "gsub": return body.Deserialize<GsubProcessor>(options);
					case "join": return body.Deserialize<JoinProcessor>(options);
					case "lowercase": return body.Deserialize<LowercaseProcessor>(options);
					case "remove": return body.Deserialize<RemoveProcessor>(options);
					case "rename": return body.Deserialize<RenameProcessor>(options);
					case "script": return body.Deserialize<ScriptProcessor>(options);
					case "set": return body.Deserialize<SetProcessor>(options);
					case "sort": return body.Deserialize<SortProcessor>(options);
					case "split": return body.Deserialize<SplitProcessor>(options);
					case "trim": return body.Deserialize<TrimProcessor>(options);
					case "uppercase": return body.Deserialize<UppercaseProcessor>(options);
					case "urldecode": return body.Deserialize<UrlDecodeProcessor>(options);
					case "bytes": return body.Deserialize<BytesProcessor>(options);
					case "dissect": return body.Deserialize<DissectProcessor>(options);
					case "pipeline": return body.Deserialize<PipelineProcessor>(options);
					case "drop": return body.Deserialize<DropProcessor>(options);
					case "csv": return body.Deserialize<CsvProcessor>(options);
					case "uri_parts": return body.Deserialize<UriPartsProcessor>(options);
					case "fingerprint": return body.Deserialize<FingerprintProcessor>(options);
					case "community_id": return body.Deserialize<NetworkCommunityIdProcessor>(options);
					case "network_direction": return body.Deserialize<NetworkDirectionProcessor>(options);
					case "text_embedding": return body.Deserialize<TextEmbeddingProcessor>(options);
					default: return null;
				}
			}

			return null;
		}

		public override void Write(Utf8JsonWriter writer, IProcessor value, JsonSerializerOptions options)
		{
			if (value?.Name == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(value.Name);

			switch (value.Name)
			{
				case "attachment": Serialize<IAttachmentProcessor>(writer, value, options); break;
				case "append": Serialize<IAppendProcessor>(writer, value, options); break;
				case "csv": Serialize<ICsvProcessor>(writer, value, options); break;
				case "convert": Serialize<IConvertProcessor>(writer, value, options); break;
				case "date": Serialize<IDateProcessor>(writer, value, options); break;
				case "date_index_name": Serialize<IDateIndexNameProcessor>(writer, value, options); break;
				case "dot_expander": Serialize<IDotExpanderProcessor>(writer, value, options); break;
				case "fail": Serialize<IFailProcessor>(writer, value, options); break;
				case "foreach": Serialize<IForeachProcessor>(writer, value, options); break;
				case "json": Serialize<IJsonProcessor>(writer, value, options); break;
				case "user_agent": Serialize<IUserAgentProcessor>(writer, value, options); break;
				case "kv": Serialize<IKeyValueProcessor>(writer, value, options); break;
				case "geoip": Serialize<IGeoIpProcessor>(writer, value, options); break;
				case "grok": Serialize<IGrokProcessor>(writer, value, options); break;
				case "gsub": Serialize<IGsubProcessor>(writer, value, options); break;
				case "join": Serialize<IJoinProcessor>(writer, value, options); break;
				case "lowercase": Serialize<ILowercaseProcessor>(writer, value, options); break;
				case "remove": Serialize<IRemoveProcessor>(writer, value, options); break;
				case "rename": Serialize<IRenameProcessor>(writer, value, options); break;
				case "script": Serialize<IScriptProcessor>(writer, value, options); break;
				case "set": Serialize<ISetProcessor>(writer, value, options); break;
				case "sort": Serialize<ISortProcessor>(writer, value, options); break;
				case "split": Serialize<ISplitProcessor>(writer, value, options); break;
				case "trim": Serialize<ITrimProcessor>(writer, value, options); break;
				case "uppercase": Serialize<IUppercaseProcessor>(writer, value, options); break;
				case "urldecode": Serialize<IUrlDecodeProcessor>(writer, value, options); break;
				case "bytes": Serialize<IBytesProcessor>(writer, value, options); break;
				case "dissect": Serialize<IDissectProcessor>(writer, value, options); break;
				case "pipeline": Serialize<IPipelineProcessor>(writer, value, options); break;
				case "drop": Serialize<IDropProcessor>(writer, value, options); break;
				case "uri_parts": Serialize<IUriPartsProcessor>(writer, value, options); break;
				case "fingerprint": Serialize<IFingerprintProcessor>(writer, value, options); break;
				case "community_id": Serialize<INetworkCommunityIdProcessor>(writer, value, options); break;
				case "network_direction": Serialize<INetworkDirectionProcessor>(writer, value, options); break;
				case "text_embedding": Serialize<ITextEmbeddingProcessor>(writer, value, options); break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}

			writer.WriteEndObject();
		}

		private static void Serialize<TProcessor>(Utf8JsonWriter writer, IProcessor value, JsonSerializerOptions options)
			where TProcessor : class, IProcessor =>
			JsonSerializer.Serialize(writer, value as TProcessor, options);
	}
}
