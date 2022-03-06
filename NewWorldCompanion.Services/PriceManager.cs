using NewWorldCompanion.Entities;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class PriceManager : IPriceManager
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsManager _settingsManager;
        private readonly IHttpClientHandler _httpClientHandler;

        private Dictionary<string, int> _itemMappings = new Dictionary<string, int>();
        private Dictionary<string, NwmarketpriceJson> _priceCache = new Dictionary<string, NwmarketpriceJson>();
        private List<string> _priceRequestQueue = new List<string>();
        private bool _priceRequestQueueBusy = false;
        private List<PriceServer> _servers = new List<PriceServer>();

        // Start of Constructor region

        #region Constructor

        public PriceManager(IEventAggregator eventAggregator, ISettingsManager settingsManager, IHttpClientHandler httpClientHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init services
            _settingsManager = settingsManager;
            _httpClientHandler = httpClientHandler;

            // Init servers
            _servers.Add(new PriceServer()
            {
                Id = "1",
                Name = "Camelot"
            });
            _servers.Add(new PriceServer()
            {
                Id = "2",
                Name = "El Dorado"
            });

            Task.Run(async () =>
            {
                try
                {
                    string json = await _httpClientHandler.GetRequest("https://nwmarketprices.com/cn/") ?? string.Empty;
                    var root = JsonSerializer.Deserialize<RootCn>(json);
                    var mappings = JsonSerializer.Deserialize<List<List<object>>>(root?.cn ?? string.Empty) ?? new List<List<object>>();
                    foreach (var mapping in mappings)
                    {
                        string key = ((JsonElement)mapping[0]).GetString() ?? string.Empty;
                        int value = ((JsonElement)mapping[1]).GetInt32();

                        if (!string.IsNullOrWhiteSpace(key))
                        {
                            _itemMappings.TryAdd(key, value);
                        }
                    }
                }
                catch (Exception)
                {

                }
            });
        }

        #endregion

        // Start of Properties region

        #region Properties

        public string ServerId { get => _settingsManager.Settings.ServerId; }
        public List<PriceServer> Servers { get => _servers; set => _servers = value; }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods

        public NwmarketpriceJson GetPriceData(string itemName)
        {
            var nwmarketpriceJson = new NwmarketpriceJson();            
            return _priceCache.GetValueOrDefault(itemName, nwmarketpriceJson); ;
        }

        public void UpdatePriceData(string itemName)
        {
            if (!_priceCache.ContainsKey(itemName))
            {
                if (!_priceRequestQueue.Contains(itemName)) _priceRequestQueue.Add(itemName);
                if (!_priceRequestQueueBusy)
                {
                    _priceRequestQueueBusy = true;
                    Task task = Task.Run(async () =>
                    {
                        bool isValid = _itemMappings.TryGetValue(itemName, out var itemId);
                        if (isValid)
                        {
                            try
                            {
                                string uri = $"https://nwmarketprices.com/{itemId}/{ServerId}?cn_id={itemId}";
                                string json = await _httpClientHandler.GetRequest(uri) ?? string.Empty;

                                var nwmarketpriceJson = JsonSerializer.Deserialize<NwmarketpriceJson>(json);
                                if (nwmarketpriceJson != null)
                                {
                                    Debug.WriteLine($"item_name: {nwmarketpriceJson.item_name}");
                                    Debug.WriteLine($"recent_lowest_price: {nwmarketpriceJson.recent_lowest_price}");
                                    Debug.WriteLine($"last_checked: {nwmarketpriceJson.last_checked}");

                                    _priceCache[itemName] = nwmarketpriceJson;
                                }
                            }
                            catch (Exception){};
                        }

                        // Always remove from queue, even with exceptions.
                        _priceRequestQueue.RemoveAll(item => item.Equals(itemName));
                        _priceRequestQueueBusy = false;
                        if (_priceRequestQueue.Any())
                        {
                            Task.Delay(100).Wait();
                            UpdatePriceData(_priceRequestQueue.First());
                        }
                    });
                }
            }
        }

        #endregion

        private class RootCn
        {
            public string cn { get; set; } = string.Empty;
        }
    }
}
