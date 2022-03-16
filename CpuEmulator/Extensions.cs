// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

using System.IO;
using System.Runtime.InteropServices;

namespace CpuEmulator;

public static class Extensions
{
    public static T ToStruct<T>(this BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();

        return theStructure;
    }

    public static int Address(this int number, int size)
    {
        return number * size;
    }

    public static byte Address(this byte number, int size)
    {
        return (byte)(number * size);
    }
}
