/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Diagnostics;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// A wildcard field stores values optimised for wildcard grep-like queries.
	/// </summary>
	[InterfaceDataContract]
	[ReadAs(typeof(WildcardProperty))]
	public interface IWildcardProperty : IDocValuesProperty
	{
		/// <summary>
		/// Accepts a string value which is substituted for any explicit null values.
		/// </summary>
		[DataMember(Name = "null_value")]
		string NullValue { get; set; }

		/// <summary>
		/// The name of the normalizer to use for normalizing the wildcard value.
		/// </summary>
		[DataMember(Name = "normalizer")]
		string Normalizer { get; set; }
	}

	/// <inheritdoc cref="IWildcardProperty"/>
	[DebuggerDisplay("{DebugDisplay}")]
	public class WildcardProperty : DocValuesPropertyBase, IWildcardProperty
	{
		public WildcardProperty() : base(FieldType.Wildcard) { }

		/// <inheritdoc />
		public string NullValue { get; set; }

		/// <inheritdoc />
		public string Normalizer { get; set; }
	}

	/// <inheritdoc cref="IWildcardProperty"/>
	[DebuggerDisplay("{DebugDisplay}")]
	public class WildcardPropertyDescriptor<T>
		: DocValuesPropertyDescriptorBase<WildcardPropertyDescriptor<T>, IWildcardProperty, T>, IWildcardProperty
		where T : class
	{
		public WildcardPropertyDescriptor() : base(FieldType.Wildcard) { }

		string IWildcardProperty.NullValue { get; set; }
		string IWildcardProperty.Normalizer { get; set; }

		/// <inheritdoc cref="IWildcardProperty.NullValue"/>
		public WildcardPropertyDescriptor<T> NullValue(string nullValue) =>
			Assign(nullValue, (a, v) => a.NullValue = v);

		/// <inheritdoc cref="IWildcardProperty.Normalizer"/>
		public WildcardPropertyDescriptor<T> Normalizer(string normalizer) =>
			Assign(normalizer, (a, v) => a.Normalizer = v);
	}
}
