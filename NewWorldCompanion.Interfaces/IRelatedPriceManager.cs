using NewWorldCompanion.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Interfaces
{
    public interface IRelatedPriceManager
    {
        List<PersistableOverlayResource> PersistableOverlayResources
        {
            get;
        }

        void SaveRelatedPricesConfig();
        void SetRawResourceRecipeVisibility(string itemIDRawResource, string itemID, bool isVisible);
    }
}
