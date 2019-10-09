// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES.Ppu
{
    public struct LoopyFlags
    {
        public ushort Register
        {
            get
            {
                return (ushort)((Unused & 1) << 15 | (FineY & 7) << 12 | (NameTableY & 1) << 11 | (NameTableX & 1) << 10 | (CoarseY & 0x1F) << 5 | (CoarseX & 0x1F));
            }

            set
            {
                CoarseX = (ushort)(value & 0x1F);
                CoarseY = (ushort)((value >> 5) & 0x1F);
                NameTableX = (ushort)((value >> 10) & 1);
                NameTableY = (ushort)((value >> 11) & 1);
                FineY = (ushort)((value >> 12) & 7);
                Unused = (ushort)((value >> 15) & 1);
            }
        }

        public ushort CoarseX;

        public ushort CoarseY;

        public ushort NameTableX;

        public ushort NameTableY;

        public ushort FineY;

        public ushort Unused;
    }
}
