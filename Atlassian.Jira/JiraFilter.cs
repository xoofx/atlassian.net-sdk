using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a JIRA filter.
    /// </summary>
    public class JiraFilter : JiraNamedResource
    {
        /// <summary>
        /// JQL for this filter.
        /// </summary>
        [JsonProperty("jql")]
        public string Jql { get; set; }

        /// <summary>
        /// Description for this filter.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
