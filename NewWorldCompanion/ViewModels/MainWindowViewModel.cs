using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using NewWorldCompanion.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.IO;
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
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly INewWorldDataStore _newWorldDataStore;
        private readonly ICraftingRecipeManager _craftingRecipeManager;

        private string _windowTitle = $"New World Companion v{Assembly.GetExecutingAssembly().GetName().Version}";

        // Start of Constructor region

        #region Constructor

        public MainWindowViewModel(IEventAggregator eventAggregator, ILogger<MainWindowViewModel> logger, ISettingsManager settingsManager, IOverlayHandler overlayHandler, IVersionManager versionManager, IDialogCoordinator dialogCoordinator, INewWorldDataStore newWorldDataStore, ICraftingRecipeManager craftingRecipeManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<VersionInfoUpdatedEvent>().Subscribe(HandleVersionInfoUpdatedEvent);
            _eventAggregator.GetEvent<NewWorldDataStoreUpdated>().Subscribe(HandleDataReadyEvent);
            _eventAggregator.GetEvent<CraftingRecipeManagerUpdated>().Subscribe(HandleDataReadyEvent);
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Subscribe(HandleDataStatusUpdatedEvent);

            // Init logger
            _logger = logger;

            // Init services
            _settingsManager = settingsManager;
            _overlayHandler = overlayHandler;
            _versionManager = versionManager;
            _dialogCoordinator = dialogCoordinator;
            _newWorldDataStore = newWorldDataStore;
            _craftingRecipeManager = craftingRecipeManager;

            // Init View commands
            LaunchNWCOnGitHubCommand = new DelegateCommand(LaunchNWCOnGitHubExecute);
            LaunchKofiCommand = new DelegateCommand(LaunchKofiExecute);
        }

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand LaunchNWCOnGitHubCommand { get; }
        public DelegateCommand LaunchKofiCommand { get; }
        
        public bool DebugModeActive { get => _settingsManager.Settings.DebugModeActive; }
        public string WindowTitle { get => _windowTitle; set => _windowTitle = value; }
        public bool WaitingForData => !(_newWorldDataStore.Available && _craftingRecipeManager.Available);
        public string DataStatusItemDefinitions => _newWorldDataStore.LoadStatusItemDefinitions;
        public string DataStatusCraftingRecipes => _newWorldDataStore.LoadStatusCraftingRecipes;
        public string DataStatusHouseItems => _newWorldDataStore.LoadStatusHouseItems;
        public string DataStatusLocalisation => _newWorldDataStore.LoadStatusLocalisation;

        #endregion

        // Start of Events region

        #region Events

        private void HandleVersionInfoUpdatedEvent()
        {
            if (!string.IsNullOrWhiteSpace(_versionManager.LatestVersion) &&
                !_versionManager.LatestVersion.Equals(_versionManager.CurrentVersion))
            {
                WindowTitle = $"New World Companion v{_versionManager.CurrentVersion} (v{_versionManager.LatestVersion} available)";

                // Ask for update when Updater.GitHub.exe exists.
                if (File.Exists("Updater.GitHub.exe"))
                {
                    _dialogCoordinator.ShowMessageAsync(this, $"Update", $"New version available, do you want to download v{_versionManager.LatestVersion}?", MessageDialogStyle.AffirmativeAndNegative).ContinueWith(t =>
                    {
                        if (t.Result == MessageDialogResult.Affirmative)
                        {
                            var app = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "NewWorldCompanion.exe";
                            app = Path.GetFileName(app);
                            Process.Start("Updater.GitHub.exe", $"--repo \"josdemmers/NewWorldCompanion\" --app \"{app}\"");
                        }
                    });
                }
                else
                {
                    _logger.LogWarning("Cannot update applicaiton, Updater.GitHub.exe not available.");
                }
            }
            else
            {
                WindowTitle = $"New World Companion v{Assembly.GetExecutingAssembly().GetName().Version}";
            }
            RaisePropertyChanged(nameof(WindowTitle));

            _logger.LogInformation(WindowTitle);
        }

        private void HandleDataReadyEvent()
        {
            RaisePropertyChanged(nameof(WaitingForData));
        }

        private void HandleDataStatusUpdatedEvent()
        {
            RaisePropertyChanged(nameof(DataStatusItemDefinitions));
            RaisePropertyChanged(nameof(DataStatusCraftingRecipes));
            RaisePropertyChanged(nameof(DataStatusHouseItems));
            RaisePropertyChanged(nameof(DataStatusLocalisation));
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void LaunchNWCOnGitHubExecute()
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://github.com/josdemmers/NewWorldCompanion/releases") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
            }
        }

        private void LaunchKofiExecute()
        {
            {
                try
                {
                    Process.Start(new ProcessStartInfo("https://ko-fi.com/josdemmers") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
                }
            }
        }

        #endregion

    }
}
