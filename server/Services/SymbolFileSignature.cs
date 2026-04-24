using System.Text;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Validates that an uploaded symbol file has a plausible, complete header for the declared file
///     type. The goal is to fail fast with a clear error when a client (typically the harvesting agent)
///     uploaded a truncated or still-being-written file, instead of bubbling up a low-level
///     <c>OverflowException</c> or similar from deep inside one of the PDB parsers.
/// </summary>
internal static class SymbolFileSignature
{
    /// <summary>
    ///     Header for modern "big" PDB format (MSF 7.00). Actual files always begin with this exact ASCII prefix.
    /// </summary>
    private static readonly byte[] PdbMsfBigMagic = Encoding.ASCII.GetBytes("Microsoft C/C++ MSF 7.00\r\n");

    /// <summary>
    ///     Header for legacy "small" PDB format (2.00). Rarely seen today but kept for compatibility.
    /// </summary>
    private static readonly byte[] PdbMsfSmallMagic = Encoding.ASCII.GetBytes("Microsoft C/C++ program database 2.00\r\n");

    /// <summary>
    ///     Minimum expected file size for a PE file to contain a valid DOS header + PE header offset.
    /// </summary>
    private const int MinimumPeFileSize = 64;

    /// <summary>
    ///     Validates that the provided stream starts with the magic signature matching the file
    ///     extension. For unknown/unsupported extensions no validation is performed and <c>true</c> is
    ///     returned (the caller's switch on extension decides what to do next).
    /// </summary>
    /// <param name="stream">Seekable, readable stream. Position is restored on exit.</param>
    /// <param name="filePath">Path or file name; only the extension is used.</param>
    public static bool HasValidMagic(Stream stream, string filePath)
    {
        if (!stream.CanSeek || !stream.CanRead)
        {
            return true;
        }

        long originalPosition = stream.Position;
        try
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".pdb" => HasPdbMagic(stream),
                ".exe" or ".dll" or ".sys" => HasPeMagic(stream),
                _ => true,
            };
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static bool HasPdbMagic(Stream stream)
    {
        int required = Math.Max(PdbMsfBigMagic.Length, PdbMsfSmallMagic.Length);
        if (stream.Length < required)
        {
            return false;
        }

        Span<byte> buffer = stackalloc byte[64];
        stream.Position = 0;
        int read = stream.Read(buffer[..required]);
        if (read < required)
        {
            return false;
        }

        return StartsWith(buffer, PdbMsfBigMagic) || StartsWith(buffer, PdbMsfSmallMagic);
    }

    private static bool HasPeMagic(Stream stream)
    {
        if (stream.Length < MinimumPeFileSize)
        {
            return false;
        }

        Span<byte> buffer = stackalloc byte[2];
        stream.Position = 0;
        int read = stream.Read(buffer);
        if (read < 2)
        {
            return false;
        }

        return buffer[0] == (byte)'M' && buffer[1] == (byte)'Z';
    }

    private static bool StartsWith(ReadOnlySpan<byte> source, ReadOnlySpan<byte> prefix)
    {
        if (source.Length < prefix.Length)
        {
            return false;
        }

        for (int i = 0; i < prefix.Length; i++)
        {
            if (source[i] != prefix[i])
            {
                return false;
            }
        }

        return true;
    }
}
