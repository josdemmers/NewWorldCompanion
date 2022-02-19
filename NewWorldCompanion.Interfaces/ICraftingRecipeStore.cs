using NewWorldCompanion.Entities;
using System.Collections.Generic;

namespace NewWorldCompanion.Interfaces
{
    public interface ICraftingRecipeStore
    {
        List<CraftingRecipe> GetCraftingRecipes();
    }
}
