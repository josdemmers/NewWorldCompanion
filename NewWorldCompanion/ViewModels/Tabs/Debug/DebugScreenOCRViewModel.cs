using NewWorldCompanion.Constants;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NewWorldCompanion.ViewModels.Tabs.Debug
{
    public class DebugScreenOCRViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsManager _settingsManager;
        private readonly IScreenProcessHandler _screenProcessHandler;
        private readonly IOcrHandler _ocrHandler;

        private string _itemName = string.Empty;
        private string _itemCount = string.Empty;
        private BitmapSource? _ocrImage = null;
        private BitmapSource? _ocrImageCount = null;

        // Start of Constructor region

        #region Constructor

        public DebugScreenOCRViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager, IScreenProcessHandler screenProcessHandler, IOcrHandler ocrHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OcrImageReadyEvent>().Subscribe(HandleOcrImageReadyEvent);
            _eventAggregator.GetEvent<OcrImageCountReadyEvent>().Subscribe(HandleOcrImageCountReadyEvent);
            _eventAggregator.GetEvent<OcrTextReadyEvent>().Subscribe(HandleOcrTextReadyEvent);
            _eventAggregator.GetEvent<OcrTextCountReadyEvent>().Subscribe(HandleOcrTextCountReadyEvent);

            // Init services
            _settingsManager = settingsManager;
            _screenProcessHandler = screenProcessHandler;
            _ocrHandler = ocrHandler;

            // Init View commands
            RestoreDefaultsCommand = new DelegateCommand(RestoreDefaultsExecute);
            CopyItemNameCommand = new DelegateCommand(CopyItemNameExecute);
            CopyItemCountCommand = new DelegateCommand(CopyItemCountExecute);
        }

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand RestoreDefaultsCommand { get; }
        public DelegateCommand CopyItemNameCommand { get; }
        public DelegateCommand CopyItemCountCommand { get; }

        public int ThresholdMin
        {
            get => _settingsManager.Settings.EmguThresholdMin;
            set
            {
                _settingsManager.Settings.EmguThresholdMin = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(ThresholdMin));
            }
        }
        public int ThresholdMax
        {
            get => _settingsManager.Settings.EmguThresholdMax;
            set
            {
                _settingsManager.Settings.EmguThresholdMax = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(ThresholdMax));
            }
        }

        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                RaisePropertyChanged(nameof(ItemName));
            }
        }

        public string ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = value;
                RaisePropertyChanged(nameof(ItemCount));
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

        public BitmapSource? OcrImageCount
        {
            get => _ocrImageCount;
            set
            {
                _ocrImageCount = value;
                RaisePropertyChanged(nameof(OcrImageCount));
            }
        }

        #endregion

        // Start of Events region

        #region Events

        private void HandleOcrImageReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                OcrImage = Helpers.ScreenCapture.ImageSourceFromBitmap(_screenProcessHandler.OcrImage);
            });
        }

        private void HandleOcrImageCountReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                OcrImageCount = Helpers.ScreenCapture.ImageSourceFromBitmap(_screenProcessHandler.OcrImageCount);
            });
        }

        private void HandleOcrTextReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                ItemName = _ocrHandler.OcrText;
            });
        }

        private void HandleOcrTextCountReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                ItemCount = _ocrHandler.OcrTextCount;
            });
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void RestoreDefaultsExecute()
        {
            ThresholdMin = EmguConstants.DefaultThresholdMin;
            ThresholdMax = EmguConstants.DefaultThresholdMax;
        }

        private void CopyItemNameExecute()
        {
            try
            {
                System.Windows.Clipboard.SetText(ItemName);
            }
            catch (Exception) { }
        }

        private void CopyItemCountExecute()
        {
            try
            {
                System.Windows.Clipboard.SetText(ItemCount);
            }
            catch (Exception) { }
        }

        #endregion
    }
}