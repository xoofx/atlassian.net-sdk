using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// The category of an issue status as defined in JIRA.
    /// </summary>
    public class IssueStatusCategory : JiraNamedEntity
    {
        private readonly RemoteStatusCategory _remoteStatusCategory;

        /// <summary>
        /// Creates an instance of the IssueStatusCategory based on a remote entity.
        /// </summary>
        public IssueStatusCategory(RemoteStatusCategory remoteStatusCategory)
            : base(remoteStatusCategory)
        {
            _remoteStatusCategory = remoteStatusCategory;
        }

        /// <summary>
        /// The color assigned to this category.
        /// </summary>
        public string ColorName
        {
            get
            {
                return _remoteStatusCategory?.ColorName;
            }
        }

        /// <summary>
        /// The key assigned to this category.
        /// </summary>
        public string Key
        {
            get
            {
                return _remoteStatusCategory?.Key;
            }
        }
    }
}