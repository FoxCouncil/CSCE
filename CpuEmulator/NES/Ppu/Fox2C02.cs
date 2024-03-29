﻿// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator.NES.Ppu
{
    using FoxEngineLib.Types.Drawing;
    using System;
    using System.Linq;

    public class Fox2C02
    {
        private Cartridge _cartridge;

        public short _scanline;

        public short _cycle;

        private byte _fineX;

        private byte _addressLatch;

        private byte _dataBuffer;

        private byte _bgNextTileId;

        private byte _bgNextTileAttribute;

        private byte _bgNextTileLsb;

        private byte _bgNextTileMsb;

        private ushort _bgShifterPatternLo;

        private ushort _bgShifterPatternHi;

        private ushort _bgShifterAttributeLo;

        private ushort _bgShifterAttributeHi;

        private StatusFlags _status;

        private MaskFlags _mask;

        private ControlFlags _control;

        private LoopyFlags _vRamAddress;

        private LoopyFlags _tRamAddress;

        private byte[,] _tablePattern = new byte[2, 4096];

        private byte[] _tablePalette = new byte[32];

        private Pixel[] _paletteScreen = new Pixel[64];

        private Sprite[] _spriteNameTable = new[] { new Sprite(256, 240), new Sprite(256, 240) };

        private Sprite[] _spritePatternTable = new[] { new Sprite(128, 128), new Sprite(128, 128) };

        private byte[] _spriteScanline = new byte[4 * 8];

        private byte[] _spriteShifterPatternLo = new byte[8];
         
        private byte[] _spriteShifterPatternHi = new byte[8];

        private byte _spriteCount;

        public Sprite Screen { get; } = new Sprite(256, 240);

        public byte[,] NameTable { get; } = new byte[2, 1024];

        public byte[] OAMData { get; set; } = new byte[64 * 4];

        public byte OAMAddress { get; set; }

        public bool Nmi { get; set; }

        public bool FrameComplete { get; set; }

        public Fox2C02()
        {
            _paletteScreen[0x00] = new Pixel(84, 84, 84);
            _paletteScreen[0x01] = new Pixel(0, 30, 116);
            _paletteScreen[0x02] = new Pixel(8, 16, 144);
            _paletteScreen[0x03] = new Pixel(48, 0, 136);
            _paletteScreen[0x04] = new Pixel(68, 0, 100);
            _paletteScreen[0x05] = new Pixel(92, 0, 48);
            _paletteScreen[0x06] = new Pixel(84, 4, 0);
            _paletteScreen[0x07] = new Pixel(60, 24, 0);
            _paletteScreen[0x08] = new Pixel(32, 42, 0);
            _paletteScreen[0x09] = new Pixel(8, 58, 0);
            _paletteScreen[0x0A] = new Pixel(0, 64, 0);
            _paletteScreen[0x0B] = new Pixel(0, 60, 0);
            _paletteScreen[0x0C] = new Pixel(0, 50, 60);
            _paletteScreen[0x0D] = new Pixel(0, 0, 0);
            _paletteScreen[0x0E] = new Pixel(0, 0, 0);
            _paletteScreen[0x0F] = new Pixel(0, 0, 0);

            _paletteScreen[0x10] = new Pixel(152, 150, 152);
            _paletteScreen[0x11] = new Pixel(8, 76, 196);
            _paletteScreen[0x12] = new Pixel(48, 50, 236);
            _paletteScreen[0x13] = new Pixel(92, 30, 228);
            _paletteScreen[0x14] = new Pixel(136, 20, 176);
            _paletteScreen[0x15] = new Pixel(160, 20, 100);
            _paletteScreen[0x16] = new Pixel(152, 34, 32);
            _paletteScreen[0x17] = new Pixel(120, 60, 0);
            _paletteScreen[0x18] = new Pixel(84, 90, 0);
            _paletteScreen[0x19] = new Pixel(40, 114, 0);
            _paletteScreen[0x1A] = new Pixel(8, 124, 0);
            _paletteScreen[0x1B] = new Pixel(0, 118, 40);
            _paletteScreen[0x1C] = new Pixel(0, 102, 120);
            _paletteScreen[0x1D] = new Pixel(0, 0, 0);
            _paletteScreen[0x1E] = new Pixel(0, 0, 0);
            _paletteScreen[0x1F] = new Pixel(0, 0, 0);

            _paletteScreen[0x20] = new Pixel(236, 238, 236);
            _paletteScreen[0x21] = new Pixel(76, 154, 236);
            _paletteScreen[0x22] = new Pixel(120, 124, 236);
            _paletteScreen[0x23] = new Pixel(176, 98, 236);
            _paletteScreen[0x24] = new Pixel(228, 84, 236);
            _paletteScreen[0x25] = new Pixel(236, 88, 180);
            _paletteScreen[0x26] = new Pixel(236, 106, 100);
            _paletteScreen[0x27] = new Pixel(212, 136, 32);
            _paletteScreen[0x28] = new Pixel(160, 170, 0);
            _paletteScreen[0x29] = new Pixel(116, 196, 0);
            _paletteScreen[0x2A] = new Pixel(76, 208, 32);
            _paletteScreen[0x2B] = new Pixel(56, 204, 108);
            _paletteScreen[0x2C] = new Pixel(56, 180, 204);
            _paletteScreen[0x2D] = new Pixel(60, 60, 60);
            _paletteScreen[0x2E] = new Pixel(0, 0, 0);
            _paletteScreen[0x2F] = new Pixel(0, 0, 0);

            _paletteScreen[0x30] = new Pixel(236, 238, 236);
            _paletteScreen[0x31] = new Pixel(168, 204, 236);
            _paletteScreen[0x32] = new Pixel(188, 188, 236);
            _paletteScreen[0x33] = new Pixel(212, 178, 236);
            _paletteScreen[0x34] = new Pixel(236, 174, 236);
            _paletteScreen[0x35] = new Pixel(236, 174, 212);
            _paletteScreen[0x36] = new Pixel(236, 180, 176);
            _paletteScreen[0x37] = new Pixel(228, 196, 144);
            _paletteScreen[0x38] = new Pixel(204, 210, 120);
            _paletteScreen[0x39] = new Pixel(180, 222, 120);
            _paletteScreen[0x3A] = new Pixel(168, 226, 144);
            _paletteScreen[0x3B] = new Pixel(152, 226, 180);
            _paletteScreen[0x3C] = new Pixel(160, 214, 228);
            _paletteScreen[0x3D] = new Pixel(160, 162, 160);
            _paletteScreen[0x3E] = new Pixel(0, 0, 0);
            _paletteScreen[0x3F] = new Pixel(0, 0, 0);
        }

        public Pixel GetColorFromPaletteRam(byte palette, byte pixel)
        {
            return _paletteScreen[PpuRead((ushort)(0x3F00 + (palette << 2) + pixel)) & 0x3F];
        }

        public Sprite GetPatternTable(byte index, byte palette)
        {
            for (ushort tileY = 0; tileY < 16; tileY++)
            {
                for (ushort tileX = 0; tileX < 16; tileX++)
                {
                    ushort nOffset = (ushort)(tileY * 256 + tileX * 16);

                    for (ushort row = 0; row < 8; row++)
                    {
                        byte tileLsb = PpuRead((ushort)(index * 0x1000 + nOffset + row));
                        byte tileMsb = PpuRead((ushort)(index * 0x1000 + nOffset + row + 0x0008));

                        for (ushort col = 0; col < 8; col++)
                        {
                            byte pixel = (byte)(((tileLsb & 0x01) << 1) | (tileMsb & 0x01));

                            tileLsb >>= 1;
                            tileMsb >>= 1;

                            _spritePatternTable[index].SetPixel(tileX * 8 + (7 - col), tileY * 8 + row, GetColorFromPaletteRam(palette, pixel));
                        }
                    }
                }
            }

            return _spritePatternTable[index];
        }

        public void ConnectCartridge(Cartridge cartridge)
        {
            _cartridge = cartridge;
        }

        public void Clock()
        {
            void incrementScrollX()
            {
                if (_mask.RenderBackground || _mask.RenderSprites)
                {
                    if (_vRamAddress.CoarseX == 31)
                    {
                        _vRamAddress.CoarseX = 0;
                        _vRamAddress.NameTableX = (ushort)(~_vRamAddress.NameTableX & 1);
                    }
                    else
                    {
                        _vRamAddress.CoarseX++;
                    }
                }
            }

            void incrementScrollY()
            {
                if (_mask.RenderBackground || _mask.RenderSprites)
                {
                    if (_vRamAddress.FineY < 7)
                    {
                        _vRamAddress.FineY++;
                    }
                    else
                    {
                        _vRamAddress.FineY = 0;

                        if (_vRamAddress.CoarseY == 29)
                        {
                            _vRamAddress.CoarseY = 0;
                            _vRamAddress.NameTableY = (ushort)(~_vRamAddress.NameTableY & 1);
                        }
                        else if (_vRamAddress.CoarseY == 31)
                        {
                            _vRamAddress.CoarseY = 0;
                        }
                        else
                        {
                            _vRamAddress.CoarseY++;
                        }
                    }
                }
            }

            void transferAddressX()
            {
                if (_mask.RenderBackground || _mask.RenderSprites)
                {
                    _vRamAddress.NameTableX = _tRamAddress.NameTableX;
                    _vRamAddress.CoarseX = _tRamAddress.CoarseX;
                }
            }

            void transferAddressY()
            {
                if (_mask.RenderBackground || _mask.RenderSprites)
                {
                    _vRamAddress.FineY = _tRamAddress.FineY;
                    _vRamAddress.NameTableY = _tRamAddress.NameTableY;
                    _vRamAddress.CoarseY = _tRamAddress.CoarseY;
                }
            }

            void loadBackgroundShifters()
            {
                _bgShifterPatternLo = (ushort)((_bgShifterPatternLo & 0xFF00) | _bgNextTileLsb);
                _bgShifterPatternHi = (ushort)((_bgShifterPatternHi & 0xFF00) | _bgNextTileMsb);

                _bgShifterAttributeLo = (ushort)((_bgShifterAttributeLo & 0xFF00) | ((_bgNextTileAttribute & 0b01) > 0 ? 0xFF : 0x00));
                _bgShifterAttributeHi = (ushort)((_bgShifterAttributeHi & 0xFF00) | ((_bgNextTileAttribute & 0b10) > 0 ? 0xFF : 0x00));
            }

            void updateShifters()
            {
                if (_mask.RenderBackground)
                {
                    _bgShifterPatternLo <<= 1;
                    _bgShifterPatternHi <<= 1;
                    _bgShifterAttributeLo <<= 1;
                    _bgShifterAttributeHi <<= 1;
                }

                if (_mask.RenderSprites && _cycle >= 1 && _cycle < 258)
                {
                    for (var i = 0; i < _spriteCount; i++)
                    {
                        var spriteAddr = i.Address(4);

                        if (_spriteScanline[spriteAddr + 3] > 0)
                        {
                            _spriteScanline[spriteAddr + 3]--;
                        }
                        else
                        {
                            _spriteShifterPatternLo[i] <<= 1;
                            _spriteShifterPatternHi[i] <<= 1;
                        }
                    }
                }
            }

            if (_scanline >= -1 && _scanline < 240)
            {
                if (_scanline == 0 && _cycle == 0)
                {
                    _cycle = 1;
                }

                if (_scanline == -1 && _cycle == 1)
                {
                    _status.VerticalBlank = false;
                    _status.SpriteOverflow = false;

                    for (var i = 0; i < _spriteCount; i++)
                    {
                        _spriteShifterPatternLo[i] = 0;
                        _spriteShifterPatternHi[i] = 0;
                    }
                }

                if ((_cycle >= 2 && _cycle < 258) || (_cycle >= 321 && _cycle < 338))
                {
                    updateShifters();

                    switch ((_cycle - 1) % 8)
                    {
                        case 0:
                        {
                            loadBackgroundShifters();

                            _bgNextTileId = PpuRead((ushort)(0x2000 | (_vRamAddress.Register & 0x0FFF)));
                        }
                        break;

                        case 2:
                        {
                            _bgNextTileAttribute = PpuRead((ushort)(0x23C0 | (_vRamAddress.NameTableY << 11) | (_vRamAddress.NameTableX << 10) | ((_vRamAddress.CoarseY >> 2) << 3) | (_vRamAddress.CoarseX >> 2)));

                            if ((_vRamAddress.CoarseY & 0x02) > 0)
                            {
                                _bgNextTileAttribute >>= 4;
                            }

                            if ((_vRamAddress.CoarseX & 0x02) > 0)
                            {
                                _bgNextTileAttribute >>= 2;
                            }

                            _bgNextTileAttribute &= 0x03;
                        }
                        break;

                        case 4:
                        {
                            ushort address = (ushort)((_control.PatternBackground << 12) + (_bgNextTileId << 4) + _vRamAddress.FineY);
                            _bgNextTileLsb = PpuRead(address);
                        }
                        break;

                        case 6:
                        {
                            ushort address = (ushort)((_control.PatternBackground << 12) + (_bgNextTileId << 4) + _vRamAddress.FineY + 8);
                            _bgNextTileMsb = PpuRead(address);
                        }
                        break;

                        case 7:
                        {
                            incrementScrollX();
                        }
                        break;
                    }
                }

                if (_cycle == 256)
                {
                    incrementScrollY();
                }
                else if (_cycle == 257)
                {
                    loadBackgroundShifters();
                    transferAddressX();
                }

                if (_scanline == -1 && _cycle >= 280 && _cycle < 305)
                {
                    transferAddressY();
                }

                if (_cycle == 338 || _cycle == 340)
                {
                    _bgNextTileId = PpuRead((ushort)(0x2000 | (_vRamAddress.Register & 0x0FFF)));
                }

                // Foreground
                if (_cycle == 257 && _scanline >= 0)
                {
                    _spriteScanline = Enumerable.Repeat((byte)0xFF, _spriteScanline.Length).ToArray();
                    _spriteCount = 0;

                    byte oamEntry = 0x00;

                    while (oamEntry < 64 && _spriteCount < 9)
                    {
                        var oamEntryAddr = oamEntry.Address(4);
                        var diff = (short)(_scanline - OAMData[oamEntryAddr]);

                        if (diff >= 0 && diff < (_control.SpriteSize > 0 ? 16 : 8))
                        {
                            if (_spriteCount < 8)
                            {
                                Array.Copy(OAMData, oamEntryAddr, _spriteScanline, _spriteCount.Address(4), 4);
                                _spriteCount++;
                            }
                        }

                        oamEntry++;
                    }

                    _status.SpriteOverflow = _spriteCount > 8;
                }

                if (_cycle == 340)
                {
                    for (var i = 0; i < _spriteCount; i++)
                    {
                        byte spritePatternBitsLo, spritePatternBitsHi;
                        ushort spritePatternAddressLo, spritePatternAddressHi;

                        var idxAddr = i.Address(4);

                        if (_control.SpriteSize == 0)
                        {
                            // 8x8
                            var attribute = _spriteScanline[idxAddr + 2];
                            
                            if ((attribute & 0x80) == 0)
                            {
                                // Sprite Not Flipped Vertically, normal
                                spritePatternAddressLo = (ushort)(
                                    (_control.PatternSprite << 12) 
                                    | (_spriteScanline[idxAddr + 1] << 4) 
                                    | (_scanline - _spriteScanline[idxAddr])
                                );

                            } 
                            else
                            {
                                // Sprite Is Flipped Vertically, upside down
                                spritePatternAddressLo = (ushort)(
                                    (_control.PatternSprite << 12) 
                                    | (_spriteScanline[idxAddr + 1] << 4) 
                                    | (7 - (_scanline - _spriteScanline[idxAddr]))
                                );
                            }
                        }
                        else
                        {
                            // 8x16
                            var attribute = _spriteScanline[idxAddr + 2];
                            
                            if ((attribute & 0x80) == 0)
                            {
                                // Sprite Not Flipped Vertically, normal
                                if (_scanline - _spriteScanline[idxAddr] < 8)
                                {
                                    // Top Half
                                    spritePatternAddressLo = (ushort)(
                                        ((_spriteScanline[idxAddr + 1] & 0x01) << 12) 
                                        | ((_spriteScanline[idxAddr + 1] & 0xFE) << 4)
                                        | ((_scanline - _spriteScanline[idxAddr]) & 0x07)
                                    );
                                }
                                else
                                {
                                    // Bottom Half
                                    spritePatternAddressLo = (ushort)(
                                        ((_spriteScanline[idxAddr + 1] & 0x01) << 12) 
                                        | (((_spriteScanline[idxAddr + 1] & 0xFE) + 1) << 4)
                                        | ((_scanline - _spriteScanline[idxAddr]) & 0x07)
                                    );
                                }
                            }
                            else
                            {
                                // Sprite Is Flipped Vertically, upside down
                                if (_scanline - _spriteScanline[idxAddr] < 8)
                                {
                                     // Top Half
                                    spritePatternAddressLo = (ushort)(
                                        ((_spriteScanline[idxAddr + 1] & 0x01) << 12) 
                                        | ((_spriteScanline[idxAddr + 1] & 0xFE) << 4)
                                        | (7 - (_scanline - _spriteScanline[idxAddr]) & 0x07)
                                    );
                                }
                                else
                                {
                                    // Bottom Half
                                    spritePatternAddressLo = (ushort)(
                                        ((_spriteScanline[idxAddr + 1] & 0x01) << 12) 
                                        | (((_spriteScanline[idxAddr + 1] & 0xFE) + 1) << 4)
                                        | (7 - (_scanline - _spriteScanline[idxAddr]) & 0x07)
                                    );
                                }
                            }
                        }

                        spritePatternAddressHi = (ushort)(spritePatternAddressLo + 8);

                        spritePatternBitsLo = PpuRead(spritePatternAddressLo);
                        spritePatternBitsHi = PpuRead(spritePatternAddressHi);

                        if ((_spriteScanline[idxAddr + 2] & 0x40) > 0)
                        {
                            Func<byte, byte> flipByte = (byte b) =>
                            {
                                b = (byte)((b & 0xF0) >> 4 | (b & 0x0F) << 4);
                                b = (byte)((b & 0xCC) >> 2 | (b & 0x33) << 2);
                                b = (byte)((b & 0xAA) >> 1 | (b & 0x55) << 1);

                                return b;
                            };

                            spritePatternBitsLo = flipByte(spritePatternBitsLo);
                            spritePatternBitsHi = flipByte(spritePatternBitsHi);
                        }

                        _spriteShifterPatternLo[i] = spritePatternBitsLo;
                        _spriteShifterPatternHi[i] = spritePatternBitsHi;
                    }
                }
            }

            if (_scanline == 240) { /* NOOP */ }

            if (_scanline >= 241 && _scanline < 261)
            {
                if (_scanline == 241 && _cycle == 1)
                {
                    _status.VerticalBlank = true;

                    if (_control.EnableNmi)
                    {
                        Nmi = true;
                    }
                }
            }

            byte bgPixel = 0;
            byte bgPalette = 0;

            if (_mask.RenderBackground)
            {
                ushort bitMux = (ushort)(0x8000 >> _fineX);

                byte p0Pixel = (byte)((_bgShifterPatternLo & bitMux) > 0 ? 1 : 0);
                byte p1Pixel = (byte)((_bgShifterPatternHi & bitMux) > 0 ? 1 : 0);

                bgPixel = (byte)((p1Pixel << 1) | p0Pixel);

                byte bgPal0 = (byte)((_bgShifterAttributeLo & bitMux) > 0 ? 1 : 0);
                byte bgPal1 = (byte)((_bgShifterAttributeHi & bitMux) > 0 ? 1 : 0);

                bgPalette = (byte)((bgPal1 << 1) | bgPal0);
            }

            byte fgPixel = 0;
            byte fgPalette = 0;
            byte fgPriority = 0;

            if (_mask.RenderSprites)
            {
                for (var i = 0; i < _spriteCount; i++)
                {
                    var spriteAddr = i.Address(4);

                    if (_spriteScanline[spriteAddr + 3] == 0)
                    {
                        byte fgPixelLo = (byte)((_spriteShifterPatternLo[i] & 0x80) > 0 ? 1 : 0);
                        byte fgPixelHi = (byte)((_spriteShifterPatternHi[i] & 0x80) > 0 ? 1 : 0);

                        fgPixel = (byte)((fgPixelHi << 1) | fgPixelLo);

                        fgPalette = (byte)((_spriteScanline[spriteAddr + 2] & 0x03) + 0x04);
                        fgPriority = (byte)((_spriteScanline[spriteAddr + 2] & 0x20) == 0 ? 1 : 0);

                        if (fgPixel != 0)
                        {
                            break;
                        }
                    } 
                }
            }

            byte pixel = 0;
            byte palette = 0;

            if (bgPixel == 0 && fgPixel == 0)
            {
                pixel = 0;
                palette = 0;
            }
            else if (bgPixel == 0 && fgPixel > 0)
            {
                pixel = fgPixel;
                palette = fgPalette;
            }
            else if (bgPixel > 0 && fgPixel == 0)
            {
                pixel = bgPixel;
                palette = bgPalette;
            }
            else
            {
                if (fgPriority > 0)
                {
                    pixel = fgPixel;
                    palette = fgPalette;
                }
                else
                {
                    pixel = bgPixel;
                    palette = bgPalette;
                }
            }

            Screen.SetPixel(_cycle - 1, _scanline, GetColorFromPaletteRam(palette, pixel));

            _cycle++;

            if (_cycle >= 341)
            {
                _cycle = 0;

                _scanline++;

                if (_scanline >= 261)
                {
                    _scanline = -1;

                    FrameComplete = true;
                }
            }
        }

        public void Reset()
        {
            _fineX = 0;
            _addressLatch = 0;
            _dataBuffer = 0;
            _scanline = 0;
            _cycle = 0;
            _bgNextTileId = 0;
            _bgNextTileAttribute = 0;
            _bgNextTileLsb = 0;
            _bgNextTileMsb = 0;
            _bgShifterPatternLo = 0;
            _bgShifterPatternHi = 0;
            _bgShifterAttributeLo = 0;
            _bgShifterAttributeHi = 0;
            _status.Register = 0;
            _mask.Register = 0;
            _control.Register = 0;
            _vRamAddress.Register = 0;
            _tRamAddress.Register = 0;
        }

        public void CpuWrite(ushort address, byte data)
        {
            switch (address)
            {
                case 0x0000: // Control
                {
                    _control.Register = data;
                    _tRamAddress.NameTableX = _control.NameTableX;
                    _tRamAddress.NameTableY = _control.NameTableY;
                }
                break;

                case 0x0001: // Mask
                {
                    _mask.Register = data;
                }
                break;

                case 0x0002: // Status
                    break;

                case 0x0003: // OAM Address
                {
                    OAMAddress = data;
                }
                break;

                case 0x0004: // OAM Data
                {
                    OAMData[OAMAddress] = data;
                }
                break;

                case 0x0005: // Scroll
                {
                    if (_addressLatch == 0)
                    {
                        _fineX = (byte)(data & 0x07);
                        _tRamAddress.CoarseX = (ushort)(data >> 3);
                        _addressLatch = 1;
                    }
                    else
                    {
                        _tRamAddress.FineY = (ushort)(data & 0x07);
                        _tRamAddress.CoarseY = (ushort)(data >> 3);
                        _addressLatch = 0;
                    }
                }
                break;

                case 0x0006: // PPU Address
                {
                    if (_addressLatch == 0)
                    {
                        _tRamAddress.Register = (ushort)(((data & 0x3F) << 8) | (_tRamAddress.Register & 0x00FF));
                        _addressLatch = 1;
                    }
                    else
                    {
                        _tRamAddress.Register = (ushort)((_tRamAddress.Register & 0xFF00) | data);
                        _vRamAddress = _tRamAddress;
                        _addressLatch = 0;
                    }
                }
                break;

                case 0x0007: // PPU Data
                {
                    PpuWrite(_vRamAddress.Register, data);
                    _vRamAddress.Register += (ushort)(_control.IncrementMode > 0 ? 32 : 1);
                }
                break;
            }
        }

        public byte CpuRead(ushort address, bool readOnly = false)
        {
            var data = byte.MinValue;

            if (readOnly)
            {
                switch (address)
                {
                    case 0x0000: // Control
                    {
                        data = _control.Register;
                    }
                    break;

                    case 0x0001: // Mask
                    {
                        data = _mask.Register;
                    }
                    break;

                    case 0x0002: // Status
                    {
                        data = _status.Register;
                    }
                    break;

                    case 0x0003: // OAM Address
                    case 0x0004: // OAM Data
                    case 0x0005: // Scroll
                    case 0x0006: // PPU Address
                    case 0x0007: // PPU Data
                        break;
                }
            }
            else
            {
                switch (address)
                {
                    case 0x0000: // Control
                    case 0x0001: // Mask
                        break;

                    case 0x0002: // Status
                    {
                        data = (byte)((_status.Register & 0xE0) | (_dataBuffer & 0x1F));

                        _status.VerticalBlank = false;

                        _addressLatch = 0;
                    }
                    break;

                    case 0x0003: // OAM Address

                    case 0x0004: // OAM Data
                    {
                        data = OAMData[OAMAddress];
                    }
                    break;

                    case 0x0005: // Scroll
                    case 0x0006: // PPU Address
                        break;

                    case 0x0007: // PPU Data
                    {
                        data = _dataBuffer;

                        _dataBuffer = PpuRead(_vRamAddress.Register);

                        if (_vRamAddress.Register >= 0x3F00)
                        {
                            data = _dataBuffer;
                        }

                        _vRamAddress.Register += (ushort)(_control.IncrementMode > 0 ? 32 : 1);
                    }
                    break;
                }
            }

            return data;
        }

        public void PpuWrite(ushort address, byte data)
        {
            address &= 0x3FFF;

            if (_cartridge.PpuWrite(address, data))
            {

            }
            else if (address >= 0 && address <= 0x1FFF)
            {
                _tablePattern[(address & 0x1000) >> 12, address & 0x0FFF] = data;
            }
            else if (address >= 0x2000 && address <= 0x3EFF)
            {
                address &= 0x0FFF;

                if (_cartridge.Mirror == Mirror.Vertical)
                {
                    // Vertical
                    if (address >= 0x0000 && address <= 0x03FF)
                    {
                        NameTable[0, address & 0x03FF] = data;
                    }
                    else if (address >= 0x0400 && address <= 0x07FF)
                    {
                        NameTable[1, address & 0x03FF] = data;
                    }
                    else if (address >= 0x0800 && address <= 0x0BFF)
                    {
                        NameTable[0, address & 0x03FF] = data;
                    }
                    else if (address >= 0x0C00 && address <= 0x0FFF)
                    {
                        NameTable[1, address & 0x03FF] = data;
                    }
                }
                else if (_cartridge.Mirror == Mirror.Horizontal)
                {
                    // Horizontal
                    if (address >= 0x0000 && address <= 0x03FF)
                    {
                        NameTable[0, address & 0x03FF] = data;
                    }
                    else if (address >= 0x0400 && address <= 0x07FF)
                    {
                        NameTable[0, address & 0x03FF] = data;
                    }
                    else if (address >= 0x0800 && address <= 0x0BFF)
                    {
                        NameTable[1, address & 0x03FF] = data;
                    }
                    else if (address >= 0x0C00 && address <= 0x0FFF)
                    {
                        NameTable[1, address & 0x03FF] = data;
                    }
                }
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                address &= 0x001F;

                if (address == 0x0010)
                {
                    address = 0x0000;
                }
                else if (address == 0x0014)
                {
                    address = 0x0004;
                }
                else if (address == 0x0018)
                {
                    address = 0x0008;
                }
                else if (address == 0x001C)
                {
                    address = 0x000C;
                }

                _tablePalette[address] = data;
            }
        }

        public byte PpuRead(ushort address)
        {
            var data = byte.MinValue;

            address &= 0x3FFF;

            if (_cartridge.PpuRead(address, ref data))
            {

            }
            else if (address >= 0 && address <= 0x1FFF)
            {
                data = _tablePattern[(address & 0x1000) >> 12, address & 0x0FFF];
            }
            else if (address >= 0x2000 && address <= 0x3EFF)
            {
                address &= 0x0FFF;

                if (_cartridge.Mirror == Mirror.Vertical)
                {
                    if (address >= 0x0000 && address <= 0x03FF)
                    {
                        data = NameTable[0, address & 0x03FF];
                    }
                    else if (address >= 0x0400 && address <= 0x07FF)
                    {
                        data = NameTable[1, address & 0x03FF];
                    }
                    else if (address >= 0x0800 && address <= 0x0BFF)
                    {
                        data = NameTable[0, address & 0x03FF];
                    }
                    else if (address >= 0x0C00 && address <= 0x0FFF)
                    {
                        data = NameTable[1, address & 0x03FF];
                    }
                }
                else if (_cartridge.Mirror == Mirror.Horizontal)
                {
                    if (address >= 0x0000 && address <= 0x03FF)
                    {
                        data = NameTable[0, address & 0x03FF];
                    }
                    else if (address >= 0x0400 && address <= 0x07FF)
                    {
                        data = NameTable[0, address & 0x03FF];
                    }
                    else if (address >= 0x0800 && address <= 0x0BFF)
                    {
                        data = NameTable[1, address & 0x03FF];
                    }
                    else if (address >= 0x0C00 && address <= 0x0FFF)
                    {
                        data = NameTable[1, address & 0x03FF];
                    }
                }
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                address &= 0x001F;

                if (address == 0x0010)
                {
                    address = 0x0000;
                }
                else if (address == 0x0014)
                {
                    address = 0x0004;
                }
                else if (address == 0x0018)
                {
                    address = 0x0008;
                }
                else if (address == 0x001C)
                {
                    address = 0x000C;
                }

                data = (byte)(_tablePalette[address] & (_mask.Grayscale ? 0x30 : 0x3F));
            }

            return data;
        }
    }
}
