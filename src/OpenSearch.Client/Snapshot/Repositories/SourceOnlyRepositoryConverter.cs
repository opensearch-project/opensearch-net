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
	/// A <see cref="System.Text.Json"/> converter for <see cref="ISourceOnlyRepository"/>, replacing the
	/// vendored Utf8Json <c>SourceOnlyRepositoryFormatter</c> as part of #388. A source-only repository is
	/// written as <c>{ "type": "source", "settings": { "delegate_type": &lt;type&gt;, …delegate settings… } }</c>:
	/// the delegate repository's settings are flattened into the <c>settings</c> object alongside the
	/// <c>delegate_type</c> discriminator. <see cref="CanConvert"/> matches the concrete implementations so
	/// it applies when the create-repository converter serializes by runtime type.
	/// </summary>
	internal sealed class SourceOnlyRepositoryConverter : JsonConverter<ISourceOnlyRepository>
	{
		public override bool CanConvert(Type typeToConvert) => typeof(ISourceOnlyRepository).IsAssignableFrom(typeToConvert);

		public override void Write(Utf8JsonWriter writer, ISourceOnlyRepository value, JsonSerializerOptions options)
		{
			if (value == null || string.IsNullOrEmpty(value.DelegateType))
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WriteString("type", "source");

			var delegateSettings = ((IRepositoryWithSettings)value).DelegateSettings;
			if (delegateSettings != null)
			{
				writer.WritePropertyName("settings");
				writer.WriteStartObject();
				writer.WriteString("delegate_type", value.DelegateType);

				// Flatten the delegate repository's settings into the same object as delegate_type.
				var bytes = JsonSerializer.SerializeToUtf8Bytes(delegateSettings, delegateSettings.GetType(), options);
				using var document = JsonDocument.Parse(bytes);
				if (document.RootElement.ValueKind == JsonValueKind.Object)
				{
					foreach (var property in document.RootElement.EnumerateObject())
						property.WriteTo(writer);
				}

				writer.WriteEndObject();
			}

			writer.WriteEndObject();
		}

		public override ISourceOnlyRepository Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			if (!root.TryGetProperty("settings", out var settings) || settings.ValueKind != JsonValueKind.Object)
				return null;

			var delegateType = settings.TryGetProperty("delegate_type", out var delegateTypeElement)
				? delegateTypeElement.GetString()
				: null;

			var raw = settings.GetRawText();
			object delegateSettings = delegateType switch
			{
				"s3" => JsonSerializer.Deserialize<S3RepositorySettings>(raw, options),
				"azure" => JsonSerializer.Deserialize<AzureRepositorySettings>(raw, options),
				"url" => JsonSerializer.Deserialize<ReadOnlyUrlRepositorySettings>(raw, options),
				"hdfs" => JsonSerializer.Deserialize<HdfsRepositorySettings>(raw, options),
				"fs" => JsonSerializer.Deserialize<FileSystemRepositorySettings>(raw, options),
				_ => null,
			};

			return new SourceOnlyRepository(delegateType, delegateSettings);
		}
	}
}
