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
using System.Net;
using Bogus;
using OpenSearch.Client;
using Tests.Configuration;

namespace Tests.Domain;

public class Ranges
{
    //for deserialization
    public Ranges() { }

    private Ranges(Faker faker)
    {
        Func<bool> r = () => faker.Random.Bool();
        SetDates(faker, r);
        SetDoubles(faker, r);
        SetFloats(faker, r);
        SetIntegers(faker, r);
        SetLongs(faker, r);
        SetIps(faker, r);
    }

    public DateRange Dates { get; set; }
    public DoubleRange Doubles { get; set; }
    public FloatRange Floats { get; set; }

    public static Faker<Ranges> Generator { get; } =
        new Faker<Ranges>()
            .UseSeed(TestConfiguration.Instance.Seed)
            .CustomInstantiator((f) => new Ranges(f));

    public IntegerRange Integers { get; set; }
    public IpAddressRange Ips { get; set; }
    public LongRange Longs { get; set; }

    private void SetDates(Faker faker, Func<bool> r)
    {
        var past = faker.Date.Past(faker.Random.Int(1, 19));
        var future = faker.Date.Future(faker.Random.Int(1, 10), past);
        var d = new DateRange();
        SwapAssign(r(), past, v => d.GreaterThan = v, v => d.GreaterThanOrEqualTo = v);
        SwapAssign(r(), future, v => d.LessThan = v, v => d.LessThanOrEqualTo = v);
        Dates = d;
    }

    private void SetDoubles(Faker faker, Func<bool> r)
    {
        var low = faker.Random.Double(-121, 10000);
        var high = faker.Random.Double(low, Math.Abs(low * 10)) + 2;
        var d = new DoubleRange();
        SwapAssign(r(), low, v => d.GreaterThan = v, v => d.GreaterThanOrEqualTo = v);
        SwapAssign(r(), high, v => d.LessThan = v, v => d.LessThanOrEqualTo = v);
        Doubles = d;
    }

    private void SetFloats(Faker faker, Func<bool> r)
    {
        var low = faker.Random.Float(-2000, 10000);
        var high = faker.Random.Float(low, Math.Abs(low * 10)) + 2;
        var d = new FloatRange();
        SwapAssign(r(), low, v => d.GreaterThan = v, v => d.GreaterThanOrEqualTo = v);
        SwapAssign(r(), high, v => d.LessThan = v, v => d.LessThanOrEqualTo = v);
        Floats = d;
    }

    private void SetIntegers(Faker faker, Func<bool> r)
    {
        var low = faker.Random.Int(-100, 10000);
        var high = faker.Random.Int(low, Math.Abs(low * 10)) + 2;
        var d = new FloatRange();
        SwapAssign(r(), low, v => d.GreaterThan = v, v => d.GreaterThanOrEqualTo = v);
        SwapAssign(r(), high, v => d.LessThan = v, v => d.LessThanOrEqualTo = v);
        Floats = d;
    }

    private void SetLongs(Faker faker, Func<bool> r)
    {
        var low = faker.Random.Long(-100, 10000);
        var high = faker.Random.Long(low, Math.Abs(low * 10)) + 2;
        var d = new LongRange();
        SwapAssign(r(), low, v => d.GreaterThan = v, v => d.GreaterThanOrEqualTo = v);
        SwapAssign(r(), high, v => d.LessThan = v, v => d.LessThanOrEqualTo = v);
        Longs = d;
    }

    private void SetIps(Faker faker, Func<bool> r)
    {
        var low = faker.Internet.Ip();
        var high = faker.Internet.Ip();
        var lowBytes = IPAddress.Parse(low).GetAddressBytes();
        var highBytes = IPAddress.Parse(high).GetAddressBytes();
        for (var i = 0; i < lowBytes.Length; i++)
        {
            var comparison = lowBytes[i].CompareTo(highBytes[i]);
            if (comparison == 0) continue;

            if (comparison > 0)
            {
                var s = low;
                low = high;
                high = s;
            }

            break;
        }
        var d = new IpAddressRange();
        SwapAssign(r(), low, v => d.GreaterThan = v, v => d.GreaterThanOrEqualTo = v);
        SwapAssign(r(), high, v => d.LessThan = v, v => d.LessThanOrEqualTo = v);
        Ips = d;
    }

    private static void SwapAssign<T>(bool b, T value, Action<T> first, Action<T> second)
    {
        if (b) first(value);
        else second(value);
    }
}
