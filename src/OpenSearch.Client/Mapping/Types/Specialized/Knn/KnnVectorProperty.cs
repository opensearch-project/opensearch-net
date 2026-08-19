/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[ReadAs(typeof(KnnVectorProperty))]
[InterfaceDataContract]
public interface IKnnVectorProperty : IDocValuesProperty
{
	/// <summary>
	/// The dimension of the vector.
	/// </summary>
	[DataMember(Name = "dimension")]
	int? Dimension { get; set; }

	/// <summary>
	/// The data type of the vector elements. Defaults to <c>float</c>. Set to <c>byte</c> to store each
	/// dimension as an 8-bit signed integer (<c>[-128, 127]</c>), or <c>binary</c> for binary vectors,
	/// reducing memory and storage requirements.
	/// </summary>
	[DataMember(Name = "data_type")]
	string DataType { get; set; }

	/// <summary>
	/// The vector space used to calculate the distance between vectors. Can be specified at the top level
	/// of the field, as an alternative to specifying it on the <see cref="Method" />.
	/// </summary>
	[DataMember(Name = "space_type")]
	string SpaceType { get; set; }

	/// <summary>
	/// The mode that determines the default configuration of the vector field, balancing between
	/// low latency and low cost (for example, <c>in_memory</c> or <c>on_disk</c>).
	/// </summary>
	[DataMember(Name = "mode")]
	string Mode { get; set; }

	/// <summary>
	/// The compression level applied to the vector field (for example, <c>1x</c>, <c>2x</c>, <c>4x</c>).
	/// Higher compression reduces memory footprint at the cost of some search accuracy.
	/// </summary>
	[DataMember(Name = "compression_level")]
	string CompressionLevel { get; set; }

	/// <summary>
	/// The model to use when the underlying Approximate k-NN algorithm requires a training step.
	/// </summary>
	[DataMember(Name = "model_id")]
	string ModelId { get; set; }

	/// <summary>
	/// The method to use when the underlying Approximate k-NN algorithm does not require training.
	/// </summary>
	[DataMember(Name = "method")]
	IKnnMethod Method { get; set; }
}

[ReadAs(typeof(KnnMethod))]
[InterfaceDataContract]
public interface IKnnMethod
{
	/// <summary>
	/// The identifier for the nearest neighbor method.
	/// </summary>
	[DataMember(Name = "name")]
	string Name { get; set; }

	/// <summary>
	/// The approximate k-NN library to use for indexing and search.
	/// </summary>
	[DataMember(Name = "engine")]
	string Engine { get; set; }

	/// <summary>
	/// The vector space used to calculate the distance between vectors.
	/// </summary>
	[DataMember(Name = "space_type")]
	string SpaceType { get; set; }

	/// <summary>
	/// The parameters used for the nearest neighbor method.
	/// </summary>
	[DataMember(Name = "parameters")]
	IDictionary<string, object> Parameters { get; set; }
}

public class KnnMethod : IKnnMethod
{
	/// <inheritdoc />
	public string Name { get; set; }
	/// <inheritdoc />
	public string Engine { get; set; }
	/// <inheritdoc />
	public string SpaceType { get; set; }
	/// <inheritdoc />
	public IDictionary<string, object> Parameters { get; set; }
}

[InterfaceDataContract]
[JsonFormatter(typeof(VerbatimDictionaryKeysFormatter<KnnMethodParameters, IKnnMethodParameters, string, object>))]
public interface IKnnMethodParameters : IIsADictionary<string, object> { }

public class KnnMethodParameters : IsADictionaryBase<string, object>, IKnnMethodParameters
{
	public KnnMethodParameters() { }

	public KnnMethodParameters(IDictionary<string, object> container) : base(container) { }

	public KnnMethodParameters(Dictionary<string, object> container) : base(container) { }

	public void Add(string name, object value) => BackingDictionary.Add(name, value);
}

[DebuggerDisplay("{DebugDisplay}")]
public class KnnVectorProperty : DocValuesPropertyBase, IKnnVectorProperty
{
	public KnnVectorProperty() : base(FieldType.KnnVector) { }

	/// <inheritdoc />
	public int? Dimension { get; set; }
	/// <inheritdoc />
	public string DataType { get; set; }
	/// <inheritdoc />
	public string SpaceType { get; set; }
	/// <inheritdoc />
	public string Mode { get; set; }
	/// <inheritdoc />
	public string CompressionLevel { get; set; }
	/// <inheritdoc />
	public string ModelId { get; set; }
	/// <inheritdoc />
	public IKnnMethod Method { get; set; }
}

