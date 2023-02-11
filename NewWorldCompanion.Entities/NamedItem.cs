using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class NamedItem : ItemDefinition
    {
        /// <value>Item category. Used to categorise, filter, and sort items.</value>
        public string ItemClass { get; set; } = string.Empty;
        /// <value>Unique identifier for items. Nwdb uses this to identify items</value>
        public string ItemID { get; set; } = string.Empty;
        public string Localisation { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public int Tier { get; set; } = 0;

        public string Url
        {
            get
            {
                return $"https://nwdb.info/db/item/{ItemID}";
            }
        }
    }
}