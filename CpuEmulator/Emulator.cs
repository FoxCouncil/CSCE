// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    using FoxEngine;
    using System;

    class Emulator : Engine
    {
        Random random = new Random();

        public Emulator() : base("CPU Emulator", 640, 480)
        {
            DrawLine(10, 10, 100, 100, Pixel.Blue);

            
        }

        public override void Create()
        {
        }

        public override void Update()
        {
            DrawTarget.Clear(Pixel.White);
            DrawString(random.Next(DrawTarget.Width), random.Next(DrawTarget.Height), "Hello World!", Pixel.Black, 2);
        }

        public override void Destroy()
        {
        }
    }
}
