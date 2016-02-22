using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a JIRA user.
    /// </summary>
    public class JiraUser
    {
        /// <summary>
        /// The 'username' for the user.
        /// </summary>
        [JsonProperty("name")]
        public string Username { get; private set; }

        /// <summary>
        /// The long display name for the user.
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; private set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        [JsonProperty("emailAddress")]
        public string Email { get; private set; }

        /// <summary>
        /// Whether the user is marked as active on the server.
        /// </summary>
        [JsonProperty("active")]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Url to access this resource.
        /// </summary>
        [JsonProperty("self")]
        public string Self { get; private set; }
    }
}
