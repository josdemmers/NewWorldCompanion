using NewWorldCompanion.Entities;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class StorageManager : IStorageManager
    {
        private readonly IEventAggregator _eventAggregator;

        private ObservableCollection<Item> _items = new ObservableCollection<Item>();

        // Start of Constructor region

        #region Constructor

        public StorageManager(IEventAggregator eventAggregator)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init recipes
            LoadStorage();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public ObservableCollection<Item> Items { get => _items; set => _items = value; }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods

        private void LoadStorage()
        {
            _items.Clear();

            string fileName = "Config/Storage.json";
            if (File.Exists(fileName))
            {
                using FileStream stream = File.OpenRead(fileName);
                _items = JsonSerializer.Deserialize<ObservableCollection<Item>>(stream) ?? new ObservableCollection<Item>();
            }

            // Sort
            List<Item> sortedItems = _items.ToList();
            sortedItems.Sort((x, y) =>
            {
                int result = string.Compare(x.Storage, y.Storage, StringComparison.Ordinal);
                return result != 0 ? result : string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            });
            _items.Clear();
            _items = new ObservableCollection<Item>(sortedItems);        

            // Sort list
            //_items.ToList().Sort((x, y) =>
            //{
            //    int result = string.Compare(x.Storage, y.Storage, StringComparison.Ordinal);
            //    return result != 0 ? result : string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            //});

            SaveStorage();
        }

        public void SaveStorage()
        {
            string fileName = "Config/Storage.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonSerializer.Serialize(stream, Items, options);
        }

        #endregion
    }
}
