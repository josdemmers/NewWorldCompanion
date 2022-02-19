using NewWorldCompanion.Entities;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

namespace NewWorldCompanion.Services
{
    public class CraftingRecipeStore : ICraftingRecipeStore
    {
        private readonly IEventAggregator _eventAggregator;

        private List<CraftingRecipeJson> _craftingRecipesJson = new List<CraftingRecipeJson>();
        private List<MasterItemDefinitionsCraftingJson> _masterItemDefinitionsCraftingJson = new List<MasterItemDefinitionsCraftingJson>();
        private Dictionary<string, string> _itemDefinitionsLocalisation = new Dictionary<string, string>();

        // Start of Constructor region

        #region Constructor

        public CraftingRecipeStore(IEventAggregator eventAggregator)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init store data
            UpdateStoreData();
        }

        #endregion

        // Start of Properties region

        #region Properties

        //public List<CraftingRecipe> CraftingRecipes { get => _craftingRecipes; set => _craftingRecipes = value; }

        #endregion

        // Start of Methods region

        #region Methods

        private void UpdateStoreData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = string.Empty;

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

            // MasterItemDefinitionsCrafting Json
            _masterItemDefinitionsCraftingJson.Clear();
            resourcePath = "MasterItemDefinitions_Crafting.json";
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    _masterItemDefinitionsCraftingJson = JsonSerializer.Deserialize<List<MasterItemDefinitionsCraftingJson>>(stream) ?? new List<MasterItemDefinitionsCraftingJson>();
                    _masterItemDefinitionsCraftingJson.RemoveAll(d => !_craftingRecipesJson.Any(r => r.RequiredAchievementID.Equals(d.SalvageAchievement, StringComparison.OrdinalIgnoreCase)));
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

        #endregion
    }
}
