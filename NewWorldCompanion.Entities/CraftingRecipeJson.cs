using System;

namespace NewWorldCompanion.Entities
{
    public class CraftingRecipeJson
    {
        /// <value>Matches SalvageAchievement from MasterItemDefinitionsCraftingJson</value> 
        public string RequiredAchievementID { get; set; } = string.Empty;
        public string Tradeskill { get; set; } = string.Empty;
    }
}