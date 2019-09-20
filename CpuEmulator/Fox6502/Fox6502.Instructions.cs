// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    using System;
    using System.Collections.Generic;
    using I = Fox6502.Instruction;

    partial class Fox6502
    {
        internal struct Instruction
        {
            public string Name { get; set; }

            public Func<byte> Operation { get; set; }

            public Func<byte> AddressMode { get; set; }

            public byte Cycles { get; set; }

            public Instruction(string name, Func<byte> op, Func<byte> addMode, byte cycles)
            {
                Name = name;
                Operation = op;
                AddressMode = addMode;
                Cycles = cycles;
            }
        }

        List<I> InstructionLookup;

        private void BuildInstructionSet()
        {
            InstructionLookup = new List<I>
            {
                new I("BRK", BRK, IMM, 7), new I("ORA", ORA, IZX, 6), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("???", NOP, IMP, 3), new I("ORA", ORA, ZP0, 3), new I("ASL", ASL, ZP0, 5), new I("???", XXX, IMP, 5), new I("PHP", PHP, IMP, 3), new I("ORA", ORA, IMM, 2), new I("ASL", ASL, IMP, 2), new I("???", XXX, IMP, 2), new I("???", NOP, IMP, 4), new I("ORA", ORA, ABS, 4), new I("ASL", ASL, ABS, 6), new I("???", XXX, IMP, 6),
                new I("BPL", BPL, REL, 2), new I("ORA", ORA, IZY, 5), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("???", NOP, IMP, 4), new I("ORA", ORA, ZPX, 4), new I("ASL", ASL, ZPX, 6), new I("???", XXX, IMP, 6), new I("CLC", CLC, IMP, 2), new I("ORA", ORA, ABY, 4), new I("???", NOP, IMP, 2), new I("???", XXX, IMP, 7), new I("???", NOP, IMP, 4), new I("ORA", ORA, ABX, 4), new I("ASL", ASL, ABX, 7), new I("???", XXX, IMP, 7),
                new I("JSR", JSR, ABS, 6), new I("AND", AND, IZX, 6), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("BIT", BIT, ZP0, 3), new I("AND", AND, ZP0, 3), new I("ROL", ROL, ZP0, 5), new I("???", XXX, IMP, 5), new I("PLP", PLP, IMP, 4), new I("AND", AND, IMM, 2), new I("ROL", ROL, IMP, 2), new I("???", XXX, IMP, 2), new I("BIT", BIT, ABS, 4), new I("AND", AND, ABS, 4), new I("ROL", ROL, ABS, 6), new I("???", XXX, IMP, 6),
                new I("BMI", BMI, REL, 2), new I("AND", AND, IZY, 5), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("???", NOP, IMP, 4), new I("AND", AND, ZPX, 4), new I("ROL", ROL, ZPX, 6), new I("???", XXX, IMP, 6), new I("SEC", SEC, IMP, 2), new I("AND", AND, ABY, 4), new I("???", NOP, IMP, 2), new I("???", XXX, IMP, 7), new I("???", NOP, IMP, 4), new I("AND", AND, ABX, 4), new I("ROL", ROL, ABX, 7), new I("???", XXX, IMP, 7),
                new I("RTI", RTI, IMP, 6), new I("EOR", EOR, IZX, 6), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("???", NOP, IMP, 3), new I("EOR", EOR, ZP0, 3), new I("LSR", LSR, ZP0, 5), new I("???", XXX, IMP, 5), new I("PHA", PHA, IMP, 3), new I("EOR", EOR, IMM, 2), new I("LSR", LSR, IMP, 2), new I("???", XXX, IMP, 2), new I("JMP", JMP, ABS, 3), new I("EOR", EOR, ABS, 4), new I("LSR", LSR, ABS, 6), new I("???", XXX, IMP, 6),
                new I("BVC", BVC, REL, 2), new I("EOR", EOR, IZY, 5), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("???", NOP, IMP, 4), new I("EOR", EOR, ZPX, 4), new I("LSR", LSR, ZPX, 6), new I("???", XXX, IMP, 6), new I("CLI", CLI, IMP, 2), new I("EOR", EOR, ABY, 4), new I("???", NOP, IMP, 2), new I("???", XXX, IMP, 7), new I("???", NOP, IMP, 4), new I("EOR", EOR, ABX, 4), new I("LSR", LSR, ABX, 7), new I("???", XXX, IMP, 7),
                new I("RTS", RTS, IMP, 6), new I("ADC", ADC, IZX, 6), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("???", NOP, IMP, 3), new I("ADC", ADC, ZP0, 3), new I("ROR", ROR, ZP0, 5), new I("???", XXX, IMP, 5), new I("PLA", PLA, IMP, 4), new I("ADC", ADC, IMM, 2), new I("ROR", ROR, IMP, 2), new I("???", XXX, IMP, 2), new I("JMP", JMP, IND, 5), new I("ADC", ADC, ABS, 4), new I("ROR", ROR, ABS, 6), new I("???", XXX, IMP, 6),
                new I("BVS", BVS, REL, 2), new I("ADC", ADC, IZY, 5), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("???", NOP, IMP, 4), new I("ADC", ADC, ZPX, 4), new I("ROR", ROR, ZPX, 6), new I("???", XXX, IMP, 6), new I("SEI", SEI, IMP, 2), new I("ADC", ADC, ABY, 4), new I("???", NOP, IMP, 2), new I("???", XXX, IMP, 7), new I("???", NOP, IMP, 4), new I("ADC", ADC, ABX, 4), new I("ROR", ROR, ABX, 7), new I("???", XXX, IMP, 7),
                new I("???", NOP, IMP, 2), new I("STA", STA, IZX, 6), new I("???", NOP, IMP, 2), new I("???", XXX, IMP, 6), new I("STY", STY, ZP0, 3), new I("STA", STA, ZP0, 3), new I("STX", STX, ZP0, 3), new I("???", XXX, IMP, 3), new I("DEY", DEY, IMP, 2), new I("???", NOP, IMP, 2), new I("TXA", TXA, IMP, 2), new I("???", XXX, IMP, 2), new I("STY", STY, ABS, 4), new I("STA", STA, ABS, 4), new I("STX", STX, ABS, 4), new I("???", XXX, IMP, 4),
                new I("BCC", BCC, REL, 2), new I("STA", STA, IZY, 6), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 6), new I("STY", STY, ZPX, 4), new I("STA", STA, ZPX, 4), new I("STX", STX, ZPY, 4), new I("???", XXX, IMP, 4), new I("TYA", TYA, IMP, 2), new I("STA", STA, ABY, 5), new I("TXS", TXS, IMP, 2), new I("???", XXX, IMP, 5), new I("???", NOP, IMP, 5), new I("STA", STA, ABX, 5), new I("???", XXX, IMP, 5), new I("???", XXX, IMP, 5),
                new I("LDY", LDY, IMM, 2), new I("LDA", LDA, IZX, 6), new I("LDX", LDX, IMM, 2), new I("???", XXX, IMP, 6), new I("LDY", LDY, ZP0, 3), new I("LDA", LDA, ZP0, 3), new I("LDX", LDX, ZP0, 3), new I("???", XXX, IMP, 3), new I("TAY", TAY, IMP, 2), new I("LDA", LDA, IMM, 2), new I("TAX", TAX, IMP, 2), new I("???", XXX, IMP, 2), new I("LDY", LDY, ABS, 4), new I("LDA", LDA, ABS, 4), new I("LDX", LDX, ABS, 4), new I("???", XXX, IMP, 4),
                new I("BCS", BCS, REL, 2), new I("LDA", LDA, IZY, 5), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 5), new I("LDY", LDY, ZPX, 4), new I("LDA", LDA, ZPX, 4), new I("LDX", LDX, ZPY, 4), new I("???", XXX, IMP, 4), new I("CLV", CLV, IMP, 2), new I("LDA", LDA, ABY, 4), new I("TSX", TSX, IMP, 2), new I("???", XXX, IMP, 4), new I("LDY", LDY, ABX, 4), new I("LDA", LDA, ABX, 4), new I("LDX", LDX, ABY, 4), new I("???", XXX, IMP, 4),
                new I("CPY", CPY, IMM, 2), new I("CMP", CMP, IZX, 6), new I("???", NOP, IMP, 2), new I("???", XXX, IMP, 8), new I("CPY", CPY, ZP0, 3), new I("CMP", CMP, ZP0, 3), new I("DEC", DEC, ZP0, 5), new I("???", XXX, IMP, 5), new I("INY", INY, IMP, 2), new I("CMP", CMP, IMM, 2), new I("DEX", DEX, IMP, 2), new I("???", XXX, IMP, 2), new I("CPY", CPY, ABS, 4), new I("CMP", CMP, ABS, 4), new I("DEC", DEC, ABS, 6), new I("???", XXX, IMP, 6),
                new I("BNE", BNE, REL, 2), new I("CMP", CMP, IZY, 5), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("???", NOP, IMP, 4), new I("CMP", CMP, ZPX, 4), new I("DEC", DEC, ZPX, 6), new I("???", XXX, IMP, 6), new I("CLD", CLD, IMP, 2), new I("CMP", CMP, ABY, 4), new I("NOP", NOP, IMP, 2), new I("???", XXX, IMP, 7), new I("???", NOP, IMP, 4), new I("CMP", CMP, ABX, 4), new I("DEC", DEC, ABX, 7), new I("???", XXX, IMP, 7),
                new I("CPX", CPX, IMM, 2), new I("SBC", SBC, IZX, 6), new I("???", NOP, IMP, 2), new I("???", XXX, IMP, 8), new I("CPX", CPX, ZP0, 3), new I("SBC", SBC, ZP0, 3), new I("INC", INC, ZP0, 5), new I("???", XXX, IMP, 5), new I("INX", INX, IMP, 2), new I("SBC", SBC, IMM, 2), new I("NOP", NOP, IMP, 2), new I("???", SBC, IMP, 2), new I("CPX", CPX, ABS, 4), new I("SBC", SBC, ABS, 4), new I("INC", INC, ABS, 6), new I("???", XXX, IMP, 6),
                new I("BEQ", BEQ, REL, 2), new I("SBC", SBC, IZY, 5), new I("???", XXX, IMP, 2), new I("???", XXX, IMP, 8), new I("???", NOP, IMP, 4), new I("SBC", SBC, ZPX, 4), new I("INC", INC, ZPX, 6), new I("???", XXX, IMP, 6), new I("SED", SED, IMP, 2), new I("SBC", SBC, ABY, 4), new I("NOP", NOP, IMP, 2), new I("???", XXX, IMP, 7), new I("???", NOP, IMP, 4), new I("SBC", SBC, ABX, 4), new I("INC", INC, ABX, 7), new I("???", XXX, IMP, 7)
            };
        }
    }
}
