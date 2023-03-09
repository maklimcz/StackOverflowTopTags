using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflowTopTags.Models;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace StackOverflowTopTags.Services
{
    public class BackgroundTagsService : IHostedService, IDisposable
    {
        private ParamsService paramsService;
        private ILogger logger;
        private Timer timer;

        public BackgroundTagsService(ParamsService paramsService, ILogger<BackgroundTagsService> logger)
        {
            this.paramsService = paramsService;
            this.logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Background service started");
            var fp = Path.Combine(paramsService.StoragePath, "tags.json");

            var elapsedFromLastPull = DateTime.Now - File.GetLastWriteTime(fp);
            /* If the file described in the path parameter does not exist,
             * then GetLastWriteTime returns 12:00 midnight, January 1, 1601 A.D. (C.E.) UTC, adjusted to local time
             * */
            var timeToNextPull = paramsService.RefreshInterval > elapsedFromLastPull
                ? paramsService.RefreshInterval - elapsedFromLastPull
                : TimeSpan.Zero;

            logger.LogInformation($"Schedule data pulls: first in {timeToNextPull}, later every {paramsService.RefreshInterval}.");

            timer = new Timer(PullTags, null, timeToNextPull, paramsService.RefreshInterval);
            return Task.CompletedTask;
        }

        private async Task<List<Tag>> PullTagsFromRemoteSource()
        {
            var pageNumbers = Enumerable.Range(1, paramsService.PageCount);
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            using (var client = new HttpClient(handler))
            {
                var tasks = new List<Task<TagsResponse>>();

                foreach (var i in pageNumbers)
                {
                    tasks.Add(client.GetFromJsonAsync<TagsResponse>(paramsService.RemoteUri + "&page=" + i));
                }

                await Task.WhenAll(tasks);

                return tasks.SelectMany(r => r.Result.Items).ToList();
            }
        }

        private async void PullTags(object? state)
        {
            try
            {
                logger.LogInformation("Pulling data from remote server started");
                var tags = await PullTagsFromRemoteSource();
                logger.LogInformation($"Pulling data from remote server finished. Got {tags.Count()} entries.");
                logger.LogInformation("Pushing data into local file started");

                var totalCount = tags.Sum(e => e.AbsolutePopularity);
                tags.ForEach(e => { e.RelativePopularity = e.AbsolutePopularity / Convert.ToDouble(totalCount); });

                var fp = Path.Combine(paramsService.StoragePath, "tags.json");
                var json = JsonSerializer.Serialize(tags);
                File.WriteAllText(fp, json);

                logger.LogInformation("Pushing data into local file finished");
            }
            catch (Exception ex)
            {
                logger.LogError("Pulling tags failed. " + ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Background service stopping");
            timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
