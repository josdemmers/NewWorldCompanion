namespace NewWorldCompanion.Entities
{
    public class MasterItemDefinitionsCraftingJson
    {
        /// <value>Unique identifier for items. Nwdb uses this to identify items</value>
        public string ItemID { get; set; } = string.Empty;
        /// <value>Contains master name for localisation</value>
        public string Name { get; set; } = string.Empty;
        /// <value>Used to define if an item is tradable</value> 
        public bool BindOnPickup { get; set; } = false;
        /// <value>Matches RequiredAchievementID from CraftingRecipeJson</value> 
        public string SalvageAchievement { get; set; } = string.Empty;
    }
}
