using Kaitai;

namespace WinDbgSymbolsCachingProxy.Kaitai.PDB;

internal record TypeNode(KaitaiStruct type)
{
    public IList<TypeNode> XRefs = new List<TypeNode>();
}