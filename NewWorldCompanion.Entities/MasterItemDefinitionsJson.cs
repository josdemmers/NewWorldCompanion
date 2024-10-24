﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class MasterItemDefinitionsJson : ItemDefinition
    {
        /// <value>Unique identifier for items. Nwdb uses this to identify items</value>
        public string ItemID { get; set; } = string.Empty;
        /// <value>Matches RequiredAchievementID from CraftingRecipeJson</value>
        public string SalvageAchievement { get; set; } = string.Empty;
        /// <value>Used as Tradeskill value for MusicSheets in CraftingRecipe</value>
        public string TradingFamily { get; set; } = string.Empty;
        /// <value>Item category. Used to categorise, filter, and sort resources.</value>
        public string ItemClass { get; set; } = string.Empty;
        /// <value>Item tier. Used to sort resources.</value>
        public int Tier { get; set; } = 0;
    }
}
