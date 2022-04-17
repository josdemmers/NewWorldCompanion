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
        List<CraftingRecipe> GetCraftingRecipes();
        bool IsBindOnPickup(string itemName);
        string GetItemId(string itemName);
        MasterItemDefinitionsCraftingJson? GetItem(string itemId);
        string GetLevenshteinItemName(string itemName);
    }
}
