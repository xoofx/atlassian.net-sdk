using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a JIRA user.
    /// </summary>
    public class JiraUser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JiraUser"/> class based on its remote counterpart.
        /// </summary>
        /// <param name="remoteUser">The remote user.</param>
        /// <param name="userPrivacyEnabled">if set to <c>true</c> enable user privacy mode (use 'accountId' insead of 'name' for serialization).</param>
        public JiraUser(RemoteJiraUser remoteUser, bool userPrivacyEnabled = false)
        {
            AccountId = remoteUser.accountId;
            DisplayName = remoteUser.displayName;
            Email = remoteUser.emailAddress;
            IsActive = remoteUser.active;
            Key = remoteUser.key;
            Locale = remoteUser.locale;
            Self = remoteUser.self;
            Username = remoteUser.name;
            AvatarUrls = remoteUser.avatarUrls;
            InternalIdentifier = userPrivacyEnabled ? remoteUser.accountId : remoteUser.name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JiraUser"/> class.
        /// </summary>
        internal JiraUser()
        {
        }

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
