// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES.Ppu
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ObjectAttributeEntry
    {
        public byte Y;

        public byte Id;

        public byte Attribute;

        public byte X;
    }
}
