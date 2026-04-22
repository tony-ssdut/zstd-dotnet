namespace System.IO.Compression.Zstandard.Tests;

public class ZstandardCompressionOptionsTests
{
    [Fact]
    public void Quality_AcceptsZeroAndBounds()
    {
        var options = new ZstandardCompressionOptions();

        options.Quality = 0;
        Assert.Equal(0, options.Quality);

        options.Quality = ZstandardCompressionOptions.MinQuality;
        Assert.Equal(ZstandardCompressionOptions.MinQuality, options.Quality);

        options.Quality = ZstandardCompressionOptions.MaxQuality;
        Assert.Equal(ZstandardCompressionOptions.MaxQuality, options.Quality);
    }

    [Fact]
    public void Quality_ThrowsOutsideBounds()
    {
        var options = new ZstandardCompressionOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.Quality = ZstandardCompressionOptions.MinQuality - 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => options.Quality = ZstandardCompressionOptions.MaxQuality + 1);
    }

    [Fact]
    public void WindowLog_ThrowsOutsideBounds()
    {
        var options = new ZstandardCompressionOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.WindowLog = ZstandardCompressionOptions.MinWindowLog - 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => options.WindowLog = ZstandardCompressionOptions.MaxWindowLog + 1);
    }

    [Fact]
    public void TargetBlockSize_ThrowsOutsideBounds()
    {
        var options = new ZstandardCompressionOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.TargetBlockSize = 1339);
        Assert.Throws<ArgumentOutOfRangeException>(() => options.TargetBlockSize = 131073);
    }
}