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

using System.Runtime.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client;

/// <summary>
/// Sets the alternate handling for strength quaternary to be either shifted or non-ignorable.
/// Which boils down to ignoring punctuation and whitespace.
/// </summary>
/// <remarks>
/// Requires analysis-icu plugin to be installed
/// </remarks>
[StringEnum]
public enum IcuCollationAlternate
{
    [EnumMember(Value = "shifted")] Shifted,
    [EnumMember(Value = "non-ignorable")] NonIgnorable
}
