# CHANGELOG
Inspired from [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [Unreleased]
### Added
- Added support for Amazon OpenSearch Serverless request signing ([#133](https://github.com/opensearch-project/opensearch-net/pull/133))
- Github workflow for changelog verification ([#111](https://github.com/opensearch-project/opensearch-net/pull/111))
- Bump version to `1.2.1`([#115](https://github.com/opensearch-project/opensearch-net/pull/115))
- Added `dependabot_changelog` workflow ([#118](https://github.com/opensearch-project/opensearch-net/pull/118))

### Changed
- Updated SDK to .NET 6 ([#126](https://github.com/opensearch-project/opensearch-net/pull/126))

### Deprecated

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

[Unreleased]: https://github.com/opensearch-project/opensearch-net/compare/1.2.0...HEAD