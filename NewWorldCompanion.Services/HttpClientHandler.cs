using Microsoft.Extensions.Logging;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class HttpClientHandler : IHttpClientHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        // HttpClient is intended to be instantiated once per application, rather than per-use.
        static readonly HttpClient _client = new HttpClient();

        // Start of Constructor region

        #region Constructor

        public HttpClientHandler(IEventAggregator eventAggregator, ILogger<HttpClientHandler> logger)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init logger
            _logger = logger;

            // Config client
            //_client.DefaultRequestHeaders.Clear();
            //_client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            _client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            _client.DefaultRequestHeaders.Add("User-Agent", "NewWorldCompanion");
            _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        }

        #endregion

        // Start of Properties region

        #region Properties

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods

        public async Task<string> GetRequest(string uri)
        {
            HttpResponseMessage? response = null;
            string responseAsString = string.Empty;
            try
            {
                response = await _client.GetAsync(uri);
                responseAsString = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return responseAsString;
            }
            catch (Exception ex)
            {
                if (response != null)
                {
                    _logger.LogError(ex, $"GetRequest({uri}). Response: {responseAsString}");
                }
                else
                {
                    _logger.LogError(ex, $"GetRequest({uri}).");
                }

                return responseAsString;
            }
        }

        #endregion
    }
}
