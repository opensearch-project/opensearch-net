/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json handling for fluent descriptors deriving from <see cref="DescriptorPromiseBase{TDescriptor,TValue}"/>.
	///
	/// A descriptor holds the value it is building in an internal <c>PromisedValue</c> field and exposes it only through
	/// the explicit <see cref="IPromise{TValue}.Value"/> implementation — it has no public data properties of its own.
	/// The legacy Utf8Json engine serialized the descriptor by walking its (private) members, so the promised value's
	/// shape was written. System.Text.Json serializes only public properties, so a descriptor serialized directly would
	/// otherwise produce <c>{}</c>. This factory detects descriptor types and serializes their promised value instead,
	/// using the value's runtime type so its own converter/contract applies.
	///
	/// In normal request flow fluent methods store the promised value (an interface such as <see cref="IProperties"/>),
	/// not the descriptor, so this only engages when a descriptor is serialized directly.
	/// </summary>
	internal class PromiseConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) => IsPromiseDescriptor(typeToConvert);

		private static bool IsPromiseDescriptor(Type type)
		{
			for (var t = type; t != null && t != typeof(object); t = t.BaseType)
			{
				if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(DescriptorPromiseBase<,>))
					return true;
			}

			return false;
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
			(JsonConverter)Activator.CreateInstance(typeof(PromiseConverter<>).MakeGenericType(typeToConvert));

		private class PromiseConverter<T> : JsonConverter<T>
		{
			// Descriptors are write-only in the serialization pipeline; the concrete value type is deserialized instead.
			public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
				throw new NotSupportedException($"{typeToConvert.Name} is a fluent descriptor and cannot be deserialized.");

			public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
			{
				// IPromise<out TValue> is covariant, so any promised value is reachable as IPromise<object>.
				if (value is IPromise<object> promise && promise.Value != null)
					JsonSerializer.Serialize(writer, promise.Value, promise.Value.GetType(), options);
				else
					writer.WriteNullValue();
			}
		}
	}
}
