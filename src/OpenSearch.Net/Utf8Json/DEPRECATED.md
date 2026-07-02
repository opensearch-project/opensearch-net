# ⚠️ Utf8Json — DEPRECATED

This directory contains a vendored copy of the [Utf8Json](https://github.com/neuecc/Utf8Json) library,
which is forked from an **abandoned upstream project** with known security warnings
([Issue #254](https://github.com/neuecc/Utf8Json/issues/254), [Issue #255](https://github.com/neuecc/Utf8Json/issues/255)).

## Status

**As of opensearch-net 2.0.0, System.Text.Json is the default serializer.**

- The low-level client (`OpenSearch.Net`) uses `SystemTextJsonSerializer` by default.
- The high-level client (`OpenSearch.Client`) uses `DefaultHighLevelSystemTextJsonSerializer` by default.
- Utf8Json code is retained only for backward compatibility and will be removed in a future major version.

## Migration

Users who explicitly depended on Utf8Json serialization behavior should migrate to System.Text.Json.
The `LowLevelRequestResponseSerializer` class remains available as an opt-in legacy serializer:

```csharp
var settings = new ConnectionConfiguration(
    pool, 
    new LowLevelRequestResponseSerializer() // opt-in to legacy Utf8Json serializer
);
```

## Removal Timeline

This code will be removed in the next major version (3.0.0). All internal formatters have been
replaced with System.Text.Json converters. Remaining references exist only for:
- `OpenSearch.Client.JsonNetSerializer` compatibility layer
- Legacy test assertions that validate Utf8Json behavior
- Users who explicitly inject `LowLevelRequestResponseSerializer`

## Security Context

The upstream Utf8Json project has explicit warnings against production use. This vendored copy
addresses known issues but receives no upstream maintenance. Migrating to System.Text.Json
(a Microsoft-supported, actively maintained library) eliminates this security concern entirely.
