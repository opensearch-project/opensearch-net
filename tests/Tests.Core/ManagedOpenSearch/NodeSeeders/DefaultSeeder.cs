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
using System.Threading.Tasks;
using OpenSearch.Client;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Domain;

namespace Tests.Core.ManagedOpenSearch.NodeSeeders;

public class DefaultSeeder
{
    public const string CommitsAliasFilter = "commits-only";
    public const string ProjectsAliasFilter = "projects-only";

    public const string ProjectsAliasName = "projects-alias";
    public const string TestsIndexTemplateName = "osc_tests";

    public const string RemoteClusterName = "remote-cluster";

    public const string PipelineName = "osc-pipeline";

    private readonly IIndexSettings _defaultIndexSettings = new IndexSettings()
    {
        NumberOfShards = 2,
        NumberOfReplicas = 0,
    };

    public DefaultSeeder(IOpenSearchClient client, IIndexSettings indexSettings)
    {
        Client = client;
        IndexSettings = indexSettings ?? _defaultIndexSettings;
    }

    public DefaultSeeder(IOpenSearchClient client) : this(client, null) { }

    private IOpenSearchClient Client { get; }

    private IIndexSettings IndexSettings { get; }

    public void SeedNode()
    {
        var alreadySeeded = false;
        if (!TestClient.Configuration.ForceReseed && (alreadySeeded = AlreadySeeded())) return;

        var t = Task.Run(async () => await SeedNodeAsync(alreadySeeded).ConfigureAwait(false));

        t.Wait(TimeSpan.FromSeconds(40));
    }

    public void SeedNodeNoData()
    {
        var alreadySeeded = false;
        if (!TestClient.Configuration.ForceReseed && (alreadySeeded = AlreadySeeded())) return;

        var t = Task.Run(async () => await SeedNodeNoDataAsync(alreadySeeded).ConfigureAwait(false));

        t.Wait(TimeSpan.FromSeconds(40));
    }

    // Sometimes we run against an manually started OpenSearch when
    // writing tests to cut down on cluster startup times.
    // If raw_fields exists assume this cluster is already seeded.
    private bool AlreadySeeded() => Client.Indices.TemplateExists(TestsIndexTemplateName).Exists;

    private async Task SeedNodeAsync(bool alreadySeeded)
    {
        // Ensure a clean slate by deleting everything regardless of whether they may already exist
        await DeleteIndicesAndTemplatesAsync(alreadySeeded).ConfigureAwait(false);
        await ClusterSettingsAsync().ConfigureAwait(false);
        await PutPipeline().ConfigureAwait(false);
        // and now recreate everything
        await CreateIndicesAndSeedIndexDataAsync().ConfigureAwait(false);
    }

    private async Task SeedNodeNoDataAsync(bool alreadySeeded)
    {
        // Ensure a clean slate by deleting everything regardless of whether they may already exist
        await DeleteIndicesAndTemplatesAsync(alreadySeeded).ConfigureAwait(false);
        await ClusterSettingsAsync().ConfigureAwait(false);
        // and now recreate everything
        await CreateIndicesAsync().ConfigureAwait(false);
    }

    public async Task ClusterSettingsAsync()
    {
        var clusterConfiguration = new Dictionary<string, object>()
        {
            { "cluster.routing.use_adaptive_replica_selection", true }
        };

        clusterConfiguration += new RemoteClusterConfiguration
        {
            { RemoteClusterName, "127.0.0.1:9300" }
        };

        var putSettingsResponse = await Client.Cluster.PutSettingsAsync(new ClusterPutSettingsRequest
        {
            Transient = clusterConfiguration
        }).ConfigureAwait(false);

        putSettingsResponse.ShouldBeValid();
    }

    public async Task PutPipeline()
    {
        var putProcessors = await Client.Ingest.PutPipelineAsync(PipelineName, pi => pi
            .Description("A pipeline registered by the OSC test framework")
            .Processors(pp => pp
                .Set<Project>(s => s.Field(p => p.Metadata).Value(new { x = "y" }))
            )
        ).ConfigureAwait(false);
        putProcessors.ShouldBeValid();
    }


    public async Task DeleteIndicesAndTemplatesAsync(bool alreadySeeded)
    {
        var tasks = new List<Task>
        {
            Client.Indices.DeleteAsync(typeof(Project)),
            Client.Indices.DeleteAsync(typeof(Developer)),
            Client.Indices.DeleteAsync(typeof(ProjectPercolation))
        };

        if (alreadySeeded)
            tasks.Add(Client.Indices.DeleteTemplateAsync(TestsIndexTemplateName));

        await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
    }

