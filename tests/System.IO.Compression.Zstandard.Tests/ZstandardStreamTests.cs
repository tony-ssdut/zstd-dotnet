using System.Text;

namespace System.IO.Compression.Zstandard.Tests;

public class ZstandardStreamTests
{
    [Fact]
    public void Ctor_CompressMode_ThrowsWhenBaseStreamIsNotWritable()
    {
        using var readOnly = new MemoryStream(new byte[16], writable: false);
        Assert.Throws<ArgumentException>(() => new ZstandardStream(readOnly, CompressionMode.Compress));
    }

    [Fact]
    public void Ctor_DecompressMode_ThrowsWhenBaseStreamIsNotReadable()
    {
        using var notReadable = new NonReadableMemoryStream();
        Assert.Throws<ArgumentException>(() => new ZstandardStream(notReadable, CompressionMode.Decompress));
    }

    [Fact]
    public void ModeCapabilities_ExposeExpectedCanReadAndCanWrite()
    {
        using var compressBase = new MemoryStream();
        using var compressStream = new ZstandardStream(compressBase, CompressionMode.Compress, leaveOpen: true);

        Assert.False(compressStream.CanRead);
        Assert.True(compressStream.CanWrite);
        Assert.False(compressStream.CanSeek);

        byte[] compressed = CompressUtf8("hello");
        using var decompressBase = new MemoryStream(compressed);
        using var decompressStream = new ZstandardStream(decompressBase, CompressionMode.Decompress);

        Assert.True(decompressStream.CanRead);
        Assert.False(decompressStream.CanWrite);
        Assert.False(decompressStream.CanSeek);
    }

    [Fact]
    public void UnsupportedMembers_ThrowNotSupported()
    {
        using var baseStream = new MemoryStream();
        using var stream = new ZstandardStream(baseStream, CompressionMode.Compress, leaveOpen: true);

        Assert.Throws<NotSupportedException>(() => _ = stream.Length);
        Assert.Throws<NotSupportedException>(() => _ = stream.Position);
        Assert.Throws<NotSupportedException>(() => stream.Position = 0);
        Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        Assert.Throws<NotSupportedException>(() => stream.SetLength(0));
    }

    [Fact]
    public void LeaveOpenFalse_DisposesUnderlyingStream()
    {
        var baseStream = new MemoryStream();

        using (var stream = new ZstandardStream(baseStream, CompressionMode.Compress, leaveOpen: false))
        {
            stream.WriteByte(1);
        }

        Assert.Throws<ObjectDisposedException>(() => baseStream.WriteByte(1));
    }

    [Fact]
    public void LeaveOpenTrue_KeepsUnderlyingStreamOpen()
    {
        var baseStream = new MemoryStream();

        using (var stream = new ZstandardStream(baseStream, CompressionMode.Compress, leaveOpen: true))
        {
            stream.WriteByte(1);
        }

        baseStream.WriteByte(2);
        Assert.True(baseStream.Length > 0);
    }

    [Fact]
    public void SetSourceLength_OnDecompressStream_ThrowsInvalidOperationException()
    {
        byte[] compressed = CompressUtf8("payload");
        using var baseStream = new MemoryStream(compressed);
        using var stream = new ZstandardStream(baseStream, CompressionMode.Decompress);

        Assert.Throws<InvalidOperationException>(() => stream.SetSourceLength(10));
    }

    [Fact]
    public void Read_FromCompressMode_ThrowsInvalidOperationException()
    {
        using var baseStream = new MemoryStream();
        using var stream = new ZstandardStream(baseStream, CompressionMode.Compress);
        byte[] buffer = new byte[8];

        Assert.Throws<InvalidOperationException>(() => stream.Read(buffer, 0, buffer.Length));
    }

    [Fact]
    public void Write_ToDecompressMode_ThrowsInvalidOperationException()
    {
        byte[] compressed = CompressUtf8("payload");
        using var baseStream = new MemoryStream(compressed);
        using var stream = new ZstandardStream(baseStream, CompressionMode.Decompress);

        Assert.Throws<InvalidOperationException>(() => stream.Write("bad"u8));
    }

    [Fact]
    public void RoundTrip_Sync_CompressAndDecompress()
    {
        byte[] original = Encoding.UTF8.GetBytes(string.Join('-', Enumerable.Repeat("zstd-sync-roundtrip", 256)));

        using var compressedBuffer = new MemoryStream();
        using (var compressor = new ZstandardStream(compressedBuffer, CompressionLevel.Optimal, leaveOpen: true))
        {
            compressor.Write(original);
        }

        compressedBuffer.Position = 0;
        using var decompressor = new ZstandardStream(compressedBuffer, CompressionMode.Decompress);
        using var output = new MemoryStream();

        byte[] readBuffer = new byte[1024];
        int read;
        while ((read = decompressor.Read(readBuffer, 0, readBuffer.Length)) > 0)
        {
            output.Write(readBuffer, 0, read);
        }

        Assert.Equal(original, output.ToArray());
    }

