// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace FoxEngine
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class Engine
    {
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        private Thread _engineThread;

        private Sprite _defaultDrawTarget;

        private Sprite _fontSprite;

        public IPlatform Platform { get; }

        public string Name { get; } = "Fox Engine";

        public Size Size { get; private set; } = new Size(640, 480);

        public bool IsRunning { get; private set; }

        public bool IsFocused { get; private set; } = true;

        public PixelMode PixelMode { get; set; } = PixelMode.NORMAL;

        public Sprite DrawTarget { get; private set; }

        public Engine(string name, int width, int height)
        {
            GenerateFontSprite();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Platform = new WindowsPlatform(this);
            }
            else
            {
                throw new NotImplementedException($"{RuntimeInformation.OSDescription} platform not yet supported.");
            }

            Name = name;

            Resize(width, height);

            _defaultDrawTarget = new Sprite(width, height);

            SetDrawTarget();

            _engineThread = new Thread(Run)
            {
                Name = "FoxEngine.Run"
            };
        }

        public abstract void Create();

        public abstract void Update();

        public abstract void Destroy();

        public void Start()
        {
            if (Platform == null)
            {
                throw new ApplicationException("The platform initialization failed");
            }

            Platform.Initialize();

            _engineThread.Start();

            Platform.SecondaryInitialize();

            _autoResetEvent.WaitOne();
        }

        public void Resize(int width, int height)
        {
            if (Size.Width == width && Size.Height == height)
            {
                // noop
                return;
            }

            Size = new Size(width, height);
        }

        public void DrawString(int x, int y, string text, Pixel color, int scale = 1)
        {
            var sx = 0;
            var sy = 0;
            var pm = PixelMode;

            if (color.A != 255)
            {
                PixelMode = PixelMode.ALPHA;
            }
            else
            {
                PixelMode = PixelMode.MASK;
            }

            foreach (var c in text)
            {
                if (c == '\n')
                {
                    sx = 0;
                    sy += 8 * scale;
                }
                else
                {
                    var ox = (c - 32) % 16;
                    var oy = (c - 32) / 16;

                    for (var i = 0; i < 8; i++)
                    {
                        for (var j = 0; j < 8; j++)
                        {
                            if (_fontSprite.GetPixel(i + ox * 8, j + oy * 8).R > 0)
                            {
                                if (scale > 1)
                                {
                                    for (var @is = 0; @is < scale; @is++)
                                    {
                                        for (var js = 0; js < scale; js++)
                                        {
                                            Draw(x + sx + (i * scale) + @is, y + sy + (j * scale) + js, color);
                                        }
                                    }
                                }
                                else
                                {
                                    Draw(x + sx + i, y + sy + j, color);
                                }
                            }
                        }
                    }

                    sx += 8 * scale;
                }
            }

            PixelMode = pm;
        }

        public void DrawLine(int x1, int y1, int x2, int y2, Pixel pixel = new Pixel())
        {
            int x, y, dx1, dy1, px, py, xe, ye, i;

            var dx = x2 - x1;

            if (dx == 0)
            {
                if (y2 > y1)
                {
                    (y2, y1) = (y1, y2);
                }

                for (y = y1; y <= y2; y++)
                {
                    Draw(x1, y, pixel);
                }

                return;
            }

            var dy = y2 - y1;

            if (dy == 0)
            {
                if (x2 < x1)
                {
                    (x2, x1) = (x1, x2);
                }

                for (x = x1; x <= x2; x++)
                {
                    Draw(x, y1, pixel);
                }

                return;
            }

            dx1 = Math.Abs(dx);
            dy1 = Math.Abs(dy);

            px = 2 * dy1 - dx1;
            py = 2 * dx1 - dy1;

            if (dy1 <= dx1)
            {
                if (dx >= 0)
                {
                    x = x1;
                    y = y1;
                    xe = x2;
                }
                else
                {
                    x = x2;
                    y = y2;
                    xe = x1;
                }

                Draw(x, y, pixel);

                for (i = 0; x < xe; i++)
                {
                    x += 1;

                    if (px < 0)
                    {
                        px = px + 2 * dy1;
                    }
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                        {
                            y += 1;
                        }
                        else
                        {
                            y -= 1;
                        }

                        px += 2 * (dy1 - dx1);
                    }

                    Draw(x, y, pixel);
                }
            }
            else
            {
                if (dy >= 0)
                {
                    x = x1;
                    y = y1;
                    ye = y2;
                }
                else
                {
                    x = x2;
                    y = y2;
                    ye = y1;
                }

                Draw(x, y, pixel);

                for (i = 0; y < ye; i++)
                {
                    y += 1;

                    if (py <= 0)
                    {
                        py = py + 2 * dx1;
                    }
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                        {
                            x += 1;
                        }
                        else
                        {
                            x -= 1;
                        }

                        py += 2 * (dx1 - dy1);
                    }

                    Draw(x, y, pixel);
                }
            }
        }

        public void Draw(int x, int y, Pixel pixel)
        {
            if (DrawTarget == null)
            {
                return;
            }

            if (PixelMode == PixelMode.NORMAL)
            {
                DrawTarget.SetPixel(x, y, pixel);
            }
            else if (PixelMode == PixelMode.MASK)
            {
                if (pixel.A == 255)
                {
                    DrawTarget.SetPixel(x, y, pixel);
                }
            }
            else if (PixelMode == PixelMode.ALPHA)
            {
                var origPixel = DrawTarget.GetPixel(x, y);
                var alpha = (float)(pixel.A / 255.0f) * 1;
                var cof = 1.0f - alpha;
                var red = alpha * (float)pixel.R + cof * (float)origPixel.R;
                var green = alpha * (float)pixel.G + cof * (float)origPixel.G;
                var blue = alpha * (float)pixel.B + cof * (float)origPixel.B;

                DrawTarget.SetPixel(x, y, new Pixel(Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue)));
            }
        }

        public void SetDrawTarget(Sprite target = null)
        {
            if (target == null)
            {
                DrawTarget = _defaultDrawTarget;
            }
            else
            {
                DrawTarget = target;
            }
        }

        private void Run(object obj)
        {
            Platform.CreateGlContext();

            IsRunning = true;

            Create();

            var stopWatch = Stopwatch.StartNew();

            while (IsRunning)
            {
                stopWatch.Stop();

                var elapsedTime = stopWatch.Elapsed;

                stopWatch = Stopwatch.StartNew();

                Update();

                Platform.Draw();

                Platform.SetWindowTitle(Math.Floor(1.0f / elapsedTime.TotalMilliseconds * 1000).ToString() + " FPS");
            }

            Destroy();
        }

        private void GenerateFontSprite()
        {
            var fontData = "?Q`0001oOch0o01o@F40o0<AGD4090LAGD<090@A7ch0?00O7Q`0600>00000000O000000nOT0063Qo4d8>?7a14Gno94AA4gno94AaOT0>o3`oO400o7QN00000400Of80001oOg<7O7moBGT7O7lABET024@aBEd714AiOdl717a_=TH013Q>00000000720D000V?V5oB3Q_HdUoE7a9@DdDE4A9@DmoE4A;Hg]oM4Aj8S4D84@`00000000OaPT1000Oa`^13P1@AI[?g`1@A=[OdAoHgljA4Ao?WlBA7l1710007l100000000ObM6000oOfMV?3QoBDD`O7a0BDDH@5A0BDD<@5A0BGeVO5ao@CQR?5Po00000000Oc``000?Ogij70PO2D]??0Ph2DUM@7i`2DTg@7lh2GUj?0TO0C1870T?0000000070<4001o?P<7?1QoHg43O;`h@GT0@:@LB@d0>:@hN@L0@?aoN@<0O7ao0000?000OcH0001SOglLA7mg24TnK7ln24US>0PL24U140PnOgl0>7QgOcH0K71S0000A00000H00000@Dm1S007@DUSg00?OdTnH7YhOfTL<7Yh@Cl0700?@Ah0300700000000<008001QL00ZA41a@6HnI<1i@FHLM81M@@0LG81?O`0nC?Y7?`0ZA7Y300080000O`082000Oh0827mo6>Hn?Wmo?6HnMb11MP08@C11H`08@FP0@@0004@00000000000P00001Oab00003OcKP0006@6=PMgl<@440MglH@000000`@000001P00000000Ob@8@@00Ob@8@Ga13R@8Mga172@8?PAo3R@827QoOb@820@0O`0007`0000007P0O`000P08Od400g`<3V=P0G`673IP0`@3>1`00P@6O`P00g`<O`000GP800000000?P9PL020O`<`N3R0@E4HC7b0@ET<ATB0@@l6C4B0O`H3N7b0?P01L3R000000020";

            _fontSprite = new Sprite(128, 48);

            var px = 0;
            var py = 0;

            for (int b = 0; b < 1024; b += 4)
            {
                var sym1 = (uint)fontData[b + 0] - 48;
                var sym2 = (uint)fontData[b + 1] - 48;
                var sym3 = (uint)fontData[b + 2] - 48;
                var sym4 = (uint)fontData[b + 3] - 48;
                var r = sym1 << 18 | sym2 << 12 | sym3 << 6 | sym4;

                for (int i = 0; i < 24; i++)
                {
                    _fontSprite.SetPixel(px, py, Convert.ToBoolean(r & (1 << i)) ? Pixel.White : Pixel.Blank);

                    if (++py == 48)
                    {
                        px++;
                        py = 0;
                    }
                }
            }
        }
    }
}
