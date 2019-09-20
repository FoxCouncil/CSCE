// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    using FoxEngine;
    using System;

    class Emulator : Engine
    {
        private Fox6502 _cpu;

        public Emulator() : base("CPU Emulator", 640, 480)
        {
            _cpu = new Fox6502();
            _cpu.A = 17;
        }

        public override void Create()
        {
        }

        public override void Update(double frameTime)
        {
            DrawTarget.Clear(Pixel.DarkBlue);

            DrawCPU(446, 2);
        }

        public override void Destroy()
        {
        }

        private void DrawCPU(int x, int y)
        {
            var offset = 72;
            DrawString(x, y, " STATUS: ", Pixel.White);
            DrawString(x + offset,       y, "N", _cpu.GetFlag(Fox6502.Flags.N) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 16,  y, "V", _cpu.GetFlag(Fox6502.Flags.V) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 32,  y, "-", _cpu.GetFlag(Fox6502.Flags.U) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 48,  y, "B", _cpu.GetFlag(Fox6502.Flags.B) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 64,  y, "D", _cpu.GetFlag(Fox6502.Flags.D) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 80,  y, "I", _cpu.GetFlag(Fox6502.Flags.I) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 96,  y, "Z", _cpu.GetFlag(Fox6502.Flags.Z) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 114, y, "C", _cpu.GetFlag(Fox6502.Flags.C) ? Pixel.Green : Pixel.Red);
            DrawString(x, y + 10, "     PC: $" + HexOutput(_cpu.PC, 4), Pixel.White);
            DrawString(x, y + 20, "      A: $" + HexOutput(_cpu.A, 2) + "  [" + _cpu.A + "]", Pixel.White);
            DrawString(x, y + 30, "      X: $" + HexOutput(_cpu.X, 2) + "  [" + _cpu.X + "]", Pixel.White);
            DrawString(x, y + 40, "      Y: $" + HexOutput(_cpu.Y, 2) + "  [" + _cpu.Y + "]", Pixel.White);
            DrawString(x, y + 50, "Stack P: $" + HexOutput(_cpu.SP, 4), Pixel.White);
        }

        private string HexOutput(int number, byte length)
        {
            return number.ToString($"X{length.ToString()}");
        }
    }
}
