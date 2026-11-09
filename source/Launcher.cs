using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace KeyPeek
{
    static class Native
    {
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, int flags);
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
        [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")] public static extern int RegisterWindowMessage(string name);

        [DllImport("dwmapi.dll")] public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
        [DllImport("dwmapi.dll")] public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
        [DllImport("user32.dll")] public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WCAData data);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int index);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int index, IntPtr value);
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")] public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")] public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr hObj);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBSection(IntPtr hDC, ref BITMAPINFO info, int usage, out IntPtr bits, IntPtr section, int offset);
        [DllImport("user32.dll")]
        public static extern bool UpdateLayeredWindow(IntPtr hWnd, IntPtr hdcDst, ref POINT ptDst, ref SIZE size,
            IntPtr hdcSrc, ref POINT ptSrc, int crKey, ref BLENDFUNCTION blend, int flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X, Y; }
        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE { public int CX, CY; }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public int biSize, biWidth, biHeight;
            public short biPlanes, biBitCount;
            public int biCompression, biSizeImage, biXPelsPerMeter, biYPelsPerMeter, biClrUsed, biClrImportant;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD { public int r, g, b, a; }
        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER Header;
            public RGBQUAD Colors;
        }

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int ULW_ALPHA = 2;
        public const int AC_SRC_OVER = 0;
        public const int AC_SRC_ALPHA = 1;
        public const int DIB_RGB_COLORS = 0;
        public const int WM_SIZE = 0x0005;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_FRAMECHANGED = 0x0020;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OSVERSIONINFO
        {
            public int Size;
            public int Major;
            public int Minor;
            public int Build;
            public int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Csd;
        }
        [DllImport("ntdll.dll", CharSet = CharSet.Unicode)]
        public static extern int RtlGetVersion(ref OSVERSIONINFO info);

        // 不能信 Environment.OSVersion：没 manifest 的 exe 会被报成 6.2
        public static int RealBuild()
        {
            try
            {
                OSVERSIONINFO v = new OSVERSIONINFO();
                v.Size = Marshal.SizeOf(typeof(OSVERSIONINFO));
                if (RtlGetVersion(ref v) == 0 && v.Build > 0) return v.Build;
            }
            catch (Exception) { }
            return Environment.OSVersion.Version.Build;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WCAData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS { public int Left; public int Right; public int Top; public int Bottom; }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int WM_HOTKEY = 0x0312;
        public const int HTCAPTION = 2;
        public const int HTBOTTOMRIGHT = 17;
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        public const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        public const int DWMSBT_NONE = 1;
        public const int DWMSBT_TRANSIENTWINDOW = 3;
        public const int DWMWCP_ROUND = 2;
    }

    static class Program
    {
        public static int WM_KEYPEEK_SHOW;

        [STAThread]
        static void Main()
        {
            try { Native.SetProcessDPIAware(); }
            catch (Exception) { }

            WM_KEYPEEK_SHOW = Native.RegisterWindowMessage("KeyPeek_Activate_384524");

            bool createdNew = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "KeyPeek_SingleInstance_384524", out createdNew);
            if (!createdNew)
            {
                Native.PostMessage(new IntPtr(0xFFFF), WM_KEYPEEK_SHOW, IntPtr.Zero, IntPtr.Zero);
                return;
            }

            try { Application.SetCompatibleTextRenderingDefault(false); }
            catch (Exception) { }
            Application.EnableVisualStyles();

            SplashForm splash = new SplashForm();
            splash.Show();
            splash.Refresh();

            MainForm main = new MainForm(splash);
            Application.Run(main);

            GC.KeepAlive(mutex);
        }
    }

    // ================= 启动画面 =================
    public class SplashForm : Form
    {
        private float sc = 1f;
        private Timer tick;
        private DateTime started = DateTime.Now;

        // 分层窗口：自己合成 RGBA 位图，圆角和描边都是抗锯齿的
        private const int W = 320;
        private const int H = 182;
        private const int PAD = 12;      // 留给投影的边距
        private const int CARDW = 296;
        private const int CARDH = 158;
        private byte fade = 255;
        private IntPtr memDC, hDib, bitsPtr;
        private bool dibReady;
        private bool layeredOk;
        private bool shown;

        public SplashForm()
        {
            float dpiX;
            using (Graphics g = CreateGraphics()) { dpiX = g.DpiX; }
            sc = Math.Max(1f, dpiX / 96f);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            ClientSize = new Size(S(W), S(H));
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(
                wa.Left + (wa.Width - Width) / 2,
                wa.Top + (int)((wa.Height - Height) * 0.42));

            // 先建句柄，再挂分层样式
            IntPtr h = Handle;
            try
            {
                long ex = (long)Native.GetWindowLongPtr(h, Native.GWL_EXSTYLE);
                Native.SetWindowLongPtr(h, Native.GWL_EXSTYLE, new IntPtr(ex | Native.WS_EX_LAYERED));
                layeredOk = true;
            }
            catch (Exception) { layeredOk = false; }
            PrepareDib();

            tick = new Timer();
            tick.Interval = 16;
            tick.Tick += new EventHandler(Tick);
            tick.Start();
            if (layeredOk && Render()) { }
            else FallbackPlain();
            SplashLog("layered=" + layeredOk + " dib=" + dibReady + " dpi=" + sc.ToString("0.##") + " size=" + ClientSize.Width + "x" + ClientSize.Height);
        }

        private static void SplashLog(string msg)
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userdata");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string f = Path.Combine(dir, "splash.log");
                if (File.Exists(f) && new FileInfo(f).Length > 4096) File.Delete(f);
                File.AppendAllText(f, DateTime.Now.ToString("HH:mm:ss.fff") + "  " + msg + Environment.NewLine);
            }
            catch (Exception) { }
        }

        // 分层窗口失手时退回普通绘制，至少不会留一块空白
        private void FallbackPlain()
        {
            layeredOk = false;
            try
            {
                IntPtr h = Handle;
                long ex = (long)Native.GetWindowLongPtr(h, Native.GWL_EXSTYLE);
                Native.SetWindowLongPtr(h, Native.GWL_EXSTYLE, new IntPtr(ex & ~Native.WS_EX_LAYERED));
            }
            catch (Exception) { }
            BackColor = Color.FromArgb(247, 247, 249);
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int r = S(14);
            GraphicsPath p = new GraphicsPath();
            p.AddArc(rect.Left, rect.Top, r * 2, r * 2, 180, 90);
            p.AddArc(rect.Right - r * 2, rect.Top, r * 2, r * 2, 270, 90);
            p.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
            p.AddArc(rect.Left, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            p.CloseFigure();
            Region = new Region(p);
            Paint += new PaintEventHandler(delegate(object s, PaintEventArgs e)
            {
                e.Graphics.Clear(Color.FromArgb(247, 247, 249));
                DrawSplash(e.Graphics, ClientSize.Width, ClientSize.Height);
            });
            Invalidate();
        }

        private int S(int v) { return (int)Math.Round(v * sc); }

        // 分层窗口不参与 WM_PAINT 绘制，内容全靠 UpdateLayeredWindow
        protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); }
        protected override void OnPaintBackground(PaintEventArgs e) { }

        // 一切动画从真正可见的那一刻起算，不会"还没露脸就播完了"
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            started = DateTime.Now;
            fade = 255;
            shown = true;
            if (layeredOk) Render();
            SplashLog("shown");
        }

        private void Tick(object sender, EventArgs e)
        {
            if (!shown) return;
            if (layeredOk) Render();
            else Invalidate();
        }

        public void Finish()
        {
            tick.Stop();
            FadeOut();
        }

        // 淡出再关，避免主窗出现时启动画面"啪"一下消失
        private void FadeOut()
        {
            Timer ft = new Timer();
            ft.Interval = 16;
            ft.Tick += new EventHandler(delegate(object s, EventArgs e)
            {
                if (layeredOk)
                {
                    if (fade <= 24) { ft.Stop(); ft.Dispose(); Close(); return; }
                    fade = (byte)Math.Max(0, fade - 46);
                    Render();
                    return;
                }
                double op = Opacity - 0.22;
                if (op <= 0.06) { ft.Stop(); ft.Dispose(); Close(); return; }
                Opacity = op;
            });
            ft.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (hDib != IntPtr.Zero) Native.DeleteObject(hDib);
                if (memDC != IntPtr.Zero) Native.DeleteDC(memDC);
            }
            catch (Exception) { }
            hDib = IntPtr.Zero; memDC = IntPtr.Zero;
            base.OnFormClosed(e);
        }

        private void PrepareDib()
        {
            try
            {
                int w = ClientSize.Width, h = ClientSize.Height;
                IntPtr screen = Native.GetDC(IntPtr.Zero);
                memDC = Native.CreateCompatibleDC(screen);
                Native.BITMAPINFO bi = new Native.BITMAPINFO();
                bi.Header.biSize = Marshal.SizeOf(typeof(Native.BITMAPINFOHEADER));
                bi.Header.biWidth = w;
                bi.Header.biHeight = -h;          // 负数＝自上而下
                bi.Header.biPlanes = 1;
                bi.Header.biBitCount = 32;
                bi.Header.biCompression = 0;      // BI_RGB
                hDib = Native.CreateDIBSection(screen, ref bi, Native.DIB_RGB_COLORS, out bitsPtr, IntPtr.Zero, 0);
                Native.ReleaseDC(IntPtr.Zero, screen);
                dibReady = (memDC != IntPtr.Zero && hDib != IntPtr.Zero && bitsPtr != IntPtr.Zero);
            }
            catch (Exception) { dibReady = false; }
        }

        private bool Render()
        {
            if (!dibReady) return false;
            int w = ClientSize.Width, h = ClientSize.Height;
            try
            {
                using (Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        DrawSplash(g, w, h);
                    }
                    // 转成预乘 alpha 的 BGRA，Otherwise 半透明像素会发黑
                    BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    byte[] src = new byte[bd.Stride * h];
                    byte[] dst = new byte[w * h * 4];
                    try
                    {
                        Marshal.Copy(bd.Scan0, src, 0, src.Length);
                        for (int y = 0; y < h; y++)
                        {
                            int srow = y * bd.Stride, drow = y * w * 4;
                            for (int x = 0; x < w; x++)
                            {
                                int si = srow + x * 4, di = drow + x * 4;
                                int a = src[si + 3];
                                dst[di] = (byte)((src[si] * a) / 255);
                                dst[di + 1] = (byte)((src[si + 1] * a) / 255);
                                dst[di + 2] = (byte)((src[si + 2] * a) / 255);
                                dst[di + 3] = (byte)a;
                            }
                        }
                    }
                    finally { bmp.UnlockBits(bd); }

                    Marshal.Copy(dst, 0, bitsPtr, dst.Length);
                }

                Native.SelectObject(memDC, hDib);
                Native.POINT dstPt = new Native.POINT();
                dstPt.X = Left; dstPt.Y = Top;
                Native.POINT srcPt = new Native.POINT();
                Native.SIZE sz = new Native.SIZE();
                sz.CX = w; sz.CY = h;
                Native.BLENDFUNCTION bf = new Native.BLENDFUNCTION();
                bf.BlendOp = (byte)Native.AC_SRC_OVER;
                bf.BlendFlags = 0;
                // 整窗淡入：避免先看到一块空白浅色卡片，再等图标出现
                float up = shown ? EaseOut((float)((DateTime.Now - started).TotalMilliseconds / 150.0)) : 0f;
                byte upA = (byte)Math.Round(255 * up);
                bf.SourceConstantAlpha = (upA < fade ? upA : fade);
                bf.AlphaFormat = (byte)Native.AC_SRC_ALPHA;
                return Native.UpdateLayeredWindow(Handle, IntPtr.Zero, ref dstPt, ref sz, memDC, ref srcPt, 0, ref bf, Native.ULW_ALPHA);
            }
            catch (Exception) { return false; }
        }

        private static float EaseOut(float p)
        {
            if (p < 0f) p = 0f;
            if (p > 1f) p = 1f;
            float u = 1f - p;
            return 1f - u * u * u;
        }

        private void DrawSplash(Graphics g, int w, int h)
        {
            Rectangle card = new Rectangle(S(PAD), S(PAD), S(CARDW), S(CARDH));
            int radius = S(14);

            // 柔和投影：几层低透明度圆角矩形，越靠外越淡
            for (int k = S(8); k >= 1; k--)
            {
                Rectangle sh = new Rectangle(card.Left - k, card.Top - k + S(2), card.Width + k * 2, card.Height + k * 2);
                using (GraphicsPath p = Round(sh, radius + k))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(3 + (S(8) - k), 0, 0, 0)))
                    g.FillPath(b, p);
            }

            // 卡片：底色 + 1px 描边（都在位图里，边缘有 alpha 过渡）
            using (GraphicsPath p = Round(card, radius))
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(247, 247, 249)))
                    g.FillPath(b, p);
                using (Pen pen = new Pen(Color.FromArgb(22, 0, 0, 0), 1f))
                    g.DrawPath(pen, p);
            }

            double ms = (DateTime.Now - started).TotalMilliseconds;
            int cx = card.Left + card.Width / 2;
            int top = card.Top + S(30);
            int markSize = S(34);

            // 1) 图标：入场 0.86→1，之后轻微呼吸（很慢），暗示还在加载
            float lp = EaseOut((float)(ms / 140.0));
            float pulse = (float)(1.0 + 0.014 * Math.Sin(ms / 1700.0 * Math.PI * 2));
            int baseSize = (int)Math.Round(markSize * (0.86f + 0.14f * lp) * pulse);
            int alpha = (int)Math.Round(255 * lp);
            Rectangle mark = new Rectangle(cx - baseSize / 2, top + (markSize - baseSize) / 2, baseSize, baseSize);
            // 文字锚点固定，不跟随图标呼吸
            int textTop = top + markSize;

            using (GraphicsPath p = Round(mark, S(10)))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, 0, 113, 227)))
                g.FillPath(b, p);

            using (Pen pen = new Pen(Color.FromArgb(alpha, 255, 255, 255), (float)S(2)))
            {
                int x = mark.X + mark.Width / 2;
                int y1 = mark.Y + (int)Math.Round(mark.Height * 0.30);
                int y2 = mark.Y + (int)Math.Round(mark.Height * 0.72);
                int ww = (int)Math.Round(mark.Width * 0.15);
                g.DrawLine(pen, x, y1, x, y2);
                g.DrawLine(pen, x, y1, x - ww, y1 + ww);
                g.DrawLine(pen, x, y1, x + ww, y1 + ww);
            }

            // 2) 文字：90ms 起淡入并上移 6px
            float tp = EaseOut((float)((ms - 45.0) / 170.0));
            int ta = (int)Math.Round(255 * tp);
            int dy = (int)Math.Round(S(6) * (1f - tp));

            string title = "KeyPeek";
            using (Font f = new Font("Segoe UI Semibold", 13.5f, FontStyle.Regular, GraphicsUnit.Point))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(ta, 29, 29, 31)))
            {
                SizeF sz = g.MeasureString(title, f);
                g.DrawString(title, f, b, cx - sz.Width / 2, textTop + S(12) + dy);
            }

            using (Font f = new Font("Microsoft YaHei UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point))
            using (SolidBrush b = new SolidBrush(Color.FromArgb((int)Math.Round(ta * 0.94), 134, 134, 139)))
            {
                string sub = "快捷键，瞄一眼就想起来";
                SizeF sz = g.MeasureString(sub, f);
                g.DrawString(sub, f, b, cx - sz.Width / 2, textTop + S(40) + dy);
            }

            // 3) 波浪线：循环流动，不表示进度，所以不会"走完了还在等"
            float bp = EaseOut((float)((ms - 90.0) / 130.0));
            int barY = card.Bottom - S(26);
            int barW = S(150);
            int barX = cx - barW / 2;
            float amp = S(2);
            float phase = (float)(ms / 2400.0 * Math.PI * 2);
            int wa = (int)Math.Round(255 * bp);

            // 两条错相位的正弦，前面一条实色、后面一条浅色
            DrawWave(g, barX, barY, barW, amp, phase, Color.FromArgb(wa, 0, 113, 227), S(2));
            DrawWave(g, barX, barY, barW, amp * 0.8f, phase + 1.9f, Color.FromArgb((int)(wa * 0.34), 0, 113, 227), S(1));
        }

        private void DrawWave(Graphics g, int x, int y, int w, float amp, float phase, Color c, float penW)
        {
            int n = Math.Max(12, w / 3);
            PointF[] pts = new PointF[n + 1];
            for (int i = 0; i <= n; i++)
            {
                float fx = (float)i / n;
                float yy = y + (float)Math.Sin(fx * Math.PI * 2f * 1.35f + phase) * amp;
                pts[i] = new PointF(x + fx * w, yy);
            }
            // 两端渐隐，避免波浪在边缘硬切
            Color c0 = Color.FromArgb(0, c);
            Color c1 = c;
            using (Pen p = new Pen(c, penW))
            {
                try
                {
                    using (LinearGradientBrush br = new LinearGradientBrush(
                        new Rectangle(x, y - (int)amp - 2, Math.Max(1, w), (int)(amp * 2) + 4),
                        c0, c0, LinearGradientMode.Horizontal))
                    {
                        ColorBlend cb = new ColorBlend(5);
                        cb.Colors = new Color[] { c0, c1, c1, c1, c0 };
                        cb.Positions = new float[] { 0f, 0.10f, 0.5f, 0.90f, 1f };
                        br.InterpolationColors = cb;
                        p.Brush = br;
                        g.DrawLines(p, pts);
                    }
                }
                catch (Exception)
                {
                    g.DrawLines(p, pts);
                }
            }
        }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            if (radius <= 0) { p.AddRectangle(r); return p; }
            int d = radius * 2;
            p.AddArc(r.Left, r.Top, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    // ================= 主窗体（悬浮小窗） =================
    public class MainForm : Form
    {
        private const int HOTKEY_ID = 0x4B50;
        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_NOREPEAT = 0x4000;
        private const int VK_SPACE = 0x20;

        private WebView2 web;
        private NotifyIcon tray;
        private string root, packsDir, statsPath, statePath;
        private SplashForm splash;
        private Timer revealTimer;
        private DateTime launchAt = DateTime.Now;
        private bool webReady, pageReady, reallyQuit, hotkeyOk;
        private bool pin = true;
        private float sc = 1f;
        private bool splashClosed;

        public MainForm(SplashForm sp)
        {
            splash = sp;
            using (Graphics g = CreateGraphics()) { sc = Math.Max(1f, g.DpiX / 96f); }

            root = Application.StartupPath;
            packsDir = Path.Combine(root, "packs");
            statsPath = Path.Combine(root, "stats.json");
            statePath = Path.Combine(root, "windowstate.ini");

            Text = "KeyPeek";
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.FromArgb(242, 242, 244);
            MinimumSize = new Size(S(340), S(420));
            Opacity = 0.0;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch (Exception) { }

            RestoreWindowState();

            web = new WebView2();
            web.Dock = DockStyle.Fill;
            web.DefaultBackgroundColor = Color.FromArgb(242, 242, 244);
            Controls.Add(web);

            Load += new EventHandler(MainForm_Load);
            FormClosing += new FormClosingEventHandler(MainForm_Closing);
            ResizeEnd += new EventHandler(MainForm_ResizeEnd);
            Activated += new EventHandler(MainForm_Activated);
            Shown += new EventHandler(MainForm_Shown);

            revealTimer = new Timer();
            revealTimer.Interval = 50;
            revealTimer.Tick += new EventHandler(RevealTick);

            BuildTray();
        }

        private int S(int v) { return (int)Math.Round(v * sc); }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            ApplyWindowChrome();
            TryRegisterHotkey();
            revealTimer.Start();
        }

        // ---------------- 窗口外观 ----------------
        private void ApplyWindowChrome()
        {
            int round = Native.DWMWCP_ROUND;
            int hr = 1;
            try { hr = Native.DwmSetWindowAttribute(Handle, Native.DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(int)); }
            catch (Exception) { hr = 1; }
            if (hr != 0)
            {
                Rectangle rect = new Rectangle(0, 0, Width, Height);
                int r = S(13);
                GraphicsPath p = new GraphicsPath();
                p.AddArc(rect.Left, rect.Top, r * 2, r * 2, 180, 90);
                p.AddArc(rect.Right - r * 2, rect.Top, r * 2, r * 2, 270, 90);
                p.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                p.AddArc(rect.Left, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                p.CloseFigure();
                Region = new Region(p);
            }
        }

        private bool ApplyAccentAcrylic()
        {
            try
            {
                Native.AccentPolicy ap = new Native.AccentPolicy();
                ap.AccentState = 4;                       // ACCENT_ENABLE_ACRYLICBLURBEHIND
                ap.AccentFlags = 2;                       // 允许四边都做
                ap.GradientColor = unchecked((int)0x99F3F3F6); // AABBGGRR
                IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Native.AccentPolicy)));
                try
                {
                    Marshal.StructureToPtr(ap, buf, false);
                    Native.WCAData d = new Native.WCAData();
                    d.Attribute = 19;                     // WCA_ACCENT_POLICY
                    d.Data = buf;
                    d.SizeOfData = Marshal.SizeOf(typeof(Native.AccentPolicy));
                    return Native.SetWindowCompositionAttribute(Handle, ref d) != 0;
                }
                finally { Marshal.FreeHGlobal(buf); }
            }
            catch (Exception) { return false; }
        }

        private void ApplySurface(bool dark)
        {
            SetDarkTitlebar(dark);
            int none = Native.DWMSBT_NONE;
            try { Native.DwmSetWindowAttribute(Handle, Native.DWMWA_SYSTEMBACKDROP_TYPE, ref none, sizeof(int)); }
            catch (Exception) { }
            Color solid = dark ? Color.FromArgb(22, 22, 24) : Color.FromArgb(242, 242, 244);
            BackColor = solid;
            web.DefaultBackgroundColor = solid;
        }

        private void SetDarkTitlebar(bool dark)
        {
            int v = dark ? 1 : 0;
            try { Native.DwmSetWindowAttribute(Handle, Native.DWMWA_USE_IMMERSIVE_DARK_MODE, ref v, sizeof(int)); }
            catch (Exception) { }
        }

        // ---------------- WebView2 ----------------
        private void MainForm_Load(object sender, EventArgs e)
        {
            InitWebAsync();
        }

        private async void InitWebAsync()
        {
            try
            {
                string userData = Path.Combine(root, "userdata");
                if (!Directory.Exists(userData)) Directory.CreateDirectory(userData);

                // 交给 WebView2 自己建环境：少一次 CreateAsync 往返，启动更快
                CoreWebView2CreationProperties props = new CoreWebView2CreationProperties();
                props.UserDataFolder = userData;
                // 关掉遮挡计算与后台降频，首帧出得更快
                props.AdditionalBrowserArguments =
                    "--disable-features=CalculateNativeWinOcclusion,msEdgeTabPreloads" +
                    " --disable-background-timer-throttling --disable-renderer-backgrounding";
                web.CreationProperties = props;

                await web.EnsureCoreWebView2Async(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "无法启动内置浏览器内核（WebView2）。\n\n" +
                    "请安装微软官方的 WebView2 Runtime（约 10MB，Win10/11 通常已自带）：\n" +
                    "https://go.microsoft.com/fwlink/p/?LinkId=2124703\n\n" +
                    "详细信息：" + ex.Message,
                    "KeyPeek", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                reallyQuit = true;
                Close();
                return;
            }

            CoreWebView2 core = web.CoreWebView2;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsZoomControlEnabled = false;
            core.Settings.AreBrowserAcceleratorKeysEnabled = false;
            core.SetVirtualHostNameToFolderMapping("app.local", root, CoreWebView2HostResourceAccessKind.Allow);
            core.WebMessageReceived += new EventHandler<CoreWebView2WebMessageReceivedEventArgs>(Core_WebMessageReceived);
            core.NavigationCompleted += new EventHandler<CoreWebView2NavigationCompletedEventArgs>(Core_NavigationCompleted);
            webReady = true;

            // 带时间戳，避免 WebView2 命中旧页面缓存
            core.Navigate("https://app.local/app/KeyPeek.html?v=" + DateTime.Now.Ticks);
        }

        private void Core_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            pageReady = true;
        }

        private void RevealTick(object sender, EventArgs e)
        {
            double ms = (DateTime.Now - launchAt).TotalMilliseconds;
            if ((pageReady && ms > 300) || ms > 2600)
            {
                revealTimer.Stop();
                Opacity = 1.0;
                Activate();
                web.Focus();
                if (splash != null && !splashClosed)
                {
                    splashClosed = true;
                    try { splash.Finish(); } catch (Exception) { }
                }
            }
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (pageReady) Post("focus", "");
        }

        // ---------------- 消息 ----------------
        private void Core_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string raw = "";
            try { raw = e.TryGetWebMessageAsString(); }
            catch (Exception) { return; }
            if (raw == null) return;

            string[] parts = raw.Split(new char[] { '|' }, 3);
            string cmd = parts[0];
            string a1 = (parts.Length > 1 && parts[1].Length > 0) ? Decode(parts[1]) : "";
            string a2 = (parts.Length > 2 && parts[2].Length > 0) ? Decode(parts[2]) : "";

            switch (cmd)
            {
                case "hello":
                    SendInit();
                    SendStats();
                    Post("themeQuery", "");
                    Post("focus", "");
                    break;
                case "reload":
                    SendInit();
                    Post("saved", "配置已重新加载");
                    break;
                case "loadStats":
                    SendStats();
                    break;
                case "saveStats":
                    SaveStats(a1);
                    break;
                case "savePack":
                    SavePack(a1, a2);
                    break;
                case "deletePack":
                    DeletePack(a1);
                    break;
                case "openFolder":
                    OpenFolder();
                    break;
                case "hide":
                    HideToTray();
                    break;
                case "quit":
                    ReallyQuit();
                    break;
                case "theme":
                    ApplyTheme(a1);
                    break;
                case "drag":
                    BeginWindowDrag(Native.HTCAPTION);
                    break;
                case "resize":
                    // 尺寸改成托盘里的固定档位，不再支持拖拽缩放
                    break;
                case "pin":
                    SetPin(a1 == "1");
                    break;
                case "openUrl":
                    OpenUrl(a1);
                    break;
            }
        }

        // 只在浏览器里开官方文档，别的一概不接
        private void OpenUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return;
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return;
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception) { }
        }

        private void BeginWindowDrag(int hit)
        {
            try
            {
                Native.ReleaseCapture();
                sizing = true;
                Native.SendMessage(Handle, Native.WM_NCLBUTTONDOWN, new IntPtr(hit), IntPtr.Zero);
                sizing = false;
                // 拖拽结束后再重挂亚克力，中途不动它，免得闪
                if (acrylic && hit == Native.HTBOTTOMRIGHT) ReapplyAcrylic();
                // 松手后贴边吸附
                SnapToEdges();
                SaveState();
            }
            catch (Exception) { sizing = false; }
        }

        // 拖到屏幕边缘 14px 内就自动贴上去；离得远就不动
        private void SnapToEdges()
        {
            try
            {
                Rectangle wa = Screen.FromControl(this).WorkingArea;
                Rectangle r = Bounds;
                int t = S(14);
                int x = r.X, y = r.Y;
                if (Math.Abs(r.Left - wa.Left) <= t) x = wa.Left;
                else if (Math.Abs(wa.Right - r.Right) <= t) x = wa.Right - r.Width;
                if (Math.Abs(r.Top - wa.Top) <= t) y = wa.Top;
                else if (Math.Abs(wa.Bottom - r.Bottom) <= t) y = wa.Bottom - r.Height;
                if (x != r.X || y != r.Y) Location = new Point(x, y);
            }
            catch (Exception) { }
        }

        private bool acrylic;
        private Timer acrylicFix;
        private bool sizing;

        // 亚克力窗口改尺寸后，DWM 会留着旧尺寸的模糊面，边界能明显看到一块"旧色"。
        // 只重设 backdrop 属性不够，必须整个拆掉再挂一次（跟隐藏再显示的效果一致）。
        private void ScheduleAcrylicFix()
        {
            if (!acrylic) return;
            if (acrylicFix == null)
            {
                acrylicFix = new Timer();
                acrylicFix.Interval = 160;
                acrylicFix.Tick += new EventHandler(delegate(object s, EventArgs e)
                {
                    acrylicFix.Stop();
                    if (!sizing) LightRefreshAcrylic();
                });
            }
            acrylicFix.Stop();
            acrylicFix.Start();
        }

        // 轻量刷新：只让 DWM 重算边框和模糊面，不拆 backdrop，
        // 拆了再挂会闪一下，换档时用它就够了
        private void LightRefreshAcrylic()
        {
            if (!acrylic) return;
            try
            {
                Native.MARGINS m = new Native.MARGINS();
                m.Left = -1; m.Right = -1; m.Top = -1; m.Bottom = -1;
                Native.DwmExtendFrameIntoClientArea(Handle, ref m);
                Native.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                    Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_FRAMECHANGED);
                Invalidate(true);
            }
            catch (Exception) { }
        }

        private void ReapplyAcrylic()
        {
            if (!acrylic) return;
            try
            {
                ClearAcrylic();
                Native.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                    Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_FRAMECHANGED);
                if (!TryApplyAcrylic()) return;
                if (web != null)
                {
                    try { web.Invalidate(); } catch (Exception) { }
                }
                Invalidate(true);
                ThemeLog("reapply after resize " + Width + "x" + Height);
            }
            catch (Exception) { }
        }

        private static void ThemeLog(string msg)
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userdata");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string f = Path.Combine(dir, "theme.log");
                if (File.Exists(f) && new FileInfo(f).Length > 8192) File.Delete(f);
                File.AppendAllText(f, DateTime.Now.ToString("HH:mm:ss.fff") + "  " + msg + Environment.NewLine);
            }
            catch (Exception) { }
        }

        private void ApplyTheme(string id)
        {
            if (id == "acrylic" && TryApplyAcrylic()) return;
            ClearAcrylic();
            ApplySurface(id == "dark");
        }

        // 系统亚克力：DWM 在窗口背后实时模糊，配合 WebView2 透明背景露出
        private bool TryApplyAcrylic()
        {
            try
            {
                if (Native.RealBuild() < 22000)
                {
                    Post("themeFallback", "");
                    return false;
                }
                int backdrop = Native.DWMSBT_TRANSIENTWINDOW;
                int hr = Native.DwmSetWindowAttribute(Handle, Native.DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
                ThemeLog("apply backdrop hr=" + hr + " build=" + Native.RealBuild());
                if (hr != 0)
                {
                    // 备用通道：老的 AccentPolicy 亚克力
                    ThemeLog("dwm backdrop not supported, using accent");
                    if (!ApplyAccentAcrylic())
                    {
                        Post("themeFallback", "");
                        return false;
                    }
                }
                Native.MARGINS m = new Native.MARGINS();
                m.Left = -1; m.Right = -1; m.Top = -1; m.Bottom = -1;
                Native.DwmExtendFrameIntoClientArea(Handle, ref m);

                acrylic = true;
                SetDarkTitlebar(false);
                // 自己不画背景，客户区留给 DWM 合成
                BackColor = Color.Black;
                web.DefaultBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                Invalidate();
                return true;
            }
            catch (Exception)
            {
                Post("themeFallback", "");
                return false;
            }
        }

        private void ClearAcrylic()
        {
            if (!acrylic) return;
            acrylic = false;
            try
            {
                Native.MARGINS m = new Native.MARGINS();
                Native.DwmExtendFrameIntoClientArea(Handle, ref m);
                int none = Native.DWMSBT_NONE;
                Native.DwmSetWindowAttribute(Handle, Native.DWMWA_SYSTEMBACKDROP_TYPE, ref none, sizeof(int));
                // 关掉 AccentPolicy 亚克力（备用通道用过的话）
                Native.AccentPolicy ap = new Native.AccentPolicy();
                ap.AccentState = 0;
                IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Native.AccentPolicy)));
                try
                {
                    Marshal.StructureToPtr(ap, buf, false);
                    Native.WCAData d = new Native.WCAData();
                    d.Attribute = 19;
                    d.Data = buf;
                    d.SizeOfData = Marshal.SizeOf(typeof(Native.AccentPolicy));
                    Native.SetWindowCompositionAttribute(Handle, ref d);
                }
                finally { Marshal.FreeHGlobal(buf); }
            }
            catch (Exception) { }
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (acrylic) return;
            base.OnPaintBackground(e);
        }

        private void SendInit()
        {
            List<string> files = new List<string>();
            try
            {
                if (Directory.Exists(packsDir))
                {
                    foreach (string f in Directory.GetFiles(packsDir, "*.json"))
                    {
                        string name = Path.GetFileName(f);
                        if (name.StartsWith("_", StringComparison.Ordinal)) continue;
                        if (name.EndsWith(".bak.json", StringComparison.OrdinalIgnoreCase)) continue;
                        files.Add(name);
                    }
                }
            }
            catch (Exception) { }
            files.Sort(StringComparer.OrdinalIgnoreCase);

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"root\":\"").Append(JsonEsc(root)).Append("\",\"files\":[");
            for (int i = 0; i < files.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append('"').Append(JsonEsc(files[i])).Append('"');
            }
            sb.Append("]}");
            Post("init", sb.ToString());
        }

        private void SendStats()
        {
            string text = "";
            try { if (File.Exists(statsPath)) text = File.ReadAllText(statsPath, Encoding.UTF8); }
            catch (Exception) { }
            Post("stats", text);
        }

        private void SaveStats(string text)
        {
            try
            {
                string trimmed = text.Trim();
                if (!trimmed.StartsWith("{") || !trimmed.EndsWith("}")) return;
                File.WriteAllText(statsPath, trimmed, new UTF8Encoding(false));
            }
            catch (Exception) { }
        }

        private void SavePack(string file, string text)
        {
            try
            {
                if (string.IsNullOrEmpty(file)) { Post("error", "缺少文件名"); return; }
                string trimmed = text.Trim();
                if (!trimmed.StartsWith("{")) { Post("error", "内容不是合法 JSON，已取消写入"); return; }
                if (!Directory.Exists(packsDir)) Directory.CreateDirectory(packsDir);
                string path = Path.Combine(packsDir, file);
                if (!Path.GetFileName(path).Equals(file, StringComparison.Ordinal)) { Post("error", "文件名不合法"); return; }
                if (File.Exists(path))
                {
                    try { File.Copy(path, path + ".bak", true); }
                    catch (Exception) { }
                }
                File.WriteAllText(path, text, new UTF8Encoding(false));
                Post("saved", file + " 已写入");
            }
            catch (Exception ex) { Post("error", "写入失败：" + ex.Message); }
        }

        private void DeletePack(string file)
        {
            try
            {
                if (string.IsNullOrEmpty(file)) { Post("error", "缺少文件名"); return; }
                string path = Path.Combine(packsDir, file);
                if (!Path.GetFileName(path).Equals(file, StringComparison.Ordinal)) { Post("error", "文件名不合法"); return; }
                if (!File.Exists(path)) { Post("error", "文件不存在：" + file); return; }
                string trash = Path.Combine(packsDir, "_已删除");
                if (!Directory.Exists(trash)) Directory.CreateDirectory(trash);
                string target = Path.Combine(trash, DateTime.Now.ToString("yyyyMMdd-HHmmss") + "_" + file);
                File.Move(path, target);
                Post("deleted", file);
            }
            catch (Exception ex) { Post("error", "删除失败：" + ex.Message); }
        }

        private void OpenFolder()
        {
            try
            {
                if (!Directory.Exists(packsDir)) Directory.CreateDirectory(packsDir);
                System.Diagnostics.Process.Start("explorer.exe", "\"" + packsDir + "\"");
            }
            catch (Exception ex) { Post("error", "打开文件夹失败：" + ex.Message); }
        }

        private void Post(string cmd, string arg)
        {
            if (!webReady || web.CoreWebView2 == null) return;
            if (arg == null) arg = "";
            try { web.CoreWebView2.PostWebMessageAsString(cmd + "|" + Encode(arg)); }
            catch (Exception) { }
        }

        private static string Encode(string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }

        private static string Decode(string s)
        {
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(s)); }
            catch (Exception) { return ""; }
        }

        private static string JsonEsc(string s)
        {
            if (s == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ') sb.Append(' ');
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        // ---------------- 托盘 / 热键 ----------------
        private void BuildTray()
        {
            Icon icon;
            try { icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch (Exception) { icon = SystemIcons.Application; }

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowImageMargin = false;
            menu.Items.Add(new ToolStripMenuItem("显示 KeyPeek", null, new EventHandler(ShowFront_Click)));
            menu.Items.Add(new ToolStripMenuItem("隐藏到托盘", null, new EventHandler(Hide_Click)));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("小窗 360×480", null, new EventHandler(SizeSmall_Click)));
            menu.Items.Add(new ToolStripMenuItem("标准 440×620", null, new EventHandler(SizeMid_Click)));
            menu.Items.Add(new ToolStripMenuItem("大窗 560×760", null, new EventHandler(SizeBig_Click)));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("置顶（当前：" + (pin ? "开" : "关") + "）", null, new EventHandler(Pin_Click)));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("退出 KeyPeek", null, new EventHandler(Quit_Click)));

            tray = new NotifyIcon();
            tray.Icon = icon;
            tray.Text = "KeyPeek · Ctrl+Alt+Space 唤出";
            tray.ContextMenuStrip = menu;
            tray.Visible = true;
            tray.DoubleClick += new EventHandler(ShowFront_Click);
            tray.BalloonTipTitle = "KeyPeek";
            tray.BalloonTipText = "按 Ctrl+Alt+Space 随时唤出";
        }

        private void ShowFront_Click(object sender, EventArgs e) { ShowFront(); }
        private void Hide_Click(object sender, EventArgs e) { HideToTray(); }
        private void Quit_Click(object sender, EventArgs e) { ReallyQuit(); }
        private void Pin_Click(object sender, EventArgs e) { SetPin(!pin); }

        private void SizeSmall_Click(object sender, EventArgs e) { ResizePreset(360, 480); }
        private void SizeMid_Click(object sender, EventArgs e) { ResizePreset(440, 620); }
        private void SizeBig_Click(object sender, EventArgs e) { ResizePreset(560, 760); }

        private void ResizePreset(int w, int h)
        {
            if (!Visible) ShowFront();
            BeginInvoke(new Action(delegate
            {
                // 亚克力：改尺寸必须和 SWP_FRAMECHANGED 一起发，DWM 才会按新尺寸重建那张模糊面。
                bool wasAcrylic = acrylic;
                if (acrylicFix != null) acrylicFix.Stop();
                if (wasAcrylic) ClearAcrylic();
                Native.SetWindowPos(Handle, IntPtr.Zero, 0, 0, S(w), S(h),
                    Native.SWP_NOMOVE | Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_FRAMECHANGED);
                if (wasAcrylic)
                {
                    if (!TryApplyAcrylic()) ApplySurface(false);
                    if (web != null)
                    {
                        try
                        {
                            web.DefaultBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                            web.Invalidate();
                        }
                        catch (Exception) { }
                    }
                    Invalidate(true);
                    ThemeLog("preset resize " + w + "x" + h);
                }
                SaveState();
            }));
        }

        private void SetPin(bool on)
        {
            pin = on;
            TopMost = pin;
            if (tray != null && tray.ContextMenuStrip != null)
            {
                foreach (ToolStripItem it in tray.ContextMenuStrip.Items)
                {
                    if (it.Text != null && it.Text.StartsWith("置顶"))
                        it.Text = "置顶（当前：" + (pin ? "开" : "关") + "）";
                }
            }
            Post("pin", pin ? "1" : "0");
            SaveState();
        }

        private void TryRegisterHotkey()
        {
            try
            {
                hotkeyOk = Native.RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_SPACE);
            }
            catch (Exception) { hotkeyOk = false; }

            if (hotkeyOk) { if (tray != null) tray.Text = "KeyPeek · Ctrl+Alt+Space 唤出"; }
            else
            {
                if (tray != null) tray.Text = "KeyPeek（热键被占用，可用托盘唤出）";
            }
        }

        private void ShowFront()
        {
            try
            {
                if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
                Show();
                BringToFront();
                TopMost = pin;
                Activate();
                web.Focus();
                Post("focus", "");
            }
            catch (Exception) { }
        }

        private void HideToTray()
        {
            SaveState();
            Hide();
        }

        private void ReallyQuit()
        {
            reallyQuit = true;
            SaveState();
            if (tray != null) { tray.Visible = false; }
            Close();
        }

        // ---------------- 状态记忆 ----------------
        private void RestoreWindowState()
        {
            Size = new Size(S(440), S(620));
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            Point corner = new Point(
                wa.Right - Width - S(28),
                wa.Bottom - Height - S(28));
            Location = corner;
            try
            {
                if (!File.Exists(statePath)) return;
                string[] lines = File.ReadAllLines(statePath, Encoding.UTF8);
                if (lines.Length >= 4)
                {
                    int x = int.Parse(lines[0]);
                    int y = int.Parse(lines[1]);
                    int w = int.Parse(lines[2]);
                    int h = int.Parse(lines[3]);
                    Rectangle r = new Rectangle(x, y, Math.Max(w, MinimumSize.Width), Math.Max(h, MinimumSize.Height));
                    bool visible = false;
                    foreach (Screen scr in Screen.AllScreens)
                        if (scr.WorkingArea.IntersectsWith(r)) { visible = true; break; }
                    if (visible) Bounds = r;
                }
                if (lines.Length >= 5)
                {
                    string flag = lines[4].Trim();
                    if (flag == "0" || flag == "1") pin = (flag == "1");
                }
            }
            catch (Exception) { }
            TopMost = pin;
        }

        private void SaveState()
        {
            try
            {
                Rectangle r = (WindowState == FormWindowState.Normal) ? Bounds : RestoreBounds;
                string content = r.X + "\r\n" + r.Y + "\r\n" + r.Width + "\r\n" + r.Height + "\r\n" + (pin ? "1" : "0") + "\r\n";
                File.WriteAllText(statePath, content, new UTF8Encoding(false));
            }
            catch (Exception) { }
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            SnapToEdges();
            SaveState();
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (!reallyQuit)
            {
                e.Cancel = true;
                HideToTray();
                return;
            }
            try { Native.UnregisterHotKey(Handle, HOTKEY_ID); }
            catch (Exception) { }
            SaveState();
            if (tray != null) { tray.Visible = false; tray.Dispose(); }
        }

        // ---------------- 热键 / 单实例 ----------------
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                if (Visible && ContainsFocus) HideToTray();
                else ShowFront();
                return;
            }
            if (m.Msg == Program.WM_KEYPEEK_SHOW)
            {
                ShowFront();
                return;
            }
            if (m.Msg == Native.WM_SIZE)
            {
                // 拖拽中动 DWM 会闪，而且并不解决边界；只在尺寸稳定后统一修
                ScheduleAcrylicFix();
            }
            base.WndProc(ref m);
        }
    }
}
