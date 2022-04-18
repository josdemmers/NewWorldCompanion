using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class ItemDefinition
    {
        /// <value>Contains master name for localisation</value>
        public string Name { get; set; } = string.Empty;
        /// <value>Used to define if an item is tradable</value> 
        public bool BindOnPickup { get; set; } = false;
    }
}
