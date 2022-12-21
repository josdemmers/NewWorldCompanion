using Microsoft.Extensions.Logging;
using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Helpers;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NewWorldCompanion.Services
{
    public class PriceManager : IPriceManager
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly IHttpClientHandler _httpClientHandler;
        private readonly INewWorldDataStore _newWorldDataStore;
        private readonly IRelatedPriceManager _relatedPriceManager;

        private readonly object priceRequestLock = new object();

        private Dictionary<string, NwmarketpriceJson> _priceCache = new Dictionary<string, NwmarketpriceJson>();
        private List<string> _priceRequestQueue = new List<string>();
        private bool _priceRequestQueueBusy = false;
        private List<PriceServer> _servers = new List<PriceServer>();

        // Start of Constructor region

        #region Constructor

        public PriceManager(IEventAggregator eventAggregator, ILogger<PriceManager> logger, ISettingsManager settingsManager, IHttpClientHandler httpClientHandler, INewWorldDataStore newWorldDataStore, IRelatedPriceManager relatedPriceManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init logger
            _logger = logger;

            // Init services
            _settingsManager = settingsManager;
            _httpClientHandler = httpClientHandler;
            _newWorldDataStore = newWorldDataStore;
            _relatedPriceManager = relatedPriceManager;

            // Init servers
            UpdateServerList();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public int ServerId { get => _settingsManager.Settings.PriceServerId; }
        public List<PriceServer> Servers { get => _servers; set => _servers = value; }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods

        private async void UpdateServerList()
        {
            try
            {
                string json = await _httpClientHandler.GetRequest("https://nwmarketprices.com/api/servers_updated/") ?? string.Empty;
                var servers = JsonSerializer.Deserialize<UpdatedServers>(json);

                _servers.Clear();
                foreach (var server in servers?.ServersLastUpdated ?? new List<List<object>>())
                {
                    var serverId = ((JsonElement)server[0]).GetInt32();
                    var serverName = ((JsonElement)server[1]).GetString() ?? string.Empty;
                    var updated = ((JsonElement)server[2]).GetDateTime();

                    _servers.Add(new PriceServer()
                    {
                        Id = serverId,
                        Name = serverName,
                        Updated = updated
                    });
                }

                // Sort server list by name
                _servers.Sort((x, y) =>
                {
                    int result = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    return result;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
            }

            _eventAggregator.GetEvent<PriceServerListUpdatedEvent>().Publish();
        }

        public NwmarketpriceJson GetPriceData(string itemName)
        {
            var nwmarketpriceJson = new NwmarketpriceJson();            
            return _priceCache.GetValueOrDefault(itemName, nwmarketpriceJson); ;
        }

        public double GetCraftingCosts(string itemId)
        {
            var recipe = _newWorldDataStore.GetCraftingRecipeDetails(itemId);
            double craftingCosts = 0;

            var ingredient = string.Empty;
            var nwmarketpriceJson = new NwmarketpriceJson();
            var qty = recipe.Qty1;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient1) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient1.Last()) ? recipe.Ingredient1 : $"{recipe.Ingredient1}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                UpdatePriceData(ingredientName);
                nwmarketpriceJson = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketpriceJson.recent_lowest_price * qty);
            }

            qty = recipe.Qty2;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient2) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient2.Last()) ? recipe.Ingredient2 : $"{recipe.Ingredient2}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                UpdatePriceData(ingredientName);
                nwmarketpriceJson = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketpriceJson.recent_lowest_price * qty);
            }

            qty = recipe.Qty3;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient3) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient3.Last()) ? recipe.Ingredient3 : $"{recipe.Ingredient3}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                UpdatePriceData(ingredientName);
                nwmarketpriceJson = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketpriceJson.recent_lowest_price * qty);
            }

            qty = recipe.Qty4;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient4) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient4.Last()) ? recipe.Ingredient4 : $"{recipe.Ingredient4}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                UpdatePriceData(ingredientName);
                nwmarketpriceJson = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketpriceJson.recent_lowest_price * qty);
            }

            qty = recipe.Qty5;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient5) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient5.Last()) ? recipe.Ingredient5 : $"{recipe.Ingredient5}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                UpdatePriceData(ingredientName);
                nwmarketpriceJson = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketpriceJson.recent_lowest_price * qty);
            }

            qty = recipe.Qty6;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient6) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient6.Last()) ? recipe.Ingredient6 : $"{recipe.Ingredient6}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                UpdatePriceData(ingredientName);
                nwmarketpriceJson = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketpriceJson.recent_lowest_price * qty);
            }

            qty = recipe.Qty7;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient7) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient7.Last()) ? recipe.Ingredient7 : $"{recipe.Ingredient7}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                UpdatePriceData(ingredientName);
                nwmarketpriceJson = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketpriceJson.recent_lowest_price * qty);
            }

            return craftingCosts;
        }

        public List<NwmarketpriceJson> GetExtendedPriceData(string itemName)
        {
            var extendedPriceDataList = new List<NwmarketpriceJson>();
            var itemId = _newWorldDataStore.GetItemId(itemName);
            var overlayResource = _relatedPriceManager.PersistableOverlayResources.FirstOrDefault(item => item.ItemId.Equals(itemId));
            if (overlayResource != null)
            {
                foreach (var recipe in overlayResource.PersistableOverlayResourceRecipes)
                {
                    if (!recipe.IsVisible) continue;

                    string recipeName = _newWorldDataStore.GetItem(recipe.ItemId)?.Name ?? string.Empty;
                    recipeName = _newWorldDataStore.GetItemLocalisation(recipeName);
                    if (string.IsNullOrWhiteSpace(recipeName))
                    {
                        continue;
                    }
                    UpdatePriceData(recipeName);

                    var nwmarketpriceJson = new NwmarketpriceJson();
                    nwmarketpriceJson = _priceCache.GetValueOrDefault(recipeName, nwmarketpriceJson);
                    extendedPriceDataList.Add(nwmarketpriceJson);
                }
            }

            return extendedPriceDataList;
        }

        public void UpdatePriceData(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return;
            }

            if (!_priceCache.ContainsKey(itemName))
            {
                if (!_priceRequestQueue.Contains(itemName)) _priceRequestQueue.Add(itemName);
                lock(priceRequestLock)
                {
                    if (!_priceRequestQueueBusy)
                    {
                        _priceRequestQueueBusy = true;
                        Task task = Task.Run(async () =>
                        {
                            string itemId = _newWorldDataStore.GetItemId(itemName);
                            bool isBindOnPickup = _newWorldDataStore.IsBindOnPickup(itemName);
                            if (!string.IsNullOrWhiteSpace(itemId) && !isBindOnPickup)
                            {
                                try
                                {
                                    string uri = $"https://nwmarketprices.com/0/{ServerId}?cn_id={itemId.ToLower()}";
                                    string json = await _httpClientHandler.GetRequest(uri);

                                    Debug.WriteLine($"uri: {uri}");
                                    Debug.WriteLine($"json: {json}");

                                    if (!string.IsNullOrWhiteSpace(json))
                                    {
                                        // create the options
                                        var options = new JsonSerializerOptions()
                                        {
                                            WriteIndented = true
                                        };
                                        // register the converter
                                        options.Converters.Add(new DoubleConverter());

                                        var nwmarketpriceJson = JsonSerializer.Deserialize<NwmarketpriceJson>(json, options);
                                        if (nwmarketpriceJson != null)
                                        {
                                            Debug.WriteLine($"item_name: {nwmarketpriceJson.item_name}");
                                            Debug.WriteLine($"recent_lowest_price: {nwmarketpriceJson.recent_lowest_price}");
                                            Debug.WriteLine($"last_checked: {nwmarketpriceJson.last_checked_string}");

                                            nwmarketpriceJson.item_name = string.IsNullOrEmpty(nwmarketpriceJson.item_name) ? itemName : nwmarketpriceJson.item_name;

                                            _priceCache[itemName] = nwmarketpriceJson;
                                            _eventAggregator.GetEvent<PriceCacheUpdatedEvent>().Publish();
                                        }
                                    }
                                    else
                                    {
                                        _priceCache[itemName] = new NwmarketpriceJson
                                        {
                                            item_name = itemName,
                                            recent_lowest_price = 0.00,
                                            last_checked = DateTime.MinValue,
                                        };
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
                                }
                            }

                            // Always remove from queue, even with exceptions.
                            _priceRequestQueue.RemoveAll(item => item.Equals(itemName));
                            _priceRequestQueueBusy = false;

                            Task.Delay(250).Wait();

                            if (_priceRequestQueue.Any())
                            {
                                UpdatePriceData(_priceRequestQueue.First());
                            }
                        });
                    }
                }
            }
        }

        #endregion

        private class UpdatedServers
        {
            [JsonPropertyName("server_last_updated")]
            public List<List<object>> ServersLastUpdated { get; set; } = new List<List<object>> { };
        }
    }
}
