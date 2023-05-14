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
	[DataMember(Name = "dimension")]
	int? Dimension { get; set; }

	[DataMember(Name = "model_id")]
	string ModelId { get; set; }

	[DataMember(Name = "method")]
	IKnnMethod Method { get; set; }
}

[ReadAs(typeof(KnnMethod))]
[InterfaceDataContract]
public interface IKnnMethod
{
	[DataMember(Name = "name")]
	string Name { get; set; }

	[DataMember(Name = "engine")]
	string Engine { get; set; }

	[DataMember(Name = "space_type")]
	string SpaceType { get; set; }

	[DataMember(Name = "parameters")]
	IDictionary<string, object> Parameters { get; set; }
}

public class KnnMethod : IKnnMethod
{
	public string Name { get; set; }
	public string Engine { get; set; }
	public string SpaceType { get; set; }
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

	public int? Dimension { get; set; }
	public string ModelId { get; set; }
	public IKnnMethod Method { get; set; }
}

[DebuggerDisplay("{DebugDisplay}")]
public class KnnVectorPropertyDescriptor<T>
	: DocValuesPropertyDescriptorBase<KnnVectorPropertyDescriptor<T>, IKnnVectorProperty, T>, IKnnVectorProperty
	where T : class
{
	public KnnVectorPropertyDescriptor() : base(FieldType.KnnVector) { }

	int? IKnnVectorProperty.Dimension { get; set; }
	string IKnnVectorProperty.ModelId { get; set; }
	IKnnMethod IKnnVectorProperty.Method { get; set; }

	public KnnVectorPropertyDescriptor<T> Dimension(int? dimension) =>
		Assign(dimension, (p, v) => p.Dimension = v);

	public KnnVectorPropertyDescriptor<T> ModelId(string modelId) =>
		Assign(modelId, (p, v) => p.ModelId = v);

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

	public KnnMethodDescriptor Name(string name) =>
		Assign(name, (c, v) => c.Name = v);

	public KnnMethodDescriptor Engine(string engine) =>
		Assign(engine, (c, v) => c.Engine = v);

	public KnnMethodDescriptor SpaceType(string spaceType) =>
		Assign(spaceType, (c, v) => c.SpaceType = v);

	public KnnMethodDescriptor Parameters(Func<KnnMethodParametersDescriptor, IPromise<IKnnMethodParameters>> selector) =>
		Assign(selector, (c, v) => c.Parameters = v?.Invoke(new KnnMethodParametersDescriptor())?.Value);
}

public class KnnMethodParametersDescriptor : IsADictionaryDescriptorBase<KnnMethodParametersDescriptor, IKnnMethodParameters, string, object>
{
	public KnnMethodParametersDescriptor() : base(new KnnMethodParameters()) { }

	public KnnMethodParametersDescriptor Parameter(string name, object value) =>
		Assign(name, value);
}
