using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Attributes;
using System.IO.Compression;

namespace ZstdDotnet.PackageComparison.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ZstdPackageComparisonBenchmarks
{
    private byte[] _payload = null!;
    private byte[] _compressedBySystemPackage = null!;

    [Params(32_768, 262_144)]
    public int PayloadSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Repeatable mixed-entropy payload to represent realistic content.
        _payload = BuildPayload(PayloadSize);
        _compressedBySystemPackage = CompressWithSystemPackage(_payload);
    }

    [Benchmark(Baseline = true)]
    public byte[] Compress_ZstdDotnet_Stream()
    {
        using var output = new MemoryStream();
        using (var stream = new ZstdStream(output, CompressionMode.Compress, leaveOpen: true))
        {
            stream.Write(_payload, 0, _payload.Length);
        }

        return output.ToArray();
    }

    [Benchmark]
    public byte[] Compress_SystemIOCompressionZstandard_Stream()
    {
        using var output = new MemoryStream();
        using (var stream = new ZstandardStream(output, CompressionMode.Compress, leaveOpen: true))
        {
            stream.Write(_payload);
        }

        return output.ToArray();
    }

    [Benchmark]
    public byte[] Decompress_ZstdDotnet_Stream()
    {
        using var input = new MemoryStream(_compressedBySystemPackage, writable: false);
        using var stream = new ZstdStream(input, CompressionMode.Decompress, leaveOpen: false);
        using var output = new MemoryStream(_payload.Length);

        var buffer = new byte[8192];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }

        return output.ToArray();
    }

    [Benchmark]
    public byte[] Decompress_SystemIOCompressionZstandard_Stream()
    {
        using var input = new MemoryStream(_compressedBySystemPackage, writable: false);
        using var stream = new ZstandardStream(input, CompressionMode.Decompress, leaveOpen: false);
        using var output = new MemoryStream(_payload.Length);

        var buffer = new byte[8192];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }

        return output.ToArray();
    }

    private static byte[] CompressWithSystemPackage(byte[] source)
    {
        using var output = new MemoryStream();
        using (var stream = new ZstandardStream(output, CompressionMode.Compress, leaveOpen: true))
        {
            stream.Write(source);
        }

        return output.ToArray();
    }

    private static byte[] BuildPayload(int size)
    {
        var payload = new byte[size];
        var random = new Random(20260424);
        random.NextBytes(payload);

        // Add structured runs to avoid purely random incompressible data.
        for (int i = 0; i < payload.Length; i += 97)
        {
            payload[i] = (byte)(i % 251);
        }

        return payload;
    }
}