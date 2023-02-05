using Microsoft.Extensions.Logging;
using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using TesserNet;

namespace NewWorldCompanion.Services
{
    public class OcrHandler : IOcrHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly INewWorldDataStore _newWorldDataStore;
        private readonly IScreenProcessHandler _screenProcessHandler;

        private readonly object nameLock = new object();
        private readonly object countLock = new object();

        private List<OcrMapping> _ocrMappings = new List<OcrMapping>();

        private string _ocrText = string.Empty;
        private string _ocrTextCount = string.Empty;
        private string _ocrTextCountRaw = string.Empty;

        // Start of Constructor region

        #region Constructor

        public OcrHandler(IEventAggregator eventAggregator, ILogger<OcrHandler> logger, INewWorldDataStore newWorldDataStore, IScreenProcessHandler screenProcessHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OcrImageReadyEvent>().Subscribe(HandleOcrImageReadyEvent);
            _eventAggregator.GetEvent<OcrImageCountReadyEvent>().Subscribe(HandleOcrImageCountReadyEvent);

            // Init logger
            _logger = logger;

            // Init services
            _newWorldDataStore = newWorldDataStore;
            _screenProcessHandler = screenProcessHandler;

            // Init ocr mappings
            UpdateOcrMappings();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public string OcrText { get => _ocrText; set => _ocrText = value; }
        public string OcrTextCount { get => _ocrTextCount; set => _ocrTextCount = value; }
        public string OcrTextCountRaw { get => _ocrTextCountRaw; set => _ocrTextCountRaw = value; }

        #endregion

        // Start of Events region

        #region Events

        private void HandleOcrImageReadyEvent()
        {
            lock (nameLock)
            {
                if (_screenProcessHandler.OcrImage != null && _newWorldDataStore.Available)
                {
                    try
                    {
                        Image image = Image.FromFile(@"ocrimages\itemname.png");
                        Tesseract tesseract = new Tesseract();
                        string ocrText = tesseract.Read(image).Trim().Replace('\n', ' ');
                        var mapping = _ocrMappings.FirstOrDefault(m => m.key.Equals(ocrText), new OcrMapping { key = ocrText, value = ocrText });
                        OcrText = mapping.value;
                        OcrText = string.IsNullOrWhiteSpace(OcrText) ? string.Empty : _newWorldDataStore.GetLevenshteinItemName(OcrText);

                        image.Dispose();
                        tesseract.Dispose();

                        _eventAggregator.GetEvent<OcrTextReadyEvent>().Publish();
                        _eventAggregator.GetEvent<OverlayShowEvent>().Publish();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
                    }
                }
            }
        }

        private void HandleOcrImageCountReadyEvent()
        {
            lock (countLock)
            {
                if (_screenProcessHandler.OcrImageCount != null)
                {
                    try
                    {
                        Image image = Image.FromFile(@"ocrimages\itemcount.png");
                        Tesseract tesseract = new Tesseract();
                        string ocrText = tesseract.Read(image).Trim().Replace('\n', ' ');
                        OcrTextCountRaw = ocrText;
                        // Remove non-numeric characters
                        ocrText = new string(ocrText.Where(c => char.IsDigit(c)).ToArray());
                        OcrTextCount = string.IsNullOrWhiteSpace(ocrText) ? OcrTextCount : ocrText;

                        image.Dispose();
                        tesseract.Dispose();

                        _eventAggregator.GetEvent<OcrTextCountReadyEvent>().Publish();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
                    }
                }
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
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
            }
        }

        #endregion
    }
}
