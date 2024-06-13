namespace HackerNewsBackend.Domain.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public class Item
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("by")]
        public string By { get; set; }
        public string Text { get; set; }
        public bool Dead { get; set; }
        public string Parent { get; set; }
        public int? Poll { get; set; }
        [JsonPropertyName("kids")]
        public List<int> Kids { get; set; }
        public string Url { get; set; }
        [JsonPropertyName("score")]
        public int? Score { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("descendants")]
        public int? Descendants { get; set; }
    }
}