    [Fact]
    public async Task RoundTrip_Async_CompressAndDecompress()
    {
        byte[] original = Encoding.UTF8.GetBytes(string.Join(':', Enumerable.Repeat("zstd-async-roundtrip", 300)));

        using var compressedBuffer = new MemoryStream();
        await using (var compressor = new ZstandardStream(compressedBuffer, CompressionLevel.Fastest, leaveOpen: true))
        {
            await compressor.WriteAsync(original);
            await compressor.FlushAsync();
        }

        compressedBuffer.Position = 0;
        await using var decompressor = new ZstandardStream(compressedBuffer, CompressionMode.Decompress);
        using var output = new MemoryStream();

        byte[] readBuffer = new byte[1024];
        int read;
        while ((read = await decompressor.ReadAsync(readBuffer, 0, readBuffer.Length)) > 0)
        {
            await output.WriteAsync(readBuffer, 0, read);
        }

        Assert.Equal(original, output.ToArray());
    }

    [Fact]
    public void RoundTrip_EmptyPayload_Works()
    {
        byte[] original = Array.Empty<byte>();

        using var compressedBuffer = new MemoryStream();
        using (var compressor = new ZstandardStream(compressedBuffer, CompressionMode.Compress, leaveOpen: true))
        {
            compressor.Write(original);
        }

        compressedBuffer.Position = 0;
        using var decompressor = new ZstandardStream(compressedBuffer, CompressionMode.Decompress);
        byte[] destination = new byte[16];

        int read = decompressor.Read(destination, 0, destination.Length);

        Assert.Equal(0, read);
    }

    [Fact]
    public void RoundTrip_WithSetSourceLength_Works()
    {
        byte[] original = Encoding.UTF8.GetBytes(string.Join('|', Enumerable.Repeat("set-source-length", 128)));

        using var compressedBuffer = new MemoryStream();
        using (var compressor = new ZstandardStream(compressedBuffer, CompressionMode.Compress, leaveOpen: true))
        {
            compressor.SetSourceLength(original.Length);
            compressor.Write(original);
        }

        compressedBuffer.Position = 0;
        using var decompressor = new ZstandardStream(compressedBuffer, CompressionMode.Decompress);
        using var output = new MemoryStream();

        byte[] readBuffer = new byte[257];
        int read;
        while ((read = decompressor.Read(readBuffer, 0, readBuffer.Length)) > 0)
        {
            output.Write(readBuffer, 0, read);
        }

        Assert.Equal(original, output.ToArray());
    }

    [Fact]
    public void RoundTrip_ChunkedWriteAndChunkedRead_Works()
    {
        byte[] original = Enumerable.Range(0, 20_000).Select(i => (byte)(i % 251)).ToArray();

        using var compressedBuffer = new MemoryStream();
        using (var compressor = new ZstandardStream(compressedBuffer, CompressionMode.Compress, leaveOpen: true))
        {
            int offset = 0;
            while (offset < original.Length)
            {
                int chunkSize = Math.Min(113, original.Length - offset);
                compressor.Write(original, offset, chunkSize);
                offset += chunkSize;
            }
        }

        compressedBuffer.Position = 0;
        using var decompressor = new ZstandardStream(compressedBuffer, CompressionMode.Decompress);
        using var output = new MemoryStream();

        byte[] readBuffer = new byte[97];
        int read;
        while ((read = decompressor.Read(readBuffer, 0, readBuffer.Length)) > 0)
        {
            output.Write(readBuffer, 0, read);
        }

        Assert.Equal(original, output.ToArray());
    }

    [Fact]
    public void Decompress_AfterEndOfStream_ReturnsZero()
    {
        byte[] compressed = CompressUtf8("read-to-end-once");

        using var compressedBuffer = new MemoryStream(compressed);
        using var decompressor = new ZstandardStream(compressedBuffer, CompressionMode.Decompress);

        byte[] buffer = new byte[256];
        while (decompressor.Read(buffer, 0, buffer.Length) > 0)
        {
        }

        int readAfterEnd = decompressor.Read(buffer, 0, buffer.Length);

        Assert.Equal(0, readAfterEnd);
    }

    [Fact]
    public async Task DecompressAsync_AfterEndOfStream_ReturnsZero()
    {
        byte[] compressed = CompressUtf8("read-to-end-async");

        using var compressedBuffer = new MemoryStream(compressed);
        await using var decompressor = new ZstandardStream(compressedBuffer, CompressionMode.Decompress);

        byte[] buffer = new byte[256];
        while (await decompressor.ReadAsync(buffer, 0, buffer.Length) > 0)
        {
        }

        int readAfterEnd = await decompressor.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(0, readAfterEnd);
    }

    private static byte[] CompressUtf8(string value)
    {
        byte[] input = Encoding.UTF8.GetBytes(value);
        using var compressed = new MemoryStream();
        using (var stream = new ZstandardStream(compressed, CompressionMode.Compress, leaveOpen: true))
        {
            stream.Write(input);
        }

        return compressed.ToArray();
    }

    private sealed class NonReadableMemoryStream : MemoryStream
    {
        public override bool CanRead => false;

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override int Read(Span<byte> buffer) => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}