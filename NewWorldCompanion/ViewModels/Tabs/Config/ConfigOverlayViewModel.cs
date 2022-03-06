using NewWorldCompanion.Entities;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.ViewModels.Tabs.Config
{
    public class ConfigOverlayViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsManager _settingsManager;
        private readonly IPriceManager _priceManager;

        private ObservableCollection<PriceServer> _servers = new ObservableCollection<PriceServer>();

        private int _serverIndex = 0;

        // Start of Constructor region

        #region Constructor

        public ConfigOverlayViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager, IPriceManager priceManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init services
            _settingsManager = settingsManager;
            _priceManager = priceManager;

            // Init servers
            updateServerList();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public ObservableCollection<PriceServer> Servers { get => _servers; set => _servers = value; }

        public int ServerIndex
        {
            get 
            { 
                return _serverIndex;
            }
            set
            {
                _serverIndex = value;
                RaisePropertyChanged(nameof(ServerIndex));

                if (_serverIndex >= 0 && Servers.Count > _serverIndex)
                {
                    _settingsManager.Settings.ServerId = Servers[value].Id;
                    _settingsManager.SaveSettings();
                }
            }
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods


        private void updateServerList()
        {
            Servers.Clear();
            Servers.AddRange(_priceManager.Servers);

            int serverIndex = Servers.ToList().FindIndex(s => s.Id == _settingsManager.Settings.ServerId);
            if (serverIndex != -1)
            {
                ServerIndex = serverIndex;
            }
        }

        #endregion
    }
}
