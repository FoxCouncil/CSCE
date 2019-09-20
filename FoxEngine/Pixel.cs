// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace FoxEngine
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Pixel
    {
        private const int AoffSet = 24;
        private const int BoffSet = 16;
        private const int GoffSet = 8;
        private const int RoffSet = 0;

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Pixel(byte red, byte green, byte blue, byte alpha = 255)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        public static Pixel White = new Pixel(255, 255, 255);
        public static Pixel Black = new Pixel(0, 0, 0);

        public static Pixel Red = new Pixel(255, 0, 0);
        public static Pixel Green = new Pixel(0, 255, 0);

        public static Pixel DarkBlue = new Pixel(2, 30, 45);
        public static Pixel Blue = new Pixel(0, 0, 255);


        public static Pixel Blank = new Pixel(0, 0, 0, 0);
    }
}