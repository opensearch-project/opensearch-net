/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using OpenSearch.Net.Extensions;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Internal;

namespace OpenSearch.Client
{
	/// <summary>
	/// Generic formatter for types whose OpenAPI schema declares <c>additionalProperties: true</c>
	/// alongside named properties. Named properties are serialized first (by their
	/// <see cref="DataMemberAttribute"/> wire names) and additional properties are written
	/// as top-level siblings. On deserialization, unrecognized keys are stored in
	/// <see cref="IHasAdditionalProperties.AdditionalProperties"/>.
	/// <para>
	/// <typeparamref name="TConcrete"/> is the concrete class (e.g. <c>Credential</c>);
	/// <typeparamref name="TInterface"/> is the generated interface (e.g. <c>ICredential</c>).
	/// The formatter is placed on the interface via
	/// <c>[JsonFormatter(typeof(OpenObjectFormatter&lt;Concrete, IInterface&gt;))]</c>
	/// so the <see cref="OpenSearch.Net.Utf8Json.Resolvers.AttributeFormatterResolver"/>
	/// returns it for <c>IJsonFormatter&lt;TInterface&gt;</c> lookups.
	/// </para>
	/// </summary>
	internal sealed class OpenObjectFormatter<TConcrete, TInterface> : IJsonFormatter<TInterface>
		where TConcrete : class, TInterface, IHasAdditionalProperties, new()
		where TInterface : class, IHasAdditionalProperties
	{
		private interface IPropertyHandler
		{
			string WireName { get; }
			bool IsNull(TInterface instance);
			void Serialize(ref JsonWriter writer, TInterface instance, IJsonFormatterResolver resolver);
			void Deserialize(ref JsonReader reader, TConcrete instance, IJsonFormatterResolver resolver);
		}

		private sealed class PropertyHandler<TProp> : IPropertyHandler
		{
			private readonly Func<TInterface, TProp> _getter;
			private readonly Action<TConcrete, TProp> _setter;

			public string WireName { get; }

			public PropertyHandler(string wireName, PropertyInfo ifaceProp, PropertyInfo concreteProp)
			{
				WireName = wireName;

				var ifaceParam = Expression.Parameter(typeof(TInterface), "x");
				_getter = Expression.Lambda<Func<TInterface, TProp>>(
					Expression.Property(ifaceParam, ifaceProp), ifaceParam).Compile();

				var concreteParam = Expression.Parameter(typeof(TConcrete), "c");
				var valParam = Expression.Parameter(typeof(TProp), "v");
				_setter = Expression.Lambda<Action<TConcrete, TProp>>(
					Expression.Assign(Expression.Property(concreteParam, concreteProp), valParam),
					concreteParam, valParam).Compile();
			}

			public bool IsNull(TInterface instance) => (object)_getter(instance) == null;

			public void Serialize(ref JsonWriter writer, TInterface instance, IJsonFormatterResolver resolver) =>
				resolver.GetFormatterWithVerify<TProp>().Serialize(ref writer, _getter(instance), resolver);

			public void Deserialize(ref JsonReader reader, TConcrete instance, IJsonFormatterResolver resolver) =>
				_setter(instance, resolver.GetFormatterWithVerify<TProp>().Deserialize(ref reader, resolver));
		}

		private static readonly IPropertyHandler[] _handlers;
		private static readonly AutomataDictionary _propertyIndex;
		private static readonly HashSet<string> _wireNames;

		private static readonly MethodInfo _createHandlerTyped =
			typeof(OpenObjectFormatter<TConcrete, TInterface>)
				.GetMethod(nameof(CreateHandlerTyped), BindingFlags.NonPublic | BindingFlags.Static);

		static OpenObjectFormatter()
		{
			var handlers = new List<IPropertyHandler>();
			var index = new AutomataDictionary();

			foreach (var ifaceProp in typeof(TInterface).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var dm = ifaceProp.GetCustomAttribute<DataMemberAttribute>();
				if (dm == null) continue;

				var concreteProp = typeof(TConcrete).GetProperty(ifaceProp.Name, BindingFlags.Public | BindingFlags.Instance);
				if (concreteProp == null) continue;

				var handler = CreateHandler(dm.Name, ifaceProp, concreteProp);
				index.Add(dm.Name, handlers.Count);
				handlers.Add(handler);
			}

			_handlers = handlers.ToArray();
			_propertyIndex = index;
			_wireNames = new HashSet<string>(handlers.Select(h => h.WireName), StringComparer.Ordinal);
		}

		private static IPropertyHandler CreateHandler(string wireName, PropertyInfo ifaceProp, PropertyInfo concreteProp) =>
			(IPropertyHandler)_createHandlerTyped
				.MakeGenericMethod(ifaceProp.PropertyType)
				.Invoke(null, new object[] { wireName, ifaceProp, concreteProp });

		private static IPropertyHandler CreateHandlerTyped<TProp>(string wireName, PropertyInfo ifaceProp, PropertyInfo concreteProp) =>
			new PropertyHandler<TProp>(wireName, ifaceProp, concreteProp);

		public void Serialize(ref JsonWriter writer, TInterface value, IJsonFormatterResolver formatterResolver)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteBeginObject();
			var count = 0;

			foreach (var handler in _handlers)
			{
				if (handler.IsNull(value)) continue;
				if (count > 0) writer.WriteValueSeparator();
				writer.WritePropertyName(handler.WireName);
				handler.Serialize(ref writer, value, formatterResolver);
				count++;
			}

			var extData = value.AdditionalProperties;
			if (extData != null && extData.Count > 0)
			{
				var objFormatter = formatterResolver.GetFormatterWithVerify<object>();
				foreach (var kvp in extData)
				{
					// Skip keys already serialized by a typed handler to prevent duplicate JSON keys.
					if (_wireNames.Contains(kvp.Key)) continue;
					if (count > 0) writer.WriteValueSeparator();
					writer.WritePropertyName(kvp.Key);
					objFormatter.Serialize(ref writer, kvp.Value, formatterResolver);
					count++;
				}
			}

			writer.WriteEndObject();
		}

		public TInterface Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
		{
			if (reader.ReadIsNull()) return default;

			var instance = new TConcrete();
			var extData = new Dictionary<string, object>();
			instance.AdditionalProperties = extData;

			var objFormatter = formatterResolver.GetFormatterWithVerify<object>();
			var count = 0;

			while (reader.ReadIsInObject(ref count))
			{
				var property = reader.ReadPropertyNameSegmentRaw();
				if (_propertyIndex.TryGetValue(property, out var idx))
					_handlers[idx].Deserialize(ref reader, instance, formatterResolver);
				else
					extData[property.Utf8String()] = objFormatter.Deserialize(ref reader, formatterResolver);
			}

			return instance;
		}
	}
}
