using NewWorldCompanion.Entities;
using NewWorldCompanion.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class RelatedPriceManager : IRelatedPriceManager
    {
        private List<PersistableOverlayResource> _persistableOverlayResources = new List<PersistableOverlayResource>();

        // Start of Constructor region

        #region Constructor

        public RelatedPriceManager()
        {
            // Init related prices config
            LoadRelatedPricesConfig();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public List<PersistableOverlayResource> PersistableOverlayResources { get => _persistableOverlayResources; }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods

        private void LoadRelatedPricesConfig()
        {
            _persistableOverlayResources.Clear();

            string fileName = "Config/RelatedPrices.json";
            if (File.Exists(fileName))
            {
                using FileStream stream = File.OpenRead(fileName);
                _persistableOverlayResources = JsonSerializer.Deserialize<List<PersistableOverlayResource>>(stream) ?? new List<PersistableOverlayResource>();
            }

            SaveRelatedPricesConfig();
        }

        public void SaveRelatedPricesConfig()
        {
            string fileName = "Config/RelatedPrices.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonSerializer.Serialize(stream, PersistableOverlayResources, options);
        }

        public void SetRawResourceRecipeVisibility(string itemIDRawResource, string itemID, bool isVisible)
        {
            if (PersistableOverlayResources.Any(resource => resource.ItemId.Equals(itemIDRawResource)))
            {
                // RawResource exists
                var rawResource = PersistableOverlayResources.FirstOrDefault(resource => resource.ItemId.Equals(itemIDRawResource));
                if (rawResource?.PersistableOverlayResourceRecipes.Any(resource => resource.ItemId.Equals(itemID)) ?? false)
                {
                    // Recipe resource exists
                    var recipe = rawResource.PersistableOverlayResourceRecipes.FirstOrDefault(resource => resource.ItemId.Equals(itemID));
                    if (recipe != null)
                    {
                        recipe.IsVisible = isVisible;
                    }
                }
                else
                {
                    // Recipe resource does net yet exists
                    rawResource?.PersistableOverlayResourceRecipes.Add(new PersistableOverlayResourceRecipe
                    {
                        ItemId = itemID,
                        IsVisible = isVisible
                    });
                }
            }
            else
            {
                // RawResource does not yet exists
                PersistableOverlayResources.Add(new PersistableOverlayResource
                {
                    ItemId = itemIDRawResource
                });
                var rawResource = PersistableOverlayResources.FirstOrDefault(resource => resource.ItemId.Equals(itemIDRawResource));
                rawResource?.PersistableOverlayResourceRecipes.Add(new PersistableOverlayResourceRecipe
                {
                    ItemId = itemID,
                    IsVisible = isVisible
                });
            }

            SaveRelatedPricesConfig();
        }

        #endregion
    }
}
