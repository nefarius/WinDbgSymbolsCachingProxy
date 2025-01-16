using System.Runtime.InteropServices;

namespace WinDbgSymbolsCachingProxy.Kaitai.PDB;

public class Hasher
{
    public static uint HashUlong(uint value)
    {
        // From Numerical Recipes in C, second edition, pg 284. 
        return (value * 1664525) + 1013904223;
    }

    public static uint HashV1(Memory<byte> buf, uint modulo)
    {
        uint nWords = (uint)(buf.Length >> 2);
        throw new NotImplementedException();
    }

    public static uint HashV2(Memory<byte> buf, uint modulo)
    {
        uint hash = 0xb170a1bf;
        Span<uint> dwords = MemoryMarshal.Cast<byte, uint>(buf.Span);
        foreach (uint dw in dwords)
        {
            hash += dw;
            hash += hash << 10;
            hash ^= hash >> 6;
        }

        Span<byte> remaining = buf.Span.Slice(buf.Length - (buf.Length % sizeof(uint)));
        foreach (byte b in remaining)
        {
            hash += b;
            hash += hash << 10;
            hash ^= hash >> 6;
        }

        return HashUlong(hash) % modulo;
    }
}