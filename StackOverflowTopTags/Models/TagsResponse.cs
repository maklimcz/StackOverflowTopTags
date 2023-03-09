using System.Text.Json.Serialization;

namespace StackOverflowTopTags.Models
{
    public class TagsResponse
    {
        public Tag[] Items { get; set; }
    }
}
