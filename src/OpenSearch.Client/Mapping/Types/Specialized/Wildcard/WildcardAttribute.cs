/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

namespace OpenSearch.Client
{
	public class WildcardAttribute : OpenSearchDocValuesPropertyAttributeBase, IWildcardProperty
	{
		public WildcardAttribute() : base(FieldType.Wildcard) { }

		public string NullValue
		{
			get => Self.NullValue;
			set => Self.NullValue = value;
		}

		public string Normalizer
		{
			get => Self.Normalizer;
			set => Self.Normalizer = value;
		}

		string IWildcardProperty.NullValue { get; set; }
		string IWildcardProperty.Normalizer { get; set; }
		private IWildcardProperty Self => this;
	}
}
