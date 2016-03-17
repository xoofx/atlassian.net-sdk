using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents an issue link type in JIRA.
    /// </summary>
    public class IssueLinkType : JiraNamedResource
    {
        /// <summary>
        /// Description of the 'inward' issue link relationship.
        /// </summary>
        [JsonProperty("inward")]
        public string Inward { get; private set; }

        /// <summary>
        /// Description of the 'outward' issue link relationship.
        /// </summary>
        [JsonProperty("outward")]
        public string Outward { get; private set; }
    }
}
