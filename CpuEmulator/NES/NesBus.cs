// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

using System;
using System.Collections.Generic;
using System.Text;

namespace CpuEmulator.NES
{
    public class NesBus
    {
        private uint _totalCycles;

        private byte[] _controller = new byte[2];

        public Fox6502 Cpu { get; } = new Fox6502();

        public Fox2C02 Ppu { get; } = new Fox2C02();

        public Cartridge Cart { get; private set; }

        // 2KB of RAM
        public byte[] Ram { get; set; } = new byte[2048];

        public byte[] Controller { get; set; } = new byte[2];

        public NesBus()
        {
            Cpu.ConnectBus(this);
        }

        public void Write(ushort address, byte data)
        {
            if (Cart != null && Cart.CpuWrite(address, data))
            {

            }
            else if (address >= 0 && address <= 0x1FFF)
            {
                Ram[address & 0x07FF] = data;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                Ppu.CpuWrite((ushort)(address & 0x0007), data);
            }
            else if (address >= 0x4016 && address <= 0x4017)
            {
                _controller[address & 0x0001] = Controller[address & 0x0001];
            }
        }

        public byte Read(ushort address)
        {
            byte data = 0;

            if (Cart != null && Cart.CpuRead(address, ref data))
            {

            }
            else if (address >= 0 && address <= 0x1FFF)
            {
                data = Ram[address & 0x07FF];
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                data = Ppu.CpuRead((ushort)(address & 0x0007));
            }
            else if (address >= 0x4016 && address <= 0x4017)
            {
                data = (byte)((_controller[address & 0x0001] & 0x80) > 0 ? 0x01 : 0x00);

                _controller[address & 0x0001] <<= 1;
            }

            return data;
        }

        public void InsertCartridge(Cartridge cartridge)
        {
            Cart = cartridge;

            Ppu.ConnectCartridge(cartridge);
        }

        public void Reset()
        {
            Cart.Reset();
            Cpu.Reset();
            Ppu.Reset();

            _totalCycles = 0;
        }

        public void Clock()
        {
            Ppu.Clock();

            if (_totalCycles % 3 == 0)
            {
                Cpu.Clock();
            }

            if (Ppu.Nmi)
            {
                Ppu.Nmi = false;
                Cpu.Nmi();
            }

            _totalCycles++;
        }
    }
}
