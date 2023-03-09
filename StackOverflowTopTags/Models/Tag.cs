using System.Text.Json.Serialization;

namespace StackOverflowTopTags.Models
{
    public class Tag
    {
        public string Name { get; set; }
        [JsonPropertyName("count")]
        public int AbsolutePopularity { get; set; }
        public double RelativePopularity { get; set; }
    }
}
