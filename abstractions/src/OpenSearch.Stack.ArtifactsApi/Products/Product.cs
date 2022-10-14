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

using System.Collections.Concurrent;
using OpenSearch.Stack.ArtifactsApi.Platform;

namespace OpenSearch.Stack.ArtifactsApi.Products
{
	public class Product
	{
		private static readonly ConcurrentDictionary<string, Product> CachedProducts =
			new ConcurrentDictionary<string, Product>();

		private static readonly OpenSearchVersion DefaultIncludePlatformSuffix = OpenSearchVersion.From("1.0.0");

		private Product(string productName) => ProductName = productName;

		protected Product(string productName, SubProduct relatedProduct, OpenSearchVersion platformVersionSuffixAfter = null) : this(productName)
		{
			SubProduct = relatedProduct;
			PlatformSuffixAfter = platformVersionSuffixAfter ?? DefaultIncludePlatformSuffix;
		}

		public SubProduct SubProduct { get; }

		public string Moniker => SubProduct?.SubProductName ?? ProductName;

		public string Extension => PlatformDependent ? OsMonikers.CurrentPlatformArchiveExtension() : "zip";

		public string ProductName { get; }

		public bool PlatformDependent => SubProduct?.PlatformDependent ?? true;

		public OpenSearchVersion PlatformSuffixAfter { get; }

		public static Product OpenSearch { get; } = From("opensearch");

		public static Product OpenSearchDashboards { get; } = From("opensearch-dashboards", platformInZipAfter: "1.0.0");

		public static Product From(string product, SubProduct subProduct = null, OpenSearchVersion platformInZipAfter = null) =>
			CachedProducts.GetOrAdd(subProduct == null ? $"{product}" : $"{product}/{subProduct.SubProductName}",
				k => new Product(product, subProduct, platformInZipAfter));

		public static Product OpenSearchPlugin(OpenSearchPlugin plugin) => From("opensearch-plugins", plugin);

		public override string ToString() =>
			SubProduct != null ? $"{ProductName}/{SubProduct.SubProductName}" : ProductName;

		public string PatchDownloadUrl(string downloadUrl) => SubProduct?.PatchDownloadUrl(downloadUrl) ?? downloadUrl;
	}
}
