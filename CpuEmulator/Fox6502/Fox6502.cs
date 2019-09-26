// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

using CpuEmulator.NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CpuEmulator
{
    public partial class Fox6502
    {
        private byte _fetched;

        private ushort _temp;

        private ushort _addrAbs;

        private ushort _addrRel;

        private byte _opcode;

        private NesBus _bus;

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

        public byte CyclesLeft { get; private set; }

        public uint CyclesTotal { get; private set; }

        public bool Complete => CyclesLeft == 0;
        
        public StreamWriter _logFile;
        
        public Fox6502()
        {
            _logFile = File.AppendText("fox6502.log");

            _logFile.WriteLine();
            _logFile.WriteLine();
            _logFile.WriteLine($"---[ Start: {DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()} ]---");

            BuildInstructionSet();
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

            CyclesLeft = 8;

            _logFile.WriteLine($"---[ RESET ]---");
        }

        public void IRQ()
        {
            if (GetFlag(Flags.I))
            {
                // Push the program counter to the stack. It's 16-bits don't forget so that takes two pushes
                BusWrite((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
                SP--;
                BusWrite((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
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
                CyclesLeft = 7;
            }
        }

        public void Nmi()
        {
            // Push the program counter to the stack. It's 16-bits don't forget so that takes two pushes
            BusWrite((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
            SP--;
            BusWrite((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
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
            CyclesLeft = 8;
        }

        public void Clock()
        {
            if (CyclesLeft == 0)
            {
                _opcode = BusRead(PC);

                var logPc = PC;

                SetFlag(Flags.U, true);

                PC++;

                var instruction = InstructionLookup[_opcode];

                CyclesLeft = (byte)(instruction.Cycles + (instruction.AddressMode() & instruction.Operation()));

                SetFlag(Flags.U, true);

                //var idx = Emulator.Code.IndexOf(Emulator.Code.First(kIdx => kIdx.Item1 == logPc));

                //var it_a = Emulator.Code[idx];

                //_logFile.WriteLine($"{it_a.Item2.PadRight(29)} A:{A.ToString("X2")} X:{X.ToString("X2")} Y:{Y.ToString("X2")} P:{P.ToString("X2")} SP:{SP.ToString("X2")} PPU:{Emulator._nes.Ppu._cycle.ToString().PadLeft(3)},{Emulator._nes.Ppu._scaline.ToString().PadLeft(3)} CYC:{CyclesTotal}");
                //_logFile.Flush();
            }

            CyclesTotal++;

            CyclesLeft--;
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

        public void ConnectBus(NesBus bus)
        {
            _bus = bus;

            Reset();
        }

        public byte BusRead(ushort address)
        {
            return _bus.Read(address);
        }

        public void BusWrite(ushort address, byte data)
        {
            _bus.Write(address, data);
        }

        public List<Tuple<ushort, string>> Disassemble(ushort nStart, ushort nStop)
        {
            uint addr = nStart;
            byte value = 0x00, lo = 0x00, hi = 0x00;

            var mapLines = new List<Tuple<ushort, string>>();

            ushort line_addr = 0;

            // Starting at the specified address we read an instruction
            // byte, which in turn yields information from the lookup table
            // as to how many additional bytes we need to read and what the
            // addressing mode is. I need this info to assemble human readable
            // syntax, which is different depending upon the addressing mode

            // As the instruction is decoded, a std::string is assembled
            // with the readable output
            while (addr <= nStop)
            {
                line_addr = (ushort)addr;

                // Prefix line with instruction address
                var sInst = "$" + addr.ToString("X4") + ": ";

                // Read instruction, and get its readable name
                byte opcode = _bus.Read((ushort)addr);

                addr++;

                var instruction = InstructionLookup[opcode];

                sInst += instruction.Name;

                if (instruction.Name != "BRK" && instruction.Name != "NOP")
                {
                    sInst += " ";

                    // Get oprands from desired locations, and form the
                    // instruction based upon its addressing mode. These
                    // routines mimmick the actual fetch routine of the
                    // 6502 in order to get accurate data as part of the
                    // instruction
                    if (InstructionLookup[opcode].AddressMode == IMP)
                    {
                        sInst += "".PadRight(13) + "{IMP}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == IMM)
                    {
                        value = _bus.Read((ushort)addr);
                        addr++;
                        sInst += "#$" + value.ToString("X2").PadRight(10) + " {IMM}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == ZP0)
                    {
                        lo = _bus.Read((ushort)addr); addr++;
                        hi = 0x00;
                        sInst += " $" + lo.ToString("X2").PadRight(10) + " {ZP0}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == ZPX)
                    {
                        lo = _bus.Read((ushort)addr); addr++;
                        hi = 0x00;
                        sInst += " $" + (lo.ToString("X2") + ", X").PadRight(10) + " {ZPX}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == ZPY)
                    {
                        lo = _bus.Read((ushort)addr); addr++;
                        hi = 0x00;
                        sInst += " $" + (lo.ToString("X2") + ", Y").PadRight(10) + " {ZPY}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == IZX)
                    {
                        lo = _bus.Read((ushort)addr); addr++;
                        hi = 0x00;
                        sInst += "($" + (lo.ToString("X2") + ", X)").PadRight(10) + " {IZX}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == IZY)
                    {
                        lo = _bus.Read((ushort)addr); addr++;
                        hi = 0x00;
                        sInst += "($" + (lo.ToString("X2") + "), Y").PadRight(10) + " {IZY}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == ABS)
                    {
                        lo = _bus.Read((ushort)addr); addr++;
                        hi = _bus.Read((ushort)addr); addr++;
                        sInst += " $" + ((ushort)(hi << 8) | lo).ToString("X4").PadRight(10) + " {ABS}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == ABX)
                    {
                        lo = _bus.Read((ushort)addr); addr++;
                        hi = _bus.Read((ushort)addr); addr++;
                        sInst += " $" + (((ushort)(hi << 8) | lo).ToString("X4") + ", X").PadRight(10) + " {ABX}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == ABY)
                    {
                        lo = _bus.Read((ushort)addr); addr++;
                        hi = _bus.Read((ushort)addr); addr++;
                        sInst += " $" + (((ushort)(hi << 8) | lo).ToString("X4") + ", Y").PadRight(10) + " {ABY}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == IND)
                    {
                        lo = _bus.Read((ushort)addr); addr++;
                        hi = _bus.Read((ushort)addr); addr++;
                        sInst += "($" + ((ushort)(hi << 8) | lo).ToString("X4") + ") {IND}";
                    }
                    else if (InstructionLookup[opcode].AddressMode == REL)
                    {
                        value = _bus.Read((ushort)addr); addr++;
                        sInst += " $" + value.ToString("X2") + " [$" + (addr + (SByte)value).ToString("X4") + "] {REL}";
                    }
                }

                // Add the formed string to a std::map, using the instruction's
                // address as the key. This makes it convenient to look for later
                // as the instructions are variable in length, so a straight up
                // incremental index is not sufficient.
                mapLines.Add(new Tuple<ushort, string>(line_addr, sInst));
            }

            return mapLines;
        }

        [Flags]
        public enum Flags : byte
        {
            /// <summary>Carry</summary>
            C = (1 << 0),
            /// <summary>Zero</summary>
            Z = (1 << 1),
            /// <summary>Interrupt Disable</summary>
            I = (1 << 2),
            /// <summary>Decimal</summary>
            D = (1 << 3),
            /// <summary>B Flag, No CPU Effect</summary>
            B = (1 << 4),
            /// <summary>U Flag, No CPU Effect</summary>
            U = (1 << 5),
            /// <summary>Overflow</summary>
            V = (1 << 6),
            /// <summary>Negative</summary>
            N = (1 << 7)
        }
    }
}
