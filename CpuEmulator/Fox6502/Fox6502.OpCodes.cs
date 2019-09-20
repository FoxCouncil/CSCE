// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    partial class Fox6502
    {
        /// <summary>ADC (ADd with Carry)</summary>
        /// <remarks>Affects Flags: N V Z C</remarks>
        /// <remarks>ADC results are dependant on the setting of the decimal flag. 
        /// In decimal mode, addition is carried out on the assumption that the values involved are packed BCD (Binary Coded Decimal). 
        /// There is no way to add without carry.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte ADC()
        {
            Fetch();

            _temp = (ushort)(A + _fetched + (GetFlag(Flags.C) ? 0 : 1));

            SetFlag(Flags.C, _temp > 255);

            SetFlag(Flags.Z, (_temp & 0x00FF) == 0);

            SetFlag(Flags.V, ((~(A ^ _fetched) & (A ^ _temp)) & 0x0080) > 0);

            SetFlag(Flags.N, (_temp & 0x80) > 0);

            A = (byte)(_temp & 0x00FF);

            return 1;
        }

        /// <summary>AND (bitwise AND with accumulator)</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>+ add 1 cycle if page boundary crossed</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte AND()
        {
            Fetch();

            A &= _fetched;

            SetFlag(Flags.Z, A == 0);

            SetFlag(Flags.N, (A & 0x80) > 0);

            return 1;
        }

        /// <summary>ASL (Arithmetic Shift Left)</summary>
        /// <remarks>Affects Flags: N Z C</remarks>
        /// <remarks>ASL shifts all bits left one position. 0 is shifted into bit 0 and the original bit 7 is shifted into the Carry.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte ASL()
        {
            Fetch();

            _temp = (ushort)(_fetched << 1);

            SetFlag(Flags.C, (_temp & 0xFF00) > 0);
	        SetFlag(Flags.Z, (_temp & 0x00FF) == 0x00);
	        SetFlag(Flags.N, (_temp & 0x80) > 0);

            if (InstructionLookup[_opcode].AddressMode == IMP)
            {
                A = (byte)(_temp & 0x00FF);
            }
            else
            {
                BusWrite(_addrAbs, (byte)(_temp & 0x00FF));
            }

            return byte.MinValue;
        }

        /// <summary>Branch Instructions: Branch on Carry Clear</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BCC()
        {
            if (!GetFlag(Flags.C))
            {
                _cycles++;

                _addrAbs = (ushort)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                {
                    _cycles++;
                }
		
		        PC = _addrAbs;
            }

            return byte.MinValue;
        }

        /// <summary>Branch Instructions: Branch on Carry Set</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BCS()
        {
            if (GetFlag(Flags.C))
            {
                _cycles++;

                _addrAbs = (ushort)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                {
                    _cycles++;
                }
		
		        PC = _addrAbs;
            }

            return byte.MinValue;
        }

        /// <summary>Branch Instructions: Branch on EQual</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BEQ()
        {
            if (GetFlag(Flags.Z))
            {
                _cycles++;

                _addrAbs = (ushort)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                {
                    _cycles++;
                }
		
		        PC = _addrAbs;
            }

            return byte.MinValue;
        }

        /// <summary>BIT (test BITs)</summary>
        /// <remarks>Affects Flags: N V Z</remarks>
        /// <remarks>Beware: a BIT instruction used in this way as a NOP does have effects: the flags may be modified, and the read of the absolute address, if it happens to access an I/O device, may cause an unwanted action.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BIT()
        {
            Fetch();

            _temp = (ushort)(A & _fetched);

            SetFlag(Flags.Z, (_temp & 0x00FF) == 0);

            SetFlag(Flags.N, (_fetched & (1 << 7)) > 0);

            SetFlag(Flags.V, (_fetched & (1 << 6)) > 0);

            return byte.MinValue;
        }

        /// <summary>Branch Instructions: Branch on MInus</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BMI()
        {
            if (GetFlag(Flags.N))
            {
                _cycles++;

                _addrAbs = (ushort)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                {
                    _cycles++;
                }
		
		        PC = _addrAbs;
            }

            return byte.MinValue;
        }

        /// <summary>Branch Instructions: Branch on Not Equal</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BNE()
        {
            if (!GetFlag(Flags.Z))
            {
                _cycles++;

                _addrAbs = (ushort)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                {
                    _cycles++;
                }
		
		        PC = _addrAbs;
            }

            return byte.MinValue;
        }

        /// <summary>Branch Instructions: Branch on PLus</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BPL()
        {
            if (!GetFlag(Flags.N))
            {
                _cycles++;

                _addrAbs = (ushort)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                {
                    _cycles++;
                }
		
		        PC = _addrAbs;
            }

            return byte.MinValue;
        }

        /// <summary>BRK (BReaK)</summary>
        /// <remarks>Affects Flags: B</remarks>
        /// <remarks>BRK causes a non-maskable interrupt and increments the program counter by one. Therefore an RTI will go to the address of the BRK +2 so that BRK may be used to replace a two-byte instruction for debugging and the subsequent RTI will be correct.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BRK()
        {
            PC++;

            SetFlag(Flags.I, true);

            BusWrite((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
            SP--;
            BusWrite((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
            SP--;

            SetFlag(Flags.B, true);

            BusWrite((ushort)(0x0100 + SP), P);
            SP--;

            SetFlag(Flags.B, false);

            PC = (ushort)(BusRead(0xFFFE) | BusRead(0xFFFF) << 8);

            return byte.MinValue;
        }

        /// <summary>Branch Instructions: Branch on oVerflow Clear</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BVC()
        {
            if (!GetFlag(Flags.V))
            {
                _cycles++;

                _addrAbs = (ushort)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                {
                    _cycles++;
                }

                PC = _addrAbs;
            }

            return byte.MinValue;
        }

        /// <summary>Branch Instructions: Branch on oVerflow Set</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte BVS()
        {
            if (GetFlag(Flags.V))
            {
                _cycles++;

                _addrAbs = (ushort)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                {
                    _cycles++;
                }

                PC = _addrAbs;
            }

            return byte.MinValue;
        }

        /// <summary>Flag Instructions: CLear Carry</summary>
        /// <remarks>Affects Flags: C</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte CLC()
        {
            SetFlag(Flags.C, false);

            return byte.MinValue;
        }

        /// <summary>Flag Instructions: CLear Decimal</summary>
        /// <remarks>Affects Flags: D</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte CLD()
        {
            SetFlag(Flags.D, false);

            return byte.MinValue;
        }

        /// <summary>Flag Instructions: CLear Interrupt</summary>
        /// <remarks>Affects Flags: I</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte CLI()
        {
            SetFlag(Flags.I, false);

            return byte.MinValue;
        }

        /// <summary>Flag Instructions: CLear oVerflow</summary>
        /// <remarks>Affects Flags: V</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte CLV()
        {
            SetFlag(Flags.V, false);

            return byte.MinValue;
        }

        /// <summary>CMP (CoMPare accumulator)</summary>
        /// <remarks>Affects Flags: N Z C</remarks>
        /// <remarks>Compare sets flags as if a subtraction had been carried out. If the value in the accumulator is equal or greater than the compared value, the Carry will be set. The equal (Z) and negative (N) flags will be set based on equality or lack thereof and the sign (i.e. A>=$80) of the accumulator.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte CMP()
        {
            Fetch();

            _temp = (ushort)(A - _fetched);

            SetFlag(Flags.C, A >= _fetched);

            SetFlag(Flags.Z, (_temp & 0x00FF) == 0);

            SetFlag(Flags.N, (_temp & 0x0080) > 0);

            return 1;
        }

        /// <summary>CPX (ComPare X register)</summary>
        /// <remarks>Affects Flags: N Z C</remarks>
        /// <remarks>Operation and flag results are identical to equivalent mode accumulator CMP ops.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte CPX()
        {
            Fetch();

            _temp = (ushort)(X - _fetched);

            SetFlag(Flags.C, X >= _fetched);

            SetFlag(Flags.Z, (_temp & 0x00FF) == 0);

            SetFlag(Flags.N, (_temp & 0x0080) > 0);

            return byte.MinValue;
        }

        /// <summary>CPY (ComPare Y register)</summary>
        /// <remarks>Affects Flags: N Z C</remarks>
        /// <remarks>Operation and flag results are identical to equivalent mode accumulator CMP ops.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte CPY()
        {
            Fetch();

            _temp = (ushort)(Y - _fetched);

            SetFlag(Flags.C, Y >= _fetched);

            SetFlag(Flags.Z, (_temp & 0x00FF) == 0);

            SetFlag(Flags.N, (_temp & 0x0080) > 0);

            return byte.MinValue;
        }

        /// <summary>DEC (DECrement memory)</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte DEC()
        {
            Fetch();

            _temp = (ushort)(_fetched - 1);

            BusWrite(_addrAbs, (byte)(_temp & 0x00FF));

            SetFlag(Flags.Z, (_temp & 0x00FF) == 0);

            SetFlag(Flags.N, (_temp & 0x0080) > 0);

            return byte.MinValue;
        }

        /// <summary>Register Instructions: DEcrement X</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte DEX()
        {
            X--;

            SetFlag(Flags.Z, X == 0);

            SetFlag(Flags.N, (X & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>Register Instructions: DEcrement Y</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte DEY()
        {
            Y--;

            SetFlag(Flags.Z, Y == 0);

            SetFlag(Flags.N, (Y & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>EOR (bitwise Exclusive OR)</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>+ add 1 cycle if page boundary crossed</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte EOR()
        {
            Fetch();

            A ^= _fetched;

            SetFlag(Flags.Z, A == 0);

            SetFlag(Flags.N, (A & 0x80) > 0);

            return 1;
        }

        /// <summary>INC (INCrement memory)</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte INC()
        {
            Fetch();

            _temp = (ushort)(_fetched + 1);

            BusWrite(_addrAbs, (byte)(_temp & 0x00FF));

            SetFlag(Flags.Z, (_temp & 0x00FF) == 0);

            SetFlag(Flags.N, (_temp & 0x0080) > 0);

            return byte.MinValue;
        }

        /// <summary>Register Instructions: INcrement X</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte INX()
        {
            X++;

            SetFlag(Flags.Z, X == 0);

            SetFlag(Flags.N, (X & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>Register Instructions: INcrement Y</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte INY()
        {
            Y++;

            SetFlag(Flags.Z, Y == 0);

            SetFlag(Flags.N, (Y & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>JMP (JuMP)</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <remarks>JMP transfers program execution to the following address (absolute) or to the location contained in the following address (indirect). Note that there is no carry associated with the indirect jump so:
        /// AN INDIRECT JUMP MUST NEVER USE A VECTOR BEGINNING ON THE LAST BYTE OF A PAGE</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte JMP()
        {
            PC = _addrAbs;

            return byte.MinValue;
        }

        /// <summary>JSR (Jump to SubRoutine)</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <remarks>JSR pushes the address-1 of the next operation on to the stack before transferring program control to the following address. Subroutines are normally terminated by a RTS op code.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte JSR()
        {
            PC--;

            BusWrite((ushort)(0x0100 + SP), (byte)((PC << 8) & 0x00FF));
            SP--;

            BusWrite((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
            SP--;

            PC = _addrAbs;

            return byte.MinValue;
        }

        /// <summary>LDA (LoaD Accumulator)</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>+ add 1 cycle if page boundary crossed</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte LDA()
        {
            Fetch();

            A = _fetched;

            SetFlag(Flags.Z, A == 0);

            SetFlag(Flags.N, (A & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>LDX (LoaD X register)</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>+ add 1 cycle if page boundary crossed</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte LDX()
        {
            Fetch();

            X = _fetched;

            SetFlag(Flags.Z, X == 0);

            SetFlag(Flags.N, (X & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>LDY (LoaD Y register)</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>+ add 1 cycle if page boundary crossed</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte LDY()
        {
            Fetch();

            Y = _fetched;

            SetFlag(Flags.Z, Y == 0);

            SetFlag(Flags.N, (Y & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>LSR (Logical Shift Right)</summary>
        /// <remarks>Affects Flags: N Z C</remarks>
        /// <remarks>LSR shifts all bits right one position. 0 is shifted into bit 7 and the original bit 0 is shifted into the Carry.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte LSR()
        {
            Fetch();

            SetFlag(Flags.C, (_fetched & 0x0001) > 0);

            _temp = (ushort)(_fetched >> 1);

            SetFlag(Flags.Z, (_temp & 0x00F) == 0);

            SetFlag(Flags.N, (_temp & 0x0080) > 0);

            if (InstructionLookup[_opcode].AddressMode == IMP)
            {
                A = (byte)(_temp & 0x00FF);
            }
            else
            {
                BusWrite(_addrAbs, (byte)(_temp & 0x00FF));
            }

            return byte.MinValue;
        }

        /// <summary>NOP (No OPeration)</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <remarks>NOP is used to reserve space for future modifications or effectively REM out existing code.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte NOP()
        {
            // https://wiki.nesdev.com/w/index.php/CPU_unofficial_opcodes
            switch (_opcode)
            {
                case 0x1C:
                case 0x3C:
                case 0x5C:
                case 0x7C:
                case 0xDC:
                case 0xFC:
	                return 1;
            }

            return byte.MinValue;
        }

        /// <summary>ORA (bitwise OR with Accumulator)</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>+ add 1 cycle if page boundary crossed</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte ORA()
        {
            Fetch();

            A |= _fetched;

            SetFlag(Flags.Z, A == 0);

            SetFlag(Flags.N, (A & 0x80) > 0);

            return 1;
        }

        /// <summary>Stack Instructions: PusH Accumulator</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte PHA()
        {
            BusWrite((ushort)(0x0100 + SP), A);
            SP--;

            return byte.MinValue;
        }

        /// <summary>Stack Instructions: PusH Processor status</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte PHP()
        {
            BusWrite((ushort)(0x0100 + SP), (byte)(P | (byte)Flags.B | (byte)Flags.U));

            SetFlag(Flags.B, false);

            SetFlag(Flags.U, false);

            SP--;

            return byte.MinValue;
        }

        /// <summary>Stack Instructions: PuLl Accumulator</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte PLA()
        {
            SP++;
            A = BusRead((ushort)(0x0100 + SP));

            SetFlag(Flags.Z, A == 0);

            SetFlag(Flags.N, (A & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>Stack Instructions: PuLl Processor status</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte PLP()
        {
            SP++;
            P = BusRead((ushort)(0x0100 + SP));

            SetFlag(Flags.U, true);

            return byte.MinValue;
        }

        /// <summary>ROL (ROtate Left)</summary>
        /// <remarks>Affects Flags: N Z C</remarks>
        /// <remarks>ROL shifts all bits left one position. The Carry is shifted into bit 0 and the original bit 7 is shifted into the Carry.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte ROL()
        {
            Fetch();

            _temp = (ushort)(_fetched << 1 | (GetFlag(Flags.C) ? 1 : 0));

            SetFlag(Flags.C, (_temp & 0xFF00) > 0);

	        SetFlag(Flags.Z, (_temp & 0x00FF) == 0);

	        SetFlag(Flags.N, (_temp & 0x0080) > 0);

            if (InstructionLookup[_opcode].AddressMode == IMP)
            {
                A = (byte)(_temp & 0x00FF);
            }
            else
            {
                BusWrite(_addrAbs, (byte)(_temp & 0x00FF));
            }

            return byte.MinValue;
        }

        /// <summary>ROR (ROtate Right)</summary>
        /// <remarks>Affects Flags: N Z C</remarks>
        /// <remarks>ROR shifts all bits right one position. The Carry is shifted into bit 7 and the original bit 0 is shifted into the Carry.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte ROR()
        {
            Fetch();

            _temp = (ushort)(((GetFlag(Flags.C) ? 1 : 0) << 7) | (_fetched >> 1));

            SetFlag(Flags.C, (_fetched & 0x01) > 0);

	        SetFlag(Flags.Z, (_temp & 0x00FF) == 0);

	        SetFlag(Flags.N, (_temp & 0x0080) > 0);

            if (InstructionLookup[_opcode].AddressMode == IMP)
            {
                A = (byte)(_temp & 0x00FF);
            }
            else
            {
                BusWrite(_addrAbs, (byte)(_temp & 0x00FF));
            }

            return byte.MinValue;
        }

        /// <summary>RTI (ReTurn from Interrupt)</summary>
        /// <remarks>Affects Flags: N V U B D I Z C</remarks>
        /// <remarks>RTI retrieves the Processor Status Word (flags) and the Program Counter from the stack in that order (interrupts push the PC first and then the PSW).</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte RTI()
        {
            SP++;
            P = BusRead((ushort)(0x0100 + SP));
            P &= (byte)~Flags.B;
            P &= (byte)~Flags.U;

            SP++;
            PC = BusRead((ushort)(0x0100 + SP));
            SP++;
            PC |= (ushort)(BusRead((ushort)(0x0100 + SP)) << 8);

            return byte.MinValue;
        }

        /// <summary>RTS (ReTurn from Subroutine)</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <remarks>RTS pulls the top two bytes off the stack (low byte first) and transfers program control to that address+1. It is used, as expected, to exit a subroutine invoked via JSR which pushed the address-1.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte RTS()
        {
            SP++;
            PC = BusRead((ushort)(0x0100 + SP));
            SP++;
            PC |= (ushort)(BusRead((ushort)(0x0100 + SP)) << 8);

            PC++;

            return byte.MinValue;
        }

        /// <summary>SBC (SuBtract with Carry)</summary>
        /// <remarks>Affects Flags: N V Z C</remarks>
        /// <remarks>+ add 1 cycle if page boundary crossed</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte SBC()
        {
            Fetch();

            var value = _fetched ^ 0x00FF;

            _temp = (ushort)(A + value + (GetFlag(Flags.C) ? 0 : 1));

            SetFlag(Flags.C, (_temp & 0xFF00) > 0);
            SetFlag(Flags.Z, (_temp & 0x00FF) == 0);
            SetFlag(Flags.V, ((_temp ^ A) & (_temp ^ value) & 0x0080) > 0);
            SetFlag(Flags.N, (_temp & 0x0080) > 0);

            A = (byte)(_temp & 0x00FF);

            return 1;
        }

        /// <summary>Flag Instructions: SEt Carry</summary>
        /// <remarks>Affects Flags: C</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte SEC()
        {
            SetFlag(Flags.C, true);

            return byte.MinValue;
        }

        /// <summary>Flag Instructions: SEt Decimal</summary>
        /// <remarks>Affects Flags: D</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte SED()
        {
            SetFlag(Flags.D, true);

            return byte.MinValue;
        }

        /// <summary>Flag Instructions: SEt Interrupt</summary>
        /// <remarks>Affects Flags: I</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte SEI()
        {
            SetFlag(Flags.I, true);

            return byte.MinValue;
        }

        /// <summary>STA (STore Accumulator)</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte STA()
        {
            BusWrite(_addrAbs, A);
            
            return byte.MinValue;
        }

        /// <summary>STX (STore X register)</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte STX()
        {
            BusWrite(_addrAbs, X);

            return byte.MinValue;
        }

        /// <summary>STY (STore Y register)</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte STY()
        {
            BusWrite(_addrAbs, Y);

            return byte.MinValue;
        }

        /// <summary>Register Instructions: Transfer A to X</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte TAX()
        {
            X = A;

            SetFlag(Flags.Z, X == 0);

            SetFlag(Flags.N, (X & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>Register Instructions: Transfer A to Y</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte TAY()
        {
            Y = A;

            SetFlag(Flags.Z, Y == 0);

            SetFlag(Flags.N, (Y & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>Stack Instructions: Transfer Stack ptr to X</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte TSX()
        {
            X = SP;

            SetFlag(Flags.Z, X == 0);

            SetFlag(Flags.N, (X & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>Register Instructions: Transfer X to A</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte TXA()
        {
            A = X;

            SetFlag(Flags.Z, A == 0);

            SetFlag(Flags.N, (A & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>Stack Instructions: Transfer X to Stack ptr</summary>
        /// <remarks>Affects Flags: None</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte TXS()
        {
            SP = X;

            return byte.MinValue;
        }

        /// <summary>Register Instructions: Transfer Y to A</summary>
        /// <remarks>Affects Flags: N Z</remarks>
        /// <remarks>These instructions are implied mode, have a length of one byte and require two machine cycles.</remarks>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte TYA()
        {
            A = Y;

            SetFlag(Flags.Z, A == 0);

            SetFlag(Flags.N, (A & 0x80) > 0);

            return byte.MinValue;
        }

        /// <summary>Illegal Instuction</summary>
        /// <returns>Any Extra Cycles Needed</returns>
        private byte XXX()
        {
            return byte.MinValue;
        }
    }
}
