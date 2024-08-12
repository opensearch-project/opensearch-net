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
using System.Collections.Generic;
using OpenSearch.Net;
using OpenSearch.Net.Extensions;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Resolvers;

namespace OpenSearch.Client;

internal class GetRepositoryResponseFormatter : IJsonFormatter<GetRepositoryResponse>
{
    public GetRepositoryResponse Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var response = new GetRepositoryResponse();
        var repositories = new Dictionary<string, ISnapshotRepository>();
        var count = 0;

        while (reader.ReadIsInObject(ref count))
        {
            var property = reader.ReadPropertyNameSegmentRaw();
            if (ResponseFormatterHelpers.ServerErrorFields.TryGetValue(property, out var errorValue))
            {
                switch (errorValue)
                {
                    case 0:
                        if (reader.GetCurrentJsonToken() == JsonToken.String)
                            response.Error = new Error { Reason = reader.ReadString() };
                        else
                        {
                            var formatter = formatterResolver.GetFormatter<Error>();
                            response.Error = formatter.Deserialize(ref reader, formatterResolver);
                        }
                        break;
                    case 1:
                        if (reader.GetCurrentJsonToken() == JsonToken.Number)
                            response.StatusCode = reader.ReadInt32();
                        else
                            reader.ReadNextBlock();
                        break;
                }
            }
            else
            {
                var name = property.Utf8String();
                var snapshotSegment = reader.ReadNextBlockSegment();
                var snapshotSegmentReader = new JsonReader(snapshotSegment.Array, snapshotSegment.Offset);
                var segmentCount = 0;

                string repositoryType = null;
                ArraySegment<byte> settings = default;

                while (snapshotSegmentReader.ReadIsInObject(ref segmentCount))
                {
                    var propertyName = snapshotSegmentReader.ReadPropertyName();
                    switch (propertyName)
                    {
                        case "type":
                            repositoryType = snapshotSegmentReader.ReadString();
                            break;
                        case "settings":
                            settings = snapshotSegmentReader.ReadNextBlockSegment();
                            break;
                        default:
                            snapshotSegmentReader.ReadNextBlock();
                            break;
                    }
                }

                switch (repositoryType)
                {
                    case "fs":
                        var fs = GetRepository<FileSystemRepository, FileSystemRepositorySettings>(settings, formatterResolver);
                        repositories.Add(name, fs);
                        break;
                    case "url":
                        var url = GetRepository<ReadOnlyUrlRepository, ReadOnlyUrlRepositorySettings>(settings, formatterResolver);
                        repositories.Add(name, url);
                        break;
                    case "azure":
                        var azure = GetRepository<AzureRepository, AzureRepositorySettings>(settings, formatterResolver);
                        repositories.Add(name, azure);
                        break;
                    case "s3":
                        var s3 = GetRepository<S3Repository, S3RepositorySettings>(settings, formatterResolver);
                        repositories.Add(name, s3);
                        break;
                    case "hdfs":
                        var hdfs = GetRepository<HdfsRepository, HdfsRepositorySettings>(settings, formatterResolver);
                        repositories.Add(name, hdfs);
                        break;
                    case "source":
                        // reset the offset
                        snapshotSegmentReader.ResetOffset();
                        var source = formatterResolver.GetFormatter<ISourceOnlyRepository>()
                            .Deserialize(ref snapshotSegmentReader, formatterResolver);
                        repositories.Add(name, source);
                        break;
                }
            }
        }

        response.Repositories = repositories;
        return response;
    }

    public void Serialize(ref JsonWriter writer, GetRepositoryResponse value, IJsonFormatterResolver formatterResolver)
    {
        var formatter = DynamicObjectResolver.ExcludeNullCamelCase.GetFormatter<GetRepositoryResponse>();
        formatter.Serialize(ref writer, value, formatterResolver);
    }

    private TRepository GetRepository<TRepository, TSettings>(ArraySegment<byte> settings, IJsonFormatterResolver formatterResolver)
        where TRepository : ISnapshotRepository
        where TSettings : IRepositorySettings
    {
        if (settings == default)
            return typeof(TRepository).CreateInstance<TRepository>();

        var formatter = formatterResolver.GetFormatter<TSettings>();
        var reader = new JsonReader(settings.Array, settings.Offset);
        var resolvedSettings = formatter.Deserialize(ref reader, formatterResolver);

        return typeof(TRepository).CreateInstance<TRepository>(resolvedSettings);
    }
}
