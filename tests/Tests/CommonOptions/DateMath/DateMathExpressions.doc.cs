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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;
using static Tests.Core.Serialization.SerializationTestHelper;
// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Tests.CommonOptions.DateMath
{
    public class DateMathExpressions
    {
        /**[[date-math-expressions]]
		 * === Date math expressions
		 * The date type supports using date math expression when using it in a query/filter
		 * Whenever durations need to be specified, eg for a timeout parameter, the duration can be specified
		 *
		 * The expression starts with an "anchor" date, which can be either now or a date string (in the applicable format) ending with `||`.
		 * It can be followed by a math expression, supporting `+`, `-` and `/` (rounding).
		 * The units supported are
		 *
		 * - `y` (year)
		 * - `M` (month)
		 * - `w` (week)
		 * - `d` (day)
		 * - `h` (hour)
		 * - `m` (minute)
		 * - `s` (second)
		 *
		 * :datemath: {ref_current}/common-options.html#date-math
		 * Be sure to read the OpenSearch documentation on {datemath}[Date Math].
		 */
        [U]
        public void SimpleExpressions()
        {
            /**
			* ==== Simple expressions
			* You can create simple expressions using any of the static methods on `DateMath`
			*/
            //Expect("now").WhenSerializing(OpenSearch.Client.DateMath.Now);
            Expect("2015-05-05T00:00:00").WhenSerializing(OpenSearch.Client.DateMath.Anchored(new DateTime(2015, 05, 05)));

            /** strings implicitly convert to `DateMath` */
            Expect("now").WhenSerializing<OpenSearch.Client.DateMath>("now");

            /** but are lenient to bad math expressions */
            var nonsense = "now||*asdaqwe";

            /** the resulting date math will assume the whole string is the anchor */
            Expect(nonsense)
                .WhenSerializing<OpenSearch.Client.DateMath>(nonsense)
                .AssertSubject(dateMath => ((IDateMath)dateMath)
                    .Anchor.Match(
                        d => d.Should().NotBe(default(DateTime)),
                        s => s.Should().Be(nonsense)
                    )
                );

            /**`DateTime` also implicitly convert to simple date math expressions; the resulting
			 * anchor will be an actual `DateTime`, even after a serialization/deserialization round trip
			 */
            var date = new DateTime(2015, 05, 05);

            /**
			 * will serialize to
			 */
            //json
            var expected = "2015-05-05T00:00:00";

            // hide
            Expect(expected)
                .WhenSerializing<OpenSearch.Client.DateMath>(date)
                .AssertSubject(dateMath => ((IDateMath)dateMath)
                    .Anchor.Match(
                        d => d.Should().Be(date),
                        s => s.Should().BeNull()
                    )
                );

            /**
			 * When the `DateTime` is local or UTC, the time zone information is included.
			 * For example, for a UTC `DateTime`
			 */
            var utcDate = new DateTime(2015, 05, 05, 0, 0, 0, DateTimeKind.Utc);

            /**
			 * will serialize to
			 */
            //json
            expected = "2015-05-05T00:00:00Z";

            // hide
            Expect(expected)
                .WhenSerializing<OpenSearch.Client.DateMath>(utcDate)
                .AssertSubject(dateMath => ((IDateMath)dateMath)
                    .Anchor.Match(
                        d => d.Should().Be(utcDate),
                        s => s.Should().BeNull()
                    )
                );
        }

        [U]
        public void ComplexExpressions()
        {
            /**
			 * ==== Complex expressions
			* Ranges can be chained on to simple expressions
			*/
            Expect("now+1d").WhenSerializing(
                OpenSearch.Client.DateMath.Now.Add("1d"));

            /** Including multiple operations */
            Expect("now+1d-1m").WhenSerializing(
                OpenSearch.Client.DateMath.Now.Add("1d").Subtract(TimeSpan.FromMinutes(1)));

            /** A rounding value can be chained to the end of the expression, after which no more ranges can be appended */
            Expect("now+1d-1m/d").WhenSerializing(
                OpenSearch.Client.DateMath.Now.Add("1d")
                    .Subtract(TimeSpan.FromMinutes(1))
                    .RoundTo(DateMathTimeUnit.Day));

            /** When anchoring dates, a `||` needs to be appended as clear separator between the anchor and ranges.
			* Again, multiple ranges can be chained
			*/
            Expect("2015-05-05T00:00:00||+1d-1m").WhenSerializing(
                OpenSearch.Client.DateMath.Anchored(new DateTime(2015, 05, 05))
                    .Add("1d")
                    .Subtract(TimeSpan.FromMinutes(1)));
        }

        [U]
        public void FractionalsUnitsAreDroppedToNearestInteger()
        {
            /**
			* ==== Fractional times
			* Date math expressions within OpenSearch do not support fractional numbers. To make working with Date math
			* easier within OSC, conversions from `string`, `TimeSpan` and `double` will convert a fractional value to the
			* largest whole number value and unit, rounded to the nearest second.
			*
			*/
            Expect("now+1w").WhenSerializing(OpenSearch.Client.DateMath.Now.Add(TimeSpan.FromDays(7)));

            Expect("now+1w").WhenSerializing(OpenSearch.Client.DateMath.Now.Add("1w"));

            Expect("now+1w").WhenSerializing(OpenSearch.Client.DateMath.Now.Add(604800000));

            Expect("now+7d").WhenSerializing(OpenSearch.Client.DateMath.Now.Add("7d"));

            Expect("now+30h").WhenSerializing(OpenSearch.Client.DateMath.Now.Add(TimeSpan.FromHours(30)));

            Expect("now+30h").WhenSerializing(OpenSearch.Client.DateMath.Now.Add("1.25d"));

            Expect("now+90001s").WhenSerializing(
                OpenSearch.Client.DateMath.Now.Add(TimeSpan.FromHours(25).Add(TimeSpan.FromSeconds(1))));

            Expect("now+90000s").WhenSerializing(
                OpenSearch.Client.DateMath.Now.Add(TimeSpan.FromHours(25).Add(TimeSpan.FromMilliseconds(1))));

            Expect("now+1y").WhenSerializing(OpenSearch.Client.DateMath.Now.Add("1y"));

            Expect("now+12M").WhenSerializing(OpenSearch.Client.DateMath.Now.Add("12M"));

            Expect("now+18M").WhenSerializing(OpenSearch.Client.DateMath.Now.Add("1.5y"));

            Expect("now+52w").WhenSerializing(OpenSearch.Client.DateMath.Now.Add(TimeSpan.FromDays(7 * 52)));
        }

        [U]
        public void Rounding()
        {
            /**
			 * ==== Rounding
			 * Rounding can be controlled using the constructor, and passing a value for rounding
			 */
            Expect("now+2s").WhenSerializing(
                OpenSearch.Client.DateMath.Now.Add(new DateMathTime("2.5s", MidpointRounding.ToEven)));

            Expect("now+3s").WhenSerializing(
                OpenSearch.Client.DateMath.Now.Add(new DateMathTime("2.5s", MidpointRounding.AwayFromZero)));

            Expect("now+0s").WhenSerializing(
                OpenSearch.Client.DateMath.Now.Add(new DateMathTime(500, MidpointRounding.ToEven)));

            Expect("now+1s").WhenSerializing(
                OpenSearch.Client.DateMath.Now.Add(new DateMathTime(500, MidpointRounding.AwayFromZero)));
        }

        [U]
        public void EqualityAndComparison()
        {
            /**
			 * ==== Equality and Comparisons
			 *
			 * `DateMathTime` supports implements equality and comparison
			 */

            var twoSeconds = new DateMathTime(2, DateMathTimeUnit.Second);
            DateMathTime twoSecondsFromString = "2s";
            DateMathTime twoSecondsFromTimeSpan = TimeSpan.FromSeconds(2);
            DateMathTime twoSecondsFromDouble = 2000;

            twoSeconds.Should().Be(twoSecondsFromString);
            twoSeconds.Should().Be(twoSecondsFromTimeSpan);
            twoSeconds.Should().Be(twoSecondsFromDouble);

            DateMathTime threeSecondsFromString = "3s";
            DateMathTime oneMinuteFromTimeSpan = TimeSpan.FromMinutes(1);

            (threeSecondsFromString > twoSecondsFromString).Should().BeTrue();
            (oneMinuteFromTimeSpan > threeSecondsFromString).Should().BeTrue();

            /**
			 * Since years and months do not
			 * contain exact values
			 *
			 * - A year is approximated to 365 days
			 * - A month is approximated to (365 / 12) days
			 */
            var oneYear = new DateMathTime(1, DateMathTimeUnit.Year);
            DateMathTime oneYearFromString = "1y";
            var twelveMonths = new DateMathTime(12, DateMathTimeUnit.Month);
            DateMathTime twelveMonthsFromString = "12M";

            oneYear.Should().Be(oneYearFromString);
            oneYear.Should().Be(twelveMonths);
            twelveMonths.Should().Be(twelveMonthsFromString);

            var thirteenMonths = new DateMathTime(13, DateMathTimeUnit.Month);
            DateMathTime thirteenMonthsFromString = "13M";
            DateMathTime fiftyTwoWeeks = "52w";

            (oneYear < thirteenMonths).Should().BeTrue();
            (oneYear < thirteenMonthsFromString).Should().BeTrue();
            (twelveMonths > fiftyTwoWeeks).Should().BeTrue();
            (oneYear > fiftyTwoWeeks).Should().BeTrue();
        }
    }
}
