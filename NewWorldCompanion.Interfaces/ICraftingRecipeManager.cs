using NewWorldCompanion.Entities;
using System.Collections.Generic;

namespace NewWorldCompanion.Interfaces
{
    public interface ICraftingRecipeManager
    {
        List<CraftingRecipe> CraftingRecipes
        {
            get;
        }

        void SaveRecipes();
    }
}
