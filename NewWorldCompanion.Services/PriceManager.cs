using Microsoft.Extensions.Logging;
using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Helpers;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private List<PriceServer> _servers = new List<PriceServer>();
        private ServerPriceData _serverPriceData = new ServerPriceData();

        // Start of Constructor region

        #region Constructor

        public PriceManager(IEventAggregator eventAggregator, ILogger<PriceManager> logger, ISettingsManager settingsManager, IHttpClientHandler httpClientHandler, INewWorldDataStore newWorldDataStore, IRelatedPriceManager relatedPriceManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<SelectedServerChanged>().Subscribe(HandleSelectedServerChangedEvent);

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

        public string ServerId { get => _settingsManager.Settings.PriceServerIdNwm; }
        public List<PriceServer> Servers { get => _servers; set => _servers = value; }

        #endregion

        // Start of Events region

        #region Events

        private async void HandleSelectedServerChangedEvent()
        {
            try
            {
                // Clear old data
                _serverPriceData = new ServerPriceData();

                string serverId = _settingsManager.Settings.PriceServerIdNwm;
                PriceServer? server = _servers.FirstOrDefault(s => s.Id.Equals(serverId));

                // Skip invalid servers
                // Skip servers with no price data
                if (server != null && server.Updated.Year > 1970)
                {
                    string json = await _httpClientHandler.GetRequest($"https://scdn.gaming.tools/nwmp/history/servers/{server.Id}.json.gz") ?? "{}";

                    var options = new JsonSerializerOptions();
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new DoubleConverter());
                    options.Converters.Add(new IntConverter());
                    _serverPriceData = JsonSerializer.Deserialize<ServerPriceData>(json, options) ?? new ServerPriceData();
                }

                _eventAggregator.GetEvent<PriceCacheUpdatedEvent>().Publish();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
            }
        }

        #endregion

        // Start of Methods region

        #region Methods

        private async void UpdateServerList()
        {
            try
            {
                string json = await _httpClientHandler.GetRequest("https://nwmpapi.gaming.tools/servers") ?? "{}";
                var options = new JsonSerializerOptions();
                options.Converters.Add(new BoolConverter());
                options.Converters.Add(new DoubleConverter());
                options.Converters.Add(new IntConverter());
                var servers = JsonSerializer.Deserialize<List<ServerInfo>>(json, options);

                _servers.Clear();
                foreach (var server in servers ?? new List<ServerInfo>())
                {
                    var serverId = server.Id;
                    var serverName = server.Name;
                    var updated = UnixTimeStamp.UnixTimeStampToDateTime(server.LastUpdated);

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

        public Nwmarketprice GetPriceData(string itemName)
        {
            var nwmarketprice = new Nwmarketprice();

            string itemId = _newWorldDataStore.GetItemId(itemName).ToLower();
            List<PriceData> priceData = new List<PriceData>();
            if (_serverPriceData.Daily.TryGetValue(itemId, out priceData))
            {
                double recentLowestPricePrev = priceData.Count >= 2 ? priceData[1].Means[0][1] / 100.0 : 0.0;
                double recentLowestPrice = priceData.Count >= 1 ? priceData[0].Means[0][1] / 100.0 : 0.0;
                int priceChange = recentLowestPricePrev > 0 ? (int)(((recentLowestPrice - recentLowestPricePrev) / (recentLowestPricePrev))*100.0) : 0;
                double recentLowestPriceAvg = priceData.Average(i => i.Means[0][1] / 100.0);

                int timeStamp = priceData[0].Timestamp;
                DateTime lastUpdated = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                lastUpdated = lastUpdated.AddSeconds(timeStamp).ToLocalTime();

                nwmarketprice.ItemId = itemId;
                nwmarketprice.ItemName = itemName;
                nwmarketprice.Days = priceData.Count;
                nwmarketprice.LastUpdated = lastUpdated;
                nwmarketprice.PriceChange = priceChange;
                nwmarketprice.RecentLowestPrice = recentLowestPrice;
                nwmarketprice.RecentLowestPriceAvg = recentLowestPriceAvg;
            }

            return nwmarketprice;
        }

        public double GetCraftingCosts(string itemId)
        {
            var recipe = _newWorldDataStore.GetCraftingRecipeDetails(itemId);
            double craftingCosts = 0;

            var ingredient = string.Empty;
            var nwmarketprice = new Nwmarketprice();
            var qty = recipe.Qty1;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient1) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient1.Last()) ? recipe.Ingredient1 : $"{recipe.Ingredient1}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                nwmarketprice = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketprice.RecentLowestPrice * qty);
            }

            qty = recipe.Qty2;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient2) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient2.Last()) ? recipe.Ingredient2 : $"{recipe.Ingredient2}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                nwmarketprice = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketprice.RecentLowestPrice * qty);
            }

            qty = recipe.Qty3;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient3) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient3.Last()) ? recipe.Ingredient3 : $"{recipe.Ingredient3}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                nwmarketprice = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketprice.RecentLowestPrice * qty);
            }

            qty = recipe.Qty4;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient4) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient4.Last()) ? recipe.Ingredient4 : $"{recipe.Ingredient4}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                nwmarketprice = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketprice.RecentLowestPrice * qty);
            }

            qty = recipe.Qty5;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient5) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient5.Last()) ? recipe.Ingredient5 : $"{recipe.Ingredient5}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                nwmarketprice = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketprice.RecentLowestPrice * qty);
            }

            qty = recipe.Qty6;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient6) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient6.Last()) ? recipe.Ingredient6 : $"{recipe.Ingredient6}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                nwmarketprice = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketprice.RecentLowestPrice * qty);
            }

            qty = recipe.Qty7;
            if (!string.IsNullOrWhiteSpace(recipe.Ingredient7) && qty != 0)
            {
                ingredient = Char.IsDigit(recipe.Ingredient7.Last()) ? recipe.Ingredient7 : $"{recipe.Ingredient7}T1";

                string ingredientName = _newWorldDataStore.GetItem(ingredient)?.Name ?? string.Empty;
                ingredientName = _newWorldDataStore.GetItemLocalisation(ingredientName);
                nwmarketprice = GetPriceData(ingredientName);
                craftingCosts = craftingCosts + (nwmarketprice.RecentLowestPrice * qty);
            }

            return craftingCosts;
        }

        public List<Nwmarketprice> GetExtendedPriceData(string itemName)
        {
            var extendedPriceDataList = new List<Nwmarketprice>();
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

                    var nwmarketprice = new Nwmarketprice();
                    nwmarketprice = GetPriceData(recipeName);
                    if (!string.IsNullOrWhiteSpace(nwmarketprice.ItemId))
                    {
                        extendedPriceDataList.Add(nwmarketprice);
                    }
                }
            }

            return extendedPriceDataList;
        }

        #endregion

        private class ServerInfo
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [JsonPropertyName("region")]
            public string Region { get; set; } = string.Empty;

            [JsonPropertyName("last_updated")]
            public double LastUpdated { get; set; } = 0;

            [JsonPropertyName("json_url")]
            public string JsonUrl { get; set; } = string.Empty;

            [JsonPropertyName("tsv_url")]
            public string TsvUrl{ get; set; } = string.Empty;
        }
    }
}
