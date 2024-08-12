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
using System.Reflection;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Core.Xunit;
using Tests.Domain;
using static OpenSearch.Client.Infer;

namespace Tests.Ingest;

using ProcFunc = Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>>;

public interface IProcessorAssertion
{
    ProcFunc Fluent { get; }
    IProcessor Initializer { get; }
    object Json { get; }
    string Key { get; }
}

public abstract class ProcessorAssertion : IProcessorAssertion
{
    public abstract ProcFunc Fluent { get; }
    public abstract IProcessor Initializer { get; }
    public abstract object Json { get; }
    public abstract string Key { get; }
}


public static class ProcessorAssertions
{
    public static IEnumerable<IProcessorAssertion> All =>
        from t in typeof(ProcessorAssertions).GetNestedTypes()
        where typeof(IProcessorAssertion).IsAssignableFrom(t) && t.IsClass
        let a = t.GetCustomAttributes(typeof(SkipVersionAttribute)).FirstOrDefault() as SkipVersionAttribute
        where a == null || !a.Ranges.Any(r => r.IsSatisfied(TestClient.Configuration.OpenSearchVersion))
        select (IProcessorAssertion)Activator.CreateInstance(t);

    public static IProcessor[] Initializers => All.Select(a => a.Initializer).ToArray();

    public static Dictionary<string, object>[] AllAsJson =>
        All.Select(a => new Dictionary<string, object>
            {
                { a.Key, a.Json }
            })
            .ToArray();

    public static IPromise<IList<IProcessor>> Fluent(ProcessorsDescriptor d)
    {
        foreach (var a in All) a.Fluent(d);
        return d;
    }

    public class Append : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent =>
            d => d.Append<Project>(a => a.Field(p => p.State).Value(StateOfBeing.Stable, StateOfBeing.VeryActive));

        public override IProcessor Initializer => new AppendProcessor { Field = "state", Value = new object[] { StateOfBeing.Stable, StateOfBeing.VeryActive } };

        public override object Json => new
        {
            field = "state",
            value = new[] { "Stable", "VeryActive" }
        };

