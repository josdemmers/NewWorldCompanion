using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
using NewWorldCompanion.Helpers;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class PriceManager : IPriceManager
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsManager _settingsManager;
        private readonly IHttpClientHandler _httpClientHandler;
        private readonly INewWorldDataStore _newWorldDataStore;

        private readonly object priceRequestLock = new object();

        private Dictionary<string, NwmarketpriceJson> _priceCache = new Dictionary<string, NwmarketpriceJson>();
        private List<string> _priceRequestQueue = new List<string>();
        private bool _priceRequestQueueBusy = false;
        private List<PriceServer> _servers = new List<PriceServer>();

        // Start of Constructor region

        #region Constructor

        public PriceManager(IEventAggregator eventAggregator, ISettingsManager settingsManager, IHttpClientHandler httpClientHandler, INewWorldDataStore newWorldDataStore)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init services
            _settingsManager = settingsManager;
            _httpClientHandler = httpClientHandler;
            _newWorldDataStore = newWorldDataStore;

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
                string json = await _httpClientHandler.GetRequest("https://nwmarketprices.com/servers/") ?? string.Empty;
                var root = JsonSerializer.Deserialize<RootServers>(json);
                var mappings = JsonSerializer.Deserialize<List<List<object>>>(root?.servers ?? string.Empty) ?? new List<List<object>>();
                foreach (var mapping in mappings)
                {
                    string serverName = ((JsonElement)mapping[0]).GetString() ?? string.Empty;
                    int serverId = ((JsonElement)mapping[1]).GetInt32();

                    if (!string.IsNullOrWhiteSpace(serverName))
                    {
                        _servers.Add(new PriceServer()
                        {
                            Id = serverId,
                            Name = serverName
                        });

                    }
                }
            }
            catch (Exception)
            {

            }

            _eventAggregator.GetEvent<PriceServerListUpdatedEvent>().Publish();
        }

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
                                            Debug.WriteLine($"last_checked: {nwmarketpriceJson.last_checked}");

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
                                            last_checked = "no data"
                                        };
                                    }
                                }
                                catch (Exception) { };
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
        }

        #endregion

        private class RootServers
        {
            public string servers { get; set; } = string.Empty;
        }
    }
}
