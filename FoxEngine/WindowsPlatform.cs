// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace FoxEngine
{
    using System;
    using System.Runtime.InteropServices;

    internal class WindowsPlatform : IPlatform
    {
        private static class Interop
        {
            private const string DllUser32 = "user32.dll";

            [DllImport(DllUser32, CharSet=CharSet.Auto)]
            public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
        }

        public void MessageBox(string title, string message)
        {
            Interop.MessageBox(IntPtr.Zero, message, title, 0);
        }
    }
}