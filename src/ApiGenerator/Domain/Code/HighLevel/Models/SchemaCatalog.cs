/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;
using NJsonSchema;
using NSwag;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Provides canonical component identities for NSwag schema wrappers and their resolved
/// <see cref="JsonSchema.ActualSchema"/> instances.
/// </summary>
public sealed class SchemaCatalog
{
    private readonly IReadOnlyDictionary<string, JsonSchema> _schemasById;
    private readonly Dictionary<JsonSchema, string> _idsBySchema =
        new(ReferenceEqualityComparer.Instance);

    public SchemaCatalog(OpenApiDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        _schemasById = new Dictionary<string, JsonSchema>(
            document.Components.Schemas,
            StringComparer.Ordinal);
        foreach (var (id, schema) in _schemasById)
        {
            Register(schema, id);
            Register(schema.ActualSchema, id);
        }
    }

    /// <summary>Gets the component ID for a schema wrapper or resolved schema instance.</summary>
    public bool TryGetId(JsonSchema schema, out string id)
    {
        ArgumentNullException.ThrowIfNull(schema);

        return _idsBySchema.TryGetValue(schema, out id!)
            || _idsBySchema.TryGetValue(schema.ActualSchema, out id!);
    }

    /// <summary>Gets a component schema by its ID.</summary>
    public bool TryGetSchema(string id, out JsonSchema schema) =>
        _schemasById.TryGetValue(id, out schema!);

    private void Register(JsonSchema schema, string id)
    {
        // NSwag may reuse one ActualSchema instance for multiple component aliases. Preserve the
        // previous reverse-map behavior: the later component in document order is canonical.
        _idsBySchema[schema] = id;
    }
}
