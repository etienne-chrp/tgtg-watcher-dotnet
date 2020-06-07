using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiClient
{
    class BussinessesItemsResponse
    {
        [JsonPropertyName("items")]
        public List<BussinessesItem> BusinessesItems { get; set; }
    }

    public class BussinessesItem
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
    
        [JsonPropertyName("items_available")]
        public int ItemsAvailable { get; set; }

        [JsonPropertyName("item")]
        public ItemInfo Item { get; set; }

        public class ItemInfo
        {
            [JsonPropertyName("item_id")]
            public string Id { get; set; }
            
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }
    }
}