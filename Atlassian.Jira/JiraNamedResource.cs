using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Class used to deserialize a generic JIRA resource as returned by the REST API.
    /// </summary>
    public class JiraNamedResource
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }
    }
}
