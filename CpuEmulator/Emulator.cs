// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    using CpuEmulator.NES;
    using FoxEngine;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Emulator : Engine
    {
        private bool _isRunning;

        private float _timeLeft;

        private byte _selectedPalette;

        public static NesBus _nes;

        private Cartridge _cart;

        public static List<Tuple<ushort, string>> Code;

        public Emulator() : base("CPU Emulator", 780, 485)
        {
            _nes = new NesBus();
        }

        public override void Create()
        {
            _cart = new Cartridge("dk.nes");

            _nes.InsertCartridge(_cart);

            Code = _nes.Cpu.Disassemble(0, 0xFFFF);

            _nes.Reset();
        }

        public override void Update(double frameTime)
        {
            Clear(Pixel.DarkBlue);

            _nes.Controller[0] = 0;
            _nes.Controller[0] |= (byte)(Keyboard[KeyboardButton.X].Held ? 0x80 : 0);
            _nes.Controller[0] |= (byte)(Keyboard[KeyboardButton.Z].Held ? 0x40 : 0);
            _nes.Controller[0] |= (byte)(Keyboard[KeyboardButton.A].Held ? 0x20 : 0);
            _nes.Controller[0] |= (byte)(Keyboard[KeyboardButton.S].Held ? 0x10 : 0);
            _nes.Controller[0] |= (byte)(Keyboard[KeyboardButton.Up].Held ? 0x08 : 0);
            _nes.Controller[0] |= (byte)(Keyboard[KeyboardButton.Down].Held ? 0x04 : 0);
            _nes.Controller[0] |= (byte)(Keyboard[KeyboardButton.Left].Held ? 0x02 : 0);
            _nes.Controller[0] |= (byte)(Keyboard[KeyboardButton.Right].Held ? 0x01 : 0);

            if (_isRunning)
            {
                if (_timeLeft > 0.0f)
                {
                    _timeLeft -= (float)frameTime;
                }
                else
                {
                    _timeLeft += (1f / 60f) - (float)frameTime;

                    do
                    {
                        _nes.Clock();
                    }
                    while (!_nes.Ppu.FrameComplete);

                    _nes.Ppu.FrameComplete = false;
                }
            }
            else
            {
                if (Keyboard[KeyboardButton.C].Pressed)
                {
                    do
                    {
                        _nes.Clock();
                    }
                    while (!_nes.Cpu.Complete);

                    do
                    {
                        _nes.Clock();
                    }
                    while (_nes.Cpu.Complete);
                }

                if (Keyboard[KeyboardButton.F].Pressed)
                {
                    do
                    {
                        _nes.Clock();
                    }
                    while (!_nes.Ppu.FrameComplete);

                    do
                    {
                        _nes.Clock();
                    }
                    while (!_nes.Cpu.Complete);

                    _nes.Ppu.FrameComplete = false;
                }
            }

            if (Keyboard[KeyboardButton.Space].Pressed)
            {
                _isRunning = !_isRunning;
            }

            if (Keyboard[KeyboardButton.R].Pressed)
            {
                _nes.Reset();
            }

            if (Keyboard[KeyboardButton.P].Pressed)
            {
                _selectedPalette++;
                _selectedPalette &= 0x07;
            }

            // DrawRam(2, 2, 0x0000, 16, 16);
            // DrawRam(2, 182, 0x8000, 16, 16);

            const int swatchSize = 6;
            const int y = 343;
            const int x = 529;

            for (int p = 0; p < 8; p++) // For each palette
            {
                for (int s = 0; s < 4; s++) // For each index
                {
                    DrawRectFilled(x + p * (swatchSize * 5) + s * swatchSize, y, swatchSize, swatchSize, _nes.Ppu.GetColorFromPaletteRam((byte)p, (byte)s));
                }
            }

            DrawRect(x + _selectedPalette * (swatchSize * 5) - 1, y, (swatchSize * 4), swatchSize, Pixel.White);

            DrawSprite(516, 352, _nes.Ppu.GetPatternTable(0, _selectedPalette), 1);
            DrawSprite(648, 352, _nes.Ppu.GetPatternTable(1, _selectedPalette), 1);

            DrawSprite(0, 0, _nes.Ppu.Screen, 2);

            //for (var yIdx = 0; yIdx < 30; yIdx++)
            //{
            //    for (var xIdx = 0; xIdx < 32; xIdx++)
            //    {
            //        var id = _nes.Ppu.NameTable[0, yIdx * 32 + xIdx];
            //        // DrawString(xIdx * 16, yIdx * 16, HexOutput(id, 2), Pixel.White);
            //        DrawSpritePartial(xIdx * 16, yIdx * 16, _nes.Ppu.GetPatternTable(1, _selectedPalette), (id & 0x0F) << 3, ((id >> 4) & 0x0F) << 3, 8, 8, 2);
            //    }
            //}

            DrawCPU(522, 2);
            // DrawCode(538, 68, 26);

            for (var idx = 0; idx < 26; idx++)
            {
                var output = $"{HexOutput(idx, 2)}: ({(_nes.Ppu.OAMData[idx * 4 + 3]).ToString().PadLeft(3, '0')}, {(_nes.Ppu.OAMData[idx * 4]).ToString().PadLeft(3, '0')}) ID: {HexOutput(_nes.Ppu.OAMData[idx * 4 + 1], 2)} AT: {_nes.Ppu.OAMData[idx * 4 + 2]}";
                DrawString(538, 72 + idx * 10, output, Pixel.White);
            }

            // DrawString(10, 370, "SPACE = CLOCK   R = RESET    I = IRQ    N = NMI", Pixel.White);
        }

        public override void Destroy()
        {
            // NOOP
        }

        void DrawCode(int x, int y, int nLines)
        {
            var idx = Code.IndexOf(Code.First(kIdx => kIdx.Item1 == _nes.Cpu.PC));

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

            idx = Code.IndexOf(Code.First(kIdx => kIdx.Item1 == _nes.Cpu.PC));

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
            DrawString(x + offset, y, "N", _nes.Cpu.GetFlag(Fox6502.Flags.N) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 16, y, "V", _nes.Cpu.GetFlag(Fox6502.Flags.V) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 32, y, "-", _nes.Cpu.GetFlag(Fox6502.Flags.U) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 48, y, "B", _nes.Cpu.GetFlag(Fox6502.Flags.B) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 64, y, "D", _nes.Cpu.GetFlag(Fox6502.Flags.D) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 80, y, "I", _nes.Cpu.GetFlag(Fox6502.Flags.I) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 96, y, "Z", _nes.Cpu.GetFlag(Fox6502.Flags.Z) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 114, y, "C", _nes.Cpu.GetFlag(Fox6502.Flags.C) ? Pixel.Green : Pixel.Red);
            DrawString(x + offset + 128, y, $"({HexOutput(_nes.Cpu.P, 2)})", Pixel.White);
            DrawString(x, y + 10, "     PC: $" + HexOutput(_nes.Cpu.PC, 4), Pixel.White);
            DrawString(x, y + 20, "      A: $" + HexOutput(_nes.Cpu.A, 2) + "  [" + _nes.Cpu.A + "]", Pixel.White);
            DrawString(x, y + 30, "      X: $" + HexOutput(_nes.Cpu.X, 2) + "  [" + _nes.Cpu.X + "]", Pixel.White);
            DrawString(x, y + 40, "      Y: $" + HexOutput(_nes.Cpu.Y, 2) + "  [" + _nes.Cpu.Y + "]", Pixel.White);
            DrawString(x, y + 50, "Stack P: $" + HexOutput(_nes.Cpu.SP, 4), Pixel.White);
        }

        private void DrawRam(int x, int y, ushort nAddr, int nRows, int nColumns)
        {
            int nRamX = x, nRamY = y;
            for (int row = 0; row < nRows; row++)
            {
                var sOffset = "$" + HexOutput(nAddr, 4) + ":";

                for (int col = 0; col < nColumns; col++)
                {
                    sOffset += " " + HexOutput(_nes.Cpu.BusRead(nAddr), 2);
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
