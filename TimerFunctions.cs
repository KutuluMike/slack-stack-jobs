using System;
using System.Linq;
using System.Net;
using System.Net.Http;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using SlackStackJobs.Models;

namespace SlackStackJobs
{
    public static class TimerFunctions
    {
        [FunctionName("FeedTimer")]
        public static void FeedTimer(
            [TimerTrigger("0 */30 * * * *")] TimerInfo timer,
            [CosmosDB("FeedStateDatabase", "FeedItemsCollection", ConnectionStringSetting = "slackstackfeed_CosmosDB", Id = "state")] SlackStackState state,
            [Queue("slackfeed-items")] ICollector<QueuedJob> triggers,
            ILogger log)
        {
            if (state == null)
            {
                log.LogDebug("no state object.");
            }

            if (state.Feeds == null)
            {
                log.LogDebug("no state feeds.");
                return;
            }

            if (!state.Feeds.Any())
            {
                log.LogDebug("state feeds empty.");
            }

            // NOTE: KeyValuePair has no built-in deconstructor, but tuple does. We could either write an extension method, or convert the Dictionary items.
            foreach ((var key, var feed) in state.Feeds.Select(x => (x.Key, x.Value)))
            {
                if (key == null || feed == null || feed.Teams == null || !feed.Teams.Any())
                {
                    log.LogDebug("no feed data.");
                    continue;
                }

                if (TimerFunctions.IsFeedUpdated(feed, log))
                {
                    foreach (var team in feed.Teams)
                    {
                        triggers.Add(new QueuedJob
                        {
                            Feed = key,
                            Team = team,
                        });
                    }
                }

                feed.Processed = DateTime.Now;
            }

            state.Processed = DateTime.Now;
        }

        private static bool IsFeedUpdated(StackFeed feed, ILogger log)
        {
            log.LogDebug($"checking for updated feed since {feed.Updated}");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.IfModifiedSince = feed.Updated;
                var result = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, feed.SourceUri)).Result;
                if (result.StatusCode == HttpStatusCode.NotModified)
                {
                    return false;
                }
            }

            return true;
        }
    }
}