using NewWorldCompanion.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NewWorldCompanion.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsManager _settingsManager;
        private readonly IOverlayHandler _overlayHandler;

        // Start of Constructor region

        #region Constructor

        public MainWindowViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager, IOverlayHandler overlayHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init services
            _settingsManager = settingsManager;
            _overlayHandler = overlayHandler;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public bool DebugModeActive { get => _settingsManager.Settings.DebugModeActive; }

        #endregion

        // Start of Methods region

        #region Methods

        #endregion

    }
}
