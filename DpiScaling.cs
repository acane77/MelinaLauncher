using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace tbm_launcher
{
    internal static class DpiHelper
    {
        public const int DefaultDpi = 96;
        public const int WM_DPICHANGED = 0x02E0;
        private const int MonitorDefaultToNearest = 2;
        private const int LogPixelsX = 88;

        private static readonly WindowsVersionInfo CachedWindowsVersion = GetWindowsVersion();

        public static void InitializeProcessDpiAwareness()
        {
            try
            {
                if (CachedWindowsVersion.Major >= 10)
                {
                    if (SetProcessDpiAwarenessContext(new IntPtr(-4)))
                    {
                        return;
                    }
                }

                if (CachedWindowsVersion.Major > 6
                    || (CachedWindowsVersion.Major == 6 && CachedWindowsVersion.Minor >= 3))
                {
                    if (SetProcessDpiAwareness(ProcessDpiAwareness.ProcessPerMonitorDpiAware) == 0)
                    {
                        return;
                    }
                }

                SetProcessDPIAware();
            }
            catch
            {
            }
        }

        public static int GetDpiForControl(Control control)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                if (control != null && control.IsHandleCreated)
                {
                    handle = control.Handle;
                }
            }
            catch
            {
                handle = IntPtr.Zero;
            }

            if (handle != IntPtr.Zero && CachedWindowsVersion.Major >= 10)
            {
                try
                {
                    uint dpi = GetDpiForWindow(handle);
                    if (dpi > 0)
                    {
                        return (int)dpi;
                    }
                }
                catch
                {
                }
            }

            if (handle != IntPtr.Zero
                && (CachedWindowsVersion.Major > 6
                    || (CachedWindowsVersion.Major == 6 && CachedWindowsVersion.Minor >= 3)))
            {
                try
                {
                    IntPtr monitor = MonitorFromWindow(handle, MonitorDefaultToNearest);
                    if (monitor != IntPtr.Zero)
                    {
                        uint dpiX;
                        uint dpiY;
                        if (GetDpiForMonitor(monitor, MonitorDpiType.Effective, out dpiX, out dpiY) == 0
                            && dpiX > 0)
                        {
                            return (int)dpiX;
                        }
                    }
                }
                catch
                {
                }
            }

            return GetSystemDpi();
        }

        public static int GetSystemDpi()
        {
            IntPtr screenDc = IntPtr.Zero;
            try
            {
                screenDc = GetDC(IntPtr.Zero);
                if (screenDc != IntPtr.Zero)
                {
                    int dpi = GetDeviceCaps(screenDc, LogPixelsX);
                    if (dpi > 0)
                    {
                        return dpi;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (screenDc != IntPtr.Zero)
                {
                    ReleaseDC(IntPtr.Zero, screenDc);
                }
            }

            return DefaultDpi;
        }

        public static int GetDpiFromWParam(IntPtr wParam)
        {
            long value = wParam.ToInt64();
            int dpi = (int)(value >> 16);
            return dpi > 0 ? dpi : DefaultDpi;
        }

        public static Rectangle GetSuggestedBounds(IntPtr lParam)
        {
            if (lParam == IntPtr.Zero)
            {
                return Rectangle.Empty;
            }

            NativeRect rect = (NativeRect)Marshal.PtrToStructure(lParam, typeof(NativeRect));
            return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static int ScaleValue(int designValue, int dpi)
        {
            return (int)Math.Round(designValue * dpi / (double)DefaultDpi);
        }

        public static Size ScaleSize(Size designSize, int dpi)
        {
            return new Size(ScaleValue(designSize.Width, dpi), ScaleValue(designSize.Height, dpi));
        }

        public static Point ScalePoint(Point designPoint, int dpi)
        {
            return new Point(ScaleValue(designPoint.X, dpi), ScaleValue(designPoint.Y, dpi));
        }

        public static Rectangle ScaleRectangle(Rectangle designRectangle, int dpi)
        {
            return new Rectangle(
                ScaleValue(designRectangle.Left, dpi),
                ScaleValue(designRectangle.Top, dpi),
                ScaleValue(designRectangle.Width, dpi),
                ScaleValue(designRectangle.Height, dpi));
        }

        public static Padding ScalePadding(Padding designPadding, int dpi)
        {
            return new Padding(
                ScaleValue(designPadding.Left, dpi),
                ScaleValue(designPadding.Top, dpi),
                ScaleValue(designPadding.Right, dpi),
                ScaleValue(designPadding.Bottom, dpi));
        }

        public static Font CreateScaledFont(Font designFont, int dpi)
        {
            if (designFont == null)
            {
                return null;
            }

            float scaledSize = (float)(designFont.SizeInPoints * dpi / (double)DefaultDpi);
            return new Font(
                designFont.FontFamily,
                scaledSize,
                designFont.Style,
                GraphicsUnit.Point,
                designFont.GdiCharSet);
        }

        private static WindowsVersionInfo GetWindowsVersion()
        {
            try
            {
                OsVersionInfo versionInfo = new OsVersionInfo();
                versionInfo.OSVersionInfoSize = Marshal.SizeOf(versionInfo);
                if (RtlGetVersion(ref versionInfo) == 0)
                {
                    return new WindowsVersionInfo(versionInfo.MajorVersion, versionInfo.MinorVersion);
                }
            }
            catch
            {
            }

            Version fallback = Environment.OSVersion.Version;
            return new WindowsVersionInfo(fallback.Major, fallback.Minor);
        }

        private struct WindowsVersionInfo
        {
            public readonly int Major;
            public readonly int Minor;

            public WindowsVersionInfo(int major, int minor)
            {
                Major = major;
                Minor = minor;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OsVersionInfo
        {
            public int OSVersionInfoSize;
            public int MajorVersion;
            public int MinorVersion;
            public int BuildNumber;
            public int PlatformId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string CsdVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private enum MonitorDpiType
        {
            Effective = 0
        }

        private enum ProcessDpiAwareness
        {
            ProcessDpiUnaware = 0,
            ProcessSystemDpiAware = 1,
            ProcessPerMonitorDpiAware = 2
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(ProcessDpiAwareness awareness);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(
            IntPtr hmonitor,
            MonitorDpiType dpiType,
            out uint dpiX,
            out uint dpiY);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int index);

        [DllImport("ntdll.dll", CharSet = CharSet.Unicode)]
        private static extern int RtlGetVersion(ref OsVersionInfo versionInfo);
    }

    public class DpiAwareForm : Form
    {
        private sealed class ControlDesignMetrics
        {
            public Rectangle Bounds;
            public Padding Margin;
            public Padding Padding;
            public Font DesignFont;
            public Font AppliedFont;
            public bool AutoSize;
        }

        private readonly Dictionary<Control, ControlDesignMetrics> designMetrics =
            new Dictionary<Control, ControlDesignMetrics>();

        private Size designClientSize;
        private bool dpiInitializationRequested;
        private bool dpiInitialized;
        private bool suppressFrameworkScaling = true;
        private int currentDpi = DpiHelper.DefaultDpi;

        protected DpiAwareForm()
        {
            base.AutoScaleMode = AutoScaleMode.None;
        }

        protected int CurrentDpi
        {
            get { return currentDpi; }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            if (suppressFrameworkScaling)
            {
                return;
            }

            base.ScaleControl(factor, specified);
        }

        protected void InitializeDpiAwareness()
        {
            AutoScaleMode = AutoScaleMode.None;
            designMetrics.Clear();
            CaptureDesignMetrics(this);
            designClientSize = ClientSize;
            dpiInitializationRequested = true;
            suppressFrameworkScaling = true;

            if (IsHandleCreated)
            {
                EnsureDpiInitialized();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            EnsureDpiInitialized();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeGeneratedFonts();
            }

            base.Dispose(disposing);
        }

        protected virtual void OnAfterDpiChanged(int oldDpi, int newDpi)
        {
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == DpiHelper.WM_DPICHANGED && dpiInitializationRequested)
            {
                int newDpi = DpiHelper.GetDpiFromWParam(m.WParam);
                Rectangle suggestedBounds = DpiHelper.GetSuggestedBounds(m.LParam);
                base.WndProc(ref m);

                if (!suggestedBounds.IsEmpty)
                {
                    Bounds = suggestedBounds;
                }

                int oldDpi = currentDpi;
                ApplyStaticDpiLayout(newDpi, false);
                OnAfterDpiChanged(oldDpi, currentDpi);
                return;
            }

            base.WndProc(ref m);
        }

        private void EnsureDpiInitialized()
        {
            if (!dpiInitializationRequested || dpiInitialized)
            {
                return;
            }

            dpiInitialized = true;
            int initialDpi = DpiHelper.GetDpiForControl(this);
            ApplyStaticDpiLayout(initialDpi, true);
            OnAfterDpiChanged(DpiHelper.DefaultDpi, currentDpi);
        }

        private void CaptureDesignMetrics(Control control)
        {
            if (control == null || designMetrics.ContainsKey(control))
            {
                return;
            }

            designMetrics[control] = new ControlDesignMetrics
            {
                Bounds = control.Bounds,
                Margin = control.Margin,
                Padding = control.Padding,
                DesignFont = control.Font,
                AutoSize = control.AutoSize
            };

            foreach (Control child in control.Controls)
            {
                CaptureDesignMetrics(child);
            }
        }

        private void ApplyStaticDpiLayout(int dpi, bool updateClientSize)
        {
            currentDpi = dpi > 0 ? dpi : DpiHelper.DefaultDpi;

            SuspendLayout();
            try
            {
                foreach (KeyValuePair<Control, ControlDesignMetrics> pair in designMetrics)
                {
                    ApplyFont(pair.Key, pair.Value);
                }

                if (updateClientSize)
                {
                    ClientSize = DpiHelper.ScaleSize(designClientSize, currentDpi);
                }

                foreach (KeyValuePair<Control, ControlDesignMetrics> pair in designMetrics)
                {
                    if (ReferenceEquals(pair.Key, this))
                    {
                        continue;
                    }

                    ApplyBoundsAndSpacing(pair.Key, pair.Value);
                }
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void ApplyFont(Control control, ControlDesignMetrics metrics)
        {
            return;
            if (control == null || control.IsDisposed || metrics == null || metrics.DesignFont == null)
            {
                return;
            }

            Font targetFont = metrics.DesignFont;
            if (currentDpi != DpiHelper.DefaultDpi)
            {
                if (metrics.AppliedFont != null)
                {
                    metrics.AppliedFont.Dispose();
                }

                metrics.AppliedFont = DpiHelper.CreateScaledFont(metrics.DesignFont, currentDpi);
                targetFont = metrics.AppliedFont ?? metrics.DesignFont;
            }
            else if (metrics.AppliedFont != null)
            {
                metrics.AppliedFont.Dispose();
                metrics.AppliedFont = null;
            }

            if (!ReferenceEquals(control.Font, targetFont))
            {
                control.Font = targetFont;
            }
        }

        private void ApplyBoundsAndSpacing(Control control, ControlDesignMetrics metrics)
        {
            if (control == null || control.IsDisposed || metrics == null)
            {
                return;
            }

            control.Margin = DpiHelper.ScalePadding(metrics.Margin, currentDpi);
            control.Padding = DpiHelper.ScalePadding(metrics.Padding, currentDpi);

            Rectangle scaledBounds = DpiHelper.ScaleRectangle(metrics.Bounds, currentDpi);
            if (metrics.AutoSize)
            {
                control.Location = scaledBounds.Location;
            }
            else
            {
                control.Bounds = scaledBounds;
            }
        }

        private void DisposeGeneratedFonts()
        {
            foreach (ControlDesignMetrics metrics in designMetrics.Values)
            {
                if (metrics.AppliedFont != null)
                {
                    metrics.AppliedFont.Dispose();
                    metrics.AppliedFont = null;
                }
            }
        }
    }
}
