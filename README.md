# ZstdDotnet

ZstdDotnet is a high-performance, streaming-friendly .NET wrapper for the Zstandard (ZSTD) compression library. It builds on the official native `libzstd` implementation and exposes modern .NET APIs that work seamlessly with `Span<byte>` and `Memory<byte>`.

> Status update: the classic `ZstdDotnet` package is being deprecated. For new projects, use `System.IO.Compression.Zstandard.Backporting` so you get net10 backport behavior and no extra dependency on net11+.
> The managed `ZstdDotnet` package delivers the .NET API surface, while `ZstdDotnet.NativeAssets` ships the cross-platform native binaries. This README covers both.

## Status and migration

The classic `ZstdDotnet` package is in deprecation path.

For new consumers:

- Use `System.IO.Compression.Zstandard.Backporting` as the single reference.
- When targeting `net8.0`, `net9.0`, or `net10.0`, the backport package uses the compatibility implementation from `System.IO.Compression.Zstandard`.
- When targeting `net11.0` or newer, the backport package does not add `System.IO.Compression.Zstandard` dependency and the application uses the .NET runtime implementation directly.

Migration recommendation:

1. Replace direct `ZstdDotnet` references with `System.IO.Compression.Zstandard.Backporting`.
2. Keep your target frameworks as-is; dependency behavior is framework-conditional.
3. Remove explicit references to the old native asset package unless you need it for legacy scenarios.

### Migration guide

This is a source-compatible migration only for simple single-frame stream usage. If your code uses multi-frame output, stream reset, or low-level decoder state, review the breaking changes before replacing package references.

#### Package change

Replace:

```xml
<PackageReference Include="ZstdDotnet" Version="1.5.7.1" />
```

With:

```xml
<PackageReference Include="System.IO.Compression.Zstandard.Backporting" Version="11.0.1" />
```

Behavior by target framework:

- `net8.0`, `net9.0`, and `net10.0`: the backport package uses the compatibility implementation from this repository.
- `net11.0+`: the backport package becomes a compatibility shim and the application uses the .NET runtime implementation.

In most application projects you should also remove any explicit `ZstdDotnet.NativeAssets` reference.

#### Common renames

| Old | New |
| --- | --- |
| `using ZstdDotnet;` | `using System.IO.Compression;` |
| `ZstdStream` | `ZstandardStream` |
| `ZstdEncoder` | `ZstandardEncoder` |
| `ZstdDecoder` | `ZstandardDecoder` |

#### Basic example

Old:

```csharp
using ZstdDotnet;
using System.IO.Compression;

using var output = new MemoryStream();
using (var zs = new ZstdStream(output, CompressionMode.Compress, leaveOpen: true))
{
    zs.Write(payload, 0, payload.Length);
}
```

New:

```csharp
using System.IO.Compression;

using var output = new MemoryStream();
using (var zs = new ZstandardStream(output, CompressionMode.Compress, leaveOpen: true))
{
    zs.Write(payload);
}
```

#### Breaking changes

The main breaking changes are:

- `ZstdStream.FlushFrame()` has no `ZstandardStream` equivalent. Create a new `ZstandardStream` per frame if you rely on explicit multi-frame output.
- `ZstdStream.Reset()` does not exist. Recreate the stream or low-level codec instance for a new session.
- Mutable `ZstdStream.CompressionLevel` is gone. Set compression behavior in the constructor or via `ZstandardCompressionOptions`.
- Concatenated-frame decompression differs. `ZstdDotnet` stitches frames together automatically; `ZstandardStream` stops at the current frame and leaves remaining compressed bytes for the caller.
- Low-level `ZstdDecoder.Decompress(...)` no longer takes `isFinalBlock` or returns `frameFinished`. Custom decode loops need review.
- Static `byte[]` convenience helpers differ. Prefer `TryCompress`, `TryDecompress`, and size helpers, or add a small adapter layer.
- Decoder max-window configuration moves from `SetMaxWindow(...)` to `new ZstandardDecoder(maxWindowLog)`.

