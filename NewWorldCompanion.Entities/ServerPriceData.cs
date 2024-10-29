using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class ServerPriceData
    {
        [JsonPropertyName("server_id")]
        public string ServerId { get; set; } = string.Empty;

        [JsonPropertyName("daily")]
        public Dictionary<string, List<PriceData>> Daily { get; set; } = new();

        [JsonPropertyName("hourly")]
        public Dictionary<string, List<PriceData>> Hourly { get; set; } = new();
    }

    public class PriceData
    {
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; } = 0;

        [JsonPropertyName("min_price")]
        public int MinPrice { get; set; } = 0;

        [JsonPropertyName("max_price")]
        public int MaxPrice { get; set; } = 0;

        [JsonPropertyName("mean_price")]
        public int MeanPrice { get; set; } = 0;

        [JsonPropertyName("median_price")]
        public int MedianPrice { get; set; } = 0;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 0;

        [JsonPropertyName("means")]
        public List<List<double>> Means { get; set; } = new();
    }
}
