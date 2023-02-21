using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using NewWorldCompanion.Services;
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

namespace NewWorldCompanion.ViewModels.Tabs.Config
{
    public class ConfigOverlayViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsManager _settingsManager;
        private readonly INewWorldDataStore _newWorldDataStore;
        private readonly IPriceManager _priceManager;
        private readonly IRelatedPriceManager _relatedPriceManager;

        private ObservableCollection<PriceServer> _servers = new ObservableCollection<PriceServer>();
        private ObservableCollection<OverlayResource> _overlayResources = new ObservableCollection<OverlayResource>();

        private OverlayResource _selectedOverlayResource = new OverlayResource();
        private bool _toggleTooltip = false;
        private bool _toggleExtendedTooltip = false;
        private bool _toggleNamedItemsTooltip = false;
        private int _serverIndex = 0;
        private string _itemNameFilter = string.Empty;

        // Start of Constructor region

        #region Constructor

        public ConfigOverlayViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager, INewWorldDataStore newWorldDataStore, IPriceManager priceManager, IRelatedPriceManager relatedPriceManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<PriceServerListUpdatedEvent>().Subscribe(HandlePriceServerListUpdatedEvent);
            _eventAggregator.GetEvent<NewWorldDataStoreUpdated>().Subscribe(HandleNewWorldDataStoreUpdatedEvent);

            // Init services
            _settingsManager = settingsManager;
            _newWorldDataStore = newWorldDataStore;
            _priceManager = priceManager;
            _relatedPriceManager = relatedPriceManager;

            // Init filter views
            CreateOverlayResourcesFilteredView();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public ObservableCollection<PriceServer> Servers { get => _servers; set => _servers = value; }
        public ObservableCollection<OverlayResource> OverlayResources { get => _overlayResources; set => _overlayResources = value; }
        public ListCollectionView? OverlayResourcesFiltered { get; private set; }

        public OverlayResource SelectedOverlayResource
        { 
            get => _selectedOverlayResource; 
            set
            {
                SetProperty(ref _selectedOverlayResource, value, () => { RaisePropertyChanged(nameof(SelectedOverlayResource)); });
            }   
        }

        public bool ToggleTooltip
        {
            get => _toggleTooltip;
            set
            {
                _toggleTooltip = value;
                RaisePropertyChanged(); 
                if (!ToggleTooltip)
                {
                    ToggleExtendedTooltip = false;
                }

                _settingsManager.Settings.TooltipEnabled = value;
                _settingsManager.SaveSettings();
            }
        }

        public bool ToggleExtendedTooltip
        {
            get => _toggleExtendedTooltip;
            set
            {
                _toggleExtendedTooltip = value;
                RaisePropertyChanged();
                if (_toggleExtendedTooltip)
                {
                    ToggleTooltip = true;
                }

                _settingsManager.Settings.ExtendedTooltipEnabled = value;
                _settingsManager.SaveSettings();
            }
        }

