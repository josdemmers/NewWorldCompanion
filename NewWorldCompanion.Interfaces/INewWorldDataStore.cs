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
        string LoadStatusLocalisationNamed { get; }

        List<CraftingRecipe> GetCraftingRecipes();
        bool IsBindOnPickup(string itemName);
        bool IsNamedItem(string itemName);
        string GetItemId(string itemName);
        ItemDefinition? GetItem(string itemId);
        string GetLevenshteinItemName(string itemName);
        List<MasterItemDefinitionsJson> GetOverlayResources();
        List<NamedItem> GetNamedItems();
        string GetItemLocalisation(string itemMasterName);
        string GetNamedItemLocalisation(string itemMasterName);
        List<CraftingRecipeJson> GetRelatedRecipes(string itemId);
        CraftingRecipeJson GetCraftingRecipeDetails(string itemId);
        void UpdateStoreData();
    }
}
