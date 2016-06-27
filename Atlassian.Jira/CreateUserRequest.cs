using Newtonsoft.Json;

namespace Atlassian.Jira
{
    /// <remarks>
    /// The JiraUser type does not map exactly to the request parameters for <see href="https://docs.atlassian.com/jira/REST/latest/#api/2/user-createUser">Create User</see>
    /// 
    /// </remarks>
    public class CreateUserRequest
    {
        /// <summary>
        /// Set the username
        /// </summary>
        [JsonProperty("name")]
        public string Username { get;  set; }

        /// <summary>
        /// Set the DisplayName
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Set the email address
        /// </summary>
        [JsonProperty("emailAddress")]
        public string Email { get;  set; }

        /// <summary>
        /// If password field is not set then password will be randomly generated.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// Set to true to have the user notified by email upon account creation. False to prevent notification.
        /// </summary>
        [JsonProperty("notification")]
        public bool Notification { get; set; }

        public override string ToString()
        {
            return Username;
        }
    }
}