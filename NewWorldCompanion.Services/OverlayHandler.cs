﻿using GameOverlay.Drawing;
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
        private readonly ISettingsManager _settingsManager;
        private readonly ICraftingRecipeManager _craftingRecipeManager;
        private readonly INewWorldDataStore _newWorldDataStore;
        private readonly IOcrHandler _ocrHandler;
        private readonly IPriceManager _priceManager;
        private readonly IScreenProcessHandler _screenProcessHandler;
        private readonly IStorageManager _storageManager;

        private readonly GraphicsWindow _window;
        private readonly Dictionary<string, SolidBrush> _brushes;
        private readonly Dictionary<string, Font> _fonts;

        private string _itemName = string.Empty;
        private string _itemNamePrevious = string.Empty;
        private int _overlayX = 0;
        private int _overlayY = 0;
        private int _overlayWidth = 0;
        private int _overlayHeigth = 0;
        private double _mouseDelta = 0;

        // Start of Constructor region

        #region Constructor

        public OverlayHandler(IEventAggregator eventAggregator, ISettingsManager settingsManager, ICraftingRecipeManager craftingRecipeManager, INewWorldDataStore newWorldDataStore,
            IOcrHandler ocrHandler, IPriceManager priceManager, IScreenProcessHandler screenProcessHandler, IStorageManager storageManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OcrTextReadyEvent>().Subscribe(HandleOcrTextReadyEvent);
            _eventAggregator.GetEvent<OverlayHideEvent>().Subscribe(HandleOverlayHideEvent);
            _eventAggregator.GetEvent<OverlayShowEvent>().Subscribe(HandleOverlayShowEvent);
            _eventAggregator.GetEvent<RoiImageReadyEvent>().Subscribe(HandleRoiImageReadyEvent);
            _eventAggregator.GetEvent<NewWorldDataStoreUpdated>().Subscribe(HandleNewWorldDataStoreUpdatedEvent);
            _eventAggregator.GetEvent<MouseDeltaUpdatedEvent>().Subscribe(HandleMouseDeltaUpdatedEvent);

            // Init services
            _settingsManager = settingsManager;
            _craftingRecipeManager = craftingRecipeManager;
            _newWorldDataStore = newWorldDataStore;
            _ocrHandler = ocrHandler;
            _priceManager = priceManager;
            _screenProcessHandler = screenProcessHandler;
            _storageManager = storageManager;

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
                if (!_newWorldDataStore.IsNamedItem(itemName))
                {
                    DrawGraphicsItem(e, itemName);
                }
                else if (_settingsManager.Settings.NamedItemsTooltipEnabled)
                {
                    DrawGraphicsNamedItem(e, itemName);
                }
                else
                {
                    // Clear
                    var gfx = e.Graphics;
                    gfx.ClearScene();
                }
            }
        }

        private void DrawGraphicsItem(DrawGraphicsEventArgs e, string itemName)
        {
            Nwmarketprice nwmarketprice = _priceManager.GetPriceData(itemName);
            List<Nwmarketprice> extendedNwmarketprice = _priceManager.GetExtendedPriceData(itemName);

            string infoItemName = itemName;
            string infoPrice = "Loading...";
            string infoPriceAvg = string.Empty;

            if (!string.IsNullOrWhiteSpace(nwmarketprice.ItemName))
            {
                var priceChange = nwmarketprice.PriceChange >= 0 ? $"+{nwmarketprice.PriceChange}" : $"{nwmarketprice.PriceChange}";
                infoPrice = $"{nwmarketprice.RecentLowestPrice.ToString("F2")} ({priceChange}%) ({nwmarketprice.LastUpdatedString})";
                infoPriceAvg = nwmarketprice.RecentLowestPriceAvg.ToString("F2");
                infoPriceAvg = string.IsNullOrWhiteSpace(infoPriceAvg) ? infoPriceAvg : $"{infoPriceAvg} ({nwmarketprice.Days}-day avg) ({nwmarketprice.LastUpdatedString})";
            }

            // Do not show Bind on pickup items.
            if (!_newWorldDataStore.IsBindOnPickup(itemName))
            {
                int ExtendedTooltipOffset = _settingsManager.Settings.ExtendedTooltipEnabled ? Math.Min(extendedNwmarketprice.Count, 3) * 20 : 0;
                // Add some extra margin
                ExtendedTooltipOffset = ExtendedTooltipOffset == 0 ? 0 : ExtendedTooltipOffset + 40;

                _window.X = _overlayX;
                _window.Y = _overlayY - ExtendedTooltipOffset;
                _window.Width = _overlayWidth;
                _window.Height = _overlayHeigth + ExtendedTooltipOffset;

                var gfx = e.Graphics;
                gfx.ClearScene(_brushes["background"]);
                gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, ExtendedTooltipOffset+20, infoItemName);
                gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, ExtendedTooltipOffset+40, infoPrice);
                gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, ExtendedTooltipOffset+60, infoPriceAvg);
                gfx.DrawRectangle(_brushes["border"], 0, 0, _overlayWidth, _overlayHeigth + ExtendedTooltipOffset, 1);

                // Extended text
                if (ExtendedTooltipOffset > 0)
                {
                    for (int i = 0; i < Math.Min(extendedNwmarketprice.Count, 3); i++)
                    {
                        var craftingCosts = _priceManager.GetCraftingCosts(extendedNwmarketprice[i].ItemId);
                        gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, ((i+1)*20), $"{extendedNwmarketprice[i].ItemName} " +
                            $"{extendedNwmarketprice[i].RecentLowestPrice.ToString("F2")} (Sell) " +
                            $"{craftingCosts.ToString("F2")} (Craft) " +
                            $"{extendedNwmarketprice[i].RecentLowestPrice - craftingCosts:F2} (Profit)");
                    }
                    gfx.DrawRectangle(_brushes["border"], 0, 0, _overlayWidth, ExtendedTooltipOffset, 1);
                }
            }
            else
            {
                _window.Hide();
            }
        }

        private void DrawGraphicsNamedItem(DrawGraphicsEventArgs e, string itemName)
        {
            string infoItemName = itemName;
            string infoStorage = $"Storage: {_storageManager.GetItemStorageInfo(itemName)}";

            _window.X = _overlayX;
            _window.Y = _overlayY;
            _window.Width = _overlayWidth;
            _window.Height = _overlayHeigth;

            var gfx = e.Graphics;
            gfx.ClearScene(_brushes["background"]);
            gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 20, infoItemName);
            gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 40, "Named");
            gfx.DrawText(_fonts["consolas"], _brushes["text"], 20, 60, infoStorage);
            gfx.DrawRectangle(_brushes["border"], 0, 0, _overlayWidth, _overlayHeigth, 1);
        }

        private void DrawGraphicsRecipe(DrawGraphicsEventArgs e, CraftingRecipe craftingRecipe)
        {
            Nwmarketprice nwmarketprice = _priceManager.GetPriceData(craftingRecipe.LocalisationUserFriendly);

            bool learnedStatus = craftingRecipe.Learned;
            string infoItemName = craftingRecipe.LocalisationUserFriendly;
            string infoLearned = $"Learned: {learnedStatus}";
            string infoPrice = "Loading...";
            string infoPriceAvg = string.Empty;

            if (!string.IsNullOrWhiteSpace(nwmarketprice.ItemName))
            {
                var priceChange = nwmarketprice.PriceChange >= 0 ? $"+{nwmarketprice.PriceChange}" : $"{nwmarketprice.PriceChange}";
                infoPrice = $"{nwmarketprice.RecentLowestPrice.ToString("F2")} ({priceChange}%) ({nwmarketprice.LastUpdatedString})";
                infoPriceAvg = nwmarketprice.RecentLowestPriceAvg.ToString("F2");
                infoPriceAvg = string.IsNullOrWhiteSpace(infoPriceAvg) ? infoPriceAvg : $"{infoPriceAvg} (15-day avg) ({nwmarketprice.LastUpdatedString})";
            }

            _window.X = _overlayX;
            _window.Y = _overlayY;
            _window.Width = _overlayWidth;
            _window.Height = _overlayHeigth;

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

        private void HandleOcrTextReadyEvent()
        {
            _itemName = _ocrHandler.OcrText;
            if (!_itemName.Equals(_itemNamePrevious))
            {
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
                if (craftingRecipe != null || !_newWorldDataStore.IsBindOnPickup(itemName) || _newWorldDataStore.IsNamedItem(itemName))
                {
                    _window.Show();
                }
            }
        }

        private void HandleOverlayHideEvent()
        {
            if (_window.IsInitialized)
            {
                if ((_settingsManager.Settings.NamedItemsTooltipEnabled && _mouseDelta != 0) ||
                    !_settingsManager.Settings.NamedItemsTooltipEnabled)
                {
                    _window.Hide();
                }
            }
        }

        private void HandleRoiImageReadyEvent()
        {
            _overlayX = _screenProcessHandler.OverlayX;
            _overlayY = _screenProcessHandler.OverlayY;
            _overlayWidth = _screenProcessHandler.OverlayWidth;
            _overlayHeigth = _screenProcessHandler.OverlayHeigth;
        }

        private void HandleNewWorldDataStoreUpdatedEvent()
        {
            StartOverlay();
        }

        private void HandleMouseDeltaUpdatedEvent(double delta)
        {
            _mouseDelta = delta;
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
