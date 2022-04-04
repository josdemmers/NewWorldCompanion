using NewWorldCompanion.Constants;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NewWorldCompanion.ViewModels.Tabs.Debug
{
    public class DebugScreenProcessViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsManager _settingsManager;
        private readonly IScreenProcessHandler _screenProcessHandler;

        private BitmapSource? _processedImage = null;
        private BitmapSource? _roiImage = null;
        private BitmapSource? _ocrImage = null;

        // Start of Constructor region

        #region Constructor

        public DebugScreenProcessViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager, IScreenProcessHandler screenProcessHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ProcessedImageReadyEvent>().Subscribe(HandleProcessedImageReadyEvent);
            _eventAggregator.GetEvent<RoiImageReadyEvent>().Subscribe(HandleRoiImageReadyEvent);
            _eventAggregator.GetEvent<OcrImageReadyEvent>().Subscribe(HandleOcrImageReadyEvent);

            // Init services
            _settingsManager = settingsManager;
            _screenProcessHandler = screenProcessHandler;

            // Init View commands
            RestoreDefaultsCommand = new DelegateCommand(RestoreDefaultsExecute);
        }

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand RestoreDefaultsCommand { get; }

        public int AreaLower
        {
            get => _settingsManager.Settings.EmguAreaLower; 
            set
            {
                _settingsManager.Settings.EmguAreaLower = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(AreaLower));
            }
        }

        public int AreaUpper
        {
            get => _settingsManager.Settings.EmguAreaUpper;
            set
            {
                _settingsManager.Settings.EmguAreaUpper = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(AreaUpper));
            }
        }
        public int HysteresisLower
        {
            get => _settingsManager.Settings.EmguHysteresisLower;
            set
            {
                _settingsManager.Settings.EmguHysteresisLower = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(HysteresisLower));
            }
        }
        public int HysteresisUpper
        {
            get => _settingsManager.Settings.EmguHysteresisUpper;
            set
            {
                _settingsManager.Settings.EmguHysteresisUpper = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(HysteresisUpper));
            }
        }

        public object? SelectedPresetConfig { get; set; }

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
            switch ((SelectedPresetConfig as ComboBoxItem)?.Content ?? string.Empty)
            {
                case "1920x1080":
                    AreaLower = EmguConstants.Default19201080AreaLower;
                    AreaUpper = EmguConstants.Default19201080AreaUpper;
                    break;
                case "2560x1440":
                    AreaLower = EmguConstants.Default25601440AreaLower;
                    AreaUpper = EmguConstants.Default25601440AreaUpper;
                    break;
                default:
                    AreaLower = EmguConstants.Default19201080AreaLower;
                    AreaUpper = EmguConstants.Default19201080AreaUpper;
                    break;
            }

            HysteresisLower = EmguConstants.DefaultHysteresisLower;
            HysteresisUpper = EmguConstants.DefaultHysteresisUpper;
        }

        #endregion
    }
}