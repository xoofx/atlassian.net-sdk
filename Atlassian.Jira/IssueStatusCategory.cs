using System.Diagnostics.CodeAnalysis;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// The status of the issue as defined in JIRA
    /// </summary>
    [SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
    [SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
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

        public string ColorName
        {
            get
            {
                return _remoteStatusCategory?.ColorName;
            }
        }

        public string Key
        {
            get
            {
                return _remoteStatusCategory?.Key;
            }
        }

        /// <summary>
        /// Operator overload to simplify LINQ queries
        /// </summary>
        /// <remarks>
        /// Allows calls in the form of issue.Priority == "High"
        /// </remarks>
        public static bool operator ==(IssueStatusCategory entity, string name)
        {
            if ((object)entity == null)
            {
                return name == null;
            }
            else if (name == null)
            {
                return false;
            }
            else
            {
                return entity.Name == name;
            }
        }

        /// <summary>
        /// Operator overload to simplify LINQ queries
        /// </summary>
        /// <remarks>
        /// Allows calls in the form of issue.Priority != "High"
        /// </remarks>
        public static bool operator !=(IssueStatusCategory entity, string name)
        {
            if ((object)entity == null)
            {
                return name != null;
            }
            else if (name == null)
            {
                return true;
            }
            else
            {
                return entity.Name != name;
            }
        }
    }
}
