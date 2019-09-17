using System;
using System.Collections.Generic;
using System.Text;

namespace CpuEmulator
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var emulator = new Emulator();

            emulator.Start();
        }
    }
}
