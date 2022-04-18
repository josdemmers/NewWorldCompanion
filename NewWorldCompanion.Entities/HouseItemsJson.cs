using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class HouseItemsJson : ItemDefinition
    {
        /// <value>Unique identifier for items. Nwdb uses this to identify items</value>
        public string HouseItemID { get; set; } = string.Empty;
    }
}
