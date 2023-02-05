using NewWorldCompanion.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Interfaces
{
    public interface INewWorldDataStore
    {
        bool Available { get; }
        string LoadStatusItemDefinitions { get; }
        string LoadStatusCraftingRecipes { get; }
        string LoadStatusHouseItems { get; }
        string LoadStatusLocalisation { get; }

        List<CraftingRecipe> GetCraftingRecipes();
        bool IsBindOnPickup(string itemName);
        string GetItemId(string itemName);
        ItemDefinition? GetItem(string itemId);
        string GetLevenshteinItemName(string itemName);
        List<MasterItemDefinitionsJson> GetOverlayResources();
        string GetItemLocalisation(string itemMasterName);
        List<CraftingRecipeJson> GetRelatedRecipes(string itemId);
        CraftingRecipeJson GetCraftingRecipeDetails(string itemId);
        void UpdateStoreData();
    }
}