    private async Task CreateIndicesAndSeedIndexDataAsync()
    {
        await CreateIndicesAsync().ConfigureAwait(false);
        await SeedIndexDataAsync().ConfigureAwait(false);
    }

    public async Task CreateIndicesAsync()
    {
        var indexTemplateResponse = await CreateIndexTemplateAsync().ConfigureAwait(false);
        indexTemplateResponse.ShouldBeValid();

        var tasks = new[]
        {
            CreateProjectIndexAsync(),
            CreateDeveloperIndexAsync(),
            CreatePercolatorIndexAsync(),
        };
        await Task.WhenAll(tasks)
            .ContinueWith(t =>
            {
                foreach (var r in t.Result)
                    r.ShouldBeValid();
            }).ConfigureAwait(false);
    }

    private async Task SeedIndexDataAsync()
    {
        var tasks = new Task[]
        {
            Client.IndexManyAsync(Project.Projects),
            Client.IndexManyAsync(Developer.Developers),
            Client.IndexDocumentAsync(new ProjectPercolation
            {
                Id = "1",
                Query = new MatchAllQuery()
            }),
            Client.BulkAsync(b => b
                .IndexMany(
                    CommitActivity.CommitActivities,
                    (d, c) => d.Document(c).Routing(c.ProjectName)
                )
            ) };
        await Task.WhenAll(tasks).ConfigureAwait(false);
        await Client.Indices.RefreshAsync(Indices.Index(typeof(Project), typeof(Developer), typeof(ProjectPercolation))).ConfigureAwait(false);
    }

    private Task<PutIndexTemplateResponse> CreateIndexTemplateAsync() => Client.Indices.PutTemplateAsync(
        new PutIndexTemplateRequest(TestsIndexTemplateName)
        {
            IndexPatterns = new[] { "*" },
            Settings = IndexSettings
        });

    private Task<CreateIndexResponse> CreateDeveloperIndexAsync() => Client.Indices.CreateAsync(Infer.Index<Developer>(), c => c
        .Map<Developer>(m => m
            .AutoMap()
            .Properties(DeveloperProperties)
        )
    );

#pragma warning disable 618
    private Task<CreateIndexResponse> CreateProjectIndexAsync() => Client.Indices.CreateAsync(typeof(Project), c => c
        .Settings(settings => settings
            .Analysis(ProjectAnalysisSettings)
            .Setting("index.knn", true)
            .Setting("index.knn.algo_param.ef_search", 100))
        .Mappings(ProjectMappings)
        .Aliases(aliases => aliases
            .Alias(ProjectsAliasName)
            .Alias(ProjectsAliasFilter, a => a
                .Filter<Project>(f => f.Term(p => p.Join, Infer.Relation<Project>()))
            )
            .Alias(CommitsAliasFilter, a => a
                .Filter<CommitActivity>(f => f.Term(p => p.Join, Infer.Relation<CommitActivity>()))
            )
        )
    );
#pragma warning restore 618

#pragma warning disable 618
    public static ITypeMapping ProjectMappings(MappingsDescriptor map) => map
        .Map<Project>(ProjectTypeMappings);
#pragma warning restore 618

    public static ITypeMapping ProjectTypeMappings(TypeMappingDescriptor<Project> mapping)
    {
        mapping
            .RoutingField(r => r.Required())
            .AutoMap()
            .Properties(ProjectProperties)
            .Properties<CommitActivity>(props => props
                .Object<Developer>(o => o
                    .AutoMap()
                    .Name(p => p.Committer)
                    .Properties(DeveloperProperties)
                    .Dynamic()
                )
                .Text(t => t
                    .Name(p => p.ProjectName)
                    .Index(false)
                )
            );

        return mapping;
    }

    public static IAnalysis ProjectAnalysisSettings(AnalysisDescriptor analysis)
    {
        analysis
            .TokenFilters(tokenFilters => tokenFilters
                .Shingle("shingle", shingle => shingle
                    .MinShingleSize(2)
                    .MaxShingleSize(4)
                )
            )
            .Analyzers(analyzers => analyzers
                .Custom("shingle", shingle => shingle
                    .Filters("shingle")
                    .Tokenizer("standard")
                )
            )
            .Normalizers(analyzers => analyzers
                .Custom("my_normalizer", n => n
                    .Filters("lowercase", "asciifolding")
                )
            );
        return analysis;
    }


