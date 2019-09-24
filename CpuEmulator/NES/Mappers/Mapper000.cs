// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

using System;
using System.Collections.Generic;
using System.Text;

namespace CpuEmulator.NES.Mappers
{
    class Mapper000 : Mapper
    {
        public Mapper000(byte programBanks, byte characterBanks) : base(programBanks, characterBanks) { }

        public override void Reset()
        {
        }

        public override bool CpuMapRead(ushort address, ref uint mappedAddress)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                mappedAddress = (uint)(address & (ProgramBanks > 1 ? 0x7FFF : 0x3FFF));

                return true;
            }

            return false;
        }

        public override bool CpuMapWrite(ushort address, ref uint mappedAddress)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                mappedAddress = (uint)(address & (ProgramBanks > 1 ? 0x7FFF : 0x3FFF));

                return true;
            }

            return false;
        }

        public override bool PpuMapRead(ushort address, ref uint mappedAddress)
        {
            if (address >= 0 && address <= 0x1FFF)
            {
                mappedAddress = address;

                return true;
            }

            return false;
        }

        public override bool PpuMapWrite(ushort address, ref uint mappedAddress)
        {
            if (address >= 0 && address <= 0x1FFF && CharacterBanks == 0)
            {
                mappedAddress = address;

                return true;
            }

            return false;
        }
    }
}
