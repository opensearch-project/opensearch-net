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
using System.Linq;
using System.Runtime.Serialization;
using Bogus;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Configuration;
using Tests.Domain.Extensions;
using Tests.Domain.Helpers;

namespace Tests.Domain;

public class ProjectRuntimeFields
{
    [DataMember(Name = "runtime_started_on_day_of_week")]
    public string StartedOnDayOfWeek { get; set; }

    [DataMember(Name = "runtime_thirty_days_after_started")]
    public string ThirtyDaysFromStarted { get; set; }
}

public class Project
{
    public static string TypeName = "project";

    public static string VersionControlConstant = "git";

    public IEnumerable<string> Branches { get; set; }
    public IList<Tag> CuratedTags { get; set; }
    public string DateString { get; set; }
    public string Description { get; set; }

    public static object InstanceAnonymous => TestConfiguration.Instance.Random.SourceSerializer
        ? InstanceAnonymousSourceSerializer
        : InstanceAnonymousDefault;

    public JoinField Join => JoinField.Root<Project>();

    public DateTime LastActivity { get; set; }
    public Developer LeadDeveloper { get; set; }
    public SimpleGeoPoint LocationPoint { get; set; }
    public IGeoShape LocationShape { get; set; }
    public Dictionary<string, Metadata> Metadata { get; set; }
    public string Name { get; set; }
    public int? NumberOfCommits { get; set; }
    public int NumberOfContributors { get; set; }

    public Ranges Ranges { get; set; }
    public int? Rank { get; set; }
    public int? RequiredBranches => Branches?.Count();

    public SourceOnlyObject SourceOnly { get; set; }
    public DateTime StartedOn { get; set; }
    public StateOfBeing State { get; set; }
    public CompletionField Suggest { get; set; }
    public IEnumerable<Tag> Tags { get; set; }

    public string Type => TypeName;

    //the first applies when using internal source serializer the latter when using JsonNetSourceSerializer
    public Visibility Visibility { get; set; }

    public string VersionControl { get; set; }

    public float[] Vector { get; set; }

    // @formatter:off — enable formatter after this line
    public static Faker<Project> Generator { get; } =
        new Faker<Project>()
            .UseSeed(TestConfiguration.Instance.Seed)
            .RuleFor(p => p.Name, f => f.Person.Company.Name + f.UniqueIndex.ToString())
            .RuleFor(p => p.Description, f => f.Lorem.Paragraphs())
            .RuleFor(p => p.State, f => f.PickRandom<StateOfBeing>())
            .RuleFor(p => p.Visibility, f => f.PickRandom<Visibility>())
            .RuleFor(p => p.StartedOn, p => p.Date.Past())
            .RuleFor(p => p.DateString, (p, d) => d.StartedOn.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"))
            .RuleFor(p => p.LastActivity, p => p.Date.Recent())
            .RuleFor(p => p.LeadDeveloper, p => Developer.Developers[Gimme.Random.Number(0, Developer.Developers.Count - 1)])
            .RuleFor(p => p.Tags, f => Tag.Generator.Generate(Gimme.Random.Number(2, 10)))
            .RuleFor(p => p.CuratedTags, f => Tag.Generator.Generate(Gimme.Random.Number(1, 5)))
            .RuleFor(p => p.LocationPoint, f => SimpleGeoPoint.Generator.Generate())
            .RuleFor(p => p.LocationShape, f => new PointGeoShape(new GeoCoordinate(f.Address.Latitude(), f.Address.Latitude())))
            .RuleFor(p => p.NumberOfCommits, f => Gimme.Random.Number(1, 1000))
            .RuleFor(p => p.NumberOfContributors, f => Gimme.Random.Number(1, 50))
            .RuleFor(p => p.Ranges, f => Ranges.Generator.Generate())
            .RuleFor(p => p.Rank, f => Gimme.Random.Number(1, 100))
            .RuleFor(p => p.Branches, f => Gimme.Random.ListItems(new List<string> { "main", "dev", "release", "qa", "test" }))
            .RuleFor(p => p.SourceOnly, f =>
                TestConfiguration.Instance.Random.SourceSerializer ? new SourceOnlyObject() : null
            )
            .RuleFor(p => p.Suggest, f => new CompletionField
            {
                Input = new[] { f.Person.Company.Name },
                Contexts = new Dictionary<string, IEnumerable<string>>
                {
                    { "color", new[] { "red", "blue", "green", "violet", "yellow" }.Take(Gimme.Random.Number(1, 4)) }
                }
            })
            .RuleFor(p => p.VersionControl, VersionControlConstant)
            .RuleFor(p => p.Vector, f => new[] { Gimme.Random.Float(0f, 5f), Gimme.Random.Float(0f, 5f) });

    public static IList<Project> Projects { get; } = Generator.Clone().Generate(100);

    public static Project First { get; } = Projects.First();

    public static readonly Project Instance = new Project
    {
        Name = First.Name,
        LeadDeveloper = new Developer() { FirstName = "Martijn", LastName = "Laarman" },
        StartedOn = new DateTime(2015, 1, 1),
        DateString = new DateTime(2015, 1, 1).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"),
        LocationPoint = new SimpleGeoPoint { Lat = 42.1523, Lon = -80.321 },
        SourceOnly = TestConfiguration.Instance.Random.SourceSerializer ? new SourceOnlyObject() : null
    };

    private static readonly object InstanceAnonymousDefault = new
    {
        name = First.Name,
        type = TypeName,
        join = Instance.Join.ToAnonymousObject(),
        state = "BellyUp",
        visibility = "Public",
        startedOn = "2015-01-01T00:00:00",
        lastActivity = "0001-01-01T00:00:00",
        numberOfContributors = 0,
        dateString = new DateTime(2015, 1, 1).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"),
        leadDeveloper = new { gender = "Male", id = 0, firstName = "Martijn", lastName = "Laarman" },
        locationPoint = new { lat = Instance.LocationPoint.Lat, lon = Instance.LocationPoint.Lon }
    };

    private static readonly object InstanceAnonymousSourceSerializer = new
    {
        name = First.Name,
        type = TypeName,
        join = Instance.Join.ToAnonymousObject(),
        state = "BellyUp",
        visibility = "Public",
        startedOn = "2015-01-01T00:00:00",
        lastActivity = "0001-01-01T00:00:00",
        numberOfContributors = 0,
        dateString = new DateTime(2015, 1, 1).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"),
        leadDeveloper = new { gender = "Male", id = 0, firstName = "Martijn", lastName = "Laarman" },
        locationPoint = new { lat = Instance.LocationPoint.Lat, lon = Instance.LocationPoint.Lon },
        sourceOnly = new { notWrittenByDefaultSerializer = "written" }
    };


    // @formatter:on — enable formatter after this line
}

//the first applies when using internal source serializer the latter when using JsonNetSourceSerializer
[StringEnum]
public enum StateOfBeing
{
    BellyUp,
    Stable,
    VeryActive
}

[StringEnum]
public enum Visibility
{
    Public,
    Private
}

public class Metadata
{
    public DateTime Created { get; set; }
}

public class ProjectTransform
{
    public double? AverageCommits { get; set; }

    public long WeekStartedOnMillis { get; set; }

    public DateTime WeekStartedOnDate { get; set; }

    public long SumIntoMain { get; set; }
}
