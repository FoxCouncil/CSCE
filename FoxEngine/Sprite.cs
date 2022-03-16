// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace FoxEngine
{
    using System;
    using System.Runtime.InteropServices;

    public class Sprite
    {
        // private const int kBpp = 4;

        public Pixel[] PixelData;

        public int Width { get; }

        public int Height { get; }

        public int TotalPixels { get; }

        public GCHandle Handle { get; }

        public Sprite()
        {
            Width = 0;
            Height = 0;
        }

        public Sprite(int width, int height)
        {
            Width = width;
            Height = height;

            TotalPixels = Width * Height;

            PixelData = new Pixel[TotalPixels];

            for (var idx = 0; idx < TotalPixels; idx++)
            {
                PixelData[idx] = Pixel.White;
            }

            Handle = GCHandle.Alloc(PixelData, GCHandleType.Pinned);
        }

        ~Sprite()
        {
            if (((IntPtr)Handle) != IntPtr.Zero)
            {
                Handle.Free();
            }
        }

        public Pixel GetPixel(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return PixelData[y*Width + x];
            }

            return Pixel.Blank;
        }

        public void SetPixel(int x, int y, Pixel pixel)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                PixelData[y*Width + x] = pixel;
            }
        }

        public void Clear(Pixel color)
        {
            for (var idx = 0; idx < TotalPixels; idx++)
            {
                PixelData[idx] = color;
            }
        }
    }
}
