namespace NewWorldCompanion.Entities
{
    public class MasterItemDefinitionsCraftingJson
    {
        public string ItemID { get; set; } = string.Empty;
        /// <value>Contains master name for localisation</value> 
        public string Name { get; set; } = string.Empty;
        /// <value>Matches RequiredAchievementID from CraftingRecipe</value> 
        public string SalvageAchievement { get; set; } = string.Empty;
    }
}
