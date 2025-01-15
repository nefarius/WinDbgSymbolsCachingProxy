using Kaitai;

namespace WinDbgSymbolsCachingProxy.Kaitai.PDB;

public class Concat
{
    private readonly ICollection<byte[]> _blocks;

    public Concat(ICollection<MsPdb.PdbPage> items)
    {
        _blocks = items.Select(itm => itm.Data).ToList();
    }

    public byte[] Decode(byte[] src)
    {
        return _blocks.SelectMany(arr => arr).ToArray();
    }
}

public class ConcatPages
{
    private readonly ICollection<MsPdb.PdbPage> _items;

    public ConcatPages(ICollection<MsPdb.PdbPageNumber> pages)
    {
        _items = pages.Select(p => p.Page).ToList();
    }

    public byte[] Decode(byte[] src)
    {
        return new Concat(_items).Decode(src);
    }
}

public class Cat
{
    private readonly byte[] _item;

    public Cat(byte[] item)
    {
        _item = item;
    }

    public byte[] Decode(byte[] _)
    {
        return _item;
    }
}