[DebuggerDisplay("{DebugDisplay}")]
public class KnnVectorPropertyDescriptor<T>
	: DocValuesPropertyDescriptorBase<KnnVectorPropertyDescriptor<T>, IKnnVectorProperty, T>, IKnnVectorProperty
	where T : class
{
	public KnnVectorPropertyDescriptor() : base(FieldType.KnnVector) { }

	int? IKnnVectorProperty.Dimension { get; set; }
	string IKnnVectorProperty.DataType { get; set; }
	string IKnnVectorProperty.SpaceType { get; set; }
	string IKnnVectorProperty.Mode { get; set; }
	string IKnnVectorProperty.CompressionLevel { get; set; }
	string IKnnVectorProperty.ModelId { get; set; }
	IKnnMethod IKnnVectorProperty.Method { get; set; }

	/// <inheritdoc cref="IKnnVectorProperty.Dimension" />
	public KnnVectorPropertyDescriptor<T> Dimension(int? dimension) =>
		Assign(dimension, (p, v) => p.Dimension = v);

	/// <inheritdoc cref="IKnnVectorProperty.DataType" />
	public KnnVectorPropertyDescriptor<T> DataType(string dataType) =>
		Assign(dataType, (p, v) => p.DataType = v);

	/// <inheritdoc cref="IKnnVectorProperty.SpaceType" />
	public KnnVectorPropertyDescriptor<T> SpaceType(string spaceType) =>
		Assign(spaceType, (p, v) => p.SpaceType = v);

	/// <inheritdoc cref="IKnnVectorProperty.Mode" />
	public KnnVectorPropertyDescriptor<T> Mode(string mode) =>
		Assign(mode, (p, v) => p.Mode = v);

	/// <inheritdoc cref="IKnnVectorProperty.CompressionLevel" />
	public KnnVectorPropertyDescriptor<T> CompressionLevel(string compressionLevel) =>
		Assign(compressionLevel, (p, v) => p.CompressionLevel = v);

	/// <inheritdoc cref="IKnnVectorProperty.ModelId" />
	public KnnVectorPropertyDescriptor<T> ModelId(string modelId) =>
		Assign(modelId, (p, v) => p.ModelId = v);

	/// <inheritdoc cref="IKnnVectorProperty.Method" />
	public KnnVectorPropertyDescriptor<T> Method(Func<KnnMethodDescriptor, IKnnMethod> selector) =>
		Assign(selector, (p, v) => p.Method = v?.Invoke(new KnnMethodDescriptor()));
}

public class KnnMethodDescriptor
	: DescriptorBase<KnnMethodDescriptor, IKnnMethod>, IKnnMethod
{
	string IKnnMethod.Name { get; set; }
	string IKnnMethod.Engine { get; set; }
	string IKnnMethod.SpaceType { get; set; }
	IDictionary<string, object> IKnnMethod.Parameters { get; set; }

	/// <inheritdoc cref="IKnnMethod.Name" />
	public KnnMethodDescriptor Name(string name) =>
		Assign(name, (c, v) => c.Name = v);

	/// <inheritdoc cref="IKnnMethod.Engine" />
	public KnnMethodDescriptor Engine(string engine) =>
		Assign(engine, (c, v) => c.Engine = v);

	/// <inheritdoc cref="IKnnMethod.SpaceType" />
	public KnnMethodDescriptor SpaceType(string spaceType) =>
		Assign(spaceType, (c, v) => c.SpaceType = v);

	/// <inheritdoc cref="IKnnMethod.Parameters" />
	public KnnMethodDescriptor Parameters(Func<KnnMethodParametersDescriptor, IPromise<IKnnMethodParameters>> selector) =>
		Assign(selector, (c, v) => c.Parameters = v?.Invoke(new KnnMethodParametersDescriptor())?.Value);
}

public class KnnMethodParametersDescriptor : IsADictionaryDescriptorBase<KnnMethodParametersDescriptor, IKnnMethodParameters, string, object>
{
	public KnnMethodParametersDescriptor() : base(new KnnMethodParameters()) { }

	public KnnMethodParametersDescriptor Parameter(string name, object value) =>
		Assign(name, value);
}
