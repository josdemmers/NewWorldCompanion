using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class NwmarketpriceJson
    {
        public string item_name { get; set; } = string.Empty;
        public string last_checked { get; set; } = string.Empty;
        //public string recent_lowest_price { get; set; } = string.Empty;
        public double recent_lowest_price { get; set; } = 0.0;
        public List<AvgGraph> avg_graph_data { get; set; } = new List<AvgGraph>();

        [JsonIgnore]
        public string RecentLowestPriceAvg
        {
            get 
            {
                string price = string.Empty;
                NumberStyles style = NumberStyles.AllowDecimalPoint;

                try
                {
                    if (avg_graph_data.Count() > 0)
                    {
                        AvgGraph priceData = avg_graph_data.Last();
                        price = priceData.price.ToString("F2");
                    }
                }
                catch (Exception){}
                
                return price;
            }
        }
    }

    public class AvgGraph
    {
        public DateTime datetime { get; set; }
        public double price { get; set; }
    }
}