#### Migration steps

1. Replace package references with `System.IO.Compression.Zstandard.Backporting`.
2. Rename namespaces and types.
3. Fix compile errors around `FlushFrame`, `Reset`, mutable `CompressionLevel`, and `SetMaxWindow`.
4. Audit code that reads or writes concatenated frames or uses custom low-level decoder loops.
5. Add regression tests for multi-frame payloads, truncation handling, and any dynamic compression configuration your application depends on.

Call sites are highest risk if they use `FlushFrame(`, `.Reset(`, `.CompressionLevel =`, `SetMaxWindow(`, `ZstdEncoder.Compress(`, or `ZstdDecoder.Decompress(`.

### Support matrix

Supported .NET application targets:

- `net8.0`
- `net9.0`
- `net10.0`

Package behavior by target:

- `net8.0`, `net9.0`, and `net10.0`: use the backport code shipped by `System.IO.Compression.Zstandard` through `System.IO.Compression.Zstandard.Backporting`.
- `net11.0+`: `System.IO.Compression.Zstandard.Backporting` becomes a no-op compatibility package and the application uses the .NET runtime implementation directly.

Backport package usage guidance:

- Reference `System.IO.Compression.Zstandard.Backporting` from application projects when you want one package reference that spans `net8.0` through `net11.0+`.
- On `net8.0` through `net10.0`, that package routes to the backport library code in this repository.
- On `net11.0` and later, that same package intentionally defers to the platform implementation instead of bringing its own compatibility layer.

Supported runtime environments for bundled native binaries:

- Windows x64: `win-x64`
- Linux x64: `linux-x64`
- macOS x64: `osx-x64`

Current native packaging scope is x64 only. ARM and other RIDs are not included by this package at this time.

### Backport implementation notes

For target frameworks where the platform does not provide `System.Runtime.InteropServices.PinnedGCHandle<T>`, this repository includes an internal compatibility implementation in `src/System.IO.Compression.Zstandard/System/Runtime/InteropServices/PinnedGCHandle.cs`.

Current strategy:

- The backport uses the public `GCHandle` API with `GCHandleType.Pinned`.
- It is only intended to support this library's internal pinning scenarios, primarily dictionary byte-array lifetime management.
- It avoids reflection and does not call runtime-internal `GCHandle.Internal*` methods.

Differences from the net11 platform implementation:

- The net11 implementation uses runtime-internal handle operations and stores the underlying handle value directly. The backport uses the public `GCHandle` wrapper type.
- The backport is not a byte-for-byte clone of the platform implementation and should be treated as a compatibility shim, not a drop-in reimplementation of every low-level behavior.
- The `Target` setter in the backport re-pins by freeing and reallocating the handle. The platform implementation updates the existing runtime handle in place.
- Low-level conversion and pointer helpers from the platform type are intentionally not exposed in the backport because they rely on runtime internals and are not needed by this library.

Practical consequence:

- For this package's current use, the behavioral difference is acceptable because the handle is created to pin dictionary data and is disposed when the owning safe handle is released.
- Consumers should not depend on undocumented runtime-level identity or pointer semantics from this compatibility type.

## Features

- Powered by the official C implementation of Zstandard, matching native compression quality and performance.
- Full streaming support: chunked writes/reads and transparent multi-frame decoding.
- True async APIs (`ReadAsync`, `WriteAsync`, `FlushAsync`, `DisposeAsync`) with no sync blocking shims.
- `Span<byte>`/`Memory<byte>` overloads to minimize allocations and copies.
- Configurable compression level via precise integers or the built-in `CompressionLevel` enum (default 5, range `ZstdProperties.MinCompressionLevel`..`ZstdProperties.MaxCompressionLevel`).
- Concurrency guards: prevents simultaneous read/write/flush/dispose on the same instance.
- Frame tooling: `ZstdFrameDecoder` and `ZstdFrameInspector` expose incremental frame metadata and async iteration.
- Hardened with 60+ unit tests covering edge cases, fuzz inputs, concurrency, cancellation, pooling, and huge frames.
- Requires native libzstd >= 1.5.0 (the library now exclusively uses the unified `ZSTD_compressStream2()` API; legacy `compress/flush/end` trio removed internally).
- Decoder uses DCtx (modern context); optional `SetMaxWindow(log)` to cap memory usage.
- Reusable decoder instances via `ZstdDecoderPool` reduce allocation and native context churn.
- Optional raw content prefix via `ZstdEncoder.SetPrefix(memory)` to boost ratio when many frames share an initial header-like segment.

