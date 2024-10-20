﻿using Microsoft.Extensions.Logging;
using NewWorldCompanion.Events;
using NewWorldCompanion.Helpers;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NewWorldCompanion.Services
{
    public class ScreenCaptureHandler : IScreenCaptureHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly INewWorldDataStore _newWorldDataStore;

        private DispatcherTimer _captureTimer = new();
        private DispatcherTimer _coordinatesTimer = new();
        private Bitmap? _currentScreen = null;
        private Bitmap? _currentScreenMouseArea = null;
        private int _delay = 100;
        private int _delayCoordinates = 100;
        private int _mouseX = 0;
        private int _mouseY = 0;
        private int _offsetX = 0;
        private int _offsetY = 0;
        private ScreenCapture _screenCapture = new ScreenCapture();

        // Start of Constructor region

        #region Constructor

        public ScreenCaptureHandler(IEventAggregator eventAggregator, ILogger<ScreenCaptureHandler> logger, ISettingsManager settingsManager, INewWorldDataStore newWorldDataStore)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init logger
            _logger = logger;

            // Init services
            _settingsManager = settingsManager;
            _newWorldDataStore = newWorldDataStore;

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
            _newWorldDataStore = newWorldDataStore;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public Bitmap? CurrentScreen { get => _currentScreen; set => _currentScreen = value; }
        public Bitmap? CurrentScreenMouseArea { get => _currentScreenMouseArea; set => _currentScreenMouseArea = value; }
        public int Delay { get => _delay; set => _delay = value; }
        public int DelayCoordinates { get => _delayCoordinates; set => _delayCoordinates = value; }
        public bool IsActive 
        { 
            get => _settingsManager.Settings.TooltipEnabled || _settingsManager.Settings.NamedItemsTooltipEnabled;
        }
        public string MouseCoordinates { get; set; } = string.Empty;
        public string MouseCoordinatesScaled { get; set; } = string.Empty;
        public int OffsetX { get => _offsetX; set => _offsetX = value; }
        public int OffsetY { get => _offsetY; set => _offsetY = value; }

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
            bool valid = false;

            if (IsActive && _newWorldDataStore.Available)
            {
                try
                {
                    IntPtr windowHandle = IntPtr.Zero;
                    Process[] processes = Process.GetProcessesByName("NewWorld");
                    Process[] processesGeForceNOW = Process.GetProcessesByName("GeForceNOW");
                    foreach (Process p in processes)
                    {
                        windowHandle = p.MainWindowHandle;
                    }
                    foreach (Process p in processesGeForceNOW)
                    {
                        windowHandle = p.MainWindowHandle;
                        if (p.MainWindowTitle.Contains("New World"))
                        {
                            break;
                        }
                    }

                    if (windowHandle.ToInt64() > 0)
                    {
                        _currentScreen = _screenCapture.GetScreenCaptureMouse(windowHandle, ref _offsetX, ref _offsetY) ?? _currentScreen;
                        _currentScreenMouseArea = _screenCapture.GetScreenCaptureMouseArea(windowHandle) ?? _currentScreenMouseArea;
                        _eventAggregator.GetEvent<ScreenCaptureReadyEvent>().Publish();
                        valid = true;

                        _captureTimer.Interval = TimeSpan.FromMilliseconds(Delay);
                    }
                    else
                    {
                        _logger.LogWarning($"{MethodBase.GetCurrentMethod()?.Name}: Invalid windowHandle. NewWorld processes found: {processes.Length}. Retry in 10 seconds.");

                        _captureTimer.Interval = TimeSpan.FromMilliseconds(Delay * 100);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);

                    _captureTimer.Interval = TimeSpan.FromMilliseconds(Delay * 10);
                }
                _captureTimer.Start();
            }
            else
            {
                _captureTimer.Interval = TimeSpan.FromMilliseconds(Delay * 10);
                _captureTimer.Start();
            }

            if (!valid)
            {
                _eventAggregator.GetEvent<OverlayHideEvent>().Publish();
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

            int x1 = _mouseX;
            int y1 = _mouseY;
            int x2 = cursorInfo.ptScreenPos.x;
            int y2 = cursorInfo.ptScreenPos.y;
            double delta = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            _mouseX = cursorInfo.ptScreenPos.x;
            _mouseY = cursorInfo.ptScreenPos.y;

            _coordinatesTimer.Interval = TimeSpan.FromMilliseconds(DelayCoordinates);
            _coordinatesTimer.IsEnabled = true;

            _eventAggregator.GetEvent<MouseCoordinatesUpdatedEvent>().Publish();
            _eventAggregator.GetEvent<MouseDeltaUpdatedEvent>().Publish(delta);
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

        public BitmapSource? ImageSourceFromScreenCaptureMouseArea()
        {
            if (CurrentScreenMouseArea != null)
            {
                return Helpers.ScreenCapture.ImageSourceFromBitmap(CurrentScreenMouseArea);
            }
            else
            {
                return null;
            }
        }

        #endregion

    }
}
