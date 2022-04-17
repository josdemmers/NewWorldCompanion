using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace NewWorldCompanion.Services
{
    public class VersionManager : IVersionManager
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IHttpClientHandler _httpClientHandler;

        private string _latestVersion = string.Empty;

        // Start of Constructor region

        #region Constructor

        public VersionManager(IEventAggregator eventAggregator, HttpClientHandler httpClientHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init services
            _httpClientHandler = httpClientHandler;

            // Init servers
            UpdateVersionInfo();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public string CurrentVersion
        {
            get
            {
                Version? version = Assembly.GetExecutingAssembly().GetName().Version;
                return version?.ToString() ?? string.Empty;
            }
        }

        public string LatestVersion { get => _latestVersion; set => _latestVersion = value; }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods

        private async void UpdateVersionInfo()
        {
            string uri = $"https://raw.githubusercontent.com/josdemmers/NewWorldCompanion/master/NewWorldCompanion/common.props";
            string xml = await _httpClientHandler.GetRequest(uri);
            if (!string.IsNullOrWhiteSpace(xml))
            {
                var xPathDocument = new XPathDocument(new StringReader(xml));
                var xPathNavigator = xPathDocument.CreateNavigator();
                var xPathExpression = xPathNavigator.Compile("/Project/PropertyGroup/FileVersion/text()");
                var xPathNodeIterator = xPathNavigator.Select(xPathExpression);
                while (xPathNodeIterator.MoveNext())
                {
                    LatestVersion = xPathNodeIterator.Current?.ToString() ?? string.Empty;
                }
            }
            else
            {
                LatestVersion = string.Empty;
            }
            _eventAggregator.GetEvent<VersionInfoUpdatedEvent>().Publish();
        }

        #endregion
    }
}
