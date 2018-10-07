// Copyright (c)Mike Edenfield <kutulu@kutulu.org>. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlackStackJobs.Models
{
    public class StackFeed
    {
        [JsonProperty("site")]
        public string Site
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

        [JsonProperty("description")]
        public string Description
        {
            get;
            set;
        }

        [JsonProperty("uri")]
        public Uri Uri
        {
            get;
            set;
        }

        [JsonProperty("sourceUri")]
        public Uri SourceUri
        {
            get;
            set;
        }

        [JsonProperty("logoUri")]
        public Uri LogoUri
        {
            get;
            set;
        }

        [JsonProperty("owner")]
        public string Owner
        {
            get;
            set;
        }

        [JsonProperty("teams")]
        public List<string> Teams
        {
            get;
            set;
        }

        [JsonProperty("updated")]
        public DateTime Updated
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
