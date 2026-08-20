/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Identifies the wire encoding of a discriminated union schema.
/// Classification is structural and spec-driven — no plugin names, ML/search-pipeline checks,
/// or rendering decisions are involved.
/// </summary>
public enum UnionEncoding
{
    /// <summary>
    /// Unknown or unsupported union pattern. Classification failed.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// OpenAPI <c>discriminator</c> keyword with <c>propertyName</c>.
    /// Wire format: <c>{ "model": "linear", ... }</c> where <c>model</c> is the discriminator field.
    /// The discriminator value is a property inside the JSON object.
    /// </summary>
    InternalDiscriminator,

    /// <summary>
    /// <c>oneOf</c> where each variant has exactly one required property as the wrapper key.
    /// Wire format: <c>{ "filter_query": { ...body... } }</c>.
    /// Used by search-pipeline RequestProcessor/ResponseProcessor.
    /// </summary>
    WrapperKeyOneOf,

    /// <summary>
    /// Object with <c>minProperties: 1</c> and <c>maxProperties: 1</c> (or equivalent semantics).
    /// Wire format: <c>{ "append": { ...body... } }</c>.
    /// Used by ingest ProcessorContainer.
    /// </summary>
    FlatWrapperKey,

    /// <summary>
    /// Dictionary where keys encode the type via <c>{name}#{type}</c> format (ES typed_keys).
    /// Wire format: <c>{ "my_avg#avg": { "value": 42.0 } }</c>.
    /// Used for aggregation results with typed_keys=true.
    /// </summary>
    TypedKeys
}
