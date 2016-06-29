using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a project category in Jira.
    /// </summary>
    public class ProjectCategory : JiraNamedResource
    {
        /// <summary>
        /// Description of the category.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; private set; }
    }
}
