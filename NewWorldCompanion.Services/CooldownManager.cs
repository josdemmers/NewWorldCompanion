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
    public class CooldownManager : ICooldownManager
    {
        private readonly IEventAggregator _eventAggregator;

        private List<CooldownTimer> _cooldownTimers = new List<CooldownTimer>();

        // Start of Constructor region

        #region Constructor

        public CooldownManager(IEventAggregator eventAggregator)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init cooldowns
            LoadCooldowns();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public List<CooldownTimer> CooldownTimers { get => _cooldownTimers; }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods

        public void AddCooldown(CooldownTimer cooldownTimer)
        {
            _cooldownTimers.Add(cooldownTimer);
        }

        public void RemoveCooldown(CooldownTimer cooldownTimer)
        {
            _cooldownTimers.Remove(cooldownTimer);
        }

        private void LoadCooldowns()
        {
            _cooldownTimers.Clear();

            string fileName = "Config/Cooldowns.json";
            if (File.Exists(fileName))
            {
                using FileStream stream = File.OpenRead(fileName);
                _cooldownTimers = JsonSerializer.Deserialize<List<CooldownTimer>>(stream) ?? new List<CooldownTimer>();
            }

            // Sort list
            _cooldownTimers.Sort((x, y) =>
            {
                int result = x.RemainingTime - y.RemainingTime > TimeSpan.Zero ? 1 : -1;
                return result != 0 ? result : string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            });

            SaveCooldowns();
        }

        public void SaveCooldowns()
        {
            string fileName = "Config/Cooldowns.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonSerializer.Serialize(stream, CooldownTimers, options);
        }

        #endregion

    }
}
