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

using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using static Tests.Framework.Extensions.Promisify;

namespace Tests.Analysis;

public class AnalysisCrudTests
    : CrudWithNoDeleteTestBase<CreateIndexResponse, GetIndexSettingsResponse, UpdateIndexSettingsResponse>
{
    public AnalysisCrudTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    /**
		* == Analysis crud
		*
		* In this example we will create an index with analysis settings, read those settings back, update the analysis settings
		* and do another read after the update to assert our new analysis setting is applied.
		* There is NO mechanism to delete an analysis setting in OpenSearch.
		*/
    protected override bool SupportsDeletes => false;

    /**
		* We can create the analysis settings as part of the create index call
		*/
    protected override LazyResponses Create() => Calls<CreateIndexDescriptor, CreateIndexRequest, ICreateIndexRequest, CreateIndexResponse>(
        CreateInitializer,
        CreateFluent,
        (s, c, f) => c.Indices.Create(s, f),
        (s, c, f) => c.Indices.CreateAsync(s, f),
        (s, c, r) => c.Indices.Create(r),
        (s, c, r) => c.Indices.CreateAsync(r)
    );

    protected virtual CreateIndexRequest CreateInitializer(string indexName) => new CreateIndexRequest(indexName)
    {
        Settings = new IndexSettings
        {
            Analysis = new OpenSearch.Client.Analysis
            {
                Analyzers = AnalysisUsageTests.AnalyzersInitializer.Analysis.Analyzers,
                CharFilters = AnalysisUsageTests.CharFiltersInitializer.Analysis.CharFilters,
                Tokenizers = AnalysisUsageTests.TokenizersInitializer.Analysis.Tokenizers,
                TokenFilters = AnalysisUsageTests.TokenFiltersInitializer.Analysis.TokenFilters,
            }
        }
    };

    protected virtual ICreateIndexRequest CreateFluent(string indexName, CreateIndexDescriptor c) =>
        c.Settings(s => s
            .Analysis(a => a
                .Analyzers(t => Promise(AnalysisUsageTests.AnalyzersFluent.Analysis.Analyzers))
                .CharFilters(t => Promise(AnalysisUsageTests.CharFiltersFluent.Analysis.CharFilters))
                .Tokenizers(t => Promise(AnalysisUsageTests.TokenizersFluent.Analysis.Tokenizers))
                .TokenFilters(t => Promise(AnalysisUsageTests.TokenFiltersFluent.Analysis.TokenFilters))
            )
        );


    /**
		* We then read back the analysis settings using `GetIndexSettings()`, you can use this method to get the settings for 1, or many indices in one go
		*/
    protected override LazyResponses Read() =>
        Calls<GetIndexSettingsDescriptor, GetIndexSettingsRequest, IGetIndexSettingsRequest, GetIndexSettingsResponse>(
            GetInitializer,
            GetFluent,
            (s, c, f) => c.Indices.GetSettings(s, f),
            (s, c, f) => c.Indices.GetSettingsAsync(s, f),
            (s, c, r) => c.Indices.GetSettings(r),
            (s, c, r) => c.Indices.GetSettingsAsync(r)
        );

    protected GetIndexSettingsRequest GetInitializer(string indexName) => new GetIndexSettingsRequest(OpenSearch.Client.Indices.Index((IndexName)indexName));

    protected IGetIndexSettingsRequest GetFluent(string indexName, GetIndexSettingsDescriptor u) => u;

    /**
		* Here we assert over the response from `GetIndexSettings()` after the index creation to make sure our analysis chain did infact
		* store our html char filter called `htmls`
		*/
    protected override void ExpectAfterCreate(GetIndexSettingsResponse response)
    {
        response.Indices.Should().NotBeNull().And.HaveCount(1);
        var index = response.Indices.Values.First();
        index.Should().NotBeNull();
        index.Settings.Should().NotBeNull();
        var indexSettings = index.Settings;
        indexSettings.Analysis.Should().NotBeNull();
        indexSettings.Analysis.CharFilters.Should().NotBeNull();

        var firstHtmlCharFilter = indexSettings.Analysis.CharFilters["htmls"];
        firstHtmlCharFilter.Should().NotBeNull();
    }

    /**
		* OpenSearch has an `UpdateIndexSettings()` call but in order to be able to use it you first need to close the index and reopen it afterwards
		*/
    protected override LazyResponses Update() =>
        Calls<UpdateIndexSettingsDescriptor, UpdateIndexSettingsRequest, IUpdateIndexSettingsRequest, UpdateIndexSettingsResponse>(
            UpdateInitializer,
            UpdateFluent,
            (s, c, f) =>
            {
                c.Indices.Close(s);
                var response = c.Indices.UpdateSettings(s, f);
                c.Indices.Open(s);
                return response;
            }
            ,
            async (s, c, f) =>
            {
                c.Indices.Close(s);
                var response = await c.Indices.UpdateSettingsAsync(s, f);
                c.Indices.Open(s);
                return response;
            },
            (s, c, r) =>
            {
                c.Indices.Close(s);
                var response = c.Indices.UpdateSettings(r);
                c.Indices.Open(s);
                return response;
            },
            async (s, c, r) =>
            {
                c.Indices.Close(s);
                var response = await c.Indices.UpdateSettingsAsync(r);
                c.Indices.Open(s);
                return response;
            }
        );

    /**
		* Here we add a new `HtmlStripCharFilter` called `differentHtml`
		*/

    protected UpdateIndexSettingsRequest UpdateInitializer(string indexName) => new UpdateIndexSettingsRequest(indexName)
    {
        IndexSettings = new IndexSettings
        {
            Analysis = new OpenSearch.Client.Analysis
            {
                CharFilters = new OpenSearch.Client.CharFilters { { "differentHtml", new HtmlStripCharFilter() } }
            }
        }
    };

    protected IUpdateIndexSettingsRequest UpdateFluent(string indexName, UpdateIndexSettingsDescriptor u) => u
        .IndexSettings(s => s
            .Analysis(a => a
                .CharFilters(c => c
                    .HtmlStrip("differentHtml")
                )
            )
        );

    /**
		* Here we assert that the `GetIndexSettings()` call after the update sees the newly introduced `differentHmtl` char filter
		*/
    protected override void ExpectAfterUpdate(GetIndexSettingsResponse response)
    {
        response.Indices.Should().NotBeNull().And.HaveCount(1);
        var index = response.Indices.Values.First();
        index.Should().NotBeNull();
        index.Settings.Should().NotBeNull();
        var indexSettings = index.Settings;
        indexSettings.Analysis.Should().NotBeNull();
        indexSettings.Analysis.CharFilters.Should().NotBeNull();
        indexSettings.Analysis.CharFilters.Should().ContainKey("differentHtml");
        var firstHtmlCharFilter = indexSettings.Analysis.CharFilters["differentHtml"];
        firstHtmlCharFilter.Should().NotBeNull();
    }
}
