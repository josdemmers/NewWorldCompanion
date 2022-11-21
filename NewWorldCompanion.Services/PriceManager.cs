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
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class PriceManager : IPriceManager
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
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

        public PriceManager(IEventAggregator eventAggregator, ILogger<PriceManager> logger, ISettingsManager settingsManager, IHttpClientHandler httpClientHandler, INewWorldDataStore newWorldDataStore)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init logger
            _logger = logger;

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
                string json = await _httpClientHandler.GetRequest("https://nwmarketprices.com/api/servers/") ?? string.Empty;
                var servers = JsonSerializer.Deserialize<Dictionary<string, Server>>(json);

                foreach (var server in servers ?? new Dictionary<string, Server>())
                {
                    string serverId = server.Key;
                    string serverName = server.Value.Name;

                    if (!string.IsNullOrWhiteSpace(serverId) && !string.IsNullOrWhiteSpace(serverName))
                    {
                        _servers.Add(new PriceServer()
                        {
                            Id = int.Parse(serverId),
                            Name = serverName
                        });
                    }
                }
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
                            Task.Delay(100).Wait();

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

        private class Server
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }
    }
}
