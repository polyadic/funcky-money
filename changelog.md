# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.4.0

### Changed
- Money division is now implemented directly instead of as the inverse of a multiplication,
  which avoids the intermediate rounding/precision loss of the previous approach.
- Target frameworks are now `net10.0` and `netstandard2.0` (the `net6.0` target was dropped;
  consumers on older runtimes are still covered by `netstandard2.0`).
- Updated dependencies, most notably `Funcky` 3.6.0.

### Internal
- Switched to NuGet's built-in Central Package Management (`Directory.Packages.props`).
- The ISO 4217 source generator now uses a fully cacheable incremental pipeline.
- General modernization (primary constructors, collection expressions, sealed types).

## 1.1.0

## 1.0.1

## 1.0.0
