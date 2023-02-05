using NewWorldCompanion.Entities;
using System.Collections.Generic;

namespace NewWorldCompanion.Interfaces
{
    public interface ICraftingRecipeManager
    {
        bool Available { get; }

        List<CraftingRecipe> CraftingRecipes
        {
            get;
        }

        void SaveRecipes();
    }
}
