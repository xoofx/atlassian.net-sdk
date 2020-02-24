using System;
using Newtonsoft.Json;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a JIRA user.
    /// </summary>
    public class JiraUser
    {
        /// <summary>
        /// The Atlassian account identifier for this user.
        /// </summary>
        [JsonProperty("accountId")]
        public string AccountId { get; internal set; }

        /// <summary>
        /// The identifier for the user as defined by JIRA.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; internal set; }

        /// <summary>
        /// The 'username' for the user.
        /// </summary>
        [JsonProperty("name")]
        public string Username { get; internal set; }

        /// <summary>
        /// The long display name for the user.
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; internal set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        [JsonProperty("emailAddress")]
        public string Email { get; internal set; }

        /// <summary>
        /// Whether the user is marked as active on the server.
        /// </summary>
        [JsonProperty("active")]
        public bool IsActive { get; internal set; }

        /// <summary>
        /// The locale of the User.
        /// </summary>
        [JsonProperty("locale")]
        public string Locale { get; internal set; }

        /// <summary>
        /// Url to access this resource.
        /// </summary>
        [JsonProperty("self")]
        public string Self { get; internal set; }

        internal string InternalIdentifier { get; set; }

        public override string ToString()
        {
            return Username;
        }

        public override bool Equals(object other)
        {
            var otherAsThisType = other as JiraUser;
            return otherAsThisType != null && Username.Equals(otherAsThisType.Username);
        }

        public override int GetHashCode()
        {
            return Username.GetHashCode();
        }
    }
}
