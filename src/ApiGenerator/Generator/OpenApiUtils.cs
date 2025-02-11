/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NJsonSchema;

namespace ApiGenerator.Generator;

public static class OpenApiUtils
{
    public static bool HasOneOf(this JsonSchema schema) => schema.OneOf.Count > 0;

    public static bool IsEnum(this JsonSchema schema)
    {
        if (schema.Type == JsonObjectType.String)
            return schema.Enumeration.Count > 0 || schema.Const<string>() != null;

        return schema.HasOneOf() && schema.OneOf.All(IsEnum);
    }

    public static IImmutableList<string> GetEnumValues(this JsonSchema schema)
    {
        var normalized = new HashSet<string>();
        var values = new SortedSet<string>();
        Visit(schema);
        return values.ToImmutableList();

        void Add(string v)
        {
            if (normalized.Add(v.ToLowerInvariant())) values.Add(v);
        }

        void Visit(JsonSchema s)
        {
            if (s.HasOneOf())
                foreach (var oneOf in schema.OneOf) Visit(oneOf);
            else if (s.Enumeration.Count > 0)
                foreach (var v in s.Enumeration.Where(v => v != null)) Add((string) v);
            else if (s.Const<string>() != null)
                Add(s.Const<string>());
        }
    }

    public static T Const<T>(this JsonSchema schema) where T: class =>
        schema.GetExtension("const") as T;

    public static object GetExtension(this IJsonExtensionObject schema, string key) =>
        schema.ExtensionData?.TryGetValue(key, out var value) ?? false ? value : null;
}
