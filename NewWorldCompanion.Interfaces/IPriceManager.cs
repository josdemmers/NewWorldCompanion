using NewWorldCompanion.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Interfaces
{
    public interface IPriceManager
    {
        List<PriceServer> Servers { get; }

        NwmarketpriceJson GetPriceData(string itemName);
        void UpdatePriceData(string itemName);
    }
}
