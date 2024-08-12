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

namespace Tests.IndexModules.Similarity;

public class SimilaritySettings
{
    /**
		 */

    public class Usage : PromiseUsageTestBase<ISimilarities, SimilaritiesDescriptor, Similarities>
    {
        protected override object ExpectJson => new
        {
            bm25 = new
            {
                k1 = 1.1,
                b = 1.0,
                discount_overlaps = true,
                type = "BM25"
            },
            dfi = new
            {
                independence_measure = "chisquared",
                type = "DFI"
            },
            dfr = new Dictionary<string, object>
            {
                { "basic_model", "d" },
                { "after_effect", "b" },
                { "normalization", "h1" },
                { "normalization.h1.c", 1.1 },
                { "type", "DFR" }
            },
            ib = new Dictionary<string, object>
            {
                { "distribution", "ll" },
                { "lambda", "df" },
                { "normalization", "h1" },
                { "normalization.h1.c", 1.2 },
                { "type", "IB" }
            },
            lmd = new
            {
                mu = 2,
                type = "LMDirichlet"
            },
            lmj = new
            {
                lambda = 2.0,
                type = "LMJelinekMercer"
            },
            my_name = new
            {
                type = "plugin_sim",
                some_property = "some value"
            },
            scripted_tfidf = new
            {
                type = "scripted",
                script = new
                {
                    source =
                        "double tf = Math.sqrt(doc.freq); double idf = Math.log((field.docCount+1.0)/(term.docFreq+1.0)) + 1.0; double norm = 1/Math.sqrt(doc.length); return query.boost * tf * idf * norm;"
                }
            }
        };

        /**
			 *
			 */
        protected override Func<SimilaritiesDescriptor, IPromise<ISimilarities>> Fluent => s => s
            .BM25("bm25", b => b
                .B(1.0)
                .K1(1.1)
                .DiscountOverlaps()
            )
            .DFI("dfi", d => d
                .IndependenceMeasure(DFIIndependenceMeasure.ChiSquared)
            )
            .DFR("dfr", d => d
                .AfterEffect(DFRAfterEffect.B)
                .BasicModel(DFRBasicModel.D)
                .NormalizationH1(1.1)
            )
            .IB("ib", d => d
                .Lambda(IBLambda.DocumentFrequency)
                .NormalizationH1(1.2)
                .Distribution(IBDistribution.LogLogistic)
            )
            .LMDirichlet("lmd", d => d.Mu(2))
            .LMJelinek("lmj", d => d.Lamdba(2.0))
            .Custom("my_name", "plugin_sim", d => d
                .Add("some_property", "some value")
            )
            .Scripted("scripted_tfidf", sc => sc
                .Script(ssc => ssc
                    .Source(
                        "double tf = Math.sqrt(doc.freq); double idf = Math.log((field.docCount+1.0)/(term.docFreq+1.0)) + 1.0; double norm = 1/Math.sqrt(doc.length); return query.boost * tf * idf * norm;")
                )
            );

        /**
			 */
        protected override Similarities Initializer =>
            new Similarities
            {
                {
                    "bm25", new BM25Similarity
                    {
                        B = 1.0,
                        K1 = 1.1,
                        DiscountOverlaps = true
                    }
                },
                {
                    "dfi", new DFISimilarity
                    {
                        IndependenceMeasure = DFIIndependenceMeasure.ChiSquared
                    }
                },
                {
                    "dfr", new DFRSimilarity
                    {
                        AfterEffect = DFRAfterEffect.B,
                        BasicModel = DFRBasicModel.D,
                        Normalization = Normalization.H1,
                        NormalizationH1C = 1.1
                    }
                },
                {
                    "ib", new IBSimilarity
                    {
                        Distribution = IBDistribution.LogLogistic,
                        Lambda = IBLambda.DocumentFrequency,
                        Normalization = Normalization.H1,
                        NormalizationH1C = 1.2
                    }
                },
                {
                    "lmd", new LMDirichletSimilarity
                    {
                        Mu = 2
                    }
                },
                {
                    "lmj", new LMJelinekMercerSimilarity
                    {
                        Lambda = 2.0
                    }
                },
                {
                    "my_name", new CustomSimilarity("plugin_sim")
                    {
                        { "some_property", "some value" }
                    }
                },
                {
                    "scripted_tfidf", new ScriptedSimilarity
                    {
                        Script = new InlineScript(
                            "double tf = Math.sqrt(doc.freq); double idf = Math.log((field.docCount+1.0)/(term.docFreq+1.0)) + 1.0; double norm = 1/Math.sqrt(doc.length); return query.boost * tf * idf * norm;")
                    }
                }
            };
    }
}