## Installation

Recommended (new projects and migration target):

```xml
<ItemGroup>
    <PackageReference Include="System.IO.Compression.Zstandard.Backporting" Version="11.0.1" />
</ItemGroup>
```

Framework behavior:

- `net10.0`: pulls `System.IO.Compression.Zstandard` via conditional dependency.
- `net11.0+`: no additional dependency from the backport package.

Legacy (classic package, being deprecated):

```xml
<ItemGroup>
  <PackageReference Include="ZstdDotnet" Version="1.5.7.1" />
</ItemGroup>
```

Referencing `ZstdDotnet` automatically pulls in `ZstdDotnet.NativeAssets`. At runtime the appropriate native binary is loaded based on the current RID. If you only need the native library, reference `ZstdDotnet.NativeAssets` directly.

## Quick start

```csharp
// Compress
var data = File.ReadAllBytes("input.bin");
using var outStream = new MemoryStream();
using (var zs = new ZstdStream(outStream, CompressionMode.Compress, leaveOpen: true))
{
    int offset = 0;
    while (offset < data.Length)
    {
        int chunk = Math.Min(8192, data.Length - offset);
        zs.Write(data, offset, chunk); // or zs.Write(data.AsSpan(offset, chunk));
        offset += chunk;
    }
}
File.WriteAllBytes("output.zst", outStream.ToArray());

// Decompress
using var compressed = File.OpenRead("output.zst");
using var zsDec = new ZstdStream(compressed, CompressionMode.Decompress);
using var restored = new MemoryStream();
var buffer = new byte[8192];
int read;
while ((read = zsDec.Read(buffer, 0, buffer.Length)) > 0)
{
    restored.Write(buffer, 0, read);
}
```

### Asynchronous usage

```csharp
byte[] payload = GetLargeBuffer();
await using var stream = new MemoryStream();
await using (var encoder = new ZstdStream(stream, CompressionMode.Compress, leaveOpen: true))
{
    int offset = 0;
    var random = new Random();
    while (offset < payload.Length)
    {
        int chunk = Math.Min(random.Next(1024, 16_384), payload.Length - offset);
        await encoder.WriteAsync(payload.AsMemory(offset, chunk));
        offset += chunk;
    }
    await encoder.FlushAsync();
}
stream.Position = 0;
await using (var decoder = new ZstdStream(stream, CompressionMode.Decompress, leaveOpen: true))
{
    var scratch = new byte[4096];
    int n;
    while ((n = await decoder.ReadAsync(scratch)) > 0)
    {
        // consume bytes
    }
}
```

### Span/Memory helpers

- Sync: `Write(ReadOnlySpan<byte>)`, `Read(Span<byte>)`
- Async: `WriteAsync(ReadOnlyMemory<byte>, CancellationToken)`, `ReadAsync(Memory<byte>, CancellationToken)`

See [docs/LowLevel.md](docs/LowLevel.md) for incrementally streaming the low-level encoder/decoder, and [docs/Advanced.md](docs/Advanced.md) for advanced tuning guidance.

## API at a glance

