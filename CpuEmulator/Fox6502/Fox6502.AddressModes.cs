// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    partial class Fox6502
    {
        private byte IMP()
        {
            _fetched = A;

            return byte.MinValue;
        }

        private byte IMM()
        {
            _addrAbs = PC++;

            return byte.MinValue;
        }

        private byte ZP0()
        {
            _addrAbs = BusRead(PC);

            PC++;

            _addrAbs &= 0x00FF;

            return byte.MinValue;
        }

        private byte ZPX()
        {
            _addrAbs = (ushort)(BusRead(PC) + X);

            PC++;

            _addrAbs &= 0x00FF;

            return byte.MinValue;
        }

        private byte ZPY()
        {
            _addrAbs = (ushort)(BusRead(PC) + Y);

            PC++;

            _addrAbs &= 0x00FF;

            return byte.MinValue;
        }

        private byte REL()
        {
            _addrRel = BusRead(PC);

            PC++;
            
            if ((_addrRel & 0x80) > 0)
            {
                _addrRel |= 0xFF00;
            }

            return byte.MinValue;
        }

        private byte ABS()
        {
            ushort lo = BusRead(PC);

            PC++;

            ushort hi = BusRead(PC);

            PC++;

            _addrAbs = (ushort)((hi << 8) | lo);

            return byte.MinValue;
        }

        private byte ABX()
        {
            ushort lo = BusRead(PC);

            PC++;

            ushort hi = BusRead(PC);

            PC++;

            _addrAbs = (ushort)((hi << 8) | lo);

            _addrAbs += X;

            if ((_addrAbs & 0xFF00) != (hi << 8))
            {
                return 1;
            }

            return byte.MinValue;
        }
        private byte ABY()
        {
            ushort lo = BusRead(PC);

            PC++;

            ushort hi = BusRead(PC);

            PC++;

            _addrAbs = (ushort)((hi << 8) | lo);

            _addrAbs += Y;

            if ((_addrAbs & 0xFF00) != (hi << 8))
            {
                return 1;
            }

            return byte.MinValue;
        }

        private byte IND()
        {
            ushort lo = BusRead(PC);

            PC++;

            ushort hi = BusRead(PC);

            PC++;

            ushort ptr = (ushort)((hi << 8) | lo);

            if (lo == 0x00FF) // Simulate page boundry hardware bug
            {
                _addrAbs = (ushort)(BusRead((ushort)((ptr & 0xFF00) << 8)) | BusRead(ptr));
            }
            else
            {
                _addrAbs = (ushort)(BusRead((ushort)((ptr + 1) << 8)) | BusRead(ptr));
            }

            return byte.MinValue;
        }

        private byte IZX()
        {
            var t = BusRead(PC);
            PC++;

            var lo = BusRead((ushort)((t + X) & 0x00FF));
            var hi = BusRead((ushort)((t + X + 1) & 0x00FF));

            _addrAbs = (ushort)((hi << 8) | lo);

            return byte.MinValue;
        }

        private byte IZY()
        {
            var t = BusRead(PC);
            PC++;

            var lo = BusRead((ushort)(t & 0x00FF));
            var hi = BusRead((ushort)((t + 1) & 0x00FF));

            _addrAbs = (ushort)((hi << 8) | lo);
            _addrAbs += Y;

            if ((_addrAbs & 0xFF00) != (hi << 8))
            {
                return 1;
            }

            return byte.MinValue;
        }
    }
}
