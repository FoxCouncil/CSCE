namespace FoxEngine;

using Silk.NET.OpenAL;
using System;
using System.Collections.Generic;
using System.Threading;

internal unsafe class Sound
{
    const uint AL_PLAYING = 0x1012;

    const uint DEFAULT_BLOCKS = 8;

    const uint DEFAULT_SAMPLES_PER_BLOCK = 512;
    
    private readonly uint _source;

    private Thread _thread;

    private bool _threadRunning;

    private Device* _device;

    private Context* _context;

    private uint[] _buffers;

    private uint[] _memory;

    private float _time;

    public uint SampleRate { get; private set; }

    public AudioChannel Channels { get; private set; }

    public uint BlockCount { get; private set; }

    public uint BlockSamples { get; private set; }

    public Queue<uint> AvailableBuffers { get; private set; }

    public Sound()
    {
    }

    public unsafe bool Start(uint sampleRate, AudioChannel channels, uint blocks = DEFAULT_BLOCKS, uint blockSamples = DEFAULT_SAMPLES_PER_BLOCK)
    {
        _thread = new Thread(new ThreadStart(Run))
        {
            IsBackground = false,
            Priority = ThreadPriority.Normal,
            Name = "FoxEngine.Audio"
        };

        SampleRate = sampleRate;
        Channels = channels;
        BlockCount = blocks;
        BlockSamples = blockSamples;

        AvailableBuffers = new Queue<uint>();

        var alc = ALContext.GetApi();

        _device = alc.OpenDevice("");

        if (_device == null)
        {
            return false;
        }

        _context = alc.CreateContext(_device, null);

        alc.MakeContextCurrent(_context);

        var al = AL.GetApi();

        al.GetError();

        _buffers = new uint[BlockCount];

        fixed (uint* rawBuffer = _buffers) al.GenBuffers((int)BlockCount, rawBuffer);
        fixed (uint* rawSource = &_source) al.GenSources(1, rawSource);

        for (uint i = 0; i < BlockCount; i++)
        {
            AvailableBuffers.Enqueue(_buffers[i]);
        }

        _memory = new uint[BlockSamples];

        _threadRunning = true;

        _thread.Start();

        return true;
    }

    public void Destroy()
    {
        _threadRunning = false;

        _thread.Join();

        var al = AL.GetApi();

        fixed (uint* rawBuffer = _buffers) al.DeleteBuffers((int)BlockCount, rawBuffer);
        fixed (uint* rawSource = &_source) al.DeleteSources(1, rawSource);

        _buffers = null;

        var alc = ALContext.GetApi();

        alc.MakeContextCurrent(null);
        alc.DestroyContext(_context);
        alc.CloseDevice(_device);
    }

    void Run()
    {
        // Booting up thread
        _time = 0.0f;

        var timeStep = 1.0f / SampleRate;
        var floatMax = (float)short.MaxValue;

        var processed = new List<uint>();

        var al = AL.GetApi();
        var alc = ALContext.GetApi();

        while (_threadRunning)
        {
            al.GetSourceProperty(_source, GetSourceInteger.SourceState, out var state);
            al.GetSourceProperty(_source, GetSourceInteger.BuffersProcessed, out var bufferProcessed);

            if (bufferProcessed == 0)
            {
                processed.Clear();
            }

            // Add processed buffers to the queue
            processed.Resize(bufferProcessed);

            var processedArray = processed.ToArray();

            fixed (uint* rawProcessedBuffer = processedArray) al.SourceUnqueueBuffers(_source, bufferProcessed, rawProcessedBuffer);

            foreach (uint buffer in processedArray)
            {
                AvailableBuffers.Enqueue(buffer);
            }

            if (AvailableBuffers.Count == 0)
            {
                continue;
            }

            for (uint n = 0; n < BlockSamples; n += (uint)Channels)
            {
                // Consumer Process
                for (uint c = 0; c < (uint)Channels; c++)
                {
                    short newSample = (short)(Clip(GetMixerOutput(c, _time, timeStep), 1.0f) * floatMax);
                    _memory[n + c] = (uint)newSample;
                }

                _time += timeStep;
            }

            var bufferFormat = Channels == AudioChannel.Mono ? BufferFormat.Mono16 : BufferFormat.Stereo16;

            var bufferId = AvailableBuffers.Dequeue();

            al.BufferData(bufferId, bufferFormat, _memory, (int)SampleRate);

            al.SourceQueueBuffers(_source, new uint[] { bufferId });

            if (state != AL_PLAYING)
            {
                al.SourcePlay(_source);
            }
        }
    }

    static double GetMixerOutput(uint channel, double time, double timeStep)
    {
        double mixerSample = 0;

        // could play "samples" here...

        // User Generated Audio Data
        mixerSample += Engine.Instance.GenerateSample(channel, time, timeStep);

        return mixerSample;
    }    
    
    static double Clip(double sample, double max)
    {
        if (sample >= 0)
        {
            return Math.Min(sample, max);
        }
        else
        {
            return Math.Max(sample, -max);
        }
    }
}

