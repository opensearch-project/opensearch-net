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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;

namespace Tests.CodeStandards;

public class Descriptors
{
    /**
		* A descriptor is fluent class that is used to build up state and should therefor return itself on each call.
		* Every descriptor should inherit from `DescriptorBase`, this hides object members from the fluent interface
		*/
    [U]
    public void DescriptorsHaveToBeMarkedWithIDescriptor()
    {
        var notDescriptors = new[] { typeof(ClusterProcessOpenFileDescriptors).Name, "DescriptorForAttribute" };

        var descriptors =
            from t in typeof(DescriptorBase<,>).Assembly.Types()
            where t.IsClass
                  && t.Name.Contains("Descriptor")
                  && !t.Namespace.StartsWith("OpenSearch.Client.Json")
                  && !t.Namespace.StartsWith("OpenSearch.Internal")
                  && !notDescriptors.Contains(t.Name)
#if __MonoCS__
					  && !t.FullName.Contains("c__AnonStore") //compiler generated
#endif
                  && t.GetInterfaces().All(i => i != typeof(IDescriptor))
            select t.FullName;
        descriptors.Should().BeEmpty();
    }

    /**
		* A selector is fluent class that is used to return state. Each method should return a completed state.
		* Every selector should inherit from `ISelector`, this hides object members from the fluent interface
		*/
    [U]
    public void SelectorsHaveToBeMarkedWithISelector()
    {
        var notSelectors = new[] { typeof(BucketSelectorAggregationDescriptor).Name, typeof(BucketSelectorAggregation).Name };
        var selectors =
            from t in typeof(SelectorBase).Assembly.Types()
            where t.IsClass
                  && t.Name.Contains("Selector")
                  && !t.Namespace.StartsWith("OpenSearch.Client.Json")
                  && !notSelectors.Contains(t.Name)
#if __MonoCS__
					  && !t.FullName.Contains("c__AnonStore") //compiler generated
#endif
                  && t.GetInterfaces().All(i => i != typeof(ISelector))
            select t.FullName;
        selectors.Should().BeEmpty();
    }

    /**
		* A descriptor is exposed as a selector func, taking a descriptor and returning the completed state.
		* Methods taking a func should have that func return an interface
		*/
    [U]
    public void DescriptorSelectorsReturnInterface()
    {
        var descriptors =
            from t in typeof(DescriptorBase<,>).Assembly.Types()
            where t.IsClass && typeof(IDescriptor).IsAssignableFrom(t)
            select t;

        var exclusions = new Dictionary<Type, Type>
        {
            {typeof(QueryContainerDescriptor<>), typeof(QueryContainer)},
            {typeof(SmoothingModelContainerDescriptor), typeof(SmoothingModelContainer)},
            {typeof(FluentDictionary<,>), typeof(FluentDictionary<,>)},
            {typeof(IntervalsDescriptor), typeof(IntervalsContainer)}
        };

        Func<Type, Type, bool> exclude = (first, second) =>
        {
            var key = first.IsGenericType
                ? first.GetGenericTypeDefinition()
                : first;

            if (!exclusions.TryGetValue(key, out var value))
                return false;

            return second.IsGenericType
                ? second.GetGenericTypeDefinition() == value
                : value.IsAssignableFrom(second);
        };

        var selectorMethods =
            from d in descriptors
            from m in d.GetMethods()
            let parameters = m.GetParameters()
            from p in parameters
            let type = p.ParameterType
            let isGeneric = type.IsGenericType
            where isGeneric
            let isFunc = type.GetGenericTypeDefinition() == typeof(Func<,>)
            where isFunc
            let firstFuncArg = type.GetGenericArguments().First()
            let secondFuncArg = type.GetGenericArguments().Last()
            where !exclude(firstFuncArg, secondFuncArg)
            let lastArgIsNotInterface = !secondFuncArg.IsInterface
            where lastArgIsNotInterface
            select $"{m.Name} on {m.DeclaringType.Name}";

        selectorMethods.Should().BeEmpty();
    }

