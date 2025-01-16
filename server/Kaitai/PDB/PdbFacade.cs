using Kaitai;

namespace WinDbgSymbolsCachingProxy.Kaitai.PDB
{
    internal class PdbFacade
    {
        private readonly MsPdb _pdb;

        public PdbFacade(MsPdb pdb)
        {
            _pdb = pdb;
        }

        public void Test()
        {
            foreach (var t in _pdb.TpiStream.Types.Types)
            {
                var data = t.Data.Body;
                switch (data)
                {
                    case MsPdb.LfFieldlist i:
                        break;
                    default:
                        throw new NotImplementedException(data.GetType().FullName);
                }
            }
        }

        public IEnumerable<KaitaiStruct> GetSymbols()
        {
            return _pdb.DbiStream.ModulesList.Items
            .Select(mod => mod?.ModuleData?.SymbolsList?.Items)
            .Where(list => list != null)
            .SelectMany(list => list!)
            .Select(sym => sym.Data.Body)
            .Where(sym => sym != null);
        }
    }
}
