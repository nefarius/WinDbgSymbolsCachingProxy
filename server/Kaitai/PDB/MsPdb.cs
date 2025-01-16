// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;

using WinDbgSymbolsCachingProxy.Kaitai.PDB;

namespace Kaitai
{
    public partial class MsPdb : KaitaiStruct
    {
        public static MsPdb FromFile(string fileName)
        {
            return new MsPdb(new KaitaiStream(fileName));
        }


        public enum PdbTypeEnum
        {
            Old = 0,
            Small = 1,
            Big = 2,
        }

        public enum DefaultStream
        {
            Pdb = 1,
            Tpi = 2,
            Dbi = 3,
            Ipi = 4,
        }

        public enum PdbVersion
        {
            V41 = 920924,
            V60 = 19960502,
            V50a = 19970116,
            V61 = 19980914,
            V69 = 19990511,
            V70Deprecated = 20000406,
            V70 = 20001102,
            V80 = 20030901,
            V110 = 20091201,
        }

        public enum PdbImplementationVersion
        {
            Vc2 = 19941610,
            Vc4 = 19950623,
            Vc41 = 19950814,
            Vc50 = 19960307,
            Vc98 = 19970604,
            Vc70Deprecated = 19990604,
            Vc70 = 20000404,
            Vc80 = 20030901,
            Vc110 = 20091201,
            Vc140 = 20140508,
        }
        public MsPdb(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            f_streamTable = false;
            f_pdbRootStream = false;
            f_zzzDbiData = false;
            f_zzzTpiData = false;
            f_dbiStream = false;
            f_pdbType = false;
            f_pageSize = false;
            f_numStreams = false;
            f_pageNumberSize = false;
            f_minTypeIndex = false;
            f_types = false;
            f_tpiStream = false;
            f_pageSizeDs = false;
            f_zzzPdbData = false;
            f_maxTypeIndex = false;
            f_pageSizeJg = false;
            _read();
        }
        private void _read()
        {
            _signature = new PdbSignature(m_io, this, m_root);
            if (Signature.Id == "DS") {
                _pdbDs = new PdbDsRoot(m_io, this, m_root);
            }
            if ( ((Signature.Id == "JG") && (Signature.VersionMajor == "2")) ) {
                _pdbJg = new PdbJgRoot(m_io, this, m_root);
            }
            if ( ((Signature.Id == "JG") && (Signature.VersionMajor == "1")) ) {
                _pdbJgOld = new PdbJgOldRoot(m_io, this, m_root);
            }
        }

        /// <remarks>
        /// Reference: ARMSWITCHTABLE
        /// </remarks>
        public partial class SymArmSwitchTable : KaitaiStruct
        {
            public static SymArmSwitchTable FromFile(string fileName)
            {
                return new SymArmSwitchTable(new KaitaiStream(fileName));
            }

            public SymArmSwitchTable(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offsetBase = m_io.ReadU4le();
                _baseSection = m_io.ReadU2le();
                _switchType = m_io.ReadU2le();
                _offsetBranch = m_io.ReadU4le();
                _offsetTable = m_io.ReadU4le();
                _branchSection = m_io.ReadU2le();
                _tableSection = m_io.ReadU2le();
                _numEntries = m_io.ReadU4le();
            }
            private uint _offsetBase;
            private ushort _baseSection;
            private ushort _switchType;
            private uint _offsetBranch;
            private uint _offsetTable;
            private ushort _branchSection;
            private ushort _tableSection;
            private uint _numEntries;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Section-relative offset to the base for switch offsets
            /// </summary>
            /// <remarks>
            /// Reference: offsetBase
            /// </remarks>
            public uint OffsetBase { get { return _offsetBase; } }

            /// <summary>
            /// Section index of the base for switch offsets
            /// </summary>
            /// <remarks>
            /// Reference: sectBase
            /// </remarks>
            public ushort BaseSection { get { return _baseSection; } }

            /// <summary>
            /// type of each entry
            /// </summary>
            /// <remarks>
            /// Reference: switchType
            /// </remarks>
            public ushort SwitchType { get { return _switchType; } }

            /// <summary>
            /// Section-relative offset to the table branch instruction
            /// </summary>
            /// <remarks>
            /// Reference: offsetBranch
            /// </remarks>
            public uint OffsetBranch { get { return _offsetBranch; } }

            /// <summary>
            /// Section-relative offset to the start of the table
            /// </summary>
            /// <remarks>
            /// Reference: offsetTable
            /// </remarks>
            public uint OffsetTable { get { return _offsetTable; } }

            /// <summary>
            /// Section index of the table branch instruction
            /// </summary>
            /// <remarks>
            /// Reference: sectBranch
            /// </remarks>
            public ushort BranchSection { get { return _branchSection; } }

            /// <summary>
            /// Section index of the table
            /// </summary>
            /// <remarks>
            /// Reference: sectTable
            /// </remarks>
            public ushort TableSection { get { return _tableSection; } }

            /// <summary>
            /// number of switch table entries
            /// </summary>
            /// <remarks>
            /// Reference: cEntries
            /// </remarks>
            public uint NumEntries { get { return _numEntries; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_ARRAY
        /// </summary>
        /// <remarks>
        /// Reference: lfArray
        /// </remarks>
        public partial class LfArray : KaitaiStruct
        {
            public LfArray(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _elementType = new TpiTypeRef(m_io, this, m_root);
                _indexingType = new TpiTypeRef(m_io, this, m_root);
                _size = new CvNumericType(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private TpiTypeRef _elementType;
            private TpiTypeRef _indexingType;
            private CvNumericType _size;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// type index of element type
            /// </summary>
            /// <remarks>
            /// Reference: elemtype
            /// </remarks>
            public TpiTypeRef ElementType { get { return _elementType; } }

            /// <summary>
            /// type index of indexing type
            /// </summary>
            /// <remarks>
            /// Reference: idxtype
            /// </remarks>
            public TpiTypeRef IndexingType { get { return _indexingType; } }

            /// <summary>
            /// variable length data specifying size in bytes
            /// </summary>
            /// <remarks>
            /// Reference: data.size
            /// </remarks>
            public CvNumericType Size { get { return _size; } }

            /// <summary>
            /// array name
            /// </summary>
            /// <remarks>
            /// Reference: data.name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class PdbBitsetWord : KaitaiStruct
        {
            public static PdbBitsetWord FromFile(string fileName)
            {
                return new PdbBitsetWord(new KaitaiStream(fileName));
            }

            public PdbBitsetWord(KaitaiStream p__io, MsPdb.PdbBitset p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _bits = new List<bool>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _bits.Add(m_io.ReadBitsIntLe(1) != 0);
                        i++;
                    }
                }
            }
            private List<bool> _bits;
            private MsPdb m_root;
            private MsPdb.PdbBitset m_parent;
            public List<bool> Bits { get { return _bits; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbBitset M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CV_modifier_t
        /// </remarks>
        public partial class LfModifierFlags : KaitaiStruct
        {
            public static LfModifierFlags FromFile(string fileName)
            {
                return new LfModifierFlags(new KaitaiStream(fileName));
            }

            public LfModifierFlags(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _const = m_io.ReadBitsIntLe(1) != 0;
                _volatile = m_io.ReadBitsIntLe(1) != 0;
                _unaligned = m_io.ReadBitsIntLe(1) != 0;
                __unnamed3 = m_io.ReadBitsIntLe(13);
            }
            private bool _const;
            private bool _volatile;
            private bool _unaligned;
            private ulong __unnamed3;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// TRUE if constant
            /// </summary>
            /// <remarks>
            /// Reference: MOD_const
            /// </remarks>
            public bool Const { get { return _const; } }

            /// <summary>
            /// TRUE if volatile
            /// </summary>
            /// <remarks>
            /// Reference: MOD_volatile
            /// </remarks>
            public bool Volatile { get { return _volatile; } }

            /// <summary>
            /// TRUE if unaligned
            /// </summary>
            /// <remarks>
            /// Reference: MOD_unaligned
            /// </remarks>
            public bool Unaligned { get { return _unaligned; } }

            /// <remarks>
            /// Reference: MOD_unused
            /// </remarks>
            public ulong Unnamed_3 { get { return __unnamed3; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: lfPointerAttr_16t
        /// </remarks>
        public partial class LfPointerAttributes16t : KaitaiStruct
        {
            public static LfPointerAttributes16t FromFile(string fileName)
            {
                return new LfPointerAttributes16t(new KaitaiStream(fileName));
            }

            public LfPointerAttributes16t(KaitaiStream p__io, MsPdb.LfPointer16t p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _pointerType = ((MsPdb.Tpi.CvPtrtype) m_io.ReadBitsIntLe(5));
                _pointerMode = ((MsPdb.Tpi.CvPtrmode) m_io.ReadBitsIntLe(3));
                _isFlat32 = m_io.ReadBitsIntLe(1) != 0;
                _isVolatile = m_io.ReadBitsIntLe(1) != 0;
                _isConst = m_io.ReadBitsIntLe(1) != 0;
                _isUnaligned = m_io.ReadBitsIntLe(1) != 0;
                __unnamed6 = m_io.ReadBitsIntLe(4);
            }
            private Tpi.CvPtrtype _pointerType;
            private Tpi.CvPtrmode _pointerMode;
            private bool _isFlat32;
            private bool _isVolatile;
            private bool _isConst;
            private bool _isUnaligned;
            private ulong __unnamed6;
            private MsPdb m_root;
            private MsPdb.LfPointer16t m_parent;

            /// <summary>
            /// ordinal specifying pointer type (CV_ptrtype_e)
            /// </summary>
            /// <remarks>
            /// Reference: ptrtype
            /// </remarks>
            public Tpi.CvPtrtype PointerType { get { return _pointerType; } }

            /// <summary>
            /// ordinal specifying pointer mode (CV_ptrmode_e)
            /// </summary>
            /// <remarks>
            /// Reference: ptrmode
            /// </remarks>
            public Tpi.CvPtrmode PointerMode { get { return _pointerMode; } }

            /// <summary>
            /// true if 0:32 pointer
            /// </summary>
            /// <remarks>
            /// Reference: isflat32
            /// </remarks>
            public bool IsFlat32 { get { return _isFlat32; } }

            /// <summary>
            /// TRUE if volatile pointer
            /// </summary>
            /// <remarks>
            /// Reference: isvolatile
            /// </remarks>
            public bool IsVolatile { get { return _isVolatile; } }

            /// <summary>
            /// TRUE if const pointer
            /// </summary>
            /// <remarks>
            /// Reference: isconst
            /// </remarks>
            public bool IsConst { get { return _isConst; } }

            /// <summary>
            /// TRUE if unaligned pointer
            /// </summary>
            /// <remarks>
            /// Reference: isunaligned
            /// </remarks>
            public bool IsUnaligned { get { return _isUnaligned; } }

            /// <summary>
            /// unused
            /// </summary>
            /// <remarks>
            /// Reference: unused
            /// </remarks>
            public ulong Unnamed_6 { get { return __unnamed6; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.LfPointer16t M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CV_prop_t
        /// </remarks>
        public partial class CvProperties : KaitaiStruct
        {
            public static CvProperties FromFile(string fileName)
            {
                return new CvProperties(new KaitaiStream(fileName));
            }

            public CvProperties(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _packed = m_io.ReadBitsIntLe(1) != 0;
                _ctor = m_io.ReadBitsIntLe(1) != 0;
                _overlappedOperators = m_io.ReadBitsIntLe(1) != 0;
                _isNested = m_io.ReadBitsIntLe(1) != 0;
                _containsNested = m_io.ReadBitsIntLe(1) != 0;
                _overlappedAssignment = m_io.ReadBitsIntLe(1) != 0;
                _castingMethods = m_io.ReadBitsIntLe(1) != 0;
                _forwardReference = m_io.ReadBitsIntLe(1) != 0;
                _scopedDefinition = m_io.ReadBitsIntLe(1) != 0;
                _hasUniqueName = m_io.ReadBitsIntLe(1) != 0;
                _sealed = m_io.ReadBitsIntLe(1) != 0;
                _hfa = ((MsPdb.Tpi.CvHfa) m_io.ReadBitsIntLe(2));
                _intrinsic = m_io.ReadBitsIntLe(1) != 0;
                _mocom = ((MsPdb.Tpi.CvMocomUdt) m_io.ReadBitsIntLe(2));
            }
            private bool _packed;
            private bool _ctor;
            private bool _overlappedOperators;
            private bool _isNested;
            private bool _containsNested;
            private bool _overlappedAssignment;
            private bool _castingMethods;
            private bool _forwardReference;
            private bool _scopedDefinition;
            private bool _hasUniqueName;
            private bool _sealed;
            private Tpi.CvHfa _hfa;
            private bool _intrinsic;
            private Tpi.CvMocomUdt _mocom;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// true if structure is packed
            /// </summary>
            /// <remarks>
            /// Reference: packed
            /// </remarks>
            public bool Packed { get { return _packed; } }

            /// <summary>
            /// true if constructors or destructors present
            /// </summary>
            /// <remarks>
            /// Reference: ctor
            /// </remarks>
            public bool Ctor { get { return _ctor; } }

            /// <summary>
            /// true if overloaded operators present
            /// </summary>
            /// <remarks>
            /// Reference: ovlops
            /// </remarks>
            public bool OverlappedOperators { get { return _overlappedOperators; } }

            /// <summary>
            /// true if this is a nested class
            /// </summary>
            /// <remarks>
            /// Reference: isnested
            /// </remarks>
            public bool IsNested { get { return _isNested; } }

            /// <summary>
            /// true if this class contains nested types
            /// </summary>
            /// <remarks>
            /// Reference: cnested
            /// </remarks>
            public bool ContainsNested { get { return _containsNested; } }

            /// <summary>
            /// true if overloaded assignment (=)
            /// </summary>
            /// <remarks>
            /// Reference: opassign
            /// </remarks>
            public bool OverlappedAssignment { get { return _overlappedAssignment; } }

            /// <summary>
            /// true if casting methods
            /// </summary>
            /// <remarks>
            /// Reference: opcast
            /// </remarks>
            public bool CastingMethods { get { return _castingMethods; } }

            /// <summary>
            /// true if forward reference (incomplete defn)
            /// </summary>
            /// <remarks>
            /// Reference: fwdref
            /// </remarks>
            public bool ForwardReference { get { return _forwardReference; } }

            /// <summary>
            /// scoped definition
            /// </summary>
            /// <remarks>
            /// Reference: scoped
            /// </remarks>
            public bool ScopedDefinition { get { return _scopedDefinition; } }

            /// <summary>
            /// true if there is a decorated name following the regular name
            /// </summary>
            /// <remarks>
            /// Reference: hasuniquename
            /// </remarks>
            public bool HasUniqueName { get { return _hasUniqueName; } }

            /// <summary>
            /// true if class cannot be used as a base class
            /// </summary>
            /// <remarks>
            /// Reference: sealed
            /// </remarks>
            public bool Sealed { get { return _sealed; } }

            /// <summary>
            /// CV_HFA_e
            /// </summary>
            /// <remarks>
            /// Reference: hfa
            /// </remarks>
            public Tpi.CvHfa Hfa { get { return _hfa; } }

            /// <summary>
            /// true if class is an intrinsic type (e.g. __m128d)
            /// </summary>
            /// <remarks>
            /// Reference: intrinsic
            /// </remarks>
            public bool Intrinsic { get { return _intrinsic; } }

            /// <summary>
            /// CV_MOCOM_UDT_e
            /// </summary>
            /// <remarks>
            /// Reference: mocom
            /// </remarks>
            public Tpi.CvMocomUdt Mocom { get { return _mocom; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SECTIONSYM
        /// </remarks>
        public partial class SymSection : KaitaiStruct
        {
            public static SymSection FromFile(string fileName)
            {
                return new SymSection(new KaitaiStream(fileName));
            }

            public SymSection(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _sectionIndex = m_io.ReadU2le();
                _sectionAlignment = m_io.ReadU1();
                _reserved = m_io.ReadU1();
                _rva = m_io.ReadU4le();
                _size = m_io.ReadU4le();
                _characteristics = m_io.ReadU4le();
                _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
            }
            private ushort _sectionIndex;
            private byte _sectionAlignment;
            private byte _reserved;
            private uint _rva;
            private uint _size;
            private uint _characteristics;
            private string _name;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Section number
            /// </summary>
            /// <remarks>
            /// Reference: isec
            /// </remarks>
            public ushort SectionIndex { get { return _sectionIndex; } }

            /// <summary>
            /// Alignment of this section (power of 2)
            /// </summary>
            /// <remarks>
            /// Reference: align
            /// </remarks>
            public byte SectionAlignment { get { return _sectionAlignment; } }

            /// <summary>
            /// Reserved.  Must be zero.
            /// </summary>
            /// <remarks>
            /// Reference: bReserved
            /// </remarks>
            public byte Reserved { get { return _reserved; } }

            /// <remarks>
            /// Reference: rva
            /// </remarks>
            public uint Rva { get { return _rva; } }

            /// <remarks>
            /// Reference: cb
            /// </remarks>
            public uint Size { get { return _size; } }

            /// <remarks>
            /// Reference: characteristics
            /// </remarks>
            public uint Characteristics { get { return _characteristics; } }

            /// <remarks>
            /// Reference: name
            /// </remarks>
            public string Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class C13Subsection : KaitaiStruct
        {
            public static C13Subsection FromFile(string fileName)
            {
                return new C13Subsection(new KaitaiStream(fileName));
            }

            public C13Subsection(KaitaiStream p__io, MsPdb.C13Lines p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = ((MsPdb.C13Lines.SubsectionType) m_io.ReadBitsIntLe(31));
                _isIgnored = m_io.ReadBitsIntLe(1) != 0;
                m_io.AlignToByte();
                _length = m_io.ReadU4le();
                if (IsIgnored) {
                    __unnamed3 = m_io.ReadBytes(Length);
                }
                if (IsIgnored == false) {
                    switch (Type) {
                    case MsPdb.C13Lines.SubsectionType.FileChkSms: {
                        __raw_data = m_io.ReadBytes(Length);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new C13SubsectionFilechecksums(io___raw_data, this, m_root);
                        break;
                    }
                    case MsPdb.C13Lines.SubsectionType.Ignore: {
                        __raw_data = m_io.ReadBytes(Length);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new C13SubsectionIgnore(io___raw_data, this, m_root);
                        break;
                    }
                    case MsPdb.C13Lines.SubsectionType.Lines: {
                        __raw_data = m_io.ReadBytes(Length);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new C13SubsectionLines(io___raw_data, this, m_root);
                        break;
                    }
                    case MsPdb.C13Lines.SubsectionType.FrameData: {
                        __raw_data = m_io.ReadBytes(Length);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new C13SubsectionFrameData(io___raw_data, this, m_root);
                        break;
                    }
                    case MsPdb.C13Lines.SubsectionType.IlLines: {
                        __raw_data = m_io.ReadBytes(Length);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new C13SubsectionLines(io___raw_data, this, m_root);
                        break;
                    }
                    case MsPdb.C13Lines.SubsectionType.InlineeLines: {
                        __raw_data = m_io.ReadBytes(Length);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new C13SubsectionInlineeLines(io___raw_data, this, m_root);
                        break;
                    }
                    case MsPdb.C13Lines.SubsectionType.StringTable: {
                        __raw_data = m_io.ReadBytes(Length);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new C13SubsectionStringtable(io___raw_data, this, m_root);
                        break;
                    }
                    default: {
                        __raw_data = m_io.ReadBytes(Length);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new C13SubsectionIgnore(io___raw_data, this, m_root);
                        break;
                    }
                    }
                }
            }
            private C13Lines.SubsectionType _type;
            private bool _isIgnored;
            private uint _length;
            private byte[] __unnamed3;
            private KaitaiStruct _data;
            private MsPdb m_root;
            private MsPdb.C13Lines m_parent;
            private byte[] __raw_data;

            /// <remarks>
            /// Reference: DEBUG_S_SUBSECTION_TYPE
            /// </remarks>
            public C13Lines.SubsectionType Type { get { return _type; } }

            /// <summary>
            /// if this bit is set in a subsection type then ignore the subsection contents
            /// </summary>
            /// <remarks>
            /// Reference: DEBUG_S_IGNORE
            /// </remarks>
            public bool IsIgnored { get { return _isIgnored; } }
            public uint Length { get { return _length; } }
            public byte[] Unnamed_3 { get { return __unnamed3; } }
            public KaitaiStruct Data { get { return _data; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13Lines M_Parent { get { return m_parent; } }
            public byte[] M_RawData { get { return __raw_data; } }
        }

        /// <summary>
        /// flag bitfields for separated code attributes
        /// </summary>
        /// <remarks>
        /// Reference: CV_SEPCODEFLAGS
        /// </remarks>
        public partial class CvSepcodeFlags : KaitaiStruct
        {
            public static CvSepcodeFlags FromFile(string fileName)
            {
                return new CvSepcodeFlags(new KaitaiStream(fileName));
            }

            public CvSepcodeFlags(KaitaiStream p__io, MsPdb.SymSepcode p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _isLexicalScope = m_io.ReadBitsIntLe(1) != 0;
                _returnsToParent = m_io.ReadBitsIntLe(1) != 0;
                _pad = m_io.ReadBitsIntLe(30);
            }
            private bool _isLexicalScope;
            private bool _returnsToParent;
            private ulong _pad;
            private MsPdb m_root;
            private MsPdb.SymSepcode m_parent;

            /// <summary>
            /// S_SEPCODE doubles as lexical scope
            /// </summary>
            /// <remarks>
            /// Reference: fIsLexicalScope
            /// </remarks>
            public bool IsLexicalScope { get { return _isLexicalScope; } }

            /// <summary>
            /// code frag returns to parent
            /// </summary>
            /// <remarks>
            /// Reference: fReturnsToParent
            /// </remarks>
            public bool ReturnsToParent { get { return _returnsToParent; } }

            /// <summary>
            /// must be zero
            /// </summary>
            /// <remarks>
            /// Reference: pad
            /// </remarks>
            public ulong Pad { get { return _pad; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.SymSepcode M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_ARRAY_16t
        /// </summary>
        /// <remarks>
        /// Reference: lfArray_16t
        /// </remarks>
        public partial class LfArray16t : KaitaiStruct
        {
            public static LfArray16t FromFile(string fileName)
            {
                return new LfArray16t(new KaitaiStream(fileName));
            }

            public LfArray16t(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _elementType = new TpiTypeRef16(m_io, this, m_root);
                _indexingType = new TpiTypeRef16(m_io, this, m_root);
                _size = new CvNumericType(m_io, this, m_root);
                _name = new PdbString(true, m_io, this, m_root);
            }
            private TpiTypeRef16 _elementType;
            private TpiTypeRef16 _indexingType;
            private CvNumericType _size;
            private PdbString _name;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// type index of element type
            /// </summary>
            /// <remarks>
            /// Reference: elemtype
            /// </remarks>
            public TpiTypeRef16 ElementType { get { return _elementType; } }

            /// <summary>
            /// type index of indexing type
            /// </summary>
            /// <remarks>
            /// Reference: idxtype
            /// </remarks>
            public TpiTypeRef16 IndexingType { get { return _indexingType; } }

            /// <summary>
            /// variable length data specifying size in bytes
            /// </summary>
            /// <remarks>
            /// Reference: data.size
            /// </remarks>
            public CvNumericType Size { get { return _size; } }

            /// <summary>
            /// array name
            /// </summary>
            /// <remarks>
            /// Reference: data.name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class PdbPage : KaitaiStruct
        {
            public static PdbPage FromFile(string fileName)
            {
                return new PdbPage(new KaitaiStream(fileName));
            }

            public PdbPage(KaitaiStream p__io, MsPdb.PdbPageNumber p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _data = m_io.ReadBytes(M_Root.PageSize);
            }
            private byte[] _data;
            private MsPdb m_root;
            private MsPdb.PdbPageNumber m_parent;
            public byte[] Data { get { return _data; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbPageNumber M_Parent { get { return m_parent; } }
        }
        public partial class PdbArray : KaitaiStruct
        {
            public PdbArray(uint p_elementSize, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _elementSize = p_elementSize;
                _read();
            }
            private void _read()
            {
                _numElements = m_io.ReadU4le();
                _data = m_io.ReadBytes((ElementSize * NumElements));
            }
            private uint _numElements;
            private byte[] _data;
            private uint _elementSize;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint NumElements { get { return _numElements; } }
            public byte[] Data { get { return _data; } }
            public uint ElementSize { get { return _elementSize; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class LfClass : KaitaiStruct
        {
            public LfClass(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _numberOfElements = m_io.ReadU2le();
                _properties = new CvProperties(m_io, this, m_root);
                _fieldType = new TpiTypeRef(m_io, this, m_root);
                _derivedType = new TpiTypeRef(m_io, this, m_root);
                _vshapeType = new TpiTypeRef(m_io, this, m_root);
                _structSize = new CvNumericType(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private ushort _numberOfElements;
            private CvProperties _properties;
            private TpiTypeRef _fieldType;
            private TpiTypeRef _derivedType;
            private TpiTypeRef _vshapeType;
            private CvNumericType _structSize;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// count of number of elements in class
            /// </summary>
            /// <remarks>
            /// Reference: count
            /// </remarks>
            public ushort NumberOfElements { get { return _numberOfElements; } }

            /// <summary>
            /// property attribute field (prop_t)
            /// </summary>
            /// <remarks>
            /// Reference: property
            /// </remarks>
            public CvProperties Properties { get { return _properties; } }

            /// <summary>
            /// type index of LF_FIELD descriptor list
            /// </summary>
            /// <remarks>
            /// Reference: field
            /// </remarks>
            public TpiTypeRef FieldType { get { return _fieldType; } }

            /// <summary>
            /// type index of derived from list if not zero
            /// </summary>
            /// <remarks>
            /// Reference: derived
            /// </remarks>
            public TpiTypeRef DerivedType { get { return _derivedType; } }

            /// <summary>
            /// type index of vshape table for this class
            /// </summary>
            /// <remarks>
            /// Reference: vshape
            /// </remarks>
            public TpiTypeRef VshapeType { get { return _vshapeType; } }

            /// <summary>
            /// data describing length of structure in bytes
            /// </summary>
            /// <remarks>
            /// Reference: data.size
            /// </remarks>
            public CvNumericType StructSize { get { return _structSize; } }

            /// <summary>
            /// class name
            /// </summary>
            /// <remarks>
            /// Reference: data.name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: REFSYM
        /// </remarks>
        public partial class SymReference : KaitaiStruct
        {
            public SymReference(bool p_stringPrefixed, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _sumName = m_io.ReadU4le();
                _symbolOffset = m_io.ReadU4le();
                _moduleIndex = m_io.ReadU2le();
                _fill = m_io.ReadU2le();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private uint _sumName;
            private uint _symbolOffset;
            private ushort _moduleIndex;
            private ushort _fill;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// SUC of the name
            /// </summary>
            /// <remarks>
            /// Reference: sumName
            /// </remarks>
            public uint SumName { get { return _sumName; } }

            /// <summary>
            /// Offset of actual symbol in $$Symbols
            /// </summary>
            /// <remarks>
            /// Reference: ibSym
            /// </remarks>
            public uint SymbolOffset { get { return _symbolOffset; } }

            /// <summary>
            /// Module containing the actual symbol
            /// </summary>
            /// <remarks>
            /// Reference: imod
            /// </remarks>
            public ushort ModuleIndex { get { return _moduleIndex; } }

            /// <summary>
            /// align this record
            /// </summary>
            /// <remarks>
            /// Reference: usFill
            /// </remarks>
            public ushort Fill { get { return _fill; } }

            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TiOffsetList : KaitaiStruct
        {
            public static TiOffsetList FromFile(string fileName)
            {
                return new TiOffsetList(new KaitaiStream(fileName));
            }

            public TiOffsetList(KaitaiStream p__io, MsPdb.TpiHashData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_itemsStart = false;
                f_itemsEnd = false;
                f_numItems = false;
                _read();
            }
            private void _read()
            {
                if (ItemsStart >= 0) {
                    _invokeItemsStart = m_io.ReadBytes(0);
                }
                _items = new List<TiOffset>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _items.Add(new TiOffset(((uint) (i)), m_io, this, m_root));
                        i++;
                    }
                }
                if (ItemsEnd >= 0) {
                    _invokeItemsEnd = m_io.ReadBytes(0);
                }
            }
            private bool f_itemsStart;
            private int _itemsStart;
            public int ItemsStart
            {
                get
                {
                    if (f_itemsStart)
                        return _itemsStart;
                    _itemsStart = (int) (M_Io.Pos);
                    f_itemsStart = true;
                    return _itemsStart;
                }
            }
            private bool f_itemsEnd;
            private int _itemsEnd;
            public int ItemsEnd
            {
                get
                {
                    if (f_itemsEnd)
                        return _itemsEnd;
                    _itemsEnd = (int) (M_Io.Pos);
                    f_itemsEnd = true;
                    return _itemsEnd;
                }
            }
            private bool f_numItems;
            private int _numItems;
            public int NumItems
            {
                get
                {
                    if (f_numItems)
                        return _numItems;
                    _numItems = (int) (((ItemsEnd - ItemsStart) / 8));
                    f_numItems = true;
                    return _numItems;
                }
            }
            private byte[] _invokeItemsStart;
            private List<TiOffset> _items;
            private byte[] _invokeItemsEnd;
            private MsPdb m_root;
            private MsPdb.TpiHashData m_parent;
            public byte[] InvokeItemsStart { get { return _invokeItemsStart; } }
            public List<TiOffset> Items { get { return _items; } }
            public byte[] InvokeItemsEnd { get { return _invokeItemsEnd; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiHashData M_Parent { get { return m_parent; } }
        }
        public partial class PdbSignature : KaitaiStruct
        {
            public static PdbSignature FromFile(string fileName)
            {
                return new PdbSignature(new KaitaiStream(fileName));
            }

            public PdbSignature(KaitaiStream p__io, MsPdb p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_versionMajorPos = false;
                f_versionMajor = false;
                f_idPos = false;
                _read();
            }
            private void _read()
            {
                _magic = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(26, false, true, true));
                if (IdPos >= 0) {
                    _invokeIdPos = m_io.ReadBytes(0);
                }
                _id = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytes(2));
            }
            private bool f_versionMajorPos;
            private int _versionMajorPos;
            public int VersionMajorPos
            {
                get
                {
                    if (f_versionMajorPos)
                        return _versionMajorPos;
                    _versionMajorPos = (int) ((IdPos - 7));
                    f_versionMajorPos = true;
                    return _versionMajorPos;
                }
            }
            private bool f_versionMajor;
            private string _versionMajor;
            public string VersionMajor
            {
                get
                {
                    if (f_versionMajor)
                        return _versionMajor;
                    _versionMajor = (string) (Magic.Substring(VersionMajorPos, (VersionMajorPos + 1) - VersionMajorPos));
                    f_versionMajor = true;
                    return _versionMajor;
                }
            }
            private bool f_idPos;
            private int _idPos;
            public int IdPos
            {
                get
                {
                    if (f_idPos)
                        return _idPos;
                    _idPos = (int) (M_Io.Pos);
                    f_idPos = true;
                    return _idPos;
                }
            }
            private string _magic;
            private byte[] _invokeIdPos;
            private string _id;
            private MsPdb m_root;
            private MsPdb m_parent;
            public string Magic { get { return _magic; } }
            public byte[] InvokeIdPos { get { return _invokeIdPos; } }
            public string Id { get { return _id; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb M_Parent { get { return m_parent; } }
        }
        public partial class TypeServerMap : KaitaiStruct
        {
            public static TypeServerMap FromFile(string fileName)
            {
                return new TypeServerMap(new KaitaiStream(fileName));
            }

            public TypeServerMap(KaitaiStream p__io, MsPdb.Dbi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_paddingSize = false;
                f_alignment = false;
                f_positionEnd = false;
                _read();
            }
            private void _read()
            {
                _reservedTypemapHandle = m_io.ReadU4le();
                _tiBase = m_io.ReadS4le();
                _signature = m_io.ReadU4le();
                _age = m_io.ReadU4le();
                _pdbPathName = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
                if (PositionEnd >= 0) {
                    _invokePositionEnd = m_io.ReadBytes(0);
                }
                _padding = m_io.ReadBytes(PaddingSize);
            }
            private bool f_paddingSize;
            private int _paddingSize;
            public int PaddingSize
            {
                get
                {
                    if (f_paddingSize)
                        return _paddingSize;
                    _paddingSize = (int) ((Alignment.Aligned - PositionEnd));
                    f_paddingSize = true;
                    return _paddingSize;
                }
            }
            private bool f_alignment;
            private Align _alignment;
            public Align Alignment
            {
                get
                {
                    if (f_alignment)
                        return _alignment;
                    _alignment = new Align(((uint) (PositionEnd)), 4, m_io, this, m_root);
                    f_alignment = true;
                    return _alignment;
                }
            }
            private bool f_positionEnd;
            private int _positionEnd;
            public int PositionEnd
            {
                get
                {
                    if (f_positionEnd)
                        return _positionEnd;
                    _positionEnd = (int) (M_Io.Pos);
                    f_positionEnd = true;
                    return _positionEnd;
                }
            }
            private uint _reservedTypemapHandle;
            private int _tiBase;
            private uint _signature;
            private uint _age;
            private string _pdbPathName;
            private byte[] _invokePositionEnd;
            private byte[] _padding;
            private MsPdb m_root;
            private MsPdb.Dbi m_parent;
            public uint ReservedTypemapHandle { get { return _reservedTypemapHandle; } }
            public int TiBase { get { return _tiBase; } }
            public uint Signature { get { return _signature; } }
            public uint Age { get { return _age; } }
            public string PdbPathName { get { return _pdbPathName; } }
            public byte[] InvokePositionEnd { get { return _invokePositionEnd; } }
            public byte[] Padding { get { return _padding; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Dbi M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: lfVftable
        /// </remarks>
        public partial class LfVftable : KaitaiStruct
        {
            public static LfVftable FromFile(string fileName)
            {
                return new LfVftable(new KaitaiStream(fileName));
            }

            public LfVftable(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_names = false;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _baseVftable = new TpiTypeRef(m_io, this, m_root);
                _offsetInObjectLayout = m_io.ReadU4le();
                _len = m_io.ReadU4le();
                __raw_zzzNamesBlock = m_io.ReadBytes(Len);
                var io___raw_zzzNamesBlock = new KaitaiStream(__raw_zzzNamesBlock);
                _zzzNamesBlock = new LfVftableNames(io___raw_zzzNamesBlock, this, m_root);
            }
            private bool f_names;
            private List<string> _names;
            public List<string> Names
            {
                get
                {
                    if (f_names)
                        return _names;
                    _names = (List<string>) (ZzzNamesBlock.Names);
                    f_names = true;
                    return _names;
                }
            }
            private TpiTypeRef _type;
            private TpiTypeRef _baseVftable;
            private uint _offsetInObjectLayout;
            private uint _len;
            private LfVftableNames _zzzNamesBlock;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;
            private byte[] __raw_zzzNamesBlock;

            /// <summary>
            /// class/structure that owns the vftable
            /// </summary>
            /// <remarks>
            /// Reference: type
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// vftable from which this vftable is derived
            /// </summary>
            /// <remarks>
            /// Reference: baseVftable
            /// </remarks>
            public TpiTypeRef BaseVftable { get { return _baseVftable; } }

            /// <summary>
            /// offset of the vfptr to this table, relative to the start of the object layout.
            /// </summary>
            /// <remarks>
            /// Reference: offsetInObjectLayout
            /// </remarks>
            public uint OffsetInObjectLayout { get { return _offsetInObjectLayout; } }

            /// <summary>
            /// length of the Names array below in bytes.
            /// </summary>
            /// <remarks>
            /// Reference: len
            /// </remarks>
            public uint Len { get { return _len; } }

            /// <summary>
            /// array of names. The first is the name of the vtable. The others are the names of the methods.
            /// </summary>
            /// <remarks>
            /// Reference: Names
            /// </remarks>
            public LfVftableNames ZzzNamesBlock { get { return _zzzNamesBlock; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
            public byte[] M_RawZzzNamesBlock { get { return __raw_zzzNamesBlock; } }
        }

        /// <remarks>
        /// Reference: DEFRANGESYMSUBFIELDREGISTER
        /// </remarks>
        public partial class SymDefrangeSubfieldRegister : KaitaiStruct
        {
            public static SymDefrangeSubfieldRegister FromFile(string fileName)
            {
                return new SymDefrangeSubfieldRegister(new KaitaiStream(fileName));
            }

            public SymDefrangeSubfieldRegister(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _register = m_io.ReadU2le();
                _attr = new CvRangeAttr(m_io, this, m_root);
                _parentOffset = m_io.ReadBitsIntLe(12);
                __unnamed3 = m_io.ReadBitsIntLe(20);
                m_io.AlignToByte();
                _range = new CvLvarAddrRange(m_io, this, m_root);
                _gaps = new List<CvLvarAddrGap>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _gaps.Add(new CvLvarAddrGap(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private ushort _register;
            private CvRangeAttr _attr;
            private ulong _parentOffset;
            private ulong __unnamed3;
            private CvLvarAddrRange _range;
            private List<CvLvarAddrGap> _gaps;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Register to hold the value of the symbol
            /// </summary>
            /// <remarks>
            /// Reference: reg
            /// </remarks>
            public ushort Register { get { return _register; } }

            /// <summary>
            /// Attribute of the register range.
            /// </summary>
            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public CvRangeAttr Attr { get { return _attr; } }

            /// <summary>
            /// Offset in parent variable.
            /// </summary>
            /// <remarks>
            /// Reference: offParent
            /// </remarks>
            public ulong ParentOffset { get { return _parentOffset; } }

            /// <summary>
            /// Padding for future use.
            /// </summary>
            /// <remarks>
            /// Reference: padding
            /// </remarks>
            public ulong Unnamed_3 { get { return __unnamed3; } }

            /// <summary>
            /// Range of addresses where this program is valid
            /// </summary>
            /// <remarks>
            /// Reference: range
            /// </remarks>
            public CvLvarAddrRange Range { get { return _range; } }

            /// <summary>
            /// The value is not available in following gaps. 
            /// </summary>
            /// <remarks>
            /// Reference: gaps
            /// </remarks>
            public List<CvLvarAddrGap> Gaps { get { return _gaps; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: HDR_16t
        /// </remarks>
        public partial class TpiHeader16 : KaitaiStruct
        {
            public static TpiHeader16 FromFile(string fileName)
            {
                return new TpiHeader16(new KaitaiStream(fileName));
            }

            public TpiHeader16(KaitaiStream p__io, MsPdb.Tpi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _version = ((MsPdb.Tpi.TpiVersion) m_io.ReadU4le());
                _minTypeIndex = m_io.ReadU2le();
                _maxTypeIndex = m_io.ReadU2le();
                _gpRecSize = m_io.ReadU4le();
                _hashStream = new PdbStreamRef(m_io, this, m_root);
            }
            private Tpi.TpiVersion _version;
            private ushort _minTypeIndex;
            private ushort _maxTypeIndex;
            private uint _gpRecSize;
            private PdbStreamRef _hashStream;
            private MsPdb m_root;
            private MsPdb.Tpi m_parent;

            /// <summary>
            /// version which created this TypeServer
            /// </summary>
            /// <remarks>
            /// Reference: vers
            /// </remarks>
            public Tpi.TpiVersion Version { get { return _version; } }

            /// <summary>
            /// lowest TI
            /// </summary>
            /// <remarks>
            /// Reference: tiMin
            /// </remarks>
            public ushort MinTypeIndex { get { return _minTypeIndex; } }

            /// <summary>
            /// highest TI + 1
            /// </summary>
            /// <remarks>
            /// Reference: tiMac
            /// </remarks>
            public ushort MaxTypeIndex { get { return _maxTypeIndex; } }

            /// <summary>
            /// count of bytes used by the gprec which follows.
            /// </summary>
            /// <remarks>
            /// Reference: cbGprec
            /// </remarks>
            public uint GpRecSize { get { return _gpRecSize; } }

            /// <summary>
            /// stream to hold hash values
            /// </summary>
            /// <remarks>
            /// Reference: snHash
            /// </remarks>
            public PdbStreamRef HashStream { get { return _hashStream; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Tpi M_Parent { get { return m_parent; } }
        }
        public partial class PdbPageNumberList : KaitaiStruct
        {
            public PdbPageNumberList(uint p_numPages, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _numPages = p_numPages;
                _read();
            }
            private void _read()
            {
                _pages = new List<PdbPageNumber>();
                for (var i = 0; i < NumPages; i++)
                {
                    _pages.Add(new PdbPageNumber(m_io, this, m_root));
                }
            }
            private List<PdbPageNumber> _pages;
            private uint _numPages;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public List<PdbPageNumber> Pages { get { return _pages; } }
            public uint NumPages { get { return _numPages; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class ModuleList : KaitaiStruct
        {
            public static ModuleList FromFile(string fileName)
            {
                return new ModuleList(new KaitaiStream(fileName));
            }

            public ModuleList(KaitaiStream p__io, MsPdb.Dbi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _items = new List<UModuleInfo>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _items.Add(new UModuleInfo(((uint) (i)), m_io, this, m_root));
                        i++;
                    }
                }
                __unnamed1 = m_io.ReadBytesFull();
            }
            private List<UModuleInfo> _items;
            private byte[] __unnamed1;
            private MsPdb m_root;
            private MsPdb.Dbi m_parent;
            public List<UModuleInfo> Items { get { return _items; } }
            public byte[] Unnamed_1 { get { return __unnamed1; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Dbi M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: rest of SectInfo
        /// </remarks>
        public partial class C11SectionInfo2 : KaitaiStruct
        {
            public static C11SectionInfo2 FromFile(string fileName)
            {
                return new C11SectionInfo2(new KaitaiStream(fileName));
            }

            public C11SectionInfo2(KaitaiStream p__io, MsPdb.C11Srcfile p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_lines = false;
                _read();
            }
            private void _read()
            {
                _sectionIndex = m_io.ReadU2le();
                _numPairs = m_io.ReadU2le();
                _lineOffsets = new List<uint>();
                for (var i = 0; i < NumPairs; i++)
                {
                    _lineOffsets.Add(m_io.ReadU4le());
                }
                _lineNumbers = new List<ushort>();
                for (var i = 0; i < NumPairs; i++)
                {
                    _lineNumbers.Add(m_io.ReadU2le());
                }
                if (KaitaiStream.Mod(NumPairs, 2) != 0) {
                    _pad = m_io.ReadU2le();
                }
            }
            private bool f_lines;
            private List<C11LineInfo> _lines;
            public List<C11LineInfo> Lines
            {
                get
                {
                    if (f_lines)
                        return _lines;
                    _lines = new List<C11LineInfo>();
                    for (var i = 0; i < NumPairs; i++)
                    {
                        _lines.Add(new C11LineInfo(LineNumbers[i], LineOffsets[i], m_io, this, m_root));
                    }
                    f_lines = true;
                    return _lines;
                }
            }
            private ushort _sectionIndex;
            private ushort _numPairs;
            private List<uint> _lineOffsets;
            private List<ushort> _lineNumbers;
            private ushort? _pad;
            private MsPdb m_root;
            private MsPdb.C11Srcfile m_parent;

            /// <remarks>
            /// Reference: isect
            /// </remarks>
            public ushort SectionIndex { get { return _sectionIndex; } }

            /// <remarks>
            /// Reference: cPair
            /// </remarks>
            public ushort NumPairs { get { return _numPairs; } }
            public List<uint> LineOffsets { get { return _lineOffsets; } }
            public List<ushort> LineNumbers { get { return _lineNumbers; } }
            public ushort? Pad { get { return _pad; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C11Srcfile M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_LABEL
        /// </summary>
        /// <remarks>
        /// Reference: lfLabel
        /// </remarks>
        public partial class LfLabel : KaitaiStruct
        {
            public static LfLabel FromFile(string fileName)
            {
                return new LfLabel(new KaitaiStream(fileName));
            }

            public LfLabel(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _mode = m_io.ReadU2le();
            }
            private ushort _mode;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// addressing mode of label
            /// </summary>
            public ushort Mode { get { return _mode; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class PdbString : KaitaiStruct
        {
            public PdbString(bool p_isPrefixed, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _isPrefixed = p_isPrefixed;
                f_value = false;
                _read();
            }
            private void _read()
            {
                if (IsPrefixed) {
                    _nameLength = m_io.ReadU1();
                }
                if (IsPrefixed) {
                    _namePrefixed = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytes(((int) (NameLength))));
                }
                if (!(IsPrefixed)) {
                    _nameCstring = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
                }
            }
            private bool f_value;
            private string _value;
            public string Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    _value = (string) ((IsPrefixed ? NamePrefixed : NameCstring));
                    f_value = true;
                    return _value;
                }
            }
            private byte? _nameLength;
            private string _namePrefixed;
            private string _nameCstring;
            private bool _isPrefixed;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public byte? NameLength { get { return _nameLength; } }
            public string NamePrefixed { get { return _namePrefixed; } }
            public string NameCstring { get { return _nameCstring; } }
            public bool IsPrefixed { get { return _isPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: FILESTATICSYM
        /// </remarks>
        public partial class SymFileStatic : KaitaiStruct
        {
            public static SymFileStatic FromFile(string fileName)
            {
                return new SymFileStatic(new KaitaiStream(fileName));
            }

            public SymFileStatic(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _modOffset = m_io.ReadU4le();
                _flags = new CvLocalVarFlags(m_io, this, m_root);
                _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
            }
            private TpiTypeRef _type;
            private uint _modOffset;
            private CvLocalVarFlags _flags;
            private string _name;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// type index
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// index of mod filename in stringtable
            /// </summary>
            /// <remarks>
            /// Reference: modOffset
            /// </remarks>
            public uint ModOffset { get { return _modOffset; } }

            /// <summary>
            /// local var flags
            /// </summary>
            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public CvLocalVarFlags Flags { get { return _flags; } }

            /// <summary>
            /// Name of this symbol, a null terminated array of UTF8 characters
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public string Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class PdbHeaderDs : KaitaiStruct
        {
            public static PdbHeaderDs FromFile(string fileName)
            {
                return new PdbHeaderDs(new KaitaiStream(fileName));
            }

            public PdbHeaderDs(KaitaiStream p__io, MsPdb.PdbDsRoot p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                __unnamed0 = m_io.ReadBytes(3);
                _pageSize = m_io.ReadU4le();
                _fpmPageNumber = m_io.ReadU4le();
                _numPages = m_io.ReadU4le();
                _directorySize = m_io.ReadU4le();
                _pageMap = m_io.ReadU4le();
            }
            private byte[] __unnamed0;
            private uint _pageSize;
            private uint _fpmPageNumber;
            private uint _numPages;
            private uint _directorySize;
            private uint _pageMap;
            private MsPdb m_root;
            private MsPdb.PdbDsRoot m_parent;
            public byte[] Unnamed_0 { get { return __unnamed0; } }
            public uint PageSize { get { return _pageSize; } }
            public uint FpmPageNumber { get { return _fpmPageNumber; } }
            public uint NumPages { get { return _numPages; } }
            public uint DirectorySize { get { return _directorySize; } }
            public uint PageMap { get { return _pageMap; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbDsRoot M_Parent { get { return m_parent; } }
        }
        public partial class PdbStreamData : KaitaiStruct
        {
            public PdbStreamData(int p_streamSize, KaitaiStream p__io, MsPdb.PdbStreamPagelist p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _streamSize = p_streamSize;
                _read();
            }
            private void _read()
            {
                if (StreamSize > 0) {
                    _data = m_io.ReadBytes(StreamSize);
                }
            }
            private byte[] _data;
            private int _streamSize;
            private MsPdb m_root;
            private MsPdb.PdbStreamPagelist m_parent;
            public byte[] Data { get { return _data; } }
            public int StreamSize { get { return _streamSize; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbStreamPagelist M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_ENUMERATE
        /// </summary>
        /// <remarks>
        /// Reference: lfEnumerate
        /// </remarks>
        public partial class LfEnumerate : KaitaiStruct
        {
            public static LfEnumerate FromFile(string fileName)
            {
                return new LfEnumerate(new KaitaiStream(fileName));
            }

            public LfEnumerate(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _attributes = new CvFieldAttributes(m_io, this, m_root);
                _value = new CvNumericType(m_io, this, m_root);
                _fieldName = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
            }
            private CvFieldAttributes _attributes;
            private CvNumericType _value;
            private string _fieldName;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// access
            /// </summary>
            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public CvFieldAttributes Attributes { get { return _attributes; } }

            /// <summary>
            /// variable length value field
            /// </summary>
            /// <remarks>
            /// Reference: value
            /// </remarks>
            public CvNumericType Value { get { return _value; } }

            /// <summary>
            /// length prefixed name
            /// </summary>
            public string FieldName { get { return _fieldName; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: _flags
        /// </remarks>
        public partial class DbiHeaderFlags : KaitaiStruct
        {
            public static DbiHeaderFlags FromFile(string fileName)
            {
                return new DbiHeaderFlags(new KaitaiStream(fileName));
            }

            public DbiHeaderFlags(KaitaiStream p__io, MsPdb.DbiHeaderNew p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _linkedIncrementally = m_io.ReadBitsIntLe(1) != 0;
                _stripped = m_io.ReadBitsIntLe(1) != 0;
                _ctypes = m_io.ReadBitsIntLe(1) != 0;
                _unused = m_io.ReadBitsIntLe(13);
            }
            private bool _linkedIncrementally;
            private bool _stripped;
            private bool _ctypes;
            private ulong _unused;
            private MsPdb m_root;
            private MsPdb.DbiHeaderNew m_parent;

            /// <summary>
            /// true if linked incrmentally (really just if ilink thunks are present)
            /// </summary>
            /// <remarks>
            /// Reference: fIncLink
            /// </remarks>
            public bool LinkedIncrementally { get { return _linkedIncrementally; } }

            /// <summary>
            /// true if PDB::CopyTo stripped the private data out
            /// </summary>
            /// <remarks>
            /// Reference: fStripped
            /// </remarks>
            public bool Stripped { get { return _stripped; } }

            /// <summary>
            /// true if this PDB is using CTypes.
            /// </summary>
            /// <remarks>
            /// Reference: fCTypes
            /// </remarks>
            public bool Ctypes { get { return _ctypes; } }

            /// <summary>
            /// reserved, must be 0.
            /// </summary>
            /// <remarks>
            /// Reference: unused
            /// </remarks>
            public ulong Unused { get { return _unused; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiHeaderNew M_Parent { get { return m_parent; } }
        }
        public partial class GetModuleIo : KaitaiStruct
        {
            public GetModuleIo(uint p_moduleIndex, KaitaiStream p__io, MsPdb.DbiSymbolRef p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _moduleIndex = p_moduleIndex;
                f_value = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_value;
            private KaitaiStream _value;
            public KaitaiStream Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    _value = (KaitaiStream) (M_Root.DbiStream.ModulesList.Items[((int) (ModuleIndex))].ModuleData.SymbolsList.M_Io);
                    f_value = true;
                    return _value;
                }
            }
            private uint _moduleIndex;
            private MsPdb m_root;
            private MsPdb.DbiSymbolRef m_parent;
            public uint ModuleIndex { get { return _moduleIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolRef M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: REGREL32
        /// </remarks>
        public partial class SymRegrel32 : KaitaiStruct
        {
            public SymRegrel32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _type = new TpiTypeRef(m_io, this, m_root);
                _register = m_io.ReadU2le();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private uint _offset;
            private TpiTypeRef _type;
            private ushort _register;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// offset of symbol
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// Type index or metadata token
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// register index for symbol
            /// </summary>
            /// <remarks>
            /// Reference: reg
            /// </remarks>
            public ushort Register { get { return _register; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_MODIFIER
        /// </summary>
        /// <remarks>
        /// Reference: lfModifier
        /// </remarks>
        public partial class LfModifier : KaitaiStruct
        {
            public static LfModifier FromFile(string fileName)
            {
                return new LfModifier(new KaitaiStream(fileName));
            }

            public LfModifier(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _modifiedType = new TpiTypeRef(m_io, this, m_root);
                _flags = new LfModifierFlags(m_io, this, m_root);
            }
            private TpiTypeRef _modifiedType;
            private LfModifierFlags _flags;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// modified type
            /// </summary>
            /// <remarks>
            /// Reference: type
            /// </remarks>
            public TpiTypeRef ModifiedType { get { return _modifiedType; } }

            /// <summary>
            /// modifier attribute modifier_t
            /// </summary>
            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public LfModifierFlags Flags { get { return _flags; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class GlobalSymbolsStream : KaitaiStruct
        {
            public static GlobalSymbolsStream FromFile(string fileName)
            {
                return new GlobalSymbolsStream(new KaitaiStream(fileName));
            }

            public GlobalSymbolsStream(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_hasCompressedBuckets = false;
                f_header = false;
                _read();
            }
            private void _read()
            {
                if (HasCompressedBuckets) {
                    __unnamed0 = new GsiHdr(m_io, this, m_root);
                }
                if (HasCompressedBuckets) {
                    _compressedHashRecords = new List<GsiHashRecord>();
                    for (var i = 0; i < Header.NumHashRecords; i++)
                    {
                        _compressedHashRecords.Add(new GsiHashRecord(m_io, this, m_root));
                    }
                }
                if (HasCompressedBuckets) {
                    _hashBuckets = m_io.ReadBytes(Header.SizeHashBuckets);
                }
            }
            private bool f_hasCompressedBuckets;
            private bool _hasCompressedBuckets;
            public bool HasCompressedBuckets
            {
                get
                {
                    if (f_hasCompressedBuckets)
                        return _hasCompressedBuckets;
                    _hasCompressedBuckets = (bool) ( ((Header.Signature == 4294967295) && (Header.Version == MsPdb.GsiHdr.VersionEnum.V70)) );
                    f_hasCompressedBuckets = true;
                    return _hasCompressedBuckets;
                }
            }
            private bool f_header;
            private GsiHdr _header;
            public GsiHdr Header
            {
                get
                {
                    if (f_header)
                        return _header;
                    long _pos = m_io.Pos;
                    m_io.Seek(0);
                    _header = new GsiHdr(m_io, this, m_root);
                    m_io.Seek(_pos);
                    f_header = true;
                    return _header;
                }
            }
            private GsiHdr __unnamed0;
            private List<GsiHashRecord> _compressedHashRecords;
            private byte[] _hashBuckets;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public GsiHdr Unnamed_0 { get { return __unnamed0; } }
            public List<GsiHashRecord> CompressedHashRecords { get { return _compressedHashRecords; } }
            public byte[] HashBuckets { get { return _hashBuckets; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class PdbJgOldRoot : KaitaiStruct
        {
            public static PdbJgOldRoot FromFile(string fileName)
            {
                return new PdbJgOldRoot(new KaitaiStream(fileName));
            }

            public PdbJgOldRoot(KaitaiStream p__io, MsPdb p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _header = new PdbHeaderJgOld(m_io, this, m_root);
                _types = new List<TpiType>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _types.Add(new TpiType(((uint) ((Header.MinTi + i))), m_io, this, m_root));
                        i++;
                    }
                }
            }
            private PdbHeaderJgOld _header;
            private List<TpiType> _types;
            private MsPdb m_root;
            private MsPdb m_parent;
            public PdbHeaderJgOld Header { get { return _header; } }

            /// <remarks>
            /// Reference: OHDR.gprec
            /// </remarks>
            public List<TpiType> Types { get { return _types; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CV_RANGEATTR
        /// </remarks>
        public partial class CvRangeAttr : KaitaiStruct
        {
            public static CvRangeAttr FromFile(string fileName)
            {
                return new CvRangeAttr(new KaitaiStream(fileName));
            }

            public CvRangeAttr(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _maybe = m_io.ReadBitsIntLe(1) != 0;
                _padding = m_io.ReadBitsIntLe(15);
            }
            private bool _maybe;
            private ulong _padding;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// May have no user name on one of control flow path.
            /// </summary>
            /// <remarks>
            /// Reference: maybe
            /// </remarks>
            public bool Maybe { get { return _maybe; } }

            /// <summary>
            /// Padding for future use.
            /// </summary>
            /// <remarks>
            /// Reference: padding
            /// </remarks>
            public ulong Padding { get { return _padding; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: MODI.flags
        /// </remarks>
        public partial class ModuleInfoFlags : KaitaiStruct
        {
            public static ModuleInfoFlags FromFile(string fileName)
            {
                return new ModuleInfoFlags(new KaitaiStream(fileName));
            }

            public ModuleInfoFlags(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _written = m_io.ReadBitsIntLe(1) != 0;
                _ecEnabled = m_io.ReadBitsIntLe(1) != 0;
                _unused = m_io.ReadBitsIntLe(6);
                _tsmListIndex = m_io.ReadBitsIntLe(8);
            }
            private bool _written;
            private bool _ecEnabled;
            private ulong _unused;
            private ulong _tsmListIndex;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// TRUE if mod has been written since DBI opened
            /// </summary>
            /// <remarks>
            /// Reference: fWritten
            /// </remarks>
            public bool Written { get { return _written; } }

            /// <summary>
            /// TRUE if mod has EC symbolic information
            /// </summary>
            /// <remarks>
            /// Reference: fECEnabled
            /// </remarks>
            public bool EcEnabled { get { return _ecEnabled; } }

            /// <summary>
            /// spare
            /// </summary>
            /// <remarks>
            /// Reference: unused
            /// </remarks>
            public ulong Unused { get { return _unused; } }

            /// <summary>
            /// index into TSM list for this mods server
            /// </summary>
            /// <remarks>
            /// Reference: iTSM
            /// </remarks>
            public ulong TsmListIndex { get { return _tsmListIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: HRFile
        /// </remarks>
        public partial class GsiHashRecord : KaitaiStruct
        {
            public static GsiHashRecord FromFile(string fileName)
            {
                return new GsiHashRecord(new KaitaiStream(fileName));
            }

            public GsiHashRecord(KaitaiStream p__io, MsPdb.GlobalSymbolsStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _referenceCount = m_io.ReadU4le();
            }
            private uint _offset;
            private uint _referenceCount;
            private MsPdb m_root;
            private MsPdb.GlobalSymbolsStream m_parent;

            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <remarks>
            /// Reference: cRef
            /// </remarks>
            public uint ReferenceCount { get { return _referenceCount; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.GlobalSymbolsStream M_Parent { get { return m_parent; } }
        }
        public partial class PdbStreamHdrVc70 : KaitaiStruct
        {
            public static PdbStreamHdrVc70 FromFile(string fileName)
            {
                return new PdbStreamHdrVc70(new KaitaiStream(fileName));
            }

            public PdbStreamHdrVc70(KaitaiStream p__io, MsPdb.PdbStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _sig70 = new Guid(m_io, this, m_root);
            }
            private Guid _sig70;
            private MsPdb m_root;
            private MsPdb.PdbStream m_parent;
            public Guid Sig70 { get { return _sig70; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbStream M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DBGTYPE
        /// </remarks>
        public partial class DebugData : KaitaiStruct
        {
            public static DebugData FromFile(string fileName)
            {
                return new DebugData(new KaitaiStream(fileName));
            }

            public DebugData(KaitaiStream p__io, MsPdb.Dbi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_haveExceptionStream = false;
                f_fpoStreamData = false;
                f_haveOmapFromSrcStream = false;
                f_haveSectionHdrOrigStream = false;
                f_haveFpoStream = false;
                f_haveNewFpoStream = false;
                f_haveFixupStream = false;
                f_haveXdataStream = false;
                f_haveTokenRidMapStream = false;
                f_havePdataStream = false;
                f_haveOmapToSrcStream = false;
                f_haveSectionHdrStream = false;
                f_sectionHdrStreamData = false;
                _read();
            }
            private void _read()
            {
                if (HaveFpoStream) {
                    _fpoStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HaveExceptionStream) {
                    _exceptionStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HaveFixupStream) {
                    _fixupStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HaveOmapToSrcStream) {
                    _omapToSrcStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HaveOmapFromSrcStream) {
                    _omapFromSrcStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HaveSectionHdrStream) {
                    _sectionHdrStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HaveTokenRidMapStream) {
                    _tokenRidMapStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HaveXdataStream) {
                    _xdataStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HavePdataStream) {
                    _pdataStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HaveNewFpoStream) {
                    _newFpoStream = new PdbStreamRef(m_io, this, m_root);
                }
                if (HaveSectionHdrOrigStream) {
                    _sectionHdrOrigStream = new PdbStreamRef(m_io, this, m_root);
                }
            }
            private bool f_haveExceptionStream;
            private bool _haveExceptionStream;
            public bool HaveExceptionStream
            {
                get
                {
                    if (f_haveExceptionStream)
                        return _haveExceptionStream;
                    _haveExceptionStream = (bool) (M_Io.Size >= (2 * 2));
                    f_haveExceptionStream = true;
                    return _haveExceptionStream;
                }
            }
            private bool f_fpoStreamData;
            private FpoStream _fpoStreamData;
            public FpoStream FpoStreamData
            {
                get
                {
                    if (f_fpoStreamData)
                        return _fpoStreamData;
                    if (FpoStream.StreamNumber > -1) {
                        __raw__raw_fpoStreamData = m_io.ReadBytes(0);
                        Cat _process__raw__raw_fpoStreamData = new Cat(FpoStream.Data);
                        __raw_fpoStreamData = _process__raw__raw_fpoStreamData.Decode(__raw__raw_fpoStreamData);
                        var io___raw_fpoStreamData = new KaitaiStream(__raw_fpoStreamData);
                        _fpoStreamData = new FpoStream(io___raw_fpoStreamData, this, m_root);
                        f_fpoStreamData = true;
                    }
                    return _fpoStreamData;
                }
            }
            private bool f_haveOmapFromSrcStream;
            private bool _haveOmapFromSrcStream;
            public bool HaveOmapFromSrcStream
            {
                get
                {
                    if (f_haveOmapFromSrcStream)
                        return _haveOmapFromSrcStream;
                    _haveOmapFromSrcStream = (bool) (M_Io.Size >= (2 * 5));
                    f_haveOmapFromSrcStream = true;
                    return _haveOmapFromSrcStream;
                }
            }
            private bool f_haveSectionHdrOrigStream;
            private bool _haveSectionHdrOrigStream;
            public bool HaveSectionHdrOrigStream
            {
                get
                {
                    if (f_haveSectionHdrOrigStream)
                        return _haveSectionHdrOrigStream;
                    _haveSectionHdrOrigStream = (bool) (M_Io.Size >= (2 * 11));
                    f_haveSectionHdrOrigStream = true;
                    return _haveSectionHdrOrigStream;
                }
            }
            private bool f_haveFpoStream;
            private bool _haveFpoStream;
            public bool HaveFpoStream
            {
                get
                {
                    if (f_haveFpoStream)
                        return _haveFpoStream;
                    _haveFpoStream = (bool) (M_Io.Size >= (2 * 1));
                    f_haveFpoStream = true;
                    return _haveFpoStream;
                }
            }
            private bool f_haveNewFpoStream;
            private bool _haveNewFpoStream;
            public bool HaveNewFpoStream
            {
                get
                {
                    if (f_haveNewFpoStream)
                        return _haveNewFpoStream;
                    _haveNewFpoStream = (bool) (M_Io.Size >= (2 * 10));
                    f_haveNewFpoStream = true;
                    return _haveNewFpoStream;
                }
            }
            private bool f_haveFixupStream;
            private bool _haveFixupStream;
            public bool HaveFixupStream
            {
                get
                {
                    if (f_haveFixupStream)
                        return _haveFixupStream;
                    _haveFixupStream = (bool) (M_Io.Size >= (2 * 3));
                    f_haveFixupStream = true;
                    return _haveFixupStream;
                }
            }
            private bool f_haveXdataStream;
            private bool _haveXdataStream;
            public bool HaveXdataStream
            {
                get
                {
                    if (f_haveXdataStream)
                        return _haveXdataStream;
                    _haveXdataStream = (bool) (M_Io.Size >= (2 * 8));
                    f_haveXdataStream = true;
                    return _haveXdataStream;
                }
            }
            private bool f_haveTokenRidMapStream;
            private bool _haveTokenRidMapStream;
            public bool HaveTokenRidMapStream
            {
                get
                {
                    if (f_haveTokenRidMapStream)
                        return _haveTokenRidMapStream;
                    _haveTokenRidMapStream = (bool) (M_Io.Size >= (2 * 7));
                    f_haveTokenRidMapStream = true;
                    return _haveTokenRidMapStream;
                }
            }
            private bool f_havePdataStream;
            private bool _havePdataStream;
            public bool HavePdataStream
            {
                get
                {
                    if (f_havePdataStream)
                        return _havePdataStream;
                    _havePdataStream = (bool) (M_Io.Size >= (2 * 9));
                    f_havePdataStream = true;
                    return _havePdataStream;
                }
            }
            private bool f_haveOmapToSrcStream;
            private bool _haveOmapToSrcStream;
            public bool HaveOmapToSrcStream
            {
                get
                {
                    if (f_haveOmapToSrcStream)
                        return _haveOmapToSrcStream;
                    _haveOmapToSrcStream = (bool) (M_Io.Size >= (2 * 4));
                    f_haveOmapToSrcStream = true;
                    return _haveOmapToSrcStream;
                }
            }
            private bool f_haveSectionHdrStream;
            private bool _haveSectionHdrStream;
            public bool HaveSectionHdrStream
            {
                get
                {
                    if (f_haveSectionHdrStream)
                        return _haveSectionHdrStream;
                    _haveSectionHdrStream = (bool) (M_Io.Size >= (2 * 6));
                    f_haveSectionHdrStream = true;
                    return _haveSectionHdrStream;
                }
            }
            private bool f_sectionHdrStreamData;
            private DebugSectionHdrStream _sectionHdrStreamData;
            public DebugSectionHdrStream SectionHdrStreamData
            {
                get
                {
                    if (f_sectionHdrStreamData)
                        return _sectionHdrStreamData;
                    if (SectionHdrStream.StreamNumber > -1) {
                        __raw__raw_sectionHdrStreamData = m_io.ReadBytes(0);
                        Cat _process__raw__raw_sectionHdrStreamData = new Cat(SectionHdrStream.Data);
                        __raw_sectionHdrStreamData = _process__raw__raw_sectionHdrStreamData.Decode(__raw__raw_sectionHdrStreamData);
                        var io___raw_sectionHdrStreamData = new KaitaiStream(__raw_sectionHdrStreamData);
                        _sectionHdrStreamData = new DebugSectionHdrStream(io___raw_sectionHdrStreamData, this, m_root);
                        f_sectionHdrStreamData = true;
                    }
                    return _sectionHdrStreamData;
                }
            }
            private PdbStreamRef _fpoStream;
            private PdbStreamRef _exceptionStream;
            private PdbStreamRef _fixupStream;
            private PdbStreamRef _omapToSrcStream;
            private PdbStreamRef _omapFromSrcStream;
            private PdbStreamRef _sectionHdrStream;
            private PdbStreamRef _tokenRidMapStream;
            private PdbStreamRef _xdataStream;
            private PdbStreamRef _pdataStream;
            private PdbStreamRef _newFpoStream;
            private PdbStreamRef _sectionHdrOrigStream;
            private MsPdb m_root;
            private MsPdb.Dbi m_parent;
            private byte[] __raw_fpoStreamData;
            private byte[] __raw__raw_fpoStreamData;
            private byte[] __raw_sectionHdrStreamData;
            private byte[] __raw__raw_sectionHdrStreamData;
            public PdbStreamRef FpoStream { get { return _fpoStream; } }
            public PdbStreamRef ExceptionStream { get { return _exceptionStream; } }
            public PdbStreamRef FixupStream { get { return _fixupStream; } }
            public PdbStreamRef OmapToSrcStream { get { return _omapToSrcStream; } }
            public PdbStreamRef OmapFromSrcStream { get { return _omapFromSrcStream; } }
            public PdbStreamRef SectionHdrStream { get { return _sectionHdrStream; } }
            public PdbStreamRef TokenRidMapStream { get { return _tokenRidMapStream; } }
            public PdbStreamRef XdataStream { get { return _xdataStream; } }
            public PdbStreamRef PdataStream { get { return _pdataStream; } }
            public PdbStreamRef NewFpoStream { get { return _newFpoStream; } }
            public PdbStreamRef SectionHdrOrigStream { get { return _sectionHdrOrigStream; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Dbi M_Parent { get { return m_parent; } }
            public byte[] M_RawFpoStreamData { get { return __raw_fpoStreamData; } }
            public byte[] M_RawM_RawFpoStreamData { get { return __raw__raw_fpoStreamData; } }
            public byte[] M_RawSectionHdrStreamData { get { return __raw_sectionHdrStreamData; } }
            public byte[] M_RawM_RawSectionHdrStreamData { get { return __raw__raw_sectionHdrStreamData; } }
        }
        public partial class DbiSymbolRef : KaitaiStruct
        {
            public DbiSymbolRef(uint p_moduleIndex, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _moduleIndex = p_moduleIndex;
                f_zzzModuleIo = false;
                f_moduleIo = false;
                f_isOffsetEof = false;
                f_symbol = false;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
            }
            private bool f_zzzModuleIo;
            private GetModuleIo _zzzModuleIo;
            public GetModuleIo ZzzModuleIo
            {
                get
                {
                    if (f_zzzModuleIo)
                        return _zzzModuleIo;
                    _zzzModuleIo = new GetModuleIo(ModuleIndex, m_io, this, m_root);
                    f_zzzModuleIo = true;
                    return _zzzModuleIo;
                }
            }
            private bool f_moduleIo;
            private KaitaiStream _moduleIo;
            public KaitaiStream ModuleIo
            {
                get
                {
                    if (f_moduleIo)
                        return _moduleIo;
                    _moduleIo = (KaitaiStream) (ZzzModuleIo.Value);
                    f_moduleIo = true;
                    return _moduleIo;
                }
            }
            private bool f_isOffsetEof;
            private bool _isOffsetEof;
            public bool IsOffsetEof
            {
                get
                {
                    if (f_isOffsetEof)
                        return _isOffsetEof;
                    _isOffsetEof = (bool) (Offset >= ModuleIo.Size);
                    f_isOffsetEof = true;
                    return _isOffsetEof;
                }
            }
            private bool f_symbol;
            private DbiSymbol _symbol;
            public DbiSymbol Symbol
            {
                get
                {
                    if (f_symbol)
                        return _symbol;
                    if ( ((IsOffsetEof == false) && (Offset > 4)) ) {
                        KaitaiStream io = ModuleIo;
                        long _pos = io.Pos;
                        io.Seek((Offset - 4));
                        _symbol = new DbiSymbol(((int) (ModuleIndex)), io, this, m_root);
                        io.Seek(_pos);
                        f_symbol = true;
                    }
                    return _symbol;
                }
            }
            private uint _offset;
            private uint _moduleIndex;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint Offset { get { return _offset; } }
            public uint ModuleIndex { get { return _moduleIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: BUILDINFOSYM
        /// </remarks>
        public partial class SymBuildInfo : KaitaiStruct
        {
            public static SymBuildInfo FromFile(string fileName)
            {
                return new SymBuildInfo(new KaitaiStream(fileName));
            }

            public SymBuildInfo(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _id = m_io.ReadU4le();
            }
            private uint _id;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// CV_ItemId of Build Info.
            /// </summary>
            /// <remarks>
            /// Reference: id
            /// </remarks>
            public uint Id { get { return _id; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: tagInlineeSourceLineEx
        /// </remarks>
        public partial class C13InlineeSourceLineEx : KaitaiStruct
        {
            public static C13InlineeSourceLineEx FromFile(string fileName)
            {
                return new C13InlineeSourceLineEx(new KaitaiStream(fileName));
            }

            public C13InlineeSourceLineEx(KaitaiStream p__io, MsPdb.C13SubsectionInlineeLines p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _inlinee = m_io.ReadU4le();
                _fileId = m_io.ReadU4le();
                _sourceLineNumber = m_io.ReadU4le();
                _countOfExtraFiles = m_io.ReadU4le();
                _extraFileIds = new List<uint>();
                for (var i = 0; i < CountOfExtraFiles; i++)
                {
                    _extraFileIds.Add(m_io.ReadU4le());
                }
            }
            private uint _inlinee;
            private uint _fileId;
            private uint _sourceLineNumber;
            private uint _countOfExtraFiles;
            private List<uint> _extraFileIds;
            private MsPdb m_root;
            private MsPdb.C13SubsectionInlineeLines m_parent;

            /// <summary>
            /// function id.
            /// </summary>
            /// <remarks>
            /// Reference: inlinee
            /// </remarks>
            public uint Inlinee { get { return _inlinee; } }

            /// <summary>
            /// offset into file table DEBUG_S_FILECHKSMS
            /// </summary>
            /// <remarks>
            /// Reference: fileId
            /// </remarks>
            public uint FileId { get { return _fileId; } }

            /// <summary>
            /// definition start line number.
            /// </summary>
            /// <remarks>
            /// Reference: sourceLineNum
            /// </remarks>
            public uint SourceLineNumber { get { return _sourceLineNumber; } }

            /// <remarks>
            /// Reference: countOfExtraFiles
            /// </remarks>
            public uint CountOfExtraFiles { get { return _countOfExtraFiles; } }

            /// <remarks>
            /// Reference: extraFileId
            /// </remarks>
            public List<uint> ExtraFileIds { get { return _extraFileIds; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13SubsectionInlineeLines M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DEBUG_S_STRINGTABLE
        /// </remarks>
        public partial class C13SubsectionStringtable : KaitaiStruct
        {
            public static C13SubsectionStringtable FromFile(string fileName)
            {
                return new C13SubsectionStringtable(new KaitaiStream(fileName));
            }

            public C13SubsectionStringtable(KaitaiStream p__io, MsPdb.C13Subsection p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _strings = new List<string>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _strings.Add(System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true)));
                        i++;
                    }
                }
            }
            private List<string> _strings;
            private MsPdb m_root;
            private MsPdb.C13Subsection m_parent;
            public List<string> Strings { get { return _strings; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13Subsection M_Parent { get { return m_parent; } }
        }
        public partial class Guid : KaitaiStruct
        {
            public static Guid FromFile(string fileName)
            {
                return new Guid(new KaitaiStream(fileName));
            }

            public Guid(KaitaiStream p__io, MsPdb.PdbStreamHdrVc70 p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _data1 = m_io.ReadU4le();
                _data2 = m_io.ReadU2le();
                _data3 = m_io.ReadU2le();
                _data4 = new List<byte>();
                for (var i = 0; i < 8; i++)
                {
                    _data4.Add(m_io.ReadU1());
                }
            }
            private uint _data1;
            private ushort _data2;
            private ushort _data3;
            private List<byte> _data4;
            private MsPdb m_root;
            private MsPdb.PdbStreamHdrVc70 m_parent;
            public uint Data1 { get { return _data1; } }
            public ushort Data2 { get { return _data2; } }
            public ushort Data3 { get { return _data3; } }
            public List<byte> Data4 { get { return _data4; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbStreamHdrVc70 M_Parent { get { return m_parent; } }
        }
        public partial class LfUnknown : KaitaiStruct
        {
            public static LfUnknown FromFile(string fileName)
            {
                return new LfUnknown(new KaitaiStream(fileName));
            }

            public LfUnknown(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _data = m_io.ReadBytesFull();
            }
            private byte[] _data;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;
            public byte[] Data { get { return _data; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_MFUNCTION
        /// </summary>
        /// <remarks>
        /// Reference: lfMFunc
        /// </remarks>
        public partial class LfMfunction : KaitaiStruct
        {
            public static LfMfunction FromFile(string fileName)
            {
                return new LfMfunction(new KaitaiStream(fileName));
            }

            public LfMfunction(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _returnType = new TpiTypeRef(m_io, this, m_root);
                _classType = new TpiTypeRef(m_io, this, m_root);
                _thisType = new TpiTypeRef(m_io, this, m_root);
                _callingConvention = ((MsPdb.Tpi.CallingConvention) m_io.ReadU1());
                _attributes = new CvFuncAttributes(m_io, this, m_root);
                _parametersCount = m_io.ReadU2le();
                _argumentListType = new TpiTypeRef(m_io, this, m_root);
                _thisAdjuster = m_io.ReadU4le();
            }
            private TpiTypeRef _returnType;
            private TpiTypeRef _classType;
            private TpiTypeRef _thisType;
            private Tpi.CallingConvention _callingConvention;
            private CvFuncAttributes _attributes;
            private ushort _parametersCount;
            private TpiTypeRef _argumentListType;
            private uint _thisAdjuster;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// type index of return value
            /// </summary>
            /// <remarks>
            /// Reference: rvtype
            /// </remarks>
            public TpiTypeRef ReturnType { get { return _returnType; } }

            /// <summary>
            /// type index of containing class
            /// </summary>
            /// <remarks>
            /// Reference: classtype
            /// </remarks>
            public TpiTypeRef ClassType { get { return _classType; } }

            /// <summary>
            /// type index of this pointer (model specific)
            /// </summary>
            /// <remarks>
            /// Reference: thistype
            /// </remarks>
            public TpiTypeRef ThisType { get { return _thisType; } }

            /// <summary>
            /// calling convention (call_t)
            /// </summary>
            /// <remarks>
            /// Reference: calltype
            /// </remarks>
            public Tpi.CallingConvention CallingConvention { get { return _callingConvention; } }

            /// <summary>
            /// attributes
            /// </summary>
            /// <remarks>
            /// Reference: funcattr
            /// </remarks>
            public CvFuncAttributes Attributes { get { return _attributes; } }

            /// <summary>
            /// number of parameters
            /// </summary>
            /// <remarks>
            /// Reference: parmcount
            /// </remarks>
            public ushort ParametersCount { get { return _parametersCount; } }

            /// <summary>
            /// type index of argument list
            /// </summary>
            /// <remarks>
            /// Reference: arglist
            /// </remarks>
            public TpiTypeRef ArgumentListType { get { return _argumentListType; } }

            /// <summary>
            /// this adjuster (long because pad required anyway)
            /// </summary>
            /// <remarks>
            /// Reference: thisadjust
            /// </remarks>
            public uint ThisAdjuster { get { return _thisAdjuster; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: MLI
        /// </remarks>
        public partial class C11Lines : KaitaiStruct
        {
            public static C11Lines FromFile(string fileName)
            {
                return new C11Lines(new KaitaiStream(fileName));
            }

            public C11Lines(KaitaiStream p__io, MsPdb.ModuleStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_alignMarker = false;
                f_zzzAlignSize = false;
                f_paddingSize = false;
                _read();
            }
            private void _read()
            {
                _numFiles = m_io.ReadU2le();
                _numSections = m_io.ReadU2le();
                _fileOffsets = new List<uint>();
                for (var i = 0; i < NumFiles; i++)
                {
                    _fileOffsets.Add(m_io.ReadU4le());
                }
                _sectionsInfo = new List<C11SectionInfo>();
                for (var i = 0; i < NumSections; i++)
                {
                    _sectionsInfo.Add(new C11SectionInfo(m_io, this, m_root));
                }
                if (AlignMarker >= 0) {
                    _invokeAlignMarker = m_io.ReadBytes(0);
                }
                __unnamed5 = m_io.ReadBytes(PaddingSize);
                _srcFiles = new List<C11Srcfile>();
                for (var i = 0; i < NumFiles; i++)
                {
                    _srcFiles.Add(new C11Srcfile(m_io, this, m_root));
                }
            }
            private bool f_alignMarker;
            private int _alignMarker;
            public int AlignMarker
            {
                get
                {
                    if (f_alignMarker)
                        return _alignMarker;
                    _alignMarker = (int) (M_Io.Pos);
                    f_alignMarker = true;
                    return _alignMarker;
                }
            }
            private bool f_zzzAlignSize;
            private Align _zzzAlignSize;
            public Align ZzzAlignSize
            {
                get
                {
                    if (f_zzzAlignSize)
                        return _zzzAlignSize;
                    _zzzAlignSize = new Align(((uint) (AlignMarker)), 4, m_io, this, m_root);
                    f_zzzAlignSize = true;
                    return _zzzAlignSize;
                }
            }
            private bool f_paddingSize;
            private int _paddingSize;
            public int PaddingSize
            {
                get
                {
                    if (f_paddingSize)
                        return _paddingSize;
                    _paddingSize = (int) (ZzzAlignSize.PaddingSize);
                    f_paddingSize = true;
                    return _paddingSize;
                }
            }
            private ushort _numFiles;
            private ushort _numSections;
            private List<uint> _fileOffsets;
            private List<C11SectionInfo> _sectionsInfo;
            private byte[] _invokeAlignMarker;
            private byte[] __unnamed5;
            private List<C11Srcfile> _srcFiles;
            private MsPdb m_root;
            private MsPdb.ModuleStream m_parent;

            /// <remarks>
            /// Reference: cfiles
            /// </remarks>
            public ushort NumFiles { get { return _numFiles; } }

            /// <remarks>
            /// Reference: csect
            /// </remarks>
            public ushort NumSections { get { return _numSections; } }
            public List<uint> FileOffsets { get { return _fileOffsets; } }
            public List<C11SectionInfo> SectionsInfo { get { return _sectionsInfo; } }
            public byte[] InvokeAlignMarker { get { return _invokeAlignMarker; } }

            /// <remarks>
            /// Reference: offFile
            /// </remarks>
            public byte[] Unnamed_5 { get { return __unnamed5; } }
            public List<C11Srcfile> SrcFiles { get { return _srcFiles; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.ModuleStream M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: lfProc
        /// </remarks>
        public partial class LfProcedure : KaitaiStruct
        {
            public static LfProcedure FromFile(string fileName)
            {
                return new LfProcedure(new KaitaiStream(fileName));
            }

            public LfProcedure(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _returnValueType = new TpiTypeRef(m_io, this, m_root);
                _callingConvention = ((MsPdb.Tpi.CallingConvention) m_io.ReadU1());
                _functionAttributes = new CvFuncAttributes(m_io, this, m_root);
                _parameterCount = m_io.ReadU2le();
                _arglist = new TpiTypeRef(m_io, this, m_root);
            }
            private TpiTypeRef _returnValueType;
            private Tpi.CallingConvention _callingConvention;
            private CvFuncAttributes _functionAttributes;
            private ushort _parameterCount;
            private TpiTypeRef _arglist;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// type index of return value
            /// </summary>
            /// <remarks>
            /// Reference: rvtype
            /// </remarks>
            public TpiTypeRef ReturnValueType { get { return _returnValueType; } }

            /// <summary>
            /// calling convention (CV_call_t)
            /// </summary>
            /// <remarks>
            /// Reference: calltype
            /// </remarks>
            public Tpi.CallingConvention CallingConvention { get { return _callingConvention; } }

            /// <summary>
            /// attributes
            /// </summary>
            /// <remarks>
            /// Reference: funcattr
            /// </remarks>
            public CvFuncAttributes FunctionAttributes { get { return _functionAttributes; } }

            /// <summary>
            /// number of parameters
            /// </summary>
            /// <remarks>
            /// Reference: parmcount
            /// </remarks>
            public ushort ParameterCount { get { return _parameterCount; } }

            /// <summary>
            /// type index of argument list
            /// </summary>
            /// <remarks>
            /// Reference: arglist
            /// </remarks>
            public TpiTypeRef Arglist { get { return _arglist; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class DbiSymbol : KaitaiStruct
        {
            public DbiSymbol(int p_moduleIndex, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _moduleIndex = p_moduleIndex;
                f_zzzExtraData = false;
                f_dataPos = false;
                f_actualLength = false;
                f_data = false;
                _read();
            }
            private void _read()
            {
                _length = m_io.ReadU2le();
                if (DataPos >= 0) {
                    _invokeDataPos = m_io.ReadBytes(0);
                }
                __unnamed2 = m_io.ReadBytes(ActualLength);
            }
            private bool f_zzzExtraData;
            private DbiExtraData _zzzExtraData;
            public DbiExtraData ZzzExtraData
            {
                get
                {
                    if (f_zzzExtraData)
                        return _zzzExtraData;
                    long _pos = m_io.Pos;
                    m_io.Seek(DataPos);
                    _zzzExtraData = new DbiExtraData(m_io, this, m_root);
                    m_io.Seek(_pos);
                    f_zzzExtraData = true;
                    return _zzzExtraData;
                }
            }
            private bool f_dataPos;
            private int _dataPos;
            public int DataPos
            {
                get
                {
                    if (f_dataPos)
                        return _dataPos;
                    _dataPos = (int) (M_Io.Pos);
                    f_dataPos = true;
                    return _dataPos;
                }
            }
            private bool f_actualLength;
            private int _actualLength;
            public int ActualLength
            {
                get
                {
                    if (f_actualLength)
                        return _actualLength;
                    _actualLength = (int) ((Length + ZzzExtraData.ExtraLength));
                    f_actualLength = true;
                    return _actualLength;
                }
            }
            private bool f_data;
            private DbiSymbolData _data;
            public DbiSymbolData Data
            {
                get
                {
                    if (f_data)
                        return _data;
                    if (Length > 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(DataPos);
                        __raw_data = m_io.ReadBytes(ActualLength);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new DbiSymbolData(((ushort) (ActualLength)), io___raw_data, this, m_root);
                        m_io.Seek(_pos);
                        f_data = true;
                    }
                    return _data;
                }
            }
            private ushort _length;
            private byte[] _invokeDataPos;
            private byte[] __unnamed2;
            private int _moduleIndex;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            private byte[] __raw_data;
            public ushort Length { get { return _length; } }
            public byte[] InvokeDataPos { get { return _invokeDataPos; } }
            public byte[] Unnamed_2 { get { return __unnamed2; } }
            public int ModuleIndex { get { return _moduleIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
            public byte[] M_RawData { get { return __raw_data; } }
        }

        /// <remarks>
        /// Reference: LinkInfo
        /// </remarks>
        public partial class LinkInfo : KaitaiStruct
        {
            public static LinkInfo FromFile(string fileName)
            {
                return new LinkInfo(new KaitaiStream(fileName));
            }


            public enum LinkInfoVersion
            {
                VliOne = 1,
                VliTwo = 2,
            }
            public LinkInfo(KaitaiStream p__io, MsPdb.LinkInfoStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _recordSize = m_io.ReadU4le();
                _version = ((LinkInfoVersion) m_io.ReadU4le());
                _cwdOffset = m_io.ReadU4le();
                _commandOffset = m_io.ReadU4le();
                _outputFileIndex = m_io.ReadU4le();
                _libsOffset = m_io.ReadU4le();
            }
            private uint _recordSize;
            private LinkInfoVersion _version;
            private uint _cwdOffset;
            private uint _commandOffset;
            private uint _outputFileIndex;
            private uint _libsOffset;
            private MsPdb m_root;
            private MsPdb.LinkInfoStream m_parent;

            /// <summary>
            /// size of the whole record.  computed as sizeof(LinkInfo) + strlen(szCwd) + 1 + strlen(szCommand) + 1
            /// </summary>
            /// <remarks>
            /// Reference: cb
            /// </remarks>
            public uint RecordSize { get { return _recordSize; } }

            /// <summary>
            /// version of this record (VerLinkInfo)
            /// </summary>
            /// <remarks>
            /// Reference: ver
            /// </remarks>
            public LinkInfoVersion Version { get { return _version; } }

            /// <summary>
            /// offset from base of this record to szCwd
            /// </summary>
            /// <remarks>
            /// Reference: offszCwd
            /// </remarks>
            public uint CwdOffset { get { return _cwdOffset; } }

            /// <summary>
            /// offset from base of this record
            /// </summary>
            /// <remarks>
            /// Reference: offszCommand
            /// </remarks>
            public uint CommandOffset { get { return _commandOffset; } }

            /// <summary>
            /// index of start of output file in szCommand
            /// </summary>
            /// <remarks>
            /// Reference: ichOutfile
            /// </remarks>
            public uint OutputFileIndex { get { return _outputFileIndex; } }

            /// <summary>
            /// offset from base of this record to szLibs
            /// </summary>
            /// <remarks>
            /// Reference: offszLibs
            /// </remarks>
            public uint LibsOffset { get { return _libsOffset; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.LinkInfoStream M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CFLAGSYM
        /// </remarks>
        public partial class SymCompile : KaitaiStruct
        {
            public static SymCompile FromFile(string fileName)
            {
                return new SymCompile(new KaitaiStream(fileName));
            }

            public SymCompile(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _machine = m_io.ReadU1();
                _language = m_io.ReadU1();
                _pcode = m_io.ReadBitsIntLe(1) != 0;
                _floatprec = m_io.ReadBitsIntLe(2);
                _floatpkg = m_io.ReadBitsIntLe(2);
                _ambdata = m_io.ReadBitsIntLe(3);
                _ambcode = m_io.ReadBitsIntLe(3);
                _mode32 = m_io.ReadBitsIntLe(1) != 0;
                _pad = m_io.ReadBitsIntLe(4);
                m_io.AlignToByte();
                _versionString = new PdbString(true, m_io, this, m_root);
            }
            private byte _machine;
            private byte _language;
            private bool _pcode;
            private ulong _floatprec;
            private ulong _floatpkg;
            private ulong _ambdata;
            private ulong _ambcode;
            private bool _mode32;
            private ulong _pad;
            private PdbString _versionString;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// target processor
            /// </summary>
            /// <remarks>
            /// Reference: machine
            /// </remarks>
            public byte Machine { get { return _machine; } }

            /// <summary>
            /// language index
            /// </summary>
            /// <remarks>
            /// Reference: language
            /// </remarks>
            public byte Language { get { return _language; } }

            /// <summary>
            /// true if pcode present
            /// </summary>
            /// <remarks>
            /// Reference: pcode
            /// </remarks>
            public bool Pcode { get { return _pcode; } }

            /// <summary>
            /// floating precision
            /// </summary>
            /// <remarks>
            /// Reference: floatprec
            /// </remarks>
            public ulong Floatprec { get { return _floatprec; } }

            /// <summary>
            /// float package
            /// </summary>
            /// <remarks>
            /// Reference: floatpkg
            /// </remarks>
            public ulong Floatpkg { get { return _floatpkg; } }

            /// <summary>
            /// ambient data model
            /// </summary>
            /// <remarks>
            /// Reference: ambdata
            /// </remarks>
            public ulong Ambdata { get { return _ambdata; } }

            /// <summary>
            /// ambient code model
            /// </summary>
            /// <remarks>
            /// Reference: ambcode
            /// </remarks>
            public ulong Ambcode { get { return _ambcode; } }

            /// <summary>
            /// true if compiled 32 bit mode
            /// </summary>
            /// <remarks>
            /// Reference: mode32
            /// </remarks>
            public bool Mode32 { get { return _mode32; } }

            /// <summary>
            /// reserved
            /// </summary>
            /// <remarks>
            /// Reference: pad
            /// </remarks>
            public ulong Pad { get { return _pad; } }

            /// <summary>
            /// Length-prefixed compiler version string
            /// </summary>
            /// <remarks>
            /// Reference: ver
            /// </remarks>
            public PdbString VersionString { get { return _versionString; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: COFFGROUPSYM
        /// </remarks>
        public partial class SymCoffGroup : KaitaiStruct
        {
            public static SymCoffGroup FromFile(string fileName)
            {
                return new SymCoffGroup(new KaitaiStream(fileName));
            }

            public SymCoffGroup(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _size = m_io.ReadU4le();
                _characteristics = m_io.ReadU4le();
                _symbolOffset = m_io.ReadU4le();
                _symbolSegment = m_io.ReadU2le();
                _name = new PdbString(false, m_io, this, m_root);
            }
            private uint _size;
            private uint _characteristics;
            private uint _symbolOffset;
            private ushort _symbolSegment;
            private PdbString _name;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// cb
            /// </summary>
            /// <remarks>
            /// Reference: cb
            /// </remarks>
            public uint Size { get { return _size; } }

            /// <remarks>
            /// Reference: characteristics
            /// </remarks>
            public uint Characteristics { get { return _characteristics; } }

            /// <summary>
            /// Symbol offset
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint SymbolOffset { get { return _symbolOffset; } }

            /// <summary>
            /// Symbol segment
            /// </summary>
            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort SymbolSegment { get { return _symbolSegment; } }

            /// <summary>
            /// name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class PdbStreamTable : KaitaiStruct
        {
            public static PdbStreamTable FromFile(string fileName)
            {
                return new PdbStreamTable(new KaitaiStream(fileName));
            }

            public PdbStreamTable(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _numStreams = m_io.ReadU4le();
                if (M_Root.PdbType == MsPdb.PdbTypeEnum.Big) {
                    _streamSizesDs = new List<PdbStreamEntryDs>();
                    for (var i = 0; i < NumStreams; i++)
                    {
                        _streamSizesDs.Add(new PdbStreamEntryDs(i, m_io, this, m_root));
                    }
                }
                if (M_Root.PdbType == MsPdb.PdbTypeEnum.Small) {
                    _streamSizesJg = new List<PdbStreamEntryJg>();
                    for (var i = 0; i < NumStreams; i++)
                    {
                        _streamSizesJg.Add(new PdbStreamEntryJg(((uint) (i)), m_io, this, m_root));
                    }
                }
                _streams = new List<PdbStreamPagelist>();
                for (var i = 0; i < NumStreams; i++)
                {
                    _streams.Add(new PdbStreamPagelist(((uint) (i)), m_io, this, m_root));
                }
            }
            private uint _numStreams;
            private List<PdbStreamEntryDs> _streamSizesDs;
            private List<PdbStreamEntryJg> _streamSizesJg;
            private List<PdbStreamPagelist> _streams;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint NumStreams { get { return _numStreams; } }
            public List<PdbStreamEntryDs> StreamSizesDs { get { return _streamSizesDs; } }
            public List<PdbStreamEntryJg> StreamSizesJg { get { return _streamSizesJg; } }
            public List<PdbStreamPagelist> Streams { get { return _streams; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: GSIHashHdr
        /// </remarks>
        public partial class GsiHdr : KaitaiStruct
        {
            public static GsiHdr FromFile(string fileName)
            {
                return new GsiHdr(new KaitaiStream(fileName));
            }


            public enum VersionEnum : uint
            {
                V70 = 4046391578,
            }
            public GsiHdr(KaitaiStream p__io, MsPdb.GlobalSymbolsStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_numHashRecords = false;
                _read();
            }
            private void _read()
            {
                _signature = m_io.ReadU4le();
                _version = ((VersionEnum) m_io.ReadU4le());
                _sizeHashRecords = m_io.ReadU4le();
                _sizeHashBuckets = m_io.ReadU4le();
            }
            private bool f_numHashRecords;
            private int _numHashRecords;
            public int NumHashRecords
            {
                get
                {
                    if (f_numHashRecords)
                        return _numHashRecords;
                    _numHashRecords = (int) ((SizeHashRecords / 8));
                    f_numHashRecords = true;
                    return _numHashRecords;
                }
            }
            private uint _signature;
            private VersionEnum _version;
            private uint _sizeHashRecords;
            private uint _sizeHashBuckets;
            private MsPdb m_root;
            private MsPdb.GlobalSymbolsStream m_parent;

            /// <remarks>
            /// Reference: verSignature
            /// </remarks>
            public uint Signature { get { return _signature; } }

            /// <remarks>
            /// Reference: verHdr
            /// </remarks>
            public VersionEnum Version { get { return _version; } }

            /// <remarks>
            /// Reference: cbHr
            /// </remarks>
            public uint SizeHashRecords { get { return _sizeHashRecords; } }

            /// <remarks>
            /// Reference: cbBuckets
            /// </remarks>
            public uint SizeHashBuckets { get { return _sizeHashBuckets; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.GlobalSymbolsStream M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: LABELSYM32
        /// </remarks>
        public partial class SymLabel32 : KaitaiStruct
        {
            public SymLabel32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _flags = new CvProcFlags(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private uint _offset;
            private ushort _segment;
            private CvProcFlags _flags;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <summary>
            /// flags
            /// </summary>
            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public CvProcFlags Flags { get { return _flags; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DATASYMHLSL
        /// </remarks>
        public partial class SymDataHlsl : KaitaiStruct
        {
            public static SymDataHlsl FromFile(string fileName)
            {
                return new SymDataHlsl(new KaitaiStream(fileName));
            }

            public SymDataHlsl(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _regType = m_io.ReadU2le();
                _dataSlot = m_io.ReadU2le();
                _dataOffset = m_io.ReadU2le();
                _texSlot = m_io.ReadU2le();
                _sampSlot = m_io.ReadU2le();
                _uavSlot = m_io.ReadU2le();
                _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
            }
            private TpiTypeRef _type;
            private ushort _regType;
            private ushort _dataSlot;
            private ushort _dataOffset;
            private ushort _texSlot;
            private ushort _sampSlot;
            private ushort _uavSlot;
            private string _name;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Type index
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// register type from CV_HLSLREG_e
            /// </summary>
            /// <remarks>
            /// Reference: regType
            /// </remarks>
            public ushort RegType { get { return _regType; } }

            /// <summary>
            /// Base data (cbuffer, groupshared, etc.) slot
            /// </summary>
            /// <remarks>
            /// Reference: dataslot
            /// </remarks>
            public ushort DataSlot { get { return _dataSlot; } }

            /// <summary>
            /// Base data byte offset start
            /// </summary>
            /// <remarks>
            /// Reference: dataoff
            /// </remarks>
            public ushort DataOffset { get { return _dataOffset; } }

            /// <summary>
            /// Texture slot start
            /// </summary>
            /// <remarks>
            /// Reference: texslot
            /// </remarks>
            public ushort TexSlot { get { return _texSlot; } }

            /// <summary>
            /// Sampler slot start
            /// </summary>
            /// <remarks>
            /// Reference: sampslot
            /// </remarks>
            public ushort SampSlot { get { return _sampSlot; } }

            /// <summary>
            /// UAV slot start
            /// </summary>
            /// <remarks>
            /// Reference: uavslot
            /// </remarks>
            public ushort UavSlot { get { return _uavSlot; } }

            /// <summary>
            /// name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public string Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class Tpi : KaitaiStruct
        {
            public static Tpi FromFile(string fileName)
            {
                return new Tpi(new KaitaiStream(fileName));
            }


            public enum CvAccess
            {
                Private = 1,
                Protected = 2,
                Public = 3,
            }

            public enum LeafType
            {
                LfModifier16t = 1,
                LfPointer16t = 2,
                LfArray16t = 3,
                LfClass16t = 4,
                LfStructure16t = 5,
                LfUnion16t = 6,
                LfEnum16t = 7,
                LfProcedure16t = 8,
                LfMfunction16t = 9,
                LfVtshape = 10,
                LfCobol016t = 11,
                LfCobol1 = 12,
                LfBarray16t = 13,
                LfLabel = 14,
                LfNull = 15,
                LfNottran = 16,
                LfDimarray16t = 17,
                LfVftpath16t = 18,
                LfPrecomp16t = 19,
                LfEndprecomp = 20,
                LfOem16t = 21,
                LfTypeserverSt = 22,
                LfPad0 = 240,
                LfPad1 = 241,
                LfPad2 = 242,
                LfPad3 = 243,
                LfPad4 = 244,
                LfPad5 = 245,
                LfPad6 = 246,
                LfPad7 = 247,
                LfPad8 = 248,
                LfPad9 = 249,
                LfPad10 = 250,
                LfPad11 = 251,
                LfPad12 = 252,
                LfPad13 = 253,
                LfPad14 = 254,
                LfPad15 = 255,
                LfSkip16t = 512,
                LfArglist16t = 513,
                LfDefarg16t = 514,
                LfList = 515,
                LfFieldlist16t = 516,
                LfDerived16t = 517,
                LfBitfield16t = 518,
                LfMethodlist16t = 519,
                LfDimconu16t = 520,
                LfDimconlu16t = 521,
                LfDimvaru16t = 522,
                LfDimvarlu16t = 523,
                LfRefsym = 524,
                LfBclass16t = 1024,
                LfVbclass16t = 1025,
                LfIvbclass16t = 1026,
                LfEnumerateSt = 1027,
                LfFriendfcn16t = 1028,
                LfIndex16t = 1029,
                LfMember16t = 1030,
                LfStmember16t = 1031,
                LfMethod16t = 1032,
                LfNesttype16t = 1033,
                LfVfunctab16t = 1034,
                LfFriendcls16t = 1035,
                LfOneMethod16t = 1036,
                LfVfuncoff16t = 1037,
                LfNesttypeex16t = 1038,
                LfMembermodify16t = 1039,
                LfTi16Max = 4096,
                LfModifier = 4097,
                LfPointer = 4098,
                LfArraySt = 4099,
                LfClassSt = 4100,
                LfStructureSt = 4101,
                LfUnionSt = 4102,
                LfEnumSt = 4103,
                LfProcedure = 4104,
                LfMfunction = 4105,
                LfCobol0 = 4106,
                LfBarray = 4107,
                LfDimarraySt = 4108,
                LfVftpath = 4109,
                LfPrecompSt = 4110,
                LfOem = 4111,
                LfAliasSt = 4112,
                LfOem2 = 4113,
                LfSkip = 4608,
                LfArglist = 4609,
                LfDefargSt = 4610,
                LfFieldlist = 4611,
                LfDerived = 4612,
                LfBitfield = 4613,
                LfMethodlist = 4614,
                LfDimconu = 4615,
                LfDimconlu = 4616,
                LfDimvaru = 4617,
                LfDimvarlu = 4618,
                LfBclass = 5120,
                LfVbclass = 5121,
                LfIvbclass = 5122,
                LfFriendfcnSt = 5123,
                LfIndex = 5124,
                LfMemberSt = 5125,
                LfStmemberSt = 5126,
                LfMethodSt = 5127,
                LfNesttypeSt = 5128,
                LfVfunctab = 5129,
                LfFriendcls = 5130,
                LfOneMethodSt = 5131,
                LfVfuncoff = 5132,
                LfNesttypeexSt = 5133,
                LfMembermodifySt = 5134,
                LfManagedSt = 5135,
                LfTypeserver = 5377,
                LfEnumerate = 5378,
                LfArray = 5379,
                LfClass = 5380,
                LfStructure = 5381,
                LfUnion = 5382,
                LfEnum = 5383,
                LfDimarray = 5384,
                LfPrecomp = 5385,
                LfAlias = 5386,
                LfDefarg = 5387,
                LfFriendfcn = 5388,
                LfMember = 5389,
                LfStmember = 5390,
                LfMethod = 5391,
                LfNesttype = 5392,
                LfOneMethod = 5393,
                LfNesttypeex = 5394,
                LfMembermodify = 5395,
                LfManaged = 5396,
                LfTypeserver2 = 5397,
                LfStridedArray = 5398,
                LfHlsl = 5399,
                LfModifierEx = 5400,
                LfInterface = 5401,
                LfBinterface = 5402,
                LfVector = 5403,
                LfMatrix = 5404,
                LfVftable = 5405,
                LfFuncId = 5633,
                LfMfuncId = 5634,
                LfBuildinfo = 5635,
                LfSubstrList = 5636,
                LfStringId = 5637,
                LfUdtSrcLine = 5638,
                LfUdtModSrcLine = 5639,
                LfChar = 32768,
                LfShort = 32769,
                LfUshort = 32770,
                LfLong = 32771,
                LfUlong = 32772,
                LfReal32 = 32773,
                LfReal64 = 32774,
                LfReal80 = 32775,
                LfReal128 = 32776,
                LfQuadword = 32777,
                LfUquadword = 32778,
                LfReal48 = 32779,
                LfComplex32 = 32780,
                LfComplex64 = 32781,
                LfComplex80 = 32782,
                LfComplex128 = 32783,
                LfVarstring = 32784,
                LfOctword = 32791,
                LfUoctword = 32792,
                LfDecimal = 32793,
                LfDate = 32794,
                LfUtf8string = 32795,
                LfReal16 = 32796,
            }

            public enum CvVtsDesc
            {
                Near = 0,
                Far = 1,
                Thin = 2,
                Outer = 3,
                Meta = 4,
                Near32 = 5,
                Far32 = 6,
                Unused = 7,
            }

            public enum CvMethodprop
            {
                Vanilla = 0,
                Virtual = 1,
                Static = 2,
                Friend = 3,
                Intro = 4,
                PureVirtual = 5,
                PureIntro = 6,
            }

            public enum CvPmtype
            {
                Undef = 0,
                DSingle = 1,
                DMultiple = 2,
                DVirtual = 3,
                DGeneral = 4,
                FSingle = 5,
                FMultiple = 6,
                FVirtual = 7,
                FGeneral = 8,
            }

            public enum CallingConvention
            {
                NearC = 0,
                FarC = 1,
                NearPascal = 2,
                FarPascal = 3,
                NearFast = 4,
                FarFast = 5,
                NearStd = 7,
                FarStd = 8,
                NearSys = 9,
                FarSys = 10,
                Thiscall = 11,
                Mipscall = 12,
                Generic = 13,
                Alphacall = 14,
                Ppccall = 15,
                Shcall = 16,
                Armcall = 17,
                Am33call = 18,
                Tricall = 19,
                Sh5call = 20,
                Mr32call = 21,
                Clrcall = 22,
                Inline = 23,
                NearVector = 24,
            }

            public enum TpiVersion
            {
                V40 = 19950410,
                V41 = 19951122,
                V50Beta = 19960307,
                V50 = 19961031,
                V70 = 19990903,
                V80 = 20040203,
            }

            public enum CvHfa
            {
                None = 0,
                Float = 1,
                Double = 2,
                Other = 3,
            }

            public enum CvCookietype
            {
                Copy = 0,
                XorSp = 1,
                OxrBp = 2,
                XorR13 = 3,
            }

            public enum CvMocomUdt
            {
                None = 0,
                Ref = 1,
                Value = 2,
                Interface = 3,
            }

            public enum CvPtrmode
            {
                Pointer = 0,
                Reference = 1,
                PointerMemberData = 2,
                PointerMemberFunction = 3,
                RvalueReference = 4,
                Reserved = 5,
            }

            public enum CvPtrtype
            {
                Near = 0,
                Far = 1,
                Huge = 2,
                BaseSeg = 3,
                BaseVal = 4,
                BaseSegVal = 5,
                BaseAddr = 6,
                BaseSegAddr = 7,
                BaseType = 8,
                BaseSelf = 9,
                Near32 = 10,
                Far32 = 11,
                Ptr64 = 12,
                Unused = 13,
            }
            public Tpi(KaitaiStream p__io, MsPdb p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_version = false;
                f_hasHeader16 = false;
                f_minTypeIndex = false;
                f_types = false;
                f_maxTypeIndex = false;
                _read();
            }
            private void _read()
            {
                if (Version <= TpiVersion.V41) {
                    _header16 = new TpiHeader16(m_io, this, m_root);
                }
                if (Version > TpiVersion.V41) {
                    _header32 = new TpiHeader(m_io, this, m_root);
                }
                _typesData = m_io.ReadBytesFull();
            }
            private bool f_version;
            private Tpi.TpiVersion _version;
            public Tpi.TpiVersion Version
            {
                get
                {
                    if (f_version)
                        return _version;
                    long _pos = m_io.Pos;
                    m_io.Seek(0);
                    _version = ((TpiVersion) m_io.ReadU4le());
                    m_io.Seek(_pos);
                    f_version = true;
                    return _version;
                }
            }
            private bool f_hasHeader16;
            private bool _hasHeader16;
            public bool HasHeader16
            {
                get
                {
                    if (f_hasHeader16)
                        return _hasHeader16;
                    _hasHeader16 = (bool) (Version <= TpiVersion.V41);
                    f_hasHeader16 = true;
                    return _hasHeader16;
                }
            }
            private bool f_minTypeIndex;
            private uint _minTypeIndex;
            public uint MinTypeIndex
            {
                get
                {
                    if (f_minTypeIndex)
                        return _minTypeIndex;
                    _minTypeIndex = (uint) ((HasHeader16 ? Header16.MinTypeIndex : Header32.MinTypeIndex));
                    f_minTypeIndex = true;
                    return _minTypeIndex;
                }
            }
            private bool f_types;
            private TpiTypes _types;
            public TpiTypes Types
            {
                get
                {
                    if (f_types)
                        return _types;
                    __raw__raw_types = m_io.ReadBytes(0);
                    Cat _process__raw__raw_types = new Cat(TypesData);
                    __raw_types = _process__raw__raw_types.Decode(__raw__raw_types);
                    var io___raw_types = new KaitaiStream(__raw_types);
                    _types = new TpiTypes(io___raw_types, this, m_root);
                    f_types = true;
                    return _types;
                }
            }
            private bool f_maxTypeIndex;
            private uint _maxTypeIndex;
            public uint MaxTypeIndex
            {
                get
                {
                    if (f_maxTypeIndex)
                        return _maxTypeIndex;
                    _maxTypeIndex = (uint) ((HasHeader16 ? Header16.MaxTypeIndex : Header32.MaxTypeIndex));
                    f_maxTypeIndex = true;
                    return _maxTypeIndex;
                }
            }
            private TpiHeader16 _header16;
            private TpiHeader _header32;
            private byte[] _typesData;
            private MsPdb m_root;
            private MsPdb m_parent;
            private byte[] __raw_types;
            private byte[] __raw__raw_types;
            public TpiHeader16 Header16 { get { return _header16; } }
            public TpiHeader Header32 { get { return _header32; } }
            public byte[] TypesData { get { return _typesData; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb M_Parent { get { return m_parent; } }
            public byte[] M_RawTypes { get { return __raw_types; } }
            public byte[] M_RawM_RawTypes { get { return __raw__raw_types; } }
        }

        /// <remarks>
        /// Reference: IMAGE_SECTION_HEADER
        /// </remarks>
        public partial class ImageSectionHeader : KaitaiStruct
        {
            public static ImageSectionHeader FromFile(string fileName)
            {
                return new ImageSectionHeader(new KaitaiStream(fileName));
            }

            public ImageSectionHeader(KaitaiStream p__io, MsPdb.DebugSectionHdrStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_physicalAddress = false;
                f_virtualSize = false;
                _read();
            }
            private void _read()
            {
                _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(KaitaiStream.BytesStripRight(m_io.ReadBytes(8), 0));
                _misc = m_io.ReadU4le();
                _virtualAddress = m_io.ReadU4le();
                _sizeOfRawData = m_io.ReadU4le();
                _pointerToRawData = m_io.ReadU4le();
                _pointerToRelocations = m_io.ReadU4le();
                _pointerToLineNumbers = m_io.ReadU4le();
                _numberOfRelocations = m_io.ReadU2le();
                _numberOfLineNumbers = m_io.ReadU2le();
                _characteristics = m_io.ReadU4le();
            }
            private bool f_physicalAddress;
            private uint _physicalAddress;

            /// <remarks>
            /// Reference: Misc.PhysicalAddress
            /// </remarks>
            public uint PhysicalAddress
            {
                get
                {
                    if (f_physicalAddress)
                        return _physicalAddress;
                    _physicalAddress = (uint) (Misc);
                    f_physicalAddress = true;
                    return _physicalAddress;
                }
            }
            private bool f_virtualSize;
            private uint _virtualSize;

            /// <remarks>
            /// Reference: Misc.VirtualSize
            /// </remarks>
            public uint VirtualSize
            {
                get
                {
                    if (f_virtualSize)
                        return _virtualSize;
                    _virtualSize = (uint) (Misc);
                    f_virtualSize = true;
                    return _virtualSize;
                }
            }
            private string _name;
            private uint _misc;
            private uint _virtualAddress;
            private uint _sizeOfRawData;
            private uint _pointerToRawData;
            private uint _pointerToRelocations;
            private uint _pointerToLineNumbers;
            private ushort _numberOfRelocations;
            private ushort _numberOfLineNumbers;
            private uint _characteristics;
            private MsPdb m_root;
            private MsPdb.DebugSectionHdrStream m_parent;

            /// <remarks>
            /// Reference: Name
            /// </remarks>
            public string Name { get { return _name; } }

            /// <remarks>
            /// Reference: Misc
            /// </remarks>
            public uint Misc { get { return _misc; } }

            /// <remarks>
            /// Reference: VirtualAddress
            /// </remarks>
            public uint VirtualAddress { get { return _virtualAddress; } }

            /// <remarks>
            /// Reference: SizeOfRawData
            /// </remarks>
            public uint SizeOfRawData { get { return _sizeOfRawData; } }

            /// <remarks>
            /// Reference: PointerToRawData
            /// </remarks>
            public uint PointerToRawData { get { return _pointerToRawData; } }

            /// <remarks>
            /// Reference: PointerToRelocations
            /// </remarks>
            public uint PointerToRelocations { get { return _pointerToRelocations; } }

            /// <remarks>
            /// Reference: PointerToLinenumbers
            /// </remarks>
            public uint PointerToLineNumbers { get { return _pointerToLineNumbers; } }

            /// <remarks>
            /// Reference: NumberOfRelocations
            /// </remarks>
            public ushort NumberOfRelocations { get { return _numberOfRelocations; } }

            /// <remarks>
            /// Reference: NumberOfLinenumbers
            /// </remarks>
            public ushort NumberOfLineNumbers { get { return _numberOfLineNumbers; } }

            /// <remarks>
            /// Reference: Characteristics
            /// </remarks>
            public uint Characteristics { get { return _characteristics; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DebugSectionHdrStream M_Parent { get { return m_parent; } }
        }
        public partial class Align : KaitaiStruct
        {
            public Align(uint p_value, uint p_alignment, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _value = p_value;
                _alignment = p_alignment;
                f_paddingSize = false;
                f_aligned = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_paddingSize;
            private int _paddingSize;
            public int PaddingSize
            {
                get
                {
                    if (f_paddingSize)
                        return _paddingSize;
                    _paddingSize = (int) ((Aligned - Value));
                    f_paddingSize = true;
                    return _paddingSize;
                }
            }
            private bool f_aligned;
            private int _aligned;
            public int Aligned
            {
                get
                {
                    if (f_aligned)
                        return _aligned;
                    _aligned = (int) ((((Value + Alignment) - 1) & ((Alignment - 1) ^ -1)));
                    f_aligned = true;
                    return _aligned;
                }
            }
            private uint _value;
            private uint _alignment;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint Value { get { return _value; } }
            public uint Alignment { get { return _alignment; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: UNAMESPACE
        /// </remarks>
        public partial class SymUnamespace : KaitaiStruct
        {
            public SymUnamespace(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: COMPILESYM3
        /// </remarks>
        public partial class SymCompile3 : KaitaiStruct
        {
            public static SymCompile3 FromFile(string fileName)
            {
                return new SymCompile3(new KaitaiStream(fileName));
            }

            public SymCompile3(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _flags = new SymCompile3Flags(m_io, this, m_root);
                _machine = m_io.ReadU2le();
                _verFeMajor = m_io.ReadU2le();
                _verFeMinor = m_io.ReadU2le();
                _verFeBuild = m_io.ReadU2le();
                _verFeQfe = m_io.ReadU2le();
                _verMajor = m_io.ReadU2le();
                _verMinor = m_io.ReadU2le();
                _verBuild = m_io.ReadU2le();
                _verQfe = m_io.ReadU2le();
                _versionString = new PdbString(false, m_io, this, m_root);
            }
            private SymCompile3Flags _flags;
            private ushort _machine;
            private ushort _verFeMajor;
            private ushort _verFeMinor;
            private ushort _verFeBuild;
            private ushort _verFeQfe;
            private ushort _verMajor;
            private ushort _verMinor;
            private ushort _verBuild;
            private ushort _verQfe;
            private PdbString _versionString;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public SymCompile3Flags Flags { get { return _flags; } }

            /// <summary>
            /// target processor
            /// </summary>
            /// <remarks>
            /// Reference: machine
            /// </remarks>
            public ushort Machine { get { return _machine; } }

            /// <summary>
            /// front end major version #
            /// </summary>
            /// <remarks>
            /// Reference: verFEMajor
            /// </remarks>
            public ushort VerFeMajor { get { return _verFeMajor; } }

            /// <summary>
            /// front end minor version #
            /// </summary>
            /// <remarks>
            /// Reference: verFEMinor
            /// </remarks>
            public ushort VerFeMinor { get { return _verFeMinor; } }

            /// <summary>
            /// front end build version #
            /// </summary>
            /// <remarks>
            /// Reference: verFEBuild
            /// </remarks>
            public ushort VerFeBuild { get { return _verFeBuild; } }

            /// <summary>
            /// front end QFE version #
            /// </summary>
            /// <remarks>
            /// Reference: verFEQFE
            /// </remarks>
            public ushort VerFeQfe { get { return _verFeQfe; } }

            /// <summary>
            /// back end major version #
            /// </summary>
            /// <remarks>
            /// Reference: verMajor
            /// </remarks>
            public ushort VerMajor { get { return _verMajor; } }

            /// <summary>
            /// back end minor version #
            /// </summary>
            /// <remarks>
            /// Reference: verMinor
            /// </remarks>
            public ushort VerMinor { get { return _verMinor; } }

            /// <summary>
            /// back end build version #
            /// </summary>
            /// <remarks>
            /// Reference: verBuild
            /// </remarks>
            public ushort VerBuild { get { return _verBuild; } }

            /// <summary>
            /// back end QFE version #
            /// </summary>
            /// <remarks>
            /// Reference: verQFE
            /// </remarks>
            public ushort VerQfe { get { return _verQfe; } }

            /// <summary>
            /// Zero terminated compiler version string
            /// </summary>
            /// <remarks>
            /// Reference: verSz
            /// </remarks>
            public PdbString VersionString { get { return _versionString; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class UModuleInfo : KaitaiStruct
        {
            public UModuleInfo(uint p_index, KaitaiStream p__io, MsPdb.ModuleList p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _index = p_index;
                f_symbolsSize = false;
                f_moduleData = false;
                f_isV50 = false;
                f_streamNumber = false;
                f_c13LinesSize = false;
                f_streamData = false;
                f_linesSize = false;
                _read();
            }
            private void _read()
            {
                if (IsV50) {
                    _module50 = new ModuleInfo50(Index, m_io, this, m_root);
                }
                if (IsV50 == false) {
                    _module = new ModuleInfo(Index, m_io, this, m_root);
                }
            }
            private bool f_symbolsSize;
            private uint _symbolsSize;
            public uint SymbolsSize
            {
                get
                {
                    if (f_symbolsSize)
                        return _symbolsSize;
                    _symbolsSize = (uint) ((IsV50 ? Module50.SymbolsSize : Module.SymbolsSize));
                    f_symbolsSize = true;
                    return _symbolsSize;
                }
            }
            private bool f_moduleData;
            private ModuleStream _moduleData;
            public ModuleStream ModuleData
            {
                get
                {
                    if (f_moduleData)
                        return _moduleData;
                    if (StreamNumber > -1) {
                        __raw__raw_moduleData = m_io.ReadBytes(0);
                        Cat _process__raw__raw_moduleData = new Cat(StreamData);
                        __raw_moduleData = _process__raw__raw_moduleData.Decode(__raw__raw_moduleData);
                        var io___raw_moduleData = new KaitaiStream(__raw_moduleData);
                        _moduleData = new ModuleStream(Index, io___raw_moduleData, this, m_root);
                        f_moduleData = true;
                    }
                    return _moduleData;
                }
            }
            private bool f_isV50;
            private bool _isV50;
            public bool IsV50
            {
                get
                {
                    if (f_isV50)
                        return _isV50;
                    _isV50 = (bool) (M_Parent.M_Parent.HeaderNew.Version == MsPdb.DbiHeaderNew.VersionEnum.V50);
                    f_isV50 = true;
                    return _isV50;
                }
            }
            private bool f_streamNumber;
            private short _streamNumber;
            public short StreamNumber
            {
                get
                {
                    if (f_streamNumber)
                        return _streamNumber;
                    _streamNumber = (short) ((IsV50 ? Module50.Stream.StreamNumber : Module.Stream.StreamNumber));
                    f_streamNumber = true;
                    return _streamNumber;
                }
            }
            private bool f_c13LinesSize;
            private uint _c13LinesSize;
            public uint C13LinesSize
            {
                get
                {
                    if (f_c13LinesSize)
                        return _c13LinesSize;
                    _c13LinesSize = (uint) ((IsV50 ? 0 : Module.C13LinesSize));
                    f_c13LinesSize = true;
                    return _c13LinesSize;
                }
            }
            private bool f_streamData;
            private byte[] _streamData;
            public byte[] StreamData
            {
                get
                {
                    if (f_streamData)
                        return _streamData;
                    _streamData = (byte[]) ((IsV50 ? Module50.Stream.Data : Module.Stream.Data));
                    f_streamData = true;
                    return _streamData;
                }
            }
            private bool f_linesSize;
            private uint _linesSize;
            public uint LinesSize
            {
                get
                {
                    if (f_linesSize)
                        return _linesSize;
                    _linesSize = (uint) ((IsV50 ? Module50.LinesSize : Module.LinesSize));
                    f_linesSize = true;
                    return _linesSize;
                }
            }
            private ModuleInfo50 _module50;
            private ModuleInfo _module;
            private uint _index;
            private MsPdb m_root;
            private MsPdb.ModuleList m_parent;
            private byte[] __raw_moduleData;
            private byte[] __raw__raw_moduleData;
            public ModuleInfo50 Module50 { get { return _module50; } }
            public ModuleInfo Module { get { return _module; } }
            public uint Index { get { return _index; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.ModuleList M_Parent { get { return m_parent; } }
            public byte[] M_RawModuleData { get { return __raw_moduleData; } }
            public byte[] M_RawM_RawModuleData { get { return __raw__raw_moduleData; } }
        }

        /// <remarks>
        /// Reference: CV_lvar_attr
        /// </remarks>
        public partial class CvLvarAttr : KaitaiStruct
        {
            public static CvLvarAttr FromFile(string fileName)
            {
                return new CvLvarAttr(new KaitaiStream(fileName));
            }

            public CvLvarAttr(KaitaiStream p__io, MsPdb.SymAttrSlot p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _flags = new CvLocalVarFlags(m_io, this, m_root);
            }
            private uint _offset;
            private ushort _segment;
            private CvLocalVarFlags _flags;
            private MsPdb m_root;
            private MsPdb.SymAttrSlot m_parent;

            /// <summary>
            /// first code address where var is live
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <summary>
            /// local var flags
            /// </summary>
            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public CvLocalVarFlags Flags { get { return _flags; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.SymAttrSlot M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_POINTER_16t
        /// </summary>
        /// <remarks>
        /// Reference: lfPointer_16t
        /// </remarks>
        public partial class LfPointer16t : KaitaiStruct
        {
            public static LfPointer16t FromFile(string fileName)
            {
                return new LfPointer16t(new KaitaiStream(fileName));
            }

            public LfPointer16t(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _attributes = new LfPointerAttributes16t(m_io, this, m_root);
                _underlyingType = new TpiTypeRef16(m_io, this, m_root);
            }
            private LfPointerAttributes16t _attributes;
            private TpiTypeRef16 _underlyingType;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public LfPointerAttributes16t Attributes { get { return _attributes; } }

            /// <summary>
            /// type index of the underlying type
            /// </summary>
            /// <remarks>
            /// Reference: utype
            /// </remarks>
            public TpiTypeRef16 UnderlyingType { get { return _underlyingType; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_ARGLIST, LF_SUBSTR_LIST
        /// </summary>
        /// <remarks>
        /// Reference: lfArgList
        /// </remarks>
        public partial class LfArglist : KaitaiStruct
        {
            public static LfArglist FromFile(string fileName)
            {
                return new LfArglist(new KaitaiStream(fileName));
            }

            public LfArglist(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _count = m_io.ReadU4le();
                _arguments = new List<TpiTypeRef>();
                for (var i = 0; i < Count; i++)
                {
                    _arguments.Add(new TpiTypeRef(m_io, this, m_root));
                }
            }
            private uint _count;
            private List<TpiTypeRef> _arguments;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// number of arguments
            /// </summary>
            /// <remarks>
            /// Reference: count
            /// </remarks>
            public uint Count { get { return _count; } }

            /// <summary>
            /// argument types
            /// </summary>
            /// <remarks>
            /// Reference: arg
            /// </remarks>
            public List<TpiTypeRef> Arguments { get { return _arguments; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: ENVBLOCKSYM.flags
        /// </remarks>
        public partial class SymEnvblockFlags : KaitaiStruct
        {
            public static SymEnvblockFlags FromFile(string fileName)
            {
                return new SymEnvblockFlags(new KaitaiStream(fileName));
            }

            public SymEnvblockFlags(KaitaiStream p__io, MsPdb.SymEnvblock p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _rev = m_io.ReadBitsIntLe(1) != 0;
                _pad = m_io.ReadBitsIntLe(7);
            }
            private bool _rev;
            private ulong _pad;
            private MsPdb m_root;
            private MsPdb.SymEnvblock m_parent;

            /// <summary>
            /// reserved
            /// </summary>
            /// <remarks>
            /// Reference: rev
            /// </remarks>
            public bool Rev { get { return _rev; } }

            /// <summary>
            /// reserved, must be 0
            /// </summary>
            /// <remarks>
            /// Reference: pad
            /// </remarks>
            public ulong Pad { get { return _pad; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.SymEnvblock M_Parent { get { return m_parent; } }
        }
        public partial class PdbPage2 : KaitaiStruct
        {
            public PdbPage2(uint p_pageSize, KaitaiStream p__io, MsPdb.PdbPagelist p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _pageSize = p_pageSize;
                _read();
            }
            private void _read()
            {
                _data = m_io.ReadBytes(PageSize);
            }
            private byte[] _data;
            private uint _pageSize;
            private MsPdb m_root;
            private MsPdb.PdbPagelist m_parent;
            public byte[] Data { get { return _data; } }
            public uint PageSize { get { return _pageSize; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbPagelist M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: FRAMECOOKIE
        /// </remarks>
        public partial class SymFrameCookie : KaitaiStruct
        {
            public static SymFrameCookie FromFile(string fileName)
            {
                return new SymFrameCookie(new KaitaiStream(fileName));
            }

            public SymFrameCookie(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _register = m_io.ReadU2le();
                _cookieType = ((MsPdb.Tpi.CvCookietype) m_io.ReadU1());
                _flags = m_io.ReadU1();
            }
            private uint _offset;
            private ushort _register;
            private Tpi.CvCookietype _cookieType;
            private byte _flags;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Frame relative offset
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// Register index
            /// </summary>
            /// <remarks>
            /// Reference: reg
            /// </remarks>
            public ushort Register { get { return _register; } }

            /// <summary>
            /// Type of the cookie
            /// </summary>
            /// <remarks>
            /// Reference: cookietype
            /// </remarks>
            public Tpi.CvCookietype CookieType { get { return _cookieType; } }

            /// <summary>
            /// Flags describing this cookie
            /// </summary>
            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public byte Flags { get { return _flags; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class SymUnknown : KaitaiStruct
        {
            public static SymUnknown FromFile(string fileName)
            {
                return new SymUnknown(new KaitaiStream(fileName));
            }

            public SymUnknown(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _data = m_io.ReadBytesFull();
            }
            private byte[] _data;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;
            public byte[] Data { get { return _data; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: REGSYM
        /// </remarks>
        public partial class SymRegister32 : KaitaiStruct
        {
            public SymRegister32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _register = m_io.ReadU2le();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private TpiTypeRef _type;
            private ushort _register;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Type index
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// register enumerate
            /// </summary>
            /// <remarks>
            /// Reference: reg
            /// </remarks>
            public ushort Register { get { return _register; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class TpiTypeRef16 : KaitaiStruct
        {
            public static TpiTypeRef16 FromFile(string fileName)
            {
                return new TpiTypeRef16(new KaitaiStream(fileName));
            }

            public TpiTypeRef16(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_arrayIndex = false;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _index = m_io.ReadU2le();
            }
            private bool f_arrayIndex;
            private int _arrayIndex;
            public int ArrayIndex
            {
                get
                {
                    if (f_arrayIndex)
                        return _arrayIndex;
                    _arrayIndex = (int) ((Index - M_Root.MinTypeIndex));
                    f_arrayIndex = true;
                    return _arrayIndex;
                }
            }
            private bool f_type;
            private TpiType _type;
            public TpiType Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    if (ArrayIndex >= 0) {
                        _type = (TpiType) (M_Root.Types[ArrayIndex]);
                    }
                    f_type = true;
                    return _type;
                }
            }
            private ushort _index;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public ushort Index { get { return _index; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class LinkInfoStream : KaitaiStruct
        {
            public static LinkInfoStream FromFile(string fileName)
            {
                return new LinkInfoStream(new KaitaiStream(fileName));
            }

            public LinkInfoStream(KaitaiStream p__io, MsPdb.PdbNamedStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_cwd = false;
                f_command = false;
                f_outFile = false;
                f_libs = false;
                _read();
            }
            private void _read()
            {
                _header = new LinkInfo(m_io, this, m_root);
            }
            private bool f_cwd;
            private string _cwd;
            public string Cwd
            {
                get
                {
                    if (f_cwd)
                        return _cwd;
                    long _pos = m_io.Pos;
                    m_io.Seek(Header.CwdOffset);
                    _cwd = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytesTerm(0, false, true, true));
                    m_io.Seek(_pos);
                    f_cwd = true;
                    return _cwd;
                }
            }
            private bool f_command;
            private string _command;
            public string Command
            {
                get
                {
                    if (f_command)
                        return _command;
                    long _pos = m_io.Pos;
                    m_io.Seek(Header.CommandOffset);
                    _command = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytesTerm(0, false, true, true));
                    m_io.Seek(_pos);
                    f_command = true;
                    return _command;
                }
            }
            private bool f_outFile;
            private string _outFile;
            public string OutFile
            {
                get
                {
                    if (f_outFile)
                        return _outFile;
                    long _pos = m_io.Pos;
                    m_io.Seek((Header.CommandOffset + Header.OutputFileIndex));
                    _outFile = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytesTerm(0, false, true, true));
                    m_io.Seek(_pos);
                    f_outFile = true;
                    return _outFile;
                }
            }
            private bool f_libs;
            private string _libs;
            public string Libs
            {
                get
                {
                    if (f_libs)
                        return _libs;
                    long _pos = m_io.Pos;
                    m_io.Seek(Header.LibsOffset);
                    _libs = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytesTerm(0, false, true, true));
                    m_io.Seek(_pos);
                    f_libs = true;
                    return _libs;
                }
            }
            private LinkInfo _header;
            private MsPdb m_root;
            private MsPdb.PdbNamedStream m_parent;
            public LinkInfo Header { get { return _header; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbNamedStream M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: FUNCTIONLIST
        /// </remarks>
        public partial class SymFunctionList : KaitaiStruct
        {
            public static SymFunctionList FromFile(string fileName)
            {
                return new SymFunctionList(new KaitaiStream(fileName));
            }

            public SymFunctionList(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _count = m_io.ReadU4le();
                _functions = new List<TpiTypeRef>();
                for (var i = 0; i < Count; i++)
                {
                    _functions.Add(new TpiTypeRef(m_io, this, m_root));
                }
                if ((M_Io.Size - M_Io.Pos) >= 4) {
                    _invocations = new List<uint>();
                    {
                        var i = 0;
                        uint M_;
                        do {
                            M_ = m_io.ReadU4le();
                            _invocations.Add(M_);
                            i++;
                        } while (!( ((M_Io.IsEof) || ((M_Io.Size - M_Io.Pos) < 4)) ));
                    }
                }
            }
            private uint _count;
            private List<TpiTypeRef> _functions;
            private List<uint> _invocations;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Number of functions
            /// </summary>
            /// <remarks>
            /// Reference: count
            /// </remarks>
            public uint Count { get { return _count; } }

            /// <summary>
            /// List of functions, dim == count
            /// </summary>
            /// <remarks>
            /// Reference: funcs
            /// </remarks>
            public List<TpiTypeRef> Functions { get { return _functions; } }

            /// <summary>
            /// array of invocation counts
            /// </summary>
            /// <remarks>
            /// Reference: invocations
            /// </remarks>
            public List<uint> Invocations { get { return _invocations; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class PdbBuffer : KaitaiStruct
        {
            public static PdbBuffer FromFile(string fileName)
            {
                return new PdbBuffer(new KaitaiStream(fileName));
            }

            public PdbBuffer(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_dataStart = false;
                _read();
            }
            private void _read()
            {
                _numBytes = m_io.ReadU4le();
                if (DataStart >= 0) {
                    _invokeDataStart = m_io.ReadBytes(0);
                }
                _data = m_io.ReadBytes(NumBytes);
            }
            private bool f_dataStart;
            private int _dataStart;
            public int DataStart
            {
                get
                {
                    if (f_dataStart)
                        return _dataStart;
                    _dataStart = (int) (M_Io.Pos);
                    f_dataStart = true;
                    return _dataStart;
                }
            }
            private uint _numBytes;
            private byte[] _invokeDataStart;
            private byte[] _data;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint NumBytes { get { return _numBytes; } }
            public byte[] InvokeDataStart { get { return _invokeDataStart; } }
            public byte[] Data { get { return _data; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class PdbPageNumber : KaitaiStruct
        {
            public static PdbPageNumber FromFile(string fileName)
            {
                return new PdbPageNumber(new KaitaiStream(fileName));
            }

            public PdbPageNumber(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_pageNumber = false;
                f_page = false;
                _read();
            }
            private void _read()
            {
                switch (M_Root.PageNumberSize) {
                case 2: {
                    _pageNumberData = m_io.ReadU2le();
                    break;
                }
                case 4: {
                    _pageNumberData = m_io.ReadU4le();
                    break;
                }
                }
            }
            private bool f_pageNumber;
            private uint _pageNumber;
            public uint PageNumber
            {
                get
                {
                    if (f_pageNumber)
                        return _pageNumber;
                    _pageNumber = (uint) (((uint) (PageNumberData)));
                    f_pageNumber = true;
                    return _pageNumber;
                }
            }
            private bool f_page;
            private PdbPage _page;
            public PdbPage Page
            {
                get
                {
                    if (f_page)
                        return _page;
                    KaitaiStream io = M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek((M_Root.PageSize * PageNumber));
                    _page = new PdbPage(io, this, m_root);
                    io.Seek(_pos);
                    f_page = true;
                    return _page;
                }
            }
            private uint _pageNumberData;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint PageNumberData { get { return _pageNumberData; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class GetNumPages : KaitaiStruct
        {
            public GetNumPages(uint p_numBytes, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _numBytes = p_numBytes;
                f_numPages = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_numPages;
            private int _numPages;
            public int NumPages
            {
                get
                {
                    if (f_numPages)
                        return _numPages;
                    _numPages = (int) ((((NumBytes + M_Root.PageSize) - 1) / M_Root.PageSize));
                    f_numPages = true;
                    return _numPages;
                }
            }
            private uint _numBytes;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint NumBytes { get { return _numBytes; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: HEAPALLOCSITE
        /// </remarks>
        public partial class SymHeapAllocSite : KaitaiStruct
        {
            public static SymHeapAllocSite FromFile(string fileName)
            {
                return new SymHeapAllocSite(new KaitaiStream(fileName));
            }

            public SymHeapAllocSite(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _false = m_io.ReadU4le();
                _section = m_io.ReadU2le();
                _instructionSize = m_io.ReadU2le();
                _type = new TpiTypeRef(m_io, this, m_root);
            }
            private uint _false;
            private ushort _section;
            private ushort _instructionSize;
            private TpiTypeRef _type;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// offset of call site
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint False { get { return _false; } }

            /// <summary>
            /// section index of call site
            /// </summary>
            /// <remarks>
            /// Reference: sect
            /// </remarks>
            public ushort Section { get { return _section; } }

            /// <summary>
            /// length of heap allocation call instruction
            /// </summary>
            /// <remarks>
            /// Reference: cbInstr
            /// </remarks>
            public ushort InstructionSize { get { return _instructionSize; } }

            /// <summary>
            /// type index describing function signature
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: ANNOTATIONSYM
        /// </remarks>
        public partial class SymAnnotation : KaitaiStruct
        {
            public static SymAnnotation FromFile(string fileName)
            {
                return new SymAnnotation(new KaitaiStream(fileName));
            }

            public SymAnnotation(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _numStrings = m_io.ReadU2le();
                _strings = new List<string>();
                for (var i = 0; i < NumStrings; i++)
                {
                    _strings.Add(System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true)));
                }
            }
            private uint _offset;
            private ushort _segment;
            private ushort _numStrings;
            private List<string> _strings;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <summary>
            /// Count of zero terminated annotation strings
            /// </summary>
            /// <remarks>
            /// Reference: csz
            /// </remarks>
            public ushort NumStrings { get { return _numStrings; } }

            /// <summary>
            /// Sequence of zero terminated annotation strings
            /// </summary>
            /// <remarks>
            /// Reference: rgsz
            /// </remarks>
            public List<string> Strings { get { return _strings; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DEBUG_S_FRAMEDATA
        /// </remarks>
        public partial class C13FrameData : KaitaiStruct
        {
            public static C13FrameData FromFile(string fileName)
            {
                return new C13FrameData(new KaitaiStream(fileName));
            }

            public C13FrameData(KaitaiStream p__io, MsPdb.C13SubsectionFrameData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _rvaStart = m_io.ReadU4le();
                _blockSize = m_io.ReadU4le();
                _localsSize = m_io.ReadU4le();
                _paramsSize = m_io.ReadU4le();
                _maxStack = m_io.ReadU4le();
                _frameFunc = m_io.ReadU4le();
                _prologSize = m_io.ReadU2le();
                _savedRegsSize = m_io.ReadU2le();
                _hasSeh = m_io.ReadBitsIntLe(1) != 0;
                _hasEh = m_io.ReadBitsIntLe(1) != 0;
                _isFunctionStart = m_io.ReadBitsIntLe(1) != 0;
                _reserved = m_io.ReadBitsIntLe(29);
            }
            private uint _rvaStart;
            private uint _blockSize;
            private uint _localsSize;
            private uint _paramsSize;
            private uint _maxStack;
            private uint _frameFunc;
            private ushort _prologSize;
            private ushort _savedRegsSize;
            private bool _hasSeh;
            private bool _hasEh;
            private bool _isFunctionStart;
            private ulong _reserved;
            private MsPdb m_root;
            private MsPdb.C13SubsectionFrameData m_parent;
            public uint RvaStart { get { return _rvaStart; } }
            public uint BlockSize { get { return _blockSize; } }
            public uint LocalsSize { get { return _localsSize; } }
            public uint ParamsSize { get { return _paramsSize; } }
            public uint MaxStack { get { return _maxStack; } }
            public uint FrameFunc { get { return _frameFunc; } }
            public ushort PrologSize { get { return _prologSize; } }
            public ushort SavedRegsSize { get { return _savedRegsSize; } }
            public bool HasSeh { get { return _hasSeh; } }
            public bool HasEh { get { return _hasEh; } }
            public bool IsFunctionStart { get { return _isFunctionStart; } }
            public ulong Reserved { get { return _reserved; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13SubsectionFrameData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: LOCALSYM
        /// </remarks>
        public partial class SymLocal : KaitaiStruct
        {
            public static SymLocal FromFile(string fileName)
            {
                return new SymLocal(new KaitaiStream(fileName));
            }

            public SymLocal(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _flags = new CvLocalVarFlags(m_io, this, m_root);
            }
            private TpiTypeRef _type;
            private CvLocalVarFlags _flags;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// type index
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// local var flags
            /// </summary>
            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public CvLocalVarFlags Flags { get { return _flags; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class PdbPagelist : KaitaiStruct
        {
            public PdbPagelist(uint p_numPages, uint p_pageSize, KaitaiStream p__io, MsPdb.PdbDsRoot p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _numPages = p_numPages;
                _pageSize = p_pageSize;
                _read();
            }
            private void _read()
            {
                __raw_page = new List<byte[]>();
                _page = new List<PdbPage2>();
                for (var i = 0; i < NumPages; i++)
                {
                    __raw_page.Add(m_io.ReadBytes(PageSize));
                    var io___raw_page = new KaitaiStream(__raw_page[__raw_page.Count - 1]);
                    _page.Add(new PdbPage2(PageSize, io___raw_page, this, m_root));
                }
            }
            private List<PdbPage2> _page;
            private uint _numPages;
            private uint _pageSize;
            private MsPdb m_root;
            private MsPdb.PdbDsRoot m_parent;
            private List<byte[]> __raw_page;
            public List<PdbPage2> Page { get { return _page; } }
            public uint NumPages { get { return _numPages; } }
            public uint PageSize { get { return _pageSize; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbDsRoot M_Parent { get { return m_parent; } }
            public List<byte[]> M_RawPage { get { return __raw_page; } }
        }

        /// <remarks>
        /// Reference: MODI
        /// </remarks>
        public partial class ModuleInfo : KaitaiStruct
        {
            public ModuleInfo(uint p_moduleIndex, KaitaiStream p__io, MsPdb.UModuleInfo p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _moduleIndex = p_moduleIndex;
                f_paddingSize = false;
                f_alignment = false;
                f_positionStart = false;
                f_positionEnd = false;
                _read();
            }
            private void _read()
            {
                if (PositionStart >= 0) {
                    _invokePositionStart = m_io.ReadBytes(0);
                }
                _openModuleHandle = m_io.ReadU4le();
                switch (M_Parent.M_Parent.M_Parent.SectionContributionsVersion) {
                case MsPdb.SectionContributionList.VersionType.New: {
                    _sectionContribution = new SectionContrib(m_io, this, m_root);
                    break;
                }
                case MsPdb.SectionContributionList.VersionType.V60: {
                    _sectionContribution = new SectionContrib(m_io, this, m_root);
                    break;
                }
                default: {
                    _sectionContribution = new SectionContrib40(m_io, this, m_root);
                    break;
                }
                }
                _flags = new ModuleInfoFlags(m_io, this, m_root);
                _stream = new PdbStreamRef(m_io, this, m_root);
                _symbolsSize = m_io.ReadU4le();
                _linesSize = m_io.ReadU4le();
                _c13LinesSize = m_io.ReadU4le();
                _numberOfFiles = m_io.ReadU2le();
                _pad0 = m_io.ReadU2le();
                _fileNamesOffsets = m_io.ReadU4le();
                _ecInfo = new EcInfo(m_io, this, m_root);
                _moduleName = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
                _objectFilename = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
                if (PositionEnd >= 0) {
                    _invokePositionEnd = m_io.ReadBytes(0);
                }
                _padding = m_io.ReadBytes(PaddingSize);
            }
            private bool f_paddingSize;
            private int _paddingSize;
            public int PaddingSize
            {
                get
                {
                    if (f_paddingSize)
                        return _paddingSize;
                    _paddingSize = (int) ((Alignment.Aligned - PositionEnd));
                    f_paddingSize = true;
                    return _paddingSize;
                }
            }
            private bool f_alignment;
            private Align _alignment;
            public Align Alignment
            {
                get
                {
                    if (f_alignment)
                        return _alignment;
                    _alignment = new Align(((uint) (PositionEnd)), 4, m_io, this, m_root);
                    f_alignment = true;
                    return _alignment;
                }
            }
            private bool f_positionStart;
            private int _positionStart;
            public int PositionStart
            {
                get
                {
                    if (f_positionStart)
                        return _positionStart;
                    _positionStart = (int) (M_Io.Pos);
                    f_positionStart = true;
                    return _positionStart;
                }
            }
            private bool f_positionEnd;
            private int _positionEnd;
            public int PositionEnd
            {
                get
                {
                    if (f_positionEnd)
                        return _positionEnd;
                    _positionEnd = (int) (M_Io.Pos);
                    f_positionEnd = true;
                    return _positionEnd;
                }
            }
            private byte[] _invokePositionStart;
            private uint _openModuleHandle;
            private KaitaiStruct _sectionContribution;
            private ModuleInfoFlags _flags;
            private PdbStreamRef _stream;
            private uint _symbolsSize;
            private uint _linesSize;
            private uint _c13LinesSize;
            private ushort _numberOfFiles;
            private ushort _pad0;
            private uint _fileNamesOffsets;
            private EcInfo _ecInfo;
            private string _moduleName;
            private string _objectFilename;
            private byte[] _invokePositionEnd;
            private byte[] _padding;
            private uint _moduleIndex;
            private MsPdb m_root;
            private MsPdb.UModuleInfo m_parent;
            public byte[] InvokePositionStart { get { return _invokePositionStart; } }

            /// <summary>
            /// currently open mod
            /// </summary>
            /// <remarks>
            /// Reference: pmod
            /// </remarks>
            public uint OpenModuleHandle { get { return _openModuleHandle; } }

            /// <summary>
            /// this module's first section contribution
            /// </summary>
            /// <remarks>
            /// Reference: sc
            /// </remarks>
            public KaitaiStruct SectionContribution { get { return _sectionContribution; } }
            public ModuleInfoFlags Flags { get { return _flags; } }

            /// <summary>
            /// SN of module debug info (syms, lines, fpo), or snNil
            /// </summary>
            /// <remarks>
            /// Reference: sn
            /// </remarks>
            public PdbStreamRef Stream { get { return _stream; } }

            /// <summary>
            /// size of local symbols debug info in stream sn
            /// </summary>
            /// <remarks>
            /// Reference: cbSyms
            /// </remarks>
            public uint SymbolsSize { get { return _symbolsSize; } }

            /// <summary>
            /// size of line number debug info in stream sn
            /// </summary>
            /// <remarks>
            /// Reference: cbLines
            /// </remarks>
            public uint LinesSize { get { return _linesSize; } }

            /// <summary>
            /// size of C13 style line number info in stream sn
            /// </summary>
            /// <remarks>
            /// Reference: cbC13Lines
            /// </remarks>
            public uint C13LinesSize { get { return _c13LinesSize; } }

            /// <summary>
            /// number of files contributing to this module
            /// </summary>
            /// <remarks>
            /// Reference: ifileMac
            /// </remarks>
            public ushort NumberOfFiles { get { return _numberOfFiles; } }
            public ushort Pad0 { get { return _pad0; } }

            /// <summary>
            /// array [0..ifileMac) of offsets into dbi.bufFilenames
            /// </summary>
            /// <remarks>
            /// Reference: mpifileichFile
            /// </remarks>
            public uint FileNamesOffsets { get { return _fileNamesOffsets; } }

            /// <remarks>
            /// Reference: ecInfo
            /// </remarks>
            public EcInfo EcInfo { get { return _ecInfo; } }

            /// <remarks>
            /// Reference: rgch.szModule
            /// </remarks>
            public string ModuleName { get { return _moduleName; } }

            /// <remarks>
            /// Reference: rgch.szObjFile
            /// </remarks>
            public string ObjectFilename { get { return _objectFilename; } }
            public byte[] InvokePositionEnd { get { return _invokePositionEnd; } }
            public byte[] Padding { get { return _padding; } }
            public uint ModuleIndex { get { return _moduleIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.UModuleInfo M_Parent { get { return m_parent; } }
        }
        public partial class StringSlice : KaitaiStruct
        {
            public StringSlice(uint p_offset, KaitaiStream p__io, MsPdb.PdbNamedStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _offset = p_offset;
                _read();
            }
            private void _read()
            {
                __unnamed0 = m_io.ReadBytes(Offset);
                _value = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytesTerm(0, false, true, true));
            }
            private byte[] __unnamed0;
            private string _value;
            private uint _offset;
            private MsPdb m_root;
            private MsPdb.PdbNamedStream m_parent;
            public byte[] Unnamed_0 { get { return __unnamed0; } }
            public string Value { get { return _value; } }
            public uint Offset { get { return _offset; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbNamedStream M_Parent { get { return m_parent; } }
        }
        public partial class PdbStream : KaitaiStruct
        {
            public static PdbStream FromFile(string fileName)
            {
                return new PdbStream(new KaitaiStream(fileName));
            }

            public PdbStream(KaitaiStream p__io, MsPdb p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_zzzFindVc140 = false;
                f_endOfHdr = false;
                f_isVc110Pdb = false;
                f_extraSignaturesSize = false;
                f_zzzFindMinimalDbgInfo = false;
                f_zzzStreamSize = false;
                f_streamSize = false;
                f_isVc140Pdb = false;
                f_isBetweenVc4Vc140 = false;
                f_extraSignaturesStart = false;
                f_zzzExtraSignaturesSize = false;
                f_zzzFindVc110 = false;
                f_isVc70Pdb = false;
                f_extraSignaturesEnd = false;
                f_hasNullTerminatedStrings = false;
                f_extraSignaturesCount = false;
                f_zzzFindNoTypeMerge = false;
                f_isNoTypeMergePdb = false;
                f_isVc2Pdb = false;
                f_isMinimalDbgInfoPdb = false;
                _read();
            }
            private void _read()
            {
                _header = new PdbStreamHdr(m_io, this, m_root);
                if (IsVc70Pdb) {
                    _headerVc70 = new PdbStreamHdrVc70(m_io, this, m_root);
                }
                _nameTable = new NameTableNi(m_io, this, m_root);
                if (ExtraSignaturesStart >= 0) {
                    _invokeExtraSignaturesStart = m_io.ReadBytes(0);
                }
                if (IsBetweenVc4Vc140) {
                    _extraSignatures = new List<uint>();
                    for (var i = 0; i < ExtraSignaturesCount; i++)
                    {
                        _extraSignatures.Add(m_io.ReadU4le());
                    }
                }
            }
            private bool f_zzzFindVc140;
            private U4Finder _zzzFindVc140;
            public U4Finder ZzzFindVc140
            {
                get
                {
                    if (f_zzzFindVc140)
                        return _zzzFindVc140;
                    if (ExtraSignaturesCount > 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(ExtraSignaturesStart);
                        __raw_zzzFindVc140 = m_io.ReadBytes(ExtraSignaturesSize);
                        var io___raw_zzzFindVc140 = new KaitaiStream(__raw_zzzFindVc140);
                        _zzzFindVc140 = new U4Finder(((uint) (MsPdb.PdbImplementationVersion.Vc140)), io___raw_zzzFindVc140, this, m_root);
                        m_io.Seek(_pos);
                        f_zzzFindVc140 = true;
                    }
                    return _zzzFindVc140;
                }
            }
            private bool f_endOfHdr;
            private int _endOfHdr;
            public int EndOfHdr
            {
                get
                {
                    if (f_endOfHdr)
                        return _endOfHdr;
                    _endOfHdr = (int) (M_Io.Pos);
                    f_endOfHdr = true;
                    return _endOfHdr;
                }
            }
            private bool f_isVc110Pdb;
            private bool _isVc110Pdb;
            public bool IsVc110Pdb
            {
                get
                {
                    if (f_isVc110Pdb)
                        return _isVc110Pdb;
                    _isVc110Pdb = (bool) ((ExtraSignaturesCount > 0 ? ZzzFindVc110.Found : false));
                    f_isVc110Pdb = true;
                    return _isVc110Pdb;
                }
            }
            private bool f_extraSignaturesSize;
            private int _extraSignaturesSize;
            public int ExtraSignaturesSize
            {
                get
                {
                    if (f_extraSignaturesSize)
                        return _extraSignaturesSize;
                    _extraSignaturesSize = (int) (((ZzzExtraSignaturesSize / 4) * 4));
                    f_extraSignaturesSize = true;
                    return _extraSignaturesSize;
                }
            }
            private bool f_zzzFindMinimalDbgInfo;
            private U4Finder _zzzFindMinimalDbgInfo;
            public U4Finder ZzzFindMinimalDbgInfo
            {
                get
                {
                    if (f_zzzFindMinimalDbgInfo)
                        return _zzzFindMinimalDbgInfo;
                    if (ExtraSignaturesCount > 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(ExtraSignaturesStart);
                        __raw_zzzFindMinimalDbgInfo = m_io.ReadBytes(ExtraSignaturesSize);
                        var io___raw_zzzFindMinimalDbgInfo = new KaitaiStream(__raw_zzzFindMinimalDbgInfo);
                        _zzzFindMinimalDbgInfo = new U4Finder(1229867341, io___raw_zzzFindMinimalDbgInfo, this, m_root);
                        m_io.Seek(_pos);
                        f_zzzFindMinimalDbgInfo = true;
                    }
                    return _zzzFindMinimalDbgInfo;
                }
            }
            private bool f_zzzStreamSize;
            private GetStreamSize _zzzStreamSize;
            public GetStreamSize ZzzStreamSize
            {
                get
                {
                    if (f_zzzStreamSize)
                        return _zzzStreamSize;
                    _zzzStreamSize = new GetStreamSize(((uint) (MsPdb.DefaultStream.Pdb)), m_io, this, m_root);
                    f_zzzStreamSize = true;
                    return _zzzStreamSize;
                }
            }
            private bool f_streamSize;
            private int _streamSize;
            public int StreamSize
            {
                get
                {
                    if (f_streamSize)
                        return _streamSize;
                    _streamSize = (int) (ZzzStreamSize.Value);
                    f_streamSize = true;
                    return _streamSize;
                }
            }
            private bool f_isVc140Pdb;
            private bool _isVc140Pdb;
            public bool IsVc140Pdb
            {
                get
                {
                    if (f_isVc140Pdb)
                        return _isVc140Pdb;
                    _isVc140Pdb = (bool) ((ExtraSignaturesCount > 0 ? ZzzFindVc140.Found : false));
                    f_isVc140Pdb = true;
                    return _isVc140Pdb;
                }
            }
            private bool f_isBetweenVc4Vc140;
            private bool _isBetweenVc4Vc140;
            public bool IsBetweenVc4Vc140
            {
                get
                {
                    if (f_isBetweenVc4Vc140)
                        return _isBetweenVc4Vc140;
                    _isBetweenVc4Vc140 = (bool) ( ((Header.ImplementationVersion >= MsPdb.PdbImplementationVersion.Vc4) && (Header.ImplementationVersion <= MsPdb.PdbImplementationVersion.Vc140)) );
                    f_isBetweenVc4Vc140 = true;
                    return _isBetweenVc4Vc140;
                }
            }
            private bool f_extraSignaturesStart;
            private int _extraSignaturesStart;
            public int ExtraSignaturesStart
            {
                get
                {
                    if (f_extraSignaturesStart)
                        return _extraSignaturesStart;
                    _extraSignaturesStart = (int) (M_Io.Pos);
                    f_extraSignaturesStart = true;
                    return _extraSignaturesStart;
                }
            }
            private bool f_zzzExtraSignaturesSize;
            private int _zzzExtraSignaturesSize;
            public int ZzzExtraSignaturesSize
            {
                get
                {
                    if (f_zzzExtraSignaturesSize)
                        return _zzzExtraSignaturesSize;
                    _zzzExtraSignaturesSize = (int) ((M_Io.Size - ExtraSignaturesStart));
                    f_zzzExtraSignaturesSize = true;
                    return _zzzExtraSignaturesSize;
                }
            }
            private bool f_zzzFindVc110;
            private U4Finder _zzzFindVc110;
            public U4Finder ZzzFindVc110
            {
                get
                {
                    if (f_zzzFindVc110)
                        return _zzzFindVc110;
                    if (ExtraSignaturesCount > 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(ExtraSignaturesStart);
                        __raw_zzzFindVc110 = m_io.ReadBytes(ExtraSignaturesSize);
                        var io___raw_zzzFindVc110 = new KaitaiStream(__raw_zzzFindVc110);
                        _zzzFindVc110 = new U4Finder(((uint) (MsPdb.PdbImplementationVersion.Vc110)), io___raw_zzzFindVc110, this, m_root);
                        m_io.Seek(_pos);
                        f_zzzFindVc110 = true;
                    }
                    return _zzzFindVc110;
                }
            }
            private bool f_isVc70Pdb;
            private bool _isVc70Pdb;
            public bool IsVc70Pdb
            {
                get
                {
                    if (f_isVc70Pdb)
                        return _isVc70Pdb;
                    _isVc70Pdb = (bool) (Header.ImplementationVersion > MsPdb.PdbImplementationVersion.Vc70Deprecated);
                    f_isVc70Pdb = true;
                    return _isVc70Pdb;
                }
            }
            private bool f_extraSignaturesEnd;
            private int _extraSignaturesEnd;
            public int ExtraSignaturesEnd
            {
                get
                {
                    if (f_extraSignaturesEnd)
                        return _extraSignaturesEnd;
                    _extraSignaturesEnd = (int) (M_Io.Pos);
                    f_extraSignaturesEnd = true;
                    return _extraSignaturesEnd;
                }
            }
            private bool f_hasNullTerminatedStrings;
            private bool _hasNullTerminatedStrings;

            /// <remarks>
            /// Reference: fIsSZPDB
            /// </remarks>
            public bool HasNullTerminatedStrings
            {
                get
                {
                    if (f_hasNullTerminatedStrings)
                        return _hasNullTerminatedStrings;
                    _hasNullTerminatedStrings = (bool) (Header.ImplementationVersion > MsPdb.PdbImplementationVersion.Vc98);
                    f_hasNullTerminatedStrings = true;
                    return _hasNullTerminatedStrings;
                }
            }
            private bool f_extraSignaturesCount;
            private int _extraSignaturesCount;
            public int ExtraSignaturesCount
            {
                get
                {
                    if (f_extraSignaturesCount)
                        return _extraSignaturesCount;
                    _extraSignaturesCount = (int) ((IsBetweenVc4Vc140 ? (ExtraSignaturesSize / 4) : 0));
                    f_extraSignaturesCount = true;
                    return _extraSignaturesCount;
                }
            }
            private bool f_zzzFindNoTypeMerge;
            private U4Finder _zzzFindNoTypeMerge;
            public U4Finder ZzzFindNoTypeMerge
            {
                get
                {
                    if (f_zzzFindNoTypeMerge)
                        return _zzzFindNoTypeMerge;
                    if (ExtraSignaturesCount > 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(ExtraSignaturesStart);
                        __raw_zzzFindNoTypeMerge = m_io.ReadBytes(ExtraSignaturesSize);
                        var io___raw_zzzFindNoTypeMerge = new KaitaiStream(__raw_zzzFindNoTypeMerge);
                        _zzzFindNoTypeMerge = new U4Finder(1297370958, io___raw_zzzFindNoTypeMerge, this, m_root);
                        m_io.Seek(_pos);
                        f_zzzFindNoTypeMerge = true;
                    }
                    return _zzzFindNoTypeMerge;
                }
            }
            private bool f_isNoTypeMergePdb;
            private bool _isNoTypeMergePdb;
            public bool IsNoTypeMergePdb
            {
                get
                {
                    if (f_isNoTypeMergePdb)
                        return _isNoTypeMergePdb;
                    _isNoTypeMergePdb = (bool) ((ExtraSignaturesCount > 0 ? ZzzFindNoTypeMerge.Found : false));
                    f_isNoTypeMergePdb = true;
                    return _isNoTypeMergePdb;
                }
            }
            private bool f_isVc2Pdb;
            private bool _isVc2Pdb;
            public bool IsVc2Pdb
            {
                get
                {
                    if (f_isVc2Pdb)
                        return _isVc2Pdb;
                    _isVc2Pdb = (bool) (StreamSize == 12);
                    f_isVc2Pdb = true;
                    return _isVc2Pdb;
                }
            }
            private bool f_isMinimalDbgInfoPdb;
            private bool _isMinimalDbgInfoPdb;
            public bool IsMinimalDbgInfoPdb
            {
                get
                {
                    if (f_isMinimalDbgInfoPdb)
                        return _isMinimalDbgInfoPdb;
                    _isMinimalDbgInfoPdb = (bool) ((ExtraSignaturesCount > 0 ? ZzzFindMinimalDbgInfo.Found : false));
                    f_isMinimalDbgInfoPdb = true;
                    return _isMinimalDbgInfoPdb;
                }
            }
            private PdbStreamHdr _header;
            private PdbStreamHdrVc70 _headerVc70;
            private NameTableNi _nameTable;
            private byte[] _invokeExtraSignaturesStart;
            private List<uint> _extraSignatures;
            private MsPdb m_root;
            private MsPdb m_parent;
            private byte[] __raw_zzzFindVc140;
            private byte[] __raw_zzzFindMinimalDbgInfo;
            private byte[] __raw_zzzFindVc110;
            private byte[] __raw_zzzFindNoTypeMerge;
            public PdbStreamHdr Header { get { return _header; } }
            public PdbStreamHdrVc70 HeaderVc70 { get { return _headerVc70; } }
            public NameTableNi NameTable { get { return _nameTable; } }
            public byte[] InvokeExtraSignaturesStart { get { return _invokeExtraSignaturesStart; } }
            public List<uint> ExtraSignatures { get { return _extraSignatures; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb M_Parent { get { return m_parent; } }
            public byte[] M_RawZzzFindVc140 { get { return __raw_zzzFindVc140; } }
            public byte[] M_RawZzzFindMinimalDbgInfo { get { return __raw_zzzFindMinimalDbgInfo; } }
            public byte[] M_RawZzzFindVc110 { get { return __raw_zzzFindVc110; } }
            public byte[] M_RawZzzFindNoTypeMerge { get { return __raw_zzzFindNoTypeMerge; } }
        }
        public partial class FileInfoString : KaitaiStruct
        {
            public static FileInfoString FromFile(string fileName)
            {
                return new FileInfoString(new KaitaiStream(fileName));
            }

            public FileInfoString(KaitaiStream p__io, MsPdb.FileInfo p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_string = false;
                _read();
            }
            private void _read()
            {
                _charsIndex = m_io.ReadU4le();
            }
            private bool f_string;
            private PdbString _string;
            public PdbString String
            {
                get
                {
                    if (f_string)
                        return _string;
                    long _pos = m_io.Pos;
                    m_io.Seek((M_Parent.StringsStart + CharsIndex));
                    _string = new PdbString(M_Root.PdbRootStream.HasNullTerminatedStrings == false, m_io, this, m_root);
                    m_io.Seek(_pos);
                    f_string = true;
                    return _string;
                }
            }
            private uint _charsIndex;
            private MsPdb m_root;
            private MsPdb.FileInfo m_parent;
            public uint CharsIndex { get { return _charsIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.FileInfo M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DEFRANGESYMREGISTER
        /// </remarks>
        public partial class SymDefrangeRegister : KaitaiStruct
        {
            public static SymDefrangeRegister FromFile(string fileName)
            {
                return new SymDefrangeRegister(new KaitaiStream(fileName));
            }

            public SymDefrangeRegister(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _reg = m_io.ReadU2le();
                _attr = new CvRangeAttr(m_io, this, m_root);
                _range = new CvLvarAddrRange(m_io, this, m_root);
                _gaps = new List<CvLvarAddrGap>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _gaps.Add(new CvLvarAddrGap(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private ushort _reg;
            private CvRangeAttr _attr;
            private CvLvarAddrRange _range;
            private List<CvLvarAddrGap> _gaps;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Register to hold the value of the symbol
            /// </summary>
            public ushort Reg { get { return _reg; } }

            /// <summary>
            /// Attribute of the register range.
            /// </summary>
            public CvRangeAttr Attr { get { return _attr; } }

            /// <summary>
            /// Range of addresses where this program is valid
            /// </summary>
            public CvLvarAddrRange Range { get { return _range; } }
            public List<CvLvarAddrGap> Gaps { get { return _gaps; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class DbiSymbolData : KaitaiStruct
        {
            public DbiSymbolData(ushort p_length, KaitaiStream p__io, MsPdb.DbiSymbol p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _length = p_length;
                f_moduleIndex = false;
                _read();
            }
            private void _read()
            {
                _type = ((MsPdb.Dbi.SymbolType) m_io.ReadU2le());
                if (Length > 2) {
                    switch (Type) {
                    case MsPdb.Dbi.SymbolType.SCallers: {
                        _body = new SymFunctionList(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SInlinees: {
                        _body = new SymFunctionList(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SRegrel32: {
                        _body = new SymRegrel32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLmanproc: {
                        _body = new SymManproc(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SPub32: {
                        _body = new SymData32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SProcrefSt: {
                        _body = new SymReference(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SManslotSt: {
                        _body = new SymAttrSlot(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SFramecookie: {
                        _body = new SymFrameCookie(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SDefrangeRegisterRel: {
                        _body = new SymDefrangeRegisterRel(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SObjname: {
                        _body = new SymObjname(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SUnamespaceSt: {
                        _body = new SymUnamespace(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SUdt: {
                        _body = new SymUdt(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SRegrel32St: {
                        _body = new SymRegrel32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.STrampoline: {
                        _body = new SymTrampoline(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLproc32Dpc: {
                        _body = new SymProc32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SEnvblock: {
                        _body = new SymEnvblock(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGthread32: {
                        _body = new SymThread32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SRegisterSt: {
                        _body = new SymRegister32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SBprel32: {
                        _body = new SymBprel32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SArmswitchtable: {
                        _body = new SymArmSwitchTable(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SDefrangeFramepointerRelFullScope: {
                        _body = new SymDefrangeFramepointerRel(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLmanprocSt: {
                        _body = new SymManproc(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SCallees: {
                        _body = new SymFunctionList(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLocal: {
                        _body = new SymLocal(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SDefrangeFramepointerRel: {
                        _body = new SymDefrangeFramepointerRel(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLdataHlsl32: {
                        _body = new SymDataHlsl32(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGmanprocSt: {
                        _body = new SymManproc(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SDatarefSt: {
                        _body = new SymReference(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SInlinesite: {
                        _body = new SymInlineSite(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SDataref: {
                        _body = new SymReference(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SCoboludtSt: {
                        _body = new SymUdt(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGdataHlsl32Ex: {
                        _body = new SymDataHlsl32Ex(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SOem: {
                        _body = new SymOem(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SBuildinfo: {
                        _body = new SymBuildInfo(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SCoffgroup: {
                        _body = new SymCoffGroup(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGthread32St: {
                        _body = new SymThread32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SCompile3: {
                        _body = new SymCompile3(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SDefrangeSubfieldRegister: {
                        _body = new SymDefrangeSubfieldRegister(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLthread32: {
                        _body = new SymThread32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SBlock32: {
                        _body = new SymBlock32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SThunk32: {
                        _body = new SymThunk32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGproc32St: {
                        _body = new SymProc32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLthread32St: {
                        _body = new SymThread32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLprocref: {
                        _body = new SymReference(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SConstant: {
                        _body = new SymConstant(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SSkip: {
                        _body = new SymSkip(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SProcref: {
                        _body = new SymReference(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SCompile: {
                        _body = new SymCompile(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SBprel32St: {
                        _body = new SymBprel32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SWith32: {
                        _body = new SymWith32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SSepcode: {
                        _body = new SymSepcode(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SCompile2St: {
                        _body = new SymCompile2(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SFilestatic: {
                        _body = new SymFileStatic(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SWith32St: {
                        _body = new SymWith32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLdataHlsl32Ex: {
                        _body = new SymDataHlsl32Ex(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGdataHlsl32: {
                        _body = new SymDataHlsl32(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SManslot: {
                        _body = new SymAttrSlot(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLabel32: {
                        _body = new SymLabel32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGdataHlsl: {
                        _body = new SymDataHlsl(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLdata32St: {
                        _body = new SymData32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLproc32St: {
                        _body = new SymProc32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SSection: {
                        _body = new SymSection(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SCoboludt: {
                        _body = new SymUdt(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLproc32: {
                        _body = new SymProc32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGproc32: {
                        _body = new SymProc32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLdataHlsl: {
                        _body = new SymDataHlsl(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SConstantSt: {
                        _body = new SymConstant(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SDefrangeRegister: {
                        _body = new SymDefrangeRegister(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SUdtSt: {
                        _body = new SymUdt(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SHeapallocsite: {
                        _body = new SymHeapAllocSite(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SCallsiteinfo: {
                        _body = new SymCallsiteInfo(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SFrameproc: {
                        _body = new SymFrameProc(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SCompile2: {
                        _body = new SymCompile2(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGdata32St: {
                        _body = new SymData32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SPub32St: {
                        _body = new SymData32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SObjnameSt: {
                        _body = new SymObjname(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SUnamespace: {
                        _body = new SymUnamespace(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SExport: {
                        _body = new SymExport(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SBlock32St: {
                        _body = new SymBlock32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SThunk32St: {
                        _body = new SymThunk32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SAnnotation: {
                        _body = new SymAnnotation(m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGmanproc: {
                        _body = new SymManproc(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLprocrefSt: {
                        _body = new SymReference(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SGdata32: {
                        _body = new SymData32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SRegister: {
                        _body = new SymRegister32(false, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLabel32St: {
                        _body = new SymLabel32(true, m_io, this, m_root);
                        break;
                    }
                    case MsPdb.Dbi.SymbolType.SLdata32: {
                        _body = new SymData32(false, m_io, this, m_root);
                        break;
                    }
                    default: {
                        _body = new SymUnknown(m_io, this, m_root);
                        break;
                    }
                    }
                }
            }
            private bool f_moduleIndex;
            private int _moduleIndex;
            public int ModuleIndex
            {
                get
                {
                    if (f_moduleIndex)
                        return _moduleIndex;
                    _moduleIndex = (int) (M_Parent.ModuleIndex);
                    f_moduleIndex = true;
                    return _moduleIndex;
                }
            }
            private Dbi.SymbolType _type;
            private KaitaiStruct _body;
            private ushort _length;
            private MsPdb m_root;
            private MsPdb.DbiSymbol m_parent;
            public Dbi.SymbolType Type { get { return _type; } }
            public KaitaiStruct Body { get { return _body; } }
            public ushort Length { get { return _length; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbol M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: lfPointerAttr
        /// </remarks>
        public partial class LfPointerAttributes : KaitaiStruct
        {
            public static LfPointerAttributes FromFile(string fileName)
            {
                return new LfPointerAttributes(new KaitaiStream(fileName));
            }

            public LfPointerAttributes(KaitaiStream p__io, MsPdb.LfPointer p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _pointerType = ((MsPdb.Tpi.CvPtrtype) m_io.ReadBitsIntLe(5));
                _pointerMode = ((MsPdb.Tpi.CvPtrmode) m_io.ReadBitsIntLe(3));
                _isFlat32 = m_io.ReadBitsIntLe(1) != 0;
                _isVolatile = m_io.ReadBitsIntLe(1) != 0;
                _isConst = m_io.ReadBitsIntLe(1) != 0;
                _isUnaligned = m_io.ReadBitsIntLe(1) != 0;
                _isRestricted = m_io.ReadBitsIntLe(1) != 0;
                _size = m_io.ReadBitsIntLe(6);
                _isMocom = m_io.ReadBitsIntLe(1) != 0;
                _isLref = m_io.ReadBitsIntLe(1) != 0;
                _isRref = m_io.ReadBitsIntLe(1) != 0;
                __unnamed11 = m_io.ReadBitsIntLe(10);
            }
            private Tpi.CvPtrtype _pointerType;
            private Tpi.CvPtrmode _pointerMode;
            private bool _isFlat32;
            private bool _isVolatile;
            private bool _isConst;
            private bool _isUnaligned;
            private bool _isRestricted;
            private ulong _size;
            private bool _isMocom;
            private bool _isLref;
            private bool _isRref;
            private ulong __unnamed11;
            private MsPdb m_root;
            private MsPdb.LfPointer m_parent;

            /// <summary>
            /// ordinal specifying pointer type (CV_ptrtype_e)
            /// </summary>
            /// <remarks>
            /// Reference: ptrtype
            /// </remarks>
            public Tpi.CvPtrtype PointerType { get { return _pointerType; } }

            /// <summary>
            /// ordinal specifying pointer mode (CV_ptrmode_e)
            /// </summary>
            /// <remarks>
            /// Reference: ptrmode
            /// </remarks>
            public Tpi.CvPtrmode PointerMode { get { return _pointerMode; } }

            /// <summary>
            /// true if 0:32 pointer
            /// </summary>
            /// <remarks>
            /// Reference: isflat32
            /// </remarks>
            public bool IsFlat32 { get { return _isFlat32; } }

            /// <summary>
            /// TRUE if volatile pointer
            /// </summary>
            /// <remarks>
            /// Reference: isvolatile
            /// </remarks>
            public bool IsVolatile { get { return _isVolatile; } }

            /// <summary>
            /// TRUE if const pointer
            /// </summary>
            /// <remarks>
            /// Reference: isconst
            /// </remarks>
            public bool IsConst { get { return _isConst; } }

            /// <summary>
            /// TRUE if unaligned pointer
            /// </summary>
            /// <remarks>
            /// Reference: isunaligned
            /// </remarks>
            public bool IsUnaligned { get { return _isUnaligned; } }

            /// <summary>
            /// TRUE if restricted pointer (allow agressive opts)
            /// </summary>
            /// <remarks>
            /// Reference: isrestrict
            /// </remarks>
            public bool IsRestricted { get { return _isRestricted; } }

            /// <summary>
            /// size of pointer (in bytes)
            /// </summary>
            /// <remarks>
            /// Reference: size
            /// </remarks>
            public ulong Size { get { return _size; } }

            /// <summary>
            /// TRUE if it is a MoCOM pointer (^ or %)
            /// </summary>
            /// <remarks>
            /// Reference: ismocom
            /// </remarks>
            public bool IsMocom { get { return _isMocom; } }

            /// <summary>
            /// TRUE if it is this pointer of member function with &amp; ref-qualifier
            /// </summary>
            /// <remarks>
            /// Reference: islref
            /// </remarks>
            public bool IsLref { get { return _isLref; } }

            /// <summary>
            /// TRUE if it is this pointer of member function with &amp;&amp; ref-qualifier
            /// </summary>
            /// <remarks>
            /// Reference: isrref
            /// </remarks>
            public bool IsRref { get { return _isRref; } }

            /// <summary>
            /// unused
            /// </summary>
            /// <remarks>
            /// Reference: unused
            /// </remarks>
            public ulong Unnamed_11 { get { return __unnamed11; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.LfPointer M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: FRAMEPROCSYM.flags
        /// </remarks>
        public partial class SymFrameProcFlags : KaitaiStruct
        {
            public static SymFrameProcFlags FromFile(string fileName)
            {
                return new SymFrameProcFlags(new KaitaiStream(fileName));
            }

            public SymFrameProcFlags(KaitaiStream p__io, MsPdb.SymFrameProc p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _hasAlloca = m_io.ReadBitsIntLe(1) != 0;
                _hasSetjmp = m_io.ReadBitsIntLe(1) != 0;
                _hasLongjmp = m_io.ReadBitsIntLe(1) != 0;
                _hasInlineAsm = m_io.ReadBitsIntLe(1) != 0;
                _hasEh = m_io.ReadBitsIntLe(1) != 0;
                _inlineSpec = m_io.ReadBitsIntLe(1) != 0;
                _hasSeh = m_io.ReadBitsIntLe(1) != 0;
                _naked = m_io.ReadBitsIntLe(1) != 0;
                _securityChecks = m_io.ReadBitsIntLe(1) != 0;
                _asyncEh = m_io.ReadBitsIntLe(1) != 0;
                _gsNoStackOrdering = m_io.ReadBitsIntLe(1) != 0;
                _wasInlined = m_io.ReadBitsIntLe(1) != 0;
                _gsCheck = m_io.ReadBitsIntLe(1) != 0;
                _safeBuffers = m_io.ReadBitsIntLe(1) != 0;
                _encodedLocalBasePointer = m_io.ReadBitsIntLe(2);
                _encodedParamBasePointer = m_io.ReadBitsIntLe(2);
                _pogoOn = m_io.ReadBitsIntLe(1) != 0;
                _validCounts = m_io.ReadBitsIntLe(1) != 0;
                _optSpeed = m_io.ReadBitsIntLe(1) != 0;
                _guardCf = m_io.ReadBitsIntLe(1) != 0;
                _guardCfw = m_io.ReadBitsIntLe(1) != 0;
                _pad = m_io.ReadBitsIntLe(9);
            }
            private bool _hasAlloca;
            private bool _hasSetjmp;
            private bool _hasLongjmp;
            private bool _hasInlineAsm;
            private bool _hasEh;
            private bool _inlineSpec;
            private bool _hasSeh;
            private bool _naked;
            private bool _securityChecks;
            private bool _asyncEh;
            private bool _gsNoStackOrdering;
            private bool _wasInlined;
            private bool _gsCheck;
            private bool _safeBuffers;
            private ulong _encodedLocalBasePointer;
            private ulong _encodedParamBasePointer;
            private bool _pogoOn;
            private bool _validCounts;
            private bool _optSpeed;
            private bool _guardCf;
            private bool _guardCfw;
            private ulong _pad;
            private MsPdb m_root;
            private MsPdb.SymFrameProc m_parent;

            /// <summary>
            /// function uses _alloca()
            /// </summary>
            /// <remarks>
            /// Reference: fHasAlloca
            /// </remarks>
            public bool HasAlloca { get { return _hasAlloca; } }

            /// <summary>
            /// function uses setjmp()
            /// </summary>
            /// <remarks>
            /// Reference: fHasSetJmp
            /// </remarks>
            public bool HasSetjmp { get { return _hasSetjmp; } }

            /// <summary>
            /// function uses longjmp()
            /// </summary>
            /// <remarks>
            /// Reference: fHasLongJmp
            /// </remarks>
            public bool HasLongjmp { get { return _hasLongjmp; } }

            /// <summary>
            /// function uses inline asm
            /// </summary>
            /// <remarks>
            /// Reference: fHasInlAsm
            /// </remarks>
            public bool HasInlineAsm { get { return _hasInlineAsm; } }

            /// <summary>
            /// function has EH states
            /// </summary>
            /// <remarks>
            /// Reference: fHasEH
            /// </remarks>
            public bool HasEh { get { return _hasEh; } }

            /// <summary>
            /// function was speced as inline
            /// </summary>
            /// <remarks>
            /// Reference: fInlSpec
            /// </remarks>
            public bool InlineSpec { get { return _inlineSpec; } }

            /// <summary>
            /// function has SEH
            /// </summary>
            /// <remarks>
            /// Reference: fHasSEH
            /// </remarks>
            public bool HasSeh { get { return _hasSeh; } }

            /// <summary>
            /// function is __declspec(naked)
            /// </summary>
            /// <remarks>
            /// Reference: fNaked
            /// </remarks>
            public bool Naked { get { return _naked; } }

            /// <summary>
            /// function has buffer security check introduced by /GS.
            /// </summary>
            /// <remarks>
            /// Reference: fSecurityChecks
            /// </remarks>
            public bool SecurityChecks { get { return _securityChecks; } }

            /// <summary>
            /// function compiled with /EHa
            /// </summary>
            /// <remarks>
            /// Reference: fAsyncEH
            /// </remarks>
            public bool AsyncEh { get { return _asyncEh; } }

            /// <summary>
            /// function has /GS buffer checks, but stack ordering couldn't be done
            /// </summary>
            /// <remarks>
            /// Reference: fGSNoStackOrdering
            /// </remarks>
            public bool GsNoStackOrdering { get { return _gsNoStackOrdering; } }

            /// <summary>
            /// function was inlined within another function
            /// </summary>
            /// <remarks>
            /// Reference: fWasInlined
            /// </remarks>
            public bool WasInlined { get { return _wasInlined; } }

            /// <summary>
            /// function is __declspec(strict_gs_check)
            /// </summary>
            /// <remarks>
            /// Reference: fGSCheck
            /// </remarks>
            public bool GsCheck { get { return _gsCheck; } }

            /// <summary>
            /// function is __declspec(safebuffers)
            /// </summary>
            /// <remarks>
            /// Reference: fSafeBuffers
            /// </remarks>
            public bool SafeBuffers { get { return _safeBuffers; } }

            /// <summary>
            /// record function's local pointer explicitly.
            /// </summary>
            /// <remarks>
            /// Reference: encodedLocalBasePointer
            /// </remarks>
            public ulong EncodedLocalBasePointer { get { return _encodedLocalBasePointer; } }

            /// <summary>
            /// record function's parameter pointer explicitly.
            /// </summary>
            /// <remarks>
            /// Reference: encodedParamBasePointer
            /// </remarks>
            public ulong EncodedParamBasePointer { get { return _encodedParamBasePointer; } }

            /// <summary>
            /// function was compiled with PGO/PGU
            /// </summary>
            /// <remarks>
            /// Reference: fPogoOn
            /// </remarks>
            public bool PogoOn { get { return _pogoOn; } }

            /// <summary>
            /// Do we have valid Pogo counts?
            /// </summary>
            /// <remarks>
            /// Reference: fValidCounts
            /// </remarks>
            public bool ValidCounts { get { return _validCounts; } }

            /// <summary>
            /// Did we optimize for speed?
            /// </summary>
            /// <remarks>
            /// Reference: fOptSpeed
            /// </remarks>
            public bool OptSpeed { get { return _optSpeed; } }

            /// <summary>
            /// function contains CFG checks (and no write checks)
            /// </summary>
            /// <remarks>
            /// Reference: fGuardCF
            /// </remarks>
            public bool GuardCf { get { return _guardCf; } }

            /// <summary>
            /// function contains CFW checks and/or instrumentation
            /// </summary>
            /// <remarks>
            /// Reference: fGuardCFW
            /// </remarks>
            public bool GuardCfw { get { return _guardCfw; } }

            /// <summary>
            /// must be zero
            /// </summary>
            /// <remarks>
            /// Reference: pad
            /// </remarks>
            public ulong Pad { get { return _pad; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.SymFrameProc M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: INLINESITESYM
        /// </remarks>
        public partial class SymInlineSite : KaitaiStruct
        {
            public static SymInlineSite FromFile(string fileName)
            {
                return new SymInlineSite(new KaitaiStream(fileName));
            }

            public SymInlineSite(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _parent = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _end = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _inlinee = m_io.ReadU4le();
                _binaryAnnotations = m_io.ReadBytesFull();
            }
            private DbiSymbolRef _parent;
            private DbiSymbolRef _end;
            private uint _inlinee;
            private byte[] _binaryAnnotations;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// pointer to the inliner
            /// </summary>
            /// <remarks>
            /// Reference: pParent
            /// </remarks>
            public DbiSymbolRef Parent { get { return _parent; } }

            /// <summary>
            /// pointer to this blocks end
            /// </summary>
            /// <remarks>
            /// Reference: pEnd
            /// </remarks>
            public DbiSymbolRef End { get { return _end; } }

            /// <summary>
            /// CV_ItemId of inlinee
            /// </summary>
            /// <remarks>
            /// Reference: inlinee
            /// </remarks>
            public uint Inlinee { get { return _inlinee; } }

            /// <summary>
            /// an array of compressed binary annotations.
            /// </summary>
            /// <remarks>
            /// Reference: binaryAnnotations
            /// </remarks>
            public byte[] BinaryAnnotations { get { return _binaryAnnotations; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// OMFSegMap - This table contains the mapping between the logical segment indices
        /// used in the symbol table and the physical segments where the program is loaded
        /// </summary>
        /// <remarks>
        /// Reference: OMFSegMapDesc
        /// </remarks>
        public partial class OmfSegmentMapDescriptor : KaitaiStruct
        {
            public static OmfSegmentMapDescriptor FromFile(string fileName)
            {
                return new OmfSegmentMapDescriptor(new KaitaiStream(fileName));
            }

            public OmfSegmentMapDescriptor(KaitaiStream p__io, MsPdb.OmfSegmentMap p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _flags = m_io.ReadU2le();
                _overlayNumber = m_io.ReadU2le();
                _groupIndex = m_io.ReadU2le();
                _segmentIndex = m_io.ReadU2le();
                _segmentNameIndex = m_io.ReadU2le();
                _classNameIndex = m_io.ReadU2le();
                _offset = m_io.ReadU4le();
                _size = m_io.ReadU4le();
            }
            private ushort _flags;
            private ushort _overlayNumber;
            private ushort _groupIndex;
            private ushort _segmentIndex;
            private ushort _segmentNameIndex;
            private ushort _classNameIndex;
            private uint _offset;
            private uint _size;
            private MsPdb m_root;
            private MsPdb.OmfSegmentMap m_parent;

            /// <summary>
            /// descriptor flags bit field.
            /// </summary>
            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public ushort Flags { get { return _flags; } }

            /// <summary>
            /// the logical overlay number
            /// </summary>
            /// <remarks>
            /// Reference: ovl
            /// </remarks>
            public ushort OverlayNumber { get { return _overlayNumber; } }

            /// <summary>
            /// group index into the descriptor array
            /// </summary>
            /// <remarks>
            /// Reference: group
            /// </remarks>
            public ushort GroupIndex { get { return _groupIndex; } }

            /// <summary>
            /// logical segment index - interpreted via flags
            /// </summary>
            /// <remarks>
            /// Reference: frame
            /// </remarks>
            public ushort SegmentIndex { get { return _segmentIndex; } }

            /// <summary>
            /// segment or group name - index into sstSegName
            /// </summary>
            /// <remarks>
            /// Reference: iSegName
            /// </remarks>
            public ushort SegmentNameIndex { get { return _segmentNameIndex; } }

            /// <summary>
            /// class name - index into sstSegName
            /// </summary>
            /// <remarks>
            /// Reference: iClassName
            /// </remarks>
            public ushort ClassNameIndex { get { return _classNameIndex; } }

            /// <summary>
            /// byte offset of the logical within the physical segment
            /// </summary>
            /// <remarks>
            /// Reference: offset
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// byte count of the logical segment or group
            /// </summary>
            /// <remarks>
            /// Reference: cbSeg
            /// </remarks>
            public uint Size { get { return _size; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.OmfSegmentMap M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// old C8.0 types-only program database header:
        /// </summary>
        /// <remarks>
        /// Reference: OHDR
        /// </remarks>
        public partial class PdbHeaderJgOld : KaitaiStruct
        {
            public static PdbHeaderJgOld FromFile(string fileName)
            {
                return new PdbHeaderJgOld(new KaitaiStream(fileName));
            }

            public PdbHeaderJgOld(KaitaiStream p__io, MsPdb.PdbJgOldRoot p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                __unnamed0 = m_io.ReadBytes(2);
                _pdbInternalVersion = ((MsPdb.PdbVersion) m_io.ReadU4le());
                _timestamp = m_io.ReadU4le();
                _age = m_io.ReadU4le();
                _minTi = m_io.ReadU2le();
                _maxTi = m_io.ReadU2le();
                _gpRecSize = m_io.ReadU4le();
            }
            private byte[] __unnamed0;
            private PdbVersion _pdbInternalVersion;
            private uint _timestamp;
            private uint _age;
            private ushort _minTi;
            private ushort _maxTi;
            private uint _gpRecSize;
            private MsPdb m_root;
            private MsPdb.PdbJgOldRoot m_parent;
            public byte[] Unnamed_0 { get { return __unnamed0; } }

            /// <summary>
            /// version which created this file
            /// </summary>
            /// <remarks>
            /// Reference: vers
            /// </remarks>
            public PdbVersion PdbInternalVersion { get { return _pdbInternalVersion; } }

            /// <summary>
            /// signature
            /// </summary>
            /// <remarks>
            /// Reference: sig
            /// </remarks>
            public uint Timestamp { get { return _timestamp; } }

            /// <summary>
            /// age (no. of times written)
            /// </summary>
            /// <remarks>
            /// Reference: age
            /// </remarks>
            public uint Age { get { return _age; } }

            /// <summary>
            /// lowest TI
            /// </summary>
            /// <remarks>
            /// Reference: tiMin
            /// </remarks>
            public ushort MinTi { get { return _minTi; } }

            /// <summary>
            /// highest TI + 1
            /// </summary>
            /// <remarks>
            /// Reference: tiMac
            /// </remarks>
            public ushort MaxTi { get { return _maxTi; } }

            /// <summary>
            /// count of bytes used by the gprec which follows.
            /// </summary>
            /// <remarks>
            /// Reference: cb
            /// </remarks>
            public uint GpRecSize { get { return _gpRecSize; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbJgOldRoot M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: THUNKSYM32
        /// </remarks>
        public partial class SymThunk32 : KaitaiStruct
        {
            public SymThunk32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _parent = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _end = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _next = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _length = m_io.ReadU2le();
                _ordinal = m_io.ReadU1();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
                _variant = m_io.ReadBytesFull();
            }
            private DbiSymbolRef _parent;
            private DbiSymbolRef _end;
            private DbiSymbolRef _next;
            private uint _offset;
            private ushort _segment;
            private ushort _length;
            private byte _ordinal;
            private PdbString _name;
            private byte[] _variant;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// pointer to the parent
            /// </summary>
            /// <remarks>
            /// Reference: pParent
            /// </remarks>
            public DbiSymbolRef Parent { get { return _parent; } }

            /// <summary>
            /// pointer to this blocks end
            /// </summary>
            /// <remarks>
            /// Reference: pEnd
            /// </remarks>
            public DbiSymbolRef End { get { return _end; } }

            /// <summary>
            /// pointer to next symbol
            /// </summary>
            /// <remarks>
            /// Reference: pNext
            /// </remarks>
            public DbiSymbolRef Next { get { return _next; } }

            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <summary>
            /// length of thunk
            /// </summary>
            /// <remarks>
            /// Reference: len
            /// </remarks>
            public ushort Length { get { return _length; } }

            /// <summary>
            /// ordinal specifying type of thunk
            /// </summary>
            /// <remarks>
            /// Reference: ord
            /// </remarks>
            public byte Ordinal { get { return _ordinal; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }

            /// <summary>
            /// variant portion of thunk
            /// </summary>
            /// <remarks>
            /// Reference: variant
            /// </remarks>
            public byte[] Variant { get { return _variant; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CALLSITEINFO
        /// </remarks>
        public partial class SymCallsiteInfo : KaitaiStruct
        {
            public static SymCallsiteInfo FromFile(string fileName)
            {
                return new SymCallsiteInfo(new KaitaiStream(fileName));
            }

            public SymCallsiteInfo(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _section = m_io.ReadU2le();
                __unnamed2 = m_io.ReadBytes(2);
                _type = new TpiTypeRef(m_io, this, m_root);
            }
            private uint _offset;
            private ushort _section;
            private byte[] __unnamed2;
            private TpiTypeRef _type;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// offset of call site
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// section index of call site
            /// </summary>
            /// <remarks>
            /// Reference: sect
            /// </remarks>
            public ushort Section { get { return _section; } }

            /// <summary>
            /// alignment padding field, must be zero
            /// </summary>
            /// <remarks>
            /// Reference: __reserved_0
            /// </remarks>
            public byte[] Unnamed_2 { get { return __unnamed2; } }

            /// <summary>
            /// type index describing function signature
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class PdbBitset : KaitaiStruct
        {
            public static PdbBitset FromFile(string fileName)
            {
                return new PdbBitset(new KaitaiStream(fileName));
            }

            public PdbBitset(KaitaiStream p__io, MsPdb.PdbMap p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_values = false;
                _read();
            }
            private void _read()
            {
                _words = new PdbArray(4, m_io, this, m_root);
            }
            private bool f_values;
            private PdbBitsetWord _values;
            public PdbBitsetWord Values
            {
                get
                {
                    if (f_values)
                        return _values;
                    __raw__raw_values = m_io.ReadBytes(0);
                    Cat _process__raw__raw_values = new Cat(Words.Data);
                    __raw_values = _process__raw__raw_values.Decode(__raw__raw_values);
                    var io___raw_values = new KaitaiStream(__raw_values);
                    _values = new PdbBitsetWord(io___raw_values, this, m_root);
                    f_values = true;
                    return _values;
                }
            }
            private PdbArray _words;
            private MsPdb m_root;
            private MsPdb.PdbMap m_parent;
            private byte[] __raw_values;
            private byte[] __raw__raw_values;
            public PdbArray Words { get { return _words; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbMap M_Parent { get { return m_parent; } }
            public byte[] M_RawValues { get { return __raw_values; } }
            public byte[] M_RawM_RawValues { get { return __raw__raw_values; } }
        }

        /// <remarks>
        /// Reference: NMTNI
        /// </remarks>
        public partial class NameTableNi : KaitaiStruct
        {
            public static NameTableNi FromFile(string fileName)
            {
                return new NameTableNi(new KaitaiStream(fileName));
            }

            public NameTableNi(KaitaiStream p__io, MsPdb.PdbStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _zzzStringTableData = new PdbBuffer(m_io, this, m_root);
                _mapOffsetIndex = new PdbMapNamedStreams(m_io, this, m_root);
                _maxIndex = m_io.ReadU4le();
            }
            private PdbBuffer _zzzStringTableData;
            private PdbMapNamedStreams _mapOffsetIndex;
            private uint _maxIndex;
            private MsPdb m_root;
            private MsPdb.PdbStream m_parent;

            /// <remarks>
            /// Reference: pbuf
            /// </remarks>
            public PdbBuffer ZzzStringTableData { get { return _zzzStringTableData; } }

            /// <remarks>
            /// Reference: mapSzoNi
            /// </remarks>
            public PdbMapNamedStreams MapOffsetIndex { get { return _mapOffsetIndex; } }

            /// <remarks>
            /// Reference: niMac
            /// </remarks>
            public uint MaxIndex { get { return _maxIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbStream M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_MFUNCTION_16t
        /// </summary>
        /// <remarks>
        /// Reference: lfMFunc_16t
        /// </remarks>
        public partial class LfMfunction16t : KaitaiStruct
        {
            public static LfMfunction16t FromFile(string fileName)
            {
                return new LfMfunction16t(new KaitaiStream(fileName));
            }

            public LfMfunction16t(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _returnType = new TpiTypeRef16(m_io, this, m_root);
                _classType = new TpiTypeRef16(m_io, this, m_root);
                _thisType = new TpiTypeRef16(m_io, this, m_root);
                _callingConvention = ((MsPdb.Tpi.CallingConvention) m_io.ReadU1());
                _attributes = new CvFuncAttributes(m_io, this, m_root);
                _parametersCount = m_io.ReadU2le();
                _argumentListType = new TpiTypeRef16(m_io, this, m_root);
                _thisAdjuster = m_io.ReadU4le();
            }
            private TpiTypeRef16 _returnType;
            private TpiTypeRef16 _classType;
            private TpiTypeRef16 _thisType;
            private Tpi.CallingConvention _callingConvention;
            private CvFuncAttributes _attributes;
            private ushort _parametersCount;
            private TpiTypeRef16 _argumentListType;
            private uint _thisAdjuster;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// type index of return value
            /// </summary>
            /// <remarks>
            /// Reference: rvtype
            /// </remarks>
            public TpiTypeRef16 ReturnType { get { return _returnType; } }

            /// <summary>
            /// type index of containing class
            /// </summary>
            /// <remarks>
            /// Reference: classtype
            /// </remarks>
            public TpiTypeRef16 ClassType { get { return _classType; } }

            /// <summary>
            /// type index of this pointer (model specific)
            /// </summary>
            /// <remarks>
            /// Reference: thistype
            /// </remarks>
            public TpiTypeRef16 ThisType { get { return _thisType; } }

            /// <summary>
            /// calling convention (call_t)
            /// </summary>
            /// <remarks>
            /// Reference: calltype
            /// </remarks>
            public Tpi.CallingConvention CallingConvention { get { return _callingConvention; } }

            /// <summary>
            /// attributes
            /// </summary>
            /// <remarks>
            /// Reference: funcattr
            /// </remarks>
            public CvFuncAttributes Attributes { get { return _attributes; } }

            /// <summary>
            /// number of parameters
            /// </summary>
            /// <remarks>
            /// Reference: parmcount
            /// </remarks>
            public ushort ParametersCount { get { return _parametersCount; } }

            /// <summary>
            /// type index of argument list
            /// </summary>
            /// <remarks>
            /// Reference: arglist
            /// </remarks>
            public TpiTypeRef16 ArgumentListType { get { return _argumentListType; } }

            /// <summary>
            /// this adjuster (long because pad required anyway)
            /// </summary>
            /// <remarks>
            /// Reference: thisadjust
            /// </remarks>
            public uint ThisAdjuster { get { return _thisAdjuster; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class GetStreamData : KaitaiStruct
        {
            public GetStreamData(uint p_streamNumber, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _streamNumber = p_streamNumber;
                f_hasData = false;
                f_value = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_hasData;
            private bool _hasData;
            public bool HasData
            {
                get
                {
                    if (f_hasData)
                        return _hasData;
                    _hasData = (bool) ((M_Root.PdbType == MsPdb.PdbTypeEnum.Big ? M_Root.PdbDs.StreamTable.Streams[((int) (StreamNumber))].HasData : M_Root.PdbJg.StreamTable.Streams[((int) (StreamNumber))].HasData));
                    f_hasData = true;
                    return _hasData;
                }
            }
            private bool f_value;
            private byte[] _value;
            public byte[] Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    _value = (byte[]) ((M_Root.PdbType == MsPdb.PdbTypeEnum.Big ? M_Root.PdbDs.StreamTable.Streams[((int) (StreamNumber))].Data : M_Root.PdbJg.StreamTable.Streams[((int) (StreamNumber))].Data));
                    f_value = true;
                    return _value;
                }
            }
            private uint _streamNumber;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint StreamNumber { get { return _streamNumber; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TpiTypes : KaitaiStruct
        {
            public static TpiTypes FromFile(string fileName)
            {
                return new TpiTypes(new KaitaiStream(fileName));
            }

            public TpiTypes(KaitaiStream p__io, MsPdb.Tpi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_types = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_types;
            private List<TpiType> _types;
            public List<TpiType> Types
            {
                get
                {
                    if (f_types)
                        return _types;
                    long _pos = m_io.Pos;
                    m_io.Seek(0);
                    _types = new List<TpiType>();
                    {
                        var i = 0;
                        while (!m_io.IsEof) {
                            _types.Add(new TpiType(((uint) ((M_Root.MinTypeIndex + i))), m_io, this, m_root));
                            i++;
                        }
                    }
                    m_io.Seek(_pos);
                    f_types = true;
                    return _types;
                }
            }
            private MsPdb m_root;
            private MsPdb.Tpi m_parent;
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Tpi M_Parent { get { return m_parent; } }
        }
        public partial class SymbolRecordsStream : KaitaiStruct
        {
            public static SymbolRecordsStream FromFile(string fileName)
            {
                return new SymbolRecordsStream(new KaitaiStream(fileName));
            }

            public SymbolRecordsStream(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _symbols = new List<DbiSymbol>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _symbols.Add(new DbiSymbol(-1, m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<DbiSymbol> _symbols;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public List<DbiSymbol> Symbols { get { return _symbols; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class CvNumericType : KaitaiStruct
        {
            public static CvNumericType FromFile(string fileName)
            {
                return new CvNumericType(new KaitaiStream(fileName));
            }

            public CvNumericType(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = ((MsPdb.Tpi.LeafType) m_io.ReadU2le());
                switch (Type) {
                case MsPdb.Tpi.LeafType.LfChar: {
                    _value = m_io.ReadS1();
                    break;
                }
                case MsPdb.Tpi.LeafType.LfQuadword: {
                    _value = m_io.ReadS8le();
                    break;
                }
                case MsPdb.Tpi.LeafType.LfShort: {
                    _value = m_io.ReadS2le();
                    break;
                }
                case MsPdb.Tpi.LeafType.LfUshort: {
                    _value = m_io.ReadU2le();
                    break;
                }
                case MsPdb.Tpi.LeafType.LfUlong: {
                    _value = m_io.ReadU4le();
                    break;
                }
                case MsPdb.Tpi.LeafType.LfUquadword: {
                    _value = m_io.ReadU8le();
                    break;
                }
                case MsPdb.Tpi.LeafType.LfLong: {
                    _value = m_io.ReadS4le();
                    break;
                }
                default: {
                    _value = new CvNumericLiteral(((ushort) (Type)), m_io, this, m_root);
                    break;
                }
                }
            }
            private Tpi.LeafType _type;
            private object _value;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public Tpi.LeafType Type { get { return _type; } }
            public object Value { get { return _value; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CONSTSYM
        /// </remarks>
        public partial class SymConstant : KaitaiStruct
        {
            public SymConstant(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _value = new CvNumericType(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private TpiTypeRef _type;
            private CvNumericType _value;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Type index (containing enum if enumerate) or metadata token
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// numeric leaf containing value
            /// </summary>
            /// <remarks>
            /// Reference: value
            /// </remarks>
            public CvNumericType Value { get { return _value; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: WITHSYM32
        /// </remarks>
        public partial class SymWith32 : KaitaiStruct
        {
            public SymWith32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _parent = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _end = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _length = m_io.ReadU4le();
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private DbiSymbolRef _parent;
            private DbiSymbolRef _end;
            private uint _length;
            private uint _offset;
            private ushort _segment;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// pointer to the parent
            /// </summary>
            /// <remarks>
            /// Reference: pParent
            /// </remarks>
            public DbiSymbolRef Parent { get { return _parent; } }

            /// <summary>
            /// pointer to this blocks end
            /// </summary>
            /// <remarks>
            /// Reference: pEnd
            /// </remarks>
            public DbiSymbolRef End { get { return _end; } }

            /// <summary>
            /// Block length
            /// </summary>
            /// <remarks>
            /// Reference: len
            /// </remarks>
            public uint Length { get { return _length; } }

            /// <summary>
            /// Offset in code segment
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// segment of label
            /// </summary>
            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <summary>
            /// Length-prefixed expression string
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: mlMethod
        /// </remarks>
        public partial class MlMethod : KaitaiStruct
        {
            public static MlMethod FromFile(string fileName)
            {
                return new MlMethod(new KaitaiStream(fileName));
            }

            public MlMethod(KaitaiStream p__io, MsPdb.LfMethodlist p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _attributes = new CvFieldAttributes(m_io, this, m_root);
                __unnamed1 = m_io.ReadBytes(2);
                _indexType = new TpiTypeRef(m_io, this, m_root);
                if ( ((Attributes.MethodProperties == MsPdb.Tpi.CvMethodprop.Intro) || (Attributes.MethodProperties == MsPdb.Tpi.CvMethodprop.PureIntro)) ) {
                    _vtableOffset = m_io.ReadU4le();
                }
            }
            private CvFieldAttributes _attributes;
            private byte[] __unnamed1;
            private TpiTypeRef _indexType;
            private uint? _vtableOffset;
            private MsPdb m_root;
            private MsPdb.LfMethodlist m_parent;

            /// <summary>
            /// method attribute
            /// </summary>
            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public CvFieldAttributes Attributes { get { return _attributes; } }

            /// <summary>
            /// internal padding, must be 0
            /// </summary>
            /// <remarks>
            /// Reference: pad0
            /// </remarks>
            public byte[] Unnamed_1 { get { return __unnamed1; } }

            /// <summary>
            /// index to type record for procedure
            /// </summary>
            /// <remarks>
            /// Reference: index
            /// </remarks>
            public TpiTypeRef IndexType { get { return _indexType; } }

            /// <summary>
            /// offset in vfunctable if intro virtual
            /// </summary>
            /// <remarks>
            /// Reference: vbaseoff
            /// </remarks>
            public uint? VtableOffset { get { return _vtableOffset; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.LfMethodlist M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CV_PROCFLAGS
        /// </remarks>
        public partial class CvProcFlags : KaitaiStruct
        {
            public static CvProcFlags FromFile(string fileName)
            {
                return new CvProcFlags(new KaitaiStream(fileName));
            }

            public CvProcFlags(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _nofpo = m_io.ReadBitsIntLe(1) != 0;
                _interrupt = m_io.ReadBitsIntLe(1) != 0;
                _farReturn = m_io.ReadBitsIntLe(1) != 0;
                _never = m_io.ReadBitsIntLe(1) != 0;
                _notReached = m_io.ReadBitsIntLe(1) != 0;
                _custCall = m_io.ReadBitsIntLe(1) != 0;
                _noInline = m_io.ReadBitsIntLe(1) != 0;
                _optDebugInfo = m_io.ReadBitsIntLe(1) != 0;
            }
            private bool _nofpo;
            private bool _interrupt;
            private bool _farReturn;
            private bool _never;
            private bool _notReached;
            private bool _custCall;
            private bool _noInline;
            private bool _optDebugInfo;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// frame pointer present
            /// </summary>
            /// <remarks>
            /// Reference: CV_PFLAG_NOFPO
            /// </remarks>
            public bool Nofpo { get { return _nofpo; } }

            /// <summary>
            /// interrupt return
            /// </summary>
            /// <remarks>
            /// Reference: CV_PFLAG_INT
            /// </remarks>
            public bool Interrupt { get { return _interrupt; } }

            /// <summary>
            /// far return
            /// </summary>
            /// <remarks>
            /// Reference: CV_PFLAG_FAR
            /// </remarks>
            public bool FarReturn { get { return _farReturn; } }

            /// <summary>
            /// function does not return
            /// </summary>
            /// <remarks>
            /// Reference: CV_PFLAG_NEVER
            /// </remarks>
            public bool Never { get { return _never; } }

            /// <summary>
            /// label isn't fallen into
            /// </summary>
            /// <remarks>
            /// Reference: CV_PFLAG_NOTREACHED
            /// </remarks>
            public bool NotReached { get { return _notReached; } }

            /// <summary>
            /// custom calling convention
            /// </summary>
            /// <remarks>
            /// Reference: CV_PFLAG_CUST_CALL
            /// </remarks>
            public bool CustCall { get { return _custCall; } }

            /// <summary>
            /// function marked as noinline
            /// </summary>
            /// <remarks>
            /// Reference: CV_PFLAG_NOINLINE
            /// </remarks>
            public bool NoInline { get { return _noInline; } }

            /// <summary>
            /// function has debug information for optimized code
            /// </summary>
            /// <remarks>
            /// Reference: CV_PFLAG_OPTDBGINFO
            /// </remarks>
            public bool OptDebugInfo { get { return _optDebugInfo; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: BLOCKSYM32
        /// </remarks>
        public partial class SymBlock32 : KaitaiStruct
        {
            public SymBlock32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _parent = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _end = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _length = m_io.ReadU4le();
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private DbiSymbolRef _parent;
            private DbiSymbolRef _end;
            private uint _length;
            private uint _offset;
            private ushort _segment;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// pointer to the parent
            /// </summary>
            /// <remarks>
            /// Reference: pParent
            /// </remarks>
            public DbiSymbolRef Parent { get { return _parent; } }

            /// <summary>
            /// pointer to this blocks end
            /// </summary>
            /// <remarks>
            /// Reference: pEnd
            /// </remarks>
            public DbiSymbolRef End { get { return _end; } }

            /// <summary>
            /// Block length
            /// </summary>
            /// <remarks>
            /// Reference: len
            /// </remarks>
            public uint Length { get { return _length; } }

            /// <summary>
            /// Offset in code segment
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// segment of label
            /// </summary>
            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SI_PERSIST
        /// </remarks>
        public partial class PdbStreamEntryJg : KaitaiStruct
        {
            public PdbStreamEntryJg(uint p_streamNumber, KaitaiStream p__io, MsPdb.PdbStreamTable p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _streamNumber = p_streamNumber;
                f_zzzNumDirectoryPages = false;
                f_numDirectoryPages = false;
                _read();
            }
            private void _read()
            {
                _streamSize = m_io.ReadU4le();
                _mapSpnPn = m_io.ReadU4le();
            }
            private bool f_zzzNumDirectoryPages;
            private GetNumPages _zzzNumDirectoryPages;
            public GetNumPages ZzzNumDirectoryPages
            {
                get
                {
                    if (f_zzzNumDirectoryPages)
                        return _zzzNumDirectoryPages;
                    _zzzNumDirectoryPages = new GetNumPages(((uint) (StreamSize)), m_io, this, m_root);
                    f_zzzNumDirectoryPages = true;
                    return _zzzNumDirectoryPages;
                }
            }
            private bool f_numDirectoryPages;
            private int _numDirectoryPages;
            public int NumDirectoryPages
            {
                get
                {
                    if (f_numDirectoryPages)
                        return _numDirectoryPages;
                    _numDirectoryPages = (int) (ZzzNumDirectoryPages.NumPages);
                    f_numDirectoryPages = true;
                    return _numDirectoryPages;
                }
            }
            private uint _streamSize;
            private uint _mapSpnPn;
            private uint _streamNumber;
            private MsPdb m_root;
            private MsPdb.PdbStreamTable m_parent;
            public uint StreamSize { get { return _streamSize; } }
            public uint MapSpnPn { get { return _mapSpnPn; } }
            public uint StreamNumber { get { return _streamNumber; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbStreamTable M_Parent { get { return m_parent; } }
        }
        public partial class LfVftableNames : KaitaiStruct
        {
            public static LfVftableNames FromFile(string fileName)
            {
                return new LfVftableNames(new KaitaiStream(fileName));
            }

            public LfVftableNames(KaitaiStream p__io, MsPdb.LfVftable p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _names = new List<string>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _names.Add(System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true)));
                        i++;
                    }
                }
            }
            private List<string> _names;
            private MsPdb m_root;
            private MsPdb.LfVftable m_parent;
            public List<string> Names { get { return _names; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.LfVftable M_Parent { get { return m_parent; } }
        }
        public partial class NameTableString : KaitaiStruct
        {
            public static NameTableString FromFile(string fileName)
            {
                return new NameTableString(new KaitaiStream(fileName));
            }

            public NameTableString(KaitaiStream p__io, MsPdb.NameTableStrings p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_string = false;
                _read();
            }
            private void _read()
            {
                _charsIndex = m_io.ReadU4le();
            }
            private bool f_string;
            private string _string;
            public string String
            {
                get
                {
                    if (f_string)
                        return _string;
                    KaitaiStream io = M_Parent.M_Parent.M_Io;
                    long _pos = io.Pos;
                    io.Seek((M_Parent.M_Parent.Buffer.DataStart + CharsIndex));
                    _string = System.Text.Encoding.GetEncoding("UTF-8").GetString(io.ReadBytesTerm(0, false, true, true));
                    io.Seek(_pos);
                    f_string = true;
                    return _string;
                }
            }
            private uint _charsIndex;
            private MsPdb m_root;
            private MsPdb.NameTableStrings m_parent;
            public uint CharsIndex { get { return _charsIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.NameTableStrings M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SPB
        /// </remarks>
        public partial class C11LinebufferSegPadBase : KaitaiStruct
        {
            public static C11LinebufferSegPadBase FromFile(string fileName)
            {
                return new C11LinebufferSegPadBase(new KaitaiStream(fileName));
            }

            public C11LinebufferSegPadBase(KaitaiStream p__io, MsPdb.C11Srcfile p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _numSegments = m_io.ReadU2le();
                _pad = m_io.ReadU2le();
                _baseSourceLen = new List<uint>();
                for (var i = 0; i < NumSegments; i++)
                {
                    _baseSourceLen.Add(m_io.ReadU4le());
                }
            }
            private ushort _numSegments;
            private ushort _pad;
            private List<uint> _baseSourceLen;
            private MsPdb m_root;
            private MsPdb.C11Srcfile m_parent;

            /// <remarks>
            /// Reference: cSeg
            /// </remarks>
            public ushort NumSegments { get { return _numSegments; } }

            /// <remarks>
            /// Reference: pad
            /// </remarks>
            public ushort Pad { get { return _pad; } }

            /// <remarks>
            /// Reference: baseSrcLn
            /// </remarks>
            public List<uint> BaseSourceLen { get { return _baseSourceLen; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C11Srcfile M_Parent { get { return m_parent; } }
        }
        public partial class CvNumericLiteral : KaitaiStruct
        {
            public CvNumericLiteral(ushort p_value, KaitaiStream p__io, MsPdb.CvNumericType p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _value = p_value;
                _read();
            }
            private void _read()
            {
            }
            private ushort _value;
            private MsPdb m_root;
            private MsPdb.CvNumericType m_parent;
            public ushort Value { get { return _value; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.CvNumericType M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: mlMethod_16t
        /// </remarks>
        public partial class MlMethod16t : KaitaiStruct
        {
            public static MlMethod16t FromFile(string fileName)
            {
                return new MlMethod16t(new KaitaiStream(fileName));
            }

            public MlMethod16t(KaitaiStream p__io, MsPdb.LfMethodlist16t p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _attributes = new CvFieldAttributes(m_io, this, m_root);
                _indexType = new TpiTypeRef16(m_io, this, m_root);
                if ( ((Attributes.MethodProperties == MsPdb.Tpi.CvMethodprop.Intro) || (Attributes.MethodProperties == MsPdb.Tpi.CvMethodprop.PureIntro)) ) {
                    _vtableOffset = m_io.ReadU4le();
                }
            }
            private CvFieldAttributes _attributes;
            private TpiTypeRef16 _indexType;
            private uint? _vtableOffset;
            private MsPdb m_root;
            private MsPdb.LfMethodlist16t m_parent;

            /// <summary>
            /// method attribute
            /// </summary>
            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public CvFieldAttributes Attributes { get { return _attributes; } }

            /// <summary>
            /// index to type record for procedure
            /// </summary>
            /// <remarks>
            /// Reference: index
            /// </remarks>
            public TpiTypeRef16 IndexType { get { return _indexType; } }

            /// <summary>
            /// offset in vfunctable if intro virtual
            /// </summary>
            /// <remarks>
            /// Reference: vbaseoff
            /// </remarks>
            public uint? VtableOffset { get { return _vtableOffset; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.LfMethodlist16t M_Parent { get { return m_parent; } }
        }
        public partial class LfChar : KaitaiStruct
        {
            public static LfChar FromFile(string fileName)
            {
                return new LfChar(new KaitaiStream(fileName));
            }

            public LfChar(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _value = m_io.ReadS1();
            }
            private sbyte _value;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public sbyte Value { get { return _value; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: lfVTShape
        /// </remarks>
        public partial class LfVtshape : KaitaiStruct
        {
            public static LfVtshape FromFile(string fileName)
            {
                return new LfVtshape(new KaitaiStream(fileName));
            }

            public LfVtshape(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _count = m_io.ReadU2le();
                _descriptors = new List<Tpi.CvVtsDesc>();
                for (var i = 0; i < Count; i++)
                {
                    _descriptors.Add(((MsPdb.Tpi.CvVtsDesc) m_io.ReadBitsIntLe(4)));
                }
            }
            private ushort _count;
            private List<Tpi.CvVtsDesc> _descriptors;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// number of entries in vfunctable
            /// </summary>
            /// <remarks>
            /// Reference: count
            /// </remarks>
            public ushort Count { get { return _count; } }

            /// <summary>
            /// 4 bit (CV_VTS_desc) descriptors
            /// </summary>
            /// <remarks>
            /// Reference: desc
            /// </remarks>
            public List<Tpi.CvVtsDesc> Descriptors { get { return _descriptors; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class GetNumPages2 : KaitaiStruct
        {
            public GetNumPages2(uint p_numBytes, uint p_pageSize, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _numBytes = p_numBytes;
                _pageSize = p_pageSize;
                f_numPages = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_numPages;
            private int _numPages;
            public int NumPages
            {
                get
                {
                    if (f_numPages)
                        return _numPages;
                    _numPages = (int) ((((NumBytes + PageSize) - 1) / PageSize));
                    f_numPages = true;
                    return _numPages;
                }
            }
            private uint _numBytes;
            private uint _pageSize;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint NumBytes { get { return _numBytes; } }
            public uint PageSize { get { return _pageSize; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class PdbMap : KaitaiStruct
        {
            public PdbMap(uint p_keySize, uint p_valueSize, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _keySize = p_keySize;
                _valueSize = p_valueSize;
                _read();
            }
            private void _read()
            {
                _cardinality = m_io.ReadU4le();
                _numElements = m_io.ReadU4le();
                _availableBitset = new PdbBitset(m_io, this, m_root);
                _deletedBitset = new PdbBitset(m_io, this, m_root);
                __raw_keyValuePairs = new List<byte[]>();
                _keyValuePairs = new List<PdbMapKvPair>();
                for (var i = 0; i < NumElements; i++)
                {
                    __raw_keyValuePairs.Add(m_io.ReadBytes(((KeySize + ValueSize) * (AvailableBitset.Values.Bits[i] ? 1 : 0))));
                    var io___raw_keyValuePairs = new KaitaiStream(__raw_keyValuePairs[__raw_keyValuePairs.Count - 1]);
                    _keyValuePairs.Add(new PdbMapKvPair(((uint) (i)), io___raw_keyValuePairs, this, m_root));
                }
            }
            private uint _cardinality;
            private uint _numElements;
            private PdbBitset _availableBitset;
            private PdbBitset _deletedBitset;
            private List<PdbMapKvPair> _keyValuePairs;
            private uint _keySize;
            private uint _valueSize;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            private List<byte[]> __raw_keyValuePairs;
            public uint Cardinality { get { return _cardinality; } }
            public uint NumElements { get { return _numElements; } }
            public PdbBitset AvailableBitset { get { return _availableBitset; } }
            public PdbBitset DeletedBitset { get { return _deletedBitset; } }
            public List<PdbMapKvPair> KeyValuePairs { get { return _keyValuePairs; } }
            public uint KeySize { get { return _keySize; } }
            public uint ValueSize { get { return _valueSize; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
            public List<byte[]> M_RawKeyValuePairs { get { return __raw_keyValuePairs; } }
        }

        /// <summary>
        /// Represents the holes in overall address range, all address is pre-bbt. it is for compress and reduce the amount of relocations need.
        /// </summary>
        /// <remarks>
        /// Reference: CV_LVAR_ADDR_GAP
        /// </remarks>
        public partial class CvLvarAddrGap : KaitaiStruct
        {
            public static CvLvarAddrGap FromFile(string fileName)
            {
                return new CvLvarAddrGap(new KaitaiStream(fileName));
            }

            public CvLvarAddrGap(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _gapStartOffset = m_io.ReadU2le();
                _gapLength = m_io.ReadU2le();
            }
            private ushort _gapStartOffset;
            private ushort _gapLength;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// relative offset from the beginning of the live range.
            /// </summary>
            /// <remarks>
            /// Reference: gapStartOffset
            /// </remarks>
            public ushort GapStartOffset { get { return _gapStartOffset; } }

            /// <summary>
            /// length of this gap.
            /// </summary>
            /// <remarks>
            /// Reference: cbRange
            /// </remarks>
            public ushort GapLength { get { return _gapLength; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_FIELDLIST
        /// </summary>
        /// <remarks>
        /// Reference: lfFieldList
        /// </remarks>
        public partial class LfFieldlist : KaitaiStruct
        {
            public static LfFieldlist FromFile(string fileName)
            {
                return new LfFieldlist(new KaitaiStream(fileName));
            }

            public LfFieldlist(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _data = new List<TpiTypeData>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _data.Add(new TpiTypeData(true, m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<TpiTypeData> _data;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// field list sub lists
            /// </summary>
            /// <remarks>
            /// Reference: data
            /// </remarks>
            public List<TpiTypeData> Data { get { return _data; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: MODI50
        /// </remarks>
        public partial class ModuleInfo50 : KaitaiStruct
        {
            public ModuleInfo50(uint p_moduleIndex, KaitaiStream p__io, MsPdb.UModuleInfo p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _moduleIndex = p_moduleIndex;
                f_paddingSize = false;
                f_alignment = false;
                f_positionStart = false;
                f_positionEnd = false;
                _read();
            }
            private void _read()
            {
                if (PositionStart >= 0) {
                    _invokePositionStart = m_io.ReadBytes(0);
                }
                _openModuleHandle = m_io.ReadU4le();
                _sectionContribution = new SectionContrib40(m_io, this, m_root);
                _flags = new ModuleInfoFlags(m_io, this, m_root);
                _stream = new PdbStreamRef(m_io, this, m_root);
                _symbolsSize = m_io.ReadU4le();
                _linesSize = m_io.ReadU4le();
                _fpoSize = m_io.ReadU4le();
                _numberOfFiles = m_io.ReadU2le();
                _pad0 = m_io.ReadU2le();
                _fileNamesOffsets = m_io.ReadU4le();
                _moduleName = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
                _objectFilename = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
                if (PositionEnd >= 0) {
                    _invokePositionEnd = m_io.ReadBytes(0);
                }
                _padding = m_io.ReadBytes(PaddingSize);
            }
            private bool f_paddingSize;
            private int _paddingSize;
            public int PaddingSize
            {
                get
                {
                    if (f_paddingSize)
                        return _paddingSize;
                    _paddingSize = (int) ((Alignment.Aligned - PositionEnd));
                    f_paddingSize = true;
                    return _paddingSize;
                }
            }
            private bool f_alignment;
            private Align _alignment;
            public Align Alignment
            {
                get
                {
                    if (f_alignment)
                        return _alignment;
                    _alignment = new Align(((uint) (PositionEnd)), 4, m_io, this, m_root);
                    f_alignment = true;
                    return _alignment;
                }
            }
            private bool f_positionStart;
            private int _positionStart;
            public int PositionStart
            {
                get
                {
                    if (f_positionStart)
                        return _positionStart;
                    _positionStart = (int) (M_Io.Pos);
                    f_positionStart = true;
                    return _positionStart;
                }
            }
            private bool f_positionEnd;
            private int _positionEnd;
            public int PositionEnd
            {
                get
                {
                    if (f_positionEnd)
                        return _positionEnd;
                    _positionEnd = (int) (M_Io.Pos);
                    f_positionEnd = true;
                    return _positionEnd;
                }
            }
            private byte[] _invokePositionStart;
            private uint _openModuleHandle;
            private SectionContrib40 _sectionContribution;
            private ModuleInfoFlags _flags;
            private PdbStreamRef _stream;
            private uint _symbolsSize;
            private uint _linesSize;
            private uint _fpoSize;
            private ushort _numberOfFiles;
            private ushort _pad0;
            private uint _fileNamesOffsets;
            private string _moduleName;
            private string _objectFilename;
            private byte[] _invokePositionEnd;
            private byte[] _padding;
            private uint _moduleIndex;
            private MsPdb m_root;
            private MsPdb.UModuleInfo m_parent;
            public byte[] InvokePositionStart { get { return _invokePositionStart; } }

            /// <summary>
            /// currently open mod
            /// </summary>
            /// <remarks>
            /// Reference: pmod
            /// </remarks>
            public uint OpenModuleHandle { get { return _openModuleHandle; } }

            /// <summary>
            /// this module's first section contribution
            /// </summary>
            /// <remarks>
            /// Reference: sc
            /// </remarks>
            public SectionContrib40 SectionContribution { get { return _sectionContribution; } }
            public ModuleInfoFlags Flags { get { return _flags; } }

            /// <summary>
            /// SN of module debug info (syms, lines, fpo), or snNil
            /// </summary>
            /// <remarks>
            /// Reference: sn
            /// </remarks>
            public PdbStreamRef Stream { get { return _stream; } }

            /// <summary>
            /// size of local symbols debug info in stream sn
            /// </summary>
            /// <remarks>
            /// Reference: cbSyms
            /// </remarks>
            public uint SymbolsSize { get { return _symbolsSize; } }

            /// <summary>
            /// size of line number debug info in stream sn
            /// </summary>
            /// <remarks>
            /// Reference: cbLines
            /// </remarks>
            public uint LinesSize { get { return _linesSize; } }

            /// <summary>
            /// size of frame pointer opt debug info in stream sn
            /// </summary>
            /// <remarks>
            /// Reference: cbFpo
            /// </remarks>
            public uint FpoSize { get { return _fpoSize; } }

            /// <summary>
            /// number of files contributing to this module
            /// </summary>
            /// <remarks>
            /// Reference: ifileMac
            /// </remarks>
            public ushort NumberOfFiles { get { return _numberOfFiles; } }
            public ushort Pad0 { get { return _pad0; } }

            /// <summary>
            /// array [0..ifileMac) of offsets into dbi.bufFilenames
            /// </summary>
            /// <remarks>
            /// Reference: mpifileichFile
            /// </remarks>
            public uint FileNamesOffsets { get { return _fileNamesOffsets; } }

            /// <remarks>
            /// Reference: rgch.szModule
            /// </remarks>
            public string ModuleName { get { return _moduleName; } }

            /// <remarks>
            /// Reference: rgch.szObjFile
            /// </remarks>
            public string ObjectFilename { get { return _objectFilename; } }
            public byte[] InvokePositionEnd { get { return _invokePositionEnd; } }
            public byte[] Padding { get { return _padding; } }
            public uint ModuleIndex { get { return _moduleIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.UModuleInfo M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DEBUG_S_FRAMEDATA
        /// </remarks>
        public partial class C13SubsectionFrameData : KaitaiStruct
        {
            public static C13SubsectionFrameData FromFile(string fileName)
            {
                return new C13SubsectionFrameData(new KaitaiStream(fileName));
            }

            public C13SubsectionFrameData(KaitaiStream p__io, MsPdb.C13Subsection p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _frames = new List<C13FrameData>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _frames.Add(new C13FrameData(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<C13FrameData> _frames;
            private MsPdb m_root;
            private MsPdb.C13Subsection m_parent;
            public List<C13FrameData> Frames { get { return _frames; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13Subsection M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: COMPILESYM.flags
        /// </remarks>
        public partial class SymCompile2Flags : KaitaiStruct
        {
            public static SymCompile2Flags FromFile(string fileName)
            {
                return new SymCompile2Flags(new KaitaiStream(fileName));
            }

            public SymCompile2Flags(KaitaiStream p__io, MsPdb.SymCompile2 p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _language = m_io.ReadU1();
                _ec = m_io.ReadBitsIntLe(1) != 0;
                _noDbgInfo = m_io.ReadBitsIntLe(1) != 0;
                _ltcg = m_io.ReadBitsIntLe(1) != 0;
                _noDataAlign = m_io.ReadBitsIntLe(1) != 0;
                _managedPresent = m_io.ReadBitsIntLe(1) != 0;
                _securityChecks = m_io.ReadBitsIntLe(1) != 0;
                _hotPatch = m_io.ReadBitsIntLe(1) != 0;
                _cvtCil = m_io.ReadBitsIntLe(1) != 0;
                _msilModule = m_io.ReadBitsIntLe(1) != 0;
                _pad = m_io.ReadBitsIntLe(15);
            }
            private byte _language;
            private bool _ec;
            private bool _noDbgInfo;
            private bool _ltcg;
            private bool _noDataAlign;
            private bool _managedPresent;
            private bool _securityChecks;
            private bool _hotPatch;
            private bool _cvtCil;
            private bool _msilModule;
            private ulong _pad;
            private MsPdb m_root;
            private MsPdb.SymCompile2 m_parent;

            /// <summary>
            /// language index
            /// </summary>
            /// <remarks>
            /// Reference: iLanguage
            /// </remarks>
            public byte Language { get { return _language; } }

            /// <summary>
            /// compiled for E/C
            /// </summary>
            /// <remarks>
            /// Reference: fEC
            /// </remarks>
            public bool Ec { get { return _ec; } }

            /// <summary>
            /// not compiled with debug info
            /// </summary>
            /// <remarks>
            /// Reference: fNoDbgInfo
            /// </remarks>
            public bool NoDbgInfo { get { return _noDbgInfo; } }

            /// <summary>
            /// compiled with LTCG
            /// </summary>
            /// <remarks>
            /// Reference: fLTCG
            /// </remarks>
            public bool Ltcg { get { return _ltcg; } }

            /// <summary>
            /// compiled with -Bzalign
            /// </summary>
            /// <remarks>
            /// Reference: fNoDataAlign
            /// </remarks>
            public bool NoDataAlign { get { return _noDataAlign; } }

            /// <summary>
            /// managed code/data present
            /// </summary>
            /// <remarks>
            /// Reference: fManagedPresent
            /// </remarks>
            public bool ManagedPresent { get { return _managedPresent; } }

            /// <summary>
            /// compiled with /GS
            /// </summary>
            /// <remarks>
            /// Reference: fSecurityChecks
            /// </remarks>
            public bool SecurityChecks { get { return _securityChecks; } }

            /// <summary>
            /// compiled with /hotpatch
            /// </summary>
            /// <remarks>
            /// Reference: fHotPatch
            /// </remarks>
            public bool HotPatch { get { return _hotPatch; } }

            /// <summary>
            /// converted with CVTCIL
            /// </summary>
            /// <remarks>
            /// Reference: fCVTCIL
            /// </remarks>
            public bool CvtCil { get { return _cvtCil; } }

            /// <summary>
            /// MSIL netmodule
            /// </summary>
            /// <remarks>
            /// Reference: fMSILModule
            /// </remarks>
            public bool MsilModule { get { return _msilModule; } }

            /// <summary>
            /// reserved, must be 0
            /// </summary>
            /// <remarks>
            /// Reference: pad
            /// </remarks>
            public ulong Pad { get { return _pad; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.SymCompile2 M_Parent { get { return m_parent; } }
        }
        public partial class PdbStreamPagelist : KaitaiStruct
        {
            public PdbStreamPagelist(uint p_streamNumber, KaitaiStream p__io, MsPdb.PdbStreamTable p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _streamNumber = p_streamNumber;
                f_zzzStreamSize = false;
                f_streamSize = false;
                f_numDirectoryPages = false;
                f_data = false;
                f_hasData = false;
                f_zzzPages = false;
                _read();
            }
            private void _read()
            {
                _pages = new PdbPageNumberList(((uint) (NumDirectoryPages)), m_io, this, m_root);
            }
            private bool f_zzzStreamSize;
            private GetStreamSize _zzzStreamSize;
            public GetStreamSize ZzzStreamSize
            {
                get
                {
                    if (f_zzzStreamSize)
                        return _zzzStreamSize;
                    _zzzStreamSize = new GetStreamSize(StreamNumber, m_io, this, m_root);
                    f_zzzStreamSize = true;
                    return _zzzStreamSize;
                }
            }
            private bool f_streamSize;
            private int _streamSize;
            public int StreamSize
            {
                get
                {
                    if (f_streamSize)
                        return _streamSize;
                    _streamSize = (int) (ZzzStreamSize.Value);
                    f_streamSize = true;
                    return _streamSize;
                }
            }
            private bool f_numDirectoryPages;
            private int _numDirectoryPages;
            public int NumDirectoryPages
            {
                get
                {
                    if (f_numDirectoryPages)
                        return _numDirectoryPages;
                    _numDirectoryPages = (int) ((M_Root.PdbType == MsPdb.PdbTypeEnum.Big ? M_Parent.StreamSizesDs[((int) (StreamNumber))].NumDirectoryPages : M_Parent.StreamSizesJg[((int) (StreamNumber))].NumDirectoryPages));
                    f_numDirectoryPages = true;
                    return _numDirectoryPages;
                }
            }
            private bool f_data;
            private byte[] _data;
            public byte[] Data
            {
                get
                {
                    if (f_data)
                        return _data;
                    _data = (byte[]) (ZzzPages.Data);
                    f_data = true;
                    return _data;
                }
            }
            private bool f_hasData;
            private bool _hasData;
            public bool HasData
            {
                get
                {
                    if (f_hasData)
                        return _hasData;
                    _hasData = (bool) (StreamSize > 0);
                    f_hasData = true;
                    return _hasData;
                }
            }
            private bool f_zzzPages;
            private PdbStreamData _zzzPages;
            public PdbStreamData ZzzPages
            {
                get
                {
                    if (f_zzzPages)
                        return _zzzPages;
                    __raw__raw_zzzPages = m_io.ReadBytes(0);
                    ConcatPages _process__raw__raw_zzzPages = new ConcatPages(Pages.Pages);
                    __raw_zzzPages = _process__raw__raw_zzzPages.Decode(__raw__raw_zzzPages);
                    var io___raw_zzzPages = new KaitaiStream(__raw_zzzPages);
                    _zzzPages = new PdbStreamData(StreamSize, io___raw_zzzPages, this, m_root);
                    f_zzzPages = true;
                    return _zzzPages;
                }
            }
            private PdbPageNumberList _pages;
            private uint _streamNumber;
            private MsPdb m_root;
            private MsPdb.PdbStreamTable m_parent;
            private byte[] __raw_zzzPages;
            private byte[] __raw__raw_zzzPages;
            public PdbPageNumberList Pages { get { return _pages; } }
            public uint StreamNumber { get { return _streamNumber; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbStreamTable M_Parent { get { return m_parent; } }
            public byte[] M_RawZzzPages { get { return __raw_zzzPages; } }
            public byte[] M_RawM_RawZzzPages { get { return __raw__raw_zzzPages; } }
        }
        public partial class C13FileBlock : KaitaiStruct
        {
            public static C13FileBlock FromFile(string fileName)
            {
                return new C13FileBlock(new KaitaiStream(fileName));
            }

            public C13FileBlock(KaitaiStream p__io, MsPdb.C13SubsectionLines p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _fileId = m_io.ReadU4le();
                _numLines = m_io.ReadU4le();
                _fileBlockLength = m_io.ReadU4le();
                _lines = new List<C13Line>();
                for (var i = 0; i < NumLines; i++)
                {
                    _lines.Add(new C13Line(m_io, this, m_root));
                }
                if (M_Parent.HaveColumns) {
                    _columns = new List<C13Column>();
                    for (var i = 0; i < NumLines; i++)
                    {
                        _columns.Add(new C13Column(m_io, this, m_root));
                    }
                }
            }
            private uint _fileId;
            private uint _numLines;
            private uint _fileBlockLength;
            private List<C13Line> _lines;
            private List<C13Column> _columns;
            private MsPdb m_root;
            private MsPdb.C13SubsectionLines m_parent;
            public uint FileId { get { return _fileId; } }
            public uint NumLines { get { return _numLines; } }
            public uint FileBlockLength { get { return _fileBlockLength; } }
            public List<C13Line> Lines { get { return _lines; } }
            public List<C13Column> Columns { get { return _columns; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13SubsectionLines M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_FIELDLIST_16t
        /// </summary>
        /// <remarks>
        /// Reference: lfFieldList_16t
        /// </remarks>
        public partial class LfFieldlist16t : KaitaiStruct
        {
            public static LfFieldlist16t FromFile(string fileName)
            {
                return new LfFieldlist16t(new KaitaiStream(fileName));
            }

            public LfFieldlist16t(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _data = new List<TpiTypeData>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _data.Add(new TpiTypeData(true, m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<TpiTypeData> _data;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// field list sub lists
            /// </summary>
            /// <remarks>
            /// Reference: data
            /// </remarks>
            public List<TpiTypeData> Data { get { return _data; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class C13Lines : KaitaiStruct
        {
            public static C13Lines FromFile(string fileName)
            {
                return new C13Lines(new KaitaiStream(fileName));
            }


            public enum SubsectionType : uint
            {
                Symbols = 241,
                Lines = 242,
                StringTable = 243,
                FileChkSms = 244,
                FrameData = 245,
                InlineeLines = 246,
                CrossScopeImports = 247,
                CrossScopeExports = 248,
                IlLines = 249,
                FuncMdtokenMap = 250,
                TypeMdtokenMap = 251,
                MergedAssemblyInput = 252,
                CoffSymbolRva = 253,
                Ignore = 2147483648,
            }
            public C13Lines(KaitaiStream p__io, MsPdb.ModuleStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _subsection = new List<C13Subsection>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _subsection.Add(new C13Subsection(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<C13Subsection> _subsection;
            private MsPdb m_root;
            private MsPdb.ModuleStream m_parent;
            public List<C13Subsection> Subsection { get { return _subsection; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.ModuleStream M_Parent { get { return m_parent; } }
        }
        public partial class OmfSegmentMap : KaitaiStruct
        {
            public static OmfSegmentMap FromFile(string fileName)
            {
                return new OmfSegmentMap(new KaitaiStream(fileName));
            }

            public OmfSegmentMap(KaitaiStream p__io, MsPdb.Dbi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _numSegments = m_io.ReadU2le();
                _numLogicalSegments = m_io.ReadU2le();
                _segments = new List<OmfSegmentMapDescriptor>();
                for (var i = 0; i < NumSegments; i++)
                {
                    _segments.Add(new OmfSegmentMapDescriptor(m_io, this, m_root));
                }
            }
            private ushort _numSegments;
            private ushort _numLogicalSegments;
            private List<OmfSegmentMapDescriptor> _segments;
            private MsPdb m_root;
            private MsPdb.Dbi m_parent;
            public ushort NumSegments { get { return _numSegments; } }
            public ushort NumLogicalSegments { get { return _numLogicalSegments; } }
            public List<OmfSegmentMapDescriptor> Segments { get { return _segments; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Dbi M_Parent { get { return m_parent; } }
        }
        public partial class FpoStream : KaitaiStruct
        {
            public static FpoStream FromFile(string fileName)
            {
                return new FpoStream(new KaitaiStream(fileName));
            }

            public FpoStream(KaitaiStream p__io, MsPdb.DebugData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _items = new List<FpoData>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _items.Add(new FpoData(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<FpoData> _items;
            private MsPdb m_root;
            private MsPdb.DebugData m_parent;
            public List<FpoData> Items { get { return _items; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DebugData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: ATTRSLOTSYM
        /// </remarks>
        public partial class SymAttrSlot : KaitaiStruct
        {
            public SymAttrSlot(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _slotIndex = m_io.ReadU4le();
                _type = new TpiTypeRef(m_io, this, m_root);
                _attr = new CvLvarAttr(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private uint _slotIndex;
            private TpiTypeRef _type;
            private CvLvarAttr _attr;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// slot index
            /// </summary>
            /// <remarks>
            /// Reference: iSlot
            /// </remarks>
            public uint SlotIndex { get { return _slotIndex; } }

            /// <summary>
            /// Type index or Metadata token
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// local var attributes
            /// </summary>
            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public CvLvarAttr Attr { get { return _attr; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class TpiTypeRef : KaitaiStruct
        {
            public static TpiTypeRef FromFile(string fileName)
            {
                return new TpiTypeRef(new KaitaiStream(fileName));
            }

            public TpiTypeRef(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_arrayIndex = false;
                f_type = false;
                _read();
            }
            private void _read()
            {
                _index = m_io.ReadU4le();
            }
            private bool f_arrayIndex;
            private int _arrayIndex;
            public int ArrayIndex
            {
                get
                {
                    if (f_arrayIndex)
                        return _arrayIndex;
                    _arrayIndex = (int) ((Index - M_Root.MinTypeIndex));
                    f_arrayIndex = true;
                    return _arrayIndex;
                }
            }
            private bool f_type;
            private TpiType _type;
            public TpiType Type
            {
                get
                {
                    if (f_type)
                        return _type;
                    if (ArrayIndex >= 0) {
                        _type = (TpiType) (M_Root.Types[ArrayIndex]);
                    }
                    f_type = true;
                    return _type;
                }
            }
            private uint _index;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint Index { get { return _index; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: EXPORTSYM
        /// </remarks>
        public partial class SymExport : KaitaiStruct
        {
            public static SymExport FromFile(string fileName)
            {
                return new SymExport(new KaitaiStream(fileName));
            }

            public SymExport(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _ordinal = m_io.ReadU2le();
                _isConstant = m_io.ReadBitsIntLe(1) != 0;
                _isData = m_io.ReadBitsIntLe(1) != 0;
                _isPrivate = m_io.ReadBitsIntLe(1) != 0;
                _isNoname = m_io.ReadBitsIntLe(1) != 0;
                _isOrdinal = m_io.ReadBitsIntLe(1) != 0;
                _isForwarder = m_io.ReadBitsIntLe(1) != 0;
                _reserved = m_io.ReadBitsIntLe(10);
                m_io.AlignToByte();
                _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
            }
            private ushort _ordinal;
            private bool _isConstant;
            private bool _isData;
            private bool _isPrivate;
            private bool _isNoname;
            private bool _isOrdinal;
            private bool _isForwarder;
            private ulong _reserved;
            private string _name;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <remarks>
            /// Reference: ordinal
            /// </remarks>
            public ushort Ordinal { get { return _ordinal; } }

            /// <summary>
            /// CONSTANT
            /// </summary>
            /// <remarks>
            /// Reference: fConstant
            /// </remarks>
            public bool IsConstant { get { return _isConstant; } }

            /// <summary>
            /// DATA
            /// </summary>
            /// <remarks>
            /// Reference: fData
            /// </remarks>
            public bool IsData { get { return _isData; } }

            /// <summary>
            /// PRIVATE
            /// </summary>
            /// <remarks>
            /// Reference: fPrivate
            /// </remarks>
            public bool IsPrivate { get { return _isPrivate; } }

            /// <summary>
            /// NONAME
            /// </summary>
            /// <remarks>
            /// Reference: fNoName
            /// </remarks>
            public bool IsNoname { get { return _isNoname; } }

            /// <summary>
            /// Ordinal was explicitly assigned
            /// </summary>
            /// <remarks>
            /// Reference: fOrdinal
            /// </remarks>
            public bool IsOrdinal { get { return _isOrdinal; } }

            /// <summary>
            /// This is a forwarder
            /// </summary>
            /// <remarks>
            /// Reference: fForwarder
            /// </remarks>
            public bool IsForwarder { get { return _isForwarder; } }

            /// <summary>
            /// Reserved. Must be zero.
            /// </summary>
            /// <remarks>
            /// Reference: reserved
            /// </remarks>
            public ulong Reserved { get { return _reserved; } }

            /// <summary>
            /// name of
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public string Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: PSGSIHDR
        /// </remarks>
        public partial class PsgiHeader : KaitaiStruct
        {
            public static PsgiHeader FromFile(string fileName)
            {
                return new PsgiHeader(new KaitaiStream(fileName));
            }

            public PsgiHeader(KaitaiStream p__io, MsPdb.PublicSymbolsStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _symHashSize = m_io.ReadU4le();
                _addressMapSize = m_io.ReadU4le();
                _numThunks = m_io.ReadU4le();
                _thunkSize = m_io.ReadU4le();
                _thunkTableSectionIndex = m_io.ReadU4le();
                _thunkTableOffset = m_io.ReadU4le();
                _numSections = m_io.ReadU4le();
            }
            private uint _symHashSize;
            private uint _addressMapSize;
            private uint _numThunks;
            private uint _thunkSize;
            private uint _thunkTableSectionIndex;
            private uint _thunkTableOffset;
            private uint _numSections;
            private MsPdb m_root;
            private MsPdb.PublicSymbolsStream m_parent;

            /// <remarks>
            /// Reference: cbSymHash
            /// </remarks>
            public uint SymHashSize { get { return _symHashSize; } }

            /// <remarks>
            /// Reference: cbAddrMap
            /// </remarks>
            public uint AddressMapSize { get { return _addressMapSize; } }

            /// <remarks>
            /// Reference: nThunks
            /// </remarks>
            public uint NumThunks { get { return _numThunks; } }

            /// <remarks>
            /// Reference: cbSizeOfThunk
            /// </remarks>
            public uint ThunkSize { get { return _thunkSize; } }

            /// <remarks>
            /// Reference: isectThunkTable
            /// </remarks>
            public uint ThunkTableSectionIndex { get { return _thunkTableSectionIndex; } }

            /// <remarks>
            /// Reference: offThunkTable
            /// </remarks>
            public uint ThunkTableOffset { get { return _thunkTableOffset; } }

            /// <remarks>
            /// Reference: nSects
            /// </remarks>
            public uint NumSections { get { return _numSections; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PublicSymbolsStream M_Parent { get { return m_parent; } }
        }
        public partial class Dbi : KaitaiStruct
        {
            public static Dbi FromFile(string fileName)
            {
                return new Dbi(new KaitaiStream(fileName));
            }


            public enum SymbolType
            {
                SCompile = 1,
                SRegister16t = 2,
                SConstant16t = 3,
                SUdt16t = 4,
                SSsearch = 5,
                SEnd = 6,
                SSkip = 7,
                SCvreserve = 8,
                SObjnameSt = 9,
                SEndarg = 10,
                SCoboludt16t = 11,
                SManyreg16t = 12,
                SReturn = 13,
                SEntrythis = 14,
                SBprel16 = 256,
                SLdata16 = 257,
                SGdata16 = 258,
                SPub16 = 259,
                SLproc16 = 260,
                SGproc16 = 261,
                SThunk16 = 262,
                SBlock16 = 263,
                SWith16 = 264,
                SLabel16 = 265,
                SCexmodel16 = 266,
                SVftable16 = 267,
                SRegrel16 = 268,
                SBprel3216t = 512,
                SLdata3216t = 513,
                SGdata3216t = 514,
                SPub3216t = 515,
                SLproc3216t = 516,
                SGproc3216t = 517,
                SThunk32St = 518,
                SBlock32St = 519,
                SWith32St = 520,
                SLabel32St = 521,
                SCexmodel32 = 522,
                SVftable3216t = 523,
                SRegrel3216t = 524,
                SLthread3216t = 525,
                SGthread3216t = 526,
                SSlink32 = 527,
                SLprocmips16t = 768,
                SGprocmips16t = 769,
                SProcrefSt = 1024,
                SDatarefSt = 1025,
                SAlign = 1026,
                SLprocrefSt = 1027,
                SOem = 1028,
                STi16Max = 4096,
                SRegisterSt = 4097,
                SConstantSt = 4098,
                SUdtSt = 4099,
                SCoboludtSt = 4100,
                SManyregSt = 4101,
                SBprel32St = 4102,
                SLdata32St = 4103,
                SGdata32St = 4104,
                SPub32St = 4105,
                SLproc32St = 4106,
                SGproc32St = 4107,
                SVftable32 = 4108,
                SRegrel32St = 4109,
                SLthread32St = 4110,
                SGthread32St = 4111,
                SLprocmipsSt = 4112,
                SGprocmipsSt = 4113,
                SFrameproc = 4114,
                SCompile2St = 4115,
                SManyreg2St = 4116,
                SLprocia64St = 4117,
                SGprocia64St = 4118,
                SLocalslotSt = 4119,
                SParamslotSt = 4120,
                SAnnotation = 4121,
                SGmanprocSt = 4122,
                SLmanprocSt = 4123,
                SReserved1 = 4124,
                SReserved2 = 4125,
                SReserved3 = 4126,
                SReserved4 = 4127,
                SLmandataSt = 4128,
                SGmandataSt = 4129,
                SManframerelSt = 4130,
                SManregisterSt = 4131,
                SManslotSt = 4132,
                SManmanyregSt = 4133,
                SManregrelSt = 4134,
                SManmanyreg2St = 4135,
                SMantypref = 4136,
                SUnamespaceSt = 4137,
                SStMax = 4352,
                SObjname = 4353,
                SThunk32 = 4354,
                SBlock32 = 4355,
                SWith32 = 4356,
                SLabel32 = 4357,
                SRegister = 4358,
                SConstant = 4359,
                SUdt = 4360,
                SCoboludt = 4361,
                SManyreg = 4362,
                SBprel32 = 4363,
                SLdata32 = 4364,
                SGdata32 = 4365,
                SPub32 = 4366,
                SLproc32 = 4367,
                SGproc32 = 4368,
                SRegrel32 = 4369,
                SLthread32 = 4370,
                SGthread32 = 4371,
                SLprocmips = 4372,
                SGprocmips = 4373,
                SCompile2 = 4374,
                SManyreg2 = 4375,
                SLprocia64 = 4376,
                SGprocia64 = 4377,
                SLocalslot = 4378,
                SParamslot = 4379,
                SLmandata = 4380,
                SGmandata = 4381,
                SManframerel = 4382,
                SManregister = 4383,
                SManslot = 4384,
                SManmanyreg = 4385,
                SManregrel = 4386,
                SManmanyreg2 = 4387,
                SUnamespace = 4388,
                SProcref = 4389,
                SDataref = 4390,
                SLprocref = 4391,
                SAnnotationref = 4392,
                STokenref = 4393,
                SGmanproc = 4394,
                SLmanproc = 4395,
                STrampoline = 4396,
                SManconstant = 4397,
                SAttrFramerel = 4398,
                SAttrRegister = 4399,
                SAttrRegrel = 4400,
                SAttrManyreg = 4401,
                SSepcode = 4402,
                SLocal2005 = 4403,
                SDefrange2005 = 4404,
                SDefrange22005 = 4405,
                SSection = 4406,
                SCoffgroup = 4407,
                SExport = 4408,
                SCallsiteinfo = 4409,
                SFramecookie = 4410,
                SDiscarded = 4411,
                SCompile3 = 4412,
                SEnvblock = 4413,
                SLocal = 4414,
                SDefrange = 4415,
                SSubfield = 4416,
                SDefrangeRegister = 4417,
                SDefrangeFramepointerRel = 4418,
                SDefrangeSubfieldRegister = 4419,
                SDefrangeFramepointerRelFullScope = 4420,
                SDefrangeRegisterRel = 4421,
                SLproc32Id = 4422,
                SGproc32Id = 4423,
                SLprocmipsId = 4424,
                SGprocmipsId = 4425,
                SLprocia64Id = 4426,
                SGprocia64Id = 4427,
                SBuildinfo = 4428,
                SInlinesite = 4429,
                SInlinesiteEnd = 4430,
                SProcIdEnd = 4431,
                SDefrangeHlsl = 4432,
                SGdataHlsl = 4433,
                SLdataHlsl = 4434,
                SFilestatic = 4435,
                SLocalDpcGroupshared = 4436,
                SLproc32Dpc = 4437,
                SLproc32DpcId = 4438,
                SDefrangeDpcPtrTag = 4439,
                SDpcSymTagMap = 4440,
                SArmswitchtable = 4441,
                SCallees = 4442,
                SCallers = 4443,
                SPogodata = 4444,
                SInlinesite2 = 4445,
                SHeapallocsite = 4446,
                SModTyperef = 4447,
                SRefMinipdb = 4448,
                SPdbmap = 4449,
                SGdataHlsl32 = 4450,
                SLdataHlsl32 = 4451,
                SGdataHlsl32Ex = 4452,
                SLdataHlsl32Ex = 4453,
                SFastlink = 4455,
                SInlinees = 4456,
            }
            public Dbi(KaitaiStream p__io, MsPdb p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_symbolsData = false;
                f_gsSymbolsData = false;
                f_isNewHdr = false;
                f_signature = false;
                f_modulesPos = false;
                f_sectionContributionsVersion = false;
                f_psSymbolsData = false;
                _read();
            }
            private void _read()
            {
                if (IsNewHdr == false) {
                    _headerOld = new DbiHeaderOld(m_io, this, m_root);
                }
                if (IsNewHdr == true) {
                    _headerNew = new DbiHeaderNew(m_io, this, m_root);
                }
                if (ModulesPos >= 0) {
                    _invokeModulesPos = m_io.ReadBytes(0);
                }
                if (HeaderNew.ModuleListSize > 0) {
                    __raw_modulesList = m_io.ReadBytes((IsNewHdr ? HeaderNew.ModuleListSize : HeaderOld.ModuleListSize));
                    var io___raw_modulesList = new KaitaiStream(__raw_modulesList);
                    _modulesList = new ModuleList(io___raw_modulesList, this, m_root);
                }
                if (HeaderNew.SectionContributionSize > 0) {
                    __raw_sectionContributions = m_io.ReadBytes((IsNewHdr ? HeaderNew.SectionContributionSize : HeaderOld.SectionContributionSize));
                    var io___raw_sectionContributions = new KaitaiStream(__raw_sectionContributions);
                    _sectionContributions = new SectionContributionList(io___raw_sectionContributions, this, m_root);
                }
                if (HeaderNew.SectionMapSize > 0) {
                    __raw_sectionMap = m_io.ReadBytes((IsNewHdr ? HeaderNew.SectionMapSize : HeaderOld.SectionMapSize));
                    var io___raw_sectionMap = new KaitaiStream(__raw_sectionMap);
                    _sectionMap = new OmfSegmentMap(io___raw_sectionMap, this, m_root);
                }
                if ( ((IsNewHdr) && (HeaderNew.FileInfoSize > 0)) ) {
                    __raw_fileInfo = m_io.ReadBytes(HeaderNew.FileInfoSize);
                    var io___raw_fileInfo = new KaitaiStream(__raw_fileInfo);
                    _fileInfo = new FileInfo(io___raw_fileInfo, this, m_root);
                }
                if ( ((IsNewHdr) && (HeaderNew.TypeServerMapSize > 0)) ) {
                    __raw_typeServerMap = m_io.ReadBytes(HeaderNew.TypeServerMapSize);
                    var io___raw_typeServerMap = new KaitaiStream(__raw_typeServerMap);
                    _typeServerMap = new TypeServerMap(io___raw_typeServerMap, this, m_root);
                }
                if ( ((IsNewHdr) && (HeaderNew.EcSubstreamSize > 0)) ) {
                    __raw_ecInfo = m_io.ReadBytes(HeaderNew.EcSubstreamSize);
                    var io___raw_ecInfo = new KaitaiStream(__raw_ecInfo);
                    _ecInfo = new NameTable(io___raw_ecInfo, this, m_root);
                }
                if ( ((IsNewHdr) && (HeaderNew.DebugHeaderSize > 0)) ) {
                    __raw_debugData = m_io.ReadBytes(HeaderNew.DebugHeaderSize);
                    var io___raw_debugData = new KaitaiStream(__raw_debugData);
                    _debugData = new DebugData(io___raw_debugData, this, m_root);
                }
            }
            private bool f_symbolsData;
            private SymbolRecordsStream _symbolsData;
            public SymbolRecordsStream SymbolsData
            {
                get
                {
                    if (f_symbolsData)
                        return _symbolsData;
                    _symbolsData = (SymbolRecordsStream) ((IsNewHdr ? HeaderNew.SymbolsData : HeaderOld.SymbolsData));
                    f_symbolsData = true;
                    return _symbolsData;
                }
            }
            private bool f_gsSymbolsData;
            private GlobalSymbolsStream _gsSymbolsData;
            public GlobalSymbolsStream GsSymbolsData
            {
                get
                {
                    if (f_gsSymbolsData)
                        return _gsSymbolsData;
                    _gsSymbolsData = (GlobalSymbolsStream) ((IsNewHdr ? HeaderNew.GsSymbolsData : HeaderOld.GsSymbolsData));
                    f_gsSymbolsData = true;
                    return _gsSymbolsData;
                }
            }
            private bool f_isNewHdr;
            private bool _isNewHdr;
            public bool IsNewHdr
            {
                get
                {
                    if (f_isNewHdr)
                        return _isNewHdr;
                    _isNewHdr = (bool) (Signature == -1);
                    f_isNewHdr = true;
                    return _isNewHdr;
                }
            }
            private bool f_signature;
            private int _signature;
            public int Signature
            {
                get
                {
                    if (f_signature)
                        return _signature;
                    long _pos = m_io.Pos;
                    m_io.Seek(0);
                    _signature = m_io.ReadS4le();
                    m_io.Seek(_pos);
                    f_signature = true;
                    return _signature;
                }
            }
            private bool f_modulesPos;
            private int _modulesPos;
            public int ModulesPos
            {
                get
                {
                    if (f_modulesPos)
                        return _modulesPos;
                    _modulesPos = (int) (M_Io.Pos);
                    f_modulesPos = true;
                    return _modulesPos;
                }
            }
            private bool f_sectionContributionsVersion;
            private SectionContributionList.VersionType _sectionContributionsVersion;
            public SectionContributionList.VersionType SectionContributionsVersion
            {
                get
                {
                    if (f_sectionContributionsVersion)
                        return _sectionContributionsVersion;
                    long _pos = m_io.Pos;
                    m_io.Seek((ModulesPos + HeaderNew.ModuleListSize));
                    _sectionContributionsVersion = ((MsPdb.SectionContributionList.VersionType) m_io.ReadU4le());
                    m_io.Seek(_pos);
                    f_sectionContributionsVersion = true;
                    return _sectionContributionsVersion;
                }
            }
            private bool f_psSymbolsData;
            private PublicSymbolsStream _psSymbolsData;
            public PublicSymbolsStream PsSymbolsData
            {
                get
                {
                    if (f_psSymbolsData)
                        return _psSymbolsData;
                    _psSymbolsData = (PublicSymbolsStream) ((IsNewHdr ? HeaderNew.PsSymbolsData : HeaderOld.PsSymbolsData));
                    f_psSymbolsData = true;
                    return _psSymbolsData;
                }
            }
            private DbiHeaderOld _headerOld;
            private DbiHeaderNew _headerNew;
            private byte[] _invokeModulesPos;
            private ModuleList _modulesList;
            private SectionContributionList _sectionContributions;
            private OmfSegmentMap _sectionMap;
            private FileInfo _fileInfo;
            private TypeServerMap _typeServerMap;
            private NameTable _ecInfo;
            private DebugData _debugData;
            private MsPdb m_root;
            private MsPdb m_parent;
            private byte[] __raw_modulesList;
            private byte[] __raw_sectionContributions;
            private byte[] __raw_sectionMap;
            private byte[] __raw_fileInfo;
            private byte[] __raw_typeServerMap;
            private byte[] __raw_ecInfo;
            private byte[] __raw_debugData;
            public DbiHeaderOld HeaderOld { get { return _headerOld; } }
            public DbiHeaderNew HeaderNew { get { return _headerNew; } }
            public byte[] InvokeModulesPos { get { return _invokeModulesPos; } }
            public ModuleList ModulesList { get { return _modulesList; } }
            public SectionContributionList SectionContributions { get { return _sectionContributions; } }
            public OmfSegmentMap SectionMap { get { return _sectionMap; } }
            public FileInfo FileInfo { get { return _fileInfo; } }
            public TypeServerMap TypeServerMap { get { return _typeServerMap; } }
            public NameTable EcInfo { get { return _ecInfo; } }
            public DebugData DebugData { get { return _debugData; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb M_Parent { get { return m_parent; } }
            public byte[] M_RawModulesList { get { return __raw_modulesList; } }
            public byte[] M_RawSectionContributions { get { return __raw_sectionContributions; } }
            public byte[] M_RawSectionMap { get { return __raw_sectionMap; } }
            public byte[] M_RawFileInfo { get { return __raw_fileInfo; } }
            public byte[] M_RawTypeServerMap { get { return __raw_typeServerMap; } }
            public byte[] M_RawEcInfo { get { return __raw_ecInfo; } }
            public byte[] M_RawDebugData { get { return __raw_debugData; } }
        }
        public partial class C11LineInfo : KaitaiStruct
        {
            public C11LineInfo(ushort p_lineNumber, uint p_offset, KaitaiStream p__io, MsPdb.C11SectionInfo2 p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _lineNumber = p_lineNumber;
                _offset = p_offset;
                _read();
            }
            private void _read()
            {
            }
            private ushort _lineNumber;
            private uint _offset;
            private MsPdb m_root;
            private MsPdb.C11SectionInfo2 m_parent;

            /// <remarks>
            /// Reference: line
            /// </remarks>
            public ushort LineNumber { get { return _lineNumber; } }

            /// <remarks>
            /// Reference: offset
            /// </remarks>
            public uint Offset { get { return _offset; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C11SectionInfo2 M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SectInfo
        /// </remarks>
        public partial class C11SectionInfo : KaitaiStruct
        {
            public static C11SectionInfo FromFile(string fileName)
            {
                return new C11SectionInfo(new KaitaiStream(fileName));
            }

            public C11SectionInfo(KaitaiStream p__io, MsPdb.C11Lines p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offsetMin = m_io.ReadU4le();
                _offsetMax = m_io.ReadU4le();
                _sectionIndex = m_io.ReadU2le();
            }
            private uint _offsetMin;
            private uint _offsetMax;
            private ushort _sectionIndex;
            private MsPdb m_root;
            private MsPdb.C11Lines m_parent;

            /// <remarks>
            /// Reference: offMin
            /// </remarks>
            public uint OffsetMin { get { return _offsetMin; } }

            /// <remarks>
            /// Reference: offMax
            /// </remarks>
            public uint OffsetMax { get { return _offsetMax; } }

            /// <remarks>
            /// Reference: isect
            /// </remarks>
            public ushort SectionIndex { get { return _sectionIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C11Lines M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// page 0
        /// </summary>
        /// <remarks>
        /// Reference: MSF_HDR
        /// </remarks>
        public partial class PdbHeaderJg : KaitaiStruct
        {
            public static PdbHeaderJg FromFile(string fileName)
            {
                return new PdbHeaderJg(new KaitaiStream(fileName));
            }

            public PdbHeaderJg(KaitaiStream p__io, MsPdb.PdbJgRoot p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                __unnamed0 = m_io.ReadBytes(2);
                _pageSize = m_io.ReadU4le();
                _fpmPageNumber = m_io.ReadU2le();
                _numPages = m_io.ReadU2le();
                _directorySize = m_io.ReadU4le();
                _pageMap = m_io.ReadU4le();
            }
            private byte[] __unnamed0;
            private uint _pageSize;
            private ushort _fpmPageNumber;
            private ushort _numPages;
            private uint _directorySize;
            private uint _pageMap;
            private MsPdb m_root;
            private MsPdb.PdbJgRoot m_parent;
            public byte[] Unnamed_0 { get { return __unnamed0; } }

            /// <summary>
            /// page size
            /// </summary>
            public uint PageSize { get { return _pageSize; } }

            /// <summary>
            /// page no. of valid FPM
            /// </summary>
            public ushort FpmPageNumber { get { return _fpmPageNumber; } }

            /// <summary>
            /// current no. of pages
            /// </summary>
            public ushort NumPages { get { return _numPages; } }

            /// <remarks>
            /// Reference: SI_PERSIST.cb
            /// </remarks>
            public uint DirectorySize { get { return _directorySize; } }

            /// <remarks>
            /// Reference: SI_PERSIST.mpspnpn
            /// </remarks>
            public uint PageMap { get { return _pageMap; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbJgRoot M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SI_PERSIST
        /// </remarks>
        public partial class PdbStreamEntryDs : KaitaiStruct
        {
            public PdbStreamEntryDs(int p_streamNumber, KaitaiStream p__io, MsPdb.PdbStreamTable p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _streamNumber = p_streamNumber;
                f_zzzNumDirectoryPages = false;
                f_numDirectoryPages = false;
                _read();
            }
            private void _read()
            {
                _streamSize = m_io.ReadS4le();
            }
            private bool f_zzzNumDirectoryPages;
            private GetNumPages _zzzNumDirectoryPages;
            public GetNumPages ZzzNumDirectoryPages
            {
                get
                {
                    if (f_zzzNumDirectoryPages)
                        return _zzzNumDirectoryPages;
                    _zzzNumDirectoryPages = new GetNumPages(((uint) (StreamSize)), m_io, this, m_root);
                    f_zzzNumDirectoryPages = true;
                    return _zzzNumDirectoryPages;
                }
            }
            private bool f_numDirectoryPages;
            private int _numDirectoryPages;
            public int NumDirectoryPages
            {
                get
                {
                    if (f_numDirectoryPages)
                        return _numDirectoryPages;
                    _numDirectoryPages = (int) ((StreamSize < 0 ? 0 : ZzzNumDirectoryPages.NumPages));
                    f_numDirectoryPages = true;
                    return _numDirectoryPages;
                }
            }
            private int _streamSize;
            private int _streamNumber;
            private MsPdb m_root;
            private MsPdb.PdbStreamTable m_parent;
            public int StreamSize { get { return _streamSize; } }
            public int StreamNumber { get { return _streamNumber; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbStreamTable M_Parent { get { return m_parent; } }
        }
        public partial class PdbMapKvPair : KaitaiStruct
        {
            public PdbMapKvPair(uint p_index, KaitaiStream p__io, MsPdb.PdbMap p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _index = p_index;
                f_keyU4 = false;
                f_valueU4 = false;
                f_isPresent = false;
                _read();
            }
            private void _read()
            {
                if (IsPresent) {
                    _key = m_io.ReadBytes(M_Parent.KeySize);
                }
                if (IsPresent) {
                    _value = m_io.ReadBytes(M_Parent.ValueSize);
                }
            }
            private bool f_keyU4;
            private uint? _keyU4;
            public uint? KeyU4
            {
                get
                {
                    if (f_keyU4)
                        return _keyU4;
                    if ( ((IsPresent) && (M_Parent.KeySize == 4)) ) {
                        long _pos = m_io.Pos;
                        m_io.Seek(0);
                        _keyU4 = m_io.ReadU4le();
                        m_io.Seek(_pos);
                        f_keyU4 = true;
                    }
                    return _keyU4;
                }
            }
            private bool f_valueU4;
            private uint? _valueU4;
            public uint? ValueU4
            {
                get
                {
                    if (f_valueU4)
                        return _valueU4;
                    if ( ((IsPresent) && (M_Parent.ValueSize == 4)) ) {
                        long _pos = m_io.Pos;
                        m_io.Seek(4);
                        _valueU4 = m_io.ReadU4le();
                        m_io.Seek(_pos);
                        f_valueU4 = true;
                    }
                    return _valueU4;
                }
            }
            private bool f_isPresent;
            private bool _isPresent;
            public bool IsPresent
            {
                get
                {
                    if (f_isPresent)
                        return _isPresent;
                    _isPresent = (bool) (M_Parent.AvailableBitset.Values.Bits[((int) (Index))]);
                    f_isPresent = true;
                    return _isPresent;
                }
            }
            private byte[] _key;
            private byte[] _value;
            private uint _index;
            private MsPdb m_root;
            private MsPdb.PdbMap m_parent;
            public byte[] Key { get { return _key; } }
            public byte[] Value { get { return _value; } }
            public uint Index { get { return _index; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbMap M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: lfMethodList
        /// </remarks>
        public partial class LfMethodlist : KaitaiStruct
        {
            public static LfMethodlist FromFile(string fileName)
            {
                return new LfMethodlist(new KaitaiStream(fileName));
            }

            public LfMethodlist(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _methods = new List<MlMethod>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _methods.Add(new MlMethod(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<MlMethod> _methods;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;
            public List<MlMethod> Methods { get { return _methods; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class SectionContributionList : KaitaiStruct
        {
            public static SectionContributionList FromFile(string fileName)
            {
                return new SectionContributionList(new KaitaiStream(fileName));
            }


            public enum VersionType : uint
            {
                V60 = 4046371373,
                New = 4046541284,
            }
            public SectionContributionList(KaitaiStream p__io, MsPdb.Dbi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_version = false;
                f_itemSize = false;
                _read();
            }
            private void _read()
            {
                if ( ((Version == VersionType.V60) || (Version == VersionType.New)) ) {
                    __unnamed0 = m_io.ReadBytes(4);
                }
                _items = new List<KaitaiStruct>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        switch (Version) {
                        case VersionType.V60: {
                            _items.Add(new SectionContrib(m_io, this, m_root));
                            break;
                        }
                        case VersionType.New: {
                            _items.Add(new SectionContrib2(m_io, this, m_root));
                            break;
                        }
                        default: {
                            _items.Add(new SectionContrib40(m_io, this, m_root));
                            break;
                        }
                        }
                        i++;
                    }
                }
            }
            private bool f_version;
            private VersionType _version;
            public VersionType Version
            {
                get
                {
                    if (f_version)
                        return _version;
                    long _pos = m_io.Pos;
                    m_io.Seek(0);
                    _version = ((VersionType) m_io.ReadU4le());
                    m_io.Seek(_pos);
                    f_version = true;
                    return _version;
                }
            }
            private bool f_itemSize;
            private int _itemSize;
            public int ItemSize
            {
                get
                {
                    if (f_itemSize)
                        return _itemSize;
                    _itemSize = (int) ((Version == VersionType.New ? 32 : (Version == VersionType.V60 ? 28 : 20)));
                    f_itemSize = true;
                    return _itemSize;
                }
            }
            private byte[] __unnamed0;
            private List<KaitaiStruct> _items;
            private MsPdb m_root;
            private MsPdb.Dbi m_parent;
            public byte[] Unnamed_0 { get { return __unnamed0; } }
            public List<KaitaiStruct> Items { get { return _items; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Dbi M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: NMT
        /// </remarks>
        public partial class NameTable : KaitaiStruct
        {
            public static NameTable FromFile(string fileName)
            {
                return new NameTable(new KaitaiStream(fileName));
            }


            public enum VersionEnum : uint
            {
                Hash = 1,
                HashV2 = 2,
            }
            public NameTable(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_strings = false;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 254, 239, 254, 239 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 254, 239, 254, 239 }, Magic, M_Io, "/types/name_table/seq/0");
                }
                _version = ((VersionEnum) m_io.ReadU4le());
                _buffer = new PdbBuffer(m_io, this, m_root);
                _indices = new PdbArray(4, m_io, this, m_root);
                _numNames = m_io.ReadU4le();
            }
            private bool f_strings;
            private NameTableStrings _strings;
            public NameTableStrings Strings
            {
                get
                {
                    if (f_strings)
                        return _strings;
                    __raw__raw_strings = m_io.ReadBytes(0);
                    Cat _process__raw__raw_strings = new Cat(Indices.Data);
                    __raw_strings = _process__raw__raw_strings.Decode(__raw__raw_strings);
                    var io___raw_strings = new KaitaiStream(__raw_strings);
                    _strings = new NameTableStrings(io___raw_strings, this, m_root);
                    f_strings = true;
                    return _strings;
                }
            }
            private byte[] _magic;
            private VersionEnum _version;
            private PdbBuffer _buffer;
            private PdbArray _indices;
            private uint _numNames;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            private byte[] __raw_strings;
            private byte[] __raw__raw_strings;

            /// <summary>
            /// verHdr
            /// </summary>
            public byte[] Magic { get { return _magic; } }

            /// <summary>
            /// vhT.ulHdr
            /// </summary>
            public VersionEnum Version { get { return _version; } }

            /// <summary>
            /// buf
            /// </summary>
            public PdbBuffer Buffer { get { return _buffer; } }

            /// <summary>
            /// mphashni
            /// </summary>
            public PdbArray Indices { get { return _indices; } }

            /// <summary>
            /// cni
            /// </summary>
            public uint NumNames { get { return _numNames; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
            public byte[] M_RawStrings { get { return __raw_strings; } }
            public byte[] M_RawM_RawStrings { get { return __raw__raw_strings; } }
        }

        /// <remarks>
        /// Reference: BPRELSYM32
        /// </remarks>
        public partial class SymBprel32 : KaitaiStruct
        {
            public SymBprel32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _type = new TpiTypeRef(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private uint _offset;
            private TpiTypeRef _type;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// BP-relative offset
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// Type index or Metadata token
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: OEMSYMBOL
        /// </remarks>
        public partial class SymOem : KaitaiStruct
        {
            public static SymOem FromFile(string fileName)
            {
                return new SymOem(new KaitaiStream(fileName));
            }

            public SymOem(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _oemId = m_io.ReadBytes(16);
                _type = new TpiTypeRef(m_io, this, m_root);
                _userData = m_io.ReadBytesFull();
            }
            private byte[] _oemId;
            private TpiTypeRef _type;
            private byte[] _userData;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// an oem ID (GUID)
            /// </summary>
            /// <remarks>
            /// Reference: idOem
            /// </remarks>
            public byte[] OemId { get { return _oemId; } }

            /// <summary>
            /// Type index
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// user data, force 4-byte alignment
            /// </summary>
            /// <remarks>
            /// Reference: rgl
            /// </remarks>
            public byte[] UserData { get { return _userData; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: OBJNAMESYM
        /// </remarks>
        public partial class SymObjname : KaitaiStruct
        {
            public SymObjname(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _signature = m_io.ReadU4le();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private uint _signature;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// signature
            /// </summary>
            /// <remarks>
            /// Reference: signature
            /// </remarks>
            public uint Signature { get { return _signature; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_MODIFIER_16t
        /// </summary>
        /// <remarks>
        /// Reference: lfModifier_16t
        /// </remarks>
        public partial class LfModifier16t : KaitaiStruct
        {
            public static LfModifier16t FromFile(string fileName)
            {
                return new LfModifier16t(new KaitaiStream(fileName));
            }

            public LfModifier16t(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _flags = new LfModifierFlags(m_io, this, m_root);
                _modifiedType = new TpiTypeRef16(m_io, this, m_root);
            }
            private LfModifierFlags _flags;
            private TpiTypeRef16 _modifiedType;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// modifier attribute modifier_t
            /// </summary>
            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public LfModifierFlags Flags { get { return _flags; } }

            /// <summary>
            /// modified type
            /// </summary>
            /// <remarks>
            /// Reference: type
            /// </remarks>
            public TpiTypeRef16 ModifiedType { get { return _modifiedType; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: lfUnion
        /// </remarks>
        public partial class LfUnion : KaitaiStruct
        {
            public LfUnion(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _count = m_io.ReadU2le();
                _property = new CvProperties(m_io, this, m_root);
                _field = new TpiTypeRef(m_io, this, m_root);
                _length = new CvNumericType(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private ushort _count;
            private CvProperties _property;
            private TpiTypeRef _field;
            private CvNumericType _length;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// count of number of elements in class
            /// </summary>
            /// <remarks>
            /// Reference: count
            /// </remarks>
            public ushort Count { get { return _count; } }

            /// <summary>
            /// property attribute field
            /// </summary>
            /// <remarks>
            /// Reference: property
            /// </remarks>
            public CvProperties Property { get { return _property; } }

            /// <summary>
            /// type index of LF_FIELD descriptor list
            /// </summary>
            /// <remarks>
            /// Reference: field
            /// </remarks>
            public TpiTypeRef Field { get { return _field; } }

            /// <summary>
            /// variable length data describing length of structure
            /// </summary>
            /// <remarks>
            /// Reference: data.length
            /// </remarks>
            public CvNumericType Length { get { return _length; } }

            /// <summary>
            /// array name
            /// </summary>
            /// <remarks>
            /// Reference: data.name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DATASYMHLSL32_EX
        /// </remarks>
        public partial class SymDataHlsl32Ex : KaitaiStruct
        {
            public static SymDataHlsl32Ex FromFile(string fileName)
            {
                return new SymDataHlsl32Ex(new KaitaiStream(fileName));
            }

            public SymDataHlsl32Ex(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _regId = m_io.ReadU4le();
                _dataOff = m_io.ReadU4le();
                _bindSpace = m_io.ReadU4le();
                _bindSlot = m_io.ReadU4le();
                _regType = m_io.ReadU2le();
                _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
            }
            private TpiTypeRef _type;
            private uint _regId;
            private uint _dataOff;
            private uint _bindSpace;
            private uint _bindSlot;
            private ushort _regType;
            private string _name;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Type index
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// Register index
            /// </summary>
            /// <remarks>
            /// Reference: regID
            /// </remarks>
            public uint RegId { get { return _regId; } }

            /// <summary>
            /// Base data byte offset start
            /// </summary>
            /// <remarks>
            /// Reference: dataoff
            /// </remarks>
            public uint DataOff { get { return _dataOff; } }

            /// <summary>
            /// Binding space
            /// </summary>
            /// <remarks>
            /// Reference: bindSpace
            /// </remarks>
            public uint BindSpace { get { return _bindSpace; } }

            /// <summary>
            /// Lower bound in binding space
            /// </summary>
            /// <remarks>
            /// Reference: bindSlot
            /// </remarks>
            public uint BindSlot { get { return _bindSlot; } }

            /// <summary>
            /// register type from CV_HLSLREG_e
            /// </summary>
            /// <remarks>
            /// Reference: regType
            /// </remarks>
            public ushort RegType { get { return _regType; } }

            /// <summary>
            /// name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public string Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class C13SubsectionIgnore : KaitaiStruct
        {
            public static C13SubsectionIgnore FromFile(string fileName)
            {
                return new C13SubsectionIgnore(new KaitaiStream(fileName));
            }

            public C13SubsectionIgnore(KaitaiStream p__io, MsPdb.C13Subsection p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _data = m_io.ReadBytesFull();
            }
            private byte[] _data;
            private MsPdb m_root;
            private MsPdb.C13Subsection m_parent;
            public byte[] Data { get { return _data; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13Subsection M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: PROCSYM32
        /// </remarks>
        public partial class SymProc32 : KaitaiStruct
        {
            public SymProc32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _parent = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _end = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _next = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _length = m_io.ReadU4le();
                _dbgStart = m_io.ReadU4le();
                _dbgEnd = m_io.ReadU4le();
                _type = new TpiTypeRef(m_io, this, m_root);
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _flags = new CvProcFlags(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private DbiSymbolRef _parent;
            private DbiSymbolRef _end;
            private DbiSymbolRef _next;
            private uint _length;
            private uint _dbgStart;
            private uint _dbgEnd;
            private TpiTypeRef _type;
            private uint _offset;
            private ushort _segment;
            private CvProcFlags _flags;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// pointer to the parent
            /// </summary>
            /// <remarks>
            /// Reference: pParent
            /// </remarks>
            public DbiSymbolRef Parent { get { return _parent; } }

            /// <summary>
            /// pointer to this blocks end
            /// </summary>
            /// <remarks>
            /// Reference: pEnd
            /// </remarks>
            public DbiSymbolRef End { get { return _end; } }

            /// <summary>
            /// pointer to next symbol
            /// </summary>
            /// <remarks>
            /// Reference: pNext
            /// </remarks>
            public DbiSymbolRef Next { get { return _next; } }

            /// <summary>
            /// Proc length
            /// </summary>
            /// <remarks>
            /// Reference: len
            /// </remarks>
            public uint Length { get { return _length; } }

            /// <summary>
            /// Debug start offset
            /// </summary>
            /// <remarks>
            /// Reference: DbgStart
            /// </remarks>
            public uint DbgStart { get { return _dbgStart; } }

            /// <summary>
            /// Debug end offset
            /// </summary>
            /// <remarks>
            /// Reference: DbgEnd
            /// </remarks>
            public uint DbgEnd { get { return _dbgEnd; } }

            /// <summary>
            /// Type index or ID
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public CvProcFlags Flags { get { return _flags; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class TpiHashHeadList : KaitaiStruct
        {
            public static TpiHashHeadList FromFile(string fileName)
            {
                return new TpiHashHeadList(new KaitaiStream(fileName));
            }

            public TpiHashHeadList(KaitaiStream p__io, MsPdb.TpiHashData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _nameToTypeIndex = new PdbMap(4, 4, m_io, this, m_root);
            }
            private PdbMap _nameToTypeIndex;
            private MsPdb m_root;
            private MsPdb.TpiHashData m_parent;
            public PdbMap NameToTypeIndex { get { return _nameToTypeIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiHashData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_POINTER
        /// </summary>
        /// <remarks>
        /// Reference: lfPointer
        /// </remarks>
        public partial class LfPointer : KaitaiStruct
        {
            public static LfPointer FromFile(string fileName)
            {
                return new LfPointer(new KaitaiStream(fileName));
            }

            public LfPointer(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _underlyingType = new TpiTypeRef(m_io, this, m_root);
                _attributes = new LfPointerAttributes(m_io, this, m_root);
            }
            private TpiTypeRef _underlyingType;
            private LfPointerAttributes _attributes;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// type index of the underlying type
            /// </summary>
            /// <remarks>
            /// Reference: utype
            /// </remarks>
            public TpiTypeRef UnderlyingType { get { return _underlyingType; } }

            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public LfPointerAttributes Attributes { get { return _attributes; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class GetStreamSize : KaitaiStruct
        {
            public GetStreamSize(uint p_streamNumber, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _streamNumber = p_streamNumber;
                f_value = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_value;
            private int _value;
            public int Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    _value = (int) ((M_Root.PdbType == MsPdb.PdbTypeEnum.Big ? ((int) (M_Root.PdbDs.StreamTable.StreamSizesDs[((int) (StreamNumber))].StreamSize)) : ((int) (M_Root.PdbJg.StreamTable.StreamSizesJg[((int) (StreamNumber))].StreamSize))));
                    f_value = true;
                    return _value;
                }
            }
            private uint _streamNumber;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint StreamNumber { get { return _streamNumber; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: ECInfo
        /// </remarks>
        public partial class EcInfo : KaitaiStruct
        {
            public static EcInfo FromFile(string fileName)
            {
                return new EcInfo(new KaitaiStream(fileName));
            }

            public EcInfo(KaitaiStream p__io, MsPdb.ModuleInfo p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _srcFilenameIndex = m_io.ReadU4le();
                _pdbFilenameIndex = m_io.ReadU4le();
            }
            private uint _srcFilenameIndex;
            private uint _pdbFilenameIndex;
            private MsPdb m_root;
            private MsPdb.ModuleInfo m_parent;

            /// <summary>
            /// NI for src file name
            /// </summary>
            /// <remarks>
            /// Reference: niSrcFile
            /// </remarks>
            public uint SrcFilenameIndex { get { return _srcFilenameIndex; } }

            /// <summary>
            /// NI for path to compiler PDB
            /// </summary>
            /// <remarks>
            /// Reference: niPdbFile
            /// </remarks>
            public uint PdbFilenameIndex { get { return _pdbFilenameIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.ModuleInfo M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: THREADSYM32
        /// </remarks>
        public partial class SymThread32 : KaitaiStruct
        {
            public SymThread32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private TpiTypeRef _type;
            private uint _offset;
            private ushort _segment;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// type index
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// offset into thread storage
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// segment of thread storage
            /// </summary>
            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <summary>
            /// length prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DEFRANGESYMREGISTERREL
        /// </remarks>
        public partial class SymDefrangeRegisterRel : KaitaiStruct
        {
            public static SymDefrangeRegisterRel FromFile(string fileName)
            {
                return new SymDefrangeRegisterRel(new KaitaiStream(fileName));
            }

            public SymDefrangeRegisterRel(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _baseRegister = m_io.ReadU2le();
                _spilledUdtMember = m_io.ReadBitsIntLe(1) != 0;
                __unnamed2 = m_io.ReadBitsIntLe(3);
                _offsetMember = m_io.ReadBitsIntLe(12);
                m_io.AlignToByte();
                _basePointerOffset = m_io.ReadU4le();
                _range = new CvLvarAddrRange(m_io, this, m_root);
                _gaps = new List<CvLvarAddrGap>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _gaps.Add(new CvLvarAddrGap(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private ushort _baseRegister;
            private bool _spilledUdtMember;
            private ulong __unnamed2;
            private ulong _offsetMember;
            private uint _basePointerOffset;
            private CvLvarAddrRange _range;
            private List<CvLvarAddrGap> _gaps;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Register to hold the base pointer of the symbol
            /// </summary>
            /// <remarks>
            /// Reference: baseReg
            /// </remarks>
            public ushort BaseRegister { get { return _baseRegister; } }

            /// <summary>
            /// Spilled member for s.i.
            /// </summary>
            /// <remarks>
            /// Reference: spilledUdtMember
            /// </remarks>
            public bool SpilledUdtMember { get { return _spilledUdtMember; } }

            /// <summary>
            /// Padding for future use.
            /// </summary>
            /// <remarks>
            /// Reference: padding
            /// </remarks>
            public ulong Unnamed_2 { get { return __unnamed2; } }

            /// <summary>
            /// Offset in parent variable.
            /// </summary>
            /// <remarks>
            /// Reference: offsetParent
            /// </remarks>
            public ulong OffsetMember { get { return _offsetMember; } }

            /// <summary>
            /// offset to register
            /// </summary>
            /// <remarks>
            /// Reference: offBasePointer
            /// </remarks>
            public uint BasePointerOffset { get { return _basePointerOffset; } }

            /// <summary>
            /// Range of addresses where this program is valid
            /// </summary>
            /// <remarks>
            /// Reference: range
            /// </remarks>
            public CvLvarAddrRange Range { get { return _range; } }

            /// <summary>
            /// The value is not available in following gaps.
            /// </summary>
            /// <remarks>
            /// Reference: gaps
            /// </remarks>
            public List<CvLvarAddrGap> Gaps { get { return _gaps; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SrcFile
        /// </remarks>
        public partial class C11Srcfile : KaitaiStruct
        {
            public static C11Srcfile FromFile(string fileName)
            {
                return new C11Srcfile(new KaitaiStream(fileName));
            }

            public C11Srcfile(KaitaiStream p__io, MsPdb.C11Lines p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_alignMarker = false;
                f_zzzAlignSize = false;
                f_paddingSize = false;
                _read();
            }
            private void _read()
            {
                _segPadBase = new C11LinebufferSegPadBase(m_io, this, m_root);
                _startEnd = new List<C11LinebufferStartEnd>();
                for (var i = 0; i < SegPadBase.NumSegments; i++)
                {
                    _startEnd.Add(new C11LinebufferStartEnd(m_io, this, m_root));
                }
                _filename = new PdbString(M_Root.PdbRootStream.HasNullTerminatedStrings == false, m_io, this, m_root);
                if (AlignMarker >= 0) {
                    _invokeAlignMarker = m_io.ReadBytes(0);
                }
                __unnamed4 = m_io.ReadBytes(PaddingSize);
                _sectionInfo = new List<C11SectionInfo2>();
                for (var i = 0; i < SegPadBase.NumSegments; i++)
                {
                    _sectionInfo.Add(new C11SectionInfo2(m_io, this, m_root));
                }
            }
            private bool f_alignMarker;
            private int _alignMarker;
            public int AlignMarker
            {
                get
                {
                    if (f_alignMarker)
                        return _alignMarker;
                    _alignMarker = (int) (M_Io.Pos);
                    f_alignMarker = true;
                    return _alignMarker;
                }
            }
            private bool f_zzzAlignSize;
            private Align _zzzAlignSize;
            public Align ZzzAlignSize
            {
                get
                {
                    if (f_zzzAlignSize)
                        return _zzzAlignSize;
                    _zzzAlignSize = new Align(((uint) (AlignMarker)), 4, m_io, this, m_root);
                    f_zzzAlignSize = true;
                    return _zzzAlignSize;
                }
            }
            private bool f_paddingSize;
            private int _paddingSize;
            public int PaddingSize
            {
                get
                {
                    if (f_paddingSize)
                        return _paddingSize;
                    _paddingSize = (int) (ZzzAlignSize.PaddingSize);
                    f_paddingSize = true;
                    return _paddingSize;
                }
            }
            private C11LinebufferSegPadBase _segPadBase;
            private List<C11LinebufferStartEnd> _startEnd;
            private PdbString _filename;
            private byte[] _invokeAlignMarker;
            private byte[] __unnamed4;
            private List<C11SectionInfo2> _sectionInfo;
            private MsPdb m_root;
            private MsPdb.C11Lines m_parent;
            public C11LinebufferSegPadBase SegPadBase { get { return _segPadBase; } }
            public List<C11LinebufferStartEnd> StartEnd { get { return _startEnd; } }

            /// <remarks>
            /// Reference: szFile
            /// </remarks>
            public PdbString Filename { get { return _filename; } }
            public byte[] InvokeAlignMarker { get { return _invokeAlignMarker; } }
            public byte[] Unnamed_4 { get { return __unnamed4; } }
            public List<C11SectionInfo2> SectionInfo { get { return _sectionInfo; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C11Lines M_Parent { get { return m_parent; } }
        }
        public partial class C13SubsectionLines : KaitaiStruct
        {
            public static C13SubsectionLines FromFile(string fileName)
            {
                return new C13SubsectionLines(new KaitaiStream(fileName));
            }

            public C13SubsectionLines(KaitaiStream p__io, MsPdb.C13Subsection p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_haveColumns = false;
                _read();
            }
            private void _read()
            {
                _contentsOffset = m_io.ReadU4le();
                _contentsSegment = m_io.ReadU2le();
                _flags = m_io.ReadU2le();
                _contentsSize = m_io.ReadU4le();
                _fileBlocks = new List<C13FileBlock>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _fileBlocks.Add(new C13FileBlock(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private bool f_haveColumns;
            private bool _haveColumns;
            public bool HaveColumns
            {
                get
                {
                    if (f_haveColumns)
                        return _haveColumns;
                    _haveColumns = (bool) ((Flags & 1) == 1);
                    f_haveColumns = true;
                    return _haveColumns;
                }
            }
            private uint _contentsOffset;
            private ushort _contentsSegment;
            private ushort _flags;
            private uint _contentsSize;
            private List<C13FileBlock> _fileBlocks;
            private MsPdb m_root;
            private MsPdb.C13Subsection m_parent;
            public uint ContentsOffset { get { return _contentsOffset; } }
            public ushort ContentsSegment { get { return _contentsSegment; } }
            public ushort Flags { get { return _flags; } }
            public uint ContentsSize { get { return _contentsSize; } }
            public List<C13FileBlock> FileBlocks { get { return _fileBlocks; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13Subsection M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SE
        /// </remarks>
        public partial class C11LinebufferStartEnd : KaitaiStruct
        {
            public static C11LinebufferStartEnd FromFile(string fileName)
            {
                return new C11LinebufferStartEnd(new KaitaiStream(fileName));
            }

            public C11LinebufferStartEnd(KaitaiStream p__io, MsPdb.C11Srcfile p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _start = m_io.ReadU4le();
                _end = m_io.ReadU4le();
            }
            private uint _start;
            private uint _end;
            private MsPdb m_root;
            private MsPdb.C11Srcfile m_parent;

            /// <remarks>
            /// Reference: start
            /// </remarks>
            public uint Start { get { return _start; } }

            /// <remarks>
            /// Reference: end
            /// </remarks>
            public uint End { get { return _end; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C11Srcfile M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: TRAMPOLINESYM
        /// </remarks>
        public partial class SymTrampoline : KaitaiStruct
        {
            public static SymTrampoline FromFile(string fileName)
            {
                return new SymTrampoline(new KaitaiStream(fileName));
            }

            public SymTrampoline(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _trampolineType = m_io.ReadU2le();
                _thunkSize = m_io.ReadU2le();
                _thunkOffset = m_io.ReadU4le();
                _thunkTargetOffset = m_io.ReadU4le();
                _thunkSectionIndex = m_io.ReadU2le();
                _thunkTargetSectionIndex = m_io.ReadU2le();
            }
            private ushort _trampolineType;
            private ushort _thunkSize;
            private uint _thunkOffset;
            private uint _thunkTargetOffset;
            private ushort _thunkSectionIndex;
            private ushort _thunkTargetSectionIndex;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// trampoline sym subtype
            /// </summary>
            /// <remarks>
            /// Reference: trampType
            /// </remarks>
            public ushort TrampolineType { get { return _trampolineType; } }

            /// <summary>
            /// size of the thunk
            /// </summary>
            /// <remarks>
            /// Reference: cbThunk
            /// </remarks>
            public ushort ThunkSize { get { return _thunkSize; } }

            /// <summary>
            /// offset of the thunk
            /// </summary>
            /// <remarks>
            /// Reference: offThunk
            /// </remarks>
            public uint ThunkOffset { get { return _thunkOffset; } }

            /// <summary>
            /// offset of the target of the thunk
            /// </summary>
            /// <remarks>
            /// Reference: offTarget
            /// </remarks>
            public uint ThunkTargetOffset { get { return _thunkTargetOffset; } }

            /// <summary>
            /// section index of the thunk
            /// </summary>
            /// <remarks>
            /// Reference: sectThunk
            /// </remarks>
            public ushort ThunkSectionIndex { get { return _thunkSectionIndex; } }

            /// <summary>
            /// section index of the target of the thunk
            /// </summary>
            /// <remarks>
            /// Reference: sectTarget
            /// </remarks>
            public ushort ThunkTargetSectionIndex { get { return _thunkTargetSectionIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class SrcHeaderBlockStream : KaitaiStruct
        {
            public static SrcHeaderBlockStream FromFile(string fileName)
            {
                return new SrcHeaderBlockStream(new KaitaiStream(fileName));
            }

            public SrcHeaderBlockStream(KaitaiStream p__io, MsPdb.PdbNamedStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                __unnamed0 = m_io.ReadBytes(0);
            }
            private byte[] __unnamed0;
            private MsPdb m_root;
            private MsPdb.PdbNamedStream m_parent;
            public byte[] Unnamed_0 { get { return __unnamed0; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbNamedStream M_Parent { get { return m_parent; } }
        }
        public partial class TpiTypeData : KaitaiStruct
        {
            public TpiTypeData(bool p_nested, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _nested = p_nested;
                f_trailingByte = false;
                f_hasPadding = false;
                f_paddingSize = false;
                f_endBodyPos = false;
                _read();
            }
            private void _read()
            {
                _type = ((MsPdb.Tpi.LeafType) m_io.ReadU2le());
                switch (Type) {
                case MsPdb.Tpi.LeafType.LfEnumSt: {
                    _body = new LfEnum(true, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfProcedure16t: {
                    _body = new LfProcedure16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfEnumerate: {
                    _body = new LfEnumerate(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfFieldlist: {
                    _body = new LfFieldlist(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfPointer16t: {
                    _body = new LfPointer16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfArglist16t: {
                    _body = new LfArglist16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfEnum: {
                    _body = new LfEnum(false, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfClassSt: {
                    _body = new LfClass(true, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfStructure16t: {
                    _body = new LfClass16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfMember: {
                    _body = new LfMember(false, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfStructure: {
                    _body = new LfClass(false, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfVtshape: {
                    _body = new LfVtshape(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfArraySt: {
                    _body = new LfArray(true, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfBitfield: {
                    _body = new LfBitfield(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfStructureSt: {
                    _body = new LfClass(true, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfMethodlist16t: {
                    _body = new LfMethodlist16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfClass: {
                    _body = new LfClass(false, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfClass16t: {
                    _body = new LfClass16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfFieldlist16t: {
                    _body = new LfFieldlist16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfUnion: {
                    _body = new LfUnion(false, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfArray16t: {
                    _body = new LfArray16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfMethodlist: {
                    _body = new LfMethodlist(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfLabel: {
                    _body = new LfLabel(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfVftable: {
                    _body = new LfVftable(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfUnionSt: {
                    _body = new LfUnion(true, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfModifier: {
                    _body = new LfModifier(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfModifier16t: {
                    _body = new LfModifier16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfMfunction16t: {
                    _body = new LfMfunction16t(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfMemberSt: {
                    _body = new LfMember(true, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfArglist: {
                    _body = new LfArglist(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfMfunction: {
                    _body = new LfMfunction(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfArray: {
                    _body = new LfArray(false, m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfOneMethod: {
                    _body = new LfOneMethod(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfProcedure: {
                    _body = new LfProcedure(m_io, this, m_root);
                    break;
                }
                case MsPdb.Tpi.LeafType.LfPointer: {
                    _body = new LfPointer(m_io, this, m_root);
                    break;
                }
                default: {
                    _body = new LfUnknown(m_io, this, m_root);
                    break;
                }
                }
                if (EndBodyPos >= 0) {
                    _invokeEndBody = m_io.ReadBytes(0);
                }
                if (Nested == false) {
                    _unparsedData = m_io.ReadBytesFull();
                }
                if (Nested == true) {
                    __unnamed4 = m_io.ReadBytes(PaddingSize);
                }
            }
            private bool f_trailingByte;
            private byte? _trailingByte;
            public byte? TrailingByte
            {
                get
                {
                    if (f_trailingByte)
                        return _trailingByte;
                    if (EndBodyPos < M_Io.Size) {
                        long _pos = m_io.Pos;
                        m_io.Seek(EndBodyPos);
                        _trailingByte = m_io.ReadU1();
                        m_io.Seek(_pos);
                        f_trailingByte = true;
                    }
                    return _trailingByte;
                }
            }
            private bool f_hasPadding;
            private bool _hasPadding;
            public bool HasPadding
            {
                get
                {
                    if (f_hasPadding)
                        return _hasPadding;
                    _hasPadding = (bool) ( ((TrailingByte >= ((byte) (MsPdb.Tpi.LeafType.LfPad1))) && (TrailingByte <= ((byte) (MsPdb.Tpi.LeafType.LfPad15)))) );
                    f_hasPadding = true;
                    return _hasPadding;
                }
            }
            private bool f_paddingSize;
            private int _paddingSize;
            public int PaddingSize
            {
                get
                {
                    if (f_paddingSize)
                        return _paddingSize;
                    _paddingSize = (int) ((HasPadding ? (TrailingByte & 15) : 0));
                    f_paddingSize = true;
                    return _paddingSize;
                }
            }
            private bool f_endBodyPos;
            private int _endBodyPos;
            public int EndBodyPos
            {
                get
                {
                    if (f_endBodyPos)
                        return _endBodyPos;
                    _endBodyPos = (int) (M_Io.Pos);
                    f_endBodyPos = true;
                    return _endBodyPos;
                }
            }
            private Tpi.LeafType _type;
            private KaitaiStruct _body;
            private byte[] _invokeEndBody;
            private byte[] _unparsedData;
            private byte[] __unnamed4;
            private bool _nested;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public Tpi.LeafType Type { get { return _type; } }
            public KaitaiStruct Body { get { return _body; } }
            public byte[] InvokeEndBody { get { return _invokeEndBody; } }
            public byte[] UnparsedData { get { return _unparsedData; } }
            public byte[] Unnamed_4 { get { return __unnamed4; } }
            public bool Nested { get { return _nested; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: lfProc_16t
        /// </remarks>
        public partial class LfProcedure16t : KaitaiStruct
        {
            public static LfProcedure16t FromFile(string fileName)
            {
                return new LfProcedure16t(new KaitaiStream(fileName));
            }

            public LfProcedure16t(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _returnValueType = new TpiTypeRef16(m_io, this, m_root);
                _callingConvention = ((MsPdb.Tpi.CallingConvention) m_io.ReadU1());
                _functionAttributes = new CvFuncAttributes(m_io, this, m_root);
                _parameterCount = m_io.ReadU2le();
                _arglist = new TpiTypeRef16(m_io, this, m_root);
            }
            private TpiTypeRef16 _returnValueType;
            private Tpi.CallingConvention _callingConvention;
            private CvFuncAttributes _functionAttributes;
            private ushort _parameterCount;
            private TpiTypeRef16 _arglist;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// type index of return value
            /// </summary>
            /// <remarks>
            /// Reference: rvtype
            /// </remarks>
            public TpiTypeRef16 ReturnValueType { get { return _returnValueType; } }

            /// <summary>
            /// calling convention (CV_call_t)
            /// </summary>
            /// <remarks>
            /// Reference: calltype
            /// </remarks>
            public Tpi.CallingConvention CallingConvention { get { return _callingConvention; } }

            /// <summary>
            /// attributes
            /// </summary>
            /// <remarks>
            /// Reference: funcattr
            /// </remarks>
            public CvFuncAttributes FunctionAttributes { get { return _functionAttributes; } }

            /// <summary>
            /// number of parameters
            /// </summary>
            /// <remarks>
            /// Reference: parmcount
            /// </remarks>
            public ushort ParameterCount { get { return _parameterCount; } }

            /// <summary>
            /// type index of argument list
            /// </summary>
            /// <remarks>
            /// Reference: arglist
            /// </remarks>
            public TpiTypeRef16 Arglist { get { return _arglist; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_ARGLIST_16t
        /// </summary>
        /// <remarks>
        /// Reference: lfArgList_16t
        /// </remarks>
        public partial class LfArglist16t : KaitaiStruct
        {
            public static LfArglist16t FromFile(string fileName)
            {
                return new LfArglist16t(new KaitaiStream(fileName));
            }

            public LfArglist16t(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _count = m_io.ReadU2le();
                _arguments = new List<TpiTypeRef16>();
                for (var i = 0; i < Count; i++)
                {
                    _arguments.Add(new TpiTypeRef16(m_io, this, m_root));
                }
            }
            private ushort _count;
            private List<TpiTypeRef16> _arguments;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// number of arguments
            /// </summary>
            /// <remarks>
            /// Reference: count
            /// </remarks>
            public ushort Count { get { return _count; } }

            /// <summary>
            /// argument types
            /// </summary>
            /// <remarks>
            /// Reference: arg
            /// </remarks>
            public List<TpiTypeRef16> Arguments { get { return _arguments; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class PdbMapNamedStreams : KaitaiStruct
        {
            public static PdbMapNamedStreams FromFile(string fileName)
            {
                return new PdbMapNamedStreams(new KaitaiStream(fileName));
            }

            public PdbMapNamedStreams(KaitaiStream p__io, MsPdb.NameTableNi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _map = new PdbMap(4, 4, m_io, this, m_root);
                _namedStreams = new List<PdbNamedStream>();
                for (var i = 0; i < Map.NumElements; i++)
                {
                    _namedStreams.Add(new PdbNamedStream(((uint) (i)), m_io, this, m_root));
                }
            }
            private PdbMap _map;
            private List<PdbNamedStream> _namedStreams;
            private MsPdb m_root;
            private MsPdb.NameTableNi m_parent;
            public PdbMap Map { get { return _map; } }
            public List<PdbNamedStream> NamedStreams { get { return _namedStreams; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.NameTableNi M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: ENVBLOCKSYM
        /// </remarks>
        public partial class SymEnvblock : KaitaiStruct
        {
            public static SymEnvblock FromFile(string fileName)
            {
                return new SymEnvblock(new KaitaiStream(fileName));
            }

            public SymEnvblock(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _flags = new SymEnvblockFlags(m_io, this, m_root);
                _strings = new List<string>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _strings.Add(System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true)));
                        i++;
                    }
                }
            }
            private SymEnvblockFlags _flags;
            private List<string> _strings;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public SymEnvblockFlags Flags { get { return _flags; } }

            /// <summary>
            /// Sequence of zero-terminated strings
            /// </summary>
            /// <remarks>
            /// Reference: rgsz
            /// </remarks>
            public List<string> Strings { get { return _strings; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DEFRANGESYMFRAMEPOINTERREL
        /// </remarks>
        public partial class SymDefrangeFramepointerRel : KaitaiStruct
        {
            public SymDefrangeFramepointerRel(bool p_fullScope, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _fullScope = p_fullScope;
                _read();
            }
            private void _read()
            {
                _framePointerOffset = m_io.ReadU4le();
                if (FullScope == false) {
                    _range = new CvLvarAddrRange(m_io, this, m_root);
                }
                if (FullScope == false) {
                    _gaps = new List<CvLvarAddrGap>();
                    {
                        var i = 0;
                        while (!m_io.IsEof) {
                            _gaps.Add(new CvLvarAddrGap(m_io, this, m_root));
                            i++;
                        }
                    }
                }
            }
            private uint _framePointerOffset;
            private CvLvarAddrRange _range;
            private List<CvLvarAddrGap> _gaps;
            private bool _fullScope;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// offset to frame pointer
            /// </summary>
            /// <remarks>
            /// Reference: offFramePointer
            /// </remarks>
            public uint FramePointerOffset { get { return _framePointerOffset; } }

            /// <summary>
            /// Range of addresses where this program is valid
            /// </summary>
            /// <remarks>
            /// Reference: range
            /// </remarks>
            public CvLvarAddrRange Range { get { return _range; } }

            /// <summary>
            /// The value is not available in following gaps. 
            /// </summary>
            /// <remarks>
            /// Reference: gaps
            /// </remarks>
            public List<CvLvarAddrGap> Gaps { get { return _gaps; } }

            /// <remarks>
            /// Reference: DEFRANGESYMFRAMEPOINTERREL_FULL_SCOPE
            /// </remarks>
            public bool FullScope { get { return _fullScope; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_ONEMETHOD
        /// </summary>
        /// <remarks>
        /// Reference: lfOneMethod
        /// </remarks>
        public partial class LfOneMethod : KaitaiStruct
        {
            public static LfOneMethod FromFile(string fileName)
            {
                return new LfOneMethod(new KaitaiStream(fileName));
            }

            public LfOneMethod(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _attributes = new CvFieldAttributes(m_io, this, m_root);
                _procedureType = new TpiTypeRef(m_io, this, m_root);
                if ( ((Attributes.MethodProperties == MsPdb.Tpi.CvMethodprop.Intro) || (Attributes.MethodProperties == MsPdb.Tpi.CvMethodprop.PureIntro)) ) {
                    _vtableOffset = m_io.ReadU4le();
                }
                _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
            }
            private CvFieldAttributes _attributes;
            private TpiTypeRef _procedureType;
            private uint? _vtableOffset;
            private string _name;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// method attribute
            /// </summary>
            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public CvFieldAttributes Attributes { get { return _attributes; } }

            /// <summary>
            /// index to type record for procedure
            /// </summary>
            /// <remarks>
            /// Reference: index
            /// </remarks>
            public TpiTypeRef ProcedureType { get { return _procedureType; } }

            /// <summary>
            /// offset in vfunctable if intro virtual
            /// </summary>
            /// <remarks>
            /// Reference: vbaseoff
            /// </remarks>
            public uint? VtableOffset { get { return _vtableOffset; } }

            /// <summary>
            /// length prefixed name of method
            /// </summary>
            public string Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// file checksums
        /// </summary>
        /// <remarks>
        /// Reference: DEBUG_S_FILECHKSMS
        /// </remarks>
        public partial class C13SubsectionFilechecksums : KaitaiStruct
        {
            public static C13SubsectionFilechecksums FromFile(string fileName)
            {
                return new C13SubsectionFilechecksums(new KaitaiStream(fileName));
            }

            public C13SubsectionFilechecksums(KaitaiStream p__io, MsPdb.C13Subsection p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _checksums = new List<C13FileChecksum>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _checksums.Add(new C13FileChecksum(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<C13FileChecksum> _checksums;
            private MsPdb m_root;
            private MsPdb.C13Subsection m_parent;
            public List<C13FileChecksum> Checksums { get { return _checksums; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13Subsection M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_CLASS_16t, LF_STRUCT_16t
        /// </summary>
        /// <remarks>
        /// Reference: lfClass_16t
        /// </remarks>
        public partial class LfClass16t : KaitaiStruct
        {
            public static LfClass16t FromFile(string fileName)
            {
                return new LfClass16t(new KaitaiStream(fileName));
            }

            public LfClass16t(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _numberOfElements = m_io.ReadU2le();
                _fieldType = new TpiTypeRef16(m_io, this, m_root);
                _properties = new CvProperties(m_io, this, m_root);
                _derivedType = new TpiTypeRef16(m_io, this, m_root);
                _vshapeType = new TpiTypeRef16(m_io, this, m_root);
                _structSize = new CvNumericType(m_io, this, m_root);
                _name = new PdbString(true, m_io, this, m_root);
            }
            private ushort _numberOfElements;
            private TpiTypeRef16 _fieldType;
            private CvProperties _properties;
            private TpiTypeRef16 _derivedType;
            private TpiTypeRef16 _vshapeType;
            private CvNumericType _structSize;
            private PdbString _name;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// count of number of elements in class
            /// </summary>
            /// <remarks>
            /// Reference: count
            /// </remarks>
            public ushort NumberOfElements { get { return _numberOfElements; } }

            /// <summary>
            /// type index of LF_FIELD descriptor list
            /// </summary>
            /// <remarks>
            /// Reference: field
            /// </remarks>
            public TpiTypeRef16 FieldType { get { return _fieldType; } }

            /// <summary>
            /// property attribute field (prop_t)
            /// </summary>
            /// <remarks>
            /// Reference: property
            /// </remarks>
            public CvProperties Properties { get { return _properties; } }

            /// <summary>
            /// type index of derived from list if not zero
            /// </summary>
            /// <remarks>
            /// Reference: derived
            /// </remarks>
            public TpiTypeRef16 DerivedType { get { return _derivedType; } }

            /// <summary>
            /// type index of vshape table for this class
            /// </summary>
            /// <remarks>
            /// Reference: vshape
            /// </remarks>
            public TpiTypeRef16 VshapeType { get { return _vshapeType; } }

            /// <summary>
            /// data describing length of structure in bytes
            /// </summary>
            /// <remarks>
            /// Reference: data.size
            /// </remarks>
            public CvNumericType StructSize { get { return _structSize; } }

            /// <summary>
            /// class name
            /// </summary>
            /// <remarks>
            /// Reference: data.name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: tagInlineeSourceLine
        /// </remarks>
        public partial class C13InlineeSourceLine : KaitaiStruct
        {
            public static C13InlineeSourceLine FromFile(string fileName)
            {
                return new C13InlineeSourceLine(new KaitaiStream(fileName));
            }

            public C13InlineeSourceLine(KaitaiStream p__io, MsPdb.C13SubsectionInlineeLines p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _inlinee = m_io.ReadU4le();
                _fileId = m_io.ReadU4le();
                _sourceLineNumber = m_io.ReadU4le();
            }
            private uint _inlinee;
            private uint _fileId;
            private uint _sourceLineNumber;
            private MsPdb m_root;
            private MsPdb.C13SubsectionInlineeLines m_parent;

            /// <summary>
            /// function id.
            /// </summary>
            /// <remarks>
            /// Reference: inlinee
            /// </remarks>
            public uint Inlinee { get { return _inlinee; } }

            /// <summary>
            /// offset into file table DEBUG_S_FILECHKSMS
            /// </summary>
            /// <remarks>
            /// Reference: fileId
            /// </remarks>
            public uint FileId { get { return _fileId; } }

            /// <summary>
            /// definition start line number.
            /// </summary>
            /// <remarks>
            /// Reference: sourceLineNum
            /// </remarks>
            public uint SourceLineNumber { get { return _sourceLineNumber; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13SubsectionInlineeLines M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SC40
        /// </remarks>
        public partial class SectionContrib40 : KaitaiStruct
        {
            public static SectionContrib40 FromFile(string fileName)
            {
                return new SectionContrib40(new KaitaiStream(fileName));
            }

            public SectionContrib40(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _sectionIndex = m_io.ReadU2le();
                _pad0 = m_io.ReadU2le();
                _offset = m_io.ReadU4le();
                _size = m_io.ReadU4le();
                _characteristics = m_io.ReadU4le();
                _moduleIndex = m_io.ReadU2le();
                _pad1 = m_io.ReadU2le();
            }
            private ushort _sectionIndex;
            private ushort _pad0;
            private uint _offset;
            private uint _size;
            private uint _characteristics;
            private ushort _moduleIndex;
            private ushort _pad1;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <remarks>
            /// Reference: isect
            /// </remarks>
            public ushort SectionIndex { get { return _sectionIndex; } }
            public ushort Pad0 { get { return _pad0; } }

            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <remarks>
            /// Reference: cb
            /// </remarks>
            public uint Size { get { return _size; } }

            /// <remarks>
            /// Reference: dwCharacteristics
            /// </remarks>
            public uint Characteristics { get { return _characteristics; } }

            /// <remarks>
            /// Reference: imod
            /// </remarks>
            public ushort ModuleIndex { get { return _moduleIndex; } }
            public ushort Pad1 { get { return _pad1; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class DebugSectionHdrStream : KaitaiStruct
        {
            public static DebugSectionHdrStream FromFile(string fileName)
            {
                return new DebugSectionHdrStream(new KaitaiStream(fileName));
            }

            public DebugSectionHdrStream(KaitaiStream p__io, MsPdb.DebugData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _hdr = new List<ImageSectionHeader>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _hdr.Add(new ImageSectionHeader(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<ImageSectionHeader> _hdr;
            private MsPdb m_root;
            private MsPdb.DebugData m_parent;
            public List<ImageSectionHeader> Hdr { get { return _hdr; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DebugData M_Parent { get { return m_parent; } }
        }
        public partial class TpiHashData : KaitaiStruct
        {
            public static TpiHashData FromFile(string fileName)
            {
                return new TpiHashData(new KaitaiStream(fileName));
            }

            public TpiHashData(KaitaiStream p__io, MsPdb.TpiHash p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_hashValues = false;
                f_tiOffsetList = false;
                f_hashHeadList = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_hashValues;
            private byte[] _hashValues;
            public byte[] HashValues
            {
                get
                {
                    if (f_hashValues)
                        return _hashValues;
                    if (M_Parent.HashValuesSlice.Size > 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(M_Parent.HashValuesSlice.Offset);
                        _hashValues = m_io.ReadBytes(M_Parent.HashValuesSlice.Size);
                        m_io.Seek(_pos);
                        f_hashValues = true;
                    }
                    return _hashValues;
                }
            }
            private bool f_tiOffsetList;
            private TiOffsetList _tiOffsetList;
            public TiOffsetList TiOffsetList
            {
                get
                {
                    if (f_tiOffsetList)
                        return _tiOffsetList;
                    if (M_Parent.TypeOffsetsSlice.Size > 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(M_Parent.TypeOffsetsSlice.Offset);
                        __raw_tiOffsetList = m_io.ReadBytes(M_Parent.TypeOffsetsSlice.Size);
                        var io___raw_tiOffsetList = new KaitaiStream(__raw_tiOffsetList);
                        _tiOffsetList = new TiOffsetList(io___raw_tiOffsetList, this, m_root);
                        m_io.Seek(_pos);
                        f_tiOffsetList = true;
                    }
                    return _tiOffsetList;
                }
            }
            private bool f_hashHeadList;
            private TpiHashHeadList _hashHeadList;
            public TpiHashHeadList HashHeadList
            {
                get
                {
                    if (f_hashHeadList)
                        return _hashHeadList;
                    if (M_Parent.HashHeadListSlice.Size > 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(M_Parent.HashHeadListSlice.Offset);
                        __raw_hashHeadList = m_io.ReadBytes(M_Parent.HashHeadListSlice.Size);
                        var io___raw_hashHeadList = new KaitaiStream(__raw_hashHeadList);
                        _hashHeadList = new TpiHashHeadList(io___raw_hashHeadList, this, m_root);
                        m_io.Seek(_pos);
                        f_hashHeadList = true;
                    }
                    return _hashHeadList;
                }
            }
            private MsPdb m_root;
            private MsPdb.TpiHash m_parent;
            private byte[] __raw_tiOffsetList;
            private byte[] __raw_hashHeadList;
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiHash M_Parent { get { return m_parent; } }
            public byte[] M_RawTiOffsetList { get { return __raw_tiOffsetList; } }
            public byte[] M_RawHashHeadList { get { return __raw_hashHeadList; } }
        }

        /// <remarks>
        /// Reference: HDR
        /// </remarks>
        public partial class TpiHeader : KaitaiStruct
        {
            public static TpiHeader FromFile(string fileName)
            {
                return new TpiHeader(new KaitaiStream(fileName));
            }

            public TpiHeader(KaitaiStream p__io, MsPdb.Tpi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _version = ((MsPdb.Tpi.TpiVersion) m_io.ReadU4le());
                _headerSize = m_io.ReadU4le();
                _minTypeIndex = m_io.ReadU4le();
                _maxTypeIndex = m_io.ReadU4le();
                _gpRecSize = m_io.ReadU4le();
                _hash = new TpiHash(m_io, this, m_root);
            }
            private Tpi.TpiVersion _version;
            private uint _headerSize;
            private uint _minTypeIndex;
            private uint _maxTypeIndex;
            private uint _gpRecSize;
            private TpiHash _hash;
            private MsPdb m_root;
            private MsPdb.Tpi m_parent;

            /// <summary>
            /// version which created this TypeServer
            /// </summary>
            /// <remarks>
            /// Reference: vers
            /// </remarks>
            public Tpi.TpiVersion Version { get { return _version; } }

            /// <summary>
            /// size of the header, allows easier upgrading and backwards compatibility
            /// </summary>
            /// <remarks>
            /// Reference: cbHdr
            /// </remarks>
            public uint HeaderSize { get { return _headerSize; } }

            /// <summary>
            /// lowest TI
            /// </summary>
            /// <remarks>
            /// Reference: tiMin
            /// </remarks>
            public uint MinTypeIndex { get { return _minTypeIndex; } }

            /// <summary>
            /// highest TI + 1
            /// </summary>
            /// <remarks>
            /// Reference: tiMac
            /// </remarks>
            public uint MaxTypeIndex { get { return _maxTypeIndex; } }

            /// <summary>
            /// count of bytes used by the gprec which follows.
            /// </summary>
            /// <remarks>
            /// Reference: cbGprec
            /// </remarks>
            public uint GpRecSize { get { return _gpRecSize; } }

            /// <summary>
            /// hash stream schema
            /// </summary>
            /// <remarks>
            /// Reference: tpihash
            /// </remarks>
            public TpiHash Hash { get { return _hash; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Tpi M_Parent { get { return m_parent; } }
        }
        public partial class DbiExtraData : KaitaiStruct
        {
            public static DbiExtraData FromFile(string fileName)
            {
                return new DbiExtraData(new KaitaiStream(fileName));
            }

            public DbiExtraData(KaitaiStream p__io, MsPdb.DbiSymbol p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_isProcrefSt = false;
                f_zzzProcrefAlignment = false;
                f_alignment = false;
                f_extraLength = false;
                _read();
            }
            private void _read()
            {
                _type = ((MsPdb.Dbi.SymbolType) m_io.ReadU2le());
                if (IsProcrefSt) {
                    _procrefData = new SymReference(true, m_io, this, m_root);
                }
            }
            private bool f_isProcrefSt;
            private bool _isProcrefSt;
            public bool IsProcrefSt
            {
                get
                {
                    if (f_isProcrefSt)
                        return _isProcrefSt;
                    _isProcrefSt = (bool) ( ((Type == MsPdb.Dbi.SymbolType.SProcrefSt) || (Type == MsPdb.Dbi.SymbolType.SLprocrefSt)) );
                    f_isProcrefSt = true;
                    return _isProcrefSt;
                }
            }
            private bool f_zzzProcrefAlignment;
            private Align _zzzProcrefAlignment;
            public Align ZzzProcrefAlignment
            {
                get
                {
                    if (f_zzzProcrefAlignment)
                        return _zzzProcrefAlignment;
                    if (IsProcrefSt) {
                        _zzzProcrefAlignment = new Align((((uint) (ProcrefData.Name.NameLength)) + 1), 4, m_io, this, m_root);
                        f_zzzProcrefAlignment = true;
                    }
                    return _zzzProcrefAlignment;
                }
            }
            private bool f_alignment;
            private uint _alignment;
            public uint Alignment
            {
                get
                {
                    if (f_alignment)
                        return _alignment;
                    _alignment = (uint) ((IsProcrefSt ? ZzzProcrefAlignment.Value : 0));
                    f_alignment = true;
                    return _alignment;
                }
            }
            private bool f_extraLength;
            private int _extraLength;
            public int ExtraLength
            {
                get
                {
                    if (f_extraLength)
                        return _extraLength;
                    _extraLength = (int) ((IsProcrefSt ? ZzzProcrefAlignment.Aligned : 0));
                    f_extraLength = true;
                    return _extraLength;
                }
            }
            private Dbi.SymbolType _type;
            private SymReference _procrefData;
            private MsPdb m_root;
            private MsPdb.DbiSymbol m_parent;
            public Dbi.SymbolType Type { get { return _type; } }
            public SymReference ProcrefData { get { return _procrefData; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbol M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DATASYMHLSL32
        /// </remarks>
        public partial class SymDataHlsl32 : KaitaiStruct
        {
            public static SymDataHlsl32 FromFile(string fileName)
            {
                return new SymDataHlsl32(new KaitaiStream(fileName));
            }

            public SymDataHlsl32(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _dataSlot = m_io.ReadU4le();
                _dataOffset = m_io.ReadU4le();
                _texSlot = m_io.ReadU4le();
                _sampSlot = m_io.ReadU4le();
                _uavSlot = m_io.ReadU4le();
                _regType = m_io.ReadU4le();
                _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
            }
            private TpiTypeRef _type;
            private uint _dataSlot;
            private uint _dataOffset;
            private uint _texSlot;
            private uint _sampSlot;
            private uint _uavSlot;
            private uint _regType;
            private string _name;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Type index
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// Base data (cbuffer, groupshared, etc.) slot
            /// </summary>
            /// <remarks>
            /// Reference: dataslot
            /// </remarks>
            public uint DataSlot { get { return _dataSlot; } }

            /// <summary>
            /// Base data byte offset start
            /// </summary>
            /// <remarks>
            /// Reference: dataoff
            /// </remarks>
            public uint DataOffset { get { return _dataOffset; } }

            /// <summary>
            /// Texture slot start
            /// </summary>
            /// <remarks>
            /// Reference: texslot
            /// </remarks>
            public uint TexSlot { get { return _texSlot; } }

            /// <summary>
            /// Sampler slot start
            /// </summary>
            /// <remarks>
            /// Reference: sampslot
            /// </remarks>
            public uint SampSlot { get { return _sampSlot; } }

            /// <summary>
            /// UAV slot start
            /// </summary>
            /// <remarks>
            /// Reference: uavslot
            /// </remarks>
            public uint UavSlot { get { return _uavSlot; } }

            /// <summary>
            /// register type from CV_HLSLREG_e
            /// </summary>
            /// <remarks>
            /// Reference: regType
            /// </remarks>
            public uint RegType { get { return _regType; } }

            /// <summary>
            /// name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public string Name { get { return _name; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: NewDBIHdr
        /// </remarks>
        public partial class DbiHeaderNew : KaitaiStruct
        {
            public static DbiHeaderNew FromFile(string fileName)
            {
                return new DbiHeaderNew(new KaitaiStream(fileName));
            }


            public enum VersionEnum : uint
            {
                V41 = 930803,
                V50 = 19960307,
                V60 = 19970606,
                V70 = 19990903,
                V110 = 20091201,
            }
            public DbiHeaderNew(KaitaiStream p__io, MsPdb.Dbi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_symbolsData = false;
                f_gsSymbolsData = false;
                f_psSymbolsData = false;
                _read();
            }
            private void _read()
            {
                _signature = m_io.ReadU4le();
                _version = ((VersionEnum) m_io.ReadU4le());
                _age = m_io.ReadU4le();
                _gsSymbolsStream = new PdbStreamRef(m_io, this, m_root);
                _internalVersion = m_io.ReadU2le();
                _psSymbolsStream = new PdbStreamRef(m_io, this, m_root);
                _pdbDllVersion = m_io.ReadU2le();
                _symbolRecordsStream = new PdbStreamRef(m_io, this, m_root);
                _rbldVersion = m_io.ReadU2le();
                _moduleListSize = m_io.ReadU4le();
                _sectionContributionSize = m_io.ReadU4le();
                _sectionMapSize = m_io.ReadU4le();
                _fileInfoSize = m_io.ReadU4le();
                _typeServerMapSize = m_io.ReadU4le();
                _mfcTypeServerIndex = m_io.ReadU4le();
                _debugHeaderSize = m_io.ReadU4le();
                _ecSubstreamSize = m_io.ReadU4le();
                _flags = new DbiHeaderFlags(m_io, this, m_root);
                _machineType = m_io.ReadU2le();
                _reserved = m_io.ReadU4le();
            }
            private bool f_symbolsData;
            private SymbolRecordsStream _symbolsData;
            public SymbolRecordsStream SymbolsData
            {
                get
                {
                    if (f_symbolsData)
                        return _symbolsData;
                    if (SymbolRecordsStream.StreamNumber > -1) {
                        __raw__raw_symbolsData = m_io.ReadBytes(0);
                        Cat _process__raw__raw_symbolsData = new Cat(SymbolRecordsStream.Data);
                        __raw_symbolsData = _process__raw__raw_symbolsData.Decode(__raw__raw_symbolsData);
                        var io___raw_symbolsData = new KaitaiStream(__raw_symbolsData);
                        _symbolsData = new SymbolRecordsStream(io___raw_symbolsData, this, m_root);
                        f_symbolsData = true;
                    }
                    return _symbolsData;
                }
            }
            private bool f_gsSymbolsData;
            private GlobalSymbolsStream _gsSymbolsData;
            public GlobalSymbolsStream GsSymbolsData
            {
                get
                {
                    if (f_gsSymbolsData)
                        return _gsSymbolsData;
                    if (GsSymbolsStream.StreamNumber > -1) {
                        __raw__raw_gsSymbolsData = m_io.ReadBytes(0);
                        Cat _process__raw__raw_gsSymbolsData = new Cat(GsSymbolsStream.Data);
                        __raw_gsSymbolsData = _process__raw__raw_gsSymbolsData.Decode(__raw__raw_gsSymbolsData);
                        var io___raw_gsSymbolsData = new KaitaiStream(__raw_gsSymbolsData);
                        _gsSymbolsData = new GlobalSymbolsStream(io___raw_gsSymbolsData, this, m_root);
                        f_gsSymbolsData = true;
                    }
                    return _gsSymbolsData;
                }
            }
            private bool f_psSymbolsData;
            private PublicSymbolsStream _psSymbolsData;
            public PublicSymbolsStream PsSymbolsData
            {
                get
                {
                    if (f_psSymbolsData)
                        return _psSymbolsData;
                    if (PsSymbolsStream.StreamNumber > -1) {
                        __raw__raw_psSymbolsData = m_io.ReadBytes(0);
                        Cat _process__raw__raw_psSymbolsData = new Cat(PsSymbolsStream.Data);
                        __raw_psSymbolsData = _process__raw__raw_psSymbolsData.Decode(__raw__raw_psSymbolsData);
                        var io___raw_psSymbolsData = new KaitaiStream(__raw_psSymbolsData);
                        _psSymbolsData = new PublicSymbolsStream(io___raw_psSymbolsData, this, m_root);
                        f_psSymbolsData = true;
                    }
                    return _psSymbolsData;
                }
            }
            private uint _signature;
            private VersionEnum _version;
            private uint _age;
            private PdbStreamRef _gsSymbolsStream;
            private ushort _internalVersion;
            private PdbStreamRef _psSymbolsStream;
            private ushort _pdbDllVersion;
            private PdbStreamRef _symbolRecordsStream;
            private ushort _rbldVersion;
            private uint _moduleListSize;
            private uint _sectionContributionSize;
            private uint _sectionMapSize;
            private uint _fileInfoSize;
            private uint _typeServerMapSize;
            private uint _mfcTypeServerIndex;
            private uint _debugHeaderSize;
            private uint _ecSubstreamSize;
            private DbiHeaderFlags _flags;
            private ushort _machineType;
            private uint _reserved;
            private MsPdb m_root;
            private MsPdb.Dbi m_parent;
            private byte[] __raw_symbolsData;
            private byte[] __raw__raw_symbolsData;
            private byte[] __raw_gsSymbolsData;
            private byte[] __raw__raw_gsSymbolsData;
            private byte[] __raw_psSymbolsData;
            private byte[] __raw__raw_psSymbolsData;

            /// <remarks>
            /// Reference: verSignature
            /// </remarks>
            public uint Signature { get { return _signature; } }

            /// <remarks>
            /// Reference: verHdr
            /// </remarks>
            public VersionEnum Version { get { return _version; } }

            /// <remarks>
            /// Reference: age
            /// </remarks>
            public uint Age { get { return _age; } }

            /// <remarks>
            /// Reference: snGSSyms
            /// </remarks>
            public PdbStreamRef GsSymbolsStream { get { return _gsSymbolsStream; } }

            /// <remarks>
            /// Reference: usVerAll
            /// </remarks>
            public ushort InternalVersion { get { return _internalVersion; } }

            /// <remarks>
            /// Reference: snPSSyms
            /// </remarks>
            public PdbStreamRef PsSymbolsStream { get { return _psSymbolsStream; } }

            /// <summary>
            /// build version of the pdb dll that built this pdb last.
            /// </summary>
            /// <remarks>
            /// Reference: usVerPdbDllBuild
            /// </remarks>
            public ushort PdbDllVersion { get { return _pdbDllVersion; } }

            /// <remarks>
            /// Reference: snSymRecs
            /// </remarks>
            public PdbStreamRef SymbolRecordsStream { get { return _symbolRecordsStream; } }

            /// <summary>
            /// rbld version of the pdb dll that built this pdb last.
            /// </summary>
            /// <remarks>
            /// Reference: usVerPdbDllRBld
            /// </remarks>
            public ushort RbldVersion { get { return _rbldVersion; } }

            /// <summary>
            /// size of rgmodi substream
            /// </summary>
            /// <remarks>
            /// Reference: cbGpModi
            /// </remarks>
            public uint ModuleListSize { get { return _moduleListSize; } }

            /// <summary>
            /// size of Section Contribution substream
            /// </summary>
            /// <remarks>
            /// Reference: cbSC
            /// </remarks>
            public uint SectionContributionSize { get { return _sectionContributionSize; } }

            /// <remarks>
            /// Reference: cbSecMap
            /// </remarks>
            public uint SectionMapSize { get { return _sectionMapSize; } }

            /// <remarks>
            /// Reference: cbFileInfo
            /// </remarks>
            public uint FileInfoSize { get { return _fileInfoSize; } }

            /// <summary>
            /// size of the Type Server Map substream
            /// </summary>
            /// <remarks>
            /// Reference: cbTSMap
            /// </remarks>
            public uint TypeServerMapSize { get { return _typeServerMapSize; } }

            /// <summary>
            /// index of MFC type server
            /// </summary>
            /// <remarks>
            /// Reference: iMFC
            /// </remarks>
            public uint MfcTypeServerIndex { get { return _mfcTypeServerIndex; } }

            /// <summary>
            /// size of optional DbgHdr info appended to the end of the stream
            /// </summary>
            /// <remarks>
            /// Reference: cbDbgHdr
            /// </remarks>
            public uint DebugHeaderSize { get { return _debugHeaderSize; } }

            /// <summary>
            /// number of bytes in EC substream, or 0 if EC no EC enabled Mods
            /// </summary>
            /// <remarks>
            /// Reference: cbECInfo
            /// </remarks>
            public uint EcSubstreamSize { get { return _ecSubstreamSize; } }

            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public DbiHeaderFlags Flags { get { return _flags; } }

            /// <summary>
            /// machine type
            /// </summary>
            /// <remarks>
            /// Reference: wMachine
            /// </remarks>
            public ushort MachineType { get { return _machineType; } }

            /// <summary>
            /// pad out to 64 bytes for future growth.
            /// </summary>
            /// <remarks>
            /// Reference: rgulReserved
            /// </remarks>
            public uint Reserved { get { return _reserved; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Dbi M_Parent { get { return m_parent; } }
            public byte[] M_RawSymbolsData { get { return __raw_symbolsData; } }
            public byte[] M_RawM_RawSymbolsData { get { return __raw__raw_symbolsData; } }
            public byte[] M_RawGsSymbolsData { get { return __raw_gsSymbolsData; } }
            public byte[] M_RawM_RawGsSymbolsData { get { return __raw__raw_gsSymbolsData; } }
            public byte[] M_RawPsSymbolsData { get { return __raw_psSymbolsData; } }
            public byte[] M_RawM_RawPsSymbolsData { get { return __raw__raw_psSymbolsData; } }
        }

        /// <remarks>
        /// Reference: TpiHash
        /// </remarks>
        public partial class TpiHash : KaitaiStruct
        {
            public static TpiHash FromFile(string fileName)
            {
                return new TpiHash(new KaitaiStream(fileName));
            }

            public TpiHash(KaitaiStream p__io, MsPdb.TpiHeader p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_tpiHashData = false;
                _read();
            }
            private void _read()
            {
                _hashStream = new PdbStreamRef(m_io, this, m_root);
                _auxHashStream = new PdbStreamRef(m_io, this, m_root);
                _hashKeySize = m_io.ReadU4le();
                _numHashBuckets = m_io.ReadU4le();
                _hashValuesSlice = new TpiSlice(m_io, this, m_root);
                _typeOffsetsSlice = new TpiSlice(m_io, this, m_root);
                _hashHeadListSlice = new TpiSlice(m_io, this, m_root);
            }
            private bool f_tpiHashData;
            private TpiHashData _tpiHashData;
            public TpiHashData TpiHashData
            {
                get
                {
                    if (f_tpiHashData)
                        return _tpiHashData;
                    __raw__raw_tpiHashData = m_io.ReadBytes(0);
                    Cat _process__raw__raw_tpiHashData = new Cat(HashStream.Data);
                    __raw_tpiHashData = _process__raw__raw_tpiHashData.Decode(__raw__raw_tpiHashData);
                    var io___raw_tpiHashData = new KaitaiStream(__raw_tpiHashData);
                    _tpiHashData = new TpiHashData(io___raw_tpiHashData, this, m_root);
                    f_tpiHashData = true;
                    return _tpiHashData;
                }
            }
            private PdbStreamRef _hashStream;
            private PdbStreamRef _auxHashStream;
            private uint _hashKeySize;
            private uint _numHashBuckets;
            private TpiSlice _hashValuesSlice;
            private TpiSlice _typeOffsetsSlice;
            private TpiSlice _hashHeadListSlice;
            private MsPdb m_root;
            private MsPdb.TpiHeader m_parent;
            private byte[] __raw_tpiHashData;
            private byte[] __raw__raw_tpiHashData;

            /// <summary>
            /// main hash stream
            /// </summary>
            /// <remarks>
            /// Reference: SN
            /// </remarks>
            public PdbStreamRef HashStream { get { return _hashStream; } }

            /// <summary>
            /// auxilliary hash data if necessary
            /// </summary>
            /// <remarks>
            /// Reference: snPad
            /// </remarks>
            public PdbStreamRef AuxHashStream { get { return _auxHashStream; } }

            /// <summary>
            /// size of hash key
            /// </summary>
            /// <remarks>
            /// Reference: cbHashKey
            /// </remarks>
            public uint HashKeySize { get { return _hashKeySize; } }

            /// <summary>
            /// how many buckets we have
            /// </summary>
            /// <remarks>
            /// Reference: cHashBuckets
            /// </remarks>
            public uint NumHashBuckets { get { return _numHashBuckets; } }

            /// <summary>
            /// offcb of hashvals
            /// </summary>
            /// <remarks>
            /// Reference: offcbHashVals
            /// </remarks>
            public TpiSlice HashValuesSlice { get { return _hashValuesSlice; } }

            /// <summary>
            /// offcb of (TI,OFF) pairs
            /// </summary>
            /// <remarks>
            /// Reference: offcbTiOff
            /// </remarks>
            public TpiSlice TypeOffsetsSlice { get { return _typeOffsetsSlice; } }

            /// <summary>
            /// offcb of hash head list, maps (hashval,ti), where ti is the head of the hashval chain.
            /// </summary>
            /// <remarks>
            /// Reference: offcbHashAdj
            /// </remarks>
            public TpiSlice HashHeadListSlice { get { return _hashHeadListSlice; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiHeader M_Parent { get { return m_parent; } }
            public byte[] M_RawTpiHashData { get { return __raw_tpiHashData; } }
            public byte[] M_RawM_RawTpiHashData { get { return __raw__raw_tpiHashData; } }
        }

        /// <remarks>
        /// Reference: PDBStream
        /// </remarks>
        public partial class PdbStreamHdr : KaitaiStruct
        {
            public static PdbStreamHdr FromFile(string fileName)
            {
                return new PdbStreamHdr(new KaitaiStream(fileName));
            }

            public PdbStreamHdr(KaitaiStream p__io, MsPdb.PdbStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _implementationVersion = ((MsPdb.PdbImplementationVersion) m_io.ReadU4le());
                _sig = m_io.ReadU4le();
                _age = m_io.ReadU4le();
            }
            private PdbImplementationVersion _implementationVersion;
            private uint _sig;
            private uint _age;
            private MsPdb m_root;
            private MsPdb.PdbStream m_parent;

            /// <summary>
            /// implementation version number
            /// </summary>
            /// <remarks>
            /// Reference: impv
            /// </remarks>
            public PdbImplementationVersion ImplementationVersion { get { return _implementationVersion; } }

            /// <summary>
            /// unique (across PDB instances) signature
            /// </summary>
            /// <remarks>
            /// Reference: sig
            /// </remarks>
            public uint Sig { get { return _sig; } }

            /// <summary>
            /// no. of times this instance has been updated
            /// </summary>
            /// <remarks>
            /// Reference: age
            /// </remarks>
            public uint Age { get { return _age; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbStream M_Parent { get { return m_parent; } }
        }
        public partial class C13FileChecksum : KaitaiStruct
        {
            public static C13FileChecksum FromFile(string fileName)
            {
                return new C13FileChecksum(new KaitaiStream(fileName));
            }


            public enum ChecksumTypeEnum
            {
                None = 0,
                Md5 = 1,
                Sha1 = 2,
                Sha256 = 3,
            }
            public C13FileChecksum(KaitaiStream p__io, MsPdb.C13SubsectionFilechecksums p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_startPos = false;
                f_endPos = false;
                f_zzzAlignment = false;
                f_padding = false;
                _read();
            }
            private void _read()
            {
                if (StartPos >= 0) {
                    _invokeStartPos = m_io.ReadBytes(0);
                }
                _filenameOffset = m_io.ReadU4le();
                _checksumSize = m_io.ReadU1();
                _checksumType = ((ChecksumTypeEnum) m_io.ReadU1());
                _checksumData = m_io.ReadBytes(ChecksumSize);
                if (EndPos >= 0) {
                    _invokeEndPos = m_io.ReadBytes(0);
                }
                _alignment = m_io.ReadBytes(Padding);
            }
            private bool f_startPos;
            private int _startPos;
            public int StartPos
            {
                get
                {
                    if (f_startPos)
                        return _startPos;
                    _startPos = (int) (M_Io.Pos);
                    f_startPos = true;
                    return _startPos;
                }
            }
            private bool f_endPos;
            private int _endPos;
            public int EndPos
            {
                get
                {
                    if (f_endPos)
                        return _endPos;
                    _endPos = (int) (M_Io.Pos);
                    f_endPos = true;
                    return _endPos;
                }
            }
            private bool f_zzzAlignment;
            private Align _zzzAlignment;
            public Align ZzzAlignment
            {
                get
                {
                    if (f_zzzAlignment)
                        return _zzzAlignment;
                    _zzzAlignment = new Align(((uint) ((EndPos - StartPos))), 4, m_io, this, m_root);
                    f_zzzAlignment = true;
                    return _zzzAlignment;
                }
            }
            private bool f_padding;
            private int _padding;
            public int Padding
            {
                get
                {
                    if (f_padding)
                        return _padding;
                    _padding = (int) ((ZzzAlignment.Aligned - ZzzAlignment.Value));
                    f_padding = true;
                    return _padding;
                }
            }
            private byte[] _invokeStartPos;
            private uint _filenameOffset;
            private byte _checksumSize;
            private ChecksumTypeEnum _checksumType;
            private byte[] _checksumData;
            private byte[] _invokeEndPos;
            private byte[] _alignment;
            private MsPdb m_root;
            private MsPdb.C13SubsectionFilechecksums m_parent;
            public byte[] InvokeStartPos { get { return _invokeStartPos; } }
            public uint FilenameOffset { get { return _filenameOffset; } }
            public byte ChecksumSize { get { return _checksumSize; } }
            public ChecksumTypeEnum ChecksumType { get { return _checksumType; } }
            public byte[] ChecksumData { get { return _checksumData; } }
            public byte[] InvokeEndPos { get { return _invokeEndPos; } }
            public byte[] Alignment { get { return _alignment; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13SubsectionFilechecksums M_Parent { get { return m_parent; } }
        }
        public partial class GetStreamNumPages : KaitaiStruct
        {
            public GetStreamNumPages(uint p_streamNumber, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _streamNumber = p_streamNumber;
                f_value = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_value;
            private int _value;
            public int Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    _value = (int) ((M_Root.PdbType == MsPdb.PdbTypeEnum.Big ? M_Root.PdbDs.StreamTable.StreamSizesDs[((int) (StreamNumber))].NumDirectoryPages : M_Root.PdbJg.StreamTable.StreamSizesJg[((int) (StreamNumber))].NumDirectoryPages));
                    f_value = true;
                    return _value;
                }
            }
            private uint _streamNumber;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public uint StreamNumber { get { return _streamNumber; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class ModuleStream : KaitaiStruct
        {

            public enum CvSignature
            {
                C6 = 0,
                C7 = 1,
                C11 = 2,
                C13 = 4,
            }
            public ModuleStream(uint p_moduleIndex, KaitaiStream p__io, MsPdb.UModuleInfo p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _moduleIndex = p_moduleIndex;
                f_symbolsSize = false;
                _read();
            }
            private void _read()
            {
                _signature = ((CvSignature) m_io.ReadU4le());
                if (SymbolsSize > 0) {
                    __raw_symbolsList = m_io.ReadBytes(SymbolsSize);
                    var io___raw_symbolsList = new KaitaiStream(__raw_symbolsList);
                    _symbolsList = new ModuleSymbols(ModuleIndex, io___raw_symbolsList, this, m_root);
                }
                if (M_Parent.LinesSize > 0) {
                    __raw_lines = m_io.ReadBytes(M_Parent.LinesSize);
                    var io___raw_lines = new KaitaiStream(__raw_lines);
                    _lines = new C11Lines(io___raw_lines, this, m_root);
                }
                if (M_Parent.C13LinesSize > 0) {
                    __raw_c13Lines = m_io.ReadBytes(M_Parent.C13LinesSize);
                    var io___raw_c13Lines = new KaitaiStream(__raw_c13Lines);
                    _c13Lines = new C13Lines(io___raw_c13Lines, this, m_root);
                }
            }
            private bool f_symbolsSize;
            private int _symbolsSize;
            public int SymbolsSize
            {
                get
                {
                    if (f_symbolsSize)
                        return _symbolsSize;
                    _symbolsSize = (int) ((M_Parent.SymbolsSize - 4));
                    f_symbolsSize = true;
                    return _symbolsSize;
                }
            }
            private CvSignature _signature;
            private ModuleSymbols _symbolsList;
            private C11Lines _lines;
            private C13Lines _c13Lines;
            private uint _moduleIndex;
            private MsPdb m_root;
            private MsPdb.UModuleInfo m_parent;
            private byte[] __raw_symbolsList;
            private byte[] __raw_lines;
            private byte[] __raw_c13Lines;
            public CvSignature Signature { get { return _signature; } }
            public ModuleSymbols SymbolsList { get { return _symbolsList; } }
            public C11Lines Lines { get { return _lines; } }
            public C13Lines C13Lines { get { return _c13Lines; } }
            public uint ModuleIndex { get { return _moduleIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.UModuleInfo M_Parent { get { return m_parent; } }
            public byte[] M_RawSymbolsList { get { return __raw_symbolsList; } }
            public byte[] M_RawLines { get { return __raw_lines; } }
            public byte[] M_RawC13Lines { get { return __raw_c13Lines; } }
        }
        public partial class TpiSlice : KaitaiStruct
        {
            public static TpiSlice FromFile(string fileName)
            {
                return new TpiSlice(new KaitaiStream(fileName));
            }

            public TpiSlice(KaitaiStream p__io, MsPdb.TpiHash p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_data = false;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _size = m_io.ReadU4le();
            }
            private bool f_data;
            private byte[] _data;
            public byte[] Data
            {
                get
                {
                    if (f_data)
                        return _data;
                    KaitaiStream io = M_Parent.M_Io;
                    long _pos = io.Pos;
                    io.Seek(Offset);
                    _data = io.ReadBytes(Size);
                    io.Seek(_pos);
                    f_data = true;
                    return _data;
                }
            }
            private uint _offset;
            private uint _size;
            private MsPdb m_root;
            private MsPdb.TpiHash m_parent;
            public uint Offset { get { return _offset; } }
            public uint Size { get { return _size; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiHash M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CV_fldattr_t
        /// </remarks>
        public partial class CvFieldAttributes : KaitaiStruct
        {
            public static CvFieldAttributes FromFile(string fileName)
            {
                return new CvFieldAttributes(new KaitaiStream(fileName));
            }

            public CvFieldAttributes(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _accessProtection = ((MsPdb.Tpi.CvAccess) m_io.ReadBitsIntLe(2));
                _methodProperties = ((MsPdb.Tpi.CvMethodprop) m_io.ReadBitsIntLe(3));
                _isPseudo = m_io.ReadBitsIntLe(1) != 0;
                _noInherit = m_io.ReadBitsIntLe(1) != 0;
                _noConstruct = m_io.ReadBitsIntLe(1) != 0;
                _compilerGenerated = m_io.ReadBitsIntLe(1) != 0;
                _isSealed = m_io.ReadBitsIntLe(1) != 0;
                __unnamed7 = m_io.ReadBitsIntLe(6);
            }
            private Tpi.CvAccess _accessProtection;
            private Tpi.CvMethodprop _methodProperties;
            private bool _isPseudo;
            private bool _noInherit;
            private bool _noConstruct;
            private bool _compilerGenerated;
            private bool _isSealed;
            private ulong __unnamed7;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// access protection
            /// </summary>
            /// <remarks>
            /// Reference: access
            /// </remarks>
            public Tpi.CvAccess AccessProtection { get { return _accessProtection; } }

            /// <summary>
            /// method properties
            /// </summary>
            /// <remarks>
            /// Reference: mprop
            /// </remarks>
            public Tpi.CvMethodprop MethodProperties { get { return _methodProperties; } }

            /// <summary>
            /// compiler generated fcn and does not exist
            /// </summary>
            /// <remarks>
            /// Reference: pseudo
            /// </remarks>
            public bool IsPseudo { get { return _isPseudo; } }

            /// <summary>
            /// true if class cannot be inherited
            /// </summary>
            /// <remarks>
            /// Reference: noinherit
            /// </remarks>
            public bool NoInherit { get { return _noInherit; } }

            /// <summary>
            /// true if class cannot be constructed
            /// </summary>
            /// <remarks>
            /// Reference: noconstruct
            /// </remarks>
            public bool NoConstruct { get { return _noConstruct; } }

            /// <summary>
            /// compiler generated fcn and does exist
            /// </summary>
            /// <remarks>
            /// Reference: compgenx
            /// </remarks>
            public bool CompilerGenerated { get { return _compilerGenerated; } }

            /// <summary>
            /// true if method cannot be overridden
            /// </summary>
            /// <remarks>
            /// Reference: sealed
            /// </remarks>
            public bool IsSealed { get { return _isSealed; } }

            /// <summary>
            /// unused
            /// </summary>
            /// <remarks>
            /// Reference: unused
            /// </remarks>
            public ulong Unnamed_7 { get { return __unnamed7; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: lfMethodList_16t
        /// </remarks>
        public partial class LfMethodlist16t : KaitaiStruct
        {
            public static LfMethodlist16t FromFile(string fileName)
            {
                return new LfMethodlist16t(new KaitaiStream(fileName));
            }

            public LfMethodlist16t(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _methods = new List<MlMethod16t>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _methods.Add(new MlMethod16t(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<MlMethod16t> _methods;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;
            public List<MlMethod16t> Methods { get { return _methods; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SC
        /// </remarks>
        public partial class SectionContrib : KaitaiStruct
        {
            public static SectionContrib FromFile(string fileName)
            {
                return new SectionContrib(new KaitaiStream(fileName));
            }

            public SectionContrib(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _base = new SectionContrib40(m_io, this, m_root);
                _dataCrc = m_io.ReadU4le();
                _relocCrc = m_io.ReadU4le();
            }
            private SectionContrib40 _base;
            private uint _dataCrc;
            private uint _relocCrc;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public SectionContrib40 Base { get { return _base; } }

            /// <remarks>
            /// Reference: dwDataCrc
            /// </remarks>
            public uint DataCrc { get { return _dataCrc; } }

            /// <remarks>
            /// Reference: dwRelocCrc
            /// </remarks>
            public uint RelocCrc { get { return _relocCrc; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CV_Column_t
        /// </remarks>
        public partial class C13Column : KaitaiStruct
        {
            public static C13Column FromFile(string fileName)
            {
                return new C13Column(new KaitaiStream(fileName));
            }

            public C13Column(KaitaiStream p__io, MsPdb.C13FileBlock p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _columnStartOffset = m_io.ReadU2le();
                _columnEndOffset = m_io.ReadU2le();
            }
            private ushort _columnStartOffset;
            private ushort _columnEndOffset;
            private MsPdb m_root;
            private MsPdb.C13FileBlock m_parent;

            /// <remarks>
            /// Reference: offColumnStart
            /// </remarks>
            public ushort ColumnStartOffset { get { return _columnStartOffset; } }

            /// <remarks>
            /// Reference: offColumnEnd
            /// </remarks>
            public ushort ColumnEndOffset { get { return _columnEndOffset; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13FileBlock M_Parent { get { return m_parent; } }
        }
        public partial class SymSkip : KaitaiStruct
        {
            public static SymSkip FromFile(string fileName)
            {
                return new SymSkip(new KaitaiStream(fileName));
            }

            public SymSkip(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                __unnamed0 = m_io.ReadBytesFull();
            }
            private byte[] __unnamed0;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;
            public byte[] Unnamed_0 { get { return __unnamed0; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: UDTSYM
        /// </remarks>
        public partial class SymUdt : KaitaiStruct
        {
            public SymUdt(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private TpiTypeRef _type;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Type index
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class PdbNamedStream : KaitaiStruct
        {
            public PdbNamedStream(uint p_index, KaitaiStream p__io, MsPdb.PdbMapNamedStreams p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _index = p_index;
                f_stream = false;
                f_zzzName = false;
                f_streamNumber = false;
                f_data = false;
                f_nameOffset = false;
                f_item = false;
                f_name = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_stream;
            private PdbStreamRefX _stream;
            public PdbStreamRefX Stream
            {
                get
                {
                    if (f_stream)
                        return _stream;
                    if (Item.IsPresent) {
                        _stream = new PdbStreamRefX(((short) (StreamNumber)), m_io, this, m_root);
                        f_stream = true;
                    }
                    return _stream;
                }
            }
            private bool f_zzzName;
            private StringSlice _zzzName;
            public StringSlice ZzzName
            {
                get
                {
                    if (f_zzzName)
                        return _zzzName;
                    if (Item.IsPresent) {
                        __raw__raw_zzzName = m_io.ReadBytes(0);
                        Cat _process__raw__raw_zzzName = new Cat(M_Parent.M_Parent.ZzzStringTableData.Data);
                        __raw_zzzName = _process__raw__raw_zzzName.Decode(__raw__raw_zzzName);
                        var io___raw_zzzName = new KaitaiStream(__raw_zzzName);
                        _zzzName = new StringSlice(((uint) (NameOffset)), io___raw_zzzName, this, m_root);
                        f_zzzName = true;
                    }
                    return _zzzName;
                }
            }
            private bool f_streamNumber;
            private uint? _streamNumber;
            public uint? StreamNumber
            {
                get
                {
                    if (f_streamNumber)
                        return _streamNumber;
                    if (Item.IsPresent) {
                        _streamNumber = (uint) (Item.ValueU4);
                    }
                    f_streamNumber = true;
                    return _streamNumber;
                }
            }
            private bool f_data;
            private object _data;
            public object Data
            {
                get
                {
                    if (f_data)
                        return _data;
                    if ( ((Item.IsPresent) && (Stream.Size > 0)) ) {
                        switch (Name) {
                        case "/LinkInfo": {
                            __raw__raw_data = m_io.ReadBytes(0);
                            Cat _process__raw__raw_data = new Cat(Stream.Data);
                            __raw_data = _process__raw__raw_data.Decode(__raw__raw_data);
                            var io___raw_data = new KaitaiStream(__raw_data);
                            _data = new LinkInfoStream(io___raw_data, this, m_root);
                            break;
                        }
                        case "/src/headerblock": {
                            __raw__raw_data = m_io.ReadBytes(0);
                            Cat _process__raw__raw_data = new Cat(Stream.Data);
                            __raw_data = _process__raw__raw_data.Decode(__raw__raw_data);
                            var io___raw_data = new KaitaiStream(__raw_data);
                            _data = new SrcHeaderBlockStream(io___raw_data, this, m_root);
                            break;
                        }
                        case "/names": {
                            __raw__raw_data = m_io.ReadBytes(0);
                            Cat _process__raw__raw_data = new Cat(Stream.Data);
                            __raw_data = _process__raw__raw_data.Decode(__raw__raw_data);
                            var io___raw_data = new KaitaiStream(__raw_data);
                            _data = new NameTable(io___raw_data, this, m_root);
                            break;
                        }
                        default: {
                            __raw_data = m_io.ReadBytes(0);
                            Cat _process__raw_data = new Cat(Stream.Data);
                            _data = _process__raw_data.Decode(__raw_data);
                            break;
                        }
                        }
                        f_data = true;
                    }
                    return _data;
                }
            }
            private bool f_nameOffset;
            private uint? _nameOffset;
            public uint? NameOffset
            {
                get
                {
                    if (f_nameOffset)
                        return _nameOffset;
                    if (Item.IsPresent) {
                        _nameOffset = (uint) (Item.KeyU4);
                    }
                    f_nameOffset = true;
                    return _nameOffset;
                }
            }
            private bool f_item;
            private PdbMapKvPair _item;
            public PdbMapKvPair Item
            {
                get
                {
                    if (f_item)
                        return _item;
                    _item = (PdbMapKvPair) (M_Parent.Map.KeyValuePairs[((int) (Index))]);
                    f_item = true;
                    return _item;
                }
            }
            private bool f_name;
            private string _name;
            public string Name
            {
                get
                {
                    if (f_name)
                        return _name;
                    if (Item.IsPresent) {
                        _name = (string) (ZzzName.Value);
                    }
                    f_name = true;
                    return _name;
                }
            }
            private uint _index;
            private MsPdb m_root;
            private MsPdb.PdbMapNamedStreams m_parent;
            private byte[] __raw_zzzName;
            private byte[] __raw__raw_zzzName;
            private byte[] __raw_data;
            private byte[] __raw__raw_data;
            public uint Index { get { return _index; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbMapNamedStreams M_Parent { get { return m_parent; } }
            public byte[] M_RawZzzName { get { return __raw_zzzName; } }
            public byte[] M_RawM_RawZzzName { get { return __raw__raw_zzzName; } }
            public byte[] M_RawData { get { return __raw_data; } }
            public byte[] M_RawM_RawData { get { return __raw__raw_data; } }
        }
        public partial class PdbDsRoot : KaitaiStruct
        {
            public static PdbDsRoot FromFile(string fileName)
            {
                return new PdbDsRoot(new KaitaiStream(fileName));
            }

            public PdbDsRoot(KaitaiStream p__io, MsPdb p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_streamTable = false;
                f_streamTablePages = false;
                f_streamTableRootPages = false;
                f_streamTablePageListSize = false;
                f_zzzNumStreamTablePages = false;
                f_numStreamTablePages = false;
                f_zzzNumStreamTablePagelistPages = false;
                f_numStreamTablePagelistPages = false;
                _read();
            }
            private void _read()
            {
                _header = new PdbHeaderDs(m_io, this, m_root);
                __raw_streamTableRootPagelistData = m_io.ReadBytes((Header.PageSize * NumStreamTablePagelistPages));
                var io___raw_streamTableRootPagelistData = new KaitaiStream(__raw_streamTableRootPagelistData);
                _streamTableRootPagelistData = new PdbPagelist(((uint) (NumStreamTablePagelistPages)), Header.PageSize, io___raw_streamTableRootPagelistData, this, m_root);
            }
            private bool f_streamTable;
            private PdbStreamTable _streamTable;
            public PdbStreamTable StreamTable
            {
                get
                {
                    if (f_streamTable)
                        return _streamTable;
                    __raw__raw_streamTable = m_io.ReadBytes(0);
                    ConcatPages _process__raw__raw_streamTable = new ConcatPages(StreamTablePages.Pages);
                    __raw_streamTable = _process__raw__raw_streamTable.Decode(__raw__raw_streamTable);
                    var io___raw_streamTable = new KaitaiStream(__raw_streamTable);
                    _streamTable = new PdbStreamTable(io___raw_streamTable, this, m_root);
                    f_streamTable = true;
                    return _streamTable;
                }
            }
            private bool f_streamTablePages;
            private PdbPageNumberList _streamTablePages;
            public PdbPageNumberList StreamTablePages
            {
                get
                {
                    if (f_streamTablePages)
                        return _streamTablePages;
                    __raw__raw_streamTablePages = m_io.ReadBytes(0);
                    ConcatPages _process__raw__raw_streamTablePages = new ConcatPages(StreamTableRootPages);
                    __raw_streamTablePages = _process__raw__raw_streamTablePages.Decode(__raw__raw_streamTablePages);
                    var io___raw_streamTablePages = new KaitaiStream(__raw_streamTablePages);
                    _streamTablePages = new PdbPageNumberList(((uint) (NumStreamTablePages)), io___raw_streamTablePages, this, m_root);
                    f_streamTablePages = true;
                    return _streamTablePages;
                }
            }
            private bool f_streamTableRootPages;
            private List<PdbPageNumber> _streamTableRootPages;
            public List<PdbPageNumber> StreamTableRootPages
            {
                get
                {
                    if (f_streamTableRootPages)
                        return _streamTableRootPages;
                    KaitaiStream io = StreamTableRootPagelistData.M_Io;
                    long _pos = io.Pos;
                    io.Seek(0);
                    _streamTableRootPages = new List<PdbPageNumber>();
                    for (var i = 0; i < NumStreamTablePagelistPages; i++)
                    {
                        _streamTableRootPages.Add(new PdbPageNumber(io, this, m_root));
                    }
                    io.Seek(_pos);
                    f_streamTableRootPages = true;
                    return _streamTableRootPages;
                }
            }
            private bool f_streamTablePageListSize;
            private int _streamTablePageListSize;
            public int StreamTablePageListSize
            {
                get
                {
                    if (f_streamTablePageListSize)
                        return _streamTablePageListSize;
                    _streamTablePageListSize = (int) ((NumStreamTablePages * 4));
                    f_streamTablePageListSize = true;
                    return _streamTablePageListSize;
                }
            }
            private bool f_zzzNumStreamTablePages;
            private GetNumPages2 _zzzNumStreamTablePages;
            public GetNumPages2 ZzzNumStreamTablePages
            {
                get
                {
                    if (f_zzzNumStreamTablePages)
                        return _zzzNumStreamTablePages;
                    _zzzNumStreamTablePages = new GetNumPages2(Header.DirectorySize, Header.PageSize, m_io, this, m_root);
                    f_zzzNumStreamTablePages = true;
                    return _zzzNumStreamTablePages;
                }
            }
            private bool f_numStreamTablePages;
            private int _numStreamTablePages;
            public int NumStreamTablePages
            {
                get
                {
                    if (f_numStreamTablePages)
                        return _numStreamTablePages;
                    _numStreamTablePages = (int) (ZzzNumStreamTablePages.NumPages);
                    f_numStreamTablePages = true;
                    return _numStreamTablePages;
                }
            }
            private bool f_zzzNumStreamTablePagelistPages;
            private GetNumPages2 _zzzNumStreamTablePagelistPages;
            public GetNumPages2 ZzzNumStreamTablePagelistPages
            {
                get
                {
                    if (f_zzzNumStreamTablePagelistPages)
                        return _zzzNumStreamTablePagelistPages;
                    _zzzNumStreamTablePagelistPages = new GetNumPages2(((uint) (StreamTablePageListSize)), Header.PageSize, m_io, this, m_root);
                    f_zzzNumStreamTablePagelistPages = true;
                    return _zzzNumStreamTablePagelistPages;
                }
            }
            private bool f_numStreamTablePagelistPages;
            private int _numStreamTablePagelistPages;
            public int NumStreamTablePagelistPages
            {
                get
                {
                    if (f_numStreamTablePagelistPages)
                        return _numStreamTablePagelistPages;
                    _numStreamTablePagelistPages = (int) (ZzzNumStreamTablePagelistPages.NumPages);
                    f_numStreamTablePagelistPages = true;
                    return _numStreamTablePagelistPages;
                }
            }
            private PdbHeaderDs _header;
            private PdbPagelist _streamTableRootPagelistData;
            private MsPdb m_root;
            private MsPdb m_parent;
            private byte[] __raw_streamTableRootPagelistData;
            private byte[] __raw_streamTable;
            private byte[] __raw__raw_streamTable;
            private byte[] __raw_streamTablePages;
            private byte[] __raw__raw_streamTablePages;
            public PdbHeaderDs Header { get { return _header; } }
            public PdbPagelist StreamTableRootPagelistData { get { return _streamTableRootPagelistData; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb M_Parent { get { return m_parent; } }
            public byte[] M_RawStreamTableRootPagelistData { get { return __raw_streamTableRootPagelistData; } }
            public byte[] M_RawStreamTable { get { return __raw_streamTable; } }
            public byte[] M_RawM_RawStreamTable { get { return __raw__raw_streamTable; } }
            public byte[] M_RawStreamTablePages { get { return __raw_streamTablePages; } }
            public byte[] M_RawM_RawStreamTablePages { get { return __raw__raw_streamTablePages; } }
        }
        public partial class PdbJgRoot : KaitaiStruct
        {
            public static PdbJgRoot FromFile(string fileName)
            {
                return new PdbJgRoot(new KaitaiStream(fileName));
            }

            public PdbJgRoot(KaitaiStream p__io, MsPdb p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_zzzNumStreamTablePages = false;
                f_numStreamTablePages = false;
                f_streamTable = false;
                _read();
            }
            private void _read()
            {
                _header = new PdbHeaderJg(m_io, this, m_root);
                __raw_streamTablePages = m_io.ReadBytes((Header.PageSize * NumStreamTablePages));
                var io___raw_streamTablePages = new KaitaiStream(__raw_streamTablePages);
                _streamTablePages = new PdbPageNumberList(((uint) (NumStreamTablePages)), io___raw_streamTablePages, this, m_root);
            }
            private bool f_zzzNumStreamTablePages;
            private GetNumPages2 _zzzNumStreamTablePages;
            public GetNumPages2 ZzzNumStreamTablePages
            {
                get
                {
                    if (f_zzzNumStreamTablePages)
                        return _zzzNumStreamTablePages;
                    _zzzNumStreamTablePages = new GetNumPages2(Header.DirectorySize, Header.PageSize, m_io, this, m_root);
                    f_zzzNumStreamTablePages = true;
                    return _zzzNumStreamTablePages;
                }
            }
            private bool f_numStreamTablePages;
            private int _numStreamTablePages;
            public int NumStreamTablePages
            {
                get
                {
                    if (f_numStreamTablePages)
                        return _numStreamTablePages;
                    _numStreamTablePages = (int) (ZzzNumStreamTablePages.NumPages);
                    f_numStreamTablePages = true;
                    return _numStreamTablePages;
                }
            }
            private bool f_streamTable;
            private PdbStreamTable _streamTable;
            public PdbStreamTable StreamTable
            {
                get
                {
                    if (f_streamTable)
                        return _streamTable;
                    __raw__raw_streamTable = m_io.ReadBytes(0);
                    ConcatPages _process__raw__raw_streamTable = new ConcatPages(StreamTablePages.Pages);
                    __raw_streamTable = _process__raw__raw_streamTable.Decode(__raw__raw_streamTable);
                    var io___raw_streamTable = new KaitaiStream(__raw_streamTable);
                    _streamTable = new PdbStreamTable(io___raw_streamTable, this, m_root);
                    f_streamTable = true;
                    return _streamTable;
                }
            }
            private PdbHeaderJg _header;
            private PdbPageNumberList _streamTablePages;
            private MsPdb m_root;
            private MsPdb m_parent;
            private byte[] __raw_streamTablePages;
            private byte[] __raw_streamTable;
            private byte[] __raw__raw_streamTable;
            public PdbHeaderJg Header { get { return _header; } }
            public PdbPageNumberList StreamTablePages { get { return _streamTablePages; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb M_Parent { get { return m_parent; } }
            public byte[] M_RawStreamTablePages { get { return __raw_streamTablePages; } }
            public byte[] M_RawStreamTable { get { return __raw_streamTable; } }
            public byte[] M_RawM_RawStreamTable { get { return __raw__raw_streamTable; } }
        }

        /// <remarks>
        /// Reference: DATASYM32
        /// </remarks>
        public partial class SymData32 : KaitaiStruct
        {
            public SymData32(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private TpiTypeRef _type;
            private uint _offset;
            private ushort _segment;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// Type index, or Metadata token if a managed symbol
            /// </summary>
            /// <remarks>
            /// Reference: typind
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <summary>
            /// Length-prefixed name
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CV_funcattr_t
        /// </remarks>
        public partial class CvFuncAttributes : KaitaiStruct
        {
            public static CvFuncAttributes FromFile(string fileName)
            {
                return new CvFuncAttributes(new KaitaiStream(fileName));
            }

            public CvFuncAttributes(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _cxxReturnUdt = m_io.ReadBitsIntLe(1) != 0;
                _isConstructor = m_io.ReadBitsIntLe(1) != 0;
                _isVirtualConstructor = m_io.ReadBitsIntLe(1) != 0;
                __unnamed3 = m_io.ReadBitsIntLe(5);
            }
            private bool _cxxReturnUdt;
            private bool _isConstructor;
            private bool _isVirtualConstructor;
            private ulong __unnamed3;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// true if C++ style ReturnUDT
            /// </summary>
            /// <remarks>
            /// Reference: cxxreturnudt
            /// </remarks>
            public bool CxxReturnUdt { get { return _cxxReturnUdt; } }

            /// <summary>
            /// true if func is an instance constructor
            /// </summary>
            /// <remarks>
            /// Reference: ctor
            /// </remarks>
            public bool IsConstructor { get { return _isConstructor; } }

            /// <summary>
            /// true if func is an instance constructor of a class with virtual bases
            /// </summary>
            /// <remarks>
            /// Reference: ctorvbase
            /// </remarks>
            public bool IsVirtualConstructor { get { return _isVirtualConstructor; } }

            /// <summary>
            /// unused
            /// </summary>
            /// <remarks>
            /// Reference: unused
            /// </remarks>
            public ulong Unnamed_3 { get { return __unnamed3; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: SC2
        /// </remarks>
        public partial class SectionContrib2 : KaitaiStruct
        {
            public static SectionContrib2 FromFile(string fileName)
            {
                return new SectionContrib2(new KaitaiStream(fileName));
            }

            public SectionContrib2(KaitaiStream p__io, MsPdb.SectionContributionList p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _base = new SectionContrib(m_io, this, m_root);
                _coffSectionIndex = m_io.ReadU4le();
            }
            private SectionContrib _base;
            private uint _coffSectionIndex;
            private MsPdb m_root;
            private MsPdb.SectionContributionList m_parent;
            public SectionContrib Base { get { return _base; } }

            /// <remarks>
            /// Reference: isectCoff
            /// </remarks>
            public uint CoffSectionIndex { get { return _coffSectionIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.SectionContributionList M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: MANPROCSYM
        /// </remarks>
        public partial class SymManproc : KaitaiStruct
        {
            public SymManproc(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _parent = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _end = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _next = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _length = m_io.ReadU4le();
                _dbgStart = m_io.ReadU4le();
                _dbgEnd = m_io.ReadU4le();
                _token = m_io.ReadU4le();
                _offset = m_io.ReadU4le();
                _segment = m_io.ReadU2le();
                _flags = new CvProcFlags(m_io, this, m_root);
                _returnRegister = m_io.ReadU2le();
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private DbiSymbolRef _parent;
            private DbiSymbolRef _end;
            private DbiSymbolRef _next;
            private uint _length;
            private uint _dbgStart;
            private uint _dbgEnd;
            private uint _token;
            private uint _offset;
            private ushort _segment;
            private CvProcFlags _flags;
            private ushort _returnRegister;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// pointer to the parent
            /// </summary>
            /// <remarks>
            /// Reference: pParent
            /// </remarks>
            public DbiSymbolRef Parent { get { return _parent; } }

            /// <summary>
            /// pointer to this blocks end
            /// </summary>
            /// <remarks>
            /// Reference: pEnd
            /// </remarks>
            public DbiSymbolRef End { get { return _end; } }

            /// <summary>
            /// pointer to next symbol
            /// </summary>
            /// <remarks>
            /// Reference: pNext
            /// </remarks>
            public DbiSymbolRef Next { get { return _next; } }

            /// <summary>
            /// Proc length
            /// </summary>
            /// <remarks>
            /// Reference: len
            /// </remarks>
            public uint Length { get { return _length; } }

            /// <summary>
            /// Debug start offset
            /// </summary>
            /// <remarks>
            /// Reference: DbgStart
            /// </remarks>
            public uint DbgStart { get { return _dbgStart; } }

            /// <summary>
            /// Debug end offset
            /// </summary>
            /// <remarks>
            /// Reference: DbgEnd
            /// </remarks>
            public uint DbgEnd { get { return _dbgEnd; } }

            /// <summary>
            /// COM+ metadata token for method
            /// </summary>
            /// <remarks>
            /// Reference: token
            /// </remarks>
            public uint Token { get { return _token; } }

            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <remarks>
            /// Reference: seg
            /// </remarks>
            public ushort Segment { get { return _segment; } }

            /// <summary>
            /// Proc flags
            /// </summary>
            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public CvProcFlags Flags { get { return _flags; } }

            /// <summary>
            /// Register return value is in (may not be used for all archs)
            /// </summary>
            /// <remarks>
            /// Reference: retReg
            /// </remarks>
            public ushort ReturnRegister { get { return _returnRegister; } }

            /// <summary>
            /// optional name field
            /// </summary>
            /// <remarks>
            /// Reference: name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class PdbStreamRefX : KaitaiStruct
        {
            public PdbStreamRefX(short p_streamNumber, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _streamNumber = p_streamNumber;
                f_zzzSize = false;
                f_size = false;
                f_data = false;
                f_isValidStream = false;
                f_zzzData = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_zzzSize;
            private GetStreamSize _zzzSize;
            public GetStreamSize ZzzSize
            {
                get
                {
                    if (f_zzzSize)
                        return _zzzSize;
                    if (IsValidStream) {
                        _zzzSize = new GetStreamSize(((uint) (StreamNumber)), m_io, this, m_root);
                        f_zzzSize = true;
                    }
                    return _zzzSize;
                }
            }
            private bool f_size;
            private int _size;
            public int Size
            {
                get
                {
                    if (f_size)
                        return _size;
                    _size = (int) ((IsValidStream ? ZzzSize.Value : 0));
                    f_size = true;
                    return _size;
                }
            }
            private bool f_data;
            private byte[] _data;
            public byte[] Data
            {
                get
                {
                    if (f_data)
                        return _data;
                    if (IsValidStream) {
                        _data = (byte[]) (ZzzData.Value);
                    }
                    f_data = true;
                    return _data;
                }
            }
            private bool f_isValidStream;
            private bool _isValidStream;
            public bool IsValidStream
            {
                get
                {
                    if (f_isValidStream)
                        return _isValidStream;
                    _isValidStream = (bool) ( ((StreamNumber > -1) && (StreamNumber < M_Root.NumStreams)) );
                    f_isValidStream = true;
                    return _isValidStream;
                }
            }
            private bool f_zzzData;
            private GetStreamData _zzzData;
            public GetStreamData ZzzData
            {
                get
                {
                    if (f_zzzData)
                        return _zzzData;
                    if (IsValidStream) {
                        _zzzData = new GetStreamData(((uint) (StreamNumber)), m_io, this, m_root);
                        f_zzzData = true;
                    }
                    return _zzzData;
                }
            }
            private short _streamNumber;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public short StreamNumber { get { return _streamNumber; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_MEMBER
        /// </summary>
        /// <remarks>
        /// Reference: lfMember
        /// </remarks>
        public partial class LfMember : KaitaiStruct
        {
            public LfMember(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _attributes = new CvFieldAttributes(m_io, this, m_root);
                _fieldType = new TpiTypeRef(m_io, this, m_root);
                _offset = new CvNumericType(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private CvFieldAttributes _attributes;
            private TpiTypeRef _fieldType;
            private CvNumericType _offset;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// attribute mask
            /// </summary>
            /// <remarks>
            /// Reference: attr
            /// </remarks>
            public CvFieldAttributes Attributes { get { return _attributes; } }

            /// <summary>
            /// index of type record for field
            /// </summary>
            /// <remarks>
            /// Reference: index
            /// </remarks>
            public TpiTypeRef FieldType { get { return _fieldType; } }

            /// <summary>
            /// variable length offset of field
            /// </summary>
            /// <remarks>
            /// Reference: offset
            /// </remarks>
            public CvNumericType Offset { get { return _offset; } }

            /// <summary>
            /// length prefixed name of field
            /// </summary>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// LF_ENUM
        /// </summary>
        /// <remarks>
        /// Reference: lfEnum
        /// </remarks>
        public partial class LfEnum : KaitaiStruct
        {
            public LfEnum(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _numElements = m_io.ReadU2le();
                _typeProperties = new CvProperties(m_io, this, m_root);
                _underlyingType = new TpiTypeRef(m_io, this, m_root);
                _fieldType = new TpiTypeRef(m_io, this, m_root);
                _name = new PdbString(StringPrefixed, m_io, this, m_root);
            }
            private ushort _numElements;
            private CvProperties _typeProperties;
            private TpiTypeRef _underlyingType;
            private TpiTypeRef _fieldType;
            private PdbString _name;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// count of number of elements in class
            /// </summary>
            /// <remarks>
            /// Reference: count
            /// </remarks>
            public ushort NumElements { get { return _numElements; } }

            /// <summary>
            /// property attribute field
            /// </summary>
            /// <remarks>
            /// Reference: property
            /// </remarks>
            public CvProperties TypeProperties { get { return _typeProperties; } }

            /// <summary>
            /// underlying type of the enum
            /// </summary>
            /// <remarks>
            /// Reference: utype
            /// </remarks>
            public TpiTypeRef UnderlyingType { get { return _underlyingType; } }

            /// <summary>
            /// type index of LF_FIELD descriptor list
            /// </summary>
            /// <remarks>
            /// Reference: field
            /// </remarks>
            public TpiTypeRef FieldType { get { return _fieldType; } }

            /// <summary>
            /// length prefixed name of enum
            /// </summary>
            /// <remarks>
            /// Reference: Name
            /// </remarks>
            public PdbString Name { get { return _name; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }
        public partial class FileInfo : KaitaiStruct
        {
            public static FileInfo FromFile(string fileName)
            {
                return new FileInfo(new KaitaiStream(fileName));
            }

            public FileInfo(KaitaiStream p__io, MsPdb.Dbi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_stringsStart = false;
                _read();
            }
            private void _read()
            {
                _numModules = m_io.ReadU2le();
                _numReferences = m_io.ReadU2le();
                _moduleToReference = new List<ushort>();
                for (var i = 0; i < NumModules; i++)
                {
                    _moduleToReference.Add(m_io.ReadU2le());
                }
                _referenceToFileIndex = new List<ushort>();
                for (var i = 0; i < NumModules; i++)
                {
                    _referenceToFileIndex.Add(m_io.ReadU2le());
                }
                _filenameIndices = new List<FileInfoString>();
                for (var i = 0; i < NumReferences; i++)
                {
                    _filenameIndices.Add(new FileInfoString(m_io, this, m_root));
                }
                if (StringsStart >= 0) {
                    _invokeStringsStart = m_io.ReadBytes(0);
                }
            }
            private bool f_stringsStart;
            private int _stringsStart;
            public int StringsStart
            {
                get
                {
                    if (f_stringsStart)
                        return _stringsStart;
                    _stringsStart = (int) (M_Io.Pos);
                    f_stringsStart = true;
                    return _stringsStart;
                }
            }
            private ushort _numModules;
            private ushort _numReferences;
            private List<ushort> _moduleToReference;
            private List<ushort> _referenceToFileIndex;
            private List<FileInfoString> _filenameIndices;
            private byte[] _invokeStringsStart;
            private MsPdb m_root;
            private MsPdb.Dbi m_parent;

            /// <remarks>
            /// Reference: imodMac
            /// </remarks>
            public ushort NumModules { get { return _numModules; } }

            /// <remarks>
            /// Reference: cRefs
            /// </remarks>
            public ushort NumReferences { get { return _numReferences; } }

            /// <remarks>
            /// Reference: mpimodiref
            /// </remarks>
            public List<ushort> ModuleToReference { get { return _moduleToReference; } }

            /// <remarks>
            /// Reference: mpimodcref
            /// </remarks>
            public List<ushort> ReferenceToFileIndex { get { return _referenceToFileIndex; } }

            /// <remarks>
            /// Reference: mpirefichFile
            /// </remarks>
            public List<FileInfoString> FilenameIndices { get { return _filenameIndices; } }
            public byte[] InvokeStringsStart { get { return _invokeStringsStart; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Dbi M_Parent { get { return m_parent; } }
        }
        public partial class U4Finder : KaitaiStruct
        {
            public U4Finder(uint p_search, KaitaiStream p__io, MsPdb.PdbStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _search = p_search;
                f_found = false;
                f_startPos = false;
                f_endPos = false;
                _read();
            }
            private void _read()
            {
                _buffer = new List<uint>();
                {
                    var i = 0;
                    uint M_;
                    do {
                        M_ = m_io.ReadU4le();
                        _buffer.Add(M_);
                        i++;
                    } while (!( ((M_ == Search) || (M_Io.IsEof)) ));
                }
            }
            private bool f_found;
            private bool _found;
            public bool Found
            {
                get
                {
                    if (f_found)
                        return _found;
                    _found = (bool) (Buffer[(EndPos - 4)] == Search);
                    f_found = true;
                    return _found;
                }
            }
            private bool f_startPos;
            private int _startPos;
            public int StartPos
            {
                get
                {
                    if (f_startPos)
                        return _startPos;
                    _startPos = (int) (M_Io.Pos);
                    f_startPos = true;
                    return _startPos;
                }
            }
            private bool f_endPos;
            private int _endPos;
            public int EndPos
            {
                get
                {
                    if (f_endPos)
                        return _endPos;
                    _endPos = (int) (M_Io.Pos);
                    f_endPos = true;
                    return _endPos;
                }
            }
            private List<uint> _buffer;
            private uint _search;
            private MsPdb m_root;
            private MsPdb.PdbStream m_parent;
            public List<uint> Buffer { get { return _buffer; } }
            public uint Search { get { return _search; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.PdbStream M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CV_LVARFLAGS
        /// </remarks>
        public partial class CvLocalVarFlags : KaitaiStruct
        {
            public static CvLocalVarFlags FromFile(string fileName)
            {
                return new CvLocalVarFlags(new KaitaiStream(fileName));
            }

            public CvLocalVarFlags(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _isParam = m_io.ReadBitsIntLe(1) != 0;
                _addrTaken = m_io.ReadBitsIntLe(1) != 0;
                _compGenx = m_io.ReadBitsIntLe(1) != 0;
                _isAggregate = m_io.ReadBitsIntLe(1) != 0;
                _isAggregated = m_io.ReadBitsIntLe(1) != 0;
                _isAliased = m_io.ReadBitsIntLe(1) != 0;
                _isAlias = m_io.ReadBitsIntLe(1) != 0;
                _isReturnValue = m_io.ReadBitsIntLe(1) != 0;
                _isOptimizedOut = m_io.ReadBitsIntLe(1) != 0;
                _isEnregisteredGlobal = m_io.ReadBitsIntLe(1) != 0;
                _isEnregisteredStatic = m_io.ReadBitsIntLe(1) != 0;
                _unused = m_io.ReadBitsIntLe(5);
            }
            private bool _isParam;
            private bool _addrTaken;
            private bool _compGenx;
            private bool _isAggregate;
            private bool _isAggregated;
            private bool _isAliased;
            private bool _isAlias;
            private bool _isReturnValue;
            private bool _isOptimizedOut;
            private bool _isEnregisteredGlobal;
            private bool _isEnregisteredStatic;
            private ulong _unused;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// variable is a parameter
            /// </summary>
            /// <remarks>
            /// Reference: fIsParam
            /// </remarks>
            public bool IsParam { get { return _isParam; } }

            /// <summary>
            /// address is taken
            /// </summary>
            /// <remarks>
            /// Reference: fAddrTaken
            /// </remarks>
            public bool AddrTaken { get { return _addrTaken; } }

            /// <summary>
            /// variable is compiler generated
            /// </summary>
            /// <remarks>
            /// Reference: fCompGenx
            /// </remarks>
            public bool CompGenx { get { return _compGenx; } }

            /// <summary>
            /// the symbol is splitted in temporaries, which are treated by compiler as independent entities
            /// </summary>
            /// <remarks>
            /// Reference: fIsAggregate
            /// </remarks>
            public bool IsAggregate { get { return _isAggregate; } }

            /// <summary>
            /// Counterpart of fIsAggregate - tells that it is a part of a fIsAggregate symbol
            /// </summary>
            /// <remarks>
            /// Reference: fIsAggregated
            /// </remarks>
            public bool IsAggregated { get { return _isAggregated; } }

            /// <summary>
            /// variable has multiple simultaneous lifetimes
            /// </summary>
            /// <remarks>
            /// Reference: fIsAliased
            /// </remarks>
            public bool IsAliased { get { return _isAliased; } }

            /// <summary>
            /// represents one of the multiple simultaneous lifetimes
            /// </summary>
            /// <remarks>
            /// Reference: fIsAlias
            /// </remarks>
            public bool IsAlias { get { return _isAlias; } }

            /// <summary>
            /// represents a function return value
            /// </summary>
            /// <remarks>
            /// Reference: fIsRetValue
            /// </remarks>
            public bool IsReturnValue { get { return _isReturnValue; } }

            /// <summary>
            /// variable has no lifetimes
            /// </summary>
            /// <remarks>
            /// Reference: fIsOptimizedOut
            /// </remarks>
            public bool IsOptimizedOut { get { return _isOptimizedOut; } }

            /// <summary>
            /// variable is an enregistered global
            /// </summary>
            /// <remarks>
            /// Reference: fIsEnregGlob
            /// </remarks>
            public bool IsEnregisteredGlobal { get { return _isEnregisteredGlobal; } }

            /// <summary>
            /// variable is an enregistered static
            /// </summary>
            /// <remarks>
            /// Reference: fIsEnregStat
            /// </remarks>
            public bool IsEnregisteredStatic { get { return _isEnregisteredStatic; } }

            /// <summary>
            /// must be zero
            /// </summary>
            /// <remarks>
            /// Reference: unused
            /// </remarks>
            public ulong Unused { get { return _unused; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TiOffset : KaitaiStruct
        {
            public TiOffset(uint p_index, KaitaiStream p__io, MsPdb.TiOffsetList p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _index = p_index;
                f_hasNextBlock = false;
                f_nextBlock = false;
                f_blockEnd = false;
                f_blockLength = false;
                _read();
            }
            private void _read()
            {
                _typeIndex = m_io.ReadU4le();
                _offset = m_io.ReadU4le();
            }
            private bool f_hasNextBlock;
            private bool _hasNextBlock;
            public bool HasNextBlock
            {
                get
                {
                    if (f_hasNextBlock)
                        return _hasNextBlock;
                    _hasNextBlock = (bool) ((Index + 1) < M_Parent.NumItems);
                    f_hasNextBlock = true;
                    return _hasNextBlock;
                }
            }
            private bool f_nextBlock;
            private TiOffset _nextBlock;
            public TiOffset NextBlock
            {
                get
                {
                    if (f_nextBlock)
                        return _nextBlock;
                    if (HasNextBlock) {
                        _nextBlock = (TiOffset) (M_Parent.Items[((int) ((Index + 1)))]);
                    }
                    f_nextBlock = true;
                    return _nextBlock;
                }
            }
            private bool f_blockEnd;
            private uint _blockEnd;
            public uint BlockEnd
            {
                get
                {
                    if (f_blockEnd)
                        return _blockEnd;
                    _blockEnd = (uint) ((HasNextBlock == true ? NextBlock.TypeIndex : M_Root.TpiStream.MaxTypeIndex));
                    f_blockEnd = true;
                    return _blockEnd;
                }
            }
            private bool f_blockLength;
            private int _blockLength;
            public int BlockLength
            {
                get
                {
                    if (f_blockLength)
                        return _blockLength;
                    _blockLength = (int) ((BlockEnd - TypeIndex));
                    f_blockLength = true;
                    return _blockLength;
                }
            }
            private uint _typeIndex;
            private uint _offset;
            private uint _index;
            private MsPdb m_root;
            private MsPdb.TiOffsetList m_parent;
            public uint TypeIndex { get { return _typeIndex; } }
            public uint Offset { get { return _offset; } }
            public uint Index { get { return _index; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TiOffsetList M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: CV_Line_t
        /// </remarks>
        public partial class C13Line : KaitaiStruct
        {
            public static C13Line FromFile(string fileName)
            {
                return new C13Line(new KaitaiStream(fileName));
            }

            public C13Line(KaitaiStream p__io, MsPdb.C13FileBlock p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_isSpecialNotStepOnto = false;
                f_isSpecialNotStepInto = false;
                _read();
            }
            private void _read()
            {
                _offset = m_io.ReadU4le();
                _linenumStart = m_io.ReadBitsIntLe(24);
                _deltaLineEnd = m_io.ReadBitsIntLe(7);
                _isStatement = m_io.ReadBitsIntLe(1) != 0;
            }
            private bool f_isSpecialNotStepOnto;
            private bool _isSpecialNotStepOnto;

            /// <summary>
            /// The compiler will generate special line numbers like 0xfeefee (not to step onto)
            /// </summary>
            public bool IsSpecialNotStepOnto
            {
                get
                {
                    if (f_isSpecialNotStepOnto)
                        return _isSpecialNotStepOnto;
                    _isSpecialNotStepOnto = (bool) (LinenumStart == 16707566);
                    f_isSpecialNotStepOnto = true;
                    return _isSpecialNotStepOnto;
                }
            }
            private bool f_isSpecialNotStepInto;
            private bool _isSpecialNotStepInto;

            /// <summary>
            /// The compiler will generate special line numbers like 0xf00f00 (not to step into)
            /// </summary>
            public bool IsSpecialNotStepInto
            {
                get
                {
                    if (f_isSpecialNotStepInto)
                        return _isSpecialNotStepInto;
                    _isSpecialNotStepInto = (bool) (LinenumStart == 15732480);
                    f_isSpecialNotStepInto = true;
                    return _isSpecialNotStepInto;
                }
            }
            private uint _offset;
            private ulong _linenumStart;
            private ulong _deltaLineEnd;
            private bool _isStatement;
            private MsPdb m_root;
            private MsPdb.C13FileBlock m_parent;

            /// <summary>
            /// Offset to start of code bytes for line number
            /// </summary>
            /// <remarks>
            /// Reference: offset
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// line where statement/expression starts
            /// </summary>
            /// <remarks>
            /// Reference: linenumStart
            /// </remarks>
            public ulong LinenumStart { get { return _linenumStart; } }

            /// <summary>
            /// delta to line where statement ends (optional)
            /// </summary>
            /// <remarks>
            /// Reference: deltaLineEnd
            /// </remarks>
            public ulong DeltaLineEnd { get { return _deltaLineEnd; } }

            /// <summary>
            /// true if a statement linenumber, else an expression line num
            /// </summary>
            /// <remarks>
            /// Reference: fStatement
            /// </remarks>
            public bool IsStatement { get { return _isStatement; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13FileBlock M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: COMPILESYM
        /// </remarks>
        public partial class SymCompile2 : KaitaiStruct
        {
            public SymCompile2(bool p_stringPrefixed, KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _stringPrefixed = p_stringPrefixed;
                _read();
            }
            private void _read()
            {
                _flags = new SymCompile2Flags(m_io, this, m_root);
                _machine = m_io.ReadU2le();
                _verFeMajor = m_io.ReadU2le();
                _verFeMinor = m_io.ReadU2le();
                _verFeBuild = m_io.ReadU2le();
                _verMajor = m_io.ReadU2le();
                _verMinor = m_io.ReadU2le();
                _verBuild = m_io.ReadU2le();
                _versionString = new PdbString(StringPrefixed, m_io, this, m_root);
                _stringsBlock = new List<string>();
                {
                    var i = 0;
                    string M_;
                    do {
                        M_ = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytesTerm(0, false, true, true));
                        _stringsBlock.Add(M_);
                        i++;
                    } while (!(M_ == ""));
                }
            }
            private SymCompile2Flags _flags;
            private ushort _machine;
            private ushort _verFeMajor;
            private ushort _verFeMinor;
            private ushort _verFeBuild;
            private ushort _verMajor;
            private ushort _verMinor;
            private ushort _verBuild;
            private PdbString _versionString;
            private List<string> _stringsBlock;
            private bool _stringPrefixed;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// flags
            /// </summary>
            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public SymCompile2Flags Flags { get { return _flags; } }

            /// <summary>
            /// target processor
            /// </summary>
            /// <remarks>
            /// Reference: machine
            /// </remarks>
            public ushort Machine { get { return _machine; } }

            /// <summary>
            /// front end major version #
            /// </summary>
            /// <remarks>
            /// Reference: verFEMajor
            /// </remarks>
            public ushort VerFeMajor { get { return _verFeMajor; } }

            /// <summary>
            /// front end minor version #
            /// </summary>
            /// <remarks>
            /// Reference: verFEMinor
            /// </remarks>
            public ushort VerFeMinor { get { return _verFeMinor; } }

            /// <summary>
            /// front end build version #
            /// </summary>
            /// <remarks>
            /// Reference: verFEBuild
            /// </remarks>
            public ushort VerFeBuild { get { return _verFeBuild; } }

            /// <summary>
            /// back end major version #
            /// </summary>
            /// <remarks>
            /// Reference: verMajor
            /// </remarks>
            public ushort VerMajor { get { return _verMajor; } }

            /// <summary>
            /// back end minor version #
            /// </summary>
            /// <remarks>
            /// Reference: verMinor
            /// </remarks>
            public ushort VerMinor { get { return _verMinor; } }

            /// <summary>
            /// back end build version #
            /// </summary>
            /// <remarks>
            /// Reference: verBuild
            /// </remarks>
            public ushort VerBuild { get { return _verBuild; } }

            /// <summary>
            /// Length-prefixed compiler version string
            /// </summary>
            /// <remarks>
            /// Reference: verSt
            /// </remarks>
            public PdbString VersionString { get { return _versionString; } }

            /// <summary>
            /// an optional block of zero terminated strings, terminated with a double zero.
            /// </summary>
            public List<string> StringsBlock { get { return _stringsBlock; } }
            public bool StringPrefixed { get { return _stringPrefixed; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: DEBUG_S_INLINEELINES
        /// </remarks>
        public partial class C13SubsectionInlineeLines : KaitaiStruct
        {
            public static C13SubsectionInlineeLines FromFile(string fileName)
            {
                return new C13SubsectionInlineeLines(new KaitaiStream(fileName));
            }


            public enum SignatureEnum
            {
                Signature = 0,
                SignatureEx = 1,
            }
            public C13SubsectionInlineeLines(KaitaiStream p__io, MsPdb.C13Subsection p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _signature = ((SignatureEnum) m_io.ReadU4le());
                if (Signature == SignatureEnum.Signature) {
                    _lines = new List<C13InlineeSourceLine>();
                    {
                        var i = 0;
                        while (!m_io.IsEof) {
                            _lines.Add(new C13InlineeSourceLine(m_io, this, m_root));
                            i++;
                        }
                    }
                }
                if (Signature == SignatureEnum.SignatureEx) {
                    _linesEx = new List<C13InlineeSourceLineEx>();
                    {
                        var i = 0;
                        while (!m_io.IsEof) {
                            _linesEx.Add(new C13InlineeSourceLineEx(m_io, this, m_root));
                            i++;
                        }
                    }
                }
            }
            private SignatureEnum _signature;
            private List<C13InlineeSourceLine> _lines;
            private List<C13InlineeSourceLineEx> _linesEx;
            private MsPdb m_root;
            private MsPdb.C13Subsection m_parent;
            public SignatureEnum Signature { get { return _signature; } }
            public List<C13InlineeSourceLine> Lines { get { return _lines; } }
            public List<C13InlineeSourceLineEx> LinesEx { get { return _linesEx; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.C13Subsection M_Parent { get { return m_parent; } }
        }
        public partial class DbiHeaderOld : KaitaiStruct
        {
            public static DbiHeaderOld FromFile(string fileName)
            {
                return new DbiHeaderOld(new KaitaiStream(fileName));
            }

            public DbiHeaderOld(KaitaiStream p__io, MsPdb.Dbi p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_symbolsData = false;
                f_gsSymbolsData = false;
                f_psSymbolsData = false;
                _read();
            }
            private void _read()
            {
                _gsSymbolsStream = new PdbStreamRef(m_io, this, m_root);
                _psSymbolsStream = new PdbStreamRef(m_io, this, m_root);
                _symbolRecordsStream = new PdbStreamRef(m_io, this, m_root);
                _moduleListSize = m_io.ReadU4le();
                _sectionContributionSize = m_io.ReadU4le();
                _sectionMapSize = m_io.ReadU4le();
            }
            private bool f_symbolsData;
            private SymbolRecordsStream _symbolsData;
            public SymbolRecordsStream SymbolsData
            {
                get
                {
                    if (f_symbolsData)
                        return _symbolsData;
                    if (SymbolRecordsStream.StreamNumber > -1) {
                        __raw__raw_symbolsData = m_io.ReadBytes(0);
                        Cat _process__raw__raw_symbolsData = new Cat(SymbolRecordsStream.Data);
                        __raw_symbolsData = _process__raw__raw_symbolsData.Decode(__raw__raw_symbolsData);
                        var io___raw_symbolsData = new KaitaiStream(__raw_symbolsData);
                        _symbolsData = new SymbolRecordsStream(io___raw_symbolsData, this, m_root);
                        f_symbolsData = true;
                    }
                    return _symbolsData;
                }
            }
            private bool f_gsSymbolsData;
            private GlobalSymbolsStream _gsSymbolsData;
            public GlobalSymbolsStream GsSymbolsData
            {
                get
                {
                    if (f_gsSymbolsData)
                        return _gsSymbolsData;
                    if (GsSymbolsStream.StreamNumber > -1) {
                        __raw__raw_gsSymbolsData = m_io.ReadBytes(0);
                        Cat _process__raw__raw_gsSymbolsData = new Cat(GsSymbolsStream.Data);
                        __raw_gsSymbolsData = _process__raw__raw_gsSymbolsData.Decode(__raw__raw_gsSymbolsData);
                        var io___raw_gsSymbolsData = new KaitaiStream(__raw_gsSymbolsData);
                        _gsSymbolsData = new GlobalSymbolsStream(io___raw_gsSymbolsData, this, m_root);
                        f_gsSymbolsData = true;
                    }
                    return _gsSymbolsData;
                }
            }
            private bool f_psSymbolsData;
            private PublicSymbolsStream _psSymbolsData;
            public PublicSymbolsStream PsSymbolsData
            {
                get
                {
                    if (f_psSymbolsData)
                        return _psSymbolsData;
                    if (PsSymbolsStream.StreamNumber > -1) {
                        __raw__raw_psSymbolsData = m_io.ReadBytes(0);
                        Cat _process__raw__raw_psSymbolsData = new Cat(PsSymbolsStream.Data);
                        __raw_psSymbolsData = _process__raw__raw_psSymbolsData.Decode(__raw__raw_psSymbolsData);
                        var io___raw_psSymbolsData = new KaitaiStream(__raw_psSymbolsData);
                        _psSymbolsData = new PublicSymbolsStream(io___raw_psSymbolsData, this, m_root);
                        f_psSymbolsData = true;
                    }
                    return _psSymbolsData;
                }
            }
            private PdbStreamRef _gsSymbolsStream;
            private PdbStreamRef _psSymbolsStream;
            private PdbStreamRef _symbolRecordsStream;
            private uint _moduleListSize;
            private uint _sectionContributionSize;
            private uint _sectionMapSize;
            private MsPdb m_root;
            private MsPdb.Dbi m_parent;
            private byte[] __raw_symbolsData;
            private byte[] __raw__raw_symbolsData;
            private byte[] __raw_gsSymbolsData;
            private byte[] __raw__raw_gsSymbolsData;
            private byte[] __raw_psSymbolsData;
            private byte[] __raw__raw_psSymbolsData;
            public PdbStreamRef GsSymbolsStream { get { return _gsSymbolsStream; } }
            public PdbStreamRef PsSymbolsStream { get { return _psSymbolsStream; } }
            public PdbStreamRef SymbolRecordsStream { get { return _symbolRecordsStream; } }
            public uint ModuleListSize { get { return _moduleListSize; } }
            public uint SectionContributionSize { get { return _sectionContributionSize; } }
            public uint SectionMapSize { get { return _sectionMapSize; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.Dbi M_Parent { get { return m_parent; } }
            public byte[] M_RawSymbolsData { get { return __raw_symbolsData; } }
            public byte[] M_RawM_RawSymbolsData { get { return __raw__raw_symbolsData; } }
            public byte[] M_RawGsSymbolsData { get { return __raw_gsSymbolsData; } }
            public byte[] M_RawM_RawGsSymbolsData { get { return __raw__raw_gsSymbolsData; } }
            public byte[] M_RawPsSymbolsData { get { return __raw_psSymbolsData; } }
            public byte[] M_RawM_RawPsSymbolsData { get { return __raw__raw_psSymbolsData; } }
        }
        public partial class NameTableStrings : KaitaiStruct
        {
            public static NameTableStrings FromFile(string fileName)
            {
                return new NameTableStrings(new KaitaiStream(fileName));
            }

            public NameTableStrings(KaitaiStream p__io, MsPdb.NameTable p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _strings = new List<NameTableString>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _strings.Add(new NameTableString(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<NameTableString> _strings;
            private MsPdb m_root;
            private MsPdb.NameTable m_parent;
            public List<NameTableString> Strings { get { return _strings; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.NameTable M_Parent { get { return m_parent; } }
        }
        public partial class ModuleSymbols : KaitaiStruct
        {
            public ModuleSymbols(uint p_moduleIndex, KaitaiStream p__io, MsPdb.ModuleStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _moduleIndex = p_moduleIndex;
                f_items = false;
                _read();
            }
            private void _read()
            {
            }
            private bool f_items;
            private List<DbiSymbol> _items;
            public List<DbiSymbol> Items
            {
                get
                {
                    if (f_items)
                        return _items;
                    long _pos = m_io.Pos;
                    m_io.Seek(0);
                    _items = new List<DbiSymbol>();
                    {
                        var i = 0;
                        while (!m_io.IsEof) {
                            _items.Add(new DbiSymbol(((int) (ModuleIndex)), m_io, this, m_root));
                            i++;
                        }
                    }
                    m_io.Seek(_pos);
                    f_items = true;
                    return _items;
                }
            }
            private uint _moduleIndex;
            private MsPdb m_root;
            private MsPdb.ModuleStream m_parent;
            public uint ModuleIndex { get { return _moduleIndex; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.ModuleStream M_Parent { get { return m_parent; } }
        }
        public partial class TpiType : KaitaiStruct
        {
            public TpiType(uint p_ti, KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _ti = p_ti;
                f_dataPos = false;
                f_data = false;
                _read();
            }
            private void _read()
            {
                if (M_Root.PdbType == MsPdb.PdbTypeEnum.Old) {
                    _hash = m_io.ReadU2le();
                }
                _length = m_io.ReadU2le();
                if (DataPos >= 0) {
                    _invokeDataPos = m_io.ReadBytes(0);
                }
                __unnamed3 = m_io.ReadBytes(Length);
            }
            private bool f_dataPos;
            private int _dataPos;
            public int DataPos
            {
                get
                {
                    if (f_dataPos)
                        return _dataPos;
                    _dataPos = (int) (M_Io.Pos);
                    f_dataPos = true;
                    return _dataPos;
                }
            }
            private bool f_data;
            private TpiTypeData _data;
            public TpiTypeData Data
            {
                get
                {
                    if (f_data)
                        return _data;
                    if (Length > 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(DataPos);
                        __raw_data = m_io.ReadBytes(Length);
                        var io___raw_data = new KaitaiStream(__raw_data);
                        _data = new TpiTypeData(false, io___raw_data, this, m_root);
                        m_io.Seek(_pos);
                        f_data = true;
                    }
                    return _data;
                }
            }
            private ushort? _hash;
            private ushort _length;
            private byte[] _invokeDataPos;
            private byte[] __unnamed3;
            private uint _ti;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            private byte[] __raw_data;
            public ushort? Hash { get { return _hash; } }
            public ushort Length { get { return _length; } }
            public byte[] InvokeDataPos { get { return _invokeDataPos; } }
            public byte[] Unnamed_3 { get { return __unnamed3; } }
            public uint Ti { get { return _ti; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
            public byte[] M_RawData { get { return __raw_data; } }
        }

        /// <summary>
        /// LF_BITFIELD
        /// </summary>
        /// <remarks>
        /// Reference: lfBitfield
        /// </remarks>
        public partial class LfBitfield : KaitaiStruct
        {
            public static LfBitfield FromFile(string fileName)
            {
                return new LfBitfield(new KaitaiStream(fileName));
            }

            public LfBitfield(KaitaiStream p__io, MsPdb.TpiTypeData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = new TpiTypeRef(m_io, this, m_root);
                _length = m_io.ReadU1();
                _position = m_io.ReadU1();
            }
            private TpiTypeRef _type;
            private byte _length;
            private byte _position;
            private MsPdb m_root;
            private MsPdb.TpiTypeData m_parent;

            /// <summary>
            /// type of bitfield
            /// </summary>
            /// <remarks>
            /// Reference: type
            /// </remarks>
            public TpiTypeRef Type { get { return _type; } }

            /// <remarks>
            /// Reference: length
            /// </remarks>
            public byte Length { get { return _length; } }

            /// <remarks>
            /// Reference: position
            /// </remarks>
            public byte Position { get { return _position; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.TpiTypeData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: FPO_DATA
        /// </remarks>
        public partial class FpoData : KaitaiStruct
        {
            public static FpoData FromFile(string fileName)
            {
                return new FpoData(new KaitaiStream(fileName));
            }


            public enum FrameTypeEnum
            {
                Fpo = 0,
                Trap = 1,
                Tss = 2,
                Std = 3,
            }
            public FpoData(KaitaiStream p__io, MsPdb.FpoStream p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _startOffset = m_io.ReadU4le();
                _procSize = m_io.ReadU4le();
                _numDwordsLocals = m_io.ReadU4le();
                _numDwordsParams = m_io.ReadU2le();
                _prologSize = m_io.ReadU1();
                _regsSize = m_io.ReadBitsIntLe(3);
                _hasSeh = m_io.ReadBitsIntLe(1) != 0;
                _useBp = m_io.ReadBitsIntLe(1) != 0;
                _reserved = m_io.ReadBitsIntLe(1) != 0;
                _frameType = ((FrameTypeEnum) m_io.ReadBitsIntLe(2));
            }
            private uint _startOffset;
            private uint _procSize;
            private uint _numDwordsLocals;
            private ushort _numDwordsParams;
            private byte _prologSize;
            private ulong _regsSize;
            private bool _hasSeh;
            private bool _useBp;
            private bool _reserved;
            private FrameTypeEnum _frameType;
            private MsPdb m_root;
            private MsPdb.FpoStream m_parent;

            /// <summary>
            /// offset 1st byte of function code
            /// </summary>
            /// <remarks>
            /// Reference: ulOffStart
            /// </remarks>
            public uint StartOffset { get { return _startOffset; } }

            /// <summary>
            /// # bytes in function
            /// </summary>
            /// <remarks>
            /// Reference: cbProcSize
            /// </remarks>
            public uint ProcSize { get { return _procSize; } }

            /// <summary>
            /// # bytes in locals/4
            /// </summary>
            /// <remarks>
            /// Reference: cdwLocals
            /// </remarks>
            public uint NumDwordsLocals { get { return _numDwordsLocals; } }

            /// <summary>
            /// # bytes in params/4
            /// </summary>
            /// <remarks>
            /// Reference: cdwParams
            /// </remarks>
            public ushort NumDwordsParams { get { return _numDwordsParams; } }

            /// <summary>
            /// # bytes in prolog
            /// </summary>
            /// <remarks>
            /// Reference: cbProlog
            /// </remarks>
            public byte PrologSize { get { return _prologSize; } }

            /// <summary>
            /// # regs saved
            /// </summary>
            /// <remarks>
            /// Reference: cbRegs
            /// </remarks>
            public ulong RegsSize { get { return _regsSize; } }

            /// <summary>
            /// TRUE if SEH in func
            /// </summary>
            /// <remarks>
            /// Reference: fHasSEH
            /// </remarks>
            public bool HasSeh { get { return _hasSeh; } }

            /// <summary>
            /// TRUE if EBP has been allocated
            /// </summary>
            /// <remarks>
            /// Reference: fUseBP
            /// </remarks>
            public bool UseBp { get { return _useBp; } }

            /// <summary>
            /// reserved for future use
            /// </summary>
            /// <remarks>
            /// Reference: reserved
            /// </remarks>
            public bool Reserved { get { return _reserved; } }

            /// <summary>
            /// frame type
            /// </summary>
            /// <remarks>
            /// Reference: cbFrame
            /// </remarks>
            public FrameTypeEnum FrameType { get { return _frameType; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.FpoStream M_Parent { get { return m_parent; } }
        }
        public partial class SymSepcode : KaitaiStruct
        {
            public static SymSepcode FromFile(string fileName)
            {
                return new SymSepcode(new KaitaiStream(fileName));
            }

            public SymSepcode(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _parent = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _end = new DbiSymbolRef(((uint) (M_Parent.ModuleIndex)), m_io, this, m_root);
                _length = m_io.ReadU4le();
                _scf = new CvSepcodeFlags(m_io, this, m_root);
                _offset = m_io.ReadU4le();
                _parentOffset = m_io.ReadU4le();
                _section = m_io.ReadU2le();
                _parentSection = m_io.ReadU2le();
            }
            private DbiSymbolRef _parent;
            private DbiSymbolRef _end;
            private uint _length;
            private CvSepcodeFlags _scf;
            private uint _offset;
            private uint _parentOffset;
            private ushort _section;
            private ushort _parentSection;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// pointer to the parent
            /// </summary>
            /// <remarks>
            /// Reference: pParent
            /// </remarks>
            public DbiSymbolRef Parent { get { return _parent; } }

            /// <summary>
            /// pointer to this blocks end
            /// </summary>
            /// <remarks>
            /// Reference: pEnd
            /// </remarks>
            public DbiSymbolRef End { get { return _end; } }

            /// <summary>
            /// count of bytes of this block
            /// </summary>
            /// <remarks>
            /// Reference: length
            /// </remarks>
            public uint Length { get { return _length; } }

            /// <summary>
            /// flags
            /// </summary>
            /// <remarks>
            /// Reference: scf
            /// </remarks>
            public CvSepcodeFlags Scf { get { return _scf; } }

            /// <summary>
            /// sect:off of the separated code
            /// </summary>
            /// <remarks>
            /// Reference: off
            /// </remarks>
            public uint Offset { get { return _offset; } }

            /// <summary>
            /// sectParent:offParent of the enclosing scope
            /// </summary>
            /// <remarks>
            /// Reference: offParent
            /// </remarks>
            public uint ParentOffset { get { return _parentOffset; } }

            /// <summary>
            /// (proc, block, or sepcode)
            /// </summary>
            /// <remarks>
            /// Reference: sect
            /// </remarks>
            public ushort Section { get { return _section; } }

            /// <remarks>
            /// Reference: sectParent
            /// </remarks>
            public ushort ParentSection { get { return _parentSection; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: FRAMEPROCSYM
        /// </remarks>
        public partial class SymFrameProc : KaitaiStruct
        {
            public static SymFrameProc FromFile(string fileName)
            {
                return new SymFrameProc(new KaitaiStream(fileName));
            }

            public SymFrameProc(KaitaiStream p__io, MsPdb.DbiSymbolData p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _frameSize = m_io.ReadU4le();
                _padSize = m_io.ReadU4le();
                _padOffset = m_io.ReadU4le();
                _saveRegsSize = m_io.ReadU4le();
                _exceptionHandlerOffset = m_io.ReadU4le();
                _exceptionHandlerSection = m_io.ReadU2le();
                _flags = new SymFrameProcFlags(m_io, this, m_root);
            }
            private uint _frameSize;
            private uint _padSize;
            private uint _padOffset;
            private uint _saveRegsSize;
            private uint _exceptionHandlerOffset;
            private ushort _exceptionHandlerSection;
            private SymFrameProcFlags _flags;
            private MsPdb m_root;
            private MsPdb.DbiSymbolData m_parent;

            /// <summary>
            /// count of bytes of total frame of procedure
            /// </summary>
            /// <remarks>
            /// Reference: cbFrame
            /// </remarks>
            public uint FrameSize { get { return _frameSize; } }

            /// <summary>
            /// count of bytes of padding in the frame
            /// </summary>
            /// <remarks>
            /// Reference: cbPad
            /// </remarks>
            public uint PadSize { get { return _padSize; } }

            /// <summary>
            /// offset (relative to frame poniter) to where padding starts
            /// </summary>
            /// <remarks>
            /// Reference: offPad
            /// </remarks>
            public uint PadOffset { get { return _padOffset; } }

            /// <summary>
            /// count of bytes of callee save registers
            /// </summary>
            /// <remarks>
            /// Reference: cbSaveRegs
            /// </remarks>
            public uint SaveRegsSize { get { return _saveRegsSize; } }

            /// <summary>
            /// offset of exception handler
            /// </summary>
            /// <remarks>
            /// Reference: offExHdlr
            /// </remarks>
            public uint ExceptionHandlerOffset { get { return _exceptionHandlerOffset; } }

            /// <summary>
            /// section id of exception handler
            /// </summary>
            /// <remarks>
            /// Reference: sectExHdlr
            /// </remarks>
            public ushort ExceptionHandlerSection { get { return _exceptionHandlerSection; } }

            /// <remarks>
            /// Reference: flags
            /// </remarks>
            public SymFrameProcFlags Flags { get { return _flags; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.DbiSymbolData M_Parent { get { return m_parent; } }
        }
        public partial class PdbStreamRef : KaitaiStruct
        {
            public static PdbStreamRef FromFile(string fileName)
            {
                return new PdbStreamRef(new KaitaiStream(fileName));
            }

            public PdbStreamRef(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_stream = false;
                f_size = false;
                f_data = false;
                _read();
            }
            private void _read()
            {
                _streamNumber = m_io.ReadS2le();
            }
            private bool f_stream;
            private PdbStreamRefX _stream;
            public PdbStreamRefX Stream
            {
                get
                {
                    if (f_stream)
                        return _stream;
                    _stream = new PdbStreamRefX(StreamNumber, m_io, this, m_root);
                    f_stream = true;
                    return _stream;
                }
            }
            private bool f_size;
            private int _size;
            public int Size
            {
                get
                {
                    if (f_size)
                        return _size;
                    _size = (int) (Stream.Size);
                    f_size = true;
                    return _size;
                }
            }
            private bool f_data;
            private byte[] _data;
            public byte[] Data
            {
                get
                {
                    if (f_data)
                        return _data;
                    if (Stream.IsValidStream) {
                        _data = (byte[]) (Stream.ZzzData.Value);
                    }
                    f_data = true;
                    return _data;
                }
            }
            private short _streamNumber;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public short StreamNumber { get { return _streamNumber; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <remarks>
        /// Reference: COMPILESYM3.flags
        /// </remarks>
        public partial class SymCompile3Flags : KaitaiStruct
        {
            public static SymCompile3Flags FromFile(string fileName)
            {
                return new SymCompile3Flags(new KaitaiStream(fileName));
            }

            public SymCompile3Flags(KaitaiStream p__io, MsPdb.SymCompile3 p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _language = m_io.ReadU1();
                _ec = m_io.ReadBitsIntLe(1) != 0;
                _noDbgInfo = m_io.ReadBitsIntLe(1) != 0;
                _ltcg = m_io.ReadBitsIntLe(1) != 0;
                _noDataAlign = m_io.ReadBitsIntLe(1) != 0;
                _managedPresent = m_io.ReadBitsIntLe(1) != 0;
                _securityChecks = m_io.ReadBitsIntLe(1) != 0;
                _hotPatch = m_io.ReadBitsIntLe(1) != 0;
                _cvtCil = m_io.ReadBitsIntLe(1) != 0;
                _msilModule = m_io.ReadBitsIntLe(1) != 0;
                _sdl = m_io.ReadBitsIntLe(1) != 0;
                _pgo = m_io.ReadBitsIntLe(1) != 0;
                _exp = m_io.ReadBitsIntLe(1) != 0;
                _pad = m_io.ReadBitsIntLe(12);
            }
            private byte _language;
            private bool _ec;
            private bool _noDbgInfo;
            private bool _ltcg;
            private bool _noDataAlign;
            private bool _managedPresent;
            private bool _securityChecks;
            private bool _hotPatch;
            private bool _cvtCil;
            private bool _msilModule;
            private bool _sdl;
            private bool _pgo;
            private bool _exp;
            private ulong _pad;
            private MsPdb m_root;
            private MsPdb.SymCompile3 m_parent;

            /// <summary>
            /// language index
            /// </summary>
            /// <remarks>
            /// Reference: iLanguage
            /// </remarks>
            public byte Language { get { return _language; } }

            /// <summary>
            /// compiled for E/C
            /// </summary>
            /// <remarks>
            /// Reference: fEC
            /// </remarks>
            public bool Ec { get { return _ec; } }

            /// <summary>
            /// not compiled with debug info
            /// </summary>
            /// <remarks>
            /// Reference: fNoDbgInfo
            /// </remarks>
            public bool NoDbgInfo { get { return _noDbgInfo; } }

            /// <summary>
            /// compiled with LTCG
            /// </summary>
            /// <remarks>
            /// Reference: fLTCG
            /// </remarks>
            public bool Ltcg { get { return _ltcg; } }

            /// <summary>
            /// compiled with -Bzalign
            /// </summary>
            /// <remarks>
            /// Reference: fNoDataAlign
            /// </remarks>
            public bool NoDataAlign { get { return _noDataAlign; } }

            /// <summary>
            /// managed code/data present
            /// </summary>
            /// <remarks>
            /// Reference: fManagedPresent
            /// </remarks>
            public bool ManagedPresent { get { return _managedPresent; } }

            /// <summary>
            /// compiled with /GS
            /// </summary>
            /// <remarks>
            /// Reference: fSecurityChecks
            /// </remarks>
            public bool SecurityChecks { get { return _securityChecks; } }

            /// <summary>
            /// compiled with /hotpatch
            /// </summary>
            /// <remarks>
            /// Reference: fHotPatch
            /// </remarks>
            public bool HotPatch { get { return _hotPatch; } }

            /// <summary>
            /// converted with CVTCIL
            /// </summary>
            /// <remarks>
            /// Reference: fCVTCIL
            /// </remarks>
            public bool CvtCil { get { return _cvtCil; } }

            /// <summary>
            /// MSIL netmodule
            /// </summary>
            /// <remarks>
            /// Reference: fMSILModule
            /// </remarks>
            public bool MsilModule { get { return _msilModule; } }

            /// <summary>
            /// compiled with /sdl
            /// </summary>
            /// <remarks>
            /// Reference: fSdl
            /// </remarks>
            public bool Sdl { get { return _sdl; } }

            /// <summary>
            /// compiled with /ltcg:pgo or pgu
            /// </summary>
            /// <remarks>
            /// Reference: fPGO
            /// </remarks>
            public bool Pgo { get { return _pgo; } }

            /// <summary>
            /// .exp module
            /// </summary>
            /// <remarks>
            /// Reference: fExp
            /// </remarks>
            public bool Exp { get { return _exp; } }

            /// <summary>
            /// reserved, must be 0
            /// </summary>
            /// <remarks>
            /// Reference: pad
            /// </remarks>
            public ulong Pad { get { return _pad; } }
            public MsPdb M_Root { get { return m_root; } }
            public MsPdb.SymCompile3 M_Parent { get { return m_parent; } }
        }
        public partial class PublicSymbolsStream : KaitaiStruct
        {
            public static PublicSymbolsStream FromFile(string fileName)
            {
                return new PublicSymbolsStream(new KaitaiStream(fileName));
            }

            public PublicSymbolsStream(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _header = new PsgiHeader(m_io, this, m_root);
                _symbolsHashMap = m_io.ReadBytes(Header.SymHashSize);
                _addressMap = m_io.ReadBytes(Header.AddressMapSize);
            }
            private PsgiHeader _header;
            private byte[] _symbolsHashMap;
            private byte[] _addressMap;
            private MsPdb m_root;
            private KaitaiStruct m_parent;
            public PsgiHeader Header { get { return _header; } }
            public byte[] SymbolsHashMap { get { return _symbolsHashMap; } }
            public byte[] AddressMap { get { return _addressMap; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }

        /// <summary>
        /// defines a range of addresses
        /// </summary>
        /// <remarks>
        /// Reference: CV_LVAR_ADDR_RANGE
        /// </remarks>
        public partial class CvLvarAddrRange : KaitaiStruct
        {
            public static CvLvarAddrRange FromFile(string fileName)
            {
                return new CvLvarAddrRange(new KaitaiStream(fileName));
            }

            public CvLvarAddrRange(KaitaiStream p__io, KaitaiStruct p__parent = null, MsPdb p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _offsetStart = m_io.ReadU4le();
                _sectionStartIndex = m_io.ReadU2le();
                _rangeLength = m_io.ReadU2le();
            }
            private uint _offsetStart;
            private ushort _sectionStartIndex;
            private ushort _rangeLength;
            private MsPdb m_root;
            private KaitaiStruct m_parent;

            /// <remarks>
            /// Reference: offStart
            /// </remarks>
            public uint OffsetStart { get { return _offsetStart; } }

            /// <remarks>
            /// Reference: isectStart
            /// </remarks>
            public ushort SectionStartIndex { get { return _sectionStartIndex; } }

            /// <remarks>
            /// Reference: cbRange
            /// </remarks>
            public ushort RangeLength { get { return _rangeLength; } }
            public MsPdb M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        private bool f_streamTable;
        private PdbStreamTable _streamTable;
        public PdbStreamTable StreamTable
        {
            get
            {
                if (f_streamTable)
                    return _streamTable;
                if ( ((PdbType == PdbTypeEnum.Big) || (PdbType == PdbTypeEnum.Small)) ) {
                    _streamTable = (PdbStreamTable) ((PdbType == PdbTypeEnum.Big ? M_Root.PdbDs.StreamTable : M_Root.PdbJg.StreamTable));
                }
                f_streamTable = true;
                return _streamTable;
            }
        }
        private bool f_pdbRootStream;
        private PdbStream _pdbRootStream;
        public PdbStream PdbRootStream
        {
            get
            {
                if (f_pdbRootStream)
                    return _pdbRootStream;
                __raw__raw_pdbRootStream = m_io.ReadBytes(0);
                Cat _process__raw__raw_pdbRootStream = new Cat(ZzzPdbData.Value);
                __raw_pdbRootStream = _process__raw__raw_pdbRootStream.Decode(__raw__raw_pdbRootStream);
                var io___raw_pdbRootStream = new KaitaiStream(__raw_pdbRootStream);
                _pdbRootStream = new PdbStream(io___raw_pdbRootStream, this, m_root);
                f_pdbRootStream = true;
                return _pdbRootStream;
            }
        }
        private bool f_zzzDbiData;
        private GetStreamData _zzzDbiData;
        public GetStreamData ZzzDbiData
        {
            get
            {
                if (f_zzzDbiData)
                    return _zzzDbiData;
                _zzzDbiData = new GetStreamData(((uint) (DefaultStream.Dbi)), m_io, this, m_root);
                f_zzzDbiData = true;
                return _zzzDbiData;
            }
        }
        private bool f_zzzTpiData;
        private GetStreamData _zzzTpiData;
        public GetStreamData ZzzTpiData
        {
            get
            {
                if (f_zzzTpiData)
                    return _zzzTpiData;
                _zzzTpiData = new GetStreamData(((uint) (DefaultStream.Tpi)), m_io, this, m_root);
                f_zzzTpiData = true;
                return _zzzTpiData;
            }
        }
        private bool f_dbiStream;
        private Dbi _dbiStream;
        public Dbi DbiStream
        {
            get
            {
                if (f_dbiStream)
                    return _dbiStream;
                if (ZzzDbiData.HasData) {
                    __raw__raw_dbiStream = m_io.ReadBytes(0);
                    Cat _process__raw__raw_dbiStream = new Cat(ZzzDbiData.Value);
                    __raw_dbiStream = _process__raw__raw_dbiStream.Decode(__raw__raw_dbiStream);
                    var io___raw_dbiStream = new KaitaiStream(__raw_dbiStream);
                    _dbiStream = new Dbi(io___raw_dbiStream, this, m_root);
                    f_dbiStream = true;
                }
                return _dbiStream;
            }
        }
        private bool f_pdbType;
        private PdbTypeEnum _pdbType;
        public PdbTypeEnum PdbType
        {
            get
            {
                if (f_pdbType)
                    return _pdbType;
                _pdbType = (PdbTypeEnum) ((M_Root.Signature.Id == "DS" ? PdbTypeEnum.Big : ( ((M_Root.Signature.Id == "JG") && (M_Root.Signature.VersionMajor == "2"))  ? PdbTypeEnum.Small : PdbTypeEnum.Old)));
                f_pdbType = true;
                return _pdbType;
            }
        }
        private bool f_pageSize;
        private uint _pageSize;
        public uint PageSize
        {
            get
            {
                if (f_pageSize)
                    return _pageSize;
                _pageSize = (uint) ((PdbType == PdbTypeEnum.Big ? PageSizeDs : (PdbType == PdbTypeEnum.Small ? PageSizeJg : 0)));
                f_pageSize = true;
                return _pageSize;
            }
        }
        private bool f_numStreams;
        private uint _numStreams;
        public uint NumStreams
        {
            get
            {
                if (f_numStreams)
                    return _numStreams;
                _numStreams = (uint) ((PdbType == PdbTypeEnum.Big ? PdbDs.StreamTable.NumStreams : (PdbType == PdbTypeEnum.Small ? PdbJg.StreamTable.NumStreams : 0)));
                f_numStreams = true;
                return _numStreams;
            }
        }
        private bool f_pageNumberSize;
        private sbyte _pageNumberSize;
        public sbyte PageNumberSize
        {
            get
            {
                if (f_pageNumberSize)
                    return _pageNumberSize;
                _pageNumberSize = (sbyte) ((PdbType == PdbTypeEnum.Big ? 4 : 2));
                f_pageNumberSize = true;
                return _pageNumberSize;
            }
        }
        private bool f_minTypeIndex;
        private uint _minTypeIndex;
        public uint MinTypeIndex
        {
            get
            {
                if (f_minTypeIndex)
                    return _minTypeIndex;
                _minTypeIndex = (uint) ((PdbType == PdbTypeEnum.Old ? PdbJgOld.Header.MinTi : TpiStream.MinTypeIndex));
                f_minTypeIndex = true;
                return _minTypeIndex;
            }
        }
        private bool f_types;
        private List<TpiType> _types;
        public List<TpiType> Types
        {
            get
            {
                if (f_types)
                    return _types;
                _types = (List<TpiType>) ((PdbType == PdbTypeEnum.Old ? PdbJgOld.Types : TpiStream.Types.Types));
                f_types = true;
                return _types;
            }
        }
        private bool f_tpiStream;
        private Tpi _tpiStream;
        public Tpi TpiStream
        {
            get
            {
                if (f_tpiStream)
                    return _tpiStream;
                __raw__raw_tpiStream = m_io.ReadBytes(0);
                Cat _process__raw__raw_tpiStream = new Cat(ZzzTpiData.Value);
                __raw_tpiStream = _process__raw__raw_tpiStream.Decode(__raw__raw_tpiStream);
                var io___raw_tpiStream = new KaitaiStream(__raw_tpiStream);
                _tpiStream = new Tpi(io___raw_tpiStream, this, m_root);
                f_tpiStream = true;
                return _tpiStream;
            }
        }
        private bool f_pageSizeDs;
        private uint? _pageSizeDs;
        public uint? PageSizeDs
        {
            get
            {
                if (f_pageSizeDs)
                    return _pageSizeDs;
                if (PdbType == PdbTypeEnum.Big) {
                    _pageSizeDs = (uint) (PdbDs.Header.PageSize);
                }
                f_pageSizeDs = true;
                return _pageSizeDs;
            }
        }
        private bool f_zzzPdbData;
        private GetStreamData _zzzPdbData;
        public GetStreamData ZzzPdbData
        {
            get
            {
                if (f_zzzPdbData)
                    return _zzzPdbData;
                _zzzPdbData = new GetStreamData(((uint) (DefaultStream.Pdb)), m_io, this, m_root);
                f_zzzPdbData = true;
                return _zzzPdbData;
            }
        }
        private bool f_maxTypeIndex;
        private uint _maxTypeIndex;
        public uint MaxTypeIndex
        {
            get
            {
                if (f_maxTypeIndex)
                    return _maxTypeIndex;
                _maxTypeIndex = (uint) ((PdbType == PdbTypeEnum.Old ? PdbJgOld.Header.MaxTi : TpiStream.MaxTypeIndex));
                f_maxTypeIndex = true;
                return _maxTypeIndex;
            }
        }
        private bool f_pageSizeJg;
        private uint? _pageSizeJg;
        public uint? PageSizeJg
        {
            get
            {
                if (f_pageSizeJg)
                    return _pageSizeJg;
                if (PdbType == PdbTypeEnum.Small) {
                    _pageSizeJg = (uint) (PdbJg.Header.PageSize);
                }
                f_pageSizeJg = true;
                return _pageSizeJg;
            }
        }
        private PdbSignature _signature;
        private PdbDsRoot _pdbDs;
        private PdbJgRoot _pdbJg;
        private PdbJgOldRoot _pdbJgOld;
        private MsPdb m_root;
        private KaitaiStruct m_parent;
        private byte[] __raw_pdbRootStream;
        private byte[] __raw__raw_pdbRootStream;
        private byte[] __raw_dbiStream;
        private byte[] __raw__raw_dbiStream;
        private byte[] __raw_tpiStream;
        private byte[] __raw__raw_tpiStream;
        public PdbSignature Signature { get { return _signature; } }
        public PdbDsRoot PdbDs { get { return _pdbDs; } }
        public PdbJgRoot PdbJg { get { return _pdbJg; } }
        public PdbJgOldRoot PdbJgOld { get { return _pdbJgOld; } }
        public MsPdb M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
        public byte[] M_RawPdbRootStream { get { return __raw_pdbRootStream; } }
        public byte[] M_RawM_RawPdbRootStream { get { return __raw__raw_pdbRootStream; } }
        public byte[] M_RawDbiStream { get { return __raw_dbiStream; } }
        public byte[] M_RawM_RawDbiStream { get { return __raw__raw_dbiStream; } }
        public byte[] M_RawTpiStream { get { return __raw_tpiStream; } }
        public byte[] M_RawM_RawTpiStream { get { return __raw__raw_tpiStream; } }
    }
}
