using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace NewWorldCompanion.Helpers
{
    public class ScreenCapture
    {
        // Docs: https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-bitblt
        const int SRCCOPY = 0x00CC0020;
        const int CAPTUREBLT = 0x40000000;

        public Bitmap GetScreenCapture(IntPtr windowHandle)
        {
            Bitmap bitmap;

            PInvoke.RECT region;
            PInvoke.User32.GetWindowRect(windowHandle, out region);

            var desktopWindowHandle = PInvoke.User32.GetDesktopWindow();
            var windowDCHandle = PInvoke.User32.GetWindowDC(desktopWindowHandle);
            var memoryDCHandle = PInvoke.Gdi32.CreateCompatibleDC(windowDCHandle);
            var bitmapHandle = PInvoke.Gdi32.CreateCompatibleBitmap(windowDCHandle,
                region.right - region.left, region.bottom - region.top);
            var bitmapOldHandle = PInvoke.Gdi32.SelectObject(memoryDCHandle, bitmapHandle);

            bool status = PInvoke.Gdi32.BitBlt(memoryDCHandle.DangerousGetHandle(), 0, 0,
                region.right - region.left, region.bottom - region.top,
                windowDCHandle.DangerousGetHandle(), region.left, region.top, SRCCOPY | CAPTUREBLT);

            try
            {
                bitmap = Image.FromHbitmap(bitmapHandle);
            }
            finally
            {
                PInvoke.Gdi32.SelectObject(memoryDCHandle, bitmapOldHandle);
                PInvoke.Gdi32.DeleteObject(bitmapHandle);
                PInvoke.Gdi32.DeleteDC(memoryDCHandle);
                PInvoke.User32.ReleaseDC(desktopWindowHandle, windowDCHandle.DangerousGetHandle());
            }

            return bitmap;
        }

        public Bitmap? GetScreenCaptureMouse(IntPtr windowHandle)
        {
            Bitmap? bitmap = null;

            PInvoke.RECT region;
            PInvoke.User32.GetWindowRect(windowHandle, out region);

            int width = (int)((region.right - region.left) * 0.75);
            int height = 400;

            PInvoke.User32.CURSORINFO cursorInfo = new PInvoke.User32.CURSORINFO();
            cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
            PInvoke.User32.GetCursorInfo(ref cursorInfo);
            int xPos = cursorInfo.ptScreenPos.x;
            int yPos = cursorInfo.ptScreenPos.y;

            var desktopWindowHandle = PInvoke.User32.GetDesktopWindow();
            var windowDCHandle = PInvoke.User32.GetWindowDC(desktopWindowHandle);
            var memoryDCHandle = PInvoke.Gdi32.CreateCompatibleDC(windowDCHandle);
            var bitmapHandle = PInvoke.Gdi32.CreateCompatibleBitmap(windowDCHandle, width, height);
            var bitmapOldHandle = PInvoke.Gdi32.SelectObject(memoryDCHandle, bitmapHandle);

            xPos = Math.Min(Math.Max(xPos - width / 2, region.left), region.right - width);
            yPos = Math.Min(Math.Max(yPos - height / 2, region.top), region.bottom - height);
            bool status = PInvoke.Gdi32.BitBlt(memoryDCHandle.DangerousGetHandle(), 0, 0, width, height, windowDCHandle.DangerousGetHandle(), xPos, yPos, SRCCOPY | CAPTUREBLT);

            try
            {
                if (status)
                {
                    bitmap = Image.FromHbitmap(bitmapHandle);
                }
            }
            finally
            {
                PInvoke.Gdi32.SelectObject(memoryDCHandle, bitmapOldHandle);
                PInvoke.Gdi32.DeleteObject(bitmapHandle);
                PInvoke.Gdi32.DeleteDC(memoryDCHandle);
                PInvoke.User32.ReleaseDC(desktopWindowHandle, windowDCHandle.DangerousGetHandle());
            }

            return bitmap;
        }

        public static BitmapSource? ImageSourceFromBitmap(Bitmap? bitmap)
        {
            if (bitmap != null)
            {
                var handle = bitmap.GetHbitmap();
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    PInvoke.Gdi32.DeleteObject(handle);
                }
            }
            else
            {
                return null;
            }
        }

        public static void WriteBitmapToFile(string filename, Bitmap bitmap)
        {
            // Use the OpenCv save function instead
            // https://stackoverflow.com/questions/52100703/bug-in-windows-nets-system-drawing-savestream-imageformat-corrupt-png-pro
            bitmap.Save(filename, ImageFormat.Png);
        }
    }
}
