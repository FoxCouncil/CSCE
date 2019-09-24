// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES
{
    using System;

    public abstract class Mapper
    {
        protected byte ProgramBanks;

        protected byte CharacterBanks;

        public Mapper(byte programBanks, byte characterBanks)
        {
            ProgramBanks = programBanks;
            CharacterBanks = characterBanks;

            Reset();
        }

        public abstract void Reset();

        public abstract bool CpuMapRead(ushort address, ref uint mappedAddress);

        public abstract bool CpuMapWrite(ushort address, ref uint mappedAddress);

        public abstract bool PpuMapRead(ushort address, ref uint mappedAddress);

        public abstract bool PpuMapWrite(ushort address, ref uint mappedAddress);
    }
}
