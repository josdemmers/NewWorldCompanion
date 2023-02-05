using NewWorldCompanion.Constants;
using NewWorldCompanion.Entities;
using NewWorldCompanion.Events;
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
using System.Windows.Shapes;
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

        private bool _available = false;

        private string _loadStatusItemDefinitions = string.Empty;
        private string _loadStatusCraftingRecipes = string.Empty;
        private string _loadStatusHouseItems = string.Empty;
        private string _loadStatusLocalisation = string.Empty;


        // Start of Constructor region

        #region Constructor

        public NewWorldDataStore(IEventAggregator eventAggregator)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init store data
            Task.Run(() => UpdateStoreData());
        }

        #endregion

        // Start of Properties region

        #region Properties

        public bool Available { get => _available; set => _available = value; }
        public string LoadStatusItemDefinitions { get => _loadStatusItemDefinitions; set => _loadStatusItemDefinitions = value; }
        public string LoadStatusCraftingRecipes { get => _loadStatusCraftingRecipes; set => _loadStatusCraftingRecipes = value; }
        public string LoadStatusHouseItems { get => _loadStatusHouseItems; set => _loadStatusHouseItems = value; }
        public string LoadStatusLocalisation { get => _loadStatusLocalisation; set => _loadStatusLocalisation = value; }

        #endregion

        // Start of Methods region

        #region Methods

        public void UpdateStoreData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = string.Empty;

            _loadStatusItemDefinitions = $"ItemDefinitions: 0. Loading common items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();

            // MasterItemDefinitions Common
            _masterItemDefinitionsJson.Clear();
            var masterItemDefinitionsJson = new List<MasterItemDefinitionsJson>();
            resourcePath = @".\Data\MasterItemDefinitions_Common.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
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

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}. Loading crafting items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();

            // MasterItemDefinitions Crafting
            masterItemDefinitionsJson.Clear();
            resourcePath = @".\Data\MasterItemDefinitions_Crafting.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
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

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}. Loading loot items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();

            // MasterItemDefinitions Loot
            masterItemDefinitionsJson.Clear();
            resourcePath = @".\Data\MasterItemDefinitions_Loot.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
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

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}. Loading quest items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();

            // MasterItemDefinitions Quest
            masterItemDefinitionsJson.Clear();
            resourcePath = @".\Data\MasterItemDefinitions_Quest.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
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

            _loadStatusItemDefinitions = $"ItemDefinitions: {_masterItemDefinitionsJson.Count}";
            _loadStatusCraftingRecipes = $"CraftingRecipes: 0. Loading recipes";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();

            // CraftingRecipe Json
            _craftingRecipesJson.Clear();
            resourcePath = @".\Data\CraftingRecipes.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
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

            _loadStatusCraftingRecipes = $"CraftingRecipes: {_craftingRecipesJson.Count}";
            _loadStatusHouseItems = $"HouseItems: 0. Loading items";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();

            // HouseItems Json
            _houseItemsJson.Clear();
            resourcePath = @".\Data\HouseItems.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    _houseItemsJson = JsonSerializer.Deserialize<List<HouseItemsJson>>(stream) ?? new List<HouseItemsJson>();
                }
            }

            _loadStatusHouseItems = $"HouseItems: {_houseItemsJson.Count}";
            _loadStatusLocalisation = $"Localisation: 0. Loading localisations";
            _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();

            // ItemDefinitionsLocalisation - Itemdefinitions
            _itemDefinitionsLocalisation.Clear();
            resourcePath = @".\Data\javelindata_itemdefinitions_master.loc.xml";
            using (FileStream? stream = File.OpenRead(resourcePath))
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

                        _loadStatusLocalisation = $"Localisation data: {_itemDefinitionsLocalisation.Count}";
                        _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
                    }
                }
            }

            // ItemDefinitionsLocalisation - Itemdefinitions - Cleanup duplicates
            _itemDefinitionsLocalisation.Remove("ArrowBT2_MasterName".ToLower());
            _itemDefinitionsLocalisation.Remove("ArrowBT4_MasterName".ToLower());
            _itemDefinitionsLocalisation.Remove("ArrowBT5_MasterName".ToLower());

            // ItemDefinitionsLocalisation - HouseItems
            resourcePath = @".\Data\javelindata_housingitems.loc.xml";
            using (FileStream? stream = File.OpenRead(resourcePath))
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

                        _loadStatusLocalisation = $"Localisation data: {_itemDefinitionsLocalisation.Count}";
                        _eventAggregator.GetEvent<NewWorldDataStoreStatusUpdated>().Publish();
                    }
                }
            }

            // Finished initializing data. Inform subscribers.
            Available = true;
            _eventAggregator.GetEvent<NewWorldDataStoreUpdated>().Publish();
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
            resourcePath = @".\Data\MasterItemDefinitions_Crafting.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
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

        public CraftingRecipeJson GetCraftingRecipeDetails(string itemId)
        {
            var craftingRecipesJson = new CraftingRecipeJson();
            craftingRecipesJson = _craftingRecipesJson.FirstOrDefault(recipe => recipe.ItemID.ToLower().Equals(itemId), craftingRecipesJson);
            return craftingRecipesJson;
        }

        #endregion

    }
}
