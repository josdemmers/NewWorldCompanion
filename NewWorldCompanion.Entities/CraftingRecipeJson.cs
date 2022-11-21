using System;

namespace NewWorldCompanion.Entities
{
    public class CraftingRecipeJson
    {
        /// <value>Crafted item Category</value> 
        public string CraftingCategory { get; set; } = string.Empty;
        /// <value>Crafted item ItemId</value> 
        public string ItemID { get; set; } = string.Empty;
        /// <value>Matches SalvageAchievement from MasterItemDefinitionsCraftingJson</value> 
        public string RequiredAchievementID { get; set; } = string.Empty;
        public string Tradeskill { get; set; } = string.Empty;

        public string Ingredient1 { get; set; } = string.Empty;
        public string Ingredient2 { get; set; } = string.Empty;
        public string Ingredient3 { get; set; } = string.Empty;
        public string Ingredient4 { get; set; } = string.Empty;
        public string Ingredient5 { get; set; } = string.Empty;
        public string Ingredient6 { get; set; } = string.Empty;
        public string Ingredient7 { get; set; } = string.Empty;
        public string Type1 { get; set; } = string.Empty;
        public string Type2 { get; set; } = string.Empty;
        public string Type3 { get; set; } = string.Empty;
        public string Type4 { get; set; } = string.Empty;
        public string Type5 { get; set; } = string.Empty;
        public string Type6 { get; set; } = string.Empty;
        public string Type7 { get; set; } = string.Empty;
        public int Qty1 { get; set; } = 0;
        public int Qty2 { get; set; } = 0;
        public int Qty3 { get; set; } = 0;
        public int Qty4 { get; set; } = 0;
        public int Qty5 { get; set; } = 0;
        public int Qty6 { get; set; } = 0;
        public int Qty7 { get; set; } = 0;
    }
}