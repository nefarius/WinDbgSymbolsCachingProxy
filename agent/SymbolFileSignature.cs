using System.Text;

namespace HarvestingAgent;

/// <summary>
///     Validates that a symbol file has finished being written by checking a file-format specific magic
///     signature. The goal is to avoid uploading empty, truncated, or still-being-written files that the
///     symbol server could not parse.
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
    ///     Returns whether the given stream starts with a valid, format-specific magic signature for the
    ///     file's extension. Returns <c>true</c> for unknown/unsupported extensions so we don't block
    ///     uploads of files we weren't asked to validate.
    /// </summary>
    /// <param name="stream">A readable, seekable stream positioned anywhere; position is restored on exit.</param>
    /// <param name="filePath">Full path; only the extension is used for format detection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task<bool> HasValidMagicAsync(Stream stream, string filePath, CancellationToken cancellationToken)
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
                ".pdb" => await HasPdbMagicAsync(stream, cancellationToken),
                ".exe" or ".dll" or ".sys" => await HasPeMagicAsync(stream, cancellationToken),
                _ => true,
            };
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static async Task<bool> HasPdbMagicAsync(Stream stream, CancellationToken cancellationToken)
    {
        int required = Math.Max(PdbMsfBigMagic.Length, PdbMsfSmallMagic.Length);
        if (stream.Length < required)
        {
            return false;
        }

        byte[] buffer = new byte[required];
        stream.Position = 0;
        int read = await stream.ReadAsync(buffer.AsMemory(0, required), cancellationToken);
        if (read < required)
        {
            return false;
        }

        return StartsWith(buffer, PdbMsfBigMagic) || StartsWith(buffer, PdbMsfSmallMagic);
    }

    private static async Task<bool> HasPeMagicAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.Length < MinimumPeFileSize)
        {
            return false;
        }

        byte[] buffer = new byte[2];
        stream.Position = 0;
        int read = await stream.ReadAsync(buffer.AsMemory(0, 2), cancellationToken);
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
