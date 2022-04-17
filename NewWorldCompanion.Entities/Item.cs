using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class Item
    {
        public int Count { get; set; } = 0;
        /// <value>Unique identifier for items. Nwdb uses this to identify items</value>
        public string ItemID { get; set; } = string.Empty;
        public string Localisation { get; set; } = string.Empty;
        /// <value>Contains master name for localisation</value>
        public string Name { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;

        [JsonIgnore]
        public string Url
        {
            get
            {
                return $"https://nwdb.info/db/item/{ItemID}";
            }
        }
    }
}
