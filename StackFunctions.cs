using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using AngleSharp.Parser.Html;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SlackStackJobs.Models;

namespace SlackStackJobs
{
    /// <summary>
    /// Azure WebJobs functions for interacting with Slack teams.
    /// </summary>
    public static class StackFunctions
    {
        /// <summary>
        /// WebJob function to post new Stack Exchange messages to one or more Slack channels
        /// </summary>
        /// <param name="job">Queue job information from Azure</param>
        /// <param name="state">Current state of the Slack connection to Stack Exchange</param>
        /// <param name="team">Information about a Slack team with one or more Stack Exchange subscriptions</param>
        /// <param name="processed">Date this feed is being processed</param>
        /// <param name="log">Logging interface</param>
        [FunctionName("StackFeeder")]
        public static void StackFeeder(
            [QueueTrigger("slackfeed-items")] QueuedJob job,
            [CosmosDB("FeedStateDatabase", "FeedItemsCollection", ConnectionStringSetting = "slackstackfeed_CosmosDB", Id = "state")] SlackStackState state,
            [CosmosDB("FeedStateDatabase", "FeedItemsCollection", ConnectionStringSetting = "slackstackfeed_CosmosDB", Id = "{team}")] SlackTeam team,
            DateTime processed,
            ILogger log)
        {
            if (!team.Active || !team.Subscriptions.ContainsKey(job.Feed))
            {
                return;
            }

            var feed = state.Feeds[job.Feed];
            try
            {
                var atom = XDocument.Load(feed.SourceUri.AbsoluteUri).Root;
                XNamespace bs = "http://www.w3.org/2005/Atom";

                DateTime.TryParse(atom.Element(bs + "updated").Value, out var updated);
                if (updated > processed)
                {
                    log.LogDebug("Feed recently updated, scanning for entries...");

                    using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                    {
                        foreach (var entry in atom.Elements(bs + "entry"))
                        {
                            var id = entry.Element(bs + "id")?.Value ?? string.Empty;

                            if (string.IsNullOrEmpty(id) || team.Posted.Contains(id))
                            {
                                continue;
                            }

                            if (!DateTime.TryParse(entry.Element(bs + "published").Value, out var published))
                            {
                                log.LogDebug("Invalid entry data -- missing published date.");
                                continue;
                            }

                            var raw = entry.Element(bs + "summary").Value;
                            var post = StackFunctions.Slackify(raw);

                            var userLink = entry.Element(bs + "author").Element(bs + "uri").Value ?? string.Empty;
                            var match = Regex.Match(userLink, $"https://{feed.Site}.stackexchange.com/users/([0-9]+)");
                            var user = default(StackUser);

                            if (match.Success)
                            {
                                var userData = client.GetStringAsync($"https://api.stackexchange.com/2.2/users/{match.Groups[1].Value}?site={feed.Site}&key=MYe90O9jVj1YJI12XqK0BA((&filter=!)RwdAtHo34gjVfkkY.BZV4L(").Result;
                                var stackData = JsonConvert.DeserializeObject<StackApiResponse<StackUser>>(userData);
                                if (string.IsNullOrEmpty(stackData.ErrorMessage) && stackData.Items.Any())
                                {
                                    user = stackData.Items[0];
                                }
                            }

                            var attachments = new[]
                                {
                                    new
                                    {
                                        mrkdwn_in = new string[] { "text", "fields" },
                                        title = entry.Element(bs + "title")?.Value ?? string.Empty,
                                        title_link = id,
                                        text = post,
                                        thumb_url = feed.LogoUri.AbsoluteUri,
                                        author_name = user?.DisplayName ?? string.Empty,
                                        author_link = user?.Link?.AbsoluteUri ?? string.Empty,
                                        author_icon = user?.ProfileImage?.AbsoluteUri ?? string.Empty,
                                        fields = entry.Elements(bs + "category").Select(tag => new { value = $"`{tag.Attribute("term").Value}`", @short = true }),
                                        ts = new DateTimeOffset(published).ToUnixTimeSeconds()
                                    }
                                };

                            foreach (var channel in team.Subscriptions[job.Feed])
                            {
                                var data = new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                    { "as_user", "false" },
                                    { "username", "Slack Stack Feed" },
                                    { "token", team.BotToken },
                                    { "channel", channel },
                                    { "text", $"New Question Posted to: <{feed.Uri}| *{feed.Name}*>." },
                                    { "unfurl_links", "false" },
                                    { "unfurl_media", "false" },
                                    { "attachments", JsonConvert.SerializeObject(attachments, new JsonSerializerSettings { Formatting = Formatting.Indented }) }
                                });

                                var response = client.PostAsync("https://slack.com/api/chat.postMessage", data).Result;
                                response.EnsureSuccessStatusCode();
                            }

                            team.Posted.Add(id);
                        }
                    }
                }

