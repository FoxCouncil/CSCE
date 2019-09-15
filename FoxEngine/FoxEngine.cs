// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace FoxEngine
{
    using System.Runtime.InteropServices;

    public class Engine
    {
        public IPlatform Platform { get; }

        public Engine()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Platform = new WindowsPlatform();
            }

            Platform.MessageBox("FoxEngine", "Hello World");
        }
    }
}
