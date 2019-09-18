// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace FoxEngine
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class Engine
    {
        public Thread _engineThread;

        private Sprite _defaultDrawTarget;

        private Sprite _fontSprite;

        private ulong _frameCount = 0;

        private int[] _fpsAvgBuffer = new int[8];

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

        public abstract void Update(double frameTime);

        public abstract void Destroy();

        public void Start()
        {
            if (Platform == null)
            {
                throw new ApplicationException("The platform initialization failed");
            }

            Platform.Initialize();

            _engineThread.Start();

            Platform.Run();
        }

        public void Stop()
        {
            IsRunning = false;

            Destroy();

            _engineThread.Join();

            Platform.Dispose();
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

        public void DrawSpritePartial(int x, int y, Sprite sprite, int ox, int oy, int width, int height, int scale)
        {
            if (sprite == null)
            {
                return;
            }

            if (scale > 1)
            {
                for (var i = 0; i < width; i++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        for (var @is = 0; @is < scale; @is++)
                        {
                            for (int js = 0; js < scale; js++)
                            {
                                Draw(x + (i * scale) + @is, y + (j * scale) + js, sprite.GetPixel(i + ox, j + oy));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        Draw(x + i, y + j, sprite.GetPixel(i + ox, j + oy));
                    }
                }
            }
        }

        public void DrawSprite(int x, int y, Sprite sprite, int scale)
        {
            if (sprite == null)
            {
                return;
            }

            if (scale > 1)
            {
                for (int i = 0; i < sprite.Width; i++)
                {
                    for (int j = 0; j < sprite.Height; j++)
                    {
                        for (int @is = 0; @is < scale; @is++)
                        {
                            for (int js = 0; js < scale; js++)
                            {
                                Draw(x + (i * scale) + @is, y + (j * scale) + js, sprite.GetPixel(i, j));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < sprite.Width; i++)
                {
                    for (int j = 0; j < sprite.Height; j++)
                    {
                        Draw(x + i, y + j, sprite.GetPixel(i, j));
                    }
                }
            }
        }

        public void DrawCircleFilled(int x, int y, int r, Pixel pixel)
        {
            // Taken from wikipedia
            var x0 = 0;
            var y0 = r;
            var d = 3 - 2 * r;

            if (r == 0)
            {
                return;
            }

            void drawline(int sx, int ex, int ny) { for (int i = sx; i <= ex; i++) { Draw(i, ny, pixel); } }

            while (y0 >= x0)
            {
                // Modified to draw scan-lines instead of edges
                drawline(x - x0, x + x0, y - y0);
                drawline(x - y0, x + y0, y - x0);
                drawline(x - x0, x + x0, y + y0);
                drawline(x - y0, x + y0, y + x0);

                if (d < 0)
                {
                    d += 4 * x0++ + 6;
                }
                else
                {
                    d += 4 * (x0++ - y0--) + 10;
                }
            }
        }

        public void DrawCircle(int x, int y, int r, Pixel pixel, ushort mask = 0xFF)
        {
            var x0 = 0;
            var y0 = r;
            var d = 3 - 2 * r;

            if (r == 0)
            {
                return;
            }

            while (y0 >= x0) // only formulate 1/8 of circle
            {
                if (Convert.ToBoolean(mask & 0x01))
                {
                    Draw(x + x0, y - y0, pixel);
                }

                if (Convert.ToBoolean(mask & 0x02))
                {
                    Draw(x + y0, y - x0, pixel);
                }

                if (Convert.ToBoolean(mask & 0x04))
                {
                    Draw(x + y0, y + x0, pixel);
                }

                if (Convert.ToBoolean(mask & 0x08))
                {
                    Draw(x + x0, y + y0, pixel);
                }

                if (Convert.ToBoolean(mask & 0x10))
                {
                    Draw(x - x0, y + y0, pixel);
                }

                if (Convert.ToBoolean(mask & 0x20))
                {
                    Draw(x - y0, y + x0, pixel);
                }

                if (Convert.ToBoolean(mask & 0x40))
                {
                    Draw(x - y0, y - x0, pixel);
                }

                if (Convert.ToBoolean(mask & 0x80))
                {
                    Draw(x - x0, y - y0, pixel);
                }

                if (d < 0)
                {
                    d += 4 * x0++ + 6;
                }
                else
                {
                    d += 4 * (x0++ - y0--) + 10;
                }
            }
        }

        public void DrawTriangleFilled(int x1, int y1, int x2, int y2, int x3, int y3, Pixel p)
        {
            void swap(object a, object b) { (a, b) = (b, a); }
            void drawline(int sx, int ex, int ny) { for (int i = sx; i <= ex; i++) { Draw(i, ny, p); } }

            var changed1 = false;
            var changed2 = false;

            int t1x, t2x, y, minx, maxx, t1xp, t2xp;
            int signx1, signx2, dx1, dy1, dx2, dy2;
            int e1, e2;
        
            // Sort vertices
            if (y1 > y2)
            {
                swap(y1, y2);
                swap(x1, x2);
            }

            if (y1 > y3)
            {
                swap(y1, y3);
                swap(x1, x3);
            }

            if (y2 > y3)
            {
                swap(y2, y3);
                swap(x2, x3);
            }

            // Starting points
            t1x = t2x = x1;
            y = y1;

            dx1 = x2 - x1;

            if (dx1 < 0)
            {
                dx1 = -dx1;
                signx1 = -1;
            }
            else
            {
                signx1 = 1;
            }

            dy1 = y2 - y1;
            dx2 = x3 - x1;

            if (dx2 < 0)
            {
                dx2 = -dx2;
                signx2 = -1;
            }
            else
            {
                signx2 = 1;
            }

            dy2 = y3 - y1;

            // swap values
            if (dy1 > dx1)
            {
                swap(dx1, dy1);
                changed1 = true;
            }

            // swap values
            if (dy2 > dx2)
            {
                swap(dy2, dx2);
                changed2 = true;
            }

            e2 = dx2 >> 1;

            // Flat top, just process the second half
            if (y1 == y2)
            {
                goto next;
            }

            e1 = dx1 >> 1;

            for (int i = 0; i < dx1;)
            {
                t1xp = 0;
                t2xp = 0;

                if (t1x < t2x)
                {
                    minx = t1x;
                    maxx = t2x;
                }
                else
                {
                    minx = t2x; maxx = t1x;
                }

                // process first line until y value is about to change
                while (i < dx1)
                {
                    i++;
                    e1 += dy1;

                    while (e1 >= dx1)
                    {
                        e1 -= dx1;

                        if (changed1)
                        {
                            t1xp = signx1; //t1x += signx1;
                        }
                        else
                        {
                            goto next1;
                        }
                    }

                    if (changed1)
                    {
                        break;
                    }
                    else
                    {
                        t1x += signx1;
                    }
                }

            // Move line
            next1:

                // process second line until y value is about to change
                while (true)
                {
                    e2 += dy2;

                    while (e2 >= dx2)
                    {
                        e2 -= dx2;

                        if (changed2)
                        {
                            t2xp = signx2;//t2x += signx2;
                        }
                        else
                        {
                            goto next2;
                        }
                    }

                    if (changed2)
                    {
                        break;
                    }
                    else
                    {
                        t2x += signx2;
                    }
                }

            next2:

                if (minx > t1x)
                {
                    minx = t1x;
                }

                if (minx > t2x)
                {
                    minx = t2x;
                }

                if (maxx < t1x)
                {
                    maxx = t1x;
                }

                if (maxx < t2x)
                {
                    maxx = t2x;
                }

                // Draw line from min to max points found on the y
                drawline(minx, maxx, y);

                // Now increase y
                if (!changed1)
                {
                    t1x += signx1;
                }

                t1x += t1xp;

                if (!changed2)
                {
                    t2x += signx2;
                }

                t2x += t2xp;
                y += 1;

                if (y == y2)
                {
                    break;
                }
            }

        // Second half
        next:

            dx1 = x3 - x2;

            if (dx1 < 0)
            {
                dx1 = -dx1;
                signx1 = -1;
            }
            else
            {
                signx1 = 1;
            }

            dy1 = y3 - y2;
            t1x = x2;

            // swap values
            if (dy1 > dx1)
            {
                swap(dy1, dx1);
                changed1 = true;
            }
            else
            {
                changed1 = false;
            }

            e1 = dx1 >> 1;

            for (int i = 0; i <= dx1; i++)
            {
                t1xp = 0;
                t2xp = 0;

                if (t1x < t2x)
                {
                    minx = t1x;
                    maxx = t2x;
                }
                else
                {
                    minx = t2x;
                    maxx = t1x;
                }
        
                // process first line until y value is about to change
                while (i < dx1)
                {
                    e1 += dy1;

                    while (e1 >= dx1)
                    {
                        e1 -= dx1;

                        if (changed1)
                        {
                            t1xp = signx1;
                            break;
                        }
                        else
                        {
                            goto next3;
                        }
                    }

                    if (changed1)
                    {
                        break;
                    }
                    else
                    {
                        t1x += signx1;
                    }

                    if (i < dx1)
                    {
                        i++;
                    }
                }

            // process second line until y value is about to change
            next3:

                while (t2x != x3)
                {
                    e2 += dy2;

                    while (e2 >= dx2)
                    {
                        e2 -= dx2;

                        if (changed2)
                        {
                            t2xp = signx2;
                        }
                        else
                        {
                            goto next4;
                        }
                    }

                    if (changed2)
                    {
                        break;
                    }
                    else
                    {
                        t2x += signx2;
                    }
                }

            next4:

                if (minx > t1x)
                {
                    minx = t1x;
                }

                if (minx > t2x)
                {
                    minx = t2x;
                }

                if (maxx < t1x)
                {
                    maxx = t1x;
                }

                if (maxx < t2x)
                {
                    maxx = t2x;
                }

                drawline(minx, maxx, y);

                if (!changed1)
                {
                    t1x += signx1;
                }

                t1x += t1xp;

                if (!changed2)
                {
                    t2x += signx2;
                }

                t2x += t2xp;
                y += 1;

                if (y > y3)
                {
                    return;
                }
            }
        }

        public void DrawTriangle(int x1, int y1, int x2, int y2, int x3, int y3, Pixel pixel)
        {
            DrawLine(x1, y1, x2, y2, pixel);
            DrawLine(x2, y2, x3, y3, pixel);
            DrawLine(x3, y3, x1, y1, pixel);
        }

        public void DrawRectFilled(int x, int y, int width, int height, Pixel pixel)
        {
            var x2 = x + width;
            var y2 = y + height;

            if (x < 0)
            {
                x = 0;
            }
            else if (x >= Size.Width)
            {
                x = Size.Width;
            }

            if (y < 0)
            {
                y = 0;
            }
            else if (y >= Size.Height)
            {
                y = Size.Height;
            }

            if (x2 < 0)
            {
                x2 = 0;
            }
            else if (x2 >= Size.Width)
            {
                x2 = Size.Width;
            }

            if (y2 < 0)
            {
                y2 = 0;
            }
            else if (y2 >= Size.Height)
            {
                y2 = Size.Height;
            }

            for (int i = x; i < x2; i++)
            {
                for (int j = y; j < y2; j++)
                {
                    Draw(i, j, pixel);
                }
            }
        }

        public void DrawRect(int x, int y, int width, int height, Pixel pixel)
        {
            DrawLine(x, y, x + width, y, pixel);
            DrawLine(x + width, y, x + width, y + height, pixel);
            DrawLine(x + width, y + height, x, y + height, pixel);
            DrawLine(x, y + height, x, y, pixel);
        }

        public void DrawLine(int x1, int y1, int x2, int y2, Pixel pixel)
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

        public void DrawString(int x, int y, string text, Pixel pixel, int scale = 1)
        {
            var sx = 0;
            var sy = 0;
            var pm = PixelMode;

            if (pixel.A != 255)
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
                                            Draw(x + sx + (i * scale) + @is, y + sy + (j * scale) + js, pixel);
                                        }
                                    }
                                }
                                else
                                {
                                    Draw(x + sx + i, y + sy + j, pixel);
                                }
                            }
                        }
                    }

                    sx += 8 * scale;
                }
            }

            PixelMode = pm;
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
                var alpha = pixel.A / 255.0f * 1;
                var cof = 1.0f - alpha;
                var red = alpha * pixel.R + cof * origPixel.R;
                var green = alpha * pixel.G + cof * origPixel.G;
                var blue = alpha * pixel.B + cof * origPixel.B;

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

                Update(elapsedTime.TotalMilliseconds);

                Platform.Draw();

                _fpsAvgBuffer[_frameCount % 8] = (int)Math.Floor(1.0f / elapsedTime.TotalMilliseconds * 1000);

                Platform.SetWindowTitle(Convert.ToInt32(_fpsAvgBuffer.Average()) + " FPS");

                _frameCount++;
            }
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