                // Only keep the last 30 posts we sent, to avoid filling up DocumentDB storage.
                team.Posted = team.Posted.Skip(team.Posted.Count - 30).ToList();
            }
            catch (Exception ex)
            {
                log.LogDebug(ex.ToString());
            }
        }

        /// <summary>
        /// Desperate attempt to bludgeon HTML into something resembling Slack markdown so we can put it into
        /// an attachment. This can't ever possibly work 100% of the time but we do what we can.
        /// </summary>
        /// <param name="html">Original HTML markup read from the Stack RSS feed.</param>
        /// <returns>Equivalent (roughly) text marked up using Slack Markdown</returns>
        private static string Slackify(string html)
        {
            var parser = new HtmlParser();
            var document = parser.Parse(WebUtility.HtmlDecode(html).Trim());

            foreach (var italics in document.All.Where(e => e.LocalName.Equals("i") || e.LocalName.Equals("em")))
            {
                italics.TextContent = $"_{italics.TextContent}_";
            }

            foreach (var bold in document.All.Where(e => e.LocalName.Equals("b") || e.LocalName.Equals("strong")))
            {
                bold.TextContent = $"*{bold.TextContent}*";
            }

            foreach (var pre in document.All.Where(e => e.LocalName.Equals("pre")))
            {
                pre.TextContent = $"`{pre.TextContent}`";
            }

            // Now, look for some specific document segments we expect to see from Stack Exchange markdown.
            foreach (var anchor in document.All.Where(a => a.LocalName.Equals("a")))
            {
                if (anchor.ChildElementCount == 1 && anchor.Children.First().LocalName.Equals("img"))
                {
                    var img = anchor.Children.First();
                    anchor.TextContent = $"<{anchor.GetAttribute("href")}|{img.GetAttribute("src")}>";
                }
                else
                {
                    anchor.TextContent = $"<{anchor.GetAttribute("href")}|{anchor.TextContent}>";
                }
            }

            foreach (var blockquote in document.All.Where(e => e.LocalName.Equals("blockquote")))
            {
                foreach (var quote in blockquote.Children.Where(e => e.LocalName.Equals("p")))
                {
                    quote.TextContent = $"> {quote.TextContent}";
                }
            }

            var duplicate = document.QuerySelectorAll("div.question-status").FirstOrDefault();
            if (duplicate != null)
            {
                if (duplicate.ClassList.Contains("question-originals-of-duplicate"))
                {
                    var link = duplicate.QuerySelectorAll("a").FirstOrDefault();
                    var count = duplicate.QuerySelectorAll("span.question-originals-answer-count").FirstOrDefault();

                    var newContent = $"[DUPLICATE QUESTION]\nDuplicate of: &lt;https://scifi.stackexchange.com{link.GetAttribute("href")}|{link.TextContent}&gt;, which has {count.TextContent.Trim()}.";
                    duplicate.OuterHtml = newContent;
                }
            }

            foreach (var embed in document.QuerySelectorAll("div.youtube-embed"))
            {
                embed.OuterHtml = string.Empty;
            }

            // This has to be last because it's going to strip most of the HTML that's left.
            foreach (var para in document.All.Where(e => e.LocalName.Equals("p")))
            {
                para.TextContent = $"{para.TextContent}\n";
            }

            // Stripping the HTML tends to leave some ugly whitespace, so try to clean that up too.
            var result = document.DocumentElement.TextContent;
            result = result.Replace("\n\n", "\n");
            result = result.Replace(" >", ">");
            result = result.Substring(0, Math.Min(result.Length, 2000));

            return result;
        }
    }
}
