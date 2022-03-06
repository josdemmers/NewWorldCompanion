using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class NwmarketpriceJson
    {
        public string item_name { get; set; } = string.Empty;
        public string last_checked { get; set; } = string.Empty;
        public string recent_lowest_price { get; set; } = string.Empty;
        //public double recent_lowest_price { get; set; } = 0.0;
    }
}
