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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Aggregations.Bucket.Children;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.DocumentationTests;
using static OpenSearch.Client.Infer;

namespace Tests.Aggregations;

/**
	*=== Reserved aggregation names
	* OSC uses a heuristics based parser to parse the aggregations back in to typed responses.
	* Because of this, some of the key properties we use to make decisions about parsing are not allowed as key names
	* for aggregations in a request.
	*
	* OpenSearch will at some point get a flag that returns the aggregations in a parsable
	* fashion. when this happens, this limitation will be lifted but until that time, avoid the following names for
	* aggregation keys:
	*/
public class ReservedAggregationNames : DocumentationTestBase
{
    public string[] Reserved => new[]
    {
        "score",
        "value_as_string",
        "keys",
        "max_score"
    };

    //hide
    private TermsAggregation Terms(string name) => new TermsAggregation(name) { Field = "x" };

    //hide
    [U]
    public void ReservedKeyWordsThrow()
    {
        foreach (var key in Reserved)
        {
            ThrowsOn(key, SearchFluent, nameof(SearchFluent));
            ThrowsOn(key, SearchInitializer, nameof(SearchInitializer));
            ThrowsOn(key, DictionaryAddInitializer, nameof(DictionaryAddInitializer));
            ThrowsOn(key, DictionaryConstructor, nameof(DictionaryConstructor));
            ThrowsOn(key, DictionaryImplict, nameof(DictionaryImplict));

            //Container themselves do not throw just their assignment to AggregationDictionary
            DoesNotThrowOn(key, ContainerImplicitConvert, nameof(ContainerImplicitConvert));
        }
    }

    //hide
    [U]
    public void NonReservedKeywordsDoNotThrow()
    {
        foreach (var key in Reserved.Select(r => r + "1"))
        {
            DoesNotThrowOn(key, SearchFluent, nameof(SearchFluent));
            DoesNotThrowOn(key, SearchInitializer, nameof(SearchInitializer));
            DoesNotThrowOn(key, DictionaryAddInitializer, nameof(DictionaryAddInitializer));
            DoesNotThrowOn(key, DictionaryConstructor, nameof(DictionaryConstructor));
            DoesNotThrowOn(key, DictionaryImplict, nameof(DictionaryImplict));

            //Container themselves do not throw just their assignment to AggregationDictionary
            DoesNotThrowOn(key, ContainerImplicitConvert, nameof(ContainerImplicitConvert));
        }
    }

    //hide
    private void SearchFluent(string name) =>
        Client.Search<Project>(s => s.Aggregations(aggs => aggs.Terms(name, t => t.Field("x"))));

    //hide
    private void SearchInitializer(string name) =>
        Client.Search<Project>(new SearchRequest<Project>
        {
            Aggregations = Terms(name)
        });

    //hide
    private void DictionaryAddInitializer(string name) => new AggregationDictionary()
    {
        {name, Terms(name)}
    };

    //hide
    private void DictionaryConstructor(string name)
    {
        var vanilla = new Dictionary<string, AggregationContainer>()
        {
            {name, Terms(name)}
        };
        var dictionary = new AggregationDictionary(vanilla);
    }

    //hide
    private void DictionaryImplict(string name)
    {
        AggregationDictionary vanilla = new Dictionary<string, AggregationContainer>()
        {
            {name, Terms(name)}
        };
    }

    //hide
    private void ContainerImplicitConvert(string name)
    {
        AggregationContainer x = Terms(name);
    }

    //hide
    private void DoesNotThrowOn(string name, Action<string> act, string origin) =>
        act.Invoking(s => s(name)).Should().NotThrow<ArgumentException>(origin);

    //hide
    private void ThrowsOn(string name, Action<string> act, string origin)
    {
        var e = act.Invoking(s => s(name)).Should().Throw<ArgumentException>(origin).Subject.First();
        AssertArgumentException(name, e);
    }

    //hide
    private void AssertArgumentException(string name, ArgumentException e)
    {
        e.Should().NotBeNull();
        e.Message.Should().StartWith($"'{name}' is one of the reserved");
    }
}
