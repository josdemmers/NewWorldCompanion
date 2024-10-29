using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace NewWorldCompanion.Entities
{
    public class Nwmarketprice
    {
        public string ItemId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;

        public int Days { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
        public string LastUpdatedString
        {
            get
            {
                return LastUpdated.ToString();
            }
        }
        /// <summary>
        /// Lowest price seen in the most recent scan compared to the lowest price seen the previous day
        /// </summary>
        public int PriceChange { get; set; }
        public double RecentLowestPrice { get; set; } = 0.0;
        /// <summary>
        /// RollingAverage
        /// </summary>
        public double RecentLowestPriceAvg { get; set; } = 0.0;
    }
}
