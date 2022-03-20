using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using TesserNet;

namespace NewWorldCompanion.Services
{
    public class OcrHandler : IOcrHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IScreenProcessHandler _screenProcessHandler;

        private List<OcrMapping> _ocrMappings = new List<OcrMapping>();

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

            // Init ocr mappings
            UpdateOcrMappings();
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
                        string ocrText = tesseract.Read(image).Trim().Replace('\n', ' ');
                        var mapping = _ocrMappings.FirstOrDefault(m => m.key.Equals(ocrText), new OcrMapping{ key = ocrText, value = ocrText });
                        OcrText = mapping.value;

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

        private void UpdateOcrMappings()
        {
            try
            {
                _ocrMappings.Clear();
                string fileName = "Config/OcrMappings.json";
                if (File.Exists(fileName))
                {
                    using FileStream stream = File.OpenRead(fileName);
                    _ocrMappings = JsonSerializer.Deserialize<List<OcrMapping>>(stream) ?? new List<OcrMapping>();
                }
            }
            catch (Exception){}
        }

        #endregion
    }
}
