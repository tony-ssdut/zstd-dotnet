# SystemIOCompression.Zstandard Migration Guide

This repository now recommends a single package reference for application projects:

```xml
<PackageReference Include="SystemIOCompression.Zstandard.Backporting" Version="11.0.1" />
```

## Package strategy

- Use `SystemIOCompression.Zstandard.Backporting` as the only package reference in app projects.
- `net8.0`, `net9.0`, `net10.0`: package routes to backport implementation.
- `net11.0+`: package becomes compatibility shim and runtime implementation is used.

## Migration comparison

### Package replacement

Replace:

```xml
<PackageReference Include="ZstdDotnet" Version="1.5.7.1" />
```

With:

```xml
<PackageReference Include="SystemIOCompression.Zstandard.Backporting" Version="11.0.1" />
```

### API mapping

| Legacy | New |
| --- | --- |
| `using ZstdDotnet;` | `using System.IO.Compression;` |
| `ZstdStream` | `ZstandardStream` |
| `ZstdEncoder` | `ZstandardEncoder` |
| `ZstdDecoder` | `ZstandardDecoder` |

### Stream/encoder/decoder compatibility notes

- Stream flush behavior: `ZstdStream.FlushFrame()` has no direct `ZstandardStream` equivalent.
- Stream reset behavior: `ZstdStream.Reset()` is removed; recreate stream/codec instance.
- Compression level mutability: `ZstdStream.CompressionLevel` mutable property is removed; configure via constructor/options.
- Concatenated-frame read semantics differ: caller must handle remaining compressed bytes explicitly.
- Low-level decoder loop contract changed: `isFinalBlock` and `frameFinished` style flow is not preserved.

## Breaking changes

- No `FlushFrame()` equivalent on `ZstandardStream`.
- No `Reset()` equivalent on `ZstandardStream`.
- No mutable `CompressionLevel` property on stream instances.
- Concatenated-frame decompression behavior differs from legacy implementation.
- Decoder max-window config pattern changes to constructor-based configuration.

## Recommended migration steps

1. Replace package reference with `SystemIOCompression.Zstandard.Backporting`.
2. Rename namespaces and API types.
3. Fix compile-time breakpoints (`FlushFrame`, `Reset`, mutable `CompressionLevel`, decoder config APIs).
4. Add/refresh tests for multi-frame payloads and truncation/error behavior.
5. Validate runtime behavior per target framework (`net8.0` through `net11.0+`).

## Legacy documentation

Legacy `ZstdDotnet` package details and historical usage notes were moved to `README.legacy.md`.