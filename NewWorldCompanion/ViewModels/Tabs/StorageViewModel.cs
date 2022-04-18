using NewWorldCompanion.Constants;
using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Extensions;
using NewWorldCompanion.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NewWorldCompanion.ViewModels.Tabs
{
    public class StorageViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        private readonly INewWorldDataStore _newWorldDataStore;
        private readonly IOcrHandler _ocrHandler;
        private readonly IStorageManager _storageManager;

        private ObservableCollection<Storage> _storages = new ObservableCollection<Storage>();

        private string _itemName = string.Empty;
        private string _itemNameFilter = string.Empty;
        private Item _selectedItem = new();
        private int _storageIndex = 0;
        private bool _toggleRefresh = false;

        // Start of Constructor region

        #region Constructor

        public StorageViewModel(IEventAggregator eventAggregator, INewWorldDataStore newWorldDataStore, IOcrHandler ocrHandler, IStorageManager storageManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OcrTextReadyEvent>().Subscribe(HandleOcrTextReadyEvent);

            // Init services
            _newWorldDataStore = newWorldDataStore;
            _ocrHandler = ocrHandler;
            _storageManager = storageManager;

            // Init View commands
            ResetStorageCommand = new DelegateCommand(ResetStorageExecute, CanResetStorageExecute);
            StorageCheckBoxCommand = new DelegateCommand(StorageCheckBoxExecute, CanStorageCheckBoxExecute);

            // Init filter views
            CreateItemsFilteredView();

            // Init storages
            InitStorage();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public DelegateCommand ResetStorageCommand { get; }
        public DelegateCommand StorageCheckBoxCommand { get; }
        
        public ObservableCollection<Storage> Storages { get => _storages; set => _storages = value; }
        public ObservableCollection<Item> Items { get => _storageManager.Items; }
        public ListCollectionView? ItemsFiltered { get; private set; }

        public int StorageIndex
        {
            get => _storageIndex;
            set
            {
                _storageIndex = value;
                RaisePropertyChanged(nameof(SelectedStorage));
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
                    SelectedItem = (Item)ItemsFiltered.GetItemAt(0);
                }
            }
        }

        public string SelectedStorage
        {
            get
            {
                if (_storageIndex >= 0 && _storageIndex < Storages.Count())
                {
                    return Storages[StorageIndex].Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value, () => { RaisePropertyChanged(nameof(SelectedItem)); });
            }
        }

        public bool ToggleRefresh
        {
            get => _toggleRefresh;
            set
            {
                _toggleRefresh = value;
                RaisePropertyChanged(nameof(ToggleRefresh));
            }
        }

        #endregion

        // Start of Events region

        #region Events

        private void HandleOcrTextReadyEvent()
        {
            if (ToggleRefresh)
            {
                // Recording active
                _itemName = _ocrHandler.OcrText;
                string itemId = _newWorldDataStore.GetItemId(_itemName);
                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    // Item valid
                    var itemDefinition = _newWorldDataStore.GetItem(itemId);
                    if (itemDefinition != null)
                    {
                        // Item valid
                        if (_storageIndex >= 0 && _storageIndex < Storages.Count())
                        {
                            // Inventory selected
                            if (Items.Any(i => i.Storage.Equals(SelectedStorage) && i.ItemID.Equals(itemId)))
                            {
                                // Update item
                                var item = Items.FirstOrDefault(i => i.Storage.Equals(SelectedStorage) && i.ItemID.Equals(itemId));
                                if (item != null)
                                {
                                    // TODO item count
                                    item.Count = 0;
                                }
                            }
                            else
                            {
                                Application.Current?.Dispatcher?.Invoke(() =>
                                {
                                    // Add item
                                    Items.Add(new Item()
                                    {
                                        // TODO item count
                                        Count = 0,
                                        ItemID = itemId,
                                        Name = itemDefinition.Name,
                                        Localisation = _itemName,
                                        Storage = SelectedStorage
                                    });
                                });
                            }
                        }
                    }
                }
                _storageManager.SaveStorage();
            }
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
                ItemsFiltered.SortDescriptions.Add(new SortDescription("Storage", ListSortDirection.Ascending));
                ItemsFiltered.SortDescriptions.Add(new SortDescription("Localisation", ListSortDirection.Ascending));

                // Live sorting
                ItemsFiltered.LiveSortingProperties.Add("Storage");
                ItemsFiltered.LiveSortingProperties.Add("Localisation");
                ItemsFiltered.IsLiveSorting = true;
            });
        }

        private bool FilterItems(object itemsObj)
        {
            var allowed = false;
            if (itemsObj == null) return false;
            Item item = (Item)itemsObj;

            switch(item.Storage)
            {
                case StorageLocationConstants.Brightwood:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.CutlassKeys:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.EbonscaleReach:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.EdengroveLastStand:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.EdengroveValorHold:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.Everfall:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.FirstLight:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.GreatCleaveCleavesPoint:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.GreatCleaveEastburn:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.MonarchsBluffs:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.Mourningdale:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.Reekwater:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.RestlessShore:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.ShatteredMountainMountainhome:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.ShatteredMountainMountainrise:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.WeaversFen:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
                case StorageLocationConstants.Windsward:
                    allowed = Storages.FirstOrDefault(s => s.Name.Equals(item.Storage, StringComparison.OrdinalIgnoreCase))?.IsEnabled ?? false;
                    break;
            }

            if (allowed)
            {
                allowed = string.IsNullOrWhiteSpace(ItemNameFilter) ? true : item.Localisation.ToLower().Contains(ItemNameFilter.ToLower());
            }
            
            return allowed;
        }

        private void InitStorage()
        {
            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.Brightwood,
                IsEnabled = true
            });
            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.CutlassKeys,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.EbonscaleReach,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.EdengroveLastStand,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.EdengroveValorHold,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.Everfall,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.FirstLight,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.GreatCleaveCleavesPoint,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.GreatCleaveEastburn,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.MonarchsBluffs,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.Mourningdale,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.Reekwater,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.RestlessShore,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.ShatteredMountainMountainhome,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.ShatteredMountainMountainrise,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.WeaversFen,
                IsEnabled = true
            });

            Storages.Add(new Storage()
            {
                Name = StorageLocationConstants.Windsward,
                IsEnabled = true
            });

            ItemsFiltered?.Refresh();
        }

        private bool CanResetStorageExecute()
        {
            return true;
        }

        private void ResetStorageExecute()
        {
            if (!string.IsNullOrWhiteSpace(SelectedStorage))
            {
                Items.Remove(item => item.Storage.Equals(SelectedStorage));
                _storageManager.SaveStorage();
            }
        }

        private bool CanStorageCheckBoxExecute()
        {
            return true;
        }

        private void StorageCheckBoxExecute()
        {
            ItemsFiltered?.Refresh();
        }

        #endregion
    }
}
