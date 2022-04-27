using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NewWorldCompanion.ViewModels.Tabs.Debug
{
    public class DebugScreenCountOCRViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IScreenProcessHandler _screenProcessHandler;
        private readonly IOcrHandler _ocrHandler;

        private string _itemCount = string.Empty;
        private BitmapSource? _ocrImageCount = null;
        private BitmapSource? _ocrImageCountRaw = null;

        private int _thresholdMinR = 0;
        private int _thresholdMinG = 0;
        private int _thresholdMinB = 0;
        private int _thresholdMaxR = 181;
        private int _thresholdMaxG = 195;
        private int _thresholdMaxB = 214;

        // Start of Constructor region

        #region Constructor

        public DebugScreenCountOCRViewModel(IEventAggregator eventAggregator, IScreenProcessHandler screenProcessHandler, IOcrHandler ocrHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OcrImageCountReadyEvent>().Subscribe(HandleOcrImageCountReadyEvent);
            _eventAggregator.GetEvent<OcrTextCountReadyEvent>().Subscribe(HandleOcrTextCountReadyEvent);

            // Init services
            _screenProcessHandler = screenProcessHandler;
            _ocrHandler = ocrHandler;

            // Init View commands
            CopyItemCountCommand = new DelegateCommand(CopyItemCountExecute);
        }

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand CopyItemCountCommand { get; }

        public string ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = value;
                RaisePropertyChanged(nameof(ItemCount));
            }
        }

        public int ThresholdMinR
        {
            get => _thresholdMinR;
            set
            {
                _thresholdMinR = value;
                _screenProcessHandler.ProcessImageCountOCRDebug(ThresholdMinR, ThresholdMinG, ThresholdMinB, ThresholdMaxR, ThresholdMaxG, ThresholdMaxB);
            }
        }

        public int ThresholdMinG
        {
            get => _thresholdMinG;
            set
            {
                _thresholdMinG = value;
                _screenProcessHandler.ProcessImageCountOCRDebug(ThresholdMinR, ThresholdMinG, ThresholdMinB, ThresholdMaxR, ThresholdMaxG, ThresholdMaxB);
            }
        }

        public int ThresholdMinB
        {
            get => _thresholdMinB;
            set
            {
                _thresholdMinB = value;
                _screenProcessHandler.ProcessImageCountOCRDebug(ThresholdMinR, ThresholdMinG, ThresholdMinB, ThresholdMaxR, ThresholdMaxG, ThresholdMaxB);
            }
        }

        public int ThresholdMaxR
        {
            get => _thresholdMaxR;
            set
            {
                _thresholdMaxR = value;
                _screenProcessHandler.ProcessImageCountOCRDebug(ThresholdMinR, ThresholdMinG, ThresholdMinB, ThresholdMaxR, ThresholdMaxG, ThresholdMaxB);
            }
        }

        public int ThresholdMaxG
        {
            get => _thresholdMaxG;
            set
            {
                _thresholdMaxG = value;
                _screenProcessHandler.ProcessImageCountOCRDebug(ThresholdMinR, ThresholdMinG, ThresholdMinB, ThresholdMaxR, ThresholdMaxG, ThresholdMaxB);
            }
        }

        public int ThresholdMaxB
        {
            get => _thresholdMaxB;
            set
            {
                _thresholdMaxB = value;
                _screenProcessHandler.ProcessImageCountOCRDebug(ThresholdMinR, ThresholdMinG, ThresholdMinB, ThresholdMaxR, ThresholdMaxG, ThresholdMaxB);
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

        public BitmapSource? OcrImageCountRaw
        {
            get => _ocrImageCountRaw;
            set
            {
                _ocrImageCountRaw = value;
                RaisePropertyChanged(nameof(OcrImageCountRaw));
            }
        }

        #endregion

        // Start of Events region

        #region Events

        private void HandleOcrImageCountReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                OcrImageCountRaw = Helpers.ScreenCapture.ImageSourceFromBitmap(_screenProcessHandler.OcrImageCountRaw);
                OcrImageCount = Helpers.ScreenCapture.ImageSourceFromBitmap(_screenProcessHandler.OcrImageCount);
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
