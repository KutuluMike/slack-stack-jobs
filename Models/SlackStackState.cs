using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SlackStackJobs.Models
{
    public class SlackStackState
    {
        [JsonProperty("id")]
        public string Id
        {
            get;
            set;
        }

        [JsonProperty("processed")]
        public DateTime Processed
        {
            get;
            set;
        }

        [JsonProperty("feeds")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1500:BracesForMultiLineStatementsMustNotShareLine", Justification = "StyleCop rule breaks on auto-implemented properties")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:ClosingBraceMustBeFollowedByBlankLine", Justification = "StyleCop rule breaks on auto-implemented properties")]
        public Dictionary<string, StackFeed> Feeds
        {
            get;
            set;
        } = new Dictionary<string, StackFeed>();
    }
}