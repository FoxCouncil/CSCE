// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    using FoxEngine;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Emulator : Engine
    {
        private Fox6502 _cpu;

        private List<Tuple<ushort, string>> Code;

        public Emulator() : base("CPU Emulator", 680, 510)
        {
            _cpu = new Fox6502();
        }

        public override void Create()
        {
            ushort offset = 0x8000;

            // "A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA EA EA"

            _cpu.BusWrite(offset++, 0xA2);
            _cpu.BusWrite(offset++, 0x0A);
            _cpu.BusWrite(offset++, 0x8E);
            _cpu.BusWrite(offset++, 0);
            _cpu.BusWrite(offset++, 0);
            _cpu.BusWrite(offset++, 0xA2);
            _cpu.BusWrite(offset++, 0x03);
            _cpu.BusWrite(offset++, 0x8E);
            _cpu.BusWrite(offset++, 0x01);
            _cpu.BusWrite(offset++, 0);
            _cpu.BusWrite(offset++, 0xAC);
            _cpu.BusWrite(offset++, 0);
            _cpu.BusWrite(offset++, 0);
            _cpu.BusWrite(offset++, 0xA9);
            _cpu.BusWrite(offset++, 0);
            _cpu.BusWrite(offset++, 0x18);
            _cpu.BusWrite(offset++, 0x6D);
            _cpu.BusWrite(offset++, 0x01);
            _cpu.BusWrite(offset++, 0);
            _cpu.BusWrite(offset++, 0x88);
            _cpu.BusWrite(offset++, 0xD0);
            _cpu.BusWrite(offset++, 0xFA);
            _cpu.BusWrite(offset++, 0x8D);
            _cpu.BusWrite(offset++, 0x02);
            _cpu.BusWrite(offset++, 0);
            _cpu.BusWrite(offset++, 0xEA);
            _cpu.BusWrite(offset++, 0xEA);
            _cpu.BusWrite(offset++, 0xEA);

            _cpu.BusWrite(0xFFFC, 0);
            _cpu.BusWrite(0xFFFD, 0x80);

            Code = _cpu.Disassemble(0, 0xFFFF);

            _cpu.Reset();

            Platform.OnKeyUp += Platform_OnKeyUp;
        }

        public override void Update(double frameTime)
        {
            Clear(Pixel.DarkBlue);

            DrawRam(2, 2, 0x0000, 16, 16);
		    DrawRam(2, 182, 0x8000, 16, 16);

            DrawCPU(446, 2);

            DrawCode(448, 72, 26);

            DrawString(10, 370, "SPACE = CLOCK   R = RESET    I = IRQ    N = NMI", Pixel.White);
        }

        public override void Destroy()
        {
            // NOOP
        }

        private void Platform_OnKeyUp(int keyCode)
        {
            switch (keyCode)
            {
                case 32:
                {
                    do
                    {
                        _cpu.Clock();
                    }
                    while (_cpu.CyclesLeft != 0);
                }
                break;

                case 82:
                {
                    _cpu.Reset();
                }
                break;

                case 73:
                {
                    _cpu.IRQ();
                }
                break;

                case 78:
                {
                    _cpu.NMI();
                }
                break;

                default:
                {
                    Console.WriteLine("KeyUP: {0} ({1:X2})", keyCode, keyCode);
                }
                break;
            }
        }

        void DrawCode(int x, int y, int nLines)
        {
            var idx = Code.IndexOf(Code.First(kIdx => kIdx.Item1 == _cpu.PC));

            var it_a = Code[idx];

            int nLineY = (nLines >> 1) * 10 + y;

            if (idx < Code.Count)
            {
                DrawString(x, nLineY, it_a.Item2, Pixel.LightBlue);

                while (nLineY < (nLines * 10) + y)
                {
                    nLineY += 10;

                    if (++idx < Code.Count)
                    {
                        it_a = Code[idx];

                        DrawString(x, nLineY, it_a.Item2, Pixel.White);
                    }
                }
            }

            idx = Code.IndexOf(Code.First(kIdx => kIdx.Item1 == _cpu.PC));

            it_a = Code[idx];

            nLineY = (nLines >> 1) * 10 + y;

            if (idx < Code.Count)
            {
                while (nLineY > y)
                {
                    nLineY -= 10;

                    if (--idx > 0)
                    {
                        it_a = Code[idx];

                        DrawString(x, nLineY, it_a.Item2, Pixel.White);
                    }
                }
            }
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

        private void DrawRam(int x, int y, ushort nAddr, int nRows, int nColumns)
        {
            int nRamX = x, nRamY = y;
            for (int row = 0; row < nRows; row++)
            {
                var sOffset = "$" + HexOutput(nAddr, 4) + ":";

                for (int col = 0; col < nColumns; col++)
                {
                    sOffset += " " + HexOutput(_cpu.BusRead(nAddr), 2);
                    nAddr += 1;
                }

                DrawString(nRamX, nRamY, sOffset, Pixel.White);

                nRamY += 10;
            }
        }

        private string HexOutput(int number, byte length)
        {
            return number.ToString($"X{length.ToString()}");
        }
    }
}
