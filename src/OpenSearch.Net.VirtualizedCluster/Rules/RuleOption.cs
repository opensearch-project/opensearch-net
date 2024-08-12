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

namespace OpenSearch.Net.VirtualizedCluster.Rules;

/// <summary>
/// Represents the union of two types, <typeparamref name="TFirst" /> and <typeparamref name="TSecond" />.
/// Used to represent a rule with multiple return states
/// </summary>
/// <typeparam name="TFirst">The first type</typeparam>
/// <typeparam name="TSecond">The second type</typeparam>
public class RuleOption<TFirst, TSecond>
{
    internal readonly int Tag;
    internal readonly TFirst Item1;
    internal readonly TSecond Item2;

    /// <summary>
    /// Creates an new instance of <see cref="RuleOption{TFirst,TSecond}" /> that encapsulates <paramref name="item" /> value
    /// </summary>
    /// <param name="item">The value to encapsulate</param>
    public RuleOption(TFirst item)
    {
        Item1 = item;
        Tag = 0;
    }

    /// <summary>
    /// Creates an new instance of <see cref="RuleOption{TFirst,TSecond}" /> that encapsulates <paramref name="item" /> value
    /// </summary>
    /// <param name="item">The value to encapsulate</param>
    public RuleOption(TSecond item)
    {
        Item2 = item;
        Tag = 1;
    }

    /// <summary>
    /// Runs an <see cref="Action{T}" /> delegate against the encapsulated value
    /// </summary>
    /// <param name="first">The delegate to run when this instance encapsulates an instance of <typeparamref name="TFirst" /></param>
    /// <param name="second">The delegate to run when this instance encapsulates an instance of <typeparamref name="TSecond" /></param>
    public void Match(Action<TFirst> first, Action<TSecond> second)
    {
        switch (Tag)
        {
            case 0:
                first(Item1);
                break;
            case 1:
                second(Item2);
                break;
            default: throw new Exception($"Unrecognized tag value: {Tag}");
        }
    }

    /// <summary>
    /// Runs a <see cref="Func{T,TResult}" /> delegate against the encapsulated value
    /// </summary>
    /// <param name="first">The delegate to run when this instance encapsulates an instance of <typeparamref name="TFirst" /></param>
    /// <param name="second">The delegate to run when this instance encapsulates an instance of <typeparamref name="TSecond" /></param>
    public T Match<T>(Func<TFirst, T> first, Func<TSecond, T> second)
    {
        switch (Tag)
        {
            case 0: return first(Item1);
            case 1: return second(Item2);
            default: throw new Exception($"Unrecognized tag value: {Tag}");
        }
    }

    public static implicit operator RuleOption<TFirst, TSecond>(TFirst first) => new RuleOption<TFirst, TSecond>(first);

    public static implicit operator RuleOption<TFirst, TSecond>(TSecond second) => new RuleOption<TFirst, TSecond>(second);
}
