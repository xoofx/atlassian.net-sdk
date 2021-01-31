using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// The type of the issue as defined in JIRA
    /// </summary>
    [SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
    [SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
    public class IssueType : JiraNamedConstant
    {
        /// <summary>
        /// Creates an instance of the IssueType based on a remote entity.
        /// </summary>
        public IssueType(RemoteIssueType remoteIssueType)
            : base(remoteIssueType)
        {
            IsSubTask = remoteIssueType.subTask;
            Statuses = remoteIssueType.statuses?.Select(x => new IssueStatus(x)).ToArray();
        }

        /// <summary>
        /// Creates an instance of the IssueType with given id and name.
        /// </summary>
        /// <param name="id">Identifiers of the issue type.</param>
        /// <param name="name">Name of the issue type.</param>
        /// <param name="isSubTask">Whether the issue type is a sub task.</param>
        public IssueType(string id, string name = null, bool isSubTask = false)
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

        public bool SearchByProjectOnly { get; set; }

        internal string ProjectKey { get; set; }

        protected override async Task<IEnumerable<JiraNamedEntity>> GetEntitiesAsync(Jira jira, CancellationToken token)
        {
            var results = await jira.IssueTypes.GetIssueTypesAsync(token).ConfigureAwait(false);

            if (!String.IsNullOrEmpty(ProjectKey) &&
                (SearchByProjectOnly || results.Distinct(new JiraEntityNameEqualityComparer()).Count() != results.Count()))
            {
                // There are multiple issue types with the same name. Possibly because there are a combination
                //  of classic and NextGen projects in Jira. Get the issue types from the project if it is defined.
                results = await jira.IssueTypes.GetIssueTypesForProjectAsync(ProjectKey).ConfigureAwait(false);
            }

            return results as IEnumerable<JiraNamedEntity>;
        }

        /// <summary>
        /// Allows assignation by name
        /// </summary>
        public static implicit operator IssueType(string name)
        {
            if (name != null)
            {
                int id;
                if (int.TryParse(name, out id))
                {
                    return new IssueType(name /*as id*/);
                }
                else
                {
                    return new IssueType(null, name);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Operator overload to simplify LINQ queries
        /// </summary>
        /// <remarks>
        /// Allows calls in the form of issue.Priority == "High"
        /// </remarks>
        public static bool operator ==(IssueType entity, string name)
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
        public static bool operator !=(IssueType entity, string name)
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
