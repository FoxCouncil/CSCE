using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpuEmulator.NES.Apu
{
    public class Fox2A03
    {
        uint frame;

        uint clock; 

        Sequencer _pulse1Sequencer = new();

        Oscillator _pulse1Oscillator = new();

        bool _pulse1Enabled;

        double _pulse1Sample;

        double globalTime;

        public double GetOuputSample()
        {
            return _pulse1Sample;
        }

        public void Clock()
        {
            var quarter = false;
            var half = false;

            globalTime += (0.3333333333 / 1789773);

            if (clock % 6 == 0)
            {
                frame++;
                
                // Four Step Sequencer
                if (frame == 3729)
                {
                    quarter = true;
                }
                else if (frame == 7457)
                {
                    quarter = true;
                    half = true;
                }
                else if (frame == 11186)
                {
                    quarter = true;
                }
                else if (frame == 14916)
                {
                    quarter = true;
                    half = true;
                    frame = 0;
                }

                // Quarter Beat, adjust the volume evelops
                if (quarter)
                {

                }

                // Half Beat, adjust note length and frequency sweepers
                if (half)
                {

                }

                //_pulse1Sequencer.Clock(_pulse1Enabled, (ref uint s) => s = (s << 7 | s >> 1));
                //_pulse1Sample = _pulse1Sequencer.Output;

                _pulse1Oscillator.Frequency = 1789773.0 / (16.0 * (double)(_pulse1Sequencer.Reload + 1));
                _pulse1Sample = _pulse1Oscillator.Sample(globalTime);
            }

            clock++;
        }

        public void Reset()
        {

        }

        public void CpuWrite(ushort address, byte data)
        {
            switch (address)
            {
                case 0x4000:
                {
                    switch ((data & 0xC0) >> 6)
                    {
                        case 0x00: _pulse1Sequencer.Sequence = 0b00000001; _pulse1Oscillator.Rate = 0.125; break;
                        case 0x01: _pulse1Sequencer.Sequence = 0b00000011; _pulse1Oscillator.Rate = 0.250; break;
                        case 0x02: _pulse1Sequencer.Sequence = 0b00001111; _pulse1Oscillator.Rate = 0.500; break;
                        case 0x03: _pulse1Sequencer.Sequence = 0b11111100; _pulse1Oscillator.Rate = 0.750; break;
                    }
                }
                break;

                case 0x4002:
                {
                    _pulse1Sequencer.Reload = (ushort)((_pulse1Sequencer.Reload & 0xFF00) | data);
                }
                break;

                case 0x4003:
                {
                    _pulse1Sequencer.Reload = (ushort)((data & 0x07) << 8 | (_pulse1Sequencer.Reload & 0x00FF));
                    _pulse1Sequencer.Timer = _pulse1Sequencer.Reload;
                }
                break;

                case 0x4015:
                {
                    _pulse1Enabled = Convert.ToBoolean(data & 0x01);
                }
                break;
            }

        }

        public byte CpuRead(ushort address, bool readOnly = false)
        {
            return 0;
        }
    }
}
