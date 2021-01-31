using Atlassian.Jira.Remote;
using System.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// The type of the issue as defined in JIRA
    /// </summary>
    public class IssueTypeWithStatus : JiraNamedConstant
    {
        /// <summary>
        /// Creates an instance of the IssueTypeWithStatus based on a remote entity.
        /// </summary>
        public IssueTypeWithStatus(RemoteIssueTypeWithStatus remoteIssueType)
            : base(remoteIssueType)
        {
            IsSubTask = remoteIssueType.subTask;
            Statuses = remoteIssueType.statuses.Select(x => new IssueStatus(x)).ToArray();
        }

        /// <summary>
        /// Creates an instance of the IssueTypeWithStatus with given id and name.
        /// </summary>
        /// <param name="id">Identifiers of the issue type.</param>
        /// <param name="name">Name of the issue type.</param>
        /// <param name="isSubTask">Whether the issue type is a sub task.</param>
        public IssueTypeWithStatus(string id, string name = null, bool isSubTask = false)
            : base(id, name)
        {
            IsSubTask = isSubTask;
        }

        /// <summary>
        /// Whether this issue type represents a sub-task.
        /// </summary>
        public bool IsSubTask { get; private set; }

        /// <summary>
        /// The list of valid status values for this issue type.
        /// </summary>
        public IssueStatus[] Statuses { get; private set; }
    }
}
