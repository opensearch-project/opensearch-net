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
using OpenSearch.Net.Extensions;

namespace OpenSearch.Net;

/// <summary>
/// A formatter that can utilize <see cref="IConnectionConfigurationValues" /> to resolve <see cref="IUrlParameter" />'s passed
/// as format arguments. It also handles known string representations for e.g bool/Enums/IEnumerable&lt;object&gt;.
/// </summary>
public class OpenSearchUrlFormatter : IFormatProvider, ICustomFormatter
{
    private readonly IConnectionConfigurationValues _settings;

    public OpenSearchUrlFormatter(IConnectionConfigurationValues settings) => _settings = settings;

    public string Format(string format, object arg, IFormatProvider formatProvider)
    {
        if (arg == null) throw new ArgumentNullException();

        if (format == "r") return arg.ToString();

        var value = CreateString(arg, _settings);
        if (value.IsNullOrEmpty() && !format.IsNullOrEmpty())
            throw new ArgumentException($"The parameter: {format} to the url is null or empty");

        return value.IsNullOrEmpty() ? string.Empty : Uri.EscapeDataString(value);
    }

    public object GetFormat(Type formatType) => formatType == typeof(ICustomFormatter) ? this : null;

    public string CreateString(object value) => CreateString(value, _settings);

    public static string CreateString(object value, IConnectionConfigurationValues settings)
    {
        switch (value)
        {
            case null: return null;
            case string s: return s;
            case string[] ss: return string.Join(",", ss);
            case Enum e: return e.GetStringValue();
            case bool b: return b ? "true" : "false";
            case DateTimeOffset offset: return offset.ToString("o");
            case IEnumerable<object> pns:
                return string.Join(",", pns.Select(o => ResolveUrlParameterOrDefault(o, settings)));
            case TimeSpan timeSpan: return timeSpan.ToTimeUnit();
            default:
                return ResolveUrlParameterOrDefault(value, settings);
        }
    }

    private static string ResolveUrlParameterOrDefault(object value, IConnectionConfigurationValues settings) =>
        value is IUrlParameter urlParam ? urlParam.GetString(settings) : value.ToString();
}
