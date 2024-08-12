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

[StringEnum]
public enum BoundaryScanner
{
    /// <summary>
    /// (default mode for the FVH): allows to configure which characters (boundary_chars) constitute a boundary for highlighting. It’s a single
    /// string with each boundary character defined in it (defaults to .,!? \t\n). It also allows configuring the boundary_max_scan to
    /// control how far to look for boundary characters (defaults to 20). Works only with the Fast Vector Highlighter.
    /// </summary>
    [EnumMember(Value = "chars")]
    Characters,

    /// <summary>
    /// sentence and word: use Java’s BreakIterator to break the highlighted fragments at the next sentence or word boundary.
    /// You can further specify boundary_scanner_locale to control which Locale is used to search the text for these boundaries.
    /// </summary>
    [EnumMember(Value = "sentence")]
    Sentence,

    /// <summary>
    /// sentence and word: use Java’s BreakIterator to break the highlighted fragments at the next sentence or word boundary.
    /// You can further specify boundary_scanner_locale to control which Locale is used to search the text for these boundaries.
    /// </summary>
    [EnumMember(Value = "word")]
    Word
}
