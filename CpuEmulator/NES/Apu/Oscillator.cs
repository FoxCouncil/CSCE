using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpuEmulator.NES.Apu;

internal struct Oscillator
{
    public double Frequency { get; set; }

    public double Rate { get; set; }

    public double Amplitude { get; set; } = 1;

    public double Harmonics { get; set; } = 20;

    public double Sample(double time)
    {
        double a = 0, b = 0, p = Rate * 2.0 * Math.PI;

        for (var i = 0; i < Harmonics; i++)
        {
            var c = i * Frequency * 2.0 * Math.PI * time;
            
            var one = -ApproxSin((float)c) / i;
            one = double.IsNaN(one) ? 0 : one;
            a += one;

            var two = -ApproxSin((float)(c - p * i)) / i;
            two = double.IsNaN(two) ? 0 : two;
            b += two;
        }

        return 2 * Amplitude / Math.PI * (a - b);
    }

    double ApproxSin(float t)
    {
        float j = t * 0.15915f;
        j = j - (int)j;
        return 20.785 * j * (j - 0.5) * (j - 1.0f);
    }
}

