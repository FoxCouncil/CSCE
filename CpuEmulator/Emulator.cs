// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    using FoxEngine;
    using System;

    class Emulator : Engine
    {
        bool isIncreasing;
        int radii = 100;
        Random random = new Random();

        public Emulator() : base("CPU Emulator", 640, 480)
        {
            // DrawLine(10, 10, 100, 100, Pixel.Blue);
        }

        public override void Create()
        {
        }

        public override void Update(double frameTime)
        {
            frameTime /= 10;

            DrawTarget.Clear(Pixel.White);

            if (isIncreasing)
            {
                radii += (int)frameTime;

                if (radii > 100)
                {
                    radii = 100;
                    isIncreasing = false;
                }
            }
            else
            {
                radii -= (int)frameTime;

                if (radii < 10)
                {
                    radii = 10;
                    isIncreasing = true;
                }
            }

            DrawCircleFilled(110, 110, radii, Pixel.Black);
        }

        public override void Destroy()
        {
        }
    }
}
