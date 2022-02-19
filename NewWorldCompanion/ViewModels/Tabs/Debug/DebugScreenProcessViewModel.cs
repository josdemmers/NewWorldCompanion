using NewWorldCompanion.Constants;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NewWorldCompanion.ViewModels.Tabs.Debug
{
    public class DebugScreenProcessViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IScreenProcessHandler _screenProcessHandler;

        private int _areaLower;
        private int _areaUpper;
        private int _hysteresisLower;
        private int _hysteresisUpper;
        private BitmapSource? _processedImage = null;
        private BitmapSource? _roiImage = null;
        private BitmapSource? _ocrImage = null;

        // Start of Constructor region

        #region Constructor

        public DebugScreenProcessViewModel(IEventAggregator eventAggregator, IScreenProcessHandler screenProcessHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ProcessedImageReadyEvent>().Subscribe(HandleProcessedImageReadyEvent);
            _eventAggregator.GetEvent<RoiImageReadyEvent>().Subscribe(HandleRoiImageReadyEvent);
            _eventAggregator.GetEvent<OcrImageReadyEvent>().Subscribe(HandleOcrImageReadyEvent);

            // Init services
            _screenProcessHandler = screenProcessHandler;

            // Init View commands
            RestoreDefaultsCommand = new DelegateCommand(RestoreDefaultsExecute);

            // Restore defaults
            AreaLower = EmguConstants.AreaLower;
            AreaUpper = EmguConstants.AreaUpper;
            HysteresisLower = EmguConstants.HysteresisLower;
            HysteresisUpper = EmguConstants.HysteresisUpper;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand RestoreDefaultsCommand { get; }

        public int AreaLower
        {
            get => _areaLower; 
            set
            {
                _areaLower = value;
                _screenProcessHandler.AreaLower = value;
                RaisePropertyChanged(nameof(AreaLower));
            }
        }
        public int AreaUpper
        {
            get => _areaUpper;
            set
            {
                _areaUpper = value;
                _screenProcessHandler.AreaUpper = value;
                RaisePropertyChanged(nameof(AreaUpper));
            }
        }
        public int HysteresisLower
        {
            get => _hysteresisLower;
            set
            {
                _hysteresisLower = value;
                _screenProcessHandler.HysteresisLower = value;
                RaisePropertyChanged(nameof(HysteresisLower));
            }
        }
        public int HysteresisUpper
        {
            get => _hysteresisUpper;
            set
            {
                _hysteresisUpper = value;
                _screenProcessHandler.HysteresisUpper = value;
                RaisePropertyChanged(nameof(HysteresisUpper));
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

        public BitmapSource? RoiImage
        {
            get => _roiImage; 
            set
            {
                _roiImage = value;
                RaisePropertyChanged(nameof(RoiImage));
            }
        }

        public BitmapSource? OcrImage
        {
            get => _ocrImage; 
            set
            {
                _ocrImage = value;
                RaisePropertyChanged(nameof(OcrImage));
            }
        }

        #endregion

        // Start of Events region

        #region Events

        private void HandleProcessedImageReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                ProcessedImage = Helpers.ScreenCapture.ImageSourceFromBitmap(_screenProcessHandler.ProcessedImage);
            });
        }

        private void HandleRoiImageReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                RoiImage = Helpers.ScreenCapture.ImageSourceFromBitmap(_screenProcessHandler.RoiImage);
            });
        }

        private void HandleOcrImageReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                OcrImage = Helpers.ScreenCapture.ImageSourceFromBitmap(_screenProcessHandler.OcrImage);
            });
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void RestoreDefaultsExecute()
        {
            AreaLower = EmguConstants.AreaLower;
            AreaUpper = EmguConstants.AreaUpper;
            HysteresisLower = EmguConstants.HysteresisLower;
            HysteresisUpper = EmguConstants.HysteresisUpper;
        }

        #endregion


    }
}