    private Task<CreateIndexResponse> CreatePercolatorIndexAsync() => Client.Indices.CreateAsync(typeof(ProjectPercolation), c => c
        .Settings(s => s
            .AutoExpandReplicas("0-all")
            .Analysis(ProjectAnalysisSettings)
        )
        .Map<ProjectPercolation>(m => m
            .AutoMap()
            .Properties(PercolatedQueryProperties)
        )
    );

    public static PropertiesDescriptor<TProject> ProjectProperties<TProject>(PropertiesDescriptor<TProject> props)
        where TProject : Project
    {
        props
            .Join(j => j
                .Name(n => n.Join)
                .Relations(r => r
                    .Join<Project, CommitActivity>()
                )
            )
            .Keyword(d => d.Name(p => p.Type))
            .Keyword(s => s
                .Name(p => p.Name)
                .Store()
                .Fields(fs => fs
                    .Text(ss => ss
                        .Name("standard")
                        .Analyzer("standard")
                    )
                    .Completion(cm => cm
                        .Name("suggest")
                    )
                )
            )
            .Text(s => s
                .Name(p => p.Description)
                .Fielddata()
                .Fields(f => f
                    .Text(t => t
                        .Name("shingle")
                        .Analyzer("shingle")
                    )
                )
            )
            .Date(d => d
                .Store()
                .Name(p => p.StartedOn)
            )
            .Text(d => d
                .Store()
                .Name(p => p.DateString)
            )
            .Keyword(d => d
                .Name(p => p.State)
                .Fields(fs => fs
                    .Text(st => st
                        .Name("offsets")
                        .IndexOptions(IndexOptions.Offsets)
                    )
                    .Keyword(sk => sk
                        .Name("keyword")
                    )
                )
            )
            .Nested<Tag>(mo => mo
                .AutoMap()
                .Name(p => p.Tags)
                .Properties(TagProperties)
            )
            .Object<Developer>(o => o
                .AutoMap()
                .Name(p => p.LeadDeveloper)
                .Properties(DeveloperProperties)
            )
            .GeoPoint(g => g
                .Name(p => p.LocationPoint)
            )
            .GeoShape(g => g
                .Name(p => p.LocationShape)
            )
            .Completion(cm => cm
                .Name(p => p.Suggest)
                .Contexts(cx => cx
                    .Category(c => c
                        .Name("color")
                    )
                    .GeoLocation(c => c
                        .Name("geo")
                        .Precision("1")
                    )
                )
            )
            .Scalar(p => p.NumberOfCommits, n => n.Store())
            .Scalar(p => p.NumberOfContributors, n => n.Store())
            .Object<Dictionary<string, Metadata>>(o => o
                .Name(p => p.Metadata)
            )
            .RankFeature(rf => rf
                .Name(p => p.Rank)
                .PositiveScoreImpact()
            )
            .KnnVector(k => k
                .Name(p => p.Vector)
                .Dimension(2)
                .Method(m => m
                    .Name("hnsw")
                    .SpaceType("l2")
                    .Engine("nmslib")
                    .Parameters(p => p
                        .Parameter("ef_construction", 128)
                        .Parameter("m", 24)
                    )
                ));

        return props;
    }

    private static PropertiesDescriptor<Tag> TagProperties(PropertiesDescriptor<Tag> props) => props
        .Keyword(s => s
            .Name(p => p.Name)
            .Fields(f => f
                .Text(st => st
                    .Name("vectors")
                    .TermVector(TermVectorOption.WithPositionsOffsetsPayloads)
                )
            )
        );

    public static PropertiesDescriptor<Developer> DeveloperProperties(PropertiesDescriptor<Developer> props) => props
        .Keyword(s => s
            .Name(p => p.OnlineHandle)
        )
        .Keyword(s => s
            .Name(p => p.Gender)
        )
        .Text(s => s
            .Name(p => p.FirstName)
            .TermVector(TermVectorOption.WithPositionsOffsetsPayloads)
        )
        .Ip(s => s
            .Name(p => p.IpAddress)
        )
        .GeoPoint(g => g
            .Name(p => p.Location)
        )
        .Object<GeoIp>(o => o
            .Name(p => p.GeoIp)
        );

    public static PropertiesDescriptor<ProjectPercolation> PercolatedQueryProperties(PropertiesDescriptor<ProjectPercolation> props) =>
        ProjectProperties(props.Percolator(pp => pp.Name(n => n.Query)));
}
