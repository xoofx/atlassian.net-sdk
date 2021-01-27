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
        public string AccountId { get; internal set; }

        /// <summary>
        /// The identifier for the user as defined by JIRA.
        /// </summary>
        public string Key { get; internal set; }

        /// <summary>
        /// The 'username' for the user.
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// The long display name for the user.
        /// </summary>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string Email { get; internal set; }

        /// <summary>
        /// Whether the user is marked as active on the server.
        /// </summary>
        public bool IsActive { get; internal set; }

        /// <summary>
        /// The locale of the User.
        /// </summary>
        public string Locale { get; internal set; }

        /// <summary>
        /// Url to access this resource.
        /// </summary>
        public string Self { get; internal set; }

        /// <summary>
        /// The list of the Avatar URL's for this user
        /// </summary>
        public AvatarUrls AvatarUrls { get; internal set; }

        internal string InternalIdentifier { get; set; }

        public override string ToString()
        {
            return InternalIdentifier;
        }

        public override bool Equals(object other)
        {
            var otherAsThisType = other as JiraUser;
            return otherAsThisType != null && InternalIdentifier.Equals(otherAsThisType.InternalIdentifier);
        }

        public override int GetHashCode()
        {
            return InternalIdentifier.GetHashCode();
        }
    }
}
