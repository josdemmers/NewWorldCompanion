using System.Text.Json.Serialization;

namespace NewWorldCompanion.Entities
{
    public class CraftingRecipe
    {
        public string Id { get; set; } = string.Empty;
        public bool Learned { get; set; } = false;

        [JsonIgnore]
        public string ItemID { get; set; } = string.Empty;
        [JsonIgnore]
        public string Localisation { get; set; } = string.Empty;
        [JsonIgnore]
        public string Tradeskill { get; set; } = string.Empty;
        [JsonIgnore]
        public string Url
        {
            get
            {
                return $"https://nwdb.info/db/item/{ItemID}";
            }
        }
    }
}
