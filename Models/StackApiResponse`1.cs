using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace SlackStackJobs.Models
{
    public class StackApiResponse<T>
    {
        [JsonProperty("items")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1500:BracesForMultiLineStatementsMustNotShareLine", Justification = "StyleCop rule breaks on auto-implemented properties")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:ClosingBraceMustBeFollowedByBlankLine", Justification = "StyleCop rule breaks on auto-implemented properties")]
        public List<T> Items
        {
            get;
            set;
        } = new List<T>();

        [JsonProperty("backoff")]
        public int BackOff
        {
            get;
            set;
        }

        [JsonProperty("error_id")]
        public int ErrorId
        {
            get;
            set;
        }

        [JsonProperty("error_message")]
        public string ErrorMessage
        {
            get;
            set;
        }

        [JsonProperty("error_name")]
        public string ErrorName
        {
            get;
            set;
        }

        [JsonProperty("has_more")]
        public bool HasMore
        {
            get;
            set;
        }

        [JsonProperty("quota_max")]
        public bool QuotaMax
        {
            get;
            set;
        }

        [JsonProperty("quota_remaining")]
        public bool QuotaRemaining
        {
            get;
            set;
        }
    }
}
