using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the security level that can be set on an issue.
    /// </summary>
    public class IssueSecurityLevel : JiraNamedResource
    {
        /// <summary>
        /// Description of this security level.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
