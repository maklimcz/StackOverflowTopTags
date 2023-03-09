namespace StackOverflowTopTags.Services
{
    public class ParamsService
    {
        public const int TagsLimit = 1000;
        public const int PageLimit = 100;

        public int PageCount { get => (int) Math.Ceiling(Convert.ToDouble(TagsLimit) / PageLimit); }

        public readonly TimeSpan RefreshInterval;
        public readonly string StoragePath;
        public readonly Uri RemoteUri;

        public ParamsService(IWebHostEnvironment webHostEnvironment) {
            RefreshInterval = TimeSpan.FromMinutes(60); ;
            StoragePath = Path.Combine(webHostEnvironment.WebRootPath, "data");
            RemoteUri = new Uri($"https://api.stackexchange.com/2.3/tags?pagesize={PageLimit}&order=desc&sort=popular&site=stackoverflow");
        }
    }
}