    /**
		 * Descriptor methods that assign to a nullable bool property should accept
		 * a nullable bool with a default value
		 */
    [U]
    public void DescriptorMethodsAcceptNullableBoolsForQueriesWithNullableBoolProperties()
    {
        var queries =
            from t in typeof(IQuery).Assembly.Types()
            where t.IsInterface && typeof(IQuery).IsAssignableFrom(t)
            where t.GetProperties().Any(p => p.PropertyType == typeof(bool?))
            select t;

        var descriptors =
            from t in typeof(DescriptorBase<,>).Assembly.Types()
            where t.IsClass && typeof(IDescriptor).IsAssignableFrom(t)
            where t.GetInterfaces().Intersect(queries).Any()
            select t;

        var breakingDescriptors = new List<string>();

        foreach (var query in queries)
        {
            var descriptor = descriptors.First(d => query.IsAssignableFrom(d));
            foreach (var boolProperty in query.GetProperties().Where(p => p.PropertyType == typeof(bool?)))
            {
                var descriptorMethod = descriptor.GetMethod(boolProperty.Name) ?? throw new Exception($"No method for property {boolProperty.Name} on {descriptor.Name}");
                var parameters = descriptorMethod.GetParameters();

                if (!parameters.Any())
                    throw new Exception($"No parameter for method {descriptorMethod.Name} on {descriptor.Name}");

                if (parameters.Length > 1)
                    throw new Exception($"More than one parameter for method {descriptorMethod.Name} on {descriptor.Name}");

                if (parameters[0].ParameterType != typeof(bool?))
                    breakingDescriptors.Add($"{descriptor.FullName} method {descriptorMethod.Name} does not take nullable bool");

                if (!parameters[0].HasDefaultValue)
                    breakingDescriptors.Add($"{descriptor.FullName} method {descriptorMethod.Name} does not have a default value");
            }
        }

        breakingDescriptors.Should().BeEmpty();
    }

    [U]
    public void ProcessorImplementationsNeedProcessorInTheirNames()
    {

        var processors = (
            from t in typeof(IProcessor).Assembly.Types()
            where typeof(IProcessor).IsAssignableFrom(t)
            select t.Name).ToList();

        processors.Should().NotBeEmpty($"expected {nameof(IProcessor)} implementations");
        processors.Should().OnlyContain(p => p.Contains("Processor"));
    }

    [U]
    public void DescriptorMethodsTakingSingleValueTypeShouldBeNullable()
    {
        var methods = from d in YieldAllDescriptors()
                      from m in d.GetMethods()
                      let ps = m.GetParameters()
                      where ps.Length == 1 && ps.Any(pp => pp.ParameterType.IsValueType)
                      let p = ps.First()
                      let pt = p.ParameterType
                      where (!pt.IsGenericType || pt.GetGenericTypeDefinition() != typeof(Nullable<>))
                      let dt = m.DeclaringType.IsGenericType ? m.DeclaringType.GetGenericTypeDefinition() : m.DeclaringType

                      //skips
                      where !(new[] { "metric", "indexMetric" }.Contains(p.Name))
                      where !(m.Name == "Interval" && d == typeof(DateHistogramAggregationDescriptor<>))
                      where !(m.Name == "CalendarInterval" && d == typeof(DateHistogramAggregationDescriptor<>))
                      where !(m.Name == "FixedInterval" && d == typeof(DateHistogramAggregationDescriptor<>))
                      where !(m.Name == "Lang" && dt == typeof(ScriptDescriptorBase<,>))
                      where !(m.Name == "Lang" && dt == typeof(StoredScriptDescriptor))
                      where !(m.Name == "Lang" && dt == typeof(ScriptQueryDescriptor<>))
                      where !(m.Name == nameof(BulkAllDescriptor<object>.RefreshOnCompleted) && dt == typeof(BulkAllDescriptor<>))
                      where !(m.Name == nameof(BulkAllDescriptor<object>.ContinueAfterDroppedDocuments) && dt == typeof(BulkAllDescriptor<>))
                      where !(m.Name == nameof(ReindexDescriptor<object, object>.OmitIndexCreation) && dt == typeof(ReindexDescriptor<,>))
                      where !(m.Name == nameof(PutMappingDescriptor<object>.AutoMap))
                      where !(m.Name == nameof(PutMappingDescriptor<object>.Dynamic))
                      where !(m.Name == "Strict" && dt == typeof(QueryDescriptorBase<,>))
                      where !(m.Name == "Verbatim" && dt == typeof(QueryDescriptorBase<,>))
                      where !(m.Name == nameof(FunctionScoreQueryDescriptor<object>.ConditionlessWhen) && dt == typeof(FunctionScoreQueryDescriptor<>))
                      where !(m.Name == nameof(ScoreFunctionsDescriptor<object>.RandomScore) && dt == typeof(ScoreFunctionsDescriptor<>))
                      where !(m.Name == nameof(HighlightFieldDescriptor<object>.Type) && dt == typeof(HighlightFieldDescriptor<>))
                      where !(m.Name == nameof(InnerHitsDescriptor<object>.Source) && dt == typeof(InnerHitsDescriptor<>))
                      where !(m.Name == nameof(SearchDescriptor<object>.Source) && dt == typeof(SearchDescriptor<>))
                      where !(m.Name == nameof(ScoreFunctionsDescriptor<object>.Weight) && dt == typeof(ScoreFunctionsDescriptor<>))
                      where !(m.Name == nameof(SortDescriptor<object>.Ascending) && dt == typeof(SortDescriptor<>))
                      where !(m.Name == nameof(SortDescriptor<object>.Descending) && dt == typeof(SortDescriptor<>))
                      where !(m.Name == nameof(ClrTypeMappingDescriptor<object>.DisableIdInference) && dt == typeof(ClrTypeMappingDescriptor<>))
                      where !(m.Name == nameof(ClrTypeMappingDescriptor.DisableIdInference) && dt == typeof(ClrTypeMappingDescriptor))
                      where !(m.Name == nameof(RankFeatureLogarithmFunctionDescriptor.ScalingFactor) && dt == typeof(RankFeatureLogarithmFunctionDescriptor))
                      where !(m.Name == nameof(RankFeatureSigmoidFunctionDescriptor.Exponent) && dt == typeof(RankFeatureSigmoidFunctionDescriptor))
                      where !(m.Name == nameof(RankFeatureSigmoidFunctionDescriptor.Pivot) && dt == typeof(RankFeatureSigmoidFunctionDescriptor))

                      select new { m, d, p };

        var breakingDescriptors = new List<string>();

        foreach (var info in methods)
        {
            var m = info.m;
            var d = info.d;
            var p = info.p;

            breakingDescriptors.Add($"{p.Name} on method {m.Name} of {d.FullName} is not nullable");
        }

        breakingDescriptors.Should().BeEmpty();
    }

