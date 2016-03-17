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
    public class JiraNamedResource : IJiraEntity
    {
        /// <summary>
        /// Identifier of this resource.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Name of this resource.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Url to access this resource.
        /// </summary>
        [JsonProperty("self")]
        public string Self { get; private set; }
    }
}
