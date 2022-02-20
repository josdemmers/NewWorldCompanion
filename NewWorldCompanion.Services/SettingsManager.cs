using NewWorldCompanion.Entities;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class SettingsManager : ISettingsManager
    {
        private readonly IEventAggregator _eventAggregator;

        private SettingsNWC _settings = new SettingsNWC();

        // Start of Constructor region

        #region Constructor

        public SettingsManager(IEventAggregator eventAggregator)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init settings
            LoadSettings();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public SettingsNWC Settings { get => _settings; set => _settings = value; }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region
        #region Methods

        private void LoadSettings()
        {
            string fileName = "Settings.json";
            if (File.Exists(fileName))
            {
                using FileStream stream = File.OpenRead(fileName);
                _settings = JsonSerializer.Deserialize<SettingsNWC>(stream) ?? new SettingsNWC();
            }

            SaveSettings();
        }

        public void SaveSettings()
        {
            string fileName = "Settings.json";
            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonSerializer.Serialize(stream, _settings, options);
        }

        #endregion
    }
}
