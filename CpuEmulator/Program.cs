// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace CpuEmulator
{
    using System.Diagnostics;

    public static class Program
    {
        static void Main(string[] args)
        {
            var emulator = new Emulator();

            emulator.Start();

            Debugger.Break();
        }
    }
}
