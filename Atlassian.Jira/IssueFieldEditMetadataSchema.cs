using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents schema object of issuefield
    /// </summary>
    public class IssueFieldEditMetadataSchema
    {
        /// <summary>
        /// Type of the field ( for example array )
        /// </summary>  
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Type of individual value ( for example user, string, ... )
        /// </summary>
        [JsonProperty("items")]
        public string Items { get; set; }

        /// <summary>
        /// System name of the field
        /// </summary>
        [JsonProperty("system")]
        public string System { get; set; }

        /// <summary>
        /// Example: custom="com.atlassian.jira.plugin.system.customfieldtypes:select"
        /// </summary>
        [JsonProperty("custom")]
        public string Custom { get; set; }

        /// <summary>
        /// Id of the Custom
        /// </summary>
        [JsonProperty("customId")]
        public int CustomId { get; set; }
    }
}
