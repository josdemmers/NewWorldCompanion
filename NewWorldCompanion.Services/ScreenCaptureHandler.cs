using NewWorldCompanion.Events;
using NewWorldCompanion.Helpers;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NewWorldCompanion.Services
{
    public class ScreenCaptureHandler : IScreenCaptureHandler
    {
        private readonly IEventAggregator _eventAggregator;

        private DispatcherTimer _captureTimer = new();
        private DispatcherTimer _coordinatesTimer = new();
        private Bitmap? _currentScreen = null;
        private int _delay = 200;
        private int _delayCoordinates = 100;
        private bool _isActive = true;
        private ScreenCapture _screenCapture = new ScreenCapture();

        // Start of Constructor region

        #region Constructor

        public ScreenCaptureHandler(IEventAggregator eventAggregator)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Capture timer
            _captureTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Delay),
                IsEnabled = true
            };
            _captureTimer.Tick += CaptureTimer_Tick;

            // Coordinates timer
            _coordinatesTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DelayCoordinates),
                IsEnabled = true
            };
            _coordinatesTimer.Tick += CoordinatesTimer_Tick;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public Bitmap? CurrentScreen { get => _currentScreen; set => _currentScreen = value; }
        public int Delay { get => _delay; set => _delay = value; }
        public int DelayCoordinates { get => _delayCoordinates; set => _delayCoordinates = value; }
        public bool IsActive { get => _isActive; set => _isActive = value; }
        public string MouseCoordinates { get; set; } = string.Empty;
        public string MouseCoordinatesScaled { get; set; } = string.Empty;

        #endregion

        // Start of Events region

        #region Events

        private void CaptureTimer_Tick(object? sender, EventArgs e)
        {
            (sender as DispatcherTimer)?.Stop();
            StartScreenCapture();
        }

        private void CoordinatesTimer_Tick(object? sender, EventArgs e)
        {
            (sender as DispatcherTimer)?.Stop();
            StartCoordinatesTask();
        }

        #endregion

        // Start of Methods region
        #region Methods

        private async void StartScreenCapture()
        {
            await Task.Run(ScreenCapture);
        }

        private async void StartCoordinatesTask()
        {
            await Task.Run(UpdateCoordinates);
        }

        private void ScreenCapture()
        {
            if (IsActive)
            {
                try
                {
                    IntPtr windowHandle = IntPtr.Zero;
                    Process[] processes = Process.GetProcessesByName("NewWorld");
                    foreach (Process p in processes)
                    {
                        windowHandle = p.MainWindowHandle;
                    }

                    if (windowHandle.ToInt64() > 0)
                    {
                        _currentScreen = _screenCapture.GetScreenCaptureMouse(windowHandle) ?? _currentScreen;
                        _eventAggregator.GetEvent<ScreenCaptureReadyEvent>().Publish();

                        _captureTimer.Interval = TimeSpan.FromMilliseconds(Delay);
                    }
                    else
                    {
                        _captureTimer.Interval = TimeSpan.FromMilliseconds(Delay * 10);
                    }
                }
                catch (Exception)
                {
                    _captureTimer.Interval = TimeSpan.FromMilliseconds(Delay * 10);
                }
                _captureTimer.Start();
            }
            else
            {
                _captureTimer.Interval = TimeSpan.FromMilliseconds(Delay * 10);
                _captureTimer.Start();
            }
        }

        private void UpdateCoordinates()
        {
            _coordinatesTimer.IsEnabled = false;

            PInvoke.User32.CURSORINFO cursorInfo = new PInvoke.User32.CURSORINFO();
            cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
            PInvoke.User32.GetCursorInfo(ref cursorInfo);
            MouseCoordinates = $"X: {cursorInfo.ptScreenPos.x}, Y: { cursorInfo.ptScreenPos.y}";
            //var monitor = PInvoke.User32.MonitorFromPoint(cursorInfo.ptScreenPos, PInvoke.User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);
            //var dpi = PInvoke.User32.GetDpiForMonitor(monitor, PInvoke.User32.MonitorDpiType.EFFECTIVE_DPI, out int dpiX, out int dpiY);
            var dpi = PInvoke.User32.GetDpiForSystem();
            var dpiScaling = Math.Round(dpi / (double)96, 2);
            MouseCoordinatesScaled = $"X: {(int)(cursorInfo.ptScreenPos.x / dpiScaling)}, Y: {(int)(cursorInfo.ptScreenPos.y / dpiScaling)}";

            _coordinatesTimer.Interval = TimeSpan.FromMilliseconds(DelayCoordinates);
            _coordinatesTimer.IsEnabled = true;

            _eventAggregator.GetEvent<MouseCoordinatesUpdatedEvent>().Publish();
        }

        public BitmapSource? ImageSourceFromScreenCapture()
        {
            if (CurrentScreen != null)
            {
                return Helpers.ScreenCapture.ImageSourceFromBitmap(CurrentScreen);
            }
            else
            {
                return null;
            }
        }

        #endregion

    }
}
