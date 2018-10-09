using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace SlackStackJobs.Models
{
    public class SlackTeam
    {
        [JsonProperty("id")]
        public string Id
        {
            get;
            set;
        }

        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty("configuration")]
        public Uri Configuration
        {
            get;
            set;
        }

        [JsonProperty("channel")]
        public string Channel
        {
            get;
            set;
        }

        [JsonProperty("channel_name")]
        public string ChannelName
        {
            get;
            set;
        }

        [JsonProperty("webhook")]
        public Uri Webhook
        {
            get;
            set;
        }

        [JsonProperty("token")]
        public string Token
        {
            get;
            set;
        }

        [JsonProperty("bot_user")]
        public string BotUser
        {
            get;
            set;
        }

        [JsonProperty("bot_token")]
        public string BotToken
        {
            get;
            set;
        }

        [JsonProperty("active")]
        public bool Active
        {
            get;
            set;
        }

        [JsonProperty("subscriptions")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1500:BracesForMultiLineStatementsMustNotShareLine", Justification = "StyleCop rule breaks on auto-implemented properties")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:ClosingBraceMustBeFollowedByBlankLine", Justification = "StyleCop rule breaks on auto-implemented properties")]
        public Dictionary<string, string[]> Subscriptions
        {
            get;
            set;
        } = new Dictionary<string, string[]>();

        [JsonProperty("posted")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1500:BracesForMultiLineStatementsMustNotShareLine", Justification = "StyleCop rule breaks on auto-implemented properties")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:ClosingBraceMustBeFollowedByBlankLine", Justification = "StyleCop rule breaks on auto-implemented properties")]

        public List<string> Posted
        {
            get;
            set;
        } = new List<string>();
    }
}