    [U]
    public void NullableBooleansShouldDefaultToTrue()
    {
        var methods = from d in YieldAllDescriptors()
                      from m in d.GetMethods()
                      let ps = m.GetParameters()
                      where ps.Length == 1 && ps.Any(pp => pp.ParameterType.IsValueType)
                      let p = ps.First()
                      let pt = p.ParameterType
                      where pt == typeof(bool?)
                      let dt = m.DeclaringType.IsGenericType ? m.DeclaringType.GetGenericTypeDefinition() : m.DeclaringType
                      where !(m.Name == nameof(BooleanPropertyDescriptor<object>.NullValue) && dt == typeof(BooleanPropertyDescriptor<>))
                      select new { m, d, p };

        var nullableBools = new List<string>();
        foreach (var info in methods)
        {
            var m = info.m;
            var d = info.d;
            var p = info.p;
            if (!p.HasDefaultValue)
                nullableBools.Add($"bool {p.Name} on method {m.Name} of {d.FullName} is has no default value");

            try
            {

                var b = ((bool?)p.RawDefaultValue);
                if (!b.HasValue)
                    nullableBools.Add($"bool {p.Name} on method {m.Name} of {d.FullName} defaults to null");
                else if (!b.Value)
                    nullableBools.Add($"bool {p.Name} on method {m.Name} of {d.FullName} default to false");
            }
            catch
            {
                nullableBools.Add($"bool {p.Name} on method {m.Name} of {d.FullName} defaults to unknown");
            }
        }
        nullableBools.Should().BeEmpty();

    }

    private static IEnumerable<Type> YieldAllDescriptors()
    {
        var descriptors =
            from t in typeof(DescriptorBase<,>).Assembly.Types()
            where t.IsClass && typeof(IDescriptor).IsAssignableFrom(t)
            where !t.IsAbstract
            select t;
        return descriptors;
    }

    //TODO methods taking params should also have a version taking IEnumerable

    //TODO methods named Index or Indices that

    //TODO some interfaces are implemented by both requests as well isolated classes to be used elsewhere in the DSL
    //We need to write tests that these have the same public methods so we do not accidentally add it without adding it to the interface

    //Write a tests that all properties of type QueryContainer have the same json converters set

    //TODO write tests that request expose QueryContainer/AggregationContainer not their interfaces
}
