/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>SourceOnlyRepositoryFormatter</c>.
	///
	/// A source-only repository is serialized as <c>{ "type": "source", "settings": { "delegate_type": "&lt;t&gt;", ... } }</c>
	/// where the delegate settings body is flattened <em>alongside</em> the <c>delegate_type</c> field inside the
	/// <c>settings</c> object. The delegate settings type is chosen by the runtime <see cref="ISourceOnlyRepository.DelegateType"/>
	/// (<c>s3</c>, <c>azure</c>, <c>url</c>, <c>hdfs</c>, <c>fs</c>; any other value falls back to the base
	/// <see cref="IRepositorySettings"/>). A repository with a null/empty delegate type writes JSON <c>null</c>.
	///
	/// On read, the discriminator lives in the nested <c>settings.delegate_type</c> field. Because
	/// <see cref="Utf8JsonReader"/> is forward-only and the legacy formatter peeked at the settings block twice (once to
	/// find <c>delegate_type</c>, once to deserialize the concrete settings), we buffer into a <see cref="JsonDocument"/>
	/// and read from the DOM. The concrete settings type is then deserialized from the whole <c>settings</c> element
	/// (the extra <c>delegate_type</c> property is ignored, matching the legacy behaviour). When there is no
	/// <c>settings</c> object the result is <c>null</c>; when the delegate type is unrecognised the repository is
	/// returned with a null settings payload.
	/// </summary>
	internal class SourceOnlyRepositoryConverter : JsonConverter<ISourceOnlyRepository>
	{
		public override ISourceOnlyRepository Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			if (!root.TryGetProperty("settings", out var settings) || settings.ValueKind != JsonValueKind.Object)
				return null;

			string delegateType = null;
			if (settings.TryGetProperty("delegate_type", out var delegateTypeElement) &&
				delegateTypeElement.ValueKind == JsonValueKind.String)
				delegateType = delegateTypeElement.GetString();

			object delegateSettings = null;
			switch (delegateType)
			{
				case "s3":
					delegateSettings = settings.Deserialize<S3RepositorySettings>(options);
					break;
				case "azure":
					delegateSettings = settings.Deserialize<AzureRepositorySettings>(options);
					break;
				case "url":
					delegateSettings = settings.Deserialize<ReadOnlyUrlRepositorySettings>(options);
					break;
				case "hdfs":
					delegateSettings = settings.Deserialize<HdfsRepositorySettings>(options);
					break;
				case "fs":
					delegateSettings = settings.Deserialize<FileSystemRepositorySettings>(options);
					break;
			}

			return new SourceOnlyRepository(delegateType, delegateSettings);
		}

		public override void Write(Utf8JsonWriter writer, ISourceOnlyRepository value, JsonSerializerOptions options)
		{
			if (value == null || string.IsNullOrEmpty(value.DelegateType))
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WriteString("type", "source");

			if (value.DelegateSettings != null)
			{
				writer.WritePropertyName("settings");
				writer.WriteStartObject();
				writer.WriteString("delegate_type", value.DelegateType);

				// Flatten the delegate settings body alongside delegate_type, choosing the settings interface by the
				// runtime delegate type (mirrors the legacy write-time dispatch).
				var settingsElement = SerializeSettings(value.DelegateType, value.DelegateSettings, options);
				if (settingsElement.ValueKind == JsonValueKind.Object)
				{
					foreach (var property in settingsElement.EnumerateObject())
						property.WriteTo(writer);
				}

				writer.WriteEndObject();
			}

			writer.WriteEndObject();
		}

		private static JsonElement SerializeSettings(string delegateType, object value, JsonSerializerOptions options)
		{
			switch (delegateType)
			{
				case "s3":
					return JsonSerializer.SerializeToElement(value as IS3RepositorySettings, options);
				case "azure":
					return JsonSerializer.SerializeToElement(value as IAzureRepositorySettings, options);
				case "url":
					return JsonSerializer.SerializeToElement(value as IReadOnlyUrlRepositorySettings, options);
				case "hdfs":
					return JsonSerializer.SerializeToElement(value as IHdfsRepositorySettings, options);
				case "fs":
					return JsonSerializer.SerializeToElement(value as IFileSystemRepositorySettings, options);
				default:
					return JsonSerializer.SerializeToElement(value as IRepositorySettings, options);
			}
		}
	}
}
