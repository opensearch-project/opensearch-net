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
        if (!schema.HasOneOf())
            return schema.Type == JsonObjectType.String && (schema.Enumeration.Count > 0 || schema.Const<string>() != null);

        var enumCount = 0;
        var booleanCount = 0;
        var totalCount = 0;

        foreach (var s in schema.OneOf)
        {
            if (s.Type == JsonObjectType.Null) continue;

            if (s.Type == JsonObjectType.Boolean)
                booleanCount++;
            else if (s.IsEnum()) enumCount++;

            totalCount++;
        }

        return enumCount == totalCount || (booleanCount == 1 && enumCount == totalCount - 1);
    }

    public static IImmutableList<(string Value, string Alias)> GetEnumValues(this JsonSchema schema)
    {
        var normalized = new HashSet<string>();
        var values = new SortedSet<(string Value, string Alias)>();
        Visit(schema);
        return values.ToImmutableList();

        void Add(string value, string alias)
        {
            if (normalized.Add(value.ToLowerInvariant())) values.Add((value, alias));
        }

        void Visit(JsonSchema s)
        {
            if (s.HasOneOf())
                foreach (var oneOf in schema.OneOf) Visit(oneOf);
            else if (s.Enumeration.Count > 0)
                foreach (var v in s.Enumeration.Where(v => v != null)) Add((string) v, null);
            else if (s.Const<string>() != null)
                Add(s.Const<string>(), s.Title);
        }
    }

    public static T Const<T>(this JsonSchema schema) where T: class =>
        schema.GetExtension("const") as T;

    public static object GetExtension(this IJsonExtensionObject schema, string key) =>
        schema.ExtensionData?.TryGetValue(key, out var value) ?? false ? value : null;
}
