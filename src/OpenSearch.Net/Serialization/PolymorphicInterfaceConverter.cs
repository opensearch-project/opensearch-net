/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Net
{
	/// <summary>
	/// A reusable <see cref="System.Text.Json"/> converter for the client's many "single object with
	/// a <c>type</c> discriminator" polymorphic hierarchies (tokenizers, char filters, token filters,
	/// analyzers, normalizers, …), replacing the per-family hand-written Utf8Json <c>*Formatter</c>
	/// dispatchers as part of the migration tracked by #388.
	/// <para>
	/// A concrete family only supplies the discriminator → concrete-type map; all read/write logic is
	/// shared here. This is the shape the converter generator emits per family, so adding a namespace
	/// is a table, not new dispatch code.
	/// </para>
	/// <list type="bullet">
	/// <item><b>Write</b> serializes the concrete runtime type. Because that type is not
	/// <typeparamref name="TInterface"/>, this does not re-enter the converter.</item>
	/// <item><b>Read</b> buffers the object, reads the discriminator property, and deserializes into
	/// the mapped concrete type (property names resolved by <see cref="DataContractResolver"/>).</item>
	/// </list>
	/// </summary>
	/// <typeparam name="TInterface">The polymorphic base interface (for example <c>ITokenizer</c>).</typeparam>
	public abstract class PolymorphicInterfaceConverter<TInterface> : JsonConverter<TInterface>
		where TInterface : class
	{
		private readonly IReadOnlyDictionary<string, Type> _typeByDiscriminator;
		private readonly string _discriminatorPropertyName;

		/// <summary>
		/// Creates the converter.
		/// </summary>
		/// <param name="typeByDiscriminator">
		/// Maps each wire discriminator value to the concrete type to (de)serialize.
		/// </param>
		/// <param name="discriminatorPropertyName">The discriminator property name; defaults to <c>type</c>.</param>
		protected PolymorphicInterfaceConverter(
			IReadOnlyDictionary<string, Type> typeByDiscriminator,
			string discriminatorPropertyName = "type")
		{
			_typeByDiscriminator = typeByDiscriminator ?? throw new ArgumentNullException(nameof(typeByDiscriminator));
			_discriminatorPropertyName = discriminatorPropertyName;
		}

		/// <inheritdoc />
		public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			if (root.ValueKind != JsonValueKind.Object
				|| !root.TryGetProperty(_discriminatorPropertyName, out var discriminatorProperty)
				|| discriminatorProperty.ValueKind != JsonValueKind.String)
				return null;

			var discriminator = discriminatorProperty.GetString();
			if (discriminator == null || !_typeByDiscriminator.TryGetValue(discriminator, out var concreteType))
				return null;

			return (TInterface)root.Deserialize(concreteType, options);
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// Serialize the concrete runtime type; the type is not TInterface so this does not recurse.
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}
	}
}
