namespace CpuEmulator.NES.Apu;

internal delegate void ActionRef<T>(ref T item);

internal struct Sequencer
{
    public uint Sequence { get; set; }

    public ushort Timer { get; set; }

    public ushort Reload { get; set; }

    public byte Output { get; set; }

    internal byte Clock(bool enabled, ActionRef<uint> func)
    {
        if (enabled)
        {
            Timer--;

            if (Timer == 0xFFFF)
            {
                Timer = (ushort)(Reload + 1);
                
                var seq = Sequence;
                func(ref seq);
                Sequence = seq;

                Output = (byte)(Sequence & 0x00000001);
            }
        }

        return Output;
    }
}
