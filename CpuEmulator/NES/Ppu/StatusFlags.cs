// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES.Ppu
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public struct StatusFlags
    {
        [FieldOffset(0)]
        public byte Register;

        [FieldOffset(0)]
        private ByteUnionBitFieldBool Flags;

        public bool SpriteOverflow { get { return Flags[5]; } set { Flags[5] = value; } }

        public bool SpriteZeroHit { get { return Flags[6]; } set { Flags[6] = value; } }

        public bool VerticalBlank { get { return Flags[7]; } set { Flags[7] = value; } }
    }
}
