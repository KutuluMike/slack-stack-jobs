using System;
using Newtonsoft.Json;

namespace SlackStackJobs.Models
{
    public class QueuedJob
    {
        [JsonProperty("feed")]
        public string Feed
        {
            get;
            set;
        }

        [JsonProperty("team")]
        public string Team
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
    }
}
