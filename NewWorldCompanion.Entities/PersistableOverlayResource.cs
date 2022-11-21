using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class PersistableOverlayResource
    {
        public string ItemId { get; set; } = string.Empty;
        public List<PersistableOverlayResourceRecipe> PersistableOverlayResourceRecipes { get; set; } = new List<PersistableOverlayResourceRecipe> { };
    }

    public class PersistableOverlayResourceRecipe
    {
        public string ItemId { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = false;
    }
}
