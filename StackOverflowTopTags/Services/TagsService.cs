using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflowTopTags.Models;
using System.Text.Json;

namespace StackOverflowTopTags.Services
{
    public interface ITagsProvider
    {
        public DateTime GetDateTime();
        public IEnumerable<Tag> GetTags();
    }

    public class FileTagsProvider : ITagsProvider
    {
        private ParamsService paramsService;
        public FileTagsProvider(ParamsService paramsService)
        {
            this.paramsService = paramsService;
        }

        public IEnumerable<Tag> GetTags()
        {
            var fp = Path.Combine(paramsService.StoragePath, "tags.json");
            if (!File.Exists(fp))
            {
                return Enumerable.Empty<Tag>();
            }
            using (var f = File.OpenText(fp))
            {
                return JsonSerializer.Deserialize<IEnumerable<Tag>>(f.ReadToEnd(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }

        public DateTime GetDateTime()
        {
            var fp = Path.Combine(paramsService.StoragePath, "tags.json");
            return File.GetLastWriteTime(fp);
        }
    }

    public class TagsService
    {
        public ITagsProvider TagsProvider { get; }

        public TagsService(ParamsService paramsService)
        {
            TagsProvider = new FileTagsProvider(paramsService);
        }

        public IEnumerable<Tag> GetTags() => TagsProvider.GetTags();
        public DateTime AsOfDate() => TagsProvider.GetDateTime();
    }
}
