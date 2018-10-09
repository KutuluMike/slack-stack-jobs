using System;
using Newtonsoft.Json;

namespace SlackStackJobs.Models
{
    public class StackUser
    {
        [JsonProperty("account_id")]
        public int AccountId
        {
            get;
            set;
        }

        [JsonProperty("user_id")]
        public int UserId
        {
            get;
            set;
        }

        [JsonProperty("display_name")]
        public string DisplayName
        {
            get;
            set;
        }

        [JsonProperty("user_type")]
        public string UserType
        {
            get;
            set;
        }

        [JsonProperty("link")]
        public Uri Link
        {
            get;
            set;
        }

        [JsonProperty("profile_image")]
        public Uri ProfileImage
        {
            get;
            set;
        }
    }
}
