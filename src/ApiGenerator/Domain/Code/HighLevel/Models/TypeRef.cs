/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Structured representation of a C# type. Separates the base type identity from nullability,
/// enabling callers (templates, serialization attribute generators) to make decisions based on
/// type shape rather than parsing strings.
/// </summary>
public abstract record TypeRef
{
    /// <summary>Whether this property is nullable (drives the trailing <c>?</c> in C#).</summary>
    public abstract bool IsNullable { get; }

    /// <summary>Render as the C# type string used in generated code.</summary>
    public abstract string ToCsharp();

    public override string ToString() => ToCsharp();
}

/// <summary>Built-in value type: bool, int, long, float, double.</summary>
public sealed record PrimitiveType : TypeRef
{
    public string Name { get; }
    public override bool IsNullable { get; }
    public PrimitiveType(string name, bool isNullable) { Name = name; IsNullable = isNullable; }
    public override string ToCsharp() => IsNullable ? $"{Name}?" : Name;
}

/// <summary>String type (reference type, never suffixed with <c>?</c>).</summary>
public sealed record StringType : TypeRef
{
    public override bool IsNullable => false;
    public override string ToCsharp() => "string";
}

/// <summary>A generated string enum (value-type semantics in C#).</summary>
public sealed record EnumType : TypeRef
{
    public string Name { get; }
    public override bool IsNullable { get; }
    public EnumType(string name, bool isNullable) { Name = name; IsNullable = isNullable; }
    public override string ToCsharp() => IsNullable ? $"{Name}?" : Name;
}

/// <summary>A reference to a generated object model (rendered as its interface name).</summary>
public sealed record ObjectRefType : TypeRef
{
    public string Name { get; }
    public override bool IsNullable { get; }
    public ObjectRefType(string name, bool isNullable) { Name = name; IsNullable = isNullable; }
    public override string ToCsharp() => $"I{Name}";
}

/// <summary>An ordered collection: <c>IList&lt;T&gt;</c>.</summary>
public sealed record ListType : TypeRef
{
    public TypeRef Element { get; }
    public override bool IsNullable { get; }
    public ListType(TypeRef element, bool isNullable) { Element = element; IsNullable = isNullable; }
    public override string ToCsharp() => $"IList<{Element.ToCsharp()}>";
}

/// <summary>A dictionary: <c>IDictionary&lt;string, T&gt;</c>.</summary>
public sealed record DictionaryType : TypeRef
{
    public TypeRef Value { get; }
    public override bool IsNullable { get; }
    public DictionaryType(TypeRef value, bool isNullable) { Value = value; IsNullable = isNullable; }
    public override string ToCsharp() => $"IDictionary<string, {Value.ToCsharp()}>";
}

/// <summary>A type mapped to an existing hand-written type name (from MappedTypes).</summary>
public sealed record MappedType : TypeRef
{
    public string Name { get; }
    public override bool IsNullable { get; }
    public MappedType(string name, bool isNullable) { Name = name; IsNullable = isNullable; }
    public override string ToCsharp() => Name;
}

/// <summary>Fallback when the type cannot be determined.</summary>
public sealed record FallbackType : TypeRef
{
    public override bool IsNullable => false;
    public override string ToCsharp() => "object";
}
