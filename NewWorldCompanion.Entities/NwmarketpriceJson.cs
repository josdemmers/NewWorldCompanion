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
    public class NwmarketpriceJson
    {
        public string item_name { get; set; } = string.Empty;
        public string nwdb_id { get; set; } = string.Empty;
        public DateTime last_checked { get; set; } = DateTime.MinValue;
        public double recent_lowest_price { get; set; } = 0.0;
        public List<GrapData> price_graph_data { get; set; } = new List<GrapData>();
        /// <summary>
        /// Lowest price seen in the most recent scan compared to the lowest price seen the previous day
        /// </summary>
        public int price_change { get; set; }

        [JsonIgnore]
        public string last_checked_string
        {
            get
            {
                return last_checked.ToString();
            }
        }

        [JsonIgnore]
        public string RecentLowestPriceAvg
        {
            get
            {
                string price = string.Empty;

                try
                {
                    if (price_graph_data.Count() > 0)
                    {
                        GrapData priceData = price_graph_data.Last();
                        price = priceData.RollingAverage.ToString("F2");
                    }
                }
                catch (Exception) { }

                return price;
            }
        }
    }

    public class GrapData
    {
        /*
        // Is the total available of that day. All listings.
        [JsonPropertyName("avail")]
        public int Avail { get; set; }
        [JsonPropertyName("date_only")]
        public string DateOnly { get; set; } = string.Empty;
        [JsonPropertyName("price_date")]
        public DateTime PriceDate { get; set; } = DateTime.MinValue;
        // Lowest price seen that day of any scan.
        [JsonPropertyName("lowest_price")]
        public double LowestPrice { get; set; }
        // The available of the lowest price on the server that day.
        [JsonPropertyName("single_price_avail")]
        public int SinglePriceAvail { get; set; }
        // Average price of the lowest 10 prices of the last scan.
        [JsonPropertyName("avg_price")]
        public double AvgPrice { get; set; }
        // Average available of the lowest 10 prices of the last scan
        [JsonPropertyName("avg_qty")]
        public double AvgQty { get; set; }
        */

        /// <summary>
        /// The exponential moving average of the lowest prices seen each day for the past 15 days.
        /// </summary>
        [JsonPropertyName("rolling_average")]
        public double RollingAverage { get; set; }
    }
}
