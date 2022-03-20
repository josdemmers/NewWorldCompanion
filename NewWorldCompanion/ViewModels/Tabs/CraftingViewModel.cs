using NewWorldCompanion.Constants;
using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace NewWorldCompanion.ViewModels.Tabs
{
    public class CraftingViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ICraftingRecipeManager _craftingRecipeManager;
        private readonly IScreenCaptureHandler _screenCaptureHandler;
        private readonly IOcrHandler _ocrHandler;

        private ObservableCollection<CraftingRecipe> _craftingRecipes = new ObservableCollection<CraftingRecipe>();

        private BitmapSource? _imageArcana = null;
        private BitmapSource? _imageArmoring = null;
        private BitmapSource? _imageCooking = null;
        private BitmapSource? _imageEngineering = null;
        private BitmapSource? _imageFurnishing = null;
        private BitmapSource? _imageJewelcrafting = null;
        private BitmapSource? _imageWeaponsmithing = null;

        private CraftingRecipe _selectedCraftingRecipe = new CraftingRecipe();
        private bool _filterRecipeLearned = true;
        private bool _filterRecipeUnlearned = true;
        private bool _toggleArcana = true;
        private bool _toggleArmoring = true;
        private bool _toggleCooking = true;
        private bool _toggleEngineering = true;
        private bool _toggleFurnishing = true;
        private bool _toggleJewelcrafting = true;
        private bool _toggleWeaponsmithing = true;
        private bool _toggleRefresh = true;
        private int _counterArcana = 0;
        private int _counterArmoring = 0;
        private int _counterCooking = 0;
        private int _counterEngineering = 0;
        private int _counterFurnishing = 0;
        private int _counterJewelcrafting = 0;
        private int _counterWeaponsmithing = 0;
        private string _ItemNameFilter = string.Empty;

        // Start of Constructor region

        #region Constructor

        public CraftingViewModel(IEventAggregator eventAggregator, ICraftingRecipeManager craftingRecipeManager, IScreenCaptureHandler screenCaptureHandler, IOcrHandler ocrHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OcrTextReadyEvent>().Subscribe(HandleOcrTextReadyEvent);

            // Init services
            _craftingRecipeManager = craftingRecipeManager;
            _screenCaptureHandler = screenCaptureHandler;
            _ocrHandler = ocrHandler;

            // Init View commands
            CraftingRecipeLearnedCommand = new DelegateCommand<object>(CraftingRecipeLearnedExecute);
            VisitNwdbCommand = new DelegateCommand<object>(VisitNwdbExecute);
            CopyRecipeNameCommand = new DelegateCommand<object>(CopyRecipeNameExecute);

            // Init filter views
            CreateCraftingRecipesFilteredView();

            // Load crafting icons
            LoadCraftingIcons();

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

        public DelegateCommand<object> CraftingRecipeLearnedCommand { get; }
        public DelegateCommand<object> VisitNwdbCommand { get; }
        public DelegateCommand<object> CopyRecipeNameCommand { get; }

        public ObservableCollection<CraftingRecipe> CraftingRecipes { get => _craftingRecipes; set => _craftingRecipes = value; }
        public ListCollectionView? CraftingRecipesFiltered { get; private set; }

        public BitmapSource? ImageArcana { get => _imageArcana; set => SetProperty(ref _imageArcana, value, () => { RaisePropertyChanged(nameof(ImageArcana)); }); }
        public BitmapSource? ImageArmoring { get => _imageArmoring; set => SetProperty(ref _imageArmoring, value, () => { RaisePropertyChanged(nameof(ImageArmoring)); }); }
        public BitmapSource? ImageCooking { get => _imageCooking; set => SetProperty(ref _imageCooking, value, () => { RaisePropertyChanged(nameof(ImageCooking)); }); }
        public BitmapSource? ImageEngineering { get => _imageEngineering; set => SetProperty(ref _imageEngineering, value, () => { RaisePropertyChanged(nameof(ImageEngineering)); }); }
        public BitmapSource? ImageFurnishing { get => _imageFurnishing; set => SetProperty(ref _imageFurnishing, value, () => { RaisePropertyChanged(nameof(ImageFurnishing)); }); }
        public BitmapSource? ImageJewelcrafting { get => _imageJewelcrafting; set => SetProperty(ref _imageJewelcrafting, value, () => { RaisePropertyChanged(nameof(ImageJewelcrafting)); }); }
        public BitmapSource? ImageWeaponsmithing { get => _imageWeaponsmithing; set => SetProperty(ref _imageWeaponsmithing, value, () => { RaisePropertyChanged(nameof(ImageWeaponsmithing)); }); }

        public CraftingRecipe SelectedCraftingRecipe { get => _selectedCraftingRecipe; set => SetProperty(ref _selectedCraftingRecipe, value, () => { RaisePropertyChanged(nameof(SelectedCraftingRecipe)); }); }
        public int CounterArcana { get => _counterArcana; set => SetProperty(ref _counterArcana, value, () => { RaisePropertyChanged(nameof(CounterArcana)); }); }
        public int CounterArmoring { get => _counterArmoring; set => SetProperty(ref _counterArmoring, value, () => { RaisePropertyChanged(nameof(CounterArmoring)); }); }
        public int CounterCooking { get => _counterCooking; set => SetProperty(ref _counterCooking, value, () => { RaisePropertyChanged(nameof(CounterCooking)); }); }
        public int CounterEngineering { get => _counterEngineering; set => SetProperty(ref _counterEngineering, value, () => { RaisePropertyChanged(nameof(CounterEngineering)); }); }
        public int CounterFurnishing { get => _counterFurnishing; set => SetProperty(ref _counterFurnishing, value, () => { RaisePropertyChanged(nameof(CounterFurnishing)); }); }
        public int CounterJewelcrafting { get => _counterJewelcrafting; set => SetProperty(ref _counterJewelcrafting, value, () => { RaisePropertyChanged(nameof(CounterJewelcrafting)); }); }
        public int CounterWeaponsmithing { get => _counterWeaponsmithing; set => SetProperty(ref _counterWeaponsmithing, value, () => { RaisePropertyChanged(nameof(CounterWeaponsmithing)); }); }
        public string ItemNameFilter
        {
            get => _ItemNameFilter;
            set
            {
                _ItemNameFilter = value;
                RaisePropertyChanged(nameof(ItemNameFilter));

                CraftingRecipesFiltered?.Refresh();

                if (CraftingRecipesFiltered?.Count == 1)
                {
                    SelectedCraftingRecipe = (CraftingRecipe)CraftingRecipesFiltered.GetItemAt(0);
                }
            }
        }

        public bool FilterRecipeLearned
        {
            get => _filterRecipeLearned;
            set
            {
                _filterRecipeLearned = value;
                CraftingRecipesFiltered?.Refresh();
            }
        }

        public bool FilterRecipeUnlearned
        {
            get => _filterRecipeUnlearned;
            set
            {
                _filterRecipeUnlearned = value;
                CraftingRecipesFiltered?.Refresh();
            }
        }

        public bool ToggleArcana
        {
            get => _toggleArcana;
            set
            {
                _toggleArcana = value;
                CraftingRecipesFiltered?.Refresh();
            }
        }
        public bool ToggleArmoring
        {
            get => _toggleArmoring;
            set
            {
                _toggleArmoring = value;
                CraftingRecipesFiltered?.Refresh();
            }
        }
        public bool ToggleCooking
        {
            get => _toggleCooking;
            set
            {
                _toggleCooking = value;
                CraftingRecipesFiltered?.Refresh();
            }
        }
        public bool ToggleEngineering
        {
            get => _toggleEngineering;
            set
            {
                _toggleEngineering = value;
                CraftingRecipesFiltered?.Refresh();
            }
        }
        public bool ToggleFurnishing
        {
            get => _toggleFurnishing;
            set
            {
                _toggleFurnishing = value;
                CraftingRecipesFiltered?.Refresh();
            }
        }
        public bool ToggleJewelcrafting
        {
            get => _toggleJewelcrafting;
            set
            {
                _toggleJewelcrafting = value;
                CraftingRecipesFiltered?.Refresh();
            }
        }
        public bool ToggleWeaponsmithing
        {
            get => _toggleWeaponsmithing;
            set
            {
                _toggleWeaponsmithing = value;
                CraftingRecipesFiltered?.Refresh();
            }
        }

        public bool ToggleRefresh
        {
            get => _toggleRefresh; 
            set
            {
                _toggleRefresh = value;
                _screenCaptureHandler.IsActive = value;
            }
        }

        #endregion

        // Start of Events region

        #region Events

        private void DispatcherTimer_Tick(object? sender, EventArgs eventArgs)
        {
            (sender as System.Windows.Threading.DispatcherTimer)?.Stop();
            UpdateCraftingRecipes();
            UpdateCraftingCounters();
        }

        private void HandleOcrTextReadyEvent()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                // Only set filter for recipe items.
                if (CraftingRecipes.Any(recipe => recipe.Localisation.StartsWith(_ocrHandler.OcrText)))
                {
                    ItemNameFilter = _ocrHandler.OcrText;
                }
            });
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void CraftingRecipeLearnedExecute(object isChecked)
        {
            UpdateCraftingCounters();
            _craftingRecipeManager.SaveRecipes();
        }

        private void VisitNwdbExecute(object url)
        {
            Process.Start(new ProcessStartInfo(url as string ?? string.Empty) { UseShellExecute = true });
        }

        private void CopyRecipeNameExecute(object obj)
        {
            // Note: New World does not accept the following special characters when using copy/paste: ':', '''.
            try
            {
                var recipe = (CraftingRecipe)obj;

                // Remove ':'
                string recipeName = recipe.Localisation.Contains(':') ?
                    recipe.Localisation.Substring(recipe.Localisation.IndexOf(':') + 1) :
                    recipe.Localisation;

                // Remove '''
                recipeName = recipeName.Contains('\'') ?
                    recipeName.Substring(0, recipeName.IndexOf('\'')) :
                    recipeName;

                System.Windows.Clipboard.SetText(recipeName.Trim());
            }
            catch (Exception) { }
        }

        private void CreateCraftingRecipesFilteredView()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                CraftingRecipesFiltered = new ListCollectionView(CraftingRecipes)
                {
                    Filter = FilterCraftingRecipes
                };
            });
        }

        private bool FilterCraftingRecipes(object craftingRecipesObj)
        {
            var allowed = false;
            if (craftingRecipesObj == null) return false;

            CraftingRecipe craftingRecipe = (CraftingRecipe)craftingRecipesObj;
            string tradeskill = craftingRecipe.Tradeskill;

            switch (tradeskill)
            {
                case TradeskillConstants.Arcana:
                    allowed = ToggleArcana;
                    break;
                case TradeskillConstants.Armoring:
                    allowed = ToggleArmoring;
                    break;
                case TradeskillConstants.Cooking:
                    allowed = ToggleCooking;
                    break;
                case TradeskillConstants.Engineering:
                    allowed = ToggleEngineering;
                    break;
                case TradeskillConstants.Furnishing:
                    allowed = ToggleFurnishing;
                    break;
                case TradeskillConstants.Jewelcrafting:
                    allowed = ToggleJewelcrafting;
                    break;
                case TradeskillConstants.Weaponsmithing:
                    allowed = ToggleWeaponsmithing;
                    break;
                default:
                    allowed = false;
                    break;
            }

            if (allowed)
            {
                allowed = string.IsNullOrWhiteSpace(ItemNameFilter) ? true : craftingRecipe.Localisation.ToLower().Contains(ItemNameFilter.ToLower());
            }

            if (allowed)
            {
                allowed = (FilterRecipeLearned ? craftingRecipe.Learned : false) || (FilterRecipeUnlearned ? !craftingRecipe.Learned : false);
            }

            return allowed;
        }

        private void LoadCraftingIcons()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = string.Empty;

            resourcePath = "arcana_trade_skill_icon_250px.png";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    ImageArcana = ImageSourceFromBitmap(new Bitmap(stream));
                }
            }

            resourcePath = "armoring_trade_skill_icon_250px.png";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    ImageArmoring = ImageSourceFromBitmap(new Bitmap(stream));
                }
            }

            resourcePath = "cooking_trade_skill_icon_250px.png";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    ImageCooking = ImageSourceFromBitmap(new Bitmap(stream));
                }
            }

            resourcePath = "engineering_trade_skill_icon_250px.png";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    ImageEngineering = ImageSourceFromBitmap(new Bitmap(stream));
                }
            }

            resourcePath = "furnishing_trade_skill_icon_250px.png";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    ImageFurnishing = ImageSourceFromBitmap(new Bitmap(stream));
                }
            }

            resourcePath = "jewelcrafting_trade_skill_icon_250px.png";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    ImageJewelcrafting = ImageSourceFromBitmap(new Bitmap(stream));
                }
            }

            resourcePath = "weaponsmithing_trade_skill_icon_250px.png";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    ImageWeaponsmithing = ImageSourceFromBitmap(new Bitmap(stream));
                }
            }
        }

        private BitmapSource ImageSourceFromBitmap(Bitmap bitmap)
        {
            var handle = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                PInvoke.Gdi32.DeleteObject(handle);
            }
        }

        private void UpdateCraftingRecipes()
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    CraftingRecipes.Clear();
                    CraftingRecipes.AddRange(_craftingRecipeManager.CraftingRecipes);
                });
            }
        }

        private void UpdateCraftingCounters()
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    CounterArcana = CraftingRecipes.Count(r => r.Learned == false && r.Tradeskill.Equals(TradeskillConstants.Arcana));
                    CounterArmoring = CraftingRecipes.Count(r => r.Learned == false && r.Tradeskill.Equals(TradeskillConstants.Armoring));
                    CounterCooking = CraftingRecipes.Count(r => r.Learned == false && r.Tradeskill.Equals(TradeskillConstants.Cooking));
                    CounterEngineering = CraftingRecipes.Count(r => r.Learned == false && r.Tradeskill.Equals(TradeskillConstants.Engineering));
                    CounterFurnishing = CraftingRecipes.Count(r => r.Learned == false && r.Tradeskill.Equals(TradeskillConstants.Furnishing));
                    CounterJewelcrafting = CraftingRecipes.Count(r => r.Learned == false && r.Tradeskill.Equals(TradeskillConstants.Jewelcrafting));
                    CounterWeaponsmithing = CraftingRecipes.Count(r => r.Learned == false && r.Tradeskill.Equals(TradeskillConstants.Weaponsmithing));
                });
            }
        }

        #endregion
    }
}
