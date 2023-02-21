using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NewWorldCompanion.ViewModels.Tabs.Items
{
    public class NamedItemsViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsManager _settingsManager;
        private readonly INewWorldDataStore _newWorldDataStore;
        private readonly IStorageManager _storageManager;

        private ObservableCollection<NamedItem> _items = new ObservableCollection<NamedItem>();

        private string _itemNameFilter = string.Empty;
        private NamedItem _selectedItem = new();
        private bool _toggleFilter = false;

        // Start of Constructor region

        #region Constructor

        public NamedItemsViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager, INewWorldDataStore newWorldDataStore, IStorageManager storageManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<NewWorldDataStoreUpdated>().Subscribe(HandleNewWorldDataStoreUpdatedEvent);
            _eventAggregator.GetEvent<StorageManagerUpdated>().Subscribe(HandleStorageManagerUpdatedEvent);

            // Init services
            _settingsManager = settingsManager;
            _newWorldDataStore = newWorldDataStore; 
            _storageManager = storageManager;

            // Init View commands
            VisitNwdbCommand = new DelegateCommand<object>(VisitNwdbExecute);

            // Init filter views
            CreateItemsFilteredView();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand<object> VisitNwdbCommand { get; }

        public ObservableCollection<NamedItem> Items { get => _items; set => _items = value; }
        public ListCollectionView? ItemsFiltered { get; private set; }

        public bool FilterTier2
        {
            get => _settingsManager.Settings.NamedItemsFilterTier2;
            set
            {
                _settingsManager.Settings.NamedItemsFilterTier2 = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterTier2));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterTier3
        {
            get => _settingsManager.Settings.NamedItemsFilterTier3;
            set
            {
                _settingsManager.Settings.NamedItemsFilterTier3 = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterTier3));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterTier4
        {
            get => _settingsManager.Settings.NamedItemsFilterTier4;
            set
            {
                _settingsManager.Settings.NamedItemsFilterTier4 = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterTier4));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterTier5
        {
            get => _settingsManager.Settings.NamedItemsFilterTier5;
            set
            {
                _settingsManager.Settings.NamedItemsFilterTier5 = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterTier5));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterItemClassArmor
        {
            get => _settingsManager.Settings.NamedItemsFilterItemClassArmor;
            set
            {
                _settingsManager.Settings.NamedItemsFilterItemClassArmor = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterItemClassArmor));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterItemClassJewelry
        {
            get => _settingsManager.Settings.NamedItemsFilterItemClassJewelry;
            set
            {
                _settingsManager.Settings.NamedItemsFilterItemClassJewelry = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterItemClassJewelry));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterItemClassWeapon
        {
            get => _settingsManager.Settings.NamedItemsFilterItemClassWeapon;
            set
            {
                _settingsManager.Settings.NamedItemsFilterItemClassWeapon = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterItemClassWeapon));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterStorageCollected
        {
            get => _settingsManager.Settings.NamedItemsFilterStorageCollected;
            set
            {
                _settingsManager.Settings.NamedItemsFilterStorageCollected = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterStorageCollected));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterStorageMissing
        {
            get => _settingsManager.Settings.NamedItemsFilterStorageMissing;
            set
            {
                _settingsManager.Settings.NamedItemsFilterStorageMissing = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterStorageMissing));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterStorageDuplicates
        {
            get => _settingsManager.Settings.NamedItemsFilterStorageDuplicates;
            set
            {
                _settingsManager.Settings.NamedItemsFilterStorageDuplicates = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterStorageDuplicates));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterBindOnEquip
        {
            get => _settingsManager.Settings.NamedItemsFilterBindOnEquip;
            set
            {
                _settingsManager.Settings.NamedItemsFilterBindOnEquip = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterBindOnEquip));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public bool FilterBindOnPickup
        {
            get => _settingsManager.Settings.NamedItemsFilterBindOnPickup;
            set
            {
                _settingsManager.Settings.NamedItemsFilterBindOnPickup = value;
                _settingsManager.SaveSettings();
                RaisePropertyChanged(nameof(FilterBindOnPickup));

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ItemsFiltered?.Refresh();
                });
            }
        }

        public string ItemNameFilter
        {
            get => _itemNameFilter;
            set
            {
                _itemNameFilter = value;
                RaisePropertyChanged(nameof(ItemNameFilter));
                ItemsFiltered?.Refresh();

                if (ItemsFiltered?.Count == 1)
                {
                    var selection = ItemsFiltered.GetItemAt(0) as NamedItem;
                    if (selection != null)
                    {
                        SelectedItem = selection;
                    }
                }
            }
        }

        public NamedItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value, () => { RaisePropertyChanged(nameof(SelectedItem)); });
            }
        }

        public bool ToggleFilter
        {
            get => _toggleFilter;
            set
            {
                _toggleFilter = value;
                RaisePropertyChanged(nameof(ToggleFilter));
            }
        }

        #endregion

        // Start of Events region

        #region Events

        private void HandleNewWorldDataStoreUpdatedEvent()
        {
            UpdateNamedItems();
            UpdateStorageInfo();
        }

        private void HandleStorageManagerUpdatedEvent()
        {
            UpdateStorageInfo();
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void CreateItemsFilteredView()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                ItemsFiltered = new ListCollectionView(Items)
                {
                    Filter = FilterItems
                };
                // Sorting
                ItemsFiltered.SortDescriptions.Add(new SortDescription("Localisation", ListSortDirection.Ascending));
                //ItemsFiltered.SortDescriptions.Add(new SortDescription("Storage", ListSortDirection.Ascending));
                //ItemsFiltered.SortDescriptions.Add(new SortDescription("Tier", ListSortDirection.Ascending));

                // Live sorting
                ItemsFiltered.LiveSortingProperties.Add("Localisation");
                //ItemsFiltered.LiveSortingProperties.Add("Storage");
                //ItemsFiltered.LiveSortingProperties.Add("Tier");
                ItemsFiltered.IsLiveSorting = true;
            });
        }

        private bool FilterItems(object itemsObj)
        {
            var allowed = true;
            if (itemsObj == null) return false;
            NamedItem item = (NamedItem)itemsObj;

            switch(item.Tier)
            {
                case 2:
                    allowed = FilterTier2;
                    break;
                case 3:
                    allowed = FilterTier3;
                    break;
                case 4:
                    allowed = FilterTier4;
                    break;
                case 5:
                    allowed = FilterTier5;
                    break;

            }

            if (allowed)
            {
                allowed = item.ItemClass.ToLower().Contains("armor") && !FilterItemClassArmor ? false : true;
            }

            if (allowed)
            {
                allowed = item.ItemClass.ToLower().Contains("jewelry") && !FilterItemClassJewelry ? false : true;
            }

            if (allowed)
            {
                allowed = item.ItemClass.ToLower().Contains("weapon") && !FilterItemClassWeapon ? false : true;
            }

            if (allowed)
            {
                var allowedCollected = item.Storage.Length > 0 && FilterStorageCollected ? true : false;
                var allowedMissing = string.IsNullOrWhiteSpace(item.Storage) && FilterStorageMissing ? true : false;
                var allowedDuplicates = item.Storage.Contains(",") && FilterStorageDuplicates ? true : false;

                allowed = allowedCollected || allowedMissing || allowedDuplicates;
            }

            if (allowed)
            {
                allowed = item.BindOnEquip && !FilterBindOnEquip ? false : true;
            }

            if (allowed)
            {
                allowed = item.BindOnPickup && !FilterBindOnPickup ? false : true;
            }

            if (allowed)
            {
                allowed = string.IsNullOrWhiteSpace(ItemNameFilter) ? true : item.Localisation.ToLower().Contains(ItemNameFilter.ToLower());
            }

            return allowed;
        }

        private void VisitNwdbExecute(object url)
        {
            Process.Start(new ProcessStartInfo(url as string ?? string.Empty) { UseShellExecute = true });
        }

        private void UpdateNamedItems()
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Items.Clear();
                    Items.AddRange(_newWorldDataStore.GetNamedItems());
                });
            }
        }

        private void UpdateStorageInfo()
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    // Reset storage info
                    foreach (var item in Items)
                    {
                        item.Storage = string.Empty;
                    }

                    // Update storage info
                    foreach (var item in _storageManager.Items)
                    {
                        if(Items.Any(i => i.ItemID.Equals(item.ItemID)))
                        {
                            var namedItem = Items.First(i => i.ItemID.Equals(item.ItemID));
                            namedItem.Storage = (namedItem.Storage.Length > 0) ? $"{namedItem.Storage}, {item.Storage}" : $"{item.Storage}";
                        }
                    }
                });
            }
        }

        #endregion

    }
}
