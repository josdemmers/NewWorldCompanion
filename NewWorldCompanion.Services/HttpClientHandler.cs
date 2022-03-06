using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class HttpClientHandler : IHttpClientHandler
    {
        private readonly IEventAggregator _eventAggregator;

        // HttpClient is intended to be instantiated once per application, rather than per-use.
        static readonly HttpClient _client = new HttpClient();

        // Start of Constructor region

        #region Constructor

        public HttpClientHandler(IEventAggregator eventAggregator)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Config client
            //_client.DefaultRequestHeaders.Clear();
            //_client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            _client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };          
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
            try
            {
                return await _client.GetStringAsync(uri);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
