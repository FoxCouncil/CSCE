// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES.Ppu
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public struct ControlFlags
    {
        [FieldOffset(0)]
        public byte Register;

        [FieldOffset(0)]
        private ByteUnionBitFieldByte Flags;

        public byte NameTableX { get { return Flags[0]; } set { Flags[0] = value; } }

        public byte NameTableY { get { return (byte)(Flags[1] >> 1); } set { Flags[1] = value; } }

        public byte IncrementMode { get { return (byte)(Flags[2] >> 2); } set { Flags[2] = value; } }

        public byte PatternSprite { get { return (byte)(Flags[3] >> 3); } set { Flags[3] = value; } }

        public byte PatternBackground { get { return (byte)(Flags[4] >> 4); } set { Flags[4] = value; } }

        public byte SpriteSize { get { return (byte)(Flags[5] >> 5); } set { Flags[5] = value; } }

        public byte SlaveMode { get { return (byte)(Flags[6] >> 6); } set { Flags[6] = value; } }

        public bool EnableNmi { get { return Flags[7] > 0; } set { Flags[7] = (byte)(value ? 1 : 0); } }
    }
}