        public bool ToggleNamedItemsTooltip
        {
            get => _toggleNamedItemsTooltip;
            set
            {
                _toggleNamedItemsTooltip = value;
                RaisePropertyChanged();

                _settingsManager.Settings.NamedItemsTooltipEnabled = value;
                _settingsManager.SaveSettings();
            }
        }

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
                    _settingsManager.Settings.PriceServerId = Servers[value].Id;
                    _settingsManager.SaveSettings();
                }
            }
        }

        public string ItemNameFilter
        {
            get => _itemNameFilter;
            set
            {
                _itemNameFilter = value;
                RaisePropertyChanged(nameof(ItemNameFilter));

                OverlayResourcesFiltered?.Refresh();

                if (OverlayResourcesFiltered?.Count == 1)
                {
                    SelectedOverlayResource = (OverlayResource)OverlayResourcesFiltered.GetItemAt(0);
                }
            }
        }

        #endregion

        // Start of Events region

        #region Events

        private void HandlePriceServerListUpdatedEvent()
        {
            updateServerList();
        }

        private void HandleNewWorldDataStoreUpdatedEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                // Init related prices
                InitOverlayResources();
                OverlayResourcesFiltered?.Refresh();
            });
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void updateServerList()
        {
            Servers.Clear();
            Servers.AddRange(_priceManager.Servers);

            int serverIndex = Servers.ToList().FindIndex(s => s.Id == _settingsManager.Settings.PriceServerId);
            if (serverIndex != -1)
            {
                ServerIndex = serverIndex;
            }
        }

        private void CreateOverlayResourcesFilteredView()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                OverlayResourcesFiltered = new ListCollectionView(OverlayResources)
                {
                    Filter = FilterOverlayResources
                };
            });
        }

        private bool FilterOverlayResources(object overlayResourceObj)
        {
            var allowed = false;
            if (overlayResourceObj == null) return false;

            OverlayResource overlayResource = (OverlayResource)overlayResourceObj;

            allowed = overlayResource.Recipes.Any();
            if (allowed)
            {
                allowed = string.IsNullOrWhiteSpace(ItemNameFilter) ? true : overlayResource.RawResource.NameLocalised.ToLower().Contains(ItemNameFilter.ToLower());
            }

            return allowed;
        }

        private void InitOverlayResources()
        {
            // Load extended tooltip toggle
            ToggleTooltip = _settingsManager.Settings.TooltipEnabled;
            ToggleExtendedTooltip = _settingsManager.Settings.ExtendedTooltipEnabled;
            ToggleNamedItemsTooltip = _settingsManager.Settings.NamedItemsTooltipEnabled;

            // Get interesting resources for extended overlay
            var overlayResources = _newWorldDataStore.GetOverlayResources();
            overlayResources.Sort((x, y) =>
            {
                int result = string.Compare(x.ItemClass, y.ItemClass, StringComparison.Ordinal);
                result = result != 0 ? result : x.Tier - y.Tier;
                return result != 0 ? result : string.Compare(x.ItemID, y.ItemID, StringComparison.Ordinal);
            });

            foreach (var overlayResource in overlayResources)
            {
                _overlayResources.Add(new OverlayResource
                {
                    RawResource = new RawResource { ItemID = overlayResource.ItemID, Name = overlayResource.Name }
                });
            }

            // Add related recipes
            foreach (var overlayResource in _overlayResources)
            {
                var relatedRecipes = _newWorldDataStore.GetRelatedRecipes(overlayResource.RawResource.ItemID);
                foreach (var recipe in relatedRecipes)
                {
                    overlayResource.Recipes.Add(new RawResourceRecipe
                    {
                        ItemID = recipe.ItemID,
                        ItemIDRawResource = overlayResource.RawResource.ItemID
                    });
                }
            }

            // Load related recipes config
            foreach (var persistableOverlayResource in _relatedPriceManager.PersistableOverlayResources)
            {
                var overlayResource = _overlayResources.FirstOrDefault(resource => resource.RawResource.ItemID.Equals(persistableOverlayResource.ItemId));
                if (overlayResource != null)
                {
                    foreach (var persistableOverlayResourceRecipe in persistableOverlayResource.PersistableOverlayResourceRecipes)
                    {
                        var overlayResourceRecipe = overlayResource.Recipes.FirstOrDefault(recipe => recipe.ItemID.Equals(persistableOverlayResourceRecipe.ItemId));
                        if (overlayResourceRecipe != null)
                        {
                            overlayResourceRecipe.IsVisible = persistableOverlayResourceRecipe.IsVisible;
                        }
                    }
                }
            }
        }

        #endregion
    }

    public class OverlayResource
    {
        public RawResource RawResource { get; set; } = new RawResource();
        public List<RawResourceRecipe> Recipes { get; set; } = new List<RawResourceRecipe>();
    }

    public class RawResource
    {
        private readonly INewWorldDataStore _newWorldDataStore;

        public RawResource()
        {
            _newWorldDataStore = (INewWorldDataStore)Prism.Ioc.ContainerLocator.Container.Resolve(typeof(INewWorldDataStore));
        }

        /// <value>Unique identifier for items. Nwdb uses this to identify items</value>
        public string ItemID { get; set; } = string.Empty;
        /// <value>Contains master name for localisation</value>
        public string Name { get; set; } = string.Empty;
        /// <value>Localised name</value>
        public string NameLocalised 
        { 
            get
            {
                return _newWorldDataStore.GetItemLocalisation(Name);
            }
        }
    }

    public class RawResourceRecipe
    {
        private readonly INewWorldDataStore _newWorldDataStore;
        private readonly IRelatedPriceManager _relatedPriceManager;

        private bool _isVisible = false;

        public RawResourceRecipe()
        {
            _newWorldDataStore = (INewWorldDataStore)Prism.Ioc.ContainerLocator.Container.Resolve(typeof(INewWorldDataStore));
            _relatedPriceManager = (IRelatedPriceManager)Prism.Ioc.ContainerLocator.Container.Resolve(typeof(IRelatedPriceManager));
        }

        /// <value>Show or hide recipe</value>
        public bool IsVisible 
        { 
            get => _isVisible;
            set
            {
                _isVisible = value;
                _relatedPriceManager.SetRawResourceRecipeVisibility(ItemIDRawResource, ItemID, IsVisible);
            }
        }

        /// <value>Crafted item ItemId</value> 
        public string ItemID { get; set; } = string.Empty;
        /// <value>RawResource ItemId of the RawResource linked to this recipe.</value>
        public string ItemIDRawResource { get; set; } = string.Empty;
        /// <value>Localised name</value>
        public string NameLocalised
        {
            get
            {
                return _newWorldDataStore.GetItemLocalisation($"@{ItemID}_MasterName");
            }
        }
    }

}