        public override string Key => "append";
    }

    public class Csv : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Csv<Project>(c => c
                .Field(p => p.Name)
                .TargetFields(new[] { "targetField1", "targetField2" })
                .EmptyValue("empty")
                .Trim()
                .Description("parses CSV")
            );

        public override IProcessor Initializer => new CsvProcessor
        {
            Field = "name",
            TargetFields = new[] { "targetField1", "targetField2" },
            EmptyValue = "empty",
            Trim = true,
            Description = "parses CSV"
        };

        public override object Json => new
        {
            field = "name",
            target_fields = new[] { "targetField1", "targetField2" },
            empty_value = "empty",
            trim = true,
            description = "parses CSV"
        };

        public override string Key => "csv";
    }

    public class Convert : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Convert<Project>(c => c
                .Field(p => p.NumberOfCommits)
                .TargetField("targetField")
                .Type(ConvertProcessorType.String)
            );

        public override IProcessor Initializer => new ConvertProcessor
        {
            Field = "numberOfCommits",
            TargetField = "targetField",
            Type = ConvertProcessorType.String
        };

        public override object Json => new
        {
            field = "numberOfCommits",
            target_field = "targetField",
            type = "string"
        };

        public override string Key => "convert";
    }

    public class Date : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Date<Project>(dt => dt
                .Field(p => p.StartedOn)
                .TargetField("timestamp")
                .Formats("dd/MM/yyyy hh:mm:ss")
                .TimeZone("Europe/Amsterdam")
            );

        public override IProcessor Initializer => new DateProcessor
        {
            Field = "startedOn",
            TargetField = "timestamp",
            Formats = new[] { "dd/MM/yyyy hh:mm:ss" },
            TimeZone = "Europe/Amsterdam"
        };

        public override object Json => new
        {
            field = "startedOn",
            target_field = "timestamp",
            formats = new[] { "dd/MM/yyyy hh:mm:ss" },
            timezone = "Europe/Amsterdam"
        };

        public override string Key => "date";
    }

    public class Fail : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d.Fail(f => f.Message("an error message"));

        public override IProcessor Initializer => new FailProcessor { Message = "an error message" };

        public override object Json => new { message = "an error message" };
        public override string Key => "fail";
    }

    public class Foreach : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Foreach<Project>(fe => fe
                .Field(p => p.Tags)
                .Processor(pps => pps
                    .Uppercase<Tag>(uc => uc
                        .Field("_value.name")
                    )
                )
            );

        public override IProcessor Initializer => new ForeachProcessor
        {
            Field = Field<Project>(p => p.Tags),
            Processor = new UppercaseProcessor
            {
                Field = "_value.name"
            }
        };

        public override object Json => new
        {
            field = "tags",
            processor = new
            {
                uppercase = new
                {
                    field = "_value.name"
                }
            }
        };

        public override string Key => "foreach";
    }

    public class Grok : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Grok<Project>(gk => gk
                .Field(p => p.Description)
                .Patterns("my %{FAVORITE_DOG:dog} is colored %{RGB:color}")
                .PatternDefinitions(pds => pds
                    .Add("FAVORITE_DOG", "border collie")
                    .Add("RGB", "RED|BLUE|GREEN")
                )
            );

        public override IProcessor Initializer => new GrokProcessor
        {
            Field = "description",
            Patterns = new[] { "my %{FAVORITE_DOG:dog} is colored %{RGB:color}" },
            PatternDefinitions = new Dictionary<string, string>
            {
                { "FAVORITE_DOG", "border collie" },
                { "RGB", "RED|BLUE|GREEN" },
            }
        };

        public override object Json => new
        {
            field = "description",
            patterns = new[] { "my %{FAVORITE_DOG:dog} is colored %{RGB:color}" },
            pattern_definitions = new Dictionary<string, string>
            {
                { "FAVORITE_DOG", "border collie" },
                { "RGB", "RED|BLUE|GREEN" },
            }
        };

        public override string Key => "grok";
    }

    public class Gsub : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Gsub<Project>(gs => gs
                .Field(p => p.Name)
                .Pattern("-")
                .Replacement("_")
            );

        public override IProcessor Initializer => new GsubProcessor
        {
            Field = "name",
            Pattern = "-",
            Replacement = "_"
        };

        public override object Json => new { field = "name", pattern = "-", replacement = "_" };
        public override string Key => "gsub";
    }

    public class Join : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent =>
            d => d.Join<Project>(j => j.Field(p => p.Branches).Separator(","));

        public override IProcessor Initializer => new JoinProcessor
        {
            Field = "branches",
            Separator = ","
        };

        public override object Json => new { field = "branches", separator = "," };
        public override string Key => "join";
    }

    public class Lowercase : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d.Lowercase<Project>(l => l.Field(p => p.Name));

        public override IProcessor Initializer => new LowercaseProcessor { Field = "name" };

        public override object Json => new { field = "name" };
        public override string Key => "lowercase";
    }

    public class Remove : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d.Remove<Project>(r => r.Field(p => p.Field(pp => pp.Suggest)));

        public override IProcessor Initializer => new RemoveProcessor { Field = "suggest" };

        public override object Json => new { field = new[] { "suggest" } };
        public override string Key => "remove";
    }

    public class Rename : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent =>
            d => d.Rename<Project>(rn => rn.Field(p => p.LeadDeveloper).TargetField("projectLead"));

        public override IProcessor Initializer => new RenameProcessor
        {
            Field = "leadDeveloper",
            TargetField = "projectLead"
        };

        public override object Json => new { field = "leadDeveloper", target_field = "projectLead" };
        public override string Key => "rename";
    }

    public class Set : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent =>
            d => d.Set<Project>(s => s.Field(p => p.Name).Value("foo"));

        public override IProcessor Initializer => new SetProcessor { Field = Field<Project>(p => p.Name), Value = "foo" };

        public override object Json => new { field = "name", value = "foo" };
        public override string Key => "set";
    }

    public class Split : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent =>
            d => d.Split<Project>(sp => sp.Field(p => p.Description).Separator("."));

        public override IProcessor Initializer => new SplitProcessor { Field = "description", Separator = "." };

        public override object Json => new { field = "description", separator = "." };
        public override string Key => "split";
    }

    public class Trim : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d.Trim<Project>(t => t.Field(p => p.Name));

        public override IProcessor Initializer => new TrimProcessor { Field = "name" };

        public override object Json => new { field = "name" };
        public override string Key => "trim";
    }

    public class Uppercase : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d.Uppercase<Project>(u => u.Field(p => p.Name));

        public override IProcessor Initializer => new UppercaseProcessor { Field = "name" };

        public override object Json => new { field = "name" };
        public override string Key => "uppercase";
    }

    public class DotExpander : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent =>
            d => d.DotExpander<Project>(de => de.Field("field.withDots"));

        public override IProcessor Initializer => new DotExpanderProcessor { Field = "field.withDots" };

        public override object Json => new { field = "field.withDots" };
        public override string Key => "dot_expander";
    }

    public class Script : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d.Script(s => s.Source("ctx.numberOfCommits++"));

        public override IProcessor Initializer => new ScriptProcessor { Source = "ctx.numberOfCommits++" };

        public override object Json => new { source = "ctx.numberOfCommits++" };
        public override string Key => "script";
    }

    public class UrlDecode : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .UrlDecode<Project>(ud => ud
                .Field(p => p.Description)
                .IgnoreMissing()
            );

        public override IProcessor Initializer => new UrlDecodeProcessor { Field = "description", IgnoreMissing = true };

        public override object Json => new { field = "description", ignore_missing = true };
        public override string Key => "urldecode";
    }

    public class Attachment : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Attachment<Project>(ud => ud
                .Field(p => p.Description)
                .IndexedCharacters(100_000)
                .Properties("title", "author")
                .IgnoreMissing()
            );

        public override IProcessor Initializer => new AttachmentProcessor
        {
            Field = "description",
            Properties = new[] { "title", "author" },
            IndexedCharacters = 100_000,
            IgnoreMissing = true
        };

        public override object Json => new
        {
            field = "description",
            ignore_missing = true,
            properties = new[] { "title", "author" },
            indexed_chars = 100_000,
        };

        public override string Key => "attachment";
    }

    public class Bytes : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Bytes<Project>(ud => ud
                .Field(p => p.Description)
                .IgnoreMissing()
            );

        public override IProcessor Initializer => new BytesProcessor { Field = "description", IgnoreMissing = true };

        public override object Json => new { field = "description", ignore_missing = true };
        public override string Key => "bytes";
    }

    public class Dissect : ProcessorAssertion
    {
        private readonly string _pattern = "%{clientip} %{ident} %{auth} [%{@timestamp}] \"%{verb} %{request} HTTP/%{httpversion}\" %{status} %{size}";

        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Dissect<Project>(ud => ud
                .Field(p => p.Description)
                .IgnoreMissing()
                .Pattern(_pattern)
                .AppendSeparator(" ")
            );

        public override IProcessor Initializer => new DissectProcessor
        {
            Field = "description",
            IgnoreMissing = true,
            Pattern = _pattern,
            AppendSeparator = " "
        };

        public override object Json => new
        {
            field = "description",
            ignore_missing = true,
            pattern = _pattern,
            append_separator = " "
        };
        public override string Key => "dissect";
    }
    public class Drop : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Drop(ud => ud.If("true"));

        public override IProcessor Initializer => new DropProcessor
        {
            If = "true"
        };

        public override object Json => new
        {
            @if = "true"
        };
        public override string Key => "drop";
    }


    public class KeyValue : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Kv<Project>(ud => ud
                .Field(p => p.Description)
                .FieldSplit("_")
                .ValueSplit(" ")
                .IgnoreMissing()
            );

        public override IProcessor Initializer => new KeyValueProcessor
        {
            Field = "description",
            FieldSplit = "_",
            ValueSplit = " ",
            IgnoreMissing = true
        };

        public override object Json => new
        {
            field = "description",
            ignore_missing = true,
            field_split = "_",
            value_split = " "
        };

        public override string Key => "kv";
    }

    public class KeyValueTrimming : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Kv<Project>(ud => ud
                .Field(p => p.Description)
                .FieldSplit("_")
                .ValueSplit(" ")
                .TrimKey("xyz")
                .TrimValue("abc")
                .StripBrackets()
                .IgnoreMissing()
            );

        public override IProcessor Initializer => new KeyValueProcessor
        {
            Field = "description",
            FieldSplit = "_",
            ValueSplit = " ",
            TrimKey = "xyz",
            TrimValue = "abc",
            StripBrackets = true,
            IgnoreMissing = true
        };

        public override object Json => new
        {
            field = "description",
            ignore_missing = true,
            field_split = "_",
            value_split = " ",
            trim_key = "xyz",
            trim_value = "abc",
            strip_brackets = true
        };

        public override string Key => "kv";
    }

    public class Pipeline : ProcessorAssertion
    {
        public override Func<ProcessorsDescriptor, IPromise<IList<IProcessor>>> Fluent => d => d
            .Pipeline(ud => ud
                .ProcessorName("x")
            );

        public override IProcessor Initializer => new PipelineProcessor
        {
            ProcessorName = "x",
        };

        public override object Json => new
        {
            name = "x",
        };

        public override string Key => "pipeline";
    }
}
