# CHANGELOG
Inspired from [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [Unreleased]
### ⚠️ Breaking Changes ⚠️
- Removed support for the `net461` target ([#256](https://github.com/opensearch-project/opensearch-net/pull/256))
- Fixed naming of `ClusterManagerTimeout` and `MasterTimeout` properties from `*TimeSpanout` in the low-level client ([#332](https://github.com/opensearch-project/opensearch-net/pull/332))

### Added
- Added support for the `Cat.PitSegments` and `Cat.SegmentReplication` APIs ([#527](https://github.com/opensearch-project/opensearch-net/pull/527))
- Added `.Strict(...)`, `.Verbatim(...)`, `.Name(...)` methods on `QueryContainer` to help modify contained query attributes  ([#509](https://github.com/opensearch-project/opensearch-net/pull/509))

### Removed
- Removed the `Features` API which is not supported by OpenSearch from the low-level client ([#331](https://github.com/opensearch-project/opensearch-net/pull/331))
- Removed the deprecated low-level `IndexTemplateV2` APIs in favour of the `ComposableIndexTemplate` APIs ([#437](https://github.com/opensearch-project/opensearch-net/pull/437))

### Fixed
- Fixed `HttpConnection.ConvertHttpMethod` to support `Patch` method ([#489](https://github.com/opensearch-project/opensearch-net/pull/489))
- Fixed `IEnumerable<int?>` property mapping. ([#503](https://github.com/opensearch-project/opensearch-net/pull/503))

### Dependencies
- Bumps `Microsoft.CodeAnalysis.CSharp` from 4.2.0 to 4.6.0
- Bumps `NSwag.Core.Yaml` from 13.19.0 to 14.0.3
- Bumps `CSharpier.Core` from 0.25.0 to 0.27.2
- Bumps `System.Diagnostics.DiagnosticSource` from 6.0.1 to 8.0.0
- Bumps `Spectre.Console` from 0.47.0 to 0.48.0
- Bumps `System.Text.Encodings.Web` from 7.0.0 to 8.0.0
- Bumps `xunit.runner.visualstudio` from 2.5.4 to 2.5.7
- Bumps `xunit` from 2.6.2 to 2.6.6
- Bumps `Argu` from 6.1.1 to 6.1.5
- Bumps `Microsoft.NET.Test.Sdk` from 17.7.2 to 17.9.0
- Bumps `JetBrains.Annotations` from 2023.2.0 to 2023.3.0
- Bumps `Bogus` from 34.0.2 to 35.3.0
- Bumps `Octokit` from 9.0.0 to 9.1.2
- Bumps `FSharp.Core` from 8.0.100 to 8.0.101
- Bumps `Proc` from 0.6.2 to 0.8.1
- Bumps `System.Text.Json` from 8.0.0 to 8.0.1
- Bumps `Bullseye` from 4.2.1 to 5.0.0
- Bumps `BenchMarkDotNet` from 0.13.11 to 0.13.12
- Bumps `Microsoft.TestPlatform.ObjectModel` from 17.8.0 to 17.9.0
- Bumps `SharpYaml` from 2.1.0 to 2.1.1

## [1.6.0]
### Added
- Added support for point-in-time search and associated APIs ([#405](https://github.com/opensearch-project/opensearch-net/pull/405))
- Added support for the component template APIs ([#411](https://github.com/opensearch-project/opensearch-net/pull/411))
- Added support for the composable index template APIs ([#437](https://github.com/opensearch-project/opensearch-net/pull/437))
- Added high-level DSL for raw HTTP methods ([#447](https://github.com/opensearch-project/opensearch-net/pull/447))

### Deprecated
- Deprecated the low-level `IndexTemplateV2` APIs in favour of the new `ComposableIndexTemplate` APIs ([#454](https://github.com/opensearch-project/opensearch-net/pull/454))

### Dependencies
- Bumps `FSharp.Data` from 6.2.0 to 6.3.0
- Bumps `BenchMarkDotNet` from 0.13.7 to 0.13.11
- Bumps `AWSSDK.Core` from 3.7.202.1 to 3.7.204.12
- Bumps `xunit.runner.visualstudio` from 2.5.0 to 2.5.4
- Bumps `Octokit` from 7.1.0 to 9.0.0
- Bumps `FSharp.Core` from 7.0.400 to 8.0.100
- Bumps `xunit` from 2.4.2 to 2.6.2
- Bumps `Microsoft.TestPlatform.ObjectModel` from 17.7.2 to 17.8.0
- Bumps `System.Text.Json` from 7.0.3 to 8.0.0
- Bumps `Microsoft.SourceLink.GitHub` from 1.1.1 to 8.0.0

## [1.5.0]
### Fixed
- Fix highlight max_analyzer_offset field name to match with the one introduced in OpenSearch 2.2.0 ([#322](https://github.com/opensearch-project/opensearch-net/pull/322))
- Fix null-ref exception when track total hits is disabled ([#341](https://github.com/opensearch-project/opensearch-net/pull/341))

### Dependencies
- Bumps `Microsoft.TestPlatform.ObjectModel` from 17.5.0 to 17.7.2
- Bumps `JunitXml.TestLogger` from 3.0.124 to 3.0.134
- Bumps `System.Reflection.Emit.Lightweight` from 4.3.0 to 4.7.0
- Bumps `BenchMarkDotNet` from 0.13.5 to 0.13.7
- Bumps `Microsoft.NET.Test.Sdk` from 17.6.2 to 17.7.2
- Bumps `AWSSDK.Core` from 3.7.106.29 to 3.7.202.1
- Bumps `Bullseye` from 3.3.0 to 4.2.1
- Bumps `xunit.runner.visualstudio` from 2.4.5 to 2.5.0
- Bumps `FluentAssertions` from 6.10.0 to 6.12.0
- Bumps `JetBrains.Annotations` from 2022.3.1 to 2023.2.0
- Bumps `Octokit` from 6.1.0 to 7.1.0
- Bumps `FSharp.Core` from 7.0.300 to 7.0.400

## [1.4.0]
### Added
- Added support for approximate k-NN search queries and k-NN vector index properties ([#215](https://github.com/opensearch-project/opensearch-net/pull/215))

### Dependencies
- Bumps `System.Reflection.Emit` from 4.3.0 to 4.7.0
- Bumps `Argu` from 5.5.0 to 6.1.1
- Bumps `Ben.Demystifier` from 0.1.4 to 0.4.1
- Bumps `System.Text.Json` from 7.0.1 to 7.0.3
- Bumps `Octokit` from 4.0.3 to 6.1.0
- Bumps `Fake.Core.Environment` from 5.23.1 to 6.0.0
- Bumps `Microsoft.TestPlatform.ObjectModel` from 17.4.1 to 17.5.0
- Bumps `SharpZipLib` from 1.4.1 to 1.4.2
- Bumps `AWSSDK.Core` from 3.7.103.17 to 3.7.106.29
- Bumps `Newtonsoft.Json` from 13.0.2 to 13.0.3
- Bumps `Microsoft.CSharp` from 4.6.0 to 4.7.0
- Bumps `FSharp.Data` from 5.0.2 to 6.2.0
- Bumps `BenchMarkDotNet` from 0.13.4 to 0.13.5
- Bumps `FSharp.Core` from 6.0.7 to 7.0.300
- Bumps `Microsoft.NET.Test.Sdk` from 17.4.1 to 17.6.2
- Bumps `Fake.IO.FileSystem` from 5.23.1 to 6.0.0
- Bumps `Microsoft.SourceLink.GitHub` from 1.0.0 to 1.1.1
- Bumps `System.Diagnostics.DiagnosticSource` from 6.0.0 to 6.0.1
- Bumps `System.Reactive` from 5.0.0 to 6.0.0
- Bumps `Proc` from 0.6.1 to 0.6.2

## [1.3.0]
### Added
- Added support for Amazon OpenSearch Serverless request signing ([#133](https://github.com/opensearch-project/opensearch-net/pull/133))

### Changed
- Updated SDK to .NET 6 ([#126](https://github.com/opensearch-project/opensearch-net/pull/126))

### Removed
- Removed `net461` target from internal packages ([#128](https://github.com/opensearch-project/opensearch-net/pull/128))

### Fixed
- Fixed parsing of date histogram buckets ([#131](https://github.com/opensearch-project/opensearch-net/pull/131))
- Allow passing both boolean and integer values to `TrackTotalHits` ([#121](https://github.com/opensearch-project/opensearch-net/pull/121))

### Security
- CVE-2019-0820: Removed transitive dependencies on `System.Text.RegularExpressions` from internal packages; **Client Not Impacted** ([#137](https://github.com/opensearch-project/opensearch-net/pull/137))

### Dependencies
- Bumps `SemanticVersioning` from 0.8.0 to 2.0.2
- Bumps `Microsoft.NET.Test.Sdk` from 16.5.0 to 17.4.1
- Bumps `Octokit` from 0.32.0 to 4.0.3
- Bumps `BenchMarkDotNet` from 0.13.1 to 0.13.4
- Bumps `System.Reactive` from 3.1.1 to 5.0.0
- Bumps `SharpZipLib` from 1.0.4 to 1.4.1 ([#136](https://github.com/opensearch-project/opensearch-net/pull/136))
- Bumps `FSharp.Core` from 4.7.0 to 6.0.7
- Bumps `FSharp.Data` from 3.1.1 to 5.0.2
- Bumps `AWSSDK.Core` from 3.7.12.11 to 3.7.103.17
- Bumps `JunitXml.TestLogger` from 2.1.78 to 3.0.124
- Bumps `Fake.*` from 5.15.0 to 5.23.1
- Bumps `ConfigureAwaitChecker.Analyzer` from 4.0.0 to 5.0.0.1
- Bumps `Bogus` from 22.1.2 to 34.0.2
- Bumps `ShellProgressBar` from 5.0.0 to 5.2.0
- Bumps `FluentAssertions` from 5.10.3 to 6.10.0
- Bumps `SharpYaml` from 1.6.5 to 2.1.0
- Bumps `Newtonsoft.Json` from 13.0.1 to 13.0.2
- Bumps `Fake.Core.SemVer` from 5.23.1 to 6.0.0
- Bumps `System.Diagnostics.DiagnosticSource` from 5.0.0 to 6.0.0
- Bumps `Microsoft.NETFramework.ReferenceAssemblies` from 1.0.0-preview.2 to 1.0.3

[Unreleased]: https://github.com/opensearch-project/opensearch-net/compare/v1.6.0...main
[1.6.0]: https://github.com/opensearch-project/opensearch-net/compare/v1.5.0...v1.6.0
[1.5.0]: https://github.com/opensearch-project/opensearch-net/compare/v1.4.0...v1.5.0
[1.4.0]: https://github.com/opensearch-project/opensearch-net/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/opensearch-project/opensearch-net/compare/v1.2.0...v1.3.0