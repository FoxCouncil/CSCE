// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES.Ppu
{
    public struct ByteUnionBitFieldByte
    {
        byte bits;

        public byte this[int i]
        {
            get
            {
                return (byte)(bits & (1 << i));
            }

            set
            {
                if (value > 0)
                {
                    bits |= (byte)(1 << i);
                }
                else
                {
                    bits &= (byte)~(1 << i);
                }
            }
        }
    }
}
