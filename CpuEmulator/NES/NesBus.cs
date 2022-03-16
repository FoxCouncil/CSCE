// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    using CpuEmulator.NES;
    using CpuEmulator.NES.Apu;
    using CpuEmulator.NES.Ppu;

    public class NesBus
    {
        uint _totalCycles;

        byte[] _controller = new byte[2];

        byte _dmaPage;

        byte _dmaAddress;

        byte _dmaData;

        bool _dmaTransfer;

        bool _dmaDummy = true;

        double _audioTime;

        double _audioTimePerSample;

        double _audioTimePerClock;

        public Fox6502 Cpu { get; } = new();

        public Fox2C02 Ppu { get; } = new();

        public Fox2A03 Apu { get; } = new();

        public Cartridge Cart { get; private set; }

        public double AudioSample { get; private set; }

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
            else if ((address >= 0x4000 && address <= 0x4013) || address == 0x4015 || address == 0x4017)
            {
                Apu.CpuWrite(address, data);
            }
            else if (address == 0x4014)
            {
                _dmaPage = data;
                _dmaAddress = 0;
                _dmaTransfer = true;
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

        public void SetFrequency(uint sampleRate)
        {
            _audioTimePerSample = 1 / (double)sampleRate;
            _audioTimePerClock = 1 / (double)5369318; // Pixel Processing Rate (NTSC)
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
            Apu.Reset();

            _totalCycles = 0;
        }

        public bool Clock()
        {
            Ppu.Clock();

            Apu.Clock();

            if (_totalCycles % 3 == 0)
            {
                if (_dmaTransfer)
                {
                    if (_dmaDummy)
                    {
                        if (_totalCycles % 2 == 1)
                        {
                            _dmaDummy = false;
                        }
                    }
                    else
                    {
                        if (_totalCycles % 2 == 0)
                        {
                            _dmaData = Read((ushort)(_dmaPage << 8 | _dmaAddress));
                        }
                        else
                        {
                            Ppu.OAMData[_dmaAddress] = _dmaData;

                            _dmaAddress++;

                            if (_dmaAddress == 0)
                            {
                                _dmaTransfer = false;
                                _dmaDummy = true;
                            }
                        }
                    }
                }
                else
                {
                    Cpu.Clock();
                }
            }

            // Synchronize with audio
            var sampleReady = false;

            _audioTime += _audioTimePerClock;

            if (_audioTime >= _audioTimePerSample)
            {
                _audioTime -= _audioTimePerSample;
                AudioSample = Apu.GetOuputSample();
                sampleReady = true;
            }

            // Vertical Blanking Period Entered Interrupt
            if (Ppu.Nmi)
            {
                Ppu.Nmi = false;
                Cpu.Nmi();
            }

            _totalCycles++;

            return sampleReady;
        }
    }
}
