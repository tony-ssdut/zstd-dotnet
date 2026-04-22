namespace System.IO.Compression.Zstandard.Tests;

public class ZstandardEncoderDecoderTests
{
    [Fact]
    public void GetMaxCompressedLength_NegativeInput_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ZstandardEncoder.GetMaxCompressedLength(-1));
    }

    [Fact]
    public void TryCompressThenTryDecompress_RoundTrips()
    {
        byte[] original = Enumerable.Range(0, 4096).Select(i => (byte)(i % 251)).ToArray();

        byte[] compressed = new byte[ZstandardEncoder.GetMaxCompressedLength(original.Length)];
        bool compressedOk = ZstandardEncoder.TryCompress(original, compressed, out int compressedWritten);

        Assert.True(compressedOk);
        Assert.True(compressedWritten > 0);

        byte[] decompressed = new byte[original.Length];
        bool decompressedOk = ZstandardDecoder.TryDecompress(compressed.AsSpan(0, compressedWritten), decompressed, out int decompressedWritten);

        Assert.True(decompressedOk);
        Assert.Equal(original.Length, decompressedWritten);
        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void TryCompress_WithTooSmallDestination_ReturnsFalse()
    {
        byte[] original = Enumerable.Repeat((byte)7, 1024).ToArray();
        Span<byte> destination = stackalloc byte[8];

        bool ok = ZstandardEncoder.TryCompress(original, destination, out int bytesWritten);

        Assert.False(ok);
        Assert.Equal(0, bytesWritten);
    }

    [Fact]
    public void TryGetMaxDecompressedLength_EmptyInput_ReturnsZeroLength()
    {
        bool ok = ZstandardDecoder.TryGetMaxDecompressedLength(ReadOnlySpan<byte>.Empty, out long length);

        Assert.True(ok);
        Assert.Equal(0, length);
    }
}