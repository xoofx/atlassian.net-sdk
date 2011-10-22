using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// The type of the issue as defined in JIRA
    /// </summary>
    public class IssueType : JiraNamedEntity
    {
         internal IssueType(AbstractNamedRemoteEntity remoteEntity)
             : base(remoteEntity)
        {
        }

        internal IssueType(Jira jira, string id)
            : base(jira, id)
        {
        }

        internal IssueType(string name)
            : base(name)
        {
        }

        protected override IEnumerable<JiraNamedEntity> GetEntities(Jira jira, string projectKey = null)
        {
            return jira.GetIssueTypes(projectKey);
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
                    return new IssueType(null, name /*as id*/);
                }
                else
                {
                    return new IssueType(name);                    
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
                return entity._name == name;
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
                return entity._name != name;
            }
        }
    }
}
