using NewWorldCompanion.Entities;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NewWorldCompanion.Services
{
    public class CraftingRecipeManager : ICraftingRecipeManager
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly INewWorldDataStore _newWorldDataStore;

        private List<CraftingRecipe> _craftingRecipes = new List<CraftingRecipe>();

        // Start of Constructor region

        #region Constructor

        public CraftingRecipeManager(IEventAggregator eventAggregator, INewWorldDataStore newWorldDataStore)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;

            // Init stores
            _newWorldDataStore = newWorldDataStore;

            // Init recipes
            LoadRecipes();
        }

        #endregion

        // Start of Properties region

        #region Properties

        public List<CraftingRecipe> CraftingRecipes { get => _craftingRecipes; }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Methods region

        #region Methods

        private void LoadRecipes()
        {
            _craftingRecipes.Clear();

            string fileName = "CraftingRecipeProgress.json";
            if (File.Exists(fileName))
            {
                using FileStream stream = File.OpenRead(fileName);
                _craftingRecipes = JsonSerializer.Deserialize<List<CraftingRecipe>>(stream) ?? new List<CraftingRecipe>();
            }

            // Update recipe list
            var recipesJson = _newWorldDataStore.GetCraftingRecipes();
            foreach (var recipeJson in recipesJson)
            {
                var recipe = _craftingRecipes.FirstOrDefault(r => r.Id.Equals(recipeJson.Id));
                if (recipe == null)
                {
                    _craftingRecipes.Add(recipeJson);
                }
                else
                {
                    recipe.ItemID = recipeJson.ItemID;
                    recipe.Localisation = recipeJson.Localisation;
                    recipe.Tradeskill = recipeJson.Tradeskill;
                }
            }

            // Sort list
            _craftingRecipes.Sort((x, y) =>
            {
                int result = string.Compare(x.Tradeskill, y.Tradeskill, StringComparison.Ordinal);
                return result != 0 ? result : string.Compare(x.Localisation, y.Localisation, StringComparison.Ordinal);
            });

            // Save recipe progress
            SaveRecipes();
        }

        public void SaveRecipes()
        {
            string fileName = "CraftingRecipeProgress.json";
            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonSerializer.Serialize(stream, CraftingRecipes, options);
        }

        #endregion
    }
}
