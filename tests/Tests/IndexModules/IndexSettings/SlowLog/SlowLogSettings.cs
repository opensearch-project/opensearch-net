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
using OpenSearch.Client;

namespace Tests.IndexModules.IndexSettings.SlowLog;

using OpenSearch.Client;

public class SlowLogSettings
{
    /**
		 */

    public class Usage : PromiseUsageTestBase<IIndexSettings, IndexSettingsDescriptor, IndexSettings>
    {
        protected override object ExpectJson => new Dictionary<string, object>
        {
            { "index.search.slowlog.threshold.query.warn", "10s" },
            { "index.search.slowlog.threshold.query.info", "5s" },
            { "index.search.slowlog.threshold.query.debug", "2s" },
            { "index.search.slowlog.threshold.query.trace", "500ms" },
            { "index.search.slowlog.threshold.fetch.warn", "1s" },
            { "index.search.slowlog.threshold.fetch.info", "800ms" },
            { "index.search.slowlog.threshold.fetch.debug", "500ms" },
            { "index.search.slowlog.threshold.fetch.trace", "200ms" },
            { "index.search.slowlog.level", "info" },
            { "index.indexing.slowlog.threshold.index.warn", "10s" },
            { "index.indexing.slowlog.threshold.index.info", "5s" },
            { "index.indexing.slowlog.threshold.index.debug", "2s" },
            { "index.indexing.slowlog.threshold.index.trace", "500ms" },
            { "index.indexing.slowlog.level", "debug" },
            { "index.indexing.slowlog.source", 100 },
        };

        /**
			 *
			 */
        protected override Func<IndexSettingsDescriptor, IPromise<IIndexSettings>> Fluent => s => s
            .SlowLog(sl => sl
                .Indexing(i => i
                    .ThresholdWarn("10s")
                    .ThresholdInfo("5s")
                    .ThresholdDebug(TimeSpan.FromSeconds(2))
                    .ThresholdTrace(TimeSpan.FromMilliseconds(500))
                    .LogLevel(LogLevel.Debug)
                    .Source(100)
                )
                .Search(search => search
                    .Query(q => q
                        .ThresholdWarn("10s")
                        .ThresholdInfo("5s")
                        .ThresholdDebug(TimeSpan.FromSeconds(2))
                        .ThresholdTrace(TimeSpan.FromMilliseconds(500))
                    )
                    .Fetch(f => f
                        .ThresholdWarn("1s")
                        .ThresholdInfo("800ms")
                        .ThresholdDebug(TimeSpan.FromMilliseconds(500))
                        .ThresholdTrace(TimeSpan.FromMilliseconds(200))
                    )
                    .LogLevel(LogLevel.Info)
                )
            );

        /**
			 */
        protected override IndexSettings Initializer =>
            new()
            {
                SlowLog = new SlowLog
                {
                    Indexing = new SlowLogIndexing
                    {
                        LogLevel = LogLevel.Debug,
                        Source = 100,
                        ThresholdInfo = TimeSpan.FromSeconds(5),
                        ThresholdDebug = "2s",
                        ThresholdTrace = "500ms",
                        ThresholdWarn = TimeSpan.FromSeconds(10)
                    },
                    Search = new SlowLogSearch
                    {
                        LogLevel = LogLevel.Info,
                        Fetch = new SlowLogSearchFetch
                        {
                            ThresholdInfo = TimeSpan.FromMilliseconds(800),
                            ThresholdDebug = "500ms",
                            ThresholdTrace = "200ms",
                            ThresholdWarn = TimeSpan.FromSeconds(1)
                        },
                        Query = new SlowLogSearchQuery
                        {
                            ThresholdInfo = TimeSpan.FromSeconds(5),
                            ThresholdDebug = "2s",
                            ThresholdTrace = "500ms",
                            ThresholdWarn = TimeSpan.FromSeconds(10)
                        }
                    }
                },
            };
    }
}
