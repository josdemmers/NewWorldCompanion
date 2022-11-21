using NewWorldCompanion.Constants;
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

        private List<MasterItemDefinitionsJson> _masterItemDefinitionsJson = new List<MasterItemDefinitionsJson>();
        private List<CraftingRecipeJson> _craftingRecipesJson = new List<CraftingRecipeJson>();
        private List<HouseItemsJson> _houseItemsJson = new List<HouseItemsJson>();
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

            // MasterItemDefinitions Common
            _masterItemDefinitionsJson.Clear();
            var masterItemDefinitionsJson = new List<MasterItemDefinitionsJson>();
            resourcePath = "MasterItemDefinitions_Common.json";
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
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                }
            }

            // MasterItemDefinitions Crafting
            masterItemDefinitionsJson.Clear();
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
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                }
            }

            // MasterItemDefinitions Loot
            masterItemDefinitionsJson.Clear();
            resourcePath = "MasterItemDefinitions_Loot.json";
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
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
                }
            }

            // MasterItemDefinitions Quest
            masterItemDefinitionsJson.Clear();
            resourcePath = "MasterItemDefinitions_Quest.json";
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
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                    _masterItemDefinitionsJson.AddRange(masterItemDefinitionsJson);
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
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    _craftingRecipesJson = JsonSerializer.Deserialize<List<CraftingRecipeJson>>(stream, options) ?? new List<CraftingRecipeJson>();
                }
            }

            // HouseItems Json
            _houseItemsJson.Clear();
            resourcePath = "HouseItems.json";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    _houseItemsJson = JsonSerializer.Deserialize<List<HouseItemsJson>>(stream) ?? new List<HouseItemsJson>();
                }
            }

            // ItemDefinitionsLocalisation
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

                        // Supported items so far:
                        // MasterItemDefinitions_Common.json
                        // MasterItemDefinitions_Crafting.json
                        // MasterItemDefinitions_Loot.json
                        // MasterItemDefinitions_Quest.json
                        if (_masterItemDefinitionsJson.Any(d => d.Name?.Equals("@" + key, StringComparison.OrdinalIgnoreCase) ?? false))
                        {
                            _itemDefinitionsLocalisation.TryAdd(key.ToLower(), value);
                        }
                    }
                }
            }

            // ItemDefinitionsLocalisation - HouseItems
            resourcePath = "javelindata_housingitems.loc.xml";
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

                        // Supported items so far:
                        // HouseItems.json
                        if (_houseItemsJson.Any(d => d.Name?.Equals("@" + key, StringComparison.OrdinalIgnoreCase) ?? false))
                        {
                            _itemDefinitionsLocalisation.TryAdd(key.ToLower(), value);
                        }
                    }
                }
            }
        }

        public List<CraftingRecipe> GetCraftingRecipes()
        {
            List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();
            foreach (var craftingRecipeJson in _craftingRecipesJson.FindAll(recipe => !string.IsNullOrWhiteSpace(recipe.RequiredAchievementID)))
            {
                string id = craftingRecipeJson.RequiredAchievementID;
                string tradeskill = craftingRecipeJson.Tradeskill;
                string itemId = _masterItemDefinitionsJson.FirstOrDefault(d => d.SalvageAchievement.Equals(craftingRecipeJson.RequiredAchievementID, StringComparison.OrdinalIgnoreCase))?.ItemID ?? string.Empty;
                string localisationId = _masterItemDefinitionsJson.FirstOrDefault(d => d.SalvageAchievement.Equals(craftingRecipeJson.RequiredAchievementID, StringComparison.OrdinalIgnoreCase))?.Name ?? string.Empty;
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

            // Workaround for adding MusicSheets as CraftingRecipe
            foreach (var masterItemDefinitionsJson in _masterItemDefinitionsJson)
            {
                string id = masterItemDefinitionsJson.ItemID;
                string tradeskill = masterItemDefinitionsJson.TradingFamily;
                string itemId = masterItemDefinitionsJson.ItemID;
                string localisationId = masterItemDefinitionsJson.Name;
                string localisation = _itemDefinitionsLocalisation.GetValueOrDefault(localisationId.Trim(new char[] { '@' }).ToLower()) ?? localisationId.Trim(new char[] { '@' });

                if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(tradeskill) &&
                    !string.IsNullOrWhiteSpace(itemId) && !string.IsNullOrWhiteSpace(localisationId) && !string.IsNullOrWhiteSpace(localisation) &&
                    tradeskill.Equals(TradeskillConstants.MusicSheets))
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

        public List<MasterItemDefinitionsJson> GetOverlayResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = string.Empty;

            // MasterItemDefinitions Crafting
            var masterItemDefinitionsJson = new List<MasterItemDefinitionsJson>();
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
                    options.Converters.Add(new IntConverter());

                    masterItemDefinitionsJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsJson>>(stream, options) ?? new List<MasterItemDefinitionsJson>();
                }
            }

            return masterItemDefinitionsJson.FindAll(items => items.TradingFamily.Equals("RawResources") &&
                items.ItemClass.Contains("+") &&
                !items.ItemClass.Contains("WeaponSchematic"));
        }

        public bool IsBindOnPickup(string itemName)
        {
            var localisationId = _itemDefinitionsLocalisation.FirstOrDefault(x => x.Value.Replace("\\n", " ").Equals(itemName, StringComparison.OrdinalIgnoreCase)).Key;
            var item = _masterItemDefinitionsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            var houseItem = _houseItemsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item.BindOnPickup;
            }
            if (houseItem != null)
            {
                return houseItem.BindOnPickup;
            }
            return true;
        }

        public string GetItemId(string itemName)
        {
            var localisationId = _itemDefinitionsLocalisation.FirstOrDefault(x => x.Value.Replace("\\n", " ").Equals(itemName, StringComparison.OrdinalIgnoreCase)).Key;
            MasterItemDefinitionsJson? item = _masterItemDefinitionsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            HouseItemsJson? houseItem = _houseItemsJson.FirstOrDefault(i => i.Name.Equals($"@{localisationId}", StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item.ItemID;
            }
            if (houseItem != null)
            {
                return houseItem.HouseItemID;
            }
            return string.Empty;
        }

        public ItemDefinition? GetItem(string itemId)
        {
            var item = _masterItemDefinitionsJson.FirstOrDefault(i => i.ItemID.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            var houseItem = _houseItemsJson.FirstOrDefault(i => i.HouseItemID.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item;
            }
            if (houseItem != null)
            {
                return houseItem;
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

            //return currentDistance <= Math.Max(3, itemName.Length) ? currentItem : itemName;
            return currentDistance <= 3 ? currentItem : itemName;
        }

        public string GetItemLocalisation(string itemMasterName)
        {
            return _itemDefinitionsLocalisation.GetValueOrDefault(itemMasterName.Trim(new char[] { '@' }).ToLower()) ?? itemMasterName.Trim(new char[] { '@' });
        }

        public List<CraftingRecipeJson> GetRelatedRecipes(string itemId)
        {
            // Note: The following recipes are ignored:
            // - Empty recipe.ItemID strings because those are all from downgrade recipes.
            // - Armor / weapons because results are random and we have no price data.
            // - Crafting quest recipes.
            return _craftingRecipesJson.FindAll(recipe => 
                !string.IsNullOrWhiteSpace(recipe.ItemID) &&
                !recipe.CraftingCategory.Equals("Armor") &&
                !recipe.CraftingCategory.Equals("CraftingQuestRecipe") &&
                !recipe.CraftingCategory.Equals("MagicStaves") &&
                !recipe.CraftingCategory.Equals("Tools") &&
                !recipe.CraftingCategory.Equals("Weapons") &&
                !recipe.CraftingCategory.StartsWith("Salvage") &&
                !recipe.CraftingCategory.StartsWith("TimelessShards") &&
                (recipe.Ingredient1.Equals(itemId) ||
                (recipe.Ingredient1.Equals(itemId.Substring(0,itemId.Length-2)) && recipe.Type1.Equals("Category_Only")) ||
                recipe.Ingredient2.Equals(itemId) ||
                recipe.Ingredient3.Equals(itemId) ||
                recipe.Ingredient4.Equals(itemId) ||
                recipe.Ingredient5.Equals(itemId) ||
                recipe.Ingredient6.Equals(itemId) ||
                recipe.Ingredient7.Equals(itemId)));
        }

        #endregion

    }
}
