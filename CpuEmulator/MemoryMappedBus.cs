﻿// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    class MemoryMappedBus
    {
        // 64K Ram
        public byte[] RAM { get; set; } = new byte[64 * 1024];

        public byte Read(ushort address, bool readOnly = false)
        {
            return RAM[address];
        }

        public void Write(ushort address, byte data)
        {
            RAM[address] = data;
        }
    }
}