| Member | Description |
| ------ | ----------- |
| `ZstdStream(Stream inner, CompressionMode mode, bool leaveOpen = false)` | Create a compression or decompression stream |
| `CompressionLevel` | Sets compression level when writing |
| `Flush()` / `FlushAsync()` | Drain pending output without finalizing a frame |
| `Flush(Span<byte>, out int)` | Low-level flush returning an `OperationStatus` |
| `FlushFrame()` | Terminates the current frame and starts a new one |
| `Dispose()` / `DisposeAsync()` | Finalizes the frame and releases resources |
| `Reset()` | Reset decoder state and continue with subsequent frames |
| `ZstdFrameInspector.EnumerateFrames(...)` | Inspect frame metadata without decompressing |
| `ZstdFrameDecoder.DecodeFramesAsync(...)` | Async frame iterator returning content + metadata |
| `ZstdProperties.LibraryVersion` | Reports the native library version |
| `ZstdProperties.ZstdVersion` / `ZstdProperties.ZstdVersionString` | Alternate strongly typed / string version forms |
| `ZstdProperties.MaxCompressionLevel` | Reports the maximum available level |

## Flush API cheat sheet

| Method | Writes frame terminator? | Continue same frame? | Starts new frame? | Primary use |
| ------ | ------------------------ | -------------------- | ---------------- | ----------- |
| `Flush()` / `FlushAsync()` | No | Yes | No | Drain the encoder buffer to lower latency |
| `Flush(Span<byte>, out int)` | No | Yes | No | Manual buffer control and `DestinationTooSmall` loops |
| `FlushFrame()` | Yes | No | Yes | Logical segmentation / multi-frame output |
| `Dispose()` / `DisposeAsync()` | Yes | No | No | Finalize the stream |

```csharp
Span<byte> scratch = stackalloc byte[1024];
while (true)
{
    var status = zs.Flush(scratch, out int written);
    if (written > 0)
        downstream.Write(scratch[..written]);
    if (status == OperationStatus.Done)
        break;
    // status == DestinationTooSmall -> loop again
}
```

## Design & performance notes

- Partial layout: `Streams/ZstdStream.cs` provides shared state, `enc/` and `dec/` contain compression/decompression logic, and `Streams/ZstdStream.Async.*.cs` adds async entry points.
- Buffer management: relies on `ArrayPool<byte>.Shared` to limit GC pressure.
- Compression pipeline: repeatedly call `encoder.Compress`, using empty input to trigger flushes.
- Concurrency guard: CAS on `activeOperation` prevents simultaneous operations on a single instance.
- Unsafe confinement: `unsafe` usage is isolated to interop; public APIs remain safe.
- Performance guidance: prefer chunk sizes above 8 KB and avoid sharing a single stream instance across threads.

Example compression level adjustment:

```csharp
using var zs = new ZstdStream(output, CompressionMode.Compress)
{
    CompressionLevel = 10
};
```

Higher levels cost more CPU—pick the right trade-off for your workload.

## Building & testing

```pwsh
pwsh> dotnet test
```

The suite currently covers flush semantics, async streaming, multi-frame behavior, edge cases, fuzz input, concurrency guards, and frame inspection (49 tests total).

## Native assets package

### Goals & layout

`ZstdDotnet.NativeAssets` publishes cross-platform `libzstd` binaries for consumption by the managed package.

```text
root
 ├─ src/ZstdDotnet.NativeAssets/  # Packing project
 ├─ bin/                          # Drop native artifacts here
 │   ├─ libzstd.dll (win-x64)
 │   └─ libzstd.so  (linux-x64)
 └─ licenses/                     # MIT + upstream licenses
```

Pack the binaries into the NuGet package with:

```xml
<None Include="../../bin/libzstd.dll" Pack="true" PackagePath="runtimes/win-x64/native" />
<None Include="../../bin/libzstd.so"  Pack="true" PackagePath="runtimes/linux-x64/native" />
```

### Building libzstd

1. Clone the upstream source:

   ```pwsh
   git clone https://github.com/facebook/zstd.git
   cd zstd
   git checkout v$(xmlstarlet sel -t -v '/Project/PropertyGroup/PackageVersion' ..\src\ZstdDotnet.NativeAssets\ZstdDotnet.NativeAssets.csproj)
   ```

