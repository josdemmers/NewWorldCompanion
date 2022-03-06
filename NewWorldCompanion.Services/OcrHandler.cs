using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Drawing;
using TesserNet;

namespace NewWorldCompanion.Services
{
    public class OcrHandler : IOcrHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IScreenProcessHandler _screenProcessHandler;

        private bool _isBusy = false;
        private string _ocrText = string.Empty;

        // Start of Constructor region

        #region Constructor

        public OcrHandler(IEventAggregator eventAggregator, IScreenProcessHandler screenProcessHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OcrImageReadyEvent>().Subscribe(HandleOcrImageReadyEvent);

            // Init services
            _screenProcessHandler = screenProcessHandler;

        }

        #endregion

        // Start of Properties region

        #region Properties

        public string OcrText { get => _ocrText; set => _ocrText = value; }

        #endregion

        // Start of Events region

        #region Events

        private void HandleOcrImageReadyEvent()
        {
            if (!_isBusy)
            {
                _isBusy = true;
                if (_screenProcessHandler.OcrImage != null)
                {
                    try
                    {
                        Image image = Image.FromFile(@"ocrimages\itemname.png");
                        Tesseract tesseract = new Tesseract();
                        OcrText = tesseract.Read(image).Trim().Replace('\n', ' ');

                        image.Dispose();
                        tesseract.Dispose();

                        _eventAggregator.GetEvent<OcrTextReadyEvent>().Publish();
                        _eventAggregator.GetEvent<OverlayShowEvent>().Publish();
                    }
                    catch (Exception) 
                    {
                        _isBusy = false;
                    }
                }
                _isBusy = false;
            }
        }

        #endregion

        // Start of Methods region

        #region Methods

        #endregion
    }
}
