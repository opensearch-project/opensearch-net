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

namespace OpenSearch.Client;

public enum GeoHashPrecision
{
    /// <summary>
    /// 5,009.4km x 4,992.6km
    /// </summary>
    Precision1 = 1,

    /// <summary>
    /// 1,252.3km x 624.1km
    /// </summary>
    Precision2 = 2,

    /// <summary>
    /// 156.5km x 156km
    /// </summary>
    Precision3 = 3,

    /// <summary>
    /// 39.1km x 19.5km
    /// </summary>
    Precision4 = 4,

    /// <summary>
    /// 4.9km x 4.9km
    /// </summary>
    Precision5 = 5,

    /// <summary>
    /// 1.2km x 609.4m
    /// </summary>
    Precision6 = 6,

    /// <summary>
    /// 152.9m x 152.4m
    /// </summary>
    Precision7 = 7,

    /// <summary>
    /// 38.2m x 19m
    /// </summary>
    Precision8 = 8,

    /// <summary>
    /// 4.8m x 4.8m
    /// </summary>
    Precision9 = 9,

    /// <summary>
    /// 1.2m x 59.5cm
    /// </summary>
    Precision10 = 10,

    /// <summary>
    /// 14.9cm x 14.9cm
    /// </summary>
    Precision11 = 11,

    /// <summary>
    /// 3.7cm x 1.9cm
    /// </summary>
    Precision12 = 12
}
