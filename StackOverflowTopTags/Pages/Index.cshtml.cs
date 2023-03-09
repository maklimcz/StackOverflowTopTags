using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflowTopTags.Models;
using StackOverflowTopTags.Services;
using System.Text.Json;

namespace StackOverflowTopTags.Pages
{
    public class IndexModel : PageModel
    {
        public TagsService TagsService;
        public IEnumerable<Tag> Tags { get; private set; }
        public DateTime AsOfDate { get; private set; }

        public IndexModel(TagsService tagsService)
        {
            TagsService = tagsService;
            Tags = new List<Tag>();
        }

        public void OnGet()
        {
            Tags = TagsService.GetTags();
            AsOfDate = TagsService.AsOfDate();
        }

        public string WordCloud(int n)
        {
            return JsonSerializer.Serialize(Tags.Take(n).Select(t => new dynamic[] { t.Name, t.RelativePopularity }).ToArray());
        }
    }
}