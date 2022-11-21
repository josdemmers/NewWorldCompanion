using GameOverlay.Drawing;
using GameOverlay.Windows;
using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NewWorldCompanion.Services
{
    public class OverlayHandler : IOverlayHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ICraftingRecipeManager _craftingRecipeManager;
        private readonly INewWorldDataStore _newWorldDataStore;
        private readonly IOcrHandler _ocrHandler;
        private readonly IPriceManager _priceManager;
        private readonly IScreenProcessHandler _screenProcessHandler;

        private DispatcherTimer _timer = new();

        private readonly GraphicsWindow _window;
        private readonly Dictionary<string, SolidBrush> _brushes;
        private readonly Dictionary<string, Font> _fonts;

        private string _itemName = string.Empty;
        private string _itemNamePrevious = string.Empty;
        private int _overlayX = 0;
        private int _overlayY = 0;
        private int _overlayWidth = 0;
        private int _overlayHeigth = 0;

        // Start of Constructor region

        #region Constructor

        public OverlayHandler(IEventAggregator eventAggregator, ICraftingRecipeManager craftingRecipeManager, INewWorldDataStore newWorldDataStore, 
            IOcrHandler ocrHandler, IPriceManager priceManager, IScreenProcessHandler screenProcessHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OcrTextReadyEvent>().Subscribe(HandleOcrTextReadyEvent);
            _eventAggregator.GetEvent<OverlayHideEvent>().Subscribe(HandleOverlayHideEvent);
            _eventAggregator.GetEvent<OverlayShowEvent>().Subscribe(HandleOverlayShowEvent);
            _eventAggregator.GetEvent<RoiImageReadyEvent>().Subscribe(HandleRoiImageReadyEvent);

            // Init services
            _craftingRecipeManager = craftingRecipeManager;
            _newWorldDataStore = newWorldDataStore;
            _ocrHandler = ocrHandler;
            _priceManager = priceManager;
            _screenProcessHandler = screenProcessHandler;

            _brushes = new Dictionary<string, SolidBrush>();
            _fonts = new Dictionary<string, Font>();

            var gfx = new Graphics()
            {
                MeasureFPS = true,
                PerPrimitiveAntiAliasing = true,
                TextAntiAliasing = true
            };

            _window = new GraphicsWindow(0, 0, 400, 100, gfx)
            {
                FPS = 60,
                IsTopmost = true,
                IsVisible = true
            };

            _window.DestroyGraphics += DestroyGraphics;
            _window.DrawGraphics += DrawGraphics;
            _window.SetupGraphics += SetupGraphics;

            // Start overlay task
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100),
                IsEnabled = true
            };
            _timer.Tick += Timer_Tick; ;
        }

        #endregion

        // Start of Properties region

        #region Properties

        #endregion

        // Start of Events region

        #region Events

        private void DestroyGraphics(object? sender, DestroyGraphicsEventArgs e)
        {
            foreach (var pair in _brushes) pair.Value.Dispose();
            foreach (var pair in _fonts) pair.Value.Dispose();
        }

        private void DrawGraphics(object? sender, DrawGraphicsEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_itemName))
            {
                return;
            }

            _window.X = _overlayX;
            _window.Y = _overlayY;
            _window.Width = _overlayWidth;
            _window.Height = _overlayHeigth;

            string itemName = _itemName;
            string itemId = _newWorldDataStore.GetItemId(itemName);
            var craftingRecipe = string.IsNullOrWhiteSpace(itemId) 
                ? _craftingRecipeManager.CraftingRecipes.FirstOrDefault(r => r.LocalisationUserFriendly.StartsWith(itemName, StringComparison.OrdinalIgnoreCase))
                : _craftingRecipeManager.CraftingRecipes.FirstOrDefault(r => r.LocalisationUserFriendly.Equals(itemName, StringComparison.OrdinalIgnoreCase));        

            if (craftingRecipe != null)
            {
                DrawGraphicsRecipe(e, craftingRecipe);
            }
            else
            {
                DrawGraphicsItem(e, itemName);
            }
        }

        private void DrawGraphicsItem(DrawGraphicsEventArgs e, string itemName)
        {
            NwmarketpriceJson nwmarketpriceJson = _priceManager.GetPriceData(itemName);

            string infoItemName = itemName;
            string infoPrice = "Loading...";
            string infoPriceAvg = string.Empty;  

            if (!string.IsNullOrWhiteSpace(nwmarketpriceJson.item_name))
            {
                var priceChange = nwmarketpriceJson.price_change >= 0 ? $"+{nwmarketpriceJson.price_change}" : $"{nwmarketpriceJson.price_change}";
                infoPrice = $"{nwmarketpriceJson.recent_lowest_price.ToString("F2")} ({priceChange}%) ({nwmarketpriceJson.last_checked_string})";
                infoPriceAvg = nwmarketpriceJson.RecentLowestPriceAvg;
                infoPriceAvg = string.IsNullOrWhiteSpace(infoPriceAvg) ? infoPriceAvg : $"{infoPriceAvg} (15-day avg) ({nwmarketpriceJson.last_checked_string})";
            }

            // Do not show Bind on pickup items.
            if (!_newWorldDataStore.IsBindOnPickup(itemName))
            {
                var gfx = e.Graphics;
                gfx.ClearScene(_brushes["background"]);
                gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 20, infoItemName);
                gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 40, infoPrice);
                gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 60, infoPriceAvg);
                gfx.DrawRectangle(_brushes["border"], 0, 0, _overlayWidth, _overlayHeigth, 1);
            }
            else
            {
                _window.Hide();
            }
        }

        private void DrawGraphicsRecipe(DrawGraphicsEventArgs e, CraftingRecipe craftingRecipe)
        {
            NwmarketpriceJson nwmarketpriceJson = _priceManager.GetPriceData(craftingRecipe.LocalisationUserFriendly);

            bool learnedStatus = craftingRecipe.Learned;
            string infoItemName = craftingRecipe.LocalisationUserFriendly;
            string infoLearned = $"Learned: {learnedStatus}";
            string infoPrice = "Loading...";
            string infoPriceAvg = string.Empty;

            if (!string.IsNullOrWhiteSpace(nwmarketpriceJson.item_name))
            {
                var priceChange = nwmarketpriceJson.price_change >= 0 ? $"+{nwmarketpriceJson.price_change}" : $"{nwmarketpriceJson.price_change}";
                infoPrice = $"{nwmarketpriceJson.recent_lowest_price.ToString("F2")} ({priceChange}%) ({nwmarketpriceJson.last_checked_string})";
                infoPriceAvg = nwmarketpriceJson.RecentLowestPriceAvg;
                infoPriceAvg = string.IsNullOrWhiteSpace(infoPriceAvg) ? infoPriceAvg : $"{infoPriceAvg} (15-day avg) ({nwmarketpriceJson.last_checked_string})";
            }

            var gfx = e.Graphics;
            gfx.ClearScene(_brushes["background"]);
            gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 20, infoItemName);
            gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 40, infoLearned);
            gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 60, infoPrice);
            gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 80, infoPriceAvg);
            gfx.DrawRectangle(_brushes["border"], 0, 0, _overlayWidth, _overlayHeigth, 1);
        }

        private void SetupGraphics(object? sender, SetupGraphicsEventArgs e)
        {
            var gfx = e.Graphics;

            if (e.RecreateResources)
            {
                foreach (var pair in _brushes) pair.Value.Dispose();
            }

            _brushes["black"] = gfx.CreateSolidBrush(0, 0, 0);
            _brushes["white"] = gfx.CreateSolidBrush(255, 255, 255);
            _brushes["red"] = gfx.CreateSolidBrush(255, 0, 0);
            _brushes["green"] = gfx.CreateSolidBrush(0, 255, 0);
            _brushes["blue"] = gfx.CreateSolidBrush(0, 0, 255);
            _brushes["background"] = gfx.CreateSolidBrush(25, 25, 25);
            _brushes["border"] = gfx.CreateSolidBrush(75, 75, 75);
            _brushes["text"] = gfx.CreateSolidBrush(200, 200, 200);

            if (e.RecreateResources) return;

            _fonts["arial"] = gfx.CreateFont("Arial", 12);
            _fonts["consolas"] = gfx.CreateFont("Consolas", 14);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            (sender as DispatcherTimer)?.Stop();
            StartOverlay();
        }

        private void HandleOcrTextReadyEvent()
        {
            _itemName = _ocrHandler.OcrText;
            if (!_itemName.Equals(_itemNamePrevious))
            {
                _priceManager.UpdatePriceData(_itemName);
                _itemNamePrevious = _itemName;
            }
        }

        private void HandleOverlayShowEvent()
        {
            // Event is triggered when OCR text is ready
            if (_window.IsInitialized)
            {
                // Only show when item is valid.
                // - Recipes
                // - Tradable items
                string itemName = _itemName;
                var craftingRecipe = _craftingRecipeManager.CraftingRecipes.FirstOrDefault(r => r.LocalisationUserFriendly.StartsWith(itemName, StringComparison.OrdinalIgnoreCase));
                if (craftingRecipe != null || !_newWorldDataStore.IsBindOnPickup(itemName))
                {
                    _window.Show();
                }
            }
        }

        private void HandleOverlayHideEvent()
        {
            if (_window.IsInitialized)
            {
                _window.Hide();
            } 
        }

        private void HandleRoiImageReadyEvent()
        {
            _overlayX = _screenProcessHandler.OverlayX;
            _overlayY = _screenProcessHandler.OverlayY;
            _overlayWidth = _screenProcessHandler.OverlayWidth;
            _overlayHeigth = _screenProcessHandler.OverlayHeigth;
        }

        #endregion

        // Start of Methods region
        #region Methods

        private async void StartOverlay()
        {
            await Task.Run(() =>
            {
                _window.Create();
                _window.Join();
            });
        }

        #endregion
    }
}
