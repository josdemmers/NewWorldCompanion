using System;
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
    }
}
