using Newtonsoft.Json;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the visibility field on a comment.
    /// </summary>
    public class CommentVisibility
    {
        /// <summary>
        /// Create an empty comment visibility object.
        /// </summary>
        public CommentVisibility()
        {
        }

        /// <summary>
        /// Creates a comment visibility object with the given role.
        /// </summary>
        /// <param name="role">The role to apply to the visibility object.</param>
        public CommentVisibility(string role)
        {
            this.Type = "role";
            this.Value = role;
        }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