2. Build with CMake (recommended):

   ```pwsh
   mkdir build
   cd build
   cmake -DZSTD_BUILD_SHARED=ON -DZSTD_BUILD_STATIC=OFF -DCMAKE_BUILD_TYPE=Release ..
   cmake --build . --config Release --target zstd
   ```

   Copy the resulting `zstd.dll`/`libzstd.so` into the repository `bin/` directory.

3. On Linux you may also run `make -C build/cmake install` to produce `libzstd.so`.

Optional validation:

```pwsh
dumpbin /exports bin\libzstd.dll | Select-String ZSTD_version
nm -D bin/libzstd.so | grep ZSTD_version
```

### Packing & publishing

```pwsh
dotnet pack src/ZstdDotnet.NativeAssets/ZstdDotnet.NativeAssets.csproj -c Release -o out
dotnet nuget push out/ZstdDotnet.NativeAssets.<version>.nupkg -k <API_KEY> -s https://api.nuget.org/v3/index.json
```

Workflow **Build Native Package**:

1. Reads `<PackageVersion>` from `src/ZstdDotnet.NativeAssets/ZstdDotnet.NativeAssets.csproj`.
2. Downloads the matching upstream release, compiles/extracts the native binaries.
3. Runs `dotnet pack` to produce the NuGet package.
4. Pushes the package to NuGet using the `NUGET_DEPLOY_KEY` secret (skipping duplicates).

Managed package publishing is handled by **Publish ZstdDotnet Package**:

1. Reads `<PackageVersion>` from `src/ZstdDotnet/ZstdDotnet.csproj` (must already be four-part).
2. Synchronizes the native dependency version inside the same project file.
3. Optionally runs tests (toggle via the `skip-tests` workflow input).
4. Runs `dotnet pack` and pushes to NuGet with `--skip-duplicate`.

### Upgrade checklist

1. Update `<PackageVersion>` in both `src/ZstdDotnet/ZstdDotnet.csproj` and `src/ZstdDotnet.NativeAssets/ZstdDotnet.NativeAssets.csproj`.
2. Trigger the native and/or managed publishing workflows as appropriate.
3. Refresh version numbers in this README and docs if needed.
4. Verify the newly published packages on NuGet.org.

## Benchmarks

Run the BenchmarkDotNet project in `benchmark/ZstdDotnet.Benchmark`:

```pwsh
dotnet run -c Release --project benchmark/ZstdDotnet.Benchmark -- --filter *LibraryCompressionBench*
```

Filter specific cases:

```pwsh
dotnet run -c Release --project benchmark/ZstdDotnet.Benchmark -- --filter *Zstd_Compress_LibCompare_Async*
```

Find results under `BenchmarkDotNet.Artifacts/results/`. Use `--job Dry` for a quick sanity pass.

## Additional documentation

- [docs/LowLevel.md](docs/LowLevel.md): Low-level encoder/decoder and static helper examples
- [docs/Advanced.md](docs/Advanced.md): Advanced parameters, dictionary roadmap, tuning hints
- [docs/FAQ.md](docs/FAQ.md): Frequently asked questions
- [docs/Benchmark.md](docs/Benchmark.md): Extended benchmarking guide
- [docs/TestMatrix.md](docs/TestMatrix.md): Test trait reference

## License

Distributed under the MIT License. Upstream Zstandard library code used here (only the `lib` directory, built as a dynamically linked native library) is under the 3-clause BSD license. See `licenses/THIRD-PARTY-NOTICES.txt` for architectural usage notes and `licenses/zstdlicense.txt` for the full upstream BSD text.

## Acknowledgements

- Facebook/Meta Zstandard maintainers for the reference implementation
- The .NET team and community for Span/Stream innovations

Questions or ideas? Open an issue or pull request.




















































