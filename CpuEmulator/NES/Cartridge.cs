// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES
{
    using CpuEmulator.NES.Mappers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    public class Cartridge
    {
        private readonly byte _mapperId = 0;

        private readonly byte _programBanks = 0;

        private readonly byte _characterBanks = 0;

        private readonly byte[] _programMemory;

        private readonly byte[] _characterMemory;

        private readonly Mapper _mapper;

        public Mirror Mirror;

        public bool IsValid { get; private set; }

        public Cartridge()
        {
            IsValid = false;
        }

        public Cartridge(string filename)
        {
            if (!File.Exists(filename))
            {
                IsValid = false;

                return;
            }

            using (var fileStream = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                var header = fileStream.ToStruct<INesHeader>();

                if ((header.Mapper1 & 0x04) > 0)
                {
                    fileStream.BaseStream.Seek(512, SeekOrigin.End);
                }

                _mapperId = (byte)(((header.Mapper2 >> 4) << 4) | (header.Mapper1 >> 4));

                Mirror = (header.Mapper1 & 0x01) > 0 ? Mirror.Vertical : Mirror.Horizontal;

                byte fileType = 1;

                if (fileType == 1)
                {
                    _programBanks = header.ProgramRomChunks;
                    _programMemory = new byte[_programBanks * 16384];
                    fileStream.Read(_programMemory, 0, _programMemory.Length);

                    _characterBanks = header.CharacterRomChunks;
                    if (_characterBanks == 0)
                    {
                        _characterMemory = new byte[_characterBanks];
                    }
                    else
                    {
                        _characterMemory = new byte[_characterBanks * 8192];
                    }

                    fileStream.Read(_characterMemory, 0, _characterMemory.Length);
                }

                switch (_mapperId)
                {
                    case 0:
                    {
                        _mapper = new Mapper000(_programBanks, _characterBanks);
                    }
                    break;
                }

                IsValid = true;
            }
        }

        public bool CpuWrite(ushort address, byte data)
        {
            uint mappedAddress = 0;

            if (_mapper.CpuMapWrite(address, ref mappedAddress))
            {
                _programMemory[mappedAddress] = data;

                return true;
            }

            return false;
        }

        public bool CpuRead(ushort address, ref byte data)
        {
            uint mappedAddress = 0;

            if (_mapper.CpuMapRead(address, ref mappedAddress))
            {
                data = _programMemory[mappedAddress];

                return true;
            }

            return false;
        }

        public bool PpuWrite(ushort address, byte data)
        {
            uint mappedAddress = 0;

            if (_mapper.PpuMapWrite(address, ref mappedAddress))
            {
                _characterMemory[mappedAddress] = data;

                return true;
            }

            return false;
        }

        public bool PpuRead(ushort address, ref byte data)
        {
            uint mappedAddress = 0;

            if (_mapper.PpuMapRead(address, ref mappedAddress))
            {
                data = _characterMemory[mappedAddress];

                return true;
            }

            return false;
        }

        public void Reset()
        {
            if (_mapper != null)
            {
                _mapper.Reset();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INesHeader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string Name;
        public byte ProgramRomChunks;
        public byte CharacterRomChunks;
        public byte Mapper1;
        public byte Mapper2;
        public byte ProgramRomSize;
        public byte TvSystem1;
        public byte TvSystem2;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string Unused;
    }

    public enum Mirror
    {
        Horizontal,
        Vertical,
        ONESCREEN_LO,
        ONESCREEN_HI
    }
}
