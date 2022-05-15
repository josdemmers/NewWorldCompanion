using Microsoft.Extensions.Logging;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace NewWorldCompanion.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly IOverlayHandler _overlayHandler;
        private readonly IVersionManager _versionManager;

        private string _windowTitle = $"New World Companion v{Assembly.GetExecutingAssembly().GetName().Version}";

        // Start of Constructor region

        #region Constructor

        public MainWindowViewModel(IEventAggregator eventAggregator, ILogger<MainWindowViewModel> logger, ISettingsManager settingsManager, IOverlayHandler overlayHandler, IVersionManager versionManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<VersionInfoUpdatedEvent>().Subscribe(HandleVersionInfoUpdatedEvent);

            // Init logger
            _logger = logger;

            // Init services
            _settingsManager = settingsManager;
            _overlayHandler = overlayHandler;
            _versionManager = versionManager;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public bool DebugModeActive { get => _settingsManager.Settings.DebugModeActive; }
        public string WindowTitle { get => _windowTitle; set => _windowTitle = value; }

        #endregion

        // Start of Events region

        #region Events

        private void HandleVersionInfoUpdatedEvent()
        {
            if (!string.IsNullOrWhiteSpace(_versionManager.LatestVersion) &&
                !_versionManager.LatestVersion.Equals(_versionManager.CurrentVersion))
            {
                WindowTitle = $"New World Companion v{_versionManager.CurrentVersion} (v{_versionManager.LatestVersion} available)";
            }
            else
            {
                WindowTitle = $"New World Companion v{Assembly.GetExecutingAssembly().GetName().Version}";
            }
            RaisePropertyChanged(nameof(WindowTitle));

            _logger.LogInformation(WindowTitle);
        }

        #endregion

        // Start of Methods region

        #region Methods

        #endregion

    }
}
