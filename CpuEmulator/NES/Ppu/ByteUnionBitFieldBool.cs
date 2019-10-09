// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES.Ppu
{
    public struct ByteUnionBitFieldBool
    {
        byte bits;

        public bool this[int i]
        {
            get
            {
                return (bits & (1 << i)) != 0;
            }

            set
            {
                if (value)
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
