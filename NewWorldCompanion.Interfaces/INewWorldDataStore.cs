﻿using NewWorldCompanion.Entities;
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
    }
}