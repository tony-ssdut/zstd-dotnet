using System.Resources;

namespace System.IO.Compression;

internal static class SR
{
    private static readonly ResourceManager s_resourceManager =
        new ResourceManager("System.IO.Compression.Zstandard.Resources.Strings", typeof(SR).Assembly);

    private static string GetString(string name) =>
        s_resourceManager.GetString(name) ?? name;

    internal static string ArgumentOutOfRange_Enum => GetString(nameof(ArgumentOutOfRange_Enum));
    internal static string CannotReadFromDeflateStream => GetString(nameof(CannotReadFromDeflateStream));
    internal static string CannotWriteToDeflateStream => GetString(nameof(CannotWriteToDeflateStream));
    internal static string GenericInvalidData => GetString(nameof(GenericInvalidData));
    internal static string InvalidBeginCall => GetString(nameof(InvalidBeginCall));
    internal static string InvalidBlockLength => GetString(nameof(InvalidBlockLength));
    internal static string InvalidHuffmanData => GetString(nameof(InvalidHuffmanData));
    internal static string NotSupported => GetString(nameof(NotSupported));
    internal static string NotSupported_UnreadableStream => GetString(nameof(NotSupported_UnreadableStream));
    internal static string NotSupported_UnwritableStream => GetString(nameof(NotSupported_UnwritableStream));
    internal static string UnknownBlockType => GetString(nameof(UnknownBlockType));
    internal static string UnknownState => GetString(nameof(UnknownState));
    internal static string ZLibErrorDLLLoadError => GetString(nameof(ZLibErrorDLLLoadError));
    internal static string ZLibErrorInconsistentStream => GetString(nameof(ZLibErrorInconsistentStream));

    // Zstandard-specific strings
    internal static string ZstandardEncoderDecoder_InvalidState => GetString(nameof(ZstandardEncoderDecoder_InvalidState));
    internal static string ZstandardDictionary_EmptyBuffer => GetString(nameof(ZstandardDictionary_EmptyBuffer));
    internal static string ZstandardDictionary_CreateCompressionFailed => GetString(nameof(ZstandardDictionary_CreateCompressionFailed));
    internal static string ZstandardDictionary_CreateDecompressionFailed => GetString(nameof(ZstandardDictionary_CreateDecompressionFailed));
    internal static string ZstandardDictionary_Train_MinimumSampleCount => GetString(nameof(ZstandardDictionary_Train_MinimumSampleCount));
    internal static string ZstandardDictionary_Train_InvalidSampleLength => GetString(nameof(ZstandardDictionary_Train_InvalidSampleLength));
    internal static string ZstandardDictionary_SampleLengthsMismatch => GetString(nameof(ZstandardDictionary_SampleLengthsMismatch));
    internal static string CannotWriteToDecompressionStream => GetString(nameof(CannotWriteToDecompressionStream));
    internal static string CannotReadFromCompressionStream => GetString(nameof(CannotReadFromCompressionStream));
    internal static string ZstandardStream_Compress_InvalidData => GetString(nameof(ZstandardStream_Compress_InvalidData));
    internal static string ZstandardStream_Decompress_InvalidData => GetString(nameof(ZstandardStream_Decompress_InvalidData));
    internal static string ZstandardStream_Decompress_InvalidStream => GetString(nameof(ZstandardStream_Decompress_InvalidStream));
    internal static string ZstandardStream_Decompress_TruncatedData => GetString(nameof(ZstandardStream_Decompress_TruncatedData));
    internal static string ZstandardStream_ConcurrentRWOperation => GetString(nameof(ZstandardStream_ConcurrentRWOperation));
    internal static string ZstandardDecoder_WindowTooLarge => GetString(nameof(ZstandardDecoder_WindowTooLarge));
    internal static string ZstandardDecoder_DictionaryWrong => GetString(nameof(ZstandardDecoder_DictionaryWrong));
    internal static string Zstd_InternalError => GetString(nameof(Zstd_InternalError));
    internal static string Stream_FalseCanWrite => GetString(nameof(Stream_FalseCanWrite));
    internal static string Stream_FalseCanRead => GetString(nameof(Stream_FalseCanRead));

    internal static string Format(string resourceFormat, params object[] args) =>
        string.Format(resourceFormat, args);
}
