using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// The resolution of the issue as defined in JIRA
    /// </summary>
    public class IssueResolution : JiraNamedEntity
    {
         internal IssueResolution(AbstractNamedRemoteEntity remoteEntity)
             : base(remoteEntity)
        {
        }

        internal IssueResolution(Jira jira, string id)
            : base(jira, id)
        {
        }

        internal IssueResolution(string name)
            : base(name)
        {
        }

        protected override IEnumerable<JiraNamedEntity> GetEntities(Jira jira, string projectKey = null)
        {
            return jira.GetIssueResolutions();
        }

        /// <summary>
        /// Allows assignation by name
        /// </summary>
        public static implicit operator IssueResolution(string name)
        {
            if (name != null)
            {
                int id;
                if (int.TryParse(name, out id))
                {
                    return new IssueResolution(null, name /*as id*/);
                }
                else
                {
                    return new IssueResolution(name);                    
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
        public static bool operator ==(IssueResolution entity, string name)
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
                return entity._name == name;
            }
        }

        /// <summary>
        /// Operator overload to simplify LINQ queries
        /// </summary>
        /// <remarks>
        /// Allows calls in the form of issue.Priority != "High"
        /// </remarks>
        public static bool operator !=(IssueResolution entity, string name)
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
                return entity._name != name;
            }
        }
    }
}
