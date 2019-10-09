// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES.Ppu
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public struct MaskFlags
    {
        [FieldOffset(0)]
        public byte Register;

        [FieldOffset(0)]
        private ByteUnionBitFieldBool Flags;

        public bool Grayscale { get { return Flags[0]; } set { Flags[0] = value; } }

        public bool RenderBackgroundLeft { get { return Flags[1]; } set { Flags[1] = value; } }

        public bool RenderSpritesLeft { get { return Flags[2]; } set { Flags[2] = value; } }

        public bool RenderBackground { get { return Flags[3]; } set { Flags[3] = value; } }

        public bool RenderSprites { get { return Flags[4]; } set { Flags[4] = value; } }

        public bool EnhanceRed { get { return Flags[5]; } set { Flags[5] = value; } }

        public bool EnhanceGreen { get { return Flags[6]; } set { Flags[6] = value; } }

        public bool EnhancheBlue { get { return Flags[7]; } set { Flags[7] = value; } }
    }
}
