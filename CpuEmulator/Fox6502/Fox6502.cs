// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

using System;

namespace CpuEmulator
{
    partial class Fox6502
    {
        private byte _fetched;

        private ushort _temp;

        private ushort _addrAbs;

        private ushort _addrRel;

        private byte _opcode;

        private byte _cycles;

        private uint _clockCount;

        private MemoryMappedBus _bus;

        // Main Register
        public byte A { get; set; }

        // Index Register
        public byte X { get; set; }

        // Index Register
        public byte Y { get; set; }

        // Index Register
        public byte SP { get; set; }

        // Program Counter
        public ushort PC { get; set; }

        // Status Register
        public byte P { get; set; }

        public Fox6502()
        {
            BuildInstructionSet();

            _bus = new MemoryMappedBus();

            Reset();
        }

        public void Reset()
        {
            _addrAbs = 0xFFFC;

            var lo = BusRead(_addrAbs);
            var hi = BusRead((ushort)(_addrAbs + 1));

            PC = (ushort)((hi << 0x8) | lo);

            A = 0;
            X = 0;
            Y = 0;
            SP = 0xFD;
            P = 0x00 | (byte)Flags.U;

            _addrRel = 0;
            _addrAbs = 0;
            _fetched = 0;

            _cycles = 8;
        }

        public void IRQ()
        {
            if (GetFlag(Flags.I))
            {
                // Push the program counter to the stack. It's 16-bits don't forget so that takes two pushes
                BusWrite((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
                SP--;
                BusWrite((ushort)(0x0100 + SP), (byte)(PC  & 0x00FF));
                SP--;

                // Then Push the status register to the stack
		        SetFlag(Flags.B, false);
		        SetFlag(Flags.U, true);
		        SetFlag(Flags.I, true);
                BusWrite((ushort)(0x0100 + SP), P);
                SP--;

                // Read new program counter location from fixed address
                _addrAbs = 0xFFFE;
                ushort lo = BusRead(_addrAbs);
                ushort hi = BusRead((ushort)(_addrAbs + 1));
                PC = (ushort)((hi << 8) | lo);

                // IRQ Takes Time!
                _cycles = 7;
            }
        }

        public void NMI()
        {
            // Push the program counter to the stack. It's 16-bits don't forget so that takes two pushes
            BusWrite((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
            SP--;
            BusWrite((ushort)(0x0100 + SP), (byte)(PC  & 0x00FF));
            SP--;

            // Then Push the status register to the stack
		    SetFlag(Flags.B, false);
		    SetFlag(Flags.U, true);
		    SetFlag(Flags.I, true);
            BusWrite((ushort)(0x0100 + SP), P);
            SP--;

            // Read new program counter location from fixed address
            _addrAbs = 0xFFFA;
            ushort lo = BusRead(_addrAbs);
            ushort hi = BusRead((ushort)(_addrAbs + 1));
            PC = (ushort)((hi << 8) | lo);

            // NMI Takes Time!
            _cycles = 8;
        }

        public void Clock()
        {
            if (_cycles == 0)
            {
                _opcode = BusRead(PC);

                SetFlag(Flags.U, true);

                PC++;

                var instruction = InstructionLookup[_opcode];

                _cycles = (byte)(instruction.Cycles + (instruction.AddressMode() & instruction.Operation()));

                SetFlag(Flags.U, true);
            }

            _clockCount++;

            _cycles--;
        }

        public bool GetFlag(Flags flag)
        {
            return (P & (byte)flag) > 0 ? true : false;
        }

        public void SetFlag(Flags flag, bool value)
        {
            if (value)
            {
                P |= (byte)flag;
            }
            else
            {
                P &= (byte)~flag;
            }
        }

        public byte Fetch()
        {
            if (InstructionLookup[_opcode].AddressMode != IMP)
            {
                _fetched = BusRead(_addrAbs);
            }

            return _fetched;
        }

        public byte BusRead(ushort address)
        {
            return _bus.Read(address);
        }

        public void BusWrite(ushort address, byte data)
        {
            _bus.Write(address, data);
        }

        [Flags]
        internal enum Flags : byte
        {
            C = (1 << 0),
            Z = (1 << 1),
            I = (1 << 2),
            D = (1 << 3),
            B = (1 << 4),
            U = (1 << 5),
            V = (1 << 6),
            N = (1 << 7)
        }
    }
}
