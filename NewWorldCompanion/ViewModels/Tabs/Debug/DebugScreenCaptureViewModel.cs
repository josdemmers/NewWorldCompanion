using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NewWorldCompanion.ViewModels.Tabs.Debug
{
    public class DebugScreenCaptureViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IScreenCaptureHandler _screenCaptureHandler;
        private readonly IScreenProcessHandler _screenProcessHandler;

        private BitmapSource? _capturedImage = null;
        private BitmapSource? _processedImage = null;
        private string _mouseCoordinates = string.Empty;
        private string _mouseCoordinatesScaled = string.Empty;

        // Start of Constructor region

        #region Constructor

        public DebugScreenCaptureViewModel(IEventAggregator eventAggregator, IScreenCaptureHandler screenCaptureHandler, IScreenProcessHandler screenProcessHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<MouseCoordinatesUpdatedEvent>().Subscribe(HandleMouseCoordinatesUpdatedEvent);
            _eventAggregator.GetEvent<ScreenCaptureReadyEvent>().Subscribe(HandleScreenCaptureReadyEvent);
            _eventAggregator.GetEvent<ProcessedImageReadyEvent>().Subscribe(HandleProcessedImageReadyEvent);

            // Init services
            _screenCaptureHandler = screenCaptureHandler;
            _screenProcessHandler = screenProcessHandler;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public BitmapSource? CapturedImage
        {
            get => _capturedImage;
            set
            {
                _capturedImage = value;
                RaisePropertyChanged(nameof(CapturedImage));
            }
        }

        public BitmapSource? ProcessedImage
        {
            get => _processedImage;
            set
            {
                _processedImage = value;
                RaisePropertyChanged(nameof(ProcessedImage));
            }
        }

        public string MouseCoordinates
        {
            get => _mouseCoordinates;
            set
            {
                _mouseCoordinates = value;
                RaisePropertyChanged(nameof(MouseCoordinates));
            }
        }

        public string MouseCoordinatesScaled
        {
            get => _mouseCoordinatesScaled;
            set
            {
                _mouseCoordinatesScaled = value;
                RaisePropertyChanged(nameof(MouseCoordinatesScaled));
            }
        }

        #endregion

        // Start of Events region

        #region Events

        private void HandleScreenCaptureReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                CapturedImage = _screenCaptureHandler.ImageSourceFromScreenCapture();
            });
        }

        private void HandleProcessedImageReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                ProcessedImage = Helpers.ScreenCapture.ImageSourceFromBitmap(_screenProcessHandler.ProcessedImage);
            });
        }

        private void HandleMouseCoordinatesUpdatedEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                MouseCoordinates = _screenCaptureHandler.MouseCoordinates;
                MouseCoordinatesScaled = _screenCaptureHandler.MouseCoordinatesScaled;
            });
        }

        #endregion
    }
}
