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
        public string recent_lowest_price { get; set; } = string.Empty;
        //public double recent_lowest_price { get; set; } = 0.0;
        public List<object> avg_graph_data { get; set; } = new List<object>();

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
                        JsonElement priceData = (JsonElement)avg_graph_data.Last();
                        switch (priceData.ValueKind)
                        {
                            case JsonValueKind.Number:
                                price = decimal.Parse(priceData.ToString(), style, CultureInfo.InvariantCulture).ToString("F2");
                                break;
                            case JsonValueKind.Array:
                                price = decimal.Parse(priceData[1].ToString(), style, CultureInfo.InvariantCulture).ToString("F2");
                                break;
                        }
                    }
                }
                catch (Exception){}
                
                return price;
            }
        }
    }
}
