using System;
using System.Runtime.InteropServices;

namespace FoxEngine
{
    public class Engine
    {
        public IPlatform Platform { get; }

        public Engine()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Platform = new WindowsPlatform();
            }
        }
    }
}
