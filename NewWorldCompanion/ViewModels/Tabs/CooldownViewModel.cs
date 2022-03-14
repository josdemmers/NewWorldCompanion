using MahApps.Metro.Controls.Dialogs;
using NewWorldCompanion.Entities;
using NewWorldCompanion.Interfaces;
using NewWorldCompanion.ViewModels.Dialogs;
using NewWorldCompanion.Views.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NewWorldCompanion.ViewModels.Tabs
{
    public class CooldownViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ICooldownManager _cooldownManager;

        private ObservableCollection<CooldownTimer> _cooldownTimers = new ObservableCollection<CooldownTimer>();

        private string _cooldownTimerName = string.Empty;
        private CooldownTimer _selectedCooldownTimer = new CooldownTimer();

        // Start of Constructor region

        #region Constructor

        public CooldownViewModel(IEventAggregator eventAggregator, IDialogCoordinator dialogCoordinator, ICooldownManager cooldownManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init services
            _dialogCoordinator = dialogCoordinator;
            _cooldownManager = cooldownManager;

            // Init View commands
            AddCooldownTimerCommand = new DelegateCommand(AddCooldownTimerExecute, CanAddCooldownTimerExecute);
            RefreshCooldownCommand = new DelegateCommand<object>(RefreshCooldownExecute);
            ConfigCooldownCommand = new DelegateCommand<object>(ConfigCooldownExecute);
            DeleteCooldownCommand = new DelegateCommand<object>(DeleteCooldownCommandExecute);

            // Update cooldown timers
            var updateCooldownTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
                IsEnabled = true
            };
            updateCooldownTimer.Tick += UpdateCooldownTimer_Tick;

            // DispatcherTimer after ctor
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
                IsEnabled = true
            };
            dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand AddCooldownTimerCommand { get; }
        public DelegateCommand<object> RefreshCooldownCommand { get; }
        public DelegateCommand<object> ConfigCooldownCommand { get; }
        public DelegateCommand<object> DeleteCooldownCommand { get; }

        public ObservableCollection<CooldownTimer> CooldownTimers { get => _cooldownTimers; set => _cooldownTimers = value; }

        public string CooldownTimerName
        {
            get => _cooldownTimerName;
            set
            {
                SetProperty(ref _cooldownTimerName, value, () => { RaisePropertyChanged(nameof(CooldownTimerName)); });
                AddCooldownTimerCommand?.RaiseCanExecuteChanged();
            }
        }

        public CooldownTimer SelectedCooldownTimer { get => _selectedCooldownTimer; set => _selectedCooldownTimer = value; }

        #endregion

        // Start of Events region

        #region Events

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            (sender as System.Windows.Threading.DispatcherTimer)?.Stop();
            UpdateCooldowns();
        }

        private void UpdateCooldownTimer_Tick(object? sender, EventArgs e)
        {
            CollectionViewSource.GetDefaultView(CooldownTimers).Refresh();
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void UpdateCooldowns()
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    CooldownTimers.Clear();
                    CooldownTimers.AddRange(_cooldownManager.CooldownTimers);
                });
            }
        }

        private bool CanAddCooldownTimerExecute()
        {
            return !string.IsNullOrWhiteSpace(CooldownTimerName) &&
                !_cooldownTimers.Any(cooldown => cooldown.Name.Equals(CooldownTimerName));
        }

        private void AddCooldownTimerExecute()
        {
            _cooldownTimers.Add(new CooldownTimer
            {
                Name = CooldownTimerName
            });
            AddCooldownTimerCommand?.RaiseCanExecuteChanged();
            _cooldownManager.AddCooldown(_cooldownTimers.Last());
            _cooldownManager.SaveCooldowns();
        }

        private void RefreshCooldownExecute(object obj)
        {
            ((CooldownTimer)obj).StartTime = DateTime.Now;
            CollectionViewSource.GetDefaultView(CooldownTimers).Refresh();
            _cooldownManager.SaveCooldowns();
        }

        private void DeleteCooldownCommandExecute(object obj)
        {
            CooldownTimers.Remove((CooldownTimer)obj);
            _cooldownManager.RemoveCooldown((CooldownTimer)obj);
            _cooldownManager.SaveCooldowns();
        }

        private async void ConfigCooldownExecute(object obj)
        {
            var cooldownConfigDialog = new CustomDialog() { Title = "Cooldown config" };
            var dataContext = new CooldownConfigViewModel(async instance =>
            {
                await cooldownConfigDialog.WaitUntilUnloadedAsync();
            },(CooldownTimer)obj);
            cooldownConfigDialog.Content = new CooldownConfigView() { DataContext = dataContext };
            await _dialogCoordinator.ShowMetroDialogAsync(this, cooldownConfigDialog);
            await cooldownConfigDialog.WaitUntilUnloadedAsync();
            _cooldownManager.SaveCooldowns();
        }

        #endregion
    }
}