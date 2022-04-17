using NewWorldCompanion.Entities;
using NewWorldCompanion.Helpers;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NewWorldCompanion.Services
{
    public class NewWorldDataStore : INewWorldDataStore
    {
        private readonly IEventAggregator _eventAggregator;

        private List<MasterItemDefinitionsCraftingJson> _masterItemDefinitionsCraftingJson = new List<MasterItemDefinitionsCraftingJson>();
        private List<CraftingRecipeJson> _craftingRecipesJson = new List<CraftingRecipeJson>();
        private Dictionary<string, string> _itemDefinitionsLocalisation = new Dictionary<string, string>();

        // Start of Constructor region

        #region Constructor

        public NewWorldDataStore(IEventAggregator eventAggregator)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init store data
            UpdateStoreData();
        }

        #endregion

        // Start of Properties region

        #region Properties



        #endregion

        // Start of Methods region

        #region Methods

        private void UpdateStoreData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = string.Empty;

            // MasterItemDefinitionsCrafting Json
            _masterItemDefinitionsCraftingJson.Clear();
            resourcePath = "MasterItemDefinitions_Crafting.json";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());

                    _masterItemDefinitionsCraftingJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsCraftingJson>>(stream, options) ?? new List<MasterItemDefinitionsCraftingJson>();
                    //_masterItemDefinitionsCraftingJson.RemoveAll(item => item.BindOnPickup);
                }
            }

            // CraftingRecipe Json
            _craftingRecipesJson.Clear();
            resourcePath = "CraftingRecipes.json";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    _craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream) ?? new List<CraftingRecipeJson>();
                    _craftingRecipesJson.RemoveAll(r => string.IsNullOrWhiteSpace(r.RequiredAchievementID));
                }
            }

            // ItemDefinitionsLocalisation Xml
            _itemDefinitionsLocalisation.Clear();
            resourcePath = "javelindata_itemdefinitions_master.loc.xml";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    var xml = XDocument.Load(stream);
                    var query = from loc in xml.Descendants()
                                where loc.Name.LocalName == "string"
                                select loc;

                    foreach (var loc in query)
                    {
                        string key = loc.Attribute("key")?.Value ?? string.Empty;
                        string value = loc.Value;

                        // TODO Add support for items not in _masterItemDefinitionsCraftingJson
                        if (_masterItemDefinitionsCraftingJson.Any(d => d.Name?.Equals("@" + key, StringComparison.OrdinalIgnoreCase) ?? false))
                        {
                            _itemDefinitionsLocalisation.Add(key.ToLower(), value);
                        }
                    }
                }
            }
        }

        public List<CraftingRecipe> GetCraftingRecipes()
        {
            List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();
            foreach (var craftingRecipeJson in _craftingRecipesJson)
            {
                string id = craftingRecipeJson.RequiredAchievementID;
                string tradeskill = craftingRecipeJson.Tradeskill;
                string itemId = _masterItemDefinitionsCraftingJson.FirstOrDefault(d => d.SalvageAchievement.Equals(craftingRecipeJson.RequiredAchievementID, StringComparison.OrdinalIgnoreCase))?.ItemID ?? string.Empty;
                string localisationId = _masterItemDefinitionsCraftingJson.FirstOrDefault(d => d.SalvageAchievement.Equals(craftingRecipeJson.RequiredAchievementID, StringComparison.OrdinalIgnoreCase))?.Name ?? string.Empty;
                string localisation = _itemDefinitionsLocalisation.GetValueOrDefault(localisationId.Trim(new char[] { '@' }).ToLower()) ?? localisationId.Trim(new char[] { '@' });

                if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(tradeskill) &&
                    !string.IsNullOrWhiteSpace(itemId) && !string.IsNullOrWhiteSpace(localisationId) && !string.IsNullOrWhiteSpace(localisation))
                {
                    craftingRecipes.Add(new CraftingRecipe
                    {
                        Id = id,
                        ItemID = itemId,
                        Localisation = localisation,
                        Tradeskill = tradeskill
                    });
                }
            }

            return craftingRecipes;
        }

        public bool IsBindOnPickup(string itemName)
        {
            // TODO Needs improvement. Look into more properties like IsTradable, quests, etc.
            var localisationId = _itemDefinitionsLocalisation.FirstOrDefault(x => x.Value.Equals(itemName, StringComparison.OrdinalIgnoreCase)).Key;
            var item = _masterItemDefinitionsCraftingJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item.BindOnPickup;
            }
            return true;
        }

        public string GetItemId(string itemName)
        {
            var localisationId = _itemDefinitionsLocalisation.FirstOrDefault(x => x.Value.Equals(itemName, StringComparison.OrdinalIgnoreCase)).Key;
            var item = _masterItemDefinitionsCraftingJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item.ItemID;
            }
            return string.Empty;
        }

        public MasterItemDefinitionsCraftingJson? GetItem(string itemId)
        {
            var item = _masterItemDefinitionsCraftingJson.FirstOrDefault(i => i.ItemID.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item;
            }
            return null;
        }

        public string GetLevenshteinItemName(string itemName)
        {
            int currentDistance = int.MaxValue;
            string currentItem = string.Empty;

            foreach (var item in _itemDefinitionsLocalisation)
            {
                int distance = LevenshteinDistance.Compute(itemName, item.Value);
                if (distance <= currentDistance)
                {
                    currentDistance = distance;
                    currentItem = item.Value;
                }
                if (currentDistance == 0) break;
            }

            Debug.WriteLine($"Levenshtein. Item: {itemName}, Match: {currentItem}, Distance: {currentDistance}");

            return currentDistance <= 3 ? currentItem : itemName;
        }

        #endregion


    }
